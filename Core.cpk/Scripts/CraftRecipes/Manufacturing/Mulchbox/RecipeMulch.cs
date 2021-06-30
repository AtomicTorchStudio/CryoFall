namespace AtomicTorch.CBND.CoreMod.CraftRecipes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.World;

    public sealed class RecipeMulch : Recipe.RecipeForManufacturing
    {
        private const ushort MulchOrganicValue = 10;

        public override bool IsAutoUnlocked => true;

        public override void ServerOnManufacturingCompleted(
            IStaticWorldObject objectMulchbox,
            CraftingQueue craftingQueue)
        {
            var privateState = GetPrivateState(objectMulchbox);
            var organicAmount = (int)privateState.OrganicAmount;

            // decrease remaining amount
            organicAmount -= MulchOrganicValue;

            if (organicAmount < 0)
            {
                // should be impossible
                Logger.Error(
                    $"Organic amount of mulchbox after crafting mulch become negative: {organicAmount}. {objectMulchbox}");
                organicAmount = 0;
            }

            privateState.OrganicAmount = (ushort)organicAmount;
        }

        protected override bool CanBeCrafted(
            IStaticWorldObject objectManufacturer,
            CraftingQueue craftingQueue,
            ushort countToCraft)
        {
            var privateState = GetPrivateState(objectManufacturer);
            return privateState.OrganicAmount >= MulchOrganicValue;
        }

        protected override void SetupRecipe(
            StationsList stations,
            out TimeSpan duration,
            InputItems inputItems,
            OutputItems outputItems)
        {
            stations.Add<ObjectMulchbox>();

            duration = CraftingDuration.VeryLong;

            outputItems.Add<ItemMulch>();
        }

        private static ObjectMulchboxPrivateState GetPrivateState(IWorldObject objectMulchbox)
        {
            return ((IProtoObjectMulchbox)objectMulchbox.ProtoWorldObject)
                .GetMulchboxPrivateState((IStaticWorldObject)objectMulchbox);
        }
    }
}