namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public static class PlayerCharacterItemsExtensions
    {
        public static void ClientInvalidateSkeletonRenderer(this ICharacter character)
        {
            if (!character.IsInitialized)
            {
                return;
            }

            var clientState = character.GetClientState<BaseCharacterClientState>();
            clientState.LastEquipmentContainerHash = null;
        }

        /// <summary>
        /// Gets player character equipment container. Please invoke only for the player characters. It's safe to invoke on the
        /// Client-side.
        /// </summary>
        public static IItemsContainer SharedGetPlayerContainerEquipment(this ICharacter character)
        {
            return PlayerCharacter.GetPublicState(character).ContainerEquipment;
        }

        /// <summary>
        /// Gets player character hand container. Please invoke only for the player characters. On the Client-side you can invoke
        /// it only for your character.
        /// </summary>
        public static IItemsContainer SharedGetPlayerContainerHand(this ICharacter character)
        {
            return GetPrivateState(character).ContainerHand;
        }

        /// <summary>
        /// Gets player character hotbar container. Please invoke only for the player characters. On the Client-side you can invoke
        /// it only for your character.
        /// </summary>
        public static IItemsContainer SharedGetPlayerContainerHotbar(this ICharacter character)
        {
            return GetPrivateState(character).ContainerHotbar;
        }

        /// <summary>
        /// Gets player character inventory container. Please invoke only for the player characters. On the Client-side you can
        /// invoke it only for your character.
        /// </summary>
        public static IItemsContainer SharedGetPlayerContainerInventory(this ICharacter character)
        {
            return GetPrivateState(character).ContainerInventory;
        }

        /// <summary>
        /// Gets player character hand container item.
        /// On the Server-side you can invoke it for the player characters.
        /// On the Client-side you can invoke it only for your character.
        /// </summary>
        public static IItem SharedGetPlayerItemInHandContainer(this ICharacter character)
        {
            return GetPrivateState(character).ContainerHand.GetItemAtSlot(0);
        }

        /// <summary>
        /// Gets player character hand container item.
        /// On the Server-side you can invoke it for the player characters.
        /// On the Client-side you can invoke it only for your character.
        /// </summary>
        public static IItem SharedGetPlayerSelectedHotbarItem(this ICharacter character)
        {
            return GetPublicState(character).SelectedItem;
        }

        /// <summary>
        /// Gets player character hand container item.
        /// Can be invoked for every player on both client and server.
        /// </summary>
        public static IProtoItem SharedGetPlayerSelectedHotbarItemProto(this ICharacter character)
        {
            return SharedGetPlayerSelectedHotbarItem(character)?.ProtoItem;
        }

        private static PlayerCharacterPrivateState GetPrivateState(ICharacter character)
        {
            return PlayerCharacter.GetPrivateState(character);
        }

        private static PlayerCharacterPublicState GetPublicState(ICharacter character)
        {
            return PlayerCharacter.GetPublicState(character);
        }
    }
}