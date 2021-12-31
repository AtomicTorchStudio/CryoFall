namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TinkerTable.Data
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowTinkerTableTabRepair : BaseViewModel
    {
        private readonly ObjectTinkerTable.PrivateState privateState;

        private readonly SkillMaintenance skill = Api.GetProtoEntity<SkillMaintenance>();

        private readonly IStaticWorldObject tinkerTableObject;

        private bool isActive;

        private IProtoItem selectedProtoItem;

        public ViewModelWindowTinkerTableTabRepair(
            IStaticWorldObject tinkerTableObject,
            ObjectTinkerTable.PrivateState privateState)
        {
            this.privateState = privateState;
            this.tinkerTableObject = tinkerTableObject;

            ClientComponentSkillsWatcher.SkillLevelChanged += this.CharacterSkillLevelChangedHandler;
            ClientCurrentCharacterFinalStatsHelper.FinalStatsCacheChanged += this.Refresh;

            this.Refresh();
        }

        public BaseCommand CommandRepair => new ActionCommand(this.ExecuteCommandRepair);

        public IClientItemsContainer ContainerInput
            => (IClientItemsContainer)this.privateState.ContainerInput;

        public IClientItemsContainer ContainerOutput
            => (IClientItemsContainer)this.privateState.ContainerOutput;

        public bool HasOutputItem { get; private set; }

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

                var containerInput = (IClientItemsContainer)this.privateState.ContainerInput;
                var containerOutput = (IClientItemsContainer)this.privateState.ContainerOutput;

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
                }
                else
                {
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

        public string PercentInput1Text { get; private set; }

        public string PercentInput2Text { get; private set; }

        public string PercentOutputText { get; private set; }

        public string PercentSkillText { get; private set; }

        public IReadOnlyList<ProtoItemWithCount> RequiredRepairComponentItems
            => ObjectTinkerTable.RequiredRepairComponentItems;

        public IProtoItem SelectedProtoItem
        {
            get => this.selectedProtoItem;
            set
            {
                if (this.selectedProtoItem == value)
                {
                    return;
                }

                this.selectedProtoItem = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.SelectedProtoItemIcon));
            }
        }

        public Brush SelectedProtoItemIcon
            => this.selectedProtoItem is not null
                   ? Api.Client.UI.GetTextureBrush(this.selectedProtoItem.Icon)
                   : null;

        public string SkillDescription => this.skill.Description;

        public Brush SkillIcon
            => Api.Client.UI.GetTextureBrush(this.skill.Icon);

        public string SkillLevelText
            => string.Format(CoreStrings.WindowSkills_CurrentLevelFormat2,
                             ClientCurrentCharacterHelper.Character.SharedGetSkill(this.skill).Level);

        public string SkillName => this.skill.Name;

        public bool HasInputItem2 { get; private set; }
        
        public bool HasInputItem1 { get; private set; }

        protected override void DisposeViewModel()
        {
            this.IsActive = false;

            ClientComponentSkillsWatcher.SkillLevelChanged -= this.CharacterSkillLevelChangedHandler;
            ClientCurrentCharacterFinalStatsHelper.FinalStatsCacheChanged -= this.Refresh;

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

        private void CharacterSkillLevelChangedHandler(
            IProtoSkill protoSkill,
            SkillLevelData levelData)
        {
            this.NotifyPropertyChanged(nameof(this.SkillLevelText));
            this.Refresh();
        }

        private void ExecuteCommandRepair()
        {
            ObjectTinkerTable.ClientRepair(this.tinkerTableObject);
        }

        private void Refresh()
        {
            if (this.IsDisposed)
            {
                return;
            }

            var character = ClientCurrentCharacterHelper.Character;
            if (ClientCurrentCharacterFinalStatsHelper.FinalStatsCache.IsDirty)
            {
                // refresh will be called again automatically when the new final stats cache is assigned
                return;
            }

            this.PercentSkillText = "+" + SkillMaintenance.SharedGetCurrentBonusPercent(character) + "%";

            var inputItem1 = this.privateState.ContainerInput.GetItemAtSlot(0);
            this.PercentInput1Text = GetDurabilityPercentText(inputItem1);

            var inputItem2 = this.privateState.ContainerInput.GetItemAtSlot(1);
            this.PercentInput2Text = GetDurabilityPercentText(inputItem2);

            this.SelectedProtoItem = inputItem1?.ProtoItem ?? inputItem2?.ProtoItem;
            
            HasInputItem1 = inputItem1 is not null;
            HasInputItem2 = inputItem2 is not null;

            var outputItem = this.privateState.ContainerOutput.GetItemAtSlot(0);
            if (outputItem is not null)
            {
                this.HasOutputItem = true;
                this.PercentOutputText = string.Empty;
                return;
            }

            this.HasOutputItem = false;
            if (inputItem1 is null
                || inputItem2 is null)
            {
                this.PercentOutputText = "?";
            }
            else
            {
                var resultPercent = ObjectTinkerTable.SharedCalculateResultDurabilityFraction(inputItem1,
                    inputItem2,
                    character);
                this.PercentOutputText = (byte)(100 * resultPercent) + "%";
            }

            static string GetDurabilityPercentText(IItem item)
                => item?.ProtoItem is not IProtoItemWithDurability
                       ? string.Empty
                       : ItemDurabilitySystem.SharedGetDurabilityPercent(item) + "%";
        }
    }
}