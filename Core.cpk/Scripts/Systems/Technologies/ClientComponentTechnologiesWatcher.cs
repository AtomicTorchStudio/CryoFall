namespace AtomicTorch.CBND.CoreMod.Systems.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentTechnologiesWatcher : ClientComponent
    {
        private static ClientComponentTechnologiesWatcher instance;

        private NetworkSyncList<TechGroup> techGroups;

        private NetworkSyncList<TechNode> techNodes;

        private PlayerCharacterTechnologies technologies;

        public static event Action CurrentTechnologiesChanged;

        public static event Action LearningPointsChanged;

        public static event Action TechGroupsChanged;

        public static event Action TechNodesChanged;

        public static PlayerCharacterTechnologies CurrentTechnologies => instance.technologies;

        public void Setup(PlayerCharacterPrivateState privateState)
        {
            TechGroupsChanged = null;
            TechNodesChanged = null;

            this.technologies = privateState.Technologies;
            this.techGroups = this.technologies.Groups;
            this.techNodes = this.technologies.Nodes;

            this.technologies.ClientSubscribe(
                _ => _.LearningPoints,
                _ => LearningPointsChanged?.Invoke(),
                this);

            this.techGroups.ClientAnyModification += this.TechGroupsClientAnyModificationHandler;
            this.techNodes.ClientAnyModification += this.TechNodesClientAnyModificationHandler;

            instance = this;

            CurrentTechnologiesChanged?.Invoke();
            TechGroupsChanged?.Invoke();
            TechNodesChanged?.Invoke();
            LearningPointsChanged?.Invoke();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.ReleaseSubscriptions();

            this.techGroups.ClientAnyModification -= this.TechGroupsClientAnyModificationHandler;
            this.techNodes.ClientAnyModification -= this.TechNodesClientAnyModificationHandler;

            if (instance == this)
            {
                instance = null;
            }
        }

        private void TechGroupsClientAnyModificationHandler(NetworkSyncList<TechGroup> source)
        {
            TechGroupsChanged?.Invoke();
        }

        private void TechNodesClientAnyModificationHandler(NetworkSyncList<TechNode> source)
        {
            TechNodesChanged?.Invoke();
        }
    }
}