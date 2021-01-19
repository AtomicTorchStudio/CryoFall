namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum FactionDiplomacyStatus : byte
    {
        [Description(CoreStrings.Faction_Diplomacy_Neutral)]
        Neutral = 0,

        [Description(CoreStrings.Faction_Diplomacy_Enemies_Mutual)]
        EnemyMutual = 10,

        [Description(CoreStrings.Faction_Diplomacy_Enemies_DeclaredByCurrentFaction)]
        EnemyDeclaredByCurrentFaction = 11,

        [Description(CoreStrings.Faction_Diplomacy_Enemies_DeclaredByOtherFaction)]
        EnemyDeclaredByOtherFaction = 12,

        /// <summary>
        /// Ally status is only mutual (alliance request is submitted separately).
        /// </summary>
        [Description(CoreStrings.Faction_Diplomacy_Ally)]
        Ally = 20
    }
}