namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolWorldSize
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelEditorToolWorldSizeSettings : BaseViewModel
    {
        public ViewModelEditorToolWorldSizeSettings()
        {
            this.ExtensionSidesCollection = EnumHelper.EnumValuesToViewModel<Side>();
            this.ExtensionSelectedSide = this.ExtensionSidesCollection[0];

            var sizes = new List<ushort>
            {
                0,
                10,
                20,
                50,
                100,
                200,
                500
            };

            this.SizesCollection = sizes.Select(s => new EditorToolWorldSizeTileSize(s)).ToArray();
            this.ExtensionSelectedSize = this.SizesCollection[1];
            this.SelectedPaddingSize = this.SizesCollection[0];
        }

        public ViewModelEditorToolWorldSizeSettings(
            Action callbackApplyWorldSizeSideExpansion,
            Action callbackApplyWorldSizeOptimization,
            Action callbackApplyWorldSizeSliceExpansion)
            : this()
        {
            this.CommandApplyWorldSizeOptimization = new ActionCommand(callbackApplyWorldSizeOptimization);
            this.CommandApplyWorldSizeSideExpansion = new ActionCommand(callbackApplyWorldSizeSideExpansion);
            this.CommandApplyWorldSizeSliceExpansion = new ActionCommand(callbackApplyWorldSizeSliceExpansion);

            this.UpdateWorldBoundsText();
            Api.Client.World.WorldBoundsChanged += this.WorldBoundsChangedHandler;
        }

        public BaseCommand CommandApplyWorldSizeOptimization { get; }

        public BaseCommand CommandApplyWorldSizeSideExpansion { get; }

        public BaseCommand CommandApplyWorldSizeSliceExpansion { get; }

        public ViewModelEnum<Side> ExtensionSelectedSide { get; set; }

        public EditorToolWorldSizeTileSize ExtensionSelectedSize { get; set; }

        public ViewModelEnum<Side>[] ExtensionSidesCollection { get; }

        public EditorToolWorldSizeTileSize SelectedPaddingSize { get; set; }

        public EditorToolWorldSizeTileSize[] SizesCollection { get; }

        public ViewModelPositionAndSizeSettings ViewModelSelectWorldSizeSliceExpansionLocation { get; }
            = new();

        public string WorldOffsetText { get; set; } = FormatPosition(new Vector2Ushort(10000, 10000));

        public string WorldSizeText { get; set; } = FormatPosition(new Vector2Ushort(100, 100));

        public void UpdateWorldBoundsText()
        {
            var bounds = Api.Client.World.WorldBounds;
            this.WorldOffsetText = FormatPosition(bounds.Offset);
            this.WorldSizeText = FormatPosition(bounds.Size);
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            Api.Client.World.WorldBoundsChanged -= this.WorldBoundsChangedHandler;
        }

        private static string FormatPosition(Vector2Ushort position)
        {
            return $"X={position.X} Y={position.Y}";
        }

        private void WorldBoundsChangedHandler()
        {
            this.UpdateWorldBoundsText();
        }
    }
}