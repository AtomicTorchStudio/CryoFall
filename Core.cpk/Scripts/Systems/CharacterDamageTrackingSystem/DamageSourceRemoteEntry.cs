namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [NotPersistent]
    public readonly struct DamageSourceRemoteEntry : IRemoteCallParameter
    {
        /// <summary>
        /// Clan tag is available when the damage source is a player character.
        /// </summary>
        public readonly string ClanTag;

        public readonly float Fraction;

        public readonly string Name;

        public readonly IProtoEntity ProtoEntity;

        public DamageSourceRemoteEntry(IProtoEntity protoEntity, string name, string clanTag, float fraction)
        {
            this.ProtoEntity = protoEntity;
            this.Name = name;
            this.ClanTag = clanTag;
            this.Fraction = fraction;
        }
    }
}