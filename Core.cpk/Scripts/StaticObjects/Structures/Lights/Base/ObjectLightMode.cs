namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;

    public enum ObjectLightMode : byte
    {
        [Description(CoreStrings.TitleModeOff)]
        Off,

        [Description(CoreStrings.TitleModeOn)]
        On,

        [Description(CoreStrings.TitleModeAuto)]
        Auto
    }
}