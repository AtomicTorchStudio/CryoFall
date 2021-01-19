namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum ObjectLightMode : byte
    {
        [Description(CoreStrings.TitleModeOff)]
        Off,

        [Description(CoreStrings.TitleModeAlwaysOn)]
        On,

        [Description(CoreStrings.TitleModeAuto)]
        Auto
    }
}