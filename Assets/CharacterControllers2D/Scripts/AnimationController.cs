using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterControllers2D
{
    public class AnimationController : MonoBehaviour
    {
        void OnEnable()
        {
            characterController.OnCharacterEvent.AddListener(OnCharacterEvent);
        }

        void OnDisable()
        {
            characterController.OnCharacterEvent.RemoveListener(OnCharacterEvent);
        }

        void OnCharacterEvent(CharacterEventType e)
        {
            switch (e)
            {
                case CharacterEventType.GroundContactLost:
                    animator.SetBool("IsGrounded", false);
                    break;
                case CharacterEventType.GroundContactRegained:
                    animator.SetBool("IsGrounded", true);
                    break;
                default:
                    break;
            }
        }

        private void FixedUpdate()
        {
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (characterController.currentControllerState == CharacterController2D.ControllerState.Grounded)
            {
                float speed = Mathf.Abs(characterController.InputVelocity.x) + Mathf.Abs(characterController.InputVelocity.y);
                speed = Mathf.Clamp01(speed);
                animator.SetFloat("Speed", speed, 0.01f, Time.deltaTime);
            }
        }

        private CharacterController2D m_CharacterController;
        private CharacterController2D characterController => m_CharacterController ?? (m_CharacterController = gameObject.GetComponent<CharacterController2D>());
        private Rigidbody m_Rigidbody;
        private Rigidbody rb => m_Rigidbody ?? (m_Rigidbody = gameObject.GetComponent<Rigidbody>());
        private Animator m_Animator;
        private Animator animator => m_Animator ?? (m_Animator = gameObject.GetComponentInChildren<Animator>());
    }
}
