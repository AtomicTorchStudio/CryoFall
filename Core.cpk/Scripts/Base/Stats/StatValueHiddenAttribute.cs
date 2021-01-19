namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;

    /// <summary>
    /// Used to decorate StatName entries that have a hidden value (such as various perks).
    /// If it's applied, the tooltip will hide the stat value (such as "+1").
    /// </summary>
    public class StatValueHiddenAttribute : Attribute
    {
    }
}