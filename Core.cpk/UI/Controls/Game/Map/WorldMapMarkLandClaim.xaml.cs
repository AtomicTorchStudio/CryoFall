namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkLandClaim : BaseUserControl
    {
        public static readonly DependencyProperty IsFounderProperty =
            DependencyProperty.Register(nameof(IsFounder),
                                        typeof(bool),
                                        typeof(WorldMapMarkLandClaim),
                                        new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsUnderShieldProperty =
            DependencyProperty.Register(nameof(IsUnderShield),
                                        typeof(bool),
                                        typeof(WorldMapMarkLandClaim),
                                        new PropertyMetadata(default(bool)));

        public ILogicObject Area;

        private bool cachedIsUnderShield;

        public bool IsFounder
        {
            get => (bool)this.GetValue(IsFounderProperty);
            set => this.SetValue(IsFounderProperty, value);
        }

        public bool IsUnderShield
        {
            get => this.cachedIsUnderShield;
            set
            {
                if (this.cachedIsUnderShield == value)
                {
                    return;
                }

                this.cachedIsUnderShield = value;
                this.SetValue(IsUnderShieldProperty, value);
            }
        }

        protected override void OnLoaded()
        {
            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;
        }

        protected override void OnUnloaded()
        {
            this.MouseEnter -= this.MouseEnterOrLeaveHandler;
            this.MouseLeave -= this.MouseEnterOrLeaveHandler;
        }

        private static string GetShieldStatusText(LandClaimAreasGroupPublicState publicState)
        {
            var time = Api.Client.CurrentGame.ServerFrameTimeApproximated;

            switch (publicState.Status)
            {
                case ShieldProtectionStatus.Active:
                {
                    var timeRemains = publicState.ShieldEstimatedExpirationTime - time;
                    timeRemains = Math.Max(0, timeRemains);
                    return string.Format(CoreStrings.ShieldProtection_NotificationBaseUnderShield_Message_Format,
                                         ClientTimeFormatHelper.FormatTimeDuration(
                                             timeRemains,
                                             appendSeconds: false));
                }

                case ShieldProtectionStatus.Activating:
                {
                    var timeRemains = publicState.ShieldActivationTime - time;
                    timeRemains = Math.Max(0, timeRemains);
                    return string.Format(
                        CoreStrings.ShieldProtection_NotificationBaseActivatingShield_Message_Format,
                        ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
                }

                default:
                    return null;
            }
        }

        private void MouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsMouseOver)
            {
                ToolTipServiceExtend.SetToolTip(this, null);
                return;
            }

            var textBlockTitle = new FormattedTextBlock()
            {
                Content = this.IsFounder
                              ? CoreStrings.WorldMapMarkLandClaim_Tooltip_Owner
                              : CoreStrings.WorldMapMarkLandClaim_Tooltip
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            grid.Children.Add(textBlockTitle);

            var areasGroup = LandClaimArea.GetPublicState(this.Area).LandClaimAreasGroup;
            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);

            if (publicState.Status == ShieldProtectionStatus.Active
                || publicState.Status == ShieldProtectionStatus.Activating)
            {
                var shieldStatusText = GetShieldStatusText(publicState);

                var textBlockShield = new FormattedTextBlock()
                {
                    Content = "[br]" + shieldStatusText,
                    MaxWidth = 300
                };

                Grid.SetRow(textBlockShield, 1);
                grid.Children.Add(textBlockShield);
            }

            ToolTipServiceExtend.SetToolTip(this, grid);
        }
    }
}