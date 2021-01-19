namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum SprinklerWateringRequestResult : byte
    {
        Success = 0,

        // never displayed in the UI
        ErrorNotInteracting = 1,

        [Description("Not enough water in the sprinkler.")]
        ErrorNotEnoughWater = 2,

        // re-use description string from SetPowerModeResult
        ErrorNotEnoughElectricity = 3,

        // never displayed in the UI
        ErrorWateredRecently = 4,

        [Description(CoreStrings.PowerConsumerState_PowerOff_Title)]
        ErrorPowerOff = 5
    }
}