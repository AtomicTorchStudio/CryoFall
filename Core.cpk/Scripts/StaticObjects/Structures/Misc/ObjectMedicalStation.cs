namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.MedicalStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.MedicalStations.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectMedicalStation : ProtoObjectStructure, IInteractableProtoWorldObject
    {
        private static readonly Lazy<ItemVialBiomaterial> ProtoItemBiomaterialVial
            = new Lazy<ItemVialBiomaterial>(GetProtoEntity<ItemVialBiomaterial>);

        private static ObjectMedicalStation instance;

        public override string Description =>
            "Special automated medical station that can be used to install or remove all types of cybernetic implants.";

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public override string Name => "Medical station";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 10000;

        public static void ClientInstall(IItem itemToInstall, byte slotId)
        {
            instance.CallServer(_ => _.ServerRemote_Install(itemToInstall, slotId));
        }

        public static void ClientUninstall(byte slotId)
        {
            instance.CallServer(_ => _.ServerRemote_Uninstall(slotId));
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            return this.ClientOpenUI(new ClientObjectData((IStaticWorldObject)worldObject));
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowMedicalStation.Open(
                new ViewModelWindowMedicalStation());
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1.8;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 10);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 10);
            build.AddStageRequiredItem<ItemIngotLithium>(count: 5);
            build.AddStageRequiredItem<ItemPlastic>(count: 5);
            build.AddStageRequiredItem<ItemComponentsHighTech>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 5);
            repair.AddStageRequiredItem<ItemPlastic>(count: 2);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();
            instance = this;
        }

        protected override void SharedCreatePhysics(
            CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.6, 1.9), offset: (0.2, 0.1))
                .AddShapeRectangle(size: (2.6, 2.1), offset: (0.2, 0.1),   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (2.5, 1.2), offset: (0.25, 0.95), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (2.6, 2.5), offset: (0.2, 0.1),   group: CollisionGroups.ClickArea);
        }

        private void ServerRemote_Install(IItem itemToInstall, byte slotId)
        {
            var character = ServerRemoteContext.Character;
            var worldObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            this.VerifyGameObject((IStaticWorldObject)worldObject);

            var containerEquipment = character.SharedGetPlayerContainerEquipment();
            var currentInstalledItem = containerEquipment.GetItemAtSlot(slotId);
            if (currentInstalledItem != null)
            {
                if (currentInstalledItem == itemToInstall)
                {
                    Logger.Info("The implant is already installed");
                    return;
                }

                throw new Exception("Please uninstall installed implant");
            }

            if (itemToInstall.Container.OwnerAsCharacter != character
                || itemToInstall.Container == containerEquipment)
            {
                throw new Exception("The item to install must be in character containers (except equipment)");
            }

            if (!containerEquipment.ProtoItemsContainer.CanAddItem(
                    new CanAddItemContext(containerEquipment,
                                          itemToInstall,
                                          slotId,
                                          byCharacter: null,
                                          isExploratoryCheck: false)))
            {
                throw new Exception("Cannot install implant item there");
            }

            var itemToInstallProto = (IProtoItemEquipmentImplant)itemToInstall.ProtoItem;
            if (itemToInstallProto is ItemImplantBroken)
            {
                throw new Exception("Cannot install broken implant");
            }

            foreach (var equippedItem in containerEquipment.Items)
            {
                if (equippedItem.ProtoItem == itemToInstallProto)
                {
                    throw new Exception("Another implant of this type is already installed: " + itemToInstallProto);
                }
            }

            var biomaterialRequiredAmount = CreativeModeSystem.SharedIsInCreativeMode(character)
                                                ? (ushort)0
                                                : itemToInstallProto.BiomaterialAmountRequiredToInstall;

            if (!character.ContainsItemsOfType(ProtoItemBiomaterialVial.Value,
                                               requiredCount: biomaterialRequiredAmount))
            {
                throw new Exception("Not enough biomaterial vials");
            }

            // move to an implant slot in the equipment container 
            // please note, we're not providing byCharacter arg here to allow this container action
            if (!Server.Items.MoveOrSwapItem(itemToInstall,
                                             containerEquipment,
                                             out _,
                                             slotId: slotId))
            {
                throw new Exception("Unknown error - cannot move implant item to the player equipment");
            }

            if (biomaterialRequiredAmount > 0)
            {
                // destroy vials
                Server.Items.DestroyItemsOfType(character,
                                                ProtoItemBiomaterialVial.Value,
                                                countToDestroy: biomaterialRequiredAmount,
                                                out _);

                NotificationSystem.ServerSendItemsNotification(
                    character,
                    ProtoItemBiomaterialVial.Value,
                    -biomaterialRequiredAmount);
            }

            character.ServerAddSkillExperience<SkillCyberneticAffinity>(
                SkillCyberneticAffinity.ExperienceAddedPerImplantInstalled);

            Logger.Info("Implant installed: " + itemToInstall);
        }

        private void ServerRemote_Uninstall(byte slotId)
        {
            var character = ServerRemoteContext.Character;
            var worldObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            this.VerifyGameObject((IStaticWorldObject)worldObject);

            var itemToUninstall = character.SharedGetPlayerContainerEquipment()
                                           .GetItemAtSlot(slotId);
            if (itemToUninstall == null)
            {
                throw new Exception("No implant installed");
            }

            var itemToUninstallProto = (IProtoItemEquipmentImplant)itemToUninstall.ProtoItem;
            var biomaterialRequiredAmount = CreativeModeSystem.SharedIsInCreativeMode(character)
                                                ? (ushort)0
                                                : itemToUninstallProto.BiomaterialAmountRequiredToUninstall;

            if (!character.ContainsItemsOfType(ProtoItemBiomaterialVial.Value,
                                               requiredCount: biomaterialRequiredAmount)
                && !CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                throw new Exception("Not enough biomaterial vials");
            }

            // move to inventory (please note, we're not providing byCharacter arg here to allow this container action)
            if (!Server.Items.MoveOrSwapItem(itemToUninstall,
                                             character.SharedGetPlayerContainerInventory(),
                                             out _))
            {
                NotificationSystem.ServerSendNotificationNoSpaceInInventory(character);
                return;
            }

            if (biomaterialRequiredAmount > 0)
            {
                // destroy vials
                Server.Items.DestroyItemsOfType(character,
                                                ProtoItemBiomaterialVial.Value,
                                                countToDestroy: biomaterialRequiredAmount,
                                                out _);

                NotificationSystem.ServerSendItemsNotification(
                    character,
                    ProtoItemBiomaterialVial.Value,
                    -biomaterialRequiredAmount);
            }

            if (itemToUninstallProto is ItemImplantBroken)
            {
                // broken implant destroys on uninstall
                Server.Items.DestroyItem(itemToUninstall);
                NotificationSystem.ServerSendItemsNotification(
                    character,
                    itemToUninstallProto,
                    -1);
            }

            character.ServerAddSkillExperience<SkillCyberneticAffinity>(
                SkillCyberneticAffinity.ExperienceAddedPerImplantUninstalled);

            Logger.Info("Implant uninstalled: " + itemToUninstall);
        }
    }
}