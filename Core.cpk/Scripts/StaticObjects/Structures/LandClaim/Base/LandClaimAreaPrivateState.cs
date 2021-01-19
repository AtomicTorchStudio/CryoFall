namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using JetBrains.Annotations;

    public class LandClaimAreaPrivateState : BasePrivateState
    {
        [SyncToClient]
        public NetworkSyncList<string> DirectLandOwners { get; set; }

        /// <summary>
        /// Determines whether the land claim was damaged to 0 HP by players.
        /// Reset when the land claim is repaired at least a bit or when the destroy is started due to the decay.
        /// </summary>
        public bool IsDestroyedByPlayers { get; set; }

        [SyncToClient]
        [CanBeNull]
        public string LandClaimFounder { get; set; }

        public IStaticWorldObject ServerLandClaimWorldObject { get; set; }

        public IEnumerable<string> ServerGetLandOwners()
        {
            var area = (ILogicObject)this.GameObject;
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            if (areasGroup is null)
            {
                // perhaps a new area
                return this.DirectLandOwners;
            }

            var faction = LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction;
            return faction is null
                       ? this.DirectLandOwners
                       : FactionSystem.ServerGetFactionMemberNames(faction);
        }
    }
}