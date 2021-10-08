namespace AtomicTorch.CBND.CoreMod.Systems.Resources
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class GatheringSystem
        : ProtoActionSystem<
            GatheringSystem,
            WorldActionRequest,
            GatheringActionState,
            PublicActionStateWithTargetObjectSounds>
    {
        public const string NotificationNothingToHarvestYet = "Nothing to harvest yet";

        public delegate void ServerGatherDelegate(ICharacter character, IStaticWorldObject worldObject);

        public static event ServerGatherDelegate ServerOnGather;

        protected override WorldActionRequest ClientTryCreateRequest(ICharacter character)
        {
            var worldObject = ClientComponentObjectInteractionHelper.MouseOverObject;
            if (worldObject?.ProtoWorldObject is IProtoObjectGatherable)
            {
                return new WorldActionRequest(character, worldObject);
            }

            // no proper target
            return null;
        }

        protected override void SharedOnActionCompletedInternal(GatheringActionState state, ICharacter character)
        {
            var worldObject = (IStaticWorldObject)state.TargetWorldObject;
            var protoGatherable = (IProtoObjectGatherable)worldObject.ProtoWorldObject;

            Logger.Info($"Gathering completed: {worldObject} by {character}", character);
            if (IsClient)
            {
                return;
            }

            // Server-side only
            if (protoGatherable.ServerGather(worldObject, character))
            {
                Api.SafeInvoke(() => ServerOnGather?.Invoke(character, worldObject));
            }
        }

        protected override GatheringActionState SharedTryCreateState(WorldActionRequest request)
        {
            var worldObject = request.WorldObject;
            var character = request.Character;

            var staticWorldObject = (IStaticWorldObject)worldObject;
            if (worldObject.ProtoGameObject is not IProtoObjectGatherable protoGatherable)
            {
                throw new Exception("Not a gatherable resource: " + worldObject);
            }

            if (IsServer)
            {
                WorldObjectClaimSystem.ServerTryClaim(staticWorldObject,
                                                      character,
                                                      WorldObjectClaimDuration.RegularObjects);
            }

            var durationSeconds = protoGatherable.DurationGatheringSeconds;
            var multiplier = protoGatherable.GetGatheringSpeedMultiplier(staticWorldObject, character);
            durationSeconds /= multiplier;

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                durationSeconds = 0.1;
            }

            return new GatheringActionState(
                character,
                staticWorldObject,
                durationSeconds);
        }

        protected override void SharedValidateRequest(WorldActionRequest request)
        {
            var worldObject = (IStaticWorldObject)request.WorldObject;
            if (worldObject?.ProtoWorldObject is not IProtoObjectGatherable protoGatherable)
            {
                throw new Exception("The world object must be gatherable");
            }

            if (!worldObject.ProtoWorldObject.SharedCanInteract(request.Character, worldObject, true))
            {
                throw new Exception("Cannot interact with " + worldObject);
            }

            if (!WorldObjectClaimSystem.SharedIsAllowInteraction(request.Character,
                                                                 worldObject,
                                                                 showClientNotification: true))
            {
                throw new Exception("Locked for another player: " + worldObject);
            }

            if (protoGatherable.SharedIsCanGather(worldObject))
            {
                return;
            }

            Logger.Warning("Cannot gather now: " + worldObject, request.Character);
            if (Api.IsClient)
            {
                CannotInteractMessageDisplay.ShowOn(worldObject, NotificationNothingToHarvestYet);
                worldObject.ProtoWorldObject.SharedGetObjectSoundPreset()
                           .PlaySound(ObjectSound.InteractFail);
            }

            throw new Exception("Cannot gather now: " + worldObject);
        }
    }
}