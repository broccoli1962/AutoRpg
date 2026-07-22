using UnityEngine;
using UnityEngine.InputSystem;

namespace Backend.Util
{
    /// <summary>
    /// Input System 전용 프로젝트에서 KeyCode 단축키 폴링을 안전하게 처리한다.
    /// </summary>
    public static class KeyboardInputUtil
    {
        public static bool WasKeyPressedThisFrame(KeyCode keyCode)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            var key = ToInputSystemKey(keyCode);
            return key != Key.None && keyboard[key].wasPressedThisFrame;
        }

        public static bool WasAnyKeyPressedThisFrame(KeyCode keyCodeA, KeyCode keyCodeB) =>
            WasKeyPressedThisFrame(keyCodeA) || WasKeyPressedThisFrame(keyCodeB);

        private static Key ToInputSystemKey(KeyCode keyCode) =>
            keyCode switch
            {
                KeyCode.A => Key.A,
                KeyCode.B => Key.B,
                KeyCode.C => Key.C,
                KeyCode.E => Key.E,
                KeyCode.F => Key.F,
                KeyCode.G => Key.G,
                KeyCode.I => Key.I,
                KeyCode.L => Key.L,
                KeyCode.O => Key.O,
                KeyCode.Q => Key.Q,
                KeyCode.R => Key.R,
                KeyCode.Alpha0 => Key.Digit0,
                KeyCode.Alpha1 => Key.Digit1,
                KeyCode.Alpha2 => Key.Digit2,
                KeyCode.Alpha3 => Key.Digit3,
                KeyCode.Alpha4 => Key.Digit4,
                KeyCode.Alpha5 => Key.Digit5,
                KeyCode.Alpha6 => Key.Digit6,
                KeyCode.Alpha7 => Key.Digit7,
                KeyCode.Alpha8 => Key.Digit8,
                KeyCode.Alpha9 => Key.Digit9,
                KeyCode.Keypad0 => Key.Numpad0,
                KeyCode.Keypad1 => Key.Numpad1,
                KeyCode.Keypad2 => Key.Numpad2,
                KeyCode.Keypad3 => Key.Numpad3,
                KeyCode.Keypad4 => Key.Numpad4,
                KeyCode.Keypad5 => Key.Numpad5,
                KeyCode.Keypad6 => Key.Numpad6,
                KeyCode.Keypad7 => Key.Numpad7,
                KeyCode.Keypad8 => Key.Numpad8,
                KeyCode.Keypad9 => Key.Numpad9,
                KeyCode.Minus => Key.Minus,
                KeyCode.KeypadMinus => Key.NumpadMinus,
                KeyCode.Comma => Key.Comma,
                KeyCode.Period => Key.Period,
                KeyCode.LeftBracket => Key.LeftBracket,
                KeyCode.RightBracket => Key.RightBracket,
                KeyCode.PageUp => Key.PageUp,
                KeyCode.PageDown => Key.PageDown,
                KeyCode.Escape => Key.Escape,
                _ => Key.None
            };
    }
}
