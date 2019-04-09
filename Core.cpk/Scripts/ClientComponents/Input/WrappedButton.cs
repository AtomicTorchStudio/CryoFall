namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System;
    using System.Collections.Generic;

    internal class WrappedButton<TButton> : IWrappedButton
        where TButton : struct, Enum
    {
        private static readonly Dictionary<TButton, WrappedButton<TButton>> WrappedButtons
            = new Dictionary<TButton, WrappedButton<TButton>>();

        public readonly TButton Button;

        private WrappedButton(TButton button)
        {
            this.Button = button;
        }

        public string WrappedButtonName => this.Button.ToString();

        public Type WrappedButtonType => typeof(TButton);

        public static WrappedButton<TButton> GetWrappedButton(TButton button)
        {
            if (WrappedButtons.TryGetValue(button, out var wrappedButton))
            {
                return wrappedButton;
            }

            wrappedButton = new WrappedButton<TButton>(button);
            WrappedButtons[button] = wrappedButton;
            return wrappedButton;
        }

        public override string ToString()
        {
            return this.Button.ToString();
        }
    }
}