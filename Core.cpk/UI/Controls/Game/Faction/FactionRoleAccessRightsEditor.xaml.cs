namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public partial class FactionRoleAccessRightsEditor : BaseUserControl
    {
        private readonly List<CheckBox> checkboxesWithEvents = new();

        private readonly List<FactionRoleTitleEditor> factionRoleTitleEditors = new();

        private Grid grid;

        protected override void InitControl()
        {
            this.grid = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            var faction = FactionSystem.ClientCurrentFaction;
            var roleTitleBinding = Faction.GetPrivateState(faction).OfficerRoleTitleBinding;
            var gridChildren = this.grid.Children;
            var controlHorizontalSeparator = this.GetResource<Style>("ControlGridHorizontalSeparator");

            var roles = EnumExtensions.GetValues<FactionMemberRole>()
                                      .ExceptOne(FactionMemberRole.Member)
                                      .ExceptOne(FactionMemberRole.Leader)
                                      .ToArray();

            var accessRights = EnumExtensions.GetValues<FactionMemberAccessRights>()
                                             .ExceptOne(FactionMemberAccessRights.None)
                                             .ExceptOne(FactionMemberAccessRights.Leader)
                                             .ToArray();

            if (PveSystem.ClientIsPve(true))
            {
                accessRights = accessRights.ExceptOne(FactionMemberAccessRights.DiplomacyManagement)
                                           .ToArray();
            }

            var columnDefinitions = this.grid.ColumnDefinitions;
            columnDefinitions.Clear();
            var firstColumnWidth = 120;
            columnDefinitions.Add(new ColumnDefinition()
                                      { Width = new GridLength(firstColumnWidth, GridUnitType.Pixel) });

            var rowDefinitions = this.grid.RowDefinitions;
            rowDefinitions.Clear();
            rowDefinitions.Add(new RowDefinition());

            var rowHeight = 35;

            for (var roleIndex = 0; roleIndex < roles.Length; roleIndex++)
            {
                columnDefinitions.Add(new ColumnDefinition());
                var role = roles[roleIndex];
                var editor = new FactionRoleTitleEditor(role, roleTitleBinding[role]);
                Grid.SetColumn(editor, roleIndex + 1);
                gridChildren.Add(editor);

                editor.SelectedTitleChanged += EditorSelectedTitleChangedHandler;
                this.factionRoleTitleEditors.Add(editor);
            }

            for (var accessRightIndex = 0; accessRightIndex < accessRights.Length; accessRightIndex++)
            {
                if (accessRightIndex > 0)
                {
                    // add padding row
                    rowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    var line = new Control() { Style = controlHorizontalSeparator };
                    Grid.SetRow(line, accessRightIndex * 2);
                    Grid.SetColumnSpan(line, roles.Length + 1);
                    gridChildren.Add(line);
                }

                rowDefinitions.Add(new RowDefinition() { MinHeight = rowHeight });
                var accessRight = accessRights[accessRightIndex];
                var textBlock = new TextBlock()
                {
                    Text = accessRight.GetDescription(),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap
                };

                if (accessRight.GetAttribute<DescriptionTooltipAttribute>() is { } descriptionTooltipAttribute)
                {
                    var controlInfoPoint = new Control()
                    {
                        Style = Api.Client.UI.GetApplicationResource<Style>("ControlInfoPointStyle"),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 0, -14, 0)
                    };

                    var elementGrid = new Grid()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    ToolTipServiceExtend.SetToolTip(elementGrid,
                                                    new FormattedTextBlock()
                                                    {
                                                        Text = descriptionTooltipAttribute.TooltipMessage,
                                                        MaxWidth = 300
                                                    });
                    elementGrid.Children.Add(textBlock);
                    elementGrid.Children.Add(controlInfoPoint);

                    Grid.SetRow(elementGrid, accessRightIndex * 2 + 1);
                    gridChildren.Add(elementGrid);
                }
                else
                {
                    Grid.SetRow(textBlock, accessRightIndex * 2 + 1);
                    gridChildren.Add(textBlock);
                }
            }

            for (var accessRightIndex = 0; accessRightIndex < accessRights.Length; accessRightIndex++)
            {
                var accessRight = accessRights[accessRightIndex];

                for (var roleIndex = 0; roleIndex < roles.Length; roleIndex++)
                {
                    var role = roles[roleIndex];

                    var checkbox = new CheckBox();
                    Grid.SetRow(checkbox, accessRightIndex * 2 + 1);
                    Grid.SetColumn(checkbox, roleIndex + 1);
                    gridChildren.Add(checkbox);

                    switch (role)
                    {
                        case FactionMemberRole.Member:
                            checkbox.IsChecked = false;
                            checkbox.IsEnabled = false;
                            checkbox.Opacity = 0.667;
                            break;

                        case FactionMemberRole.Leader:
                            checkbox.IsChecked = true;
                            checkbox.IsEnabled = false;
                            break;

                        default:
                            checkbox.Tag = (role, accessRight);
                            checkbox.IsChecked = FactionSystem.SharedGetRoleAccessRights(faction, role)
                                                              .HasFlag(accessRight);

                            this.checkboxesWithEvents.Add(checkbox);
                            checkbox.Checked += CheckboxChangedHandler;
                            checkbox.Unchecked += CheckboxChangedHandler;
                            break;
                    }
                }
            }
        }

        protected override void OnUnloaded()
        {
            foreach (var checkbox in this.checkboxesWithEvents)
            {
                checkbox.Checked -= CheckboxChangedHandler;
                checkbox.Unchecked -= CheckboxChangedHandler;
            }

            this.checkboxesWithEvents.Clear();

            foreach (var editor in this.factionRoleTitleEditors)
            {
                editor.SelectedTitleChanged -= EditorSelectedTitleChangedHandler;
            }

            this.factionRoleTitleEditors.Clear();

            this.grid.Children.Clear();
        }

        private static void CheckboxChangedHandler(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            (var role, var accessRight) = (ValueTuple<FactionMemberRole, FactionMemberAccessRights>)checkbox.Tag;

            FactionSystem.ClientLeaderSetMemberRoleAccessRight(role,
                                                               accessRight,
                                                               isAssigned: checkbox.IsChecked == true);
        }

        private static void EditorSelectedTitleChangedHandler(FactionRoleTitleEditor editor)
        {
            FactionSystem.ClientLeaderSetOfficerRoleTitle(editor.Role, editor.SelectedTitle);
        }
    }
}