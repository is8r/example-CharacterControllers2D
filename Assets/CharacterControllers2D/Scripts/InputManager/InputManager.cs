/*
        InputManager.Instance.OnActionEvent.AddListener(delegate (ActionType type) {
            print("ActionType: " + type.ToString());
        });
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CharacterControllers2D.Inputs
{
    public enum ButtonType
    {
        A,
        B,
        X,
        Y,
        L,
        R
    }

    public enum ActionType
    {
        Jump,
        Attack,
        Dash,
        Avoid
    }

    [System.Serializable]
    public class ActionEvent : UnityEvent<ActionType> { };

    [System.Serializable]
    public class ButtonEvent : UnityEvent<ButtonType> { };

    [DefaultExecutionOrder(-1)]
    public class InputManager : SingletonMonoBehaviour<InputManager>
    {
        //ボタンイベントの発行
        [SerializeField, HideInInspector] public ButtonEvent OnButtonEvent;
        [SerializeField, HideInInspector] public ActionEvent OnActionEvent;

        private void Update()
        {
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown(ButtonType.Y.ToString()))
            {
                if (OnActionEvent != null)
                {
                    OnActionEvent.Invoke(ActionType.Jump);
                }
            }

            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown(ButtonType.R.ToString()))
            {
                if (OnActionEvent != null)
                {
                    OnActionEvent.Invoke(ActionType.Attack);
                }
            }

            if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown(ButtonType.A.ToString()))
            {
                if (OnActionEvent != null)
                {
                    OnActionEvent.Invoke(ActionType.Avoid);
                }
            }
        }
    }
}

