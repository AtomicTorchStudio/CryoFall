namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data
{
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class InputKeyNameHelper
    {
        public const string Title_CapsLock = "Caps Lock";

        public const string Title_LeftMouseButton = "Left mouse button";

        public const string Title_MouseScrollButton = "Mouse scroll button";

        public const string Title_PageDown = "Page Down";

        public const string Title_PageUp = "Page Up";

        public const string Title_RightMouseButton = "Right mouse button";

        public const string Title_SpaceBar = "Space bar";

        public static string GetKeyText(InputKey key)
        {
            if (key >= InputKey.D0
                && key <= InputKey.D9)
            {
                return key.ToString().TrimStart('D');
            }

            switch (key)
            {
                case InputKey.None:
                    return string.Empty;

                case InputKey.MouseLeftButton:
                    return Title_LeftMouseButton;

                case InputKey.MouseRightButton:
                    return Title_RightMouseButton;

                case InputKey.MouseScrollButton:
                    return Title_MouseScrollButton;

                case InputKey.Space:
                    return Title_SpaceBar;

                case InputKey.CapsLock:
                    return Title_CapsLock;

                case InputKey.OemTilde:
                    return "~";
                
                case InputKey.CircumflexAccent:
                    return '\u005E'.ToString();

                case InputKey.OemMinus:
                    return "-";

                case InputKey.OemPlus:
                    return "+";

                case InputKey.OemComma:
                    return ",";

                case InputKey.OemPeriod:
                    return ".";

                case InputKey.OemQuestion:
                    return "?";

                case InputKey.PageUp:
                    return Title_PageUp;

                case InputKey.PageDown:
                    return Title_PageDown;

                default:
                    return key.ToString();
            }
        }
    }
}