namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientSeedPlacerHelper
    {
        public const double MaxSeedPlacementDistance = 2.5; // allow to place in the same or neighbor tiles

        private static ClientComponentObjectPlacementHelper blueprintComponent;

        private static IProtoItemSeed currentSelectedProtoSeed;

        private static IItem currentSelectedSeedItem;

        public static void Setup(IItem item, bool isSelected)
        {
            if (!isSelected)
            {
                item = null;
            }

            if (currentSelectedSeedItem == item)
            {
                return;
            }

            currentSelectedSeedItem = item;
            currentSelectedProtoSeed = currentSelectedSeedItem?.ProtoItem as IProtoItemSeed;
            if (currentSelectedProtoSeed is null)
            {
                // seed is not selected anymore
                blueprintComponent?.SceneObject.Destroy();
                blueprintComponent = null;
                return;
            }

            // seed is selected - create blueprint component
            blueprintComponent ??= Api.Client.Scene.CreateSceneObject("Seed placer helper")
                                      .AddComponent<ClientComponentObjectPlacementHelper>();

            blueprintComponent.Setup(
                protoStaticWorldObject: currentSelectedProtoSeed.ObjectPlantProto,
                isCancelable: false,
                isRepeatCallbackIfHeld: true,
                isDrawConstructionGrid: true,
                isBlockingInput: false,
                validateCanPlaceCallback: OnValidate,
                placeSelectedCallback: OnPlaceSelected);
        }

        private static void OnPlaceSelected(Vector2Ushort tilePosition)
        {
            currentSelectedProtoSeed.ClientPlaceAt(currentSelectedSeedItem, tilePosition);
        }

        private static void OnValidate(
            Vector2Ushort tilePosition,
            bool logErrors,
            out bool canPlace,
            out bool isTooFar)
        {
            currentSelectedProtoSeed.SharedIsValidPlacementPosition(
                tilePosition,
                Api.Client.Characters.CurrentPlayerCharacter,
                logErrors: logErrors,
                canPlace: out canPlace,
                isTooFar: out isTooFar);
        }
    }
}