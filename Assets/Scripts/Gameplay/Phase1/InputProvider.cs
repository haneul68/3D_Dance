using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SystemicOverload.Phase1
{
    /// <summary>
    /// Phase 1 전용 입력을 읽고, 다른 스크립트에서 사용하기 쉬운 상태값으로 제공하는 클래스.
    /// 
    /// InputProvider 뜻:
    /// Input = 입력
    /// Provider = 제공자
    /// 즉, "입력 제공자"
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class InputProvider : MonoBehaviour
    {
        // Control = 조작
        // Device = 장치
        // Kind = 종류
        // 즉, "조작 장치 종류"
        public enum ControlDeviceKind
        {
            None,           // 없음
            KeyboardMouse,  // 키보드 + 마우스
            Gamepad,        // 게임패드
            Other           // 그 외 장치
        }

        // Look = 바라보기 / 조준
        // Device = 장치
        // Kind = 종류
        // 즉, "조준 입력 장치 종류"
        public enum LookDeviceKind
        {
            None,       // 없음
            Pointer,    // 마우스, 터치, 펜 같은 포인터 입력
            Gamepad,    // 게임패드
            Other       // 그 외 장치
        }

        // 기본 InputActionAsset 경로
        // Resources.Load에서는 Assets/Resources 뒤의 경로만 사용하고 확장자는 제외한다.
        private const string DefaultActionAssetResourcesPath = "Input/Phase1Gameplay";

        // 에디터에서 직접 에셋을 찾기 위한 경로
        private const string DefaultActionAssetEditorPath = "Assets/Resources/Input/Phase1Gameplay.inputactions";

        // InputActionAsset 안에 있는 Action Map 이름
        private const string GameplayMapName = "Player";

        // InputActionAsset 안에 있는 각 Action 이름들
        private const string MoveActionName = "Move";                         // 이동 입력
        private const string LookActionName = "Look";                         // 시야 / 조준 입력
        private const string PointerPositionActionName = "PointerPosition";   // 마우스 위치 입력
        private const string ZoomActionName = "Zoom";                         // 줌 입력
        private const string PrimaryHoldActionName = "PrimaryHold";           // 주 입력 유지, 보통 좌클릭
        private const string SecondaryHoldActionName = "SecondaryHold";       // 보조 입력 유지, 보통 우클릭
        private const string AttackActionName = "Attack";                     // 공격 입력

        [Header("Action Asset")]
        // InputActionAsset = 유니티 Input System에서 입력 설정을 담고 있는 에셋
        [SerializeField] private InputActionAsset inputActionsAsset;

        // inputActionsAsset이 직접 연결되지 않았을 때 Resources 폴더에서 찾을 경로
        [SerializeField] private string resourcesFallbackPath = DefaultActionAssetResourcesPath;

        [Header("Movement")]
        // normalize = 정규화하다
        // diagonal = 대각선
        // 대각선 이동 시 속도가 빨라지는 문제를 막을지 여부
        [SerializeField] private bool normalizeDiagonalInput = true;

        // 좌클릭 + 우클릭을 동시에 누르면 앞으로 이동하게 할지 여부
        [SerializeField] private bool enableDualMouseForwardMove = true;

        // 좌클릭 + 우클릭으로 앞으로 이동할 때 추가할 이동량
        [SerializeField] private float dualMouseForwardAmount = 1.0f;

        [Header("Gamepad")]
        // Deadzone = 데드존
        // 게임패드 스틱의 아주 작은 흔들림을 무시하기 위한 값
        [SerializeField] private float gamepadLookDeadzone = 0.15f;

        // 실행 중 사용할 InputActionAsset 복사본
        // runtime = 실행 중
        private InputActionAsset runtimeInputActions;

        // Player Action Map
        private InputActionMap gameplayMap;

        // 각각의 입력 액션들
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction pointerPositionAction;
        private InputAction zoomAction;
        private InputAction primaryHoldAction;
        private InputAction secondaryHoldAction;
        private InputAction attackAction;

        // 콜백이 이미 연결되었는지 확인하는 변수
        private bool callbacksBound;

        // 초기화에 실패했는지 확인하는 변수
        private bool initializationFailed;

        // Raw = 원본
        // 보정되지 않은 이동 입력값
        public Vector2 RawMoveInput { get; private set; }

        // 보정된 최종 이동 입력값
        public Vector2 MoveInput { get; private set; }

        // 마우스 / 터치 위치
        public Vector2 PointerScreenPosition { get; private set; }

        // 마우스 델타 또는 게임패드 조준 입력값
        public Vector2 LookInput { get; private set; }

        // 줌 입력값
        public float ZoomDelta { get; private set; }

        // Primary = 주요 / 첫 번째
        // 보통 좌클릭을 누르고 있는지 여부
        public bool IsPrimaryHeld { get; private set; }

        // Secondary = 보조 / 두 번째
        // 보통 우클릭을 누르고 있는지 여부
        public bool IsSecondaryHeld { get; private set; }

        // 좌클릭 누르는 중인지 확인
        public bool IsLeftMouseHeld => IsPrimaryHeld;

        // 우클릭 누르는 중인지 확인
        public bool IsRightMouseHeld => IsSecondaryHeld;

        // 좌클릭 + 우클릭으로 앞으로 이동 중인지 확인
        public bool IsDualMouseForwardHeld => IsDualInputForwardHeld;

        // Dual = 두 개의
        // Input = 입력
        // Forward = 앞으로
        // 좌클릭과 우클릭을 동시에 누르고 있는지 확인
        public bool IsDualInputForwardHeld => IsPrimaryHeld && IsSecondaryHeld;

        // 마지막으로 사용한 조작 장치 종류
        public ControlDeviceKind LastUsedDeviceKind { get; private set; }

        // 현재 조준에 사용 중인 장치 종류
        public LookDeviceKind CurrentLookDeviceKind { get; private set; }

        // 현재 Look 입력이 마우스 계열이면 LookInput 반환
        // 아니면 Vector2.zero 반환
        public Vector2 PointerLookDelta => CurrentLookDeviceKind == LookDeviceKind.Pointer ? LookInput : Vector2.zero;

        // 현재 Look 입력이 게임패드면 LookInput 반환
        // 아니면 Vector2.zero 반환
        public Vector2 GamepadLookInput => CurrentLookDeviceKind == LookDeviceKind.Gamepad ? LookInput : Vector2.zero;

        // 게임패드 조준 입력이 데드존보다 큰지 확인
        public bool HasGamepadLookInput => GamepadLookInput.sqrMagnitude > gamepadLookDeadzone * gamepadLookDeadzone;

        // 현재 게임패드를 사용 중인지 확인
        public bool IsUsingGamepad => LastUsedDeviceKind == ControlDeviceKind.Gamepad;

        // 캐릭터를 카메라 방향으로 정렬해야 하는지 여부
        // 우클릭 중이거나 게임패드 조준 입력이 있으면 true
        public bool ShouldAlignCharacterToCamera => IsSecondaryHeld || HasGamepadLookInput;

        // 포인터 방향 바라보기를 막아야 하는지 여부
        // 좌클릭 중이고 게임패드 조준이 없으면 true
        public bool ShouldBlockPointerFacing => IsPrimaryHeld && !HasGamepadLookInput;

        // 카메라 조작 입력이 눌려있는지 여부
        public bool IsCameraLookHeld => IsPrimaryHeld || IsSecondaryHeld;

        /// <summary>
        /// 이번 프레임에 공격 입력이 눌렸는지 여부.
        /// WasPressedThisFrame은 누른 순간 한 프레임만 true가 된다.
        /// </summary>
        public bool WasAttackPressedThisFrame { get; private set; }

        private void Reset()
        {
            // 컴포넌트를 처음 붙였을 때 기본 InputActionAsset 자동 연결 시도
            TryAssignDefaultAssetInEditor();
        }

        private void OnValidate()
        {
            // dualMouseForwardAmount가 음수가 되지 않도록 보정
            dualMouseForwardAmount = Mathf.Max(0.0f, dualMouseForwardAmount);

            // gamepadLookDeadzone은 0~1 사이 값만 허용
            gamepadLookDeadzone = Mathf.Clamp01(gamepadLookDeadzone);

            // Resources 경로가 비어있으면 기본값으로 복구
            if (string.IsNullOrWhiteSpace(resourcesFallbackPath))
            {
                resourcesFallbackPath = DefaultActionAssetResourcesPath;
            }

            // 에디터에서 기본 InputActionAsset 자동 연결 시도
            TryAssignDefaultAssetInEditor();
        }

        private void OnEnable()
        {
            // InputAction 초기화 실패 시 중단
            if (!EnsureInputActionsInitialized())
            {
                return;
            }

            // 입력 장치 판별용 콜백 연결
            BindCallbacks();

            // Player Action Map 활성화
            gameplayMap.Enable();

            // 현재 입력값 즉시 한 번 읽기
            SampleActions();
        }

        private void Update()
        {
            // 입력 초기화가 안 되어 있으면 초기화 시도
            if (!EnsureInputActionsInitialized())
            {
                return;
            }

            // 매 프레임 입력값 갱신
            SampleActions();
        }

        private void OnDisable()
        {
            // Action Map 비활성화
            if (gameplayMap != null)
            {
                gameplayMap.Disable();
            }

            // 콜백 제거
            UnbindCallbacks();

            // 입력 상태 초기화
            ClearRuntimeState();
        }

        private void OnDestroy()
        {
            // Instantiate로 만든 런타임 InputActionAsset 제거
            if (runtimeInputActions != null)
            {
                Destroy(runtimeInputActions);
                runtimeInputActions = null;
            }
        }

        private bool EnsureInputActionsInitialized()
        {
            // 이미 초기화된 상태라면 true 반환
            if (runtimeInputActions != null)
            {
                return true;
            }

            // 이전에 초기화 실패했다면 다시 시도하지 않음
            if (initializationFailed)
            {
                return false;
            }

            // 사용할 InputActionAsset 찾기
            InputActionAsset sourceAsset = ResolveSourceAsset();

            // 찾지 못했다면 에러 출력
            if (sourceAsset == null)
            {
                initializationFailed = true;
                Debug.LogError("InputProvider could not resolve a Phase 1 InputActionAsset.", this);
                return false;
            }

            // 원본 에셋을 직접 쓰지 않고 복사본을 만들어 사용
            runtimeInputActions = Instantiate(sourceAsset);

            // Player Action Map 찾기
            gameplayMap = runtimeInputActions.FindActionMap(GameplayMapName, true);

            // 각 Action 찾기
            moveAction = gameplayMap.FindAction(MoveActionName, true);
            lookAction = gameplayMap.FindAction(LookActionName, true);
            pointerPositionAction = gameplayMap.FindAction(PointerPositionActionName, true);
            zoomAction = gameplayMap.FindAction(ZoomActionName, true);
            primaryHoldAction = gameplayMap.FindAction(PrimaryHoldActionName, true);
            secondaryHoldAction = gameplayMap.FindAction(SecondaryHoldActionName, true);

            // Attack은 없어도 에러를 내지 않음
            // 그래서 false 사용
            attackAction = gameplayMap.FindAction(AttackActionName, false);

            if (attackAction == null)
            {
                Debug.LogWarning(
                    $"InputProvider: '{AttackActionName}' 액션을 찾을 수 없습니다. Phase1Gameplay.inputactions를 갱신했는지 확인하세요.",
                    this);
            }

            return true;
        }

        private void BindCallbacks()
        {
            // 이미 콜백이 연결되어 있으면 중복 연결 방지
            if (callbacksBound)
            {
                return;
            }

            // performed = 입력이 실행되었을 때 호출
            moveAction.performed += OnGameplayActionPerformed;
            lookAction.performed += OnGameplayActionPerformed;
            pointerPositionAction.performed += OnGameplayActionPerformed;
            zoomAction.performed += OnGameplayActionPerformed;
            primaryHoldAction.performed += OnGameplayActionPerformed;
            secondaryHoldAction.performed += OnGameplayActionPerformed;

            if (attackAction != null)
            {
                attackAction.performed += OnGameplayActionPerformed;
            }

            callbacksBound = true;
        }

        private void UnbindCallbacks()
        {
            // 콜백이 연결되지 않았거나 moveAction이 없으면 중단
            if (!callbacksBound || moveAction == null)
            {
                callbacksBound = false;
                return;
            }

            // 연결했던 콜백 제거
            moveAction.performed -= OnGameplayActionPerformed;
            lookAction.performed -= OnGameplayActionPerformed;
            pointerPositionAction.performed -= OnGameplayActionPerformed;
            zoomAction.performed -= OnGameplayActionPerformed;
            primaryHoldAction.performed -= OnGameplayActionPerformed;
            secondaryHoldAction.performed -= OnGameplayActionPerformed;

            if (attackAction != null)
            {
                attackAction.performed -= OnGameplayActionPerformed;
            }

            callbacksBound = false;
        }

        private void OnGameplayActionPerformed(InputAction.CallbackContext context)
        {
            // 입력이 발생한 장치를 분류해서 저장
            // 예: 키보드, 마우스, 게임패드
            LastUsedDeviceKind = ClassifyControlDevice(context.control.device);
        }

        private void SampleActions()
        {
            // 좌클릭 / 우클릭 같은 Hold 입력 읽기
            IsPrimaryHeld = primaryHoldAction.IsPressed();
            IsSecondaryHeld = secondaryHoldAction.IsPressed();

            // 이동 입력 읽기
            RawMoveInput = moveAction.ReadValue<Vector2>();

            // 이동 입력 보정 후 최종 이동값 저장
            MoveInput = ApplyDualMouseForward(PrepareMoveInput(RawMoveInput));

            // 마우스 / 터치 위치 읽기
            Vector2 pointerPosition = pointerPositionAction.ReadValue<Vector2>();

            // 포인터 위치가 정상이라면 해당 위치 사용
            // 값이 없다면 화면 중앙 사용
            PointerScreenPosition = pointerPosition.sqrMagnitude > 0.0f
                ? pointerPosition
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // 조준 입력 읽기
            LookInput = lookAction.ReadValue<Vector2>();

            // Look 입력이 어떤 장치에서 왔는지 확인
            CurrentLookDeviceKind = ClassifyLookDevice(lookAction.activeControl?.device);

            // 줌 입력 읽기
            ZoomDelta = zoomAction.ReadValue<float>();

            // 이번 프레임에 공격 입력이 눌렸는지 확인
            WasAttackPressedThisFrame = attackAction != null && attackAction.WasPressedThisFrame();
        }

        private Vector2 PrepareMoveInput(Vector2 sourceMoveInput)
        {
            // 대각선 이동 시 벡터 길이가 1보다 크면 정규화
            // W + D 입력 시 (1, 1)이 되어 대각선 이동이 더 빨라지는 걸 방지
            if (normalizeDiagonalInput && sourceMoveInput.sqrMagnitude > 1.0f)
            {
                sourceMoveInput.Normalize();
            }

            return sourceMoveInput;
        }

        private Vector2 ApplyDualMouseForward(Vector2 sourceMoveInput)
        {
            // 좌클릭 + 우클릭 앞으로 이동 기능이 꺼져 있거나
            // 두 입력을 동시에 누르지 않았다면 원래 이동값 반환
            if (!enableDualMouseForwardMove || !IsDualInputForwardHeld)
            {
                return sourceMoveInput;
            }

            // 기존 이동 입력에 앞으로 이동값 추가
            Vector2 composedInput = sourceMoveInput + Vector2.up * dualMouseForwardAmount;

            // 추가된 이동값도 대각선 속도 증가 방지를 위해 정규화
            if (normalizeDiagonalInput && composedInput.sqrMagnitude > 1.0f)
            {
                composedInput.Normalize();
            }

            return composedInput;
        }

        private void ClearRuntimeState()
        {
            // 모든 입력 상태 초기화
            RawMoveInput = Vector2.zero;
            MoveInput = Vector2.zero;
            PointerScreenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            LookInput = Vector2.zero;
            ZoomDelta = 0.0f;
            IsPrimaryHeld = false;
            IsSecondaryHeld = false;
            LastUsedDeviceKind = ControlDeviceKind.None;
            CurrentLookDeviceKind = LookDeviceKind.None;
            WasAttackPressedThisFrame = false;
        }

        private InputActionAsset ResolveSourceAsset()
        {
            // 인스펙터에 직접 연결된 에셋이 있으면 그것을 사용
            if (inputActionsAsset != null)
            {
                return inputActionsAsset;
            }

            // 없으면 Resources 폴더에서 찾기
            if (!string.IsNullOrWhiteSpace(resourcesFallbackPath))
            {
                inputActionsAsset = Resources.Load<InputActionAsset>(resourcesFallbackPath);
            }

            return inputActionsAsset;
        }

        private static LookDeviceKind ClassifyLookDevice(InputDevice device)
        {
            // 장치가 없으면 None
            if (device == null)
            {
                return LookDeviceKind.None;
            }

            // 마우스, 펜, 터치스크린은 Pointer로 분류
            if (device is Mouse || device is Pen || device is Touchscreen)
            {
                return LookDeviceKind.Pointer;
            }

            // 게임패드는 Gamepad로 분류
            if (device is Gamepad)
            {
                return LookDeviceKind.Gamepad;
            }

            // 나머지는 Other
            return LookDeviceKind.Other;
        }

        private static ControlDeviceKind ClassifyControlDevice(InputDevice device)
        {
            // 장치가 없으면 None
            if (device == null)
            {
                return ControlDeviceKind.None;
            }

            // 게임패드는 Gamepad
            if (device is Gamepad)
            {
                return ControlDeviceKind.Gamepad;
            }

            // 키보드, 마우스, 펜, 터치스크린은 KeyboardMouse로 분류
            if (device is Keyboard || device is Mouse || device is Pen || device is Touchscreen)
            {
                return ControlDeviceKind.KeyboardMouse;
            }

            // 나머지는 Other
            return ControlDeviceKind.Other;
        }

        // UNITY_EDITOR일 때만 실행되는 메서드
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void TryAssignDefaultAssetInEditor()
        {
            // 이미 연결되어 있으면 중단
            if (inputActionsAsset != null)
            {
                return;
            }

#if UNITY_EDITOR
            // 에디터에서 기본 InputActionAsset 자동 연결
            inputActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(DefaultActionAssetEditorPath);
#endif
        }
    }
}