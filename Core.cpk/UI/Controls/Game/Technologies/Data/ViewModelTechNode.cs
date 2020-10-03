namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data.TreeLayout;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelTechNode : BaseViewModel, ITreeNodeForLayout<ViewModelTechNode>
    {
        private static readonly IReadOnlyList<ViewModelTechNode> EmptyList = new ViewModelTechNode[0];

        private readonly string testTitle;

        [ViewModelNotAutoDisposeField]
        private readonly ViewModelTechTreeControl viewModelTechTree;

        private bool canUnlock;

        private List<ViewModelTechNode> childrenNodes;

        private bool isUnlocked;

        private IReadOnlyList<ViewModelTechNodeEffectPerkUnlock> listEffectsPerks;

        private IReadOnlyList<ViewModelTechNodeEffectRecipeUnlock> listEffectsRecipes;

        private IReadOnlyList<ViewModelTechNodeEffectStructureUnlock> listEffectsStructures;

        private IReadOnlyList<ViewModelTechNodeEffectVehicleUnlock> listEffectsVehicles;

        private ViewModelTechNode parentNode;

        private Vector2D position;

        public ViewModelTechNode(
            TechNode techNode,
            ViewModelTechTreeControl viewModelTechTree,
            BaseCommand commandOnNodeSelect)
        {
            this.viewModelTechTree = viewModelTechTree;
            this.CommandOnNodeSelect = commandOnNodeSelect;
            this.TechNode = techNode;

            this.HierarchyLevel = techNode.HierarchyLevel;

            ClientComponentTechnologiesWatcher.TechGroupsChanged += this.Refresh;
            ClientComponentTechnologiesWatcher.TechNodesChanged += this.Refresh;
            ClientComponentTechnologiesWatcher.LearningPointsChanged += this.Refresh;

            this.Refresh();
        }

        public ViewModelTechNode(
            string testTitle,
            int hierarchyLevel,
            ViewModelTechTreeControl viewModelTechTree,
            bool isUnlocked,
            bool canUnlock)
        {
            this.viewModelTechTree = viewModelTechTree;
            this.testTitle = testTitle;
            this.HierarchyLevel = hierarchyLevel;
            this.isUnlocked = isUnlocked;
            this.canUnlock = !isUnlocked && canUnlock;
        }

        public event Action CanUnlockChanged;

        public event Action IsUnlockedChanged;

        public string CannotUnlockMessage { get; private set; }

        public bool CanUnlock
        {
            get => this.canUnlock;
            private set
            {
                if (this.canUnlock == value)
                {
                    return;
                }

                this.canUnlock = value;
                this.NotifyThisPropertyChanged();

                this.CanUnlockChanged?.Invoke();
            }
        }

        public IReadOnlyList<ViewModelTechNode> ChildrenNodes => this.childrenNodes ?? EmptyList;

        public BaseCommand CommandOnNodeSelect { get; }

        public string Description => this.TechNode?.Description;

        public IReadOnlyList<ViewModelTechNodeEffectPerkUnlock> EffectsPerks
            => this.listEffectsPerks
                   ??= this.CollectEffectsOfType
                       <TechNodeEffectPerkUnlock,
                           ViewModelTechNodeEffectPerkUnlock>();

        public IReadOnlyList<ViewModelTechNodeEffectRecipeUnlock> EffectsRecipes
            => this.listEffectsRecipes
                   ??= this.CollectEffectsOfType
                       <TechNodeEffectRecipeUnlock,
                           ViewModelTechNodeEffectRecipeUnlock>();

        public IReadOnlyList<ViewModelTechNodeEffectStructureUnlock> EffectsStructures
            => this.listEffectsStructures
                   ??= this.CollectEffectsOfType
                       <TechNodeEffectStructureUnlock,
                           ViewModelTechNodeEffectStructureUnlock>();

        public IReadOnlyList<ViewModelTechNodeEffectVehicleUnlock> EffectsVehicles
            => this.listEffectsVehicles
                   ??= this.CollectEffectsOfType
                       <TechNodeEffectVehicleUnlock,
                           ViewModelTechNodeEffectVehicleUnlock>();

        public int HierarchyLevel { get; }

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.TechNode?.Icon);

        public bool IsUnlocked
        {
            get => this.isUnlocked;
            private set
            {
                if (this.isUnlocked == value)
                {
                    return;
                }

                this.isUnlocked = value;
                this.NotifyThisPropertyChanged();

                this.IsUnlockedChanged?.Invoke();
            }
        }

        public ushort LearningPointsPrice => this.TechNode?.LearningPointsPrice ?? 0;

        public ViewModelTechNode ParentNode => this.parentNode;

        public Vector2D Position => this.position;

        public double PositionX => this.position.X;

        public double PositionY => this.position.Y;

        public int PositionZ => 100;

        public TechNode TechNode { get; }

        public string Title => this.TechNode?.Name ?? this.testTitle;

        public Visibility VisibilityDescription => string.IsNullOrEmpty(this.Description)
                                                       ? Visibility.Collapsed
                                                       : Visibility.Visible;

        public Visibility VisibilityNoEffects => this.TechNode.NodeEffects.Count == 0
                                                     ? Visibility.Visible
                                                     : Visibility.Collapsed;

        IReadOnlyList<ViewModelTechNode> ITreeNodeForLayout<ViewModelTechNode>.Children => this.ChildrenNodes;

        public void SetLayoutPositionX(double x)
        {
            var vm = this.viewModelTechTree;
            var nodeSize = vm.NodeWidth;
            this.position = (
                                // padding + offset
                                x * nodeSize,
                                this.HierarchyLevel * vm.NodeLevelHeight);
        }

        public void SetParentNode(ViewModelTechNode parentNode)
        {
            if (this.parentNode is not null)
            {
                throw new Exception("Parent node is already set");
            }

            this.parentNode = parentNode ?? throw new ArgumentNullException();
            parentNode.RegisterChildren(this);
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return this.Title + " > " + (this.ParentNode?.ToString() ?? "<root>");
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            this.IsUnlockedChanged = null;
            if (this.TechNode is null)
            {
                return;
            }

            ClientComponentTechnologiesWatcher.TechGroupsChanged -= this.Refresh;
            ClientComponentTechnologiesWatcher.TechNodesChanged -= this.Refresh;
            ClientComponentTechnologiesWatcher.LearningPointsChanged -= this.Refresh;
        }

        private IReadOnlyList<TViewModel> CollectEffectsOfType<TEffect, TViewModel>()
            where TEffect : BaseTechNodeEffect
            where TViewModel : BaseViewModelTechNodeEffect
        {
            if (IsDesignTime)
            {
                return Array.Empty<TViewModel>();
            }

            var list = new List<TViewModel>();
            foreach (var effect in this.TechNode.NodeEffects.OfType<TEffect>())
            {
                var vm = effect.CreateViewModel();
                if (vm is not null)
                {
                    list.Add((TViewModel)vm);
                }
            }

            return list;
        }

        private void Refresh()
        {
            var isUnlocked = ClientComponentTechnologiesWatcher.CurrentTechnologies
                                                               .SharedIsNodeUnlocked(this.TechNode);
            this.IsUnlocked = isUnlocked;

            string cannotUnlockMessage = null;
            this.CanUnlock = !isUnlocked
                             && this.TechNode.SharedCanUnlock(
                                 Api.Client.Characters.CurrentPlayerCharacter,
                                 out cannotUnlockMessage);

            if (this.CanUnlock
                || cannotUnlockMessage is null)
            {
                this.CannotUnlockMessage = null;
                return;
            }

            if (!cannotUnlockMessage.EndsWith("."))
            {
                cannotUnlockMessage += '.';
            }

            this.CannotUnlockMessage = cannotUnlockMessage;
        }

        private void RegisterChildren(ViewModelTechNode childNode)
        {
            this.childrenNodes ??= new List<ViewModelTechNode>();
            this.childrenNodes.Add(childNode);
        }
    }
}