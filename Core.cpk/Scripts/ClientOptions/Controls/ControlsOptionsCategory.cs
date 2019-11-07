namespace AtomicTorch.CBND.CoreMod.ClientOptions.Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Audio;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ControlsOptionsCategory : ProtoOptionsCategory
    {
        public const string TitleGeneralControlsCategory = "General";

        private Dictionary<IWrappedButton, ButtonMapping> savedMapping;

        public override bool IsModified
        {
            get
            {
                var currentMapping = ClientInputManager.CloneMapping();
                if (this.savedMapping == null)
                {
                    // not initialized yet
                    return false;
                }

                if (currentMapping.SequenceEqual(this.savedMapping))
                {
                    // exactly the same collections - definitely not modified
                    return false;
                }

                // probably only the order is modified, so let's make a diff and compare
                currentMapping.GetDiff(this.savedMapping, out var added, out var removed);
                return added.Count > 0
                       || removed.Count > 0;
            }
        }

        public override string Name => "Controls";

        public override ProtoOptionsCategory OrderAfterCategory => GetProtoEntity<AudioOptionsCategory>();

        public override UIElement CreateControl()
        {
            var tableControl = new TableControl()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            tableControl.Loaded += (_, e) => PopulateKeys(tableControl);

            return new ScrollViewer()
            {
                Content = tableControl,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible
            };
        }

        public void SaveMappingIfRequired()
        {
            if (this.savedMapping == null)
            {
                this.SaveMapping();
            }
        }

        protected override void OnApply()
        {
            this.SaveMapping();
            ClientInputManager.Save();
        }

        protected override void OnCancel()
        {
            ClientInputManager.SetMapping(this.savedMapping);
        }

        protected override void OnReset()
        {
            ClientInputManager.ResetMappingToDefault();
            this.SaveMapping();
        }

        private static FrameworkElement GetInputMappingControl(IWrappedButton button, ButtonInfoAttribute buttonInfo)
        {
            var control = new ButtonMappingControl();
            control.Setup(button, buttonInfo);
            return control;
        }

        private static void PopulateKeys(TableControl tableControl)
        {
            tableControl.Clear();
            tableControl.FontSize = 14;

            var categories = ClientInputManager.GetKnownButtons()
                                               .GroupBy(pair => pair.Value.Category)
                                               .OrderBy(category => category.Key);

            var isFirstCategory = true;

            foreach (var category in categories)
            {
                var textHeader = category.Key;
                if (string.IsNullOrEmpty(textHeader))
                {
                    textHeader = TitleGeneralControlsCategory;
                }

                // add category header
                var marginTop = isFirstCategory ? 0 : 8;
                isFirstCategory = false;
                var categoryHeader = new TextBlock()
                {
                    Text = textHeader,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, marginTop, 0, 5)
                };

                Grid.SetColumnSpan(categoryHeader, 3);
                tableControl.Add(categoryHeader, null);

                // add keys for this category
                foreach (var pair in category)
                {
                    var button = pair.Key;
                    var info = pair.Value;

                    var labelControl = new FormattedTextBlock
                    {
                        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                        Content = info.Title,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                        LineHeight = 13,
                        LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                        MaxWidth = 150,
                        Margin = new Thickness(0, 0, 0, 6),
                        TextAlignment = TextAlignment.Right,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    //ToolTipServiceExtend.SetToolTip(
                    //    labelControl,
                    //    new TextBlock()
                    //    {
                    //        Text = info.Description ?? info.Name,
                    //        TextWrapping = TextWrapping.Wrap,
                    //        MaxWidth = 200,
                    //        VerticalAlignment = VerticalAlignment.Center
                    //    });

                    var inputMappingControl = GetInputMappingControl(button, info);
                    tableControl.Add(labelControl, inputMappingControl);
                }
            }
        }

        private void SaveMapping()
        {
            this.savedMapping = ClientInputManager.CloneMapping();
        }
    }
}