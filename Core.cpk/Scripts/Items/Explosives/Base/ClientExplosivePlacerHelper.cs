namespace AtomicTorch.CBND.CoreMod.Items.Explosives
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.ItemExplosive;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientExplosivePlacerHelper : ClientComponent
    {
        private static ClientComponentObjectPlacementHelper blueprintComponent;

        private static IItem currentSelectedExplosiveItem;

        private static IProtoItemExplosive currentSelectedProtoExplosive;

        public static void Setup(IItem item, bool isSelected)
        {
            if (!isSelected)
            {
                item = null;
            }

            currentSelectedExplosiveItem = item;
            currentSelectedProtoExplosive = currentSelectedExplosiveItem?.ProtoItem as IProtoItemExplosive;
            if (currentSelectedProtoExplosive is null)
            {
                // explosive is not selected anymore
                blueprintComponent?.SceneObject.Destroy();
                blueprintComponent = null;
                return;
            }

            // explosive is selected - create blueprint component
            blueprintComponent ??= Client.Scene.CreateSceneObject("Explosive placer helper")
                                         .AddComponent<ClientComponentObjectPlacementHelper>();

            blueprintComponent.Setup(
                protoStaticWorldObject: currentSelectedProtoExplosive.ObjectExplosiveProto,
                isCancelable: false,
                isRepeatCallbackIfHeld: false,
                isDrawConstructionGrid: true,
                isBlockingInput: false,
                validateCanPlaceCallback: OnValidate,
                placeSelectedCallback: OnPlaceSelected);
        }

        private static void OnPlaceSelected(Vector2Ushort tilePosition)
        {
            if (ClientItemsManager.ItemInHand is not null)
            {
                return;
            }

            ItemExplosiveSystem.Instance.ClientTryStartAction(tilePosition);

            var privateState = ClientCurrentCharacterHelper.PrivateState;
            if (privateState.CurrentActionState is not null)
            {
                blueprintComponent.IsFrozen = true;
            }
        }

        private static void OnValidate(
            Vector2Ushort tilePosition,
            bool logErrors,
            out string errorMessage,
            out bool canPlace,
            out bool isTooFar)
        {
            currentSelectedProtoExplosive.SharedValidatePlacement(
                Client.Characters.CurrentPlayerCharacter,
                tilePosition,
                logErrors: logErrors,
                canPlace: out canPlace,
                isTooFar: out isTooFar,
                errorMessage: out errorMessage);
        }
    }
}