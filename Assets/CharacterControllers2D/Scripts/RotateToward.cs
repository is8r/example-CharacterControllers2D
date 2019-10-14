using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterControllers2D
{
	public class RotateToward : MonoBehaviour
	{
		public Transform target;
		public float speed = 500f;

		Transform parentTransform;
		Transform targetTransform;
		float fallOffAngle = 90f;
		float currentYRotation = 0f;

		void Start()
		{
			targetTransform = target.transform;
			parentTransform = transform;
		}

		void LateUpdate()
		{
			Vector2 _velocity = rb2d.velocity;

            if(Mathf.Abs(_velocity.x) > 0.1f)
            {
                if (_velocity.x < 0)
                {
                    targetTransform.localScale = new Vector3(-1f, targetTransform.localScale.y, targetTransform.localScale.z);
                }
                else
                {
                    targetTransform.localScale = new Vector3(1f, targetTransform.localScale.y, targetTransform.localScale.z);
                }
            }

            targetTransform.rotation = Quaternion.Euler(0, 0, characterController.GroundAngle);
        }

        private CharacterController2D m_CharacterController;
        private CharacterController2D characterController => m_CharacterController ?? (m_CharacterController = gameObject.GetComponent<CharacterController2D>());
        private Rigidbody2D m_Rigidbody2D;
		private Rigidbody2D rb2d => m_Rigidbody2D ?? (m_Rigidbody2D = gameObject.GetComponent<Rigidbody2D>());
	}
}
