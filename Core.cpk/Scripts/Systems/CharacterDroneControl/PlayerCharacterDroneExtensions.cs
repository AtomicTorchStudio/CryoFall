namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDroneControl
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;

    public static class PlayerCharacterDroneExtensions
    {
        public static int SharedGetCurrentControlledDronesNumber(this ICharacter character)
        {
            return SharedGetCurrentlyControlledDrones(character).Count;
        }

        public static NetworkSyncList<IDynamicWorldObject> SharedGetCurrentlyControlledDrones(
            ILogicObject droneController)
        {
            return CharacterDroneController.GetPublicState(droneController)
                                           .CurrentlyControlledDrones;
        }

        public static NetworkSyncList<IDynamicWorldObject> SharedGetCurrentlyControlledDrones(
            this ICharacter character)
        {
            var droneController = character.SharedGetDroneController();
            return SharedGetCurrentlyControlledDrones(droneController);
        }

        public static ILogicObject SharedGetDroneController(this ICharacter character)
        {
            return PlayerCharacter.GetPrivateState(character)
                                  .DroneController;
        }
    }
}