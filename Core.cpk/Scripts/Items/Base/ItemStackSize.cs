namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// Standard stack sizes used in the game for all items.
    /// </summary>
    public class ItemStackSize : ProtoEntity
    {
        static ItemStackSize()
        {
            if (Api.IsServer)
            {
                SharedApplyRates();
            }
        }

        // Do NOT edit these properties here, see the default values defined below.
        public static ushort Big { get; private set; } = Defaults.Big;

        public static ushort Huge { get; private set; } = Defaults.Huge;

        public static ushort Medium { get; private set; } = Defaults.Medium;

        public static ushort Small { get; private set; } = Defaults.Small;

        [NotLocalizable]
        public override string Name => nameof(ItemStackSize);

        protected override void PrepareProto()
        {
            if (IsServer)
            {
                return;
            }

            RateItemStackSizeMultiplier.ClientValueChanged += SharedApplyRates;
            if (RateItemStackSizeMultiplier.ClientIsValueReceived)
            {
                SharedApplyRates();
            }
        }

        private static ushort ApplyRate(ushort defaultValue)
        {
            return (ushort)MathHelper.Clamp(defaultValue * RateItemStackSizeMultiplier.SharedValue,
                                            min: 1,
                                            max: ushort.MaxValue);
        }

        private static void SharedApplyRates()
        {
            Small = ApplyRate(Defaults.Small);
            Medium = ApplyRate(Defaults.Medium);
            Big = ApplyRate(Defaults.Big);
            Huge = ApplyRate(Defaults.Huge);
        }

        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        private static class Defaults
        {
            /// <summary>
            /// Big item stack - 250 max.
            /// Used for ores and other things that are acquired in large quantities.
            /// </summary>
            public const ushort Big = 250;

            /// <summary>
            /// Highest possible size for the item stack (1000).
            /// Used for things like coins.
            /// </summary>
            public const ushort Huge = 1000;

            /// <summary>
            /// Medium item stack - 100 max.
            /// Used as default stack size for most item types.
            /// </summary>
            public const ushort Medium = 100;

            /// <summary>
            /// Small item stack - 10 max.
            /// Used for items that we want to be stored in smaller numbers, such as food, meds, etc.
            /// </summary>
            public const ushort Small = 10;
        }
    }
}