namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class ProtoObjectDecorationWithSoundSource : ProtoObjectDecoration
    {
        private const int UseIntervalSeconds = 3;

        private static double clientLastInteractTime;

        private readonly Lazy<ReadOnlySoundResourceSet> cachedSoundSet;

        protected ProtoObjectDecorationWithSoundSource()
        {
            this.cachedSoundSet = new Lazy<ReadOnlySoundResourceSet>(
                () => this.SoundSet);
        }

        protected abstract ReadOnlySoundResourceSet SoundSet { get; }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!this.IsInteractableObject)
            {
                return false;
            }

            // Please note: no PvE access checks here. So base guests can use a door bell and similar structures.
            return this.SharedIsInsideCharacterInteractionArea(character,
                                                               worldObject,
                                                               writeToLog);
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            var time = Client.Core.ClientRealTime;
            if (time - clientLastInteractTime < UseIntervalSeconds)
            {
                return;
            }

            clientLastInteractTime = time;

            var randomSound = this.ClientGetRandomSound(out var soundIndex);
            Client.Audio.PlayOneShot(randomSound);

            this.CallServer(_ => _.ServerRemote_ObjectUse(data.GameObject, soundIndex));
        }

        private SoundResource ClientGetRandomSound(out int index)
        {
            var soundSet = this.cachedSoundSet.Value;
            var soundResource = soundSet.GetSound();
            index = soundSet.IndexOf(soundResource);
            return soundResource;
        }

        private void ClientRemote_OnObjectUseByOtherPlayer(IStaticWorldObject worldObject, int index)
        {
            var soundResource = this.cachedSoundSet.Value.GetSoundAtIndex(index);
            if (soundResource is not null)
            {
                Client.Audio.PlayOneShot(soundResource, worldObject);
            }
        }

        [RemoteCallSettings(timeInterval: UseIntervalSeconds, clientMaxSendQueueSize: 1)]
        private void ServerRemote_ObjectUse(IStaticWorldObject worldObject, int soundIndex)
        {
            var character = ServerRemoteContext.Character;
            if (!worldObject.ProtoStaticWorldObject.SharedCanInteract(character,
                                                                      worldObject,
                                                                      writeToLog: true))
            {
                return;
            }

            using var observers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(worldObject, observers);
            observers.Remove(character);

            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_OnObjectUseByOtherPlayer(worldObject, soundIndex));
        }
    }
}