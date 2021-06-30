namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(Recipe))]
    [PrepareOrder(afterType: typeof(IProtoStaticWorldObject))]
    public abstract class TechNode : ProtoEntity
    {
        public const string ErrorRequiresUnlockedTechGroup = "Requires unlocked tech: {0}";

        public static readonly IReadOnlyList<TechNode> EmptyList
            = new TechNode[0];

        protected static readonly TextureResource IconPlaceholder
            = new("Technologies/TempNodeIcon.png");

        private ushort? cachedLearningPointsPrice;

        private List<TechNode> dependentNodes;

        private Lazy<byte> lazyHierarchyLevel;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected TechNode()
        {
            var thisType = this.GetType();
            var name = thisType.Name;
            if (!name.StartsWith("TechNode", StringComparison.Ordinal))
            {
                throw new Exception("TechNode class name must start with \"TechNode\": " + thisType.Name);
            }

            var shortId = name.Substring(startIndex: "TechNode".Length);
            this.ShortId = shortId;
        }

        public virtual FeatureAvailability AvailableIn => FeatureAvailability.All;

        public IReadOnlyList<TechNode> DependentNodes => this.dependentNodes ?? EmptyList;

        public virtual string Description { get; }

        public abstract TechGroup Group { get; }

        public byte HierarchyLevel => this.lazyHierarchyLevel.Value;

        public virtual ITextureResource Icon
            => this.NodeEffects.Count > 0
                   ? this.NodeEffects[0].Icon
                   : IconPlaceholder;

        public bool IsAvailable
        {
            get
            {
                if (!this.Group.IsAvailable)
                {
                    return false;
                }

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

        public bool IsRootNode => this.RequiredNode is null
                                  || !this.RequiredNode.IsAvailable;

        public ushort LearningPointsPrice
        {
            get
            {
                this.cachedLearningPointsPrice ??= this.CalculateLearningPointsPrice();
                return this.cachedLearningPointsPrice.Value;
            }
        }

        public override string Name => this.NodeEffects[0].ShortDescription;

        public IReadOnlyTechNodeEffects NodeEffects { get; private set; }

        public virtual int Order => 0;

        public TechNode RequiredNode { get; private set; }

        public override string ShortId { get; }

        protected virtual double NodeUnlockPriceMultiplier => 1d;

        public bool SharedCanUnlock(ICharacter character, out string error)
        {
            if (!this.IsAvailable)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                error = "This tech node is not available in the current version of the game";
                return false;
            }

            var technologies = character.SharedGetTechnologies();
            if (!technologies.SharedIsGroupUnlocked(this.Group))
            {
                error = TechGroup.ErrorTechGroupIsLocked;
                return false;
            }

            if (technologies.SharedIsNodeUnlocked(this))
            {
                error = TechGroup.ErrorTechIsAlreadyUnlocked;
                return false;
            }

            if (this.RequiredNode is not null
                && this.RequiredNode.IsAvailable)
            {
                if (!technologies.SharedIsNodeUnlocked(this.RequiredNode))
                {
                    error = string.Format(ErrorRequiresUnlockedTechGroup, this.RequiredNode.Name);
                    return false;
                }
            }

            if (technologies.LearningPoints
                < this.LearningPointsPrice)
            {
                error = TechGroup.ErrorNotEnoughLearningPoints;
                return false;
            }

            error = null;
            return true;
        }

        public void SharedResetCachedLearningPointsPrice()
        {
            this.cachedLearningPointsPrice = null;
        }

        public void SharedValidateCanUnlock(ICharacter character)
        {
            if (!this.SharedCanUnlock(character, out var error))
            {
                throw new Exception($"Cannot unlock: {this}{Environment.NewLine}{error}");
            }
        }

        protected static TTechGroup GetGroup<TTechGroup>()
            where TTechGroup : TechGroup, new()
        {
            return Api.GetProtoEntity<TTechGroup>();
        }

        protected static TTechNode GetNode<TTechNode>()
            where TTechNode : TechNode, new()
        {
            return Api.GetProtoEntity<TTechNode>();
        }

        protected sealed override void PrepareProto()
        {
            this.PrepareTechNode(out var requiredNode, out var effects);

            this.RequiredNode = requiredNode;
            this.ValidateRequiredNode();

            this.RequiredNode?.PrepareRegisterDependentNode(this);

            if (effects is not null)
                //&& effects.Count > 0)
            {
                this.NodeEffects = effects;
            }
            else
            {
                throw new Exception("No effects provided");
            }

            if (this.AvailableIn != FeatureAvailability.None)
            {
                foreach (var effect in effects)
                {
                    effect.PrepareEffect(this);
                }
            }

            this.lazyHierarchyLevel = new Lazy<byte>(
                () =>
                {
                    var level = 0;
                    var node = this.RequiredNode;
                    while (node is not null)
                    {
                        level++;
                        node = node.RequiredNode;
                    }

                    if (level > byte.MaxValue)
                    {
                        throw new Exception("Max level of hierarchy exceeded: " + this);
                    }

                    return (byte)level;
                });

            // validate the name property
            try
            {
                // ReSharper disable once UnusedVariable
                var name = this.Name;
            }
            catch
            {
                throw new Exception(
                    "The tech node was not properly setup - please override Name or add at least one effect");
            }
        }

        protected abstract void PrepareTechNode(
            out TechNode requiredNode,
            out IReadOnlyTechNodeEffects effects);

        private ushort CalculateLearningPointsPrice()
        {
            if (this.IsRootNode
                && this.Group.LearningPointsPrice > 0)
            {
                return 0;
            }

            var price = TechConstants.LearningPointsPriceBase
                        * TechConstants.TierNodePriceMultiplier[(byte)this.Group.Tier - 1]
                        * this.Group.GroupNodesUnlockPriceMultiplier
                        * this.NodeUnlockPriceMultiplier;

            price = Math.Round(price, MidpointRounding.AwayFromZero);
            if (price > ushort.MaxValue)
            {
                throw new Exception("Learning points price exceeded: " + price);
            }

            return (ushort)price;
        }

        private void PrepareRegisterDependentNode(TechNode dependentNode)
        {
            this.dependentNodes ??= new List<TechNode>();
            this.dependentNodes.Add(dependentNode);
        }

        private void ValidateRequiredNode()
        {
            var requiredNode = this.RequiredNode;
            if (requiredNode is null)
            {
                return;
            }

            if (requiredNode == this)
            {
                throw new Exception("The node requires itself to unlock");
            }

            if (requiredNode.Group != this.Group)
            {
                throw new Exception("Cannot require node from another group");
            }

            var baseNode = requiredNode;
            do
            {
                requiredNode = requiredNode.RequiredNode;
                if (requiredNode is null)
                {
                    return;
                }

                if (requiredNode == this)
                {
                    throw new Exception(
                        $"Cyclic dependency found. This node requires another nodes, one of which (node {baseNode}) requires this node");
                }
            }
            while (true);
        }
    }

    public abstract class TechNode<TProtoTechGroup> : TechNode
        where TProtoTechGroup : TechGroup, new()
    {
        private static readonly Lazy<TechGroup> LazyGroup
            = new(GetProtoEntity<TProtoTechGroup>);

        public sealed override TechGroup Group => LazyGroup.Value;

        protected sealed override void PrepareTechNode(
            out TechNode requiredNode,
            out IReadOnlyTechNodeEffects effects)
        {
            var config = new Config();
            this.PrepareTechNode(config);
            requiredNode = config.GetRequiredNode();
            effects = config.Effects;
        }

        protected abstract void PrepareTechNode(Config config);

        protected class Config
        {
            public readonly Effects Effects = new();

            private TechNode<TProtoTechGroup> requiredNode;

            public TechNode<TProtoTechGroup> GetRequiredNode()
            {
                return this.requiredNode;
            }

            public void SetRequiredNode<TTechNode>()
                where TTechNode : TechNode<TProtoTechGroup>, new()
            {
                this.requiredNode = GetNode<TTechNode>();
            }
        }

        protected class Effects : List<BaseTechNodeEffect>, IReadOnlyTechNodeEffects
        {
            public Effects AddPerk<TPerk>()
                where TPerk : ProtoPerk, new()
            {
                this.Add(new TechNodeEffectPerkUnlock(GetProtoEntity<TPerk>()));
                return this;
            }

            public Effects AddRecipe<TRecipe>(bool isHidden = false)
                where TRecipe : Recipe, new()
            {
                this.Add(new TechNodeEffectRecipeUnlock(GetProtoEntity<TRecipe>(), isHidden));
                return this;
            }

            public Effects AddStructure<TStructure>()
                where TStructure : class, IProtoObjectStructure, new()
            {
                this.Add(new TechNodeEffectStructureUnlock(GetProtoEntity<TStructure>()));
                return this;
            }

            public Effects AddVehicle<TVehicle>()
                where TVehicle : class, IProtoVehicle, new()
            {
                this.Add(new TechNodeEffectVehicleUnlock(GetProtoEntity<TVehicle>()));
                return this;
            }
        }
    }
}