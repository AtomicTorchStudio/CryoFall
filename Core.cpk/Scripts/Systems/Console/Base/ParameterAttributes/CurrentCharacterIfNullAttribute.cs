namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class CurrentCharacterIfNullAttribute : Attribute
    {
    }
}