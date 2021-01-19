namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWorldObjectFactionAccessEditorControl : BaseViewModel
    {
        private readonly IObjectWithAccessModePrivateState privateState;

        private readonly IStaticWorldObject worldObject;

        public ViewModelWorldObjectFactionAccessEditorControl(
            IStaticWorldObject worldObject,
            bool canSetAccessMode)
        {
            this.worldObject = worldObject;
            this.CanSetAccessMode = canSetAccessMode;

            if (!(worldObject.ProtoGameObject is IProtoObjectWithAccessMode protoObjectWithAccessMode))
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            this.IsClosedModeAvailable = protoObjectWithAccessMode.IsClosedAccessModeAvailable;

            IEnumerable<WorldObjectFactionAccessModes> accessModes
                = EnumExtensions.GetValues<WorldObjectFactionAccessModes>();

            if (!this.IsClosedModeAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectFactionAccessModes.Closed);
            }

            if (!FactionSystem.SharedIsDiplomacyFeatureAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectFactionAccessModes.AllyFactionMembers);
            }

            this.ViewModelAccessModes = accessModes
                                        .Select(mode =>
                                                    mode == WorldObjectFactionAccessModes.Closed
                                                        ? new ViewModelFactionAccessModeClosed(
                                                            this.OnModeIsClosedChanged)
                                                        : new ViewModelFactionAccessMode(mode, this.OnModeChanged))
                                        .ToArray();

            if (!this.IsClosedModeAvailable)
            {
                this.ViewModelAccessModes[0].IsEnabled = false;
            }

            this.privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();

            this.privateState.ClientSubscribe(
                _ => _.FactionAccessMode,
                _ => this.RefreshCheckboxes(),
                this);

            this.RefreshCheckboxes();
        }

        public bool CanSetAccessMode { get; }

        public bool IsClosedModeAvailable { get; }

        public ViewModelFactionAccessMode[] ViewModelAccessModes { get; }

        private void OnModeChanged()
        {
            WorldObjectFactionAccessModes accessModes
                = this.IsClosedModeAvailable
                      ? WorldObjectFactionAccessModes.Closed
                      : WorldObjectFactionAccessModes.Leader;

            // first entry is skipped as it's for the Closed mode
            foreach (var viewModel in this.ViewModelAccessModes.Skip(1))
            {
                if (viewModel.IsEnabled
                    && viewModel.IsChecked)
                {
                    accessModes |= viewModel.Mode;
                }
            }

            this.RefreshCheckboxes();
            WorldObjectAccessModeSystem.ClientSetFactionAccessMode(this.worldObject, accessModes);
        }

        private void OnModeIsClosedChanged()
        {
            var isClosed = this.ViewModelAccessModes[0].IsChecked;
            if (isClosed)
            {
                this.RefreshCheckboxes();
                WorldObjectAccessModeSystem.ClientSetFactionAccessMode(this.worldObject,
                                                                       WorldObjectFactionAccessModes.Closed);
                return;
            }

            this.OnModeChanged();
        }

        private void RefreshCheckboxes()
        {
            var accessModes = this.privateState.FactionAccessMode;
            foreach (var viewModel in this.ViewModelAccessModes)
            {
                viewModel.Refresh(accessModes);
            }
        }

        public class ViewModelFactionAccessMode : BaseViewModel
        {
            public readonly WorldObjectFactionAccessModes Mode;

            private readonly Action callbackIsCheckedChanged;

            private bool isChecked;

            public ViewModelFactionAccessMode(WorldObjectFactionAccessModes mode, Action callbackIsCheckedChanged)
            {
                this.Mode = mode;
                this.callbackIsCheckedChanged = callbackIsCheckedChanged;
            }

            public bool IsChecked
            {
                get => this.isChecked;
                set
                {
                    if (this.isChecked == value)
                    {
                        return;
                    }

                    this.isChecked = value;
                    this.NotifyThisPropertyChanged();

                    this.callbackIsCheckedChanged();
                }
            }

            public bool IsEnabled { get; set; } = true;

            public string Title
            {
                get
                {
                    var descriptionAttribute = this.Mode.GetAttribute<DescriptionAttribute>();
                    if (descriptionAttribute is not null)
                    {
                        return descriptionAttribute.Description;
                    }

                    switch (this.Mode)
                    {
                        case WorldObjectFactionAccessModes.Closed:
                            return WorldObjectDirectAccessMode.Closed.GetDescription();

                        case WorldObjectFactionAccessModes.Leader:
                            return string.Format(CoreStrings.ObjectAccessModeEditor_OpensTo_Format,
                                                 CoreStrings.Faction_Role_Leader);

                        case WorldObjectFactionAccessModes.Officer1:
                            return GetTextOpensToOfficerRole(FactionMemberRole.Officer1);

                        case WorldObjectFactionAccessModes.Officer2:
                            return GetTextOpensToOfficerRole(FactionMemberRole.Officer2);

                        case WorldObjectFactionAccessModes.Officer3:
                            return GetTextOpensToOfficerRole(FactionMemberRole.Officer3);

                        case WorldObjectFactionAccessModes.Everyone:
                            return WorldObjectDirectAccessMode.OpensToEveryone.GetDescription();

                        default:
                            Logger.Error("Not defined: " + this.Mode);
                            return this.Mode.ToString();
                    }
                }
            }

            public void Refresh(WorldObjectFactionAccessModes currentModes)
            {
                var shouldBeChecked = this.IsShouldBeChecked(currentModes);
                if (this.isChecked == shouldBeChecked)
                {
                    return;
                }

                this.isChecked = shouldBeChecked;
                this.NotifyPropertyChanged(nameof(this.IsChecked));
            }

            protected virtual bool IsShouldBeChecked(WorldObjectFactionAccessModes currentModes)
            {
                return this.Mode == WorldObjectFactionAccessModes.Closed
                           ? this.Mode == WorldObjectFactionAccessModes.Closed
                           : currentModes.HasFlag(this.Mode);
            }

            private static string GetTextOpensToOfficerRole(FactionMemberRole role)
            {
                if (FactionSystem.ClientCurrentFaction is null)
                {
                    // useful for creative mode
                    return role.ToString();
                }

                return string.Format(CoreStrings.ObjectAccessModeEditor_OpensTo_Format,
                                     FactionSystem.ClientGetRoleTitle(role));
            }
        }

        private class ViewModelFactionAccessModeClosed : ViewModelFactionAccessMode
        {
            public ViewModelFactionAccessModeClosed(Action onModeChanged)
                : base(
                    WorldObjectFactionAccessModes.Closed,
                    onModeChanged)
            {
            }

            protected override bool IsShouldBeChecked(WorldObjectFactionAccessModes currentModes)
            {
                return currentModes == WorldObjectFactionAccessModes.Closed;
            }
        }
    }
}