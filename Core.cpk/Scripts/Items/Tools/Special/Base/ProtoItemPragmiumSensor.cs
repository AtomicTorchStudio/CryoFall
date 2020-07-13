namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using System;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
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
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var item = data.GameObject;
            var privateState = data.PrivateState;

            var character = item.Container.OwnerAsCharacter;
            if (character is null
                || !ReferenceEquals(item, character.SharedGetPlayerSelectedHotbarItem()))
            {
                // not a selected player item
                privateState.ServerCurrentSignalStrength = 0;
                return;
            }

            privateState.ServerTimeToPing -= data.DeltaTime;
            if (privateState.ServerTimeToPing > 0)
            {
                if (privateState.ServerTimeToPong <= 0)
                {
                    // pong signal is already sent or no pong signal should be sent
                    return;
                }

                privateState.ServerTimeToPong -= data.DeltaTime;
                if (privateState.ServerTimeToPong <= 0)
                {
                    // time to send pong
                    this.CallClient(character, _ => _.ClientRemote_OnSignal(item, PragmiumSensorSignalKind.Pong));
                }

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
            var distance = this.ServerCalculateDistanceToTheClosestPragmiumSpire(character.TilePosition);
            var signalStrength = this.SharedConvertDistanceToSignalStrength(distance);
            privateState.ServerTimeToPong = SharedCalculateTimeToPong(signalStrength);

            if (privateState.ServerCurrentSignalStrength != signalStrength)
            {
                privateState.ServerCurrentSignalStrength = signalStrength;
                //Logger.Dev(string.Format("Signal strength changed: {0}. Time to send pong: {1} ms.",
                //                         signalStrength,
                //                         (int)(privateState.ServerTimeToPong * 1000)));
            }

            this.CallClient(character, _ => _.ClientRemote_OnSignal(item, PragmiumSensorSignalKind.Ping));
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

        private double ServerCalculateDistanceToTheClosestPragmiumSpire(Vector2Ushort position)
        {
            using var tempList = Api.Shared.GetTempList<IStaticWorldObject>();
            LazyPragmiumSource.Value.GetAllGameObjects(tempList.AsList());

            var closestDistanceSqr = double.MaxValue;
            foreach (var staticWorldObject in tempList.AsList())
            {
                var distanceSqr = position.TileSqrDistanceTo(staticWorldObject.TilePosition);
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                }
            }

            if (closestDistanceSqr >= this.MaxRange * this.MaxRange)
            {
                // the closest object is too far
                return -1;
            }

            return Math.Sqrt(closestDistanceSqr);
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