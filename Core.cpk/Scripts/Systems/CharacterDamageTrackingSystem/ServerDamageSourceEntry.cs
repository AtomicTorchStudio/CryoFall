namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [Serializable]
    public readonly struct ServerDamageSourceEntry : IEquatable<ServerDamageSourceEntry>, IRemoteCallParameter
    {
        public readonly string ClanTag;

        public readonly string Name;

        public readonly IProtoGameObject ProtoEntity;

        public ServerDamageSourceEntry(IProtoGameObject byProtoEntity)
        {
            this.ProtoEntity = byProtoEntity;
            this.Name = null;
            this.ClanTag = null;
        }

        public ServerDamageSourceEntry(IGameObjectWithProto byGameObject)
        {
            this.ProtoEntity = byGameObject.ProtoGameObject;

            if (byGameObject is ICharacter byPlayerCharacter
                && !byPlayerCharacter.IsNpc)
            {
                this.Name = byPlayerCharacter.Name;
                this.ClanTag = PlayerCharacter.GetPublicState(byPlayerCharacter).ClanTag;
            }
            else
            {
                this.Name = null;
                this.ClanTag = null;
            }
        }

        public bool Equals(ServerDamageSourceEntry other)
        {
            return Equals(this.ProtoEntity, other.ProtoEntity)
                   && this.ClanTag == other.ClanTag
                   && this.Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is ServerDamageSourceEntry other
                   && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ProtoEntity, this.Name, this.ClanTag);
        }
    }
}