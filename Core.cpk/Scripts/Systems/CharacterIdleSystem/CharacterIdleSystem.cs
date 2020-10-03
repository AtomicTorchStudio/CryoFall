namespace AtomicTorch.CBND.CoreMod.Systems.CharacterIdleSystem
{
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// This is AFK system which is activated when player is inactive for a while.
    /// This system is used to determine whether thirst, hunger and implants decay systems should apply.
    /// </summary>
    public class CharacterIdleSystem : ProtoSystem<CharacterIdleSystem>
    {
        public const string Notification_PlayerIdle_Message_Part1
            = @"It looks like you have been inactive for a while.
  [br]Thirst and hunger are suspended.";

        public const string Notification_PlayerIdle_Message_Part2
            = "Chatting, crafting, or using consumables is permitted.";

        public const string Notification_PlayerIdle_Message_Part3
            = "Any other actions will deactivate AFK mode.";

        public const string Notification_PlayerIdle_Title
            = "AFK Mode";

        // If a player is idle beyond this threshold the AFK mode will be activated.
        private const double ThresholdIdleSeconds = 3 * 60; // 3 minutes

        private const double TimeIntervalSeconds = 1.0;

        private HudNotificationControl clientNotificationIsIdle;

        public static bool ClientIsCurrentPlayerIdle
        {
            get
            {
                var playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
                if (playerCharacter is null
                    || playerCharacter.IsDestroyed)
                {
                    return false;
                }

                var privateState = PlayerCharacter.GetPrivateState(playerCharacter);
                return privateState.IsIdle;
            }
        }

        public override string Name => "Character idle system";

        public static bool ServerIsIdlePlayer(ICharacter character)
            => PlayerCharacter.GetPrivateState(character).IsIdle;

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                BootstrapperClientGame.InitCallback += this.ClientInitCallback;
                return;
            }

            // configure time interval trigger
            TriggerEveryFrame.ServerRegister(
                callback: ServerGlobalUpdate,
                name: "System." + this.ShortId);
        }

        private static void ServerGlobalUpdate()
        {
            // update last active time
            const double spreadDeltaTime = TimeIntervalSeconds;
            var serverTime = Server.Game.FrameTime;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (character.ProtoCharacter.GetType() != typeof(PlayerCharacter))
                {
                    // only characters of specific type (PlayerCharacter) are processed
                    continue;
                }

                var publicState = PlayerCharacter.GetPublicState(character);
                var privateState = PlayerCharacter.GetPrivateState(character);

                if (publicState.IsDead)
                {
                    // dead characters are not processed
                    privateState.IsIdle = false;
                    continue;
                }

                var isIdlePlayer = true;
                if (character.ServerIsOnline)
                {
                    var moveMode = publicState.AppliedInput.MoveModes;
                    isIdlePlayer = (moveMode == CharacterMoveModes.None
                                    || moveMode == CharacterMoveModes.ModifierRun)
                                   && privateState.CurrentActionState is null
                                   && !privateState.WeaponState.IsFiring;
                    // please note that crafting is not considered as an activity to prevent AFK crafting players from thirst/hunger
                }

                if (isIdlePlayer)
                {
                    // assume player idle only if the threshold has been reached
                    privateState.IsIdle = serverTime - privateState.ServerLastActiveTime
                                          >= ThresholdIdleSeconds;
                }
                else
                {
                    privateState.ServerLastActiveTime = serverTime;
                    privateState.IsIdle = false;
                }
            }
        }

        private void ClientCurrentCharacterIdleStateChangedHandler()
        {
            if (!ClientIsCurrentPlayerIdle)
            {
                this.clientNotificationIsIdle?.Hide(quick: true);
                this.clientNotificationIsIdle = null;
                return;
            }

            // idle player
            if (this.clientNotificationIsIdle is not null
                && !this.clientNotificationIsIdle.IsHiding)
            {
                return;
            }

            this.clientNotificationIsIdle = NotificationSystem.ClientShowNotification(
                title: Notification_PlayerIdle_Title,
                message: Notification_PlayerIdle_Message_Part1
                         + "[br]"
                         + Notification_PlayerIdle_Message_Part2
                         + "[br]"
                         + Notification_PlayerIdle_Message_Part3,
                color: NotificationColor.Good,
                autoHide: false,
                playSound: false);
        }

        private void ClientInitCallback(ICharacter playerCharacter)
        {
            this.clientNotificationIsIdle?.Hide(quick: true);

            var privateState = PlayerCharacter.GetPrivateState(playerCharacter);
            privateState.ClientSubscribe(_ => _.IsIdle,
                                         _ => this.ClientCurrentCharacterIdleStateChangedHandler(),
                                         PlayerCharacter.GetClientState(playerCharacter));

            // do not display the notification when player is already idle when logging it as it will be confusing
            //this.ClientCurrentCharacterIdleStateChangedHandler();
        }
    }
}