namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TinkerTable.Data
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowTinkerTableTabDisassembly : BaseViewModel
    {
        private readonly ObjectTinkerTable.PrivateState privateState;

        private readonly IStaticWorldObject tinkerTableObject;

        private ClientInputContext inputListener;

        private bool isActive;

        public ViewModelWindowTinkerTableTabDisassembly(
            IStaticWorldObject tinkerTableObject,
            ObjectTinkerTable.PrivateState privateState)
        {
            this.privateState = privateState;
            this.tinkerTableObject = tinkerTableObject;

            this.Refresh();
        }

        public BaseCommand CommandDisassemble => new ActionCommand(this.ExecuteCommandDisassemble);

        public BaseCommand CommandTakeAll
            => new ActionCommand(this.ExecuteCommandTakeAll);

        public IClientItemsContainer ContainerInput
            => (IClientItemsContainer)this.privateState.ContainerDisassemblyInput;

        public IClientItemsContainer ContainerOutput
            => (IClientItemsContainer)this.privateState.ContainerDisassemblyOutput;

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                var containerInput = (IClientItemsContainer)this.privateState.ContainerDisassemblyInput;
                var containerOutput = (IClientItemsContainer)this.privateState.ContainerDisassemblyOutput;

                if (this.isActive)
                {
                    containerInput.ItemAdded += this.AnyContainerItemAddedHandler;
                    containerInput.ItemRemoved += this.AnyContainerItemRemovedHandler;
                    containerInput.ItemsReset += this.AnyContainerResetHandler;

                    containerOutput.ItemAdded += this.AnyContainerItemAddedHandler;
                    containerOutput.ItemRemoved += this.AnyContainerItemRemovedHandler;
                    containerOutput.ItemsReset += this.AnyContainerResetHandler;

                    // register containers exchange
                    var character = ClientCurrentCharacterHelper.Character;
                    ClientContainersExchangeManager.Register(
                        this,
                        containerOutput,
                        allowedTargets: new[]
                        {
                            character.SharedGetPlayerContainerInventory(),
                            character.SharedGetPlayerContainerHotbar()
                        });

                    ClientContainersExchangeManager.Register(
                        this,
                        containerInput,
                        allowedTargets: new[]
                        {
                            character.SharedGetPlayerContainerInventory(),
                            character.SharedGetPlayerContainerHotbar()
                        });

                    // setup shortcuts
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    this.inputListener = ClientInputContext
                                         .Start("Tinker table disassembly shortcuts")
                                         .HandleButtonDown(GameButton.ContainerTakeAll,
                                                           this.ExecuteCommandTakeAll);
                }
                else
                {
                    this.inputListener.Stop();
                    this.inputListener = null;

                    ClientContainersExchangeManager.Unregister(this);

                    containerInput.ItemAdded -= this.AnyContainerItemAddedHandler;
                    containerInput.ItemRemoved -= this.AnyContainerItemRemovedHandler;
                    containerInput.ItemsReset -= this.AnyContainerResetHandler;

                    containerOutput.ItemAdded -= this.AnyContainerItemAddedHandler;
                    containerOutput.ItemRemoved -= this.AnyContainerItemRemovedHandler;
                    containerOutput.ItemsReset -= this.AnyContainerResetHandler;
                }

                this.NotifyThisPropertyChanged();
            }
        }

        protected override void DisposeViewModel()
        {
            this.IsActive = false;
            base.DisposeViewModel();
        }

        private void AnyContainerItemAddedHandler(IItem item)
        {
            this.Refresh();
        }

        private void AnyContainerItemRemovedHandler(IItem item, byte slotid)
        {
            this.Refresh();
        }

        private void AnyContainerResetHandler()
        {
            this.Refresh();
        }

        private void ExecuteCommandDisassemble()
        {
            ObjectTinkerTable.ClientDisassemble(this.tinkerTableObject);
        }

        private void ExecuteCommandTakeAll()
        {
            var character = ClientCurrentCharacterHelper.Character;
            character.ProtoCharacter.ClientTryTakeAllItems(character,
                                                           this.ContainerOutput,
                                                           showNotificationIfInventoryFull: true);
        }

        private void Refresh()
        {
            if (this.IsDisposed)
            {
                return;
            }
        }
    }
}