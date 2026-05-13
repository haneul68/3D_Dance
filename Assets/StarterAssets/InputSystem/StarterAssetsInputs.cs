using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool attack;
        public bool dance_1;
        public bool interact;
        public bool rangedAttack;

        [Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;



#if ENABLE_INPUT_SYSTEM
        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }
        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
        public void OnAttack(InputValue value)
        {
            AttackInput(value.isPressed);
        }
        public void OnDance_1(InputValue value)
        {
            if (value.isPressed)
            {
                Dance_1_Input(true);
            }
        }
        public void OnRangedAttack(InputValue value)
        {
            RangedAttackInput(value.isPressed);
        }
#endif

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;

            if (newMoveDirection.sqrMagnitude > 0.01f)
            {
                dance_1 = false;
            }
        }

        public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
        public void AttackInput(bool newAttackState)
        {
            attack = newAttackState;
        }
        public void Dance_1_Input(bool newDanceState)
        {
            dance_1 = newDanceState;
        }
        public void RangedAttackInput(bool newRangedAttackState)
        {
            rangedAttack = newRangedAttackState;
        }

    }

}