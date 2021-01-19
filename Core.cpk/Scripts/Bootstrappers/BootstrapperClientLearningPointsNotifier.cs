namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.LearningPointsNotifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperClientLearningPointsNotifier : BaseBootstrapper
    {
        private static uint lastLearningPoints;

        private static PlayerCharacterTechnologies playerTechnologies;

        private static StateSubscriptionStorage stateSubscriptionStorage;

        public override void ClientInitialize()
        {
            BootstrapperClientGame.InitEndCallback += GameInitHandler;
            BootstrapperClientGame.ResetCallback += GameResetHandler;
        }

        private static void GameInitHandler(ICharacter character)
        {
            playerTechnologies = PlayerCharacter.GetPrivateState(character).Technologies;
            stateSubscriptionStorage = new StateSubscriptionStorage();
            lastLearningPoints = playerTechnologies.LearningPoints;

            playerTechnologies.ClientSubscribe(
                t => t.LearningPoints,
                LearningPointsChangedHandler,
                stateSubscriptionStorage);
        }

        private static void GameResetHandler()
        {
            stateSubscriptionStorage?.Dispose();
            stateSubscriptionStorage = null;
        }

        private static void LearningPointsChangedHandler()
        {
            var delta = playerTechnologies.LearningPoints - (long)lastLearningPoints;
            lastLearningPoints = playerTechnologies.LearningPoints;

            if (delta != 0)
            {
                HUDLearningPointsNotificationsPanelControl.Show(delta, lastLearningPoints);
            }
        }
    }
}