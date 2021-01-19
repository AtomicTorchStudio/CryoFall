namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class ViewModelTechRequiredItemsControl : BaseViewModel
    {
        private static readonly SolidColorBrush BrushLockedForeground =
            new(Api.Client.UI.GetApplicationResource<Color>("ColorRed6").WithAlpha(204));

        public ViewModelTechRequiredItemsControl(
            IReadOnlyList<IProtoItem> requiredProtoItems,
            [CanBeNull] TechNode techNode)
        {
            var count = requiredProtoItems.Count;
            this.Visibility = count > 0
                                  ? Visibility.Visible
                                  : Visibility.Collapsed;

            if (count == 0)
            {
                return;
            }

            var array = new DataEntryRequiredItemProto[count];

            for (var index = 0; index < count; index++)
            {
                var protoItem = requiredProtoItems[index];
                array[index] = new DataEntryRequiredItemProto(protoItem,
                                                              techNode,
                                                              hasNextEntry: index < count - 1);
            }

            this.RequiredProtoItems = array;
        }

        public IReadOnlyList<DataEntryRequiredItemProto> RequiredProtoItems { get; }

        public Visibility Visibility { get; }

        public IEnumerable<Inline> CreateRequiredProtoItemsInlines()
        {
            foreach (var entry in this.RequiredProtoItems)
            {
                var isAvailabe = entry.IsTechAvailable;
                var text = entry.Text;
                var run = new Run(text);
                if (!isAvailabe)
                {
                    run.Foreground = BrushLockedForeground;
                }

                yield return run;
            }
        }
    }
}