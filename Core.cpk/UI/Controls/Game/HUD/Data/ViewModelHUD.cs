namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelHUD : BaseViewModel
    {
        public ViewModelHUD()
        {
            this.Stats = new ViewModelHUDStats();

            if (IsDesignTime)
            {
                return;
            }

            this.MenuConstruction = Menu.Register(ConstructionPlacementSystem.Instance);

            this.MenuCrafting = Menu.Register<WindowHandCrafting>();
            this.MenuInventory = Menu.Register<WindowInventory>();
            this.MenuMap = Menu.Register<WindowWorldMap>();
            this.MenuSkills = Menu.Register<WindowSkills>();
            this.MenuSocial = Menu.Register<WindowSocial>();
            this.MenuPolitics = Menu.Register<WindowPolitics>();
            this.MenuTechTree = Menu.Register<WindowTechnologies>();
            this.MenuQuests = Menu.Register<WindowQuests>();
            this.MenuCompletionist = Menu.Register<WindowCompletionist>();

            ClientCurrentCharacterHelper.PublicState
                                        .ClientSubscribe(_ => _.CurrentVehicle,
                                                         _ => this.RefreshVehicleUI(),
                                                         this);

            this.RefreshVehicleUI();
        }

        public BaseCommand CommandQuitVehicle
            => new ActionCommand(ExecuteCommandQuitVehicle);

        public bool IsConstructionMenuAvailable { get; private set; }

        public bool IsPlayersHotbarVisible { get; private set; }

        public bool IsQuitVehicleButtonVisible { get; private set; }

        public Menu MenuCompletionist { get; }

        public Menu MenuConstruction { get; }

        public Menu MenuCrafting { get; }

        public Menu MenuInventory { get; }

        public Menu MenuMap { get; }

        public Menu MenuPolitics { get; }

        public Menu MenuQuests { get; }

        public Menu MenuSkills { get; }

        public Menu MenuSocial { get; }

        public Menu MenuTechTree { get; }

        public ViewModelHUDStats Stats { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            this.MenuConstruction.Dispose();
            this.MenuCrafting.Dispose();
            this.MenuInventory.Dispose();
            this.MenuMap.Dispose();
            this.MenuSkills.Dispose();
            this.MenuSocial.Dispose();
            this.MenuPolitics.Dispose();
            this.MenuTechTree.Dispose();
            this.MenuQuests.Dispose();
        }

        private static void ExecuteCommandQuitVehicle()
        {
            VehicleSystem.ClientOnVehicleEnterOrExitRequest();
        }

        private void RefreshVehicleUI()
        {
            var currentVehicle = ClientCurrentCharacterHelper.PublicState.CurrentVehicle;
            if (currentVehicle is not null
                && (!currentVehicle.IsInitialized
                    || !currentVehicle.ClientHasPrivateState))
            {
                // not yet ready - refresh after delay
                ClientTimersSystem.AddAction(delaySeconds: 0.1,
                                             this.RefreshVehicleUI);
                currentVehicle = null;
            }

            this.IsQuitVehicleButtonVisible = currentVehicle is not null;
            this.IsConstructionMenuAvailable = currentVehicle is null;
            this.IsPlayersHotbarVisible = currentVehicle is null
                                          || ((IProtoVehicle)currentVehicle.ProtoGameObject)
                                          .IsPlayersHotbarAndEquipmentItemsAllowed;
        }
    }
}