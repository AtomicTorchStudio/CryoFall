namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemPragmiumSensor
        : ProtoItemTool<ItemPragmiumSensorPrivateState,
              EmptyPublicState,
              EmptyClientState>,
          IProtoItemWithHotbarOverlay
    {
        private const byte MaxNumberOfPongsPerScan = 2;

        /// <summary>
        /// Scan pragmium sources once a second.
        /// </summary>
        private const double ServerScanInterval = 3.0;

        /// <summary>
        /// Number of signal levels (gradations).
        /// </summary>
        private const byte SignalLevelsNumber = 5;

        private static readonly Lazy<ObjectMineralPragmiumSource> LazyPragmiumSource
            = new Lazy<ObjectMineralPragmiumSource>(Api.GetProtoEntity<ObjectMineralPragmiumSource>);

        public static event Action<IItem, PragmiumSensorSignalKind> ServerSignalReceived;

        public override bool CanBeSelectedInVehicle => true;

        public abstract ushort DurabilityDecreasePerSecond { get; }

        public abstract double EnergyConsumptionPerSecond { get; }

        /// <summary>
        /// Max range of the sensor. Pragmium spires beyond that distance will be not detected.
        /// </summary>
        public abstract double MaxRange { get; }

        public override double ServerUpdateIntervalSeconds => 0.1;

        public override double ServerUpdateRareIntervalSeconds => 10;

        protected virtual SoundResource SoundResourcePing { get; }
            = new SoundResource("Items/Tools/PragmiumSensor/Ping");

        protected virtual SoundResource SoundResourcePong { get; }
            = new SoundResource("Items/Tools/PragmiumSensor/Pong");

        protected float SoundVolume => 0.5f;

        public Control ClientCreateHotbarOverlayControl(IItem item)
        {
            return new HotbarItemPragmiumSensorOverlayControl(item);
        }

        public override void ServerItemHotbarSelectionChanged(
            IItem item,
            ICharacter character,
            bool isSelected)
        {
            if (isSelected)
            {
                GetPrivateState(item).ServerTimeToPing = 0;
                this.ServerSetUpdateRate(item, isRare: false);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var item = data.GameObject;
            var privateState = data.PrivateState;

            var character = item.Container.OwnerAsCharacter;
            if (!IsItemSelectedByPlayer(character, item))
            {
                // not a selected player item
                this.ServerSetUpdateRate(item, isRare: true);
                return;
            }

            privateState.ServerTimeToPing -= data.DeltaTime;
            if (privateState.ServerTimeToPing > 0)
            {
                return;
            }

            privateState.ServerTimeToPing = ServerScanInterval;

            if (!CharacterEnergySystem.ServerDeductEnergyCharge(
                    character,
                    requiredEnergyAmount: this.EnergyConsumptionPerSecond * ServerScanInterval))

            {
                // no power
                this.CallClient(character, _ => _.ClientRemote_NoPower());
                return;
            }

            ItemDurabilitySystem.ServerModifyDurability(
                item,
                delta: -(int)Math.Round(this.DurabilityDecreasePerSecond * ServerScanInterval));

            if (item.IsDestroyed)
            {
                // zero durability reached
                return;
            }

            // update signal strength
            using var tempSignalStrength = Api.Shared.GetTempList<byte>();
            this.ServerCalculateStrengthToTheClosestPragmiumSpires(character,
                                                                   tempSignalStrength.AsList(),
                                                                   MaxNumberOfPongsPerScan);

            var previousSignalStrength = -1;
            foreach (var signalStrength in tempSignalStrength.AsList())
            {
                if (signalStrength == previousSignalStrength)
                {
                    // don't send multiple pongs for the signals of the same strength
                    continue;
                }

                previousSignalStrength = signalStrength;

                var serverTimeToPong = SharedCalculateTimeToPong(signalStrength);
                ServerTimersSystem.AddAction(
                    serverTimeToPong,
                    () =>
                    {
                        var currentCharacter = item.Container.OwnerAsCharacter;
                        if (IsItemSelectedByPlayer(currentCharacter, item))
                        {
                            this.CallClient(currentCharacter,
                                            _ => _.ClientRemote_OnSignal(item,
                                                                         PragmiumSensorSignalKind.Pong));
                        }
                    });

                //Logger.Dev(string.Format("Pragmium scanner signal: {0} strength. Time to send pong: {1} ms.",
                //                         signalStrength,
                //                         (int)(serverTimeToPong * 1000)));
            }

            this.CallClient(character, _ => _.ClientRemote_OnSignal(item, PragmiumSensorSignalKind.Ping));

            bool IsItemSelectedByPlayer(ICharacter c, IItem i)
                => c is not null
                   && ReferenceEquals(i, c.SharedGetPlayerSelectedHotbarItem());
        }

        private static double SharedCalculateTimeToPong(byte signalStrength)
        {
            if (signalStrength == 0
                || signalStrength > SignalLevelsNumber)
            {
                return 0;
            }

            return (1 + SignalLevelsNumber - signalStrength) * 0.5 * ServerScanInterval / SignalLevelsNumber;
        }

        private void ClientRemote_NoPower()
        {
            if (!ReferenceEquals(this, ClientCurrentCharacterHelper.PublicState?.SelectedItem?.ProtoItem))
            {
                return;
            }

            CharacterEnergySystem.ClientShowNotificationNotEnoughEnergyCharge(this);
        }

        private void ClientRemote_OnSignal(IItem item, PragmiumSensorSignalKind signalKind)
        {
            if (!ReferenceEquals(item, ClientCurrentCharacterHelper.PublicState?.SelectedItem))
            {
                return;
            }

            var soundResource = signalKind switch
            {
                PragmiumSensorSignalKind.Ping => this.SoundResourcePing,
                PragmiumSensorSignalKind.Pong => this.SoundResourcePong,
                _ => throw new ArgumentOutOfRangeException(nameof(signalKind), signalKind, null)
            };

            Api.Client.Audio.PlayOneShot(soundResource,
                                         volume: this.SoundVolume);

            Api.SafeInvoke(() => ServerSignalReceived?.Invoke(item, signalKind));
        }

        private void ServerCalculateDistanceSqrToTheClosestPragmiumSpires(
            ICharacter character,
            Vector2Ushort position,
            List<double> closestSqrDistances)
        {
            var isObjectClaimSystemEnabled = WorldObjectClaimSystem.SharedIsEnabled;
            using var tempList = Api.Shared.GetTempList<IStaticWorldObject>();
            LazyPragmiumSource.Value.GetAllGameObjects(tempList.AsList());

            foreach (var staticWorldObject in tempList.AsList())
            {
                var distanceSqr = position.TileSqrDistanceTo(staticWorldObject.TilePosition);
                if (distanceSqr >= this.MaxRange * this.MaxRange)
                {
                    // too far
                    continue;
                }

                if (isObjectClaimSystemEnabled
                    && !WorldObjectClaimSystem.SharedIsAllowInteraction(character,
                                                                        staticWorldObject,
                                                                        showClientNotification: false))
                {
                    // this pragmium source is claimed for another player
                    continue;
                }

                closestSqrDistances.Add(distanceSqr);
            }

            closestSqrDistances.Sort();
        }

        private void ServerCalculateStrengthToTheClosestPragmiumSpires(
            ICharacter character,
            List<byte> results,
            int maxNumberOfSpires)
        {
            using var tempSqrDistances = Api.Shared.GetTempList<double>();
            var listSqrResults = tempSqrDistances.AsList();
            this.ServerCalculateDistanceSqrToTheClosestPragmiumSpires(character,
                                                                      character.TilePosition,
                                                                      listSqrResults);

            for (var index = 0;
                 index < Math.Min(maxNumberOfSpires, listSqrResults.Count);
                 index++)
            {
                var distanceSqr = listSqrResults[index];
                var distance = Math.Sqrt(distanceSqr);
                var signalStrength = this.SharedConvertDistanceToSignalStrength(distance);
                results.Add(signalStrength);
            }
        }

        private byte SharedConvertDistanceToSignalStrength(double distance)
        {
            if (distance < 0)
            {
                // certainly no objects nearby
                return 0;
            }

            var d = this.MaxRange - distance;
            if (d <= 0)
            {
                // no signal
                return 0;
            }

            return (byte)Math.Ceiling(SignalLevelsNumber * d / this.MaxRange);
        }
    }
}