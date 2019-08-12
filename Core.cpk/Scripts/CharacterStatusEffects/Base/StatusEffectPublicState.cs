namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class StatusEffectPublicState : BasePublicState
    {
        [SyncToClient(isSendChanges: false)]
        public ICharacter Character { get; private set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 10,
            networkDataType: typeof(float))]
        public double Intensity { get; private set; }

        [TempOnly]
        public bool ServerIsAddedToCharacterPublicState { get; set; }

        public void SetIntensity(double newIntensity)
        {
            if (newIntensity <= 0)
            {
                // remove status effect
                Api.Logger.Important($"Status effect intensity dropped <= 0: {this.GameObject} at {this.Character}");
                var protoStatusEffect = (IProtoStatusEffect)this.GameObject.ProtoGameObject;
                this.Character.ServerRemoveStatusEffect(protoStatusEffect);
                return;
            }

            if (newIntensity >= 1)
            {
                newIntensity = 1;
            }

            this.Intensity = newIntensity;
        }

        public void Setup(ICharacter character)
        {
            if (this.Character != null)
            {
                throw new Exception("Cannot change status effect owner - it's already set");
            }

            this.Character = character;
        }
    }
}