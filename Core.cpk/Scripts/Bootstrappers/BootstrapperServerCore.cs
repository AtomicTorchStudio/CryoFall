namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperServerCore : BaseBootstrapper
    {
        public override void ServerInitialize(IServerConfiguration serverConfiguration)
        {
            ServerPlayerSpawnManager.Setup(serverConfiguration);

            ServerOperatorSystem.Setup(serverConfiguration);

            serverConfiguration.SetupPlayerCharacterProto(
                Api.GetProtoEntity<PlayerCharacter>());

            serverConfiguration.SetupItemsContainerDefaultProto(
                Api.GetProtoEntity<ItemsContainerDefault>());
        }
    }
}