namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWorldObjectFactionAccessEditorControl : BaseViewModel
    {
        private readonly FactionMemberAccessRights accessRight;

        private readonly bool isClosedModeAvailable;

        private readonly bool isEveryoneModeAvailable;

        private readonly IObjectWithAccessModePrivateState privateState;

        private readonly IWorldObject worldObject;

        public ViewModelWorldObjectFactionAccessEditorControl(IWorldObject worldObject)
        {
            this.worldObject = worldObject;
            this.accessRight = WorldObjectAccessModeSystem.SharedGetFactionAccessRightRequirementForObject(worldObject);

            if (worldObject.ProtoGameObject is not IProtoObjectWithAccessMode protoObjectWithAccessMode)
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            this.isClosedModeAvailable = protoObjectWithAccessMode.IsClosedAccessModeAvailable;
            this.isEveryoneModeAvailable = protoObjectWithAccessMode.IsEveryoneAccessModeAvailable;

            this.privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();
            this.privateState.ClientSubscribe(_ => _.FactionAccessMode,
                                              _ => this.RefreshCheckboxes(),
                                              this);

            FactionSystem.ClientCurrentFactionAccessRightsChanged += this.CurrentFactionAccessRightsChanged;

            this.RecreateViewModelAccessModes();
        }

        public bool HasFactionAccessRight
            => FactionSystem.ClientCurrentFaction is not null
               && (CreativeModeSystem.ClientIsInCreativeMode()
                   || FactionSystem.ClientHasAccessRight(this.accessRight));

        public ViewModelFactionAccessMode[] ViewModelAccessModes { get; set; }

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionAccessRightsChanged -= this.CurrentFactionAccessRightsChanged;
            base.DisposeViewModel();
        }

        private void CurrentFactionAccessRightsChanged()
        {
            this.NotifyPropertyChanged(nameof(this.HasFactionAccessRight));
            this.RecreateViewModelAccessModes();
        }

        private void OnModeChanged()
        {
            var accessModes = this.isClosedModeAvailable
                                  ? WorldObjectFactionAccessModes.Closed
                                  : WorldObjectFactionAccessModes.Leader;

            // first entry is skipped as it's for the Closed mode
            foreach (var viewModel in this.ViewModelAccessModes.Skip(1))
            {
                if (viewModel.IsChecked)
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

        private void RecreateViewModelAccessModes()
        {
            IEnumerable<WorldObjectFactionAccessModes> accessModes
                = EnumExtensions.GetValues<WorldObjectFactionAccessModes>();

            if (!this.isClosedModeAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectFactionAccessModes.Closed);
            }

            if (!this.isEveryoneModeAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectFactionAccessModes.Everyone);
                accessModes = accessModes.ExceptOne(WorldObjectFactionAccessModes.AllyFactionMembers);
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

            if (!this.isClosedModeAvailable)
            {
                this.ViewModelAccessModes[0].IsEnabled = false;
            }

            var protoObjectWithAccessMode = (IProtoObjectWithAccessMode)this.worldObject.ProtoGameObject;
            if (!protoObjectWithAccessMode.CanChangeFactionRoleAccessForSelfRole)
            {
                var currentRole = FactionSystem.ClientCurrentRole;
                foreach (var vm in this.ViewModelAccessModes)
                {
                    var mode = vm.Mode;
                    if ((mode == WorldObjectFactionAccessModes.Officer1
                         && currentRole == FactionMemberRole.Officer1)
                        || (mode == WorldObjectFactionAccessModes.Officer2
                            && currentRole == FactionMemberRole.Officer2)
                        || (mode == WorldObjectFactionAccessModes.Officer3
                            && currentRole == FactionMemberRole.Officer3))
                    {
                        // cannot change access rights for the current role (to prevent lock-out)
                        vm.IsEnabled = false;
                        vm.TooltipMessage = string.Format("({0} {1})",
                                                          CoreStrings.Faction_CurrentRole_Field,
                                                          FactionSystem.ClientGetRoleTitle(currentRole));
                    }
                }
            }

            this.RefreshCheckboxes();
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

            public bool HasExtraPadding => this.Mode == WorldObjectFactionAccessModes.Everyone;

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

            public string TooltipMessage { get; set; }

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