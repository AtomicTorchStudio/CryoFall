namespace AtomicTorch.CBND.CoreMod.Systems.PvE
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class PveSystem : ProtoSystem<PveSystem>
    {
        public const double DepositExtractorWithoutDepositActionSpeedMultiplier = 1 / 5.0;

        public const string DuelMode_Button_Disable = "Disable duel mode";

        public const string DuelMode_Button_Enable = "Enable duel mode";

        public const string DuelMode_Description =
            @"You can enable duel mode in order to challenge other players to a fight.
              [br]You do not lose items upon death.
              [br]Both players must have duel mode enabled.";

        public const string DuelMode_Title = "Duel Mode";

        public const string Notification_CannotDamagePlayers_Message =
            "You cannot damage other players or structures belonging to a player on a PvE server.";

        public const string Notification_OtherPlayerDontHaveDuelMode_Message =
            "Both players must enable Duel Mode in order to fight.";

        // Displayed when player attempting to take the dropped loot of another player on a PvE server.
        public const string Notification_StuffBelongsToAnotherPlayer_Message =
            "This belongs to another player.";

        private static readonly bool serverIsPvE;

        private static bool? clientIsPvE;

        private static TaskCompletionSource<bool> clientPvErequestTask
            = new TaskCompletionSource<bool>();

        static PveSystem()
        {
            if (IsServer)
            {
                serverIsPvE = ServerRates.Get("PvE",
                                              defaultValue: 0,
                                              description:
                                              @"PvP / PvE mode switch.
                              Set it to 1 to make this server PvE-only. Otherwise it will default to PvP.
                              Please note: this changes game mechanics dramatically.
                              Do NOT change it after the server world is created as it might lead to unexpected consequences.")
                              > 0;
            }
        }

        public static event Action ClientIsPvEChanged;

        public static bool ClientIsDuelModeEnabled
        {
            get => SharedIsDuelModeEnabled(ClientCurrentCharacterHelper.Character);
            set
            {
                if (ClientIsDuelModeEnabled == value)
                {
                    return;
                }

                Instance.CallServer(_ => _.ServerRemote_SetDuelMode(value));
            }
        }

        // for client this flag is received via bootstrapper call
        public static bool ServerIsPvE
        {
            get
            {
                Api.ValidateIsServer();
                return serverIsPvE;
            }
        }

        [NotLocalizable]
        public override string Name => "PvE system";

        public static Task ClientAwaitPvEModeFromServer()
        {
            return clientPvErequestTask.Task;
        }

        public static bool ClientIsPve(bool logErrorIfDataIsNotYetAvailable)
        {
            if (clientIsPvE.HasValue)
            {
                return clientIsPvE.Value;
            }

            Api.ValidateIsClient();
            if (logErrorIfDataIsNotYetAvailable)
            {
                Logger.Error("Client has not yet received PvP/PvE server mode");
            }

            return false;
        }

        public static void ClientShowDuelModeRequiredNotificationIfNecessary(
            ICharacter characterA,
            ICharacter characterB)
        {
            if (!ClientIsPve(logErrorIfDataIsNotYetAvailable: false)
                || characterA.IsNpc
                || characterB.IsNpc)
            {
                return;
            }

            var isDuelModeA = SharedIsDuelModeEnabled(characterA);
            var isDuelModeB = SharedIsDuelModeEnabled(characterB);
            if (isDuelModeA && isDuelModeB)
            {
                // duel mode is allowed
                return;
            }

            if (!isDuelModeA
                && !isDuelModeB)
            {
                ClientShowNotificationCannotDamagePlayersAndStructures();
                return;
            }

            NotificationSystem.ClientShowNotification(
                DuelMode_Title,
                Notification_OtherPlayerDontHaveDuelMode_Message,
                NotificationColor.Bad);
        }

        public static bool SharedAllowPvPDamage(ICharacter characterA, ICharacter characterB)
        {
            if (!SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false)
                || characterA.IsNpc
                || characterB.IsNpc)
            {
                return true;
            }

            return SharedIsDuelModeEnabled(characterA)
                   && SharedIsDuelModeEnabled(characterB);
        }

        public static bool SharedIsAllowStructureDamage(
            IStaticWorldObject targetObject,
            bool showClientNotification)
        {
            if (!SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
            {
                return true;
            }

            if (!(targetObject.ProtoWorldObject is IProtoObjectStructure))
            {
                return true;
            }

            if (!LandClaimSystem.SharedIsObjectInsideAnyArea(targetObject))
            {
                // always allow damage to structures outside of land claim areas
                return true;
            }

            // don't allow damage as the structure is inside the land claim area
            if (IsClient && showClientNotification)
            {
                if (LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(targetObject,
                                                                        ClientCurrentCharacterHelper.Character))
                {
                    // that's the owned land claim area - no need to display a notification
                }
                else
                {
                    ClientShowNotificationCannotDamagePlayersAndStructures();
                }
            }

            return false;
        }

        public static bool SharedIsDuelModeEnabled(ICharacter character)
        {
            if (character.IsNpc)
            {
                return true;
            }

            return PlayerCharacter.GetPublicState(character)
                                  .IsPveDuelModeEnabled;
        }

        public static bool SharedIsPve(bool clientLogErrorIfDataIsNotYetAvailable)
        {
            if (IsServer)
            {
                return ServerIsPvE;
            }

            return ClientIsPve(clientLogErrorIfDataIsNotYetAvailable);
        }

        public static bool SharedValidateInteractionIsNotForbidden(
            ICharacter character,
            IStaticWorldObject worldObject,
            bool writeToLog)
        {
            if (!SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: true)
                || LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(worldObject, character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            if (IsClient && writeToLog)
            {
                ClientShowNotificationActionForbidden();
            }

            return false;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected override void PrepareSystem()
        {
            base.PrepareSystem();

            if (IsClient)
            {
                return;
            }

            var isPvE = ServerIsPvE;
            Server.Core.AddServerInfoTag(isPvE ? "PvE" : "PvP");

            if (isPvE)
            {
                ServerCharacterDeathMechanic.CharacterDeath += this.ServerCharacterDeathHandler;
                Server.Characters.PlayerOnlineStateChanged += this.ServerPlayerOnlineStateChangedHandler;
            }
        }

        private static void ClientShowNotificationActionForbidden()
        {
            NotificationSystem.ClientShowNotification(
                CoreStrings.Notification_ActionForbidden,
                Notification_StuffBelongsToAnotherPlayer_Message,
                color: NotificationColor.Bad);
        }

        private static void ClientShowNotificationCannotDamagePlayersAndStructures()
        {
            NotificationSystem.ClientShowNotification(
                CoreStrings.Notification_ActionForbidden,
                Notification_CannotDamagePlayers_Message,
                color: NotificationColor.Bad);
        }

        private static void ServerSetDuelMode(ICharacter character, bool isEnabled)
        {
            if (!ServerIsPvE && isEnabled)
            {
                isEnabled = false;
                Logger.Info("Cannot switch PvE duel mode to enabled on PvP servers",
                            character);
            }

            var publicState = PlayerCharacter.GetPublicState(character);
            if (publicState.IsPveDuelModeEnabled == isEnabled)
            {
                return;
            }

            publicState.IsPveDuelModeEnabled = isEnabled;
            Logger.Important("Switched PvE duel mode to " + (isEnabled ? "enabled" : "disabled"), character);
        }

        private void ClientRemote_IsPvE(bool isPvE)
        {
            try
            {
                if (clientIsPvE == isPvE)
                {
                    return;
                }

                clientIsPvE = isPvE;
                if (ClientIsPvEChanged != null)
                {
                    Api.SafeInvoke(ClientIsPvEChanged);
                }
            }
            finally
            {
                clientPvErequestTask.SetResult(isPvE);
            }
        }

        private void ServerCharacterDeathHandler(ICharacter character)
        {
            if (character.IsNpc
                || !ServerIsPvE)
            {
                return;
            }

            // Mutation status effect is added upon death if EITHER one of the two conditions are met:
            // player had 15% or more radiation poisoning when they died
            // OR they had 15% or more damage received from radiation when they died.
            if (ServerIsDeathFromRadiation())
            {
                Logger.Info("Character died from radiation, adding mutation", character);
                character.ServerAddStatusEffect<StatusEffectMutation>(intensity: 1.0);
            }

            bool ServerIsDeathFromRadiation()
            {
                if (character.SharedGetStatusEffectIntensity<StatusEffectRadiationPoisoning>()
                    > 0.15)
                {
                    return true;
                }

                var damageSources = CharacterDamageTrackingSystem.ServerGetDamageSources(character);
                if (damageSources == null)
                {
                    return false;
                }

                foreach (var damageSource in damageSources)
                {
                    if (damageSource.ProtoEntity is StatusEffectRadiationPoisoning
                        && damageSource.Fraction > 0.15)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            if (!isOnline)
            {
                ServerSetDuelMode(playerCharacter, isEnabled: false);
            }
        }

        private void ServerRemote_RequestIsPvE()
        {
            this.CallClient(ServerRemoteContext.Character,
                            _ => _.ClientRemote_IsPvE(ServerIsPvE));
        }

        private void ServerRemote_SetDuelMode(bool isEnabled)
        {
            ServerSetDuelMode(ServerRemoteContext.Character, isEnabled);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                void Refresh()
                {
                    clientPvErequestTask?.TrySetCanceled();

                    clientPvErequestTask = new TaskCompletionSource<bool>();
                    if (Api.Client.Characters.CurrentPlayerCharacter != null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestIsPvE());
                    }
                }
            }
        }
    }
}