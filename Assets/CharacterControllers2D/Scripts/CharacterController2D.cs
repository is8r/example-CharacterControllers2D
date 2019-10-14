/*
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CharacterControllers2D.Inputs;
using CharacterControllers2D.Utilities;

namespace CharacterControllers2D
{
    public enum CharacterEventType
    {
        GroundContactLost,
        GroundContactRegained
    }

    [System.Serializable]
    public class CharacterEvent : UnityEvent<CharacterEventType> { };

    public class CharacterController2D : MonoBehaviour
    {
        [SerializeField, HideInInspector] public CharacterEvent OnCharacterEvent;

        public float walkSpeed = 10f;
        public float jumpSpeed = 20f;
        public float airControl = 0.4f;
        public float gravity = 1f;
        public LayerMask groundLayer = 1 << 0;

        float slopeLimit = 30f;//床判定に使う床の角度
        float slopeMargin = 0.4f;//床判定に使う床の角度
        float groundLimit = 0.2f;//床判定に使う床との距離

        public Vector2 velocity = Vector2.zero;//移動量
        public Vector2 momentum = Vector2.zero;//勢い
        Vector2 inputVelocity;//入力値
        public Vector2 InputVelocity//入力値を外から参照できるように（アニメーションで使用）
        {
            get { return this.inputVelocity; }
        }

        float groundDistance;//地面までの距離
        float groundAngle;//地面までの距離
        public float GroundAngle//入力値を外から参照できるように（アニメーションで使用）
        {
            get { return groundAngle; }
        }

        public enum ControllerState
        {
            Grounded,
            Falling,
            BeforeRising,
            Rising
        }
        public ControllerState currentControllerState = ControllerState.Falling;

        private void Start()
        {
            InitPhysicMaterial();

            InputManager.Instance.OnActionEvent.AddListener(delegate (ActionType type) {
                Jump();
            });
        }

        private void Update()
        {
            CheckState();

            inputVelocity.x = Input.GetAxis("Horizontal");
            inputVelocity.y = Input.GetAxis("Vertical");
            groundDistance = GetGroundDistance();//地面の距離
            groundAngle = GetGroundAngle();//地面の角度
        }

        private void FixedUpdate()
        {
            //動く力
            velocity = GetVelocity();

            //勢い
            momentum = GetMomentum();

            //移動
            velocity += momentum;
            rb2d.velocity = velocity;
        }

        //入力値に対しての移動量を取得
        private Vector2 GetVelocity()
        {
            Vector2 _velocity = Vector2.zero;

            if (IsSlipping())
            {
                Vector2 angleVector = GetGroundAngleVector();
                _velocity += angleVector * inputVelocity.x;
            }
            else
            {
                _velocity += Vector2.right * inputVelocity.x;
            }

            if (_velocity.magnitude > 1f)
            {
                _velocity.Normalize();
            }
            _velocity *= walkSpeed;

            return _velocity;
        }

        //勢いの値を取得
        private Vector3 GetMomentum()
        {
            Vector3 _momentum = GetVerticalMomentum() + GetHorizontalMomentum();
            return _momentum;
        }

        private Vector2 GetVerticalMomentum()
        {
            Vector2 _verticalMomentum = Vector2.zero;

            //現在の勢いから垂直方向を抽出
            if (momentum != Vector2.zero)
            {
                _verticalMomentum = Calc.ExtractDotVector(momentum, transform.up);
            }

            if (currentControllerState == ControllerState.BeforeRising)
            {
                //ジャンプ前の場合にはジャンプ開始
                _verticalMomentum = Vector3.up * jumpSpeed;

                //momentum以外のジャンプ開始処理
                JumpStart();
            }
            else if (currentControllerState == ControllerState.Rising)
            {
                //上昇中は追加重力
                _verticalMomentum -= Vector2.up * gravity;
            }
            else if (currentControllerState == ControllerState.Falling && !IsSlipping())
            {
                _verticalMomentum -= Vector2.up * gravity;
            }
            else if (currentControllerState == ControllerState.Grounded)
            {
                //どれでも無ければ0
                _verticalMomentum = Vector3.zero;
            }

            return _verticalMomentum;
        }

        //水平方向の値を取得
        private Vector2 GetHorizontalMomentum()
        {
            Vector2 _horizontalMomentum = Vector3.zero;

            //現在の勢いから水平方向を抽出
            if (momentum != Vector2.zero)
            {
                Vector2 _verticalMomentum = Calc.ExtractDotVector(momentum, transform.up);
                _horizontalMomentum = momentum - _verticalMomentum;
            }

            //摩擦を計測
            float friction = cc2d.sharedMaterial.friction;
            _horizontalMomentum = Calc.IncrementVectorLengthTowardTargetLength(_horizontalMomentum, friction, Time.deltaTime, 0);

            return _horizontalMomentum;
        }

        //地面を離れた（移動中の勢いを保持）
        private void OnGroundContactLost()
        {
            Vector3 _currentVelocity = rb2d.velocity;
            _currentVelocity = Calc.RemoveDotVector(_currentVelocity, transform.up);

            float _length = _currentVelocity.magnitude;

            //速度方向を計算
            Vector3 _velocityDirection = Vector3.zero;
            if (System.Math.Abs(_length) > 0.001f)
            {
                _velocityDirection = _currentVelocity / _length;
            }

            //「walkSpeed」および「airControl」に基づいて「_length」から減算し、オーバーシュートを確認
            if (_length >= walkSpeed * airControl)
            {
                _length -= walkSpeed * airControl;
            }

            //momentumを更新
            momentum = _velocityDirection * _length;

            //イベント発行
            if (OnCharacterEvent != null) OnCharacterEvent.Invoke(CharacterEventType.GroundContactLost);
        }

        //地面に戻った（momentumをリセット）
        private void OnGroundContactRegained()
        {
            //momentumを更新
            momentum = Vector3.zero;

            //イベント発行
            if (OnCharacterEvent != null) OnCharacterEvent.Invoke(CharacterEventType.GroundContactRegained);
        }

        //ジャンプが入力された
        private void Jump()
        {
            if (!IsGrounded()) return;

            currentControllerState = ControllerState.BeforeRising;
        }

        //ジャンプを開始
        private void JumpStart()
        {
            currentControllerState = ControllerState.Rising;

            //地面を喪失
            OnGroundContactLost();
        }

        //状態を更新
        private void CheckState()
        {
            if (IsGrounded() && currentControllerState == ControllerState.Falling)
            {
                //地面に近づいたら着地
                currentControllerState = ControllerState.Grounded;

                //momentumを変更
                OnGroundContactRegained();
            }
            else if (currentControllerState == ControllerState.Rising)
            {
                //ジャンプの上昇中
                float _length = Vector2.Dot(rb2d.velocity, transform.up);
                if (_length < 1f)
                {
                    //上昇中に下がり始めたら降下モード
                    currentControllerState = ControllerState.Falling;
                }
            }
            else if (!IsGrounded() && currentControllerState == ControllerState.Grounded)
            {
                //Grounded中、地面がなくなったらFalling
                currentControllerState = ControllerState.Falling;

                //地面を喪失
                OnGroundContactLost();
            }
        }

        //地面についているかどうか
        private bool IsGrounded()
        {
            if (groundDistance < groundLimit || IsSlipping())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //坂道の上にいるかどうか
        private bool IsSlipping()
        {
            if (Mathf.Abs(groundAngle) > 0.01f && groundAngle < slopeLimit && groundDistance < slopeMargin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //地面との距離
        float GetGroundDistance()
        {
            //Layを飛ばす長さ
            float layLength = (cc2d.size.y / 2) + groundLimit + slopeMargin;

            //Raycastを飛ばして地面を調べる
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, layLength, groundLayer);

            //Rayを可視化
            //Debug.DrawRay(transform.position, new Vector3(0, -layLength, 0), Color.blue, 1);

            //接地しているかどうかを返す
            if (hit)
            {
                return (hit.distance - (cc2d.size.y / 2));
            }
            else
            {
                return Mathf.Infinity;
            }
        }

        //地面の角度
        float GetGroundAngle()
        {
            //Layを飛ばす長さ
            float layLength = (cc2d.size.y / 2) + groundLimit + slopeMargin;

            //Raycastを飛ばして地面を調べる
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, layLength, groundLayer);

            //Rayを可視化
            //Debug.DrawRay(transform.position, new Vector3(0, -layLength, 0), Color.blue, 1);

            //接地している角度を返す
            if (hit)
            {
                //Vector3 velocity = Vector3.ProjectOnPlane(velocity, hit.normal).normalized;
                return Vector2.Angle(hit.normal, Vector2.up);
            }
            else
            {
                return 0;
            }
        }

        //地面の角度（Vector2）
        Vector2 GetGroundAngleVector()
        {
            float radian = (-groundAngle + 90f) * Mathf.Deg2Rad;
            float x = (float)Mathf.Sin(radian);
            float y = (float)Mathf.Cos(radian);
            return new Vector2(x, y);
        }

        //キャラクタ用のPhysicMaterialを作成してアタッチ
        private void InitPhysicMaterial()
        {
            PhysicsMaterial2D noFrictionPhysics = new PhysicsMaterial2D();
            noFrictionPhysics.name = "noFrictionPhysics";
            noFrictionPhysics.friction = 0;
            cc2d.sharedMaterial = noFrictionPhysics;
        }

        private CapsuleCollider2D m_CapsuleCollider2D;
        private CapsuleCollider2D cc2d => m_CapsuleCollider2D ?? (m_CapsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>());
        private Rigidbody2D m_Rigidbody2D;
        private Rigidbody2D rb2d => m_Rigidbody2D ?? (m_Rigidbody2D = gameObject.GetComponent<Rigidbody2D>());
    }
}
