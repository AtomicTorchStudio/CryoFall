namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
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
            = new(FindProtoEntities<TechNode>);

        static TechGroup()
        {
            if (IsClient)
            {
                PveSystem.ClientIsPvEChanged += SharedRebuildAllNodes;
                RatePvPTimeGates.ClientValueChanged += SharedRebuildAllNodes;
            }
        }

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
            iconPath = iconPath.Substring(0, iconPath.Length - thisType.Name.Length);
            iconPath = iconPath.TrimEnd('.');
            var icon = new TextureResource("Technologies/" + iconPath.Replace(".", "/"));
            if (!Api.Shared.IsFileExists(icon))
            {
                Api.Logger.Warning("Icon not found: " + icon.FullPath + ", using default generic icon.");
                // icon not found - fallback to default texture
                icon = new TextureResource("Technologies/GenericGroupIcon.png");
            }

            this.Icon = icon;
        }

        public static event Action AvailableTechGroupsChanged;

        public event Action NodesChanged;

        public static IReadOnlyList<TechGroup> AvailableTechGroups { get; private set; }

        public virtual FeatureAvailability AvailableIn => FeatureAvailability.All;

        public abstract string Description { get; }

        public IReadOnlyTechGroupRequirements GroupRequirements { get; private set; }

        public ITextureResource Icon { get; }

        public bool IsAvailable
        {
            get
            {
                return this.AvailableIn switch
                {
                    FeatureAvailability.None    => false,
                    FeatureAvailability.All     => true,
                    FeatureAvailability.OnlyPvP => !PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false),
                    FeatureAvailability.OnlyPvE => PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false),
                    _                           => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        /// Determines whether this tech group belongs to
        /// the primary tech groups section or to the secondary
        /// (this is a pure UI feature and doesn't have any other effect).
        /// </summary>
        public virtual bool IsPrimary => false;

        public ushort LearningPointsPrice { get; private set; }

        public string NameWithTierName
            => string.Format(CoreStrings.TechGroup_NameWithTier_Format,
                             this.Name,
                             ViewModelTechTier.GetTierText(this.Tier));

        public IReadOnlyList<TechNode> Nodes { get; private set; }

        public virtual int Order { get; } = 0;

        public IReadOnlyList<TechNode> RootNodes { get; private set; }

        public override string ShortId { get; }

        public abstract TechTier Tier { get; }

        public double TimeGatePvP { get; private set; }

        protected internal virtual double GroupNodesUnlockPriceMultiplier { get; } = 1;

        protected virtual double GroupUnlockPriceMultiplier { get; } = 1;

        public bool SharedCanUnlock(ICharacter character, bool skipLearningPointsCheck, out string error)
        {
            if (!this.IsAvailable)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                error = "This tech group is not available in the current version of the game";
                return false;
            }

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

            this.LearningPointsPrice = this.CalculateLearningPointsPrice();
            SharedRebuildAllNodes();
        }

        protected abstract void PrepareTechGroup(Requirements requirements);

        private static void SharedRebuildAllNodes()
        {
            if (Api.IsClient
                && !PveSystem.ClientIsPveFlagReceived)
            {
                AvailableTechGroups = new TechGroup[0];
                return;
            }

            var isPvE = PveSystem.SharedIsPve(false);

            var allTechGroups = Api.FindProtoEntities<TechGroup>();
            foreach (var techGroup in allTechGroups)
            {
                SetupTimeGate(techGroup);

                techGroup.Nodes = techGroup.IsAvailable
                                      ? LazyAllNodesWithoutFiltering
                                        .Value
                                        .Where(n => n.Group == techGroup
                                                    && n.IsAvailable)
                                        .OrderBy(n => n.HierarchyLevel)
                                        .ThenBy(n => n.Order)
                                        .ThenBy(n => n.ShortId)
                                        .ToList()
                                      : new TechNode[0];

                var rootNodes = new List<TechNode>();
                foreach (var node in techGroup.Nodes)
                {
                    node.SharedResetCachedLearningPointsPrice();

                    if (node.IsRootNode)
                    {
                        rootNodes.Add(node);
                    }
                }

                techGroup.RootNodes = rootNodes;

                var requirements = new Requirements();
                techGroup.PrepareTechGroup(requirements);
                if (techGroup.LearningPointsPrice > 0)
                {
                    requirements.Insert(0, new TechGroupRequirementLearningPoints(techGroup.LearningPointsPrice));
                }

                if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false)
                    && techGroup.TimeGatePvP > 0)
                {
                    requirements.Add(new TechGroupRequirementTimeGate(techGroup.TimeGatePvP));
                }

                techGroup.GroupRequirements = requirements;

                Api.SafeInvoke(techGroup.NodesChanged);
            }

            AvailableTechGroups = allTechGroups.Where(t => t.IsAvailable).ToArray();
            Api.SafeInvoke(AvailableTechGroupsChanged);

            void SetupTimeGate(TechGroup techGroup)
            {
                if (isPvE)
                {
                    techGroup.TimeGatePvP = 0;
                    return;
                }

                techGroup.TimeGatePvP = TechConstants.PvPTimeGates.Get(techGroup.Tier,
                                                                       isSpecialized: !techGroup.IsPrimary);
            }
        }

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