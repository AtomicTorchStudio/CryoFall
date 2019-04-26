namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem
{
    using System;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    [NotPersistent]
    public readonly struct ServerDamageSourceEntry : IEquatable<ServerDamageSourceEntry>
    {
        public readonly string Name;

        public readonly IProtoGameObject ProtoEntity;

        public ServerDamageSourceEntry(IProtoGameObject byProtoEntity)
        {
            this.ProtoEntity = byProtoEntity;
            this.Name = null;
        }

        public ServerDamageSourceEntry(IGameObjectWithProto byGameObject)
        {
            this.ProtoEntity = byGameObject.ProtoGameObject;

            if (byGameObject is ICharacter character
                && !character.IsNpc)
            {
                this.Name = byGameObject.Name;
            }
            else
            {
                this.Name = null;
            }
        }

        public bool Equals(ServerDamageSourceEntry other)
        {
            return Equals(this.ProtoEntity, other.ProtoEntity)
                   && string.Equals(this.Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ServerDamageSourceEntry other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.ProtoEntity != null ? this.ProtoEntity.GetHashCode() : 0) * 397)
                       ^ (this.Name != null ? this.Name.GetHashCode() : 0);
            }
        }
    }
}