namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class VehicleEnergyConsumptionSystem : ProtoSystem<VehicleEnergyConsumptionSystem>
    {
        public const string Notification_EnergyDepleted_Message = "Energy depleted.";

        public static readonly SoundResource SoundResourceNoEnergy
            = new("Objects/Vehicles/NoEnergy");

        public static void ClientShowNotificationNotEnoughEnergy(IProtoVehicle protoVehicle)
        {
            Client.Audio.PlayOneShot(SoundResourceNoEnergy, volume: 0.4f);

            NotificationSystem.ClientShowNotification(
                string.Format(VehicleSystem.Notification_CannotUseVehicle_TitleFormat, protoVehicle.Name),
                Notification_EnergyDepleted_Message,
                color: NotificationColor.Bad,
                icon: protoVehicle.Icon,
                playSound: false);
        }

        public static void ServerNotifyClientNotEnoughEnergy(ICharacter character, IProtoVehicle protoVehicle)
        {
            Instance.CallClient(character,
                                _ => _.ClientRemote_OnNotEnoughEnergy(protoVehicle));
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will consume vehicle energy
                return;
            }

            TriggerEveryFrame.ServerRegister(
                callback: ServerUpdate,
                name: "System." + this.ShortId);
        }

        private static void ServerUpdate()
        {
            // perform update once per second per player
            const double spreadDeltaTime = 1;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (!character.ServerIsOnline)
                {
                    continue;
                }

                var vehicle = character.SharedGetCurrentVehicle();
                if (vehicle is null)
                {
                    continue;
                }

                var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
                var isMoving = vehicle.PhysicsBody.Velocity != Vector2D.Zero;

                var energyConsumptionRate = character.SharedGetFinalStatMultiplier(StatName.VehicleFuelConsumptionRate);
                energyConsumptionRate = Math.Max(energyConsumptionRate, 0);

                var energyConsumption = protoVehicle.SharedGetCurrentEnergyConsumption(vehicle);
                energyConsumption = energyConsumption * energyConsumptionRate * spreadDeltaTime;

                if (VehicleEnergySystem.ServerDeductEnergyCharge(
                    vehicle,
                    requiredEnergyAmount: (ushort)Math.Floor(energyConsumption)))
                {
                    // consumed energy
                    if (isMoving)
                    {
                        character.ServerAddSkillExperience<SkillVehicles>(
                            SkillVehicles.ExperienceForDrivingVehiclePerSecond * spreadDeltaTime);
                    }

                    continue;
                }

                // not enough power charge - remove vehicle's pilot
                if (vehicle.GetPublicState<VehiclePublicState>().IsDismountRequested)
                {
                    // quit requested/vehicle cannot be used and awaiting pilot's quit
                    continue;
                }

                VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: false);
                Instance.CallClient(character,
                                    _ => _.ClientRemote_OnNotEnoughEnergy(protoVehicle));
            }
        }

        private void ClientRemote_OnNotEnoughEnergy(IProtoVehicle protoVehicle)
        {
            ClientShowNotificationNotEnoughEnergy(protoVehicle);
        }
    }
}