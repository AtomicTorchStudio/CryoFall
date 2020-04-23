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

    public class ViewModelWindowTinkerTable : BaseViewModel
    {
        private readonly IClientItemsContainer containerInput;

        private readonly IClientItemsContainer containerOutput;

        private readonly SkillMaintenance skill = Api.GetProtoEntity<SkillMaintenance>();

        private readonly IStaticWorldObject tinkerTableObject;

        public ViewModelWindowTinkerTable(
            IStaticWorldObject tinkerTableObject,
            ObjectTinkerTable.PrivateState privateState)
        {
            this.tinkerTableObject = tinkerTableObject;

            this.containerInput = (IClientItemsContainer)privateState.ContainerInput;
            this.containerInput.ItemAdded += this.AnyContainerItemAddedHandler;
            this.containerInput.ItemRemoved += this.AnyContainerItemRemovedHandler;
            this.containerInput.ItemsReset += this.AnyContainerResetHandler;

            this.containerOutput = (IClientItemsContainer)privateState.ContainerOutput;
            this.containerOutput.ItemAdded += this.AnyContainerItemAddedHandler;
            this.containerOutput.ItemRemoved += this.AnyContainerItemRemovedHandler;
            this.containerOutput.ItemsReset += this.AnyContainerResetHandler;

            ClientComponentSkillsWatcher.SkillLevelChanged += this.CharacterSkillLevelChangedHandler;
            ClientCurrentCharacterFinalStatsHelper.FinalStatsCacheChanged += this.Refresh;

            // register containers exchange
            var character = ClientCurrentCharacterHelper.Character;
            ClientContainersExchangeManager.Register(
                this,
                this.containerOutput,
                allowedTargets: new[]
                {
                    character.SharedGetPlayerContainerInventory(),
                    character.SharedGetPlayerContainerHotbar()
                });

            ClientContainersExchangeManager.Register(
                this,
                this.containerInput,
                allowedTargets: new[]
                {
                    character.SharedGetPlayerContainerInventory(),
                    character.SharedGetPlayerContainerHotbar()
                });

            this.Refresh();
        }

        public BaseCommand CommandRepair => new ActionCommand(this.ExecuteCommandRepair);

        public IClientItemsContainer ContainerInput => this.containerInput;

        public IClientItemsContainer ContainerOutput => this.containerOutput;

        public string PercentInput1Text { get; private set; }

        public string PercentInput2Text { get; private set; }

        public string PercentOutputText { get; private set; }

        public string PercentSkillText { get; private set; }

        public IReadOnlyList<ProtoItemWithCount> RequiredRepairComponentItems
            => ObjectTinkerTable.RequiredRepairComponentItems;

        public string SkillDescription => this.skill.Description;

        public Brush SkillIcon
            => Api.Client.UI.GetTextureBrush(this.skill.Icon);

        public string SkillLevelText
            => string.Format(CoreStrings.WindowSkills_CurrentLevelFormat2,
                             ClientCurrentCharacterHelper.Character.SharedGetSkill(this.skill).Level);

        public string SkillName => this.skill.Name;

        protected override void DisposeViewModel()
        {
            this.containerInput.ItemAdded -= this.AnyContainerItemAddedHandler;
            this.containerInput.ItemRemoved -= this.AnyContainerItemRemovedHandler;
            this.containerInput.ItemsReset -= this.AnyContainerResetHandler;

            this.containerOutput.ItemAdded -= this.AnyContainerItemAddedHandler;
            this.containerOutput.ItemRemoved -= this.AnyContainerItemRemovedHandler;
            this.containerOutput.ItemsReset -= this.AnyContainerResetHandler;

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

            this.PercentSkillText = SkillMaintenance.SharedGetCurrentBonusPercent(character) + "%";

            var inputItem1 = this.containerInput.GetItemAtSlot(0);
            this.PercentInput1Text = GetDurabilityPercentText(inputItem1);

            var inputItem2 = this.containerInput.GetItemAtSlot(1);
            this.PercentInput2Text = GetDurabilityPercentText(inputItem2);

            var outputItem = this.containerOutput.GetItemAtSlot(0);
            if (outputItem != null)
            {
                this.PercentOutputText = string.Empty;
                return;
            }

            if (inputItem1 == null
                || inputItem2 == null)
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
                => item == null
                   || !(item.ProtoItem is IProtoItemWithDurability)
                       ? string.Empty
                       : ItemDurabilitySystem.SharedGetDurabilityPercent(item) + "%";
        }
    }
}