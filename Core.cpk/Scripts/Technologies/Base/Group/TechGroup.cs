namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(TechNode))]
    public abstract class TechGroup : ProtoEntity
    {
        public const string ErrorNotEnoughLearningPoints = "Not enough learning points";

        public const string ErrorTechGroupIsLocked = "The tech tree is locked";

        public const string ErrorTechIsAlreadyUnlocked = "The tech is already unlocked";

        private static readonly Lazy<List<TechNode>> LazyAllNodesWithoutFiltering
            = new Lazy<List<TechNode>>(FindProtoEntities<TechNode>);

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected TechGroup()
        {
            var thisType = this.GetType();
            var name = thisType.Name;
            if (!name.StartsWith("TechGroup", StringComparison.Ordinal))
            {
                throw new Exception("TechGroup class name must start with \"TechGroup\": " + thisType.Name);
            }

            this.ShortId = name.Substring(startIndex: "TechGroup".Length);

            var protoTechGroup = typeof(TechGroup);
            var iconPath = thisType.FullName
                                   // remove namespace of base class
                                   .Substring(protoTechGroup.FullName.Length - protoTechGroup.Name.Length);
            iconPath = iconPath.Substring(0, iconPath.Length - thisType.Name.Length) + "Group";

            var icon = new TextureResource("Technologies/" + iconPath.Replace('.', '/'));
            if (!Api.Shared.IsFileExists(icon))
            {
                Api.Logger.Warning("Icon not found: " + icon.FullPath + ", using default generic icon.");
                // icon not found - fallback to default texture
                icon = new TextureResource("Technologies/GenericGroupIcon.png");
            }

            this.Icon = icon;
        }

        public event Action NodesChanged;

        public abstract string Description { get; }

        public IReadOnlyTechGroupRequirements GroupRequirements { get; private set; }

        public ITextureResource Icon { get; }

        /// <summary>
        /// Determines whether this tech group belongs to
        /// the primary tech groups section or to the secondary
        /// (this is a pure UI feature and doesn't have any other effect).
        /// </summary>
        public virtual bool IsPrimary => false;

        public ushort LearningPointsPrice { get; private set; }

        public IReadOnlyList<TechNode> Nodes { get; private set; }

        public virtual int Order { get; } = 0;

        public IReadOnlyList<TechNode> RootNodes { get; private set; }

        public override string ShortId { get; }

        public abstract TechTier Tier { get; }

        protected internal virtual double GroupNodesUnlockPriceMultiplier { get; } = 1;

        protected virtual double GroupUnlockPriceMultiplier { get; } = 1;

        public bool SharedCanUnlock(ICharacter character, bool skipLearningPointsCheck, out string error)
        {
            var technologies = character.SharedGetTechnologies();
            if (technologies.SharedIsGroupUnlocked(this))
            {
                error = ErrorTechIsAlreadyUnlocked;
                return false;
            }

            foreach (var requirement in this.GroupRequirements)
            {
                if (skipLearningPointsCheck 
                    && requirement is TechGroupRequirementLearningPoints)
                {
                    continue;
                }

                if (!requirement.IsSatisfied(character, out error))
                {
                    return false;
                }
            }

            error = null;
            return true;
        }

        public void SharedValidateCanUnlock(ICharacter character, bool skipLearningPointsCheck)
        {
            if (!this.SharedCanUnlock(character, skipLearningPointsCheck, out var error))
            {
                throw new Exception($"Cannot unlock: {this}{Environment.NewLine}{error}");
            }
        }

        protected sealed override void PrepareProto()
        {
            if (this.Tier < TechConstants.MinTier)
            {
                throw new Exception(
                    $"Tier out of range: {this.Tier} - min tier is {TechConstants.MinTier}");
            }

            if (this.Tier > TechConstants.MaxTier)
            {
                throw new Exception(
                    $"Tier out of range: {this.Tier} - max tier is {TechConstants.MaxTier}");
            }

            this.SharedRebuildAllNodes();

            var rootNodes = new List<TechNode>();
            foreach (var protoTechNode in this.Nodes)
            {
                if (protoTechNode.RequiredNode == null)
                {
                    rootNodes.Add(protoTechNode);
                }
            }

            this.RootNodes = rootNodes;
            this.LearningPointsPrice = this.CalculateLearningPointsPrice();

            var requirements = new Requirements();
            this.PrepareTechGroup(requirements);
            if (this.LearningPointsPrice > 0)
            {
                requirements.Insert(0, new TechGroupRequirementLearningPoints(this.LearningPointsPrice));
            }

            this.GroupRequirements = requirements;

            if (IsClient)
            {
                PveSystem.ClientIsPvEChanged += this.SharedRebuildAllNodes;
            }
        }

        protected abstract void PrepareTechGroup(Requirements requirements);

        private ushort CalculateLearningPointsPrice()
        {
            var price = TechConstants.LearningPointsPriceBase
                        * TechConstants.TierGroupPriceMultiplier[(byte)this.Tier - 1]
                        * this.GroupUnlockPriceMultiplier;

            price = Math.Round(price, MidpointRounding.AwayFromZero);
            if (price > ushort.MaxValue)
            {
                throw new Exception("Learning points price exceeded: " + price);
            }

            return (ushort)price;
        }

        private void SharedRebuildAllNodes()
        {
            this.Nodes = LazyAllNodesWithoutFiltering
                         .Value
                         .Where(n => n.Group == this
                                     && n.IsAvailable)
                         .OrderBy(n => n.HierarchyLevel)
                         .ThenBy(n => n.Order)
                         .ThenBy(n => n.ShortId)
                         .ToList();

            Api.SafeInvoke(this.NodesChanged);
        }

        protected class Requirements : List<BaseTechGroupRequirement>, IReadOnlyTechGroupRequirements
        {
            public Requirements(int capacity = 4) : base(capacity)
            {
            }

            /// <summary>
            /// Adds group into requirements.
            /// </summary>
            /// <typeparam name="TProtoTechGroup">Group prototype.</typeparam>
            /// <param name="completion">Completion percent (from 0 to 1).</param>
            public Requirements AddGroup<TProtoTechGroup>(double completion)
                where TProtoTechGroup : TechGroup, new()
            {
                this.Add(new TechGroupRequirementGroupUnlocked<TProtoTechGroup>(completion));
                return this;
            }
        }
    }
}