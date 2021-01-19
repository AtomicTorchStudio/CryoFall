namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelFactionDiplomacyPublicInfoControl : BaseViewModel
    {
        public bool IsAlliancesEnabled => FactionConstants.SharedPvpAlliancesEnabled;

        public List<ViewModelFactionDiplomacyStatusEntry> ListAllies { get; private set; }

        public List<ViewModelFactionDiplomacyStatusEntry> ListWars { get; private set; }

        public void SetData(
            IReadOnlyList<FactionSystem.FactionDiplomacyPublicStatusEntry> relationsData)
        {
            this.ListAllies = relationsData.Where(e => e.IsAlly)
                                           .Select(e => new ViewModelFactionDiplomacyStatusEntry(
                                                       e.ClanTag,
                                                       FactionDiplomacyStatus.Ally))
                                           .OrderBy(e => e.ClanTag)
                                           .ToList();

            this.ListWars = relationsData.Where(e => !e.IsAlly)
                                         .Select(e => new ViewModelFactionDiplomacyStatusEntry(
                                                     e.ClanTag,
                                                     // even though it may be not a mutual war declaration, it's fine 
                                                     FactionDiplomacyStatus.EnemyMutual))
                                         .OrderBy(e => e.ClanTag)
                                         .ToList();
        }
    }
}