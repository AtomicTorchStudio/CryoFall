namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public readonly struct DropItemContext
    {
        private readonly ICharacter character;

        private readonly IStaticWorldObject staticWorldObject;

        public DropItemContext(
            ICharacter character,
            IStaticWorldObject staticWorldObject,
            IProtoItemWeapon byWeaponProto = null)
        {
            this.ByWeaponProto = byWeaponProto;
            this.character = character;
            this.staticWorldObject = staticWorldObject;
        }

        public IProtoItemWeapon ByWeaponProto { get; }

        public ICharacter Character => this.character
                                       ?? throw new Exception(
                                           "Spawning in non-character context - there is no Character property provided to the context");

        public bool HasCharacter => this.character != null;

        public IStaticWorldObject StaticWorldObject => this.staticWorldObject
                                                       ?? throw new Exception(
                                                           "Spawning in non-static world object context - there is no StaticWorldObject property provided to the context");
    }
}