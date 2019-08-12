namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Beds
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class ProtoObjectBed
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectBed
        where TPrivateState : ObjectWithOwnerPrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string DialogClaimBed_Message
            = @"Do you want to make it your current bed?
  [br]This will set it as your active respawn point.";

        public const string DialogClaimBed_Title
            = "This bed doesn't belong to anybody";

        public const string ErrorClaimBed_AlreadyHasOwner
            = "The bed already has an owner.";

        public const string ErrorClaimBed_AnotherPlayersLandClaim
            = "The bed is located in another player's land claim area.";

        public const string MessageBedBelongsToCurrentPlayer
            = "This bed belongs to you";

        public const string MessageFormatBedBelongsToAnotherPlayer
            = "This bed belongs to[br]{0}";

        public const string NotificationBedClaimedSuccessfully
            = "This bed is now set as your active respawn point.";

        public abstract double RespawnCooldownDurationSeconds { get; }

        public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            if (!(byCharacter.ProtoCharacter is PlayerCharacter))
            {
                // built without player
                return;
            }

            ServerSetCurrentBed(structure, byCharacter);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var bedPrivateState = GetPrivateState(gameObject);
            var ownerCharacter = bedPrivateState.Owner;
            if (ownerCharacter == null)
            {
                // the bed doesn't have the owner
                return;
            }

            var playerPrivateState = PlayerCharacter.GetPrivateState(ownerCharacter);
            if (playerPrivateState.CurrentBedObject != gameObject)
            {
                // player has a new bed
                return;
            }

            playerPrivateState.CurrentBedObject = null;
            Logger.Important($"Player's bed for {ownerCharacter} destroyed - {gameObject}");
        }

        protected override async void ClientInteractStart(ClientObjectData data)
        {
            var bedObject = data.GameObject;
            var (isSuccess, name) = await this.CallServer(_ => _.ServerRemote_GetBedOwnerName(bedObject));
            if (!isSuccess)
            {
                ClientOnCannotInteract(bedObject, CoreStrings.Notification_TooFar, isOutOfRange: true);
                return;
            }

            var isFree = string.IsNullOrEmpty(name);
            if (isFree)
            {
                DialogWindow.ShowDialog(
                    DialogClaimBed_Title,
                    DialogClaimBed_Message,
                    okText: CoreStrings.Yes,
                    okAction: () => this.ClientMakeCurrentBed(bedObject),
                    hideCancelButton: false);
                return;
            }

            var message = name == Client.Characters.CurrentPlayerCharacter.Name
                              ? MessageBedBelongsToCurrentPlayer
                              : string.Format(MessageFormatBedBelongsToAnotherPlayer, name);

            CannotInteractMessageDisplay.ShowOn(bedObject, message);
            this.SoundPresetObject.PlaySound(ObjectSound.InteractSuccess);
        }

        private static void ServerSetCurrentBed(IStaticWorldObject bedObject, ICharacter owner)
        {
            var playerPrivateState = PlayerCharacter.GetPrivateState(owner);
            var previousBed = playerPrivateState.CurrentBedObject;
            if (previousBed != null)
            {
                // remove from the previous bed the current character
                GetPrivateState(previousBed).Owner = null;
            }

            playerPrivateState.CurrentBedObject = bedObject;
            var bedPrivateState = GetPrivateState(bedObject);
            bedPrivateState.Owner = owner;
            Logger.Important($"Last built bed for {owner} now is {bedObject}");
        }

        private async void ClientMakeCurrentBed(IStaticWorldObject bedObject)
        {
            var errorMessage = await this.CallServer(_ => _.ServerRemote_MakeCurrentBed(bedObject));
            if (errorMessage == null)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationBedClaimedSuccessfully,
                    color: NotificationColor.Good,
                    icon: this.Icon);
                return;
            }

            NotificationSystem.ClientShowNotification(
                CoreStrings.TitleActionFailed,
                errorMessage,
                NotificationColor.Bad,
                this.Icon);
        }

        private (bool isSuccess, string ownerName) ServerRemote_GetBedOwnerName(IStaticWorldObject bedObject)
        {
            var character = ServerRemoteContext.Character;
            if (!this.SharedIsInsideCharacterInteractionArea(character, bedObject, writeToLog: true))
            {
                return (isSuccess: false, null);
            }

            var owner = GetPrivateState(bedObject).Owner;
            return (isSuccess: true, owner?.Name);
        }

        private string ServerRemote_MakeCurrentBed(IStaticWorldObject bedObject)
        {
            var character = ServerRemoteContext.Character;
            if (!this.SharedIsInsideCharacterInteractionArea(character, bedObject, writeToLog: true))
            {
                return CoreStrings.Notification_TooFar;
            }

            var privateState = GetPrivateState(bedObject);
            if (privateState.Owner == null)
            {
                // can set new owner
                if (!LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(bedObject, character))
                {
                    return ErrorClaimBed_AnotherPlayersLandClaim;
                }

                ServerSetCurrentBed(bedObject, character);
                return null;
            }

            if (privateState.Owner == character)
            {
                // the owner is the same
                return null;
            }

            return ErrorClaimBed_AlreadyHasOwner;
        }
    }

    public abstract class ProtoObjectBed
        : ProtoObjectBed<ObjectWithOwnerPrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}