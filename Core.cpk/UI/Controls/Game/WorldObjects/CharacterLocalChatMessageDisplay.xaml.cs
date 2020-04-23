namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CharacterLocalChatMessageDisplay : BaseUserControl
    {
        private const double TimeoutSeconds = 6;

        private static readonly ConditionalWeakTable<ICharacter, CharacterLocalChatMessageDisplay> DisplayedMessages
            = new ConditionalWeakTable<ICharacter, CharacterLocalChatMessageDisplay>();

        private IComponentAttachedControl attachedComponent;

        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        public static void ShowOn(ICharacter character, string message)
        {
            if (!ClientChatDisclaimerConfirmationHelper.IsChatAllowedForCurrentServer)
            {
                return;
            }

            message = message.ToLowerInvariant();

            var positionOffset = (0,
                                  character.ProtoCharacter.CharacterWorldHeight + 0.25);

            if (DisplayedMessages.TryGetValue(character, out var control))
            {
                DisplayedMessages.Remove(character);

                if (control.isLoaded)
                {
                    // destroy already displayed message control
                    control.Hide(fast: true);
                    ClientTimersSystem.AddAction(
                        0.075,
                        () => ShowOn(character, message));
                    return;
                }
            }

            // create and setup new message control
            control = new CharacterLocalChatMessageDisplay();
            control.Setup(message);

            control.attachedComponent = Api.Client.UI.AttachControl(
                character,
                control,
                positionOffset: positionOffset,
                isScaleWithCameraZoom: false,
                isFocusable: false);

            ClientTimersSystem.AddAction(TimeoutSeconds, () => control.Hide(fast: false));
            DisplayedMessages.Add(character, control);
        }

        public void Setup(string message)
        {
            this.DataContext = message;
        }

        protected override void InitControl()
        {
            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
        }

        protected override void OnLoaded()
        {
            this.storyboardHide.Completed += this.StoryboardHideCompletedHandler;
            this.storyboardShow.Begin();
        }

        protected override void OnUnloaded()
        {
            this.storyboardHide.Completed -= this.StoryboardHideCompletedHandler;
        }

        private void Hide(bool fast)
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (fast)
            {
                // normal hide speed is 0.75 seconds so let's hide in 0.1 second
                this.storyboardHide.SpeedRatio = 7.5;
            }

            this.storyboardHide.Begin();
        }

        private void StoryboardHideCompletedHandler(object sender, EventArgs e)
        {
            this.attachedComponent.Destroy();
        }
    }
};