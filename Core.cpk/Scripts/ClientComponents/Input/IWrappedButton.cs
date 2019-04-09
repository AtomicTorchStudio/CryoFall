namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System;

    public interface IWrappedButton
    {
        string WrappedButtonName { get; }

        Type WrappedButtonType { get; }
    }
}