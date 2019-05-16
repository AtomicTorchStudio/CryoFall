namespace AtomicTorch.CBND.CoreMod.Systems.CharacterUnstuck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Triggers;
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

        public const double UnstuckDelaySecondsTotal = 5 * 60; // 5 minutes

        private const double MaxUnstuckMovementDistance = 1.5;

        private static readonly Dictionary<ICharacter, CharacterUnstuckRequest> serverRequests
            = IsServer
                  ? new Dictionary<ICharacter, CharacterUnstuckRequest>()
                  : null;

        public override string Name => "Character unstuck system";

        public static void ClientCreateUnstuckRequest()
        {
            if (!SharedValidateCanUnstuck(ClientCurrentCharacterHelper.Character))
            {
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_CreateUnstuckRequest());
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will update food and water values
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(1),
                callback: this.ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private static bool SharedValidateCanUnstuck(ICharacter character)
        {
            using (var tempAreas = Api.Shared.GetTempList<ILogicObject>())
            {
                var bounds = new RectangleInt(
                    character.TilePosition - new Vector2Int(1, 1),
                    new Vector2Int(2, 2));

                LandClaimSystem.SharedGetAreasInBounds(bounds, tempAreas, addGracePadding: false);
                if (tempAreas.Any(LandClaimSystem.SharedIsAreaUnderRaid))
                {
                    Logger.Important("Cannot unstuck when located in an area under raid");
                    LandClaimSystem.SharedSendNotificationActionRestrictedUnderRaidblock(character);
                    return false;
                }

                return true;
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_UnstuckAlreadyQueued()
        {
            NotificationSystem.ClientShowNotification(
                NotificationAlreadyRequestedUnstuck_Title,
                NotificationAlreadyRequestedUnstuck_Message);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_UnstuckFailedCharacterMoved()
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckCancelled_Title,
                NotificationUnstuckCancelled_Message,
                NotificationColor.Bad);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_UnstuckFailedDead()
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckImpossible_Title,
                NotificationUnstuckImpossible_Dead,
                NotificationColor.Bad);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_UnstuckQueued(double unstuckDelaySecondsTotal)
        {
            NotificationSystem.ClientShowNotification(
                NotificationUnstuckRequested_Title,
                string.Format(NotificationUnstuckRequested_MessageFormat,
                              ClientTimeFormatHelper.FormatTimeDuration(unstuckDelaySecondsTotal)));
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
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

            serverRequests.Add(character,
                               new CharacterUnstuckRequest(
                                   unstuckAfter: Server.Game.FrameTime
                                                 + UnstuckDelaySecondsTotal,
                                   initialPosition: character.Position));

            this.CallClient(character, _ => _.ClientRemote_UnstuckQueued(UnstuckDelaySecondsTotal));
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
                        this.CallClient(character, _ => _.ClientRemote_UnstuckFailedCharacterMoved());
                        return true; // remove this request
                    }

                    if (request.UnstuckAfter > time)
                    {
                        // time is not expired yet
                        return false; // don't remove this request
                    }

                    if (!SharedValidateCanUnstuck(character))
                    {
                        return true; // remove this request
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
                removeCallback: pair => { });
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