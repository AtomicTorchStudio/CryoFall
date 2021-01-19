namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data
{
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class InputKeyNameHelper
    {
        public const string Title_CapsLock = "Caps Lock";

        public const string Title_LeftMouseButton = "Left mouse button";

        public const string Title_MouseExtraButton1 = "Mouse extra button 1";

        public const string Title_MouseExtraButton2 = "Mouse extra button 2";

        public const string Title_MouseScrollButton = "Mouse scroll button";

        public const string Title_PageDown = "Page Down";

        public const string Title_PageUp = "Page Up";

        public const string Title_RightMouseButton = "Right mouse button";

        public const string Title_SpaceBar = "Space bar";

        // Key or button not assigned (has no binding in the game controls options).
        public const string TitlePlaceholder = "not assigned";

        public static string GetKeyText(InputKey key, bool returnPlaceholderIfNone = true)
        {
            if (key >= InputKey.D0
                && key <= InputKey.D9)
            {
                return key.ToString().TrimStart('D');
            }

            return key switch
            {
                InputKey.None => returnPlaceholderIfNone
                                     ? TitlePlaceholder
                                     : string.Empty,
                InputKey.MouseLeftButton   => Title_LeftMouseButton,
                InputKey.MouseRightButton  => Title_RightMouseButton,
                InputKey.MouseScrollButton => Title_MouseScrollButton,
                InputKey.MouseExtraButton1 => Title_MouseExtraButton1,
                InputKey.MouseExtraButton2 => Title_MouseExtraButton2,
                InputKey.Space             => Title_SpaceBar,
                InputKey.CapsLock          => Title_CapsLock,
                InputKey.OemTilde          => "~",
                InputKey.CircumflexAccent  => '\u005E'.ToString(),
                InputKey.OemMinus          => "-",
                InputKey.OemPlus           => "+",
                InputKey.OemComma          => ",",
                InputKey.OemPeriod         => ".",
                InputKey.OemSemicolon      => ";",
                InputKey.OemQuestion       => "?",
                InputKey.OemPipe           => "|",
                InputKey.OemOpenBrackets   => "[",
                InputKey.OemCloseBrackets  => "]",
                InputKey.PageUp            => Title_PageUp,
                InputKey.PageDown          => Title_PageDown,
                _                          => key.ToString()
            };
        }
    }
}