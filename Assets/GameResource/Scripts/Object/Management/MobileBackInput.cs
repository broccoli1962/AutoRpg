using Backend.Object.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Backend.Object.Management
{
    /// <summary>
    /// Android 뒤로가기 / UI Cancel 입력을 UIManager.PopBack 에 연결한다.
    /// 키보드 ESC 전용 폴링 없이 Input System UI Cancel 액션만 사용한다.
    /// </summary>
    public sealed class MobileBackInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _inputActions;

        private InputAction _cancelAction;

        private void Awake()
        {
            if (_inputActions == null)
                return;

            var uiMap = _inputActions.FindActionMap("UI", throwIfNotFound: false);
            if (uiMap == null)
                return;

            _cancelAction = uiMap.FindAction("Cancel", throwIfNotFound: false);
            if (_cancelAction == null)
                return;

            _cancelAction.performed += OnCancelPerformed;
        }

        private void OnEnable()
        {
            _cancelAction?.Enable();
        }

        private void OnDisable()
        {
            _cancelAction?.Disable();
        }

        private void OnDestroy()
        {
            if (_cancelAction != null)
                _cancelAction.performed -= OnCancelPerformed;
        }

        private static void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (!context.performed || GameStateUtil.IsQuitting)
                return;

            UIManager.PopBack();
        }
    }
}
