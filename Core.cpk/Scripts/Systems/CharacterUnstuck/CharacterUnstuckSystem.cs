namespace AtomicTorch.CBND.CoreMod.Systems.CharacterUnstuck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CharacterUnstuckSystem : ProtoSystem<CharacterUnstuckSystem>
    {
        public const string NotificationAlreadyRequestedUnstuck_Message =
            "Please wait until your request is processed.";

        public const string NotificationAlreadyRequestedUnstuck_Title =
            "You've already requested unstuck";

        public const string NotificationUnstuckCancelled_Message =
            "You should not move your character while timer is active. Unstuck request has been cancelled.";

        public const string NotificationUnstuckCancelled_Title =
            "Unstuck cancelled";

        public const string NotificationUnstuckImpossible_Dead =
            "Your character is dead.";

        public const string NotificationUnstuckImpossible_Title =
            "Unstuck impossible";

        public const string NotificationUnstuckRequested_MessageFormat =
            @"Please wait for {0}
              [br]Don't move, otherwise your unstuck request will be cancelled.";

        public const string NotificationUnstuckRequested_Title =
            "Unstuck requested";

        public const string NotificationUnstuckSuccessful_Message =
            "You're free now. Enjoy!";

        public const string NotificationUnstuckSuccessful_Title =
            "Unstuck successful";

        public const double UnstuckDelaySecondsOnEnemyBaseTotal = 10 * 60; // 10 minutes

        public const double UnstuckDelaySecondsTotal = 5 * 60; // 5 minutes

        private const double MaxUnstuckMovementDistance = 1.5;

        private static readonly Dictionary<ICharacter, CharacterUnstuckRequest> serverRequests
            = IsServer
                  ? new Dictionary<ICharacter, CharacterUnstuckRequest>()
                  : null;

        private static HUDNotificationControl ClientCurrentUnstuckNotification;

        public override string Name => "Character unstuck system";

        public static void ClientCreateUnstuckRequest()
        {
            if (!SharedValidateCanUnstuck(ClientCurrentCharacterHelper.Character))
            {
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_CreateUnstuckRequest());
        }

        public static void ServerTryCancelUnstuckRequest(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            if (!serverRequests.Remove(character))
            {
                // no unstuck requests
                return;
            }

            ServerOnUnstuckRequestRemoved(character);
            ServerNotifyUnstuckCancelledCharacterMoved(character);
        }

        public static bool SharedValidateCanUnstuck(ICharacter character)
        {
            using var tempAreas = Api.Shared.GetTempList<ILogicObject>();
            var bounds = new RectangleInt(
                offset: character.TilePosition - (1, 1),
                size: (2, 2));

            LandClaimSystem.SharedGetAreasInBounds(bounds, tempAreas, addGracePadding: false);
            if (tempAreas.AsList().Any(LandClaimSystem.SharedIsAreaUnderRaid))
            {
                Logger.Info("Cannot unstuck when located in an area under raid", character);
                LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(character);
                return false;
            }

            return true;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                ClientRefreshCurrentUnstuckRequestStatus();
            }
            else
            {
                // configure server time interval trigger
                TriggerTimeInterval.ServerConfigureAndRegister(
                    interval: TimeSpan.FromSeconds(1),
                    callback: this.ServerTimerTickCallback,
                    name: "System." + this.ShortId);
            }
        }

        private static void ClientRefreshCurrentUnstuckRequestStatus()
        {
            // invoke self again a second later
            ClientTimersSystem.AddAction(delaySeconds: 1,
                                         ClientRefreshCurrentUnstuckRequestStatus);

            var characterPublicState = ClientCurrentCharacterHelper.PublicState;
            if (ClientCurrentUnstuckNotification != null)
            {
                if (ClientCurrentUnstuckNotification.IsHiding)
                {
                    ClientCurrentUnstuckNotification = null;
                }
                else if (ClientCurrentUnstuckNotification.Tag != characterPublicState)
                {
                    // outdated notification
                    ClientCurrentUnstuckNotification?.Hide(quick: false);
                    ClientCurrentUnstuckNotification = null;
                }
            }

            if (characterPublicState == null)
            {
                ClientCurrentUnstuckNotification?.Hide(quick: false);
                ClientCurrentUnstuckNotification = null;
                return;
            }

            var timeRemains = characterPublicState.UnstuckExecutionTime
                              - Client.CurrentGame.ServerFrameTimeApproximated;

            if (timeRemains <= 0)
            {
                // no active unstuck requests
                ClientCurrentUnstuckNotification?.Hide(quick: false);
                ClientCurrentUnstuckNotification = null;
                return;
            }

            // has active unstuck request, create or update the notification
            var message = string.Format(NotificationUnstuckRequested_MessageFormat,
                                        ClientTimeFormatHelper.FormatTimeDuration(timeRemains));

            if (ClientCurrentUnstuckNotification == null)
            {
                ClientCurrentUnstuckNotification = NotificationSystem.ClientShowNotification(
                    NotificationUnstuckRequested_Title,
                    message,
                    autoHide: false);
                ClientCurrentUnstuckNotification.Tag = characterPublicState;
            }
            else
            {
                ClientCurrentUnstuckNotification.SetMessage(message);
            }
        }

        private static void ServerNotifyUnstuckCancelledCharacterMoved(ICharacter character)
        {
            Instance.CallClient(character,
                                _ => _.ClientRemote_UnstuckFailedCharacterMoved());
        }

        private static void ServerOnUnstuckRequestRemoved(ICharacter character)
        {
            PlayerCharacter.GetPublicState(character)
                           .UnstuckExecutionTime = 0;
        }

        private void ClientRemote_UnstuckAlreadyQueued()
        {
            NotificationSystem.ClientShowNotification(
                NotificationAlreadyRequestedUnstuck_Title,
                NotificationAlreadyRequestedUnstuck_Message);
        }

        private void ClientRemote_UnstuckFailedCharacterMoved()
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckCancelled_Title,
                NotificationUnstuckCancelled_Message,
                NotificationColor.Bad);
        }

        private void ClientRemote_UnstuckFailedDead()
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckImpossible_Title,
                NotificationUnstuckImpossible_Dead,
                NotificationColor.Bad);
        }

        private void ClientRemote_UnstuckImpossible()
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckImpossible_Title,
                color: NotificationColor.Bad);
        }

        private void ClientRemote_UnstuckSuccessful()
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckSuccessful_Title,
                NotificationUnstuckSuccessful_Message);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ServerRemote_CreateUnstuckRequest()
        {
            var character = ServerRemoteContext.Character;
            if (serverRequests.ContainsKey(character))
            {
                this.CallClient(character, _ => _.ClientRemote_UnstuckAlreadyQueued());
                return;
            }

            if (character.GetPublicState<ICharacterPublicState>()
                         .IsDead)
            {
                // character is dead
                this.CallClient(character, _ => _.ClientRemote_UnstuckFailedDead());
                return;
            }

            var vehicle = character.SharedGetCurrentVehicle();
            if (vehicle != null)
            {
                VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);

                if (vehicle.GetPublicState<VehiclePublicState>().PilotCharacter != null)
                {
                    // cannot quit vehicle here, cannot unstuck
                    this.CallClient(character, _ => _.ClientRemote_UnstuckImpossible());
                    return;
                }
            }

            var delay = LandClaimSystem.SharedIsPositionInsideOwnedOrFreeArea(character.TilePosition,
                                                                              character)
                            ? UnstuckDelaySecondsTotal
                            : UnstuckDelaySecondsOnEnemyBaseTotal;

            var unstuckTime = Server.Game.FrameTime + delay;
            PlayerCharacter.GetPublicState(character).UnstuckExecutionTime = unstuckTime;

            serverRequests.Add(character,
                               new CharacterUnstuckRequest(
                                   unstuckAfter: unstuckTime,
                                   initialPosition: character.Position));
        }

        private void ServerTimerTickCallback()
        {
            var time = Server.Game.FrameTime;
            serverRequests.ProcessAndRemoveByPair(
                pair =>
                {
                    // process unstuck
                    var character = pair.Key;
                    var request = pair.Value;

                    if (character.GetPublicState<ICharacterPublicState>()
                                 .IsDead)
                    {
                        // character is dead
                        this.CallClient(character, _ => _.ClientRemote_UnstuckFailedDead());
                        return true; // remove this request
                    }

                    if (request.InitialPosition.DistanceSquaredTo(character.Position)
                        > MaxUnstuckMovementDistance * MaxUnstuckMovementDistance)
                    {
                        // character moved
                        ServerNotifyUnstuckCancelledCharacterMoved(character);
                        return true; // remove this request
                    }

                    if (character.SharedGetCurrentVehicle() != null)
                    {
                        // character entered vehicle
                        ServerNotifyUnstuckCancelledCharacterMoved(character);
                        return true;
                    }

                    if (!SharedValidateCanUnstuck(character))
                    {
                        return true; // remove this request
                    }

                    if (request.UnstuckAfter > time)
                    {
                        // time is not expired yet
                        return false; // don't remove this request
                    }

                    // try to teleport player away but nearby
                    if (!CharacterRespawnSystem.ServerTryPlaceCharacterNearby(character, character.Position))
                    {
                        // try to simply respawn in starting area (only the character position will be changed)
                        ServerPlayerSpawnManager.PlacePlayer(character, isRespawn: false);
                    }

                    Logger.Info(
                        $"Character unstuck from {request.InitialPosition} to {character.TilePosition}",
                        character);
                    this.CallClient(character, _ => _.ClientRemote_UnstuckSuccessful());
                    return true; // remove this request
                },
                removeCallback: pair =>
                                {
                                    var character = pair.Key;
                                    ServerOnUnstuckRequestRemoved(character);
                                });
        }

        private readonly struct CharacterUnstuckRequest
        {
            public readonly Vector2D InitialPosition;

            // server timestamp when unstuck will be processed
            public readonly double UnstuckAfter;

            public CharacterUnstuckRequest(double unstuckAfter, Vector2D initialPosition)
            {
                this.UnstuckAfter = unstuckAfter;
                this.InitialPosition = initialPosition;
            }
        }
    }
}