namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Crates.Data
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelCrateIconControl : BaseViewModel
    {
        private static readonly ITextureResource TextureResourcePlaceholderIcon
            = new TextureResource("StaticObjects/Structures/Crates/ObjectCrate_PlateIcon");

        private readonly ObjectCratePublicState publicState;

        private readonly IStaticWorldObject worldObjectCrate;

        public ViewModelCrateIconControl(IStaticWorldObject worldObjectCrate)
        {
            this.worldObjectCrate = worldObjectCrate;
            this.publicState = worldObjectCrate.GetPublicState<ObjectCratePublicState>();
            this.publicState.ClientSubscribe(_ => _.IconSource,
                                             _ =>
                                             {
                                                 this.NotifyPropertyChanged(nameof(this.IsIconAvailable));
                                                 this.NotifyPropertyChanged(nameof(this.Icon));
                                             },
                                             this);
        }

        public BaseCommand CommandOpenCrateIconSelectorWindow
            => new ActionCommand(this.ExecuteCommandOpenCrateIconSelectorWindow);

        public TextureBrush Icon
        {
            get
            {
                var icon = ClientCrateIconHelper.GetIcon(this.publicState.IconSource);
                return icon != null
                           ? Api.Client.UI.GetTextureBrush(icon)
                           : null;
            }
        }

        public TextureBrush IconPlaceholder
            => Api.Client.UI.GetTextureBrush(TextureResourcePlaceholderIcon);

        public bool IsIconAvailable
            => ClientCrateIconHelper.GetOriginalIcon(this.publicState.IconSource) != null;

        private void ExecuteCommandOpenCrateIconSelectorWindow()
        {
            var existingItems = this.worldObjectCrate.GetPrivateState<ObjectCratePrivateState>().ItemsContainer.Items
                                    .ToList();
            var window = new WindowCrateIconSelector(this.publicState.IconSource, existingItems);
            window.EventWindowClosing += WindowClosingHandler;
            Api.Client.UI.LayoutRootChildren.Add(window);

            void WindowClosingHandler()
            {
                window.EventWindowClosing -= WindowClosingHandler;
                if (window.DialogResult != DialogResult.OK)
                {
                    return;
                }

                var selectedIconSource = window.ViewModel.SelectedProtoEntity;
                var protoObjectCrate = (IProtoObjectCrate)this.worldObjectCrate.ProtoGameObject;
                protoObjectCrate.ClientSetIconSource(this.worldObjectCrate, selectedIconSource);
            }
        }
    }
}