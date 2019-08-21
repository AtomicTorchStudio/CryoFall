namespace AtomicTorch.CBND.CoreMod.Systems.PvE
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
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
            if (IsClient)
            {
                return;
            }

            serverIsPvE = ServerRates.Get("PvE",
                                          defaultValue: 0,
                                          description:
                                          @"PvP / PvE mode switch.
                              Set it to 1 to make this server PvE-only. Otherwise it will default to PvP.
                              Please note: this changes game mechanics dramatically.
                              Do NOT change it after the server world is created as it might lead to unexpected consequences.")
                          > 0;
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
            ICharacter character,
            IStaticWorldObject targetObject,
            bool showClientNotification)
        {
            if (!SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
            {
                return true;
            }

            var protoWorldObject = targetObject.ProtoWorldObject;
            var isStructure = protoWorldObject is IProtoObjectStructure;
            var isVegetation = protoWorldObject is IProtoObjectVegetation;
            if (!isStructure
                && !isVegetation)
            {
                // can damage such objects everywhere
                return true;
            }

            if (!LandClaimSystem.SharedIsObjectInsideAnyArea(targetObject))
            {
                // always allow damage outside of land claim areas
                return true;
            }

            if (character == null)
            {
                // non-player damage is always forbidden on the owned objects
                return false;
            }

            if (LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(targetObject, character))
            {
                if (isVegetation)
                {
                    // allow damaging vegetation in the owned land claim area
                    return true;
                }

                // don't allow damaging the structures in the owned land claim area
                return false;
            }

            // don't allow damage as the object is inside the not-owned land claim area
            if (IsClient && showClientNotification)
            {
                ClientShowNotificationCannotDamagePlayersAndStructures();
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
                Server.Characters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
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

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            if (!isOnline)
            {
                ServerSetDuelMode(playerCharacter, isEnabled: false);
            }
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