namespace AtomicTorch.CBND.CoreMod.Systems.Technologies
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class PlayerCharacterTechnologies : BaseNetObject
    {
        public delegate void CharacterNodeAddedOrRemovedDelegate(
            ICharacter character,
            TechNode techNode,
            bool isAdded);

        public delegate void CharacterTechGroupAddedOrRemovedDelegate(
            ICharacter character,
            TechGroup techGroup,
            bool isAdded);

        public delegate void DelegateCharacterGainedLearningPoints(
            ICharacter character,
            int gainedLearningPoints,
            bool isModifiedByStat);

        public static event DelegateCharacterGainedLearningPoints ServerCharacterGainedLearningPoints;

        public static event CharacterTechGroupAddedOrRemovedDelegate ServerCharacterGroupAddedOrRemoved;

        public static event CharacterNodeAddedOrRemovedDelegate ServerCharacterTechNodeAddedOrRemoved;

        public ICharacter Character => (ICharacter)this.GameObject;

        [SyncToClient]
        public NetworkSyncList<TechGroup> Groups { get; }
            = new NetworkSyncList<TechGroup>();

        [SyncToClient]
        public bool IsTechTreeChanged { get; set; }

        [SyncToClient]
        public ushort LearningPoints { get; private set; }

        /// <summary>
        /// Accumulator for the remainder (less than 1.0) amount of learning points.
        /// Not [SyncToClient] as it would waste the bandwidth.
        /// </summary>
        public double LearningPointsRemainderAccumulator { get; private set; }

        /// <summary>
        /// This is a list of the researched/unlocked tech nodes.
        /// Do not modify as all modifications should go through PlayerCharacterTechnologies.
        /// </summary>
        [SyncToClient]
        public NetworkSyncList<TechNode> Nodes { get; } = new NetworkSyncList<TechNode>();

        public void ServerAddGroup(TechGroup techGroup)
        {
            if (!techGroup.IsAvailable)
            {
                return;
            }

            if (this.Groups.Contains(techGroup))
            {
                return;
            }

            this.Groups.Add(techGroup);
            var character = this.Character;
            Api.Logger.Info("Tech group added: " + techGroup.ShortId, character);
            Api.SafeInvoke(() => ServerCharacterGroupAddedOrRemoved?.Invoke(character, techGroup, isAdded: true));
        }

        public void ServerAddLearningPoints(double points, bool allowModifyingByStat = true)
        {
            if (points <= 0)
            {
                return;
            }

            if (allowModifyingByStat)
            {
                points *= this.Character.SharedGetFinalStatMultiplier(StatName.LearningsPointsGainMultiplier);
            }

            var pointsToAdd = (int)points;
            var remainder = this.LearningPointsRemainderAccumulator + (points - pointsToAdd);

            if (remainder >= 1.0)
            {
                pointsToAdd += (int)remainder;
                remainder -= (int)remainder;
            }

            this.LearningPointsRemainderAccumulator = remainder;

            if (pointsToAdd < 1)
            {
                // accumulated not enough whole points to add
                return;
            }

            // add points
            var result = this.LearningPoints + pointsToAdd;
            if (result > ushort.MaxValue)
            {
                result = ushort.MaxValue;
            }

            var learningPointsWas = this.LearningPoints;
            this.LearningPoints = (ushort)result;

            pointsToAdd = this.LearningPoints - learningPointsWas;
            if (pointsToAdd <= 0)
            {
                // max LP reached
                return;
            }

            Api.Logger.Info($"Learning points gained: +{pointsToAdd} (total: {result})", this.Character);

            Api.SafeInvoke(
                () => ServerCharacterGainedLearningPoints?.Invoke(this.Character, pointsToAdd, allowModifyingByStat));

            this.Character.ServerAddSkillExperience<SkillLearning>(
                pointsToAdd * SkillLearning.ExperienceAddedPerLPEarned);
        }

        public void ServerAddNode(TechNode techNode)
        {
            if (!this.Groups.Contains(techNode.Group))
            {
                throw new Exception("Cannot add node - the group is locked");
            }

            this.ServerAddNodeNoCheckGroup(techNode);
        }

        public void ServerInit()
        {
            TechnologiesSystem.ServerInitCharacterTechnologies(this);
        }

        public void ServerRefundLearningPoints(int lpToRefund)
        {
            var pointsToAdd = Math.Max(0, lpToRefund);
            this.LearningPoints = (ushort)Math.Min(ushort.MaxValue,
                                                   this.LearningPoints + pointsToAdd);
            Api.Logger.Important(
                $"Learning points refunded: +{pointsToAdd} (total: {this.LearningPoints}) for {this.Character}");
        }

        public void ServerRemoveAllTechnologies()
        {
            if (this.Groups.Count > 0)
            {
                foreach (var techGroup in this.Groups.ToList())
                {
                    this.ServerRemoveGroup(techGroup);
                }

                this.Groups.Clear();
            }

            if (this.Nodes.Count > 0)
            {
                foreach (var techNode in this.Nodes.ToList())
                {
                    this.ServerRemoveNode(techNode);
                }

                this.Nodes.Clear();
            }

            Api.Logger.Info("Technologies reset", this.Character);
            this.ServerInit();

            this.Character.SharedSetFinalStatsCacheDirty();
        }

        /// <summary>
        /// Remove group and all nodes of this group.
        /// </summary>
        public void ServerRemoveGroup(TechGroup techGroup)
        {
            if (!this.Groups.Remove(techGroup))
            {
                return;
            }

            Api.SafeInvoke(() => ServerCharacterGroupAddedOrRemoved?.Invoke(this.Character, techGroup, isAdded: false));

            // remove nodes of this group
            for (var index = 0; index < this.Nodes.Count; index++)
            {
                var node = this.Nodes[index];
                if (node.Group != techGroup)
                {
                    continue;
                }

                this.ServerRemoveNode(node);
                // restart removal
                index = 0;
            }
        }

        public void ServerRemoveLearningPoints(ushort points)
        {
            var result = this.LearningPoints - points;
            if (result < 0)
            {
                throw new Exception(
                    $"Not enough learning points: has {this.LearningPoints} need to remove {points}");
            }

            this.LearningPoints = (ushort)result;
            Api.Logger.Info($"Learning points removed: -{points} (total: {result})", this.Character);
        }

        /// <summary>
        /// Remove node and all dependent nodes.
        /// </summary>
        public void ServerRemoveNode(TechNode techNode)
        {
            if (!this.Nodes.Remove(techNode))
            {
                return;
            }

            Api.SafeInvoke(
                () => ServerCharacterTechNodeAddedOrRemoved?.Invoke(this.Character, techNode, isAdded: false));
            this.Character.SharedSetFinalStatsCacheDirty();

            // remove all the dependent nodes
            foreach (var dependentNode in techNode.DependentNodes)
            {
                this.ServerRemoveNode(dependentNode);
            }
        }

        public void ServerResetLearningPointsRemainder()
        {
            this.LearningPointsRemainderAccumulator = 0;
        }

        public void ServerSetLearningPoints(int points)
        {
            var pointsUshort = (ushort)MathHelper.Clamp(points, 0, ushort.MaxValue);
            this.LearningPoints = pointsUshort;
            this.LearningPointsRemainderAccumulator = 0;
            Api.Logger.Info("Learning points reset to " + pointsUshort, this.Character);
        }

        public int SharedGetUnlockedNodesCount(TechGroup techGroup)
        {
            return this.Groups.Contains(techGroup)
                       ? this.Nodes.Count(n => n.Group == techGroup)
                       : 0;
        }

        public double SharedGetUnlockedNodesPercent(TechGroup techGroup)
        {
            return this.SharedGetUnlockedNodesCount(techGroup) / (double)techGroup.Nodes.Count;
        }

        public bool SharedIsGroupUnlocked<TTechGroup>(double unlockedSkillsPercent)
            where TTechGroup : TechGroup, new()
        {
            return this.SharedIsGroupUnlocked(Api.GetProtoEntity<TTechGroup>(), unlockedSkillsPercent);
        }

        public bool SharedIsGroupUnlocked(TechGroup techGroup, double unlockedSkillsPercent)
        {
            var unlockedPercent = this.SharedGetUnlockedNodesPercent(techGroup);
            return unlockedPercent >= unlockedSkillsPercent;
        }

        public bool SharedIsGroupUnlocked(TechGroup techGroup)
        {
            return this.Groups.Contains(techGroup);
        }

        public bool SharedIsGroupUnlocked<TTechGroup>()
            where TTechGroup : TechGroup
        {
            return this.Groups.Any(g => g is TTechGroup);
        }

        public bool SharedIsNodeUnlocked(TechNode techNode)
        {
            return this.Nodes.Contains(techNode);
        }

        public bool SharedIsNodeUnlocked<TTechNode>()
            where TTechNode : TechNode
        {
            return this.Nodes.Any(g => g is TTechNode);
        }

        private void ServerAddNodeNoCheckGroup(TechNode techNode)
        {
            if (!techNode.IsAvailable)
            {
                return;
            }

            if (this.Nodes.Contains(techNode))
            {
                return;
            }

            this.Nodes.Add(techNode);
            var character = this.Character;
            Api.Logger.Info("Tech node added: " + techNode.ShortId, character);
            Api.SafeInvoke(() => ServerCharacterTechNodeAddedOrRemoved?.Invoke(character, techNode, isAdded: true));

            // add all required nodes recursively
            var currentNode = techNode;
            do
            {
                this.ServerAddNodeNoCheckGroup(currentNode);
                currentNode = currentNode.RequiredNode;
            }
            while (currentNode is not null);

            character.SharedSetFinalStatsCacheDirty();
        }
    }
}