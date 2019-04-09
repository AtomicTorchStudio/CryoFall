namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Helpers.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelPhysicsSpaceVisualizerLegend : BaseViewModel
    {
        private readonly Action sourceFilterChangedCallback;

        private bool isClientTestRendered;

        private bool isServerTestRendered;

        public ViewModelPhysicsSpaceVisualizerLegend(
            IReadOnlyList<ViewModelPhysicsGroup> groups,
            bool isClientTestRendered,
            bool isServerTestRendered,
            Action sourceFilterChangedCallback)
        {
            this.Groups = groups;
            this.isClientTestRendered = isClientTestRendered;
            this.isServerTestRendered = isServerTestRendered;
            this.sourceFilterChangedCallback = sourceFilterChangedCallback;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public ViewModelPhysicsSpaceVisualizerLegend()
            : this(
                new List<ViewModelPhysicsGroup>()
                {
                    new ViewModelPhysicsGroup("Test 1", CollisionGroupId.Default,     true,  Brushes.White),
                    new ViewModelPhysicsGroup("Test 2", CollisionGroupId.Default,     false, Brushes.GreenYellow),
                    new ViewModelPhysicsGroup("Test 3", CollisionGroupId.HitboxMelee, true,  Brushes.Red)
                },
                isClientTestRendered: true,
                isServerTestRendered: true,
                sourceFilterChangedCallback: null)
        {
        }

        public IReadOnlyList<ViewModelPhysicsGroup> Groups { get; }

        public bool IsClientTestRendered
        {
            get => this.isClientTestRendered;
            set
            {
                if (this.isClientTestRendered == value)
                {
                    return;
                }

                this.isClientTestRendered = value;
                this.NotifyThisPropertyChanged();

                this.sourceFilterChangedCallback();
            }
        }

        public bool IsServerTestRendered
        {
            get => this.isServerTestRendered;
            set
            {
                if (this.isServerTestRendered == value)
                {
                    return;
                }

                this.isServerTestRendered = value;
                this.NotifyThisPropertyChanged();

                this.sourceFilterChangedCallback();
            }
        }
    }
}