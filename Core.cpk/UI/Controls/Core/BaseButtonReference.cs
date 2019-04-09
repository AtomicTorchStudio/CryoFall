namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;

    public abstract class BaseButtonReference<TGameButton> : IButtonReference
        where TGameButton : struct, Enum
    {
        public IWrappedButton AbstractButton => WrappedButton<TGameButton>.GetWrappedButton(this.Button);

        public TGameButton Button { get; set; }
    }
}