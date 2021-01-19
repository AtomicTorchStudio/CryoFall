namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelFactionDiplomacyStatusEntry : BaseViewModel
    {
        public ViewModelFactionDiplomacyStatusEntry(
            string clanTag,
            FactionDiplomacyStatus status)
        {
            this.ClanTag = clanTag;
            this.Status = status;
        }

        public string ClanTag { get; }

        public Brush Emblem
            => ClientFactionEmblemCache.GetEmblemTextureBrush(this.ClanTag);

        public string LeaderName => null;

        public FactionDiplomacyStatus Status { get; }
    }
}