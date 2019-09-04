namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ViewModelCharacterUnstuckInfoControl : BaseViewModel
    {
        private readonly PlayerCharacterPublicState characterPublicState;

        private double lastTimeRemains;

        public ViewModelCharacterUnstuckInfoControl(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            this.characterPublicState = PlayerCharacter.GetPublicState(character);
            this.Update();
        }

        public string UnstuckInfoMessage { get; private set; }

        public Visibility VisibilityUnstuckInfo { get; private set; } = Visibility.Collapsed;

        private void Update()
        {
            if (this.IsDisposed)
            {
                return;
            }

            // schedule next update
            ClientTimersSystem.AddAction(1, this.Update);

            var timeRemains = this.characterPublicState.UnstuckExecutionTime
                              - Client.CurrentGame.ServerFrameTimeApproximated;
            timeRemains = Math.Max(timeRemains, 0);
            if (this.lastTimeRemains == timeRemains)
            {
                return;
            }

            this.lastTimeRemains = timeRemains;
            if (timeRemains > 0)
            {
                this.VisibilityUnstuckInfo = Visibility.Visible;
                this.UnstuckInfoMessage = string.Format(CoreStrings.UnstuckInFormat,
                                                        ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
            }
            else
            {
                this.VisibilityUnstuckInfo = Visibility.Collapsed;
                this.UnstuckInfoMessage = string.Empty;
            }
        }
    }
}