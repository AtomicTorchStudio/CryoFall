namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// Standard stack sizes used in the game for all items.
    /// </summary>
    public class ItemStackSize : ProtoEntity
    {
        private static double itemStackSizeMultiplier = 1.0;

        static ItemStackSize()
        {
            if (Api.IsClient)
            {
                return;
            }

            const string key = "ItemStackSizeMultiplier";
            const double defaultValue = 1.0,
                         minValue = 1.0,
                         maxValue = 50;

            var description
                = $@"Item stack size multiplier.
                     For example, by default one slot can contain up to 250 stone.
                     You can increase this number by raising this multiplier.
                     (allowed range: from {minValue:0.0###} to {maxValue:0.0###}).";
            itemStackSizeMultiplier = ServerRates.Get(key,
                                                      defaultValue,
                                                      description);

            if (itemStackSizeMultiplier < minValue
                || itemStackSizeMultiplier > maxValue)
            {
                itemStackSizeMultiplier = defaultValue;
                ServerRates.Reset(key,
                                  defaultValue,
                                  description);
            }

            SharedApplyRates();
            Logger.Important($"Item stack size multiplier from rates config: x{itemStackSizeMultiplier:0.##}");
        }

        // see the default values defined below 
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
                Server.Characters.PlayerOnlineStateChanged += this.ServerPlayerOnlineStateChangedHandler;
            }
        }

        private static ushort ApplyRate(ushort defaultValue)
        {
            return (ushort)MathHelper.Clamp(defaultValue * itemStackSizeMultiplier,
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

        private void ClientRemote_SetItemStackSizeMultiplier(double multiplier)
        {
            Logger.Important($"Received item stack size multiplier from server: x{multiplier:0.##}");
            itemStackSizeMultiplier = multiplier;
            SharedApplyRates();
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (isOnline)
            {
                this.CallClient(character,
                                _ => this.ClientRemote_SetItemStackSizeMultiplier(itemStackSizeMultiplier));
            }
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