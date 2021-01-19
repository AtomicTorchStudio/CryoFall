namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;

    [Serializable]
    public readonly struct FactionMemberEntry
    {
        public readonly string Name;

        public readonly FactionMemberRole Role;

        public FactionMemberEntry(string name, FactionMemberRole role)
        {
            this.Name = name;
            this.Role = role;
        }
    }
}