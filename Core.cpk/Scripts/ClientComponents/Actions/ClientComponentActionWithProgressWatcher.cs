namespace AtomicTorch.CBND.CoreMod.ClientComponents.Actions
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentActionWithProgressWatcher : ClientComponent
    {
        private static ClientComponentActionWithProgressWatcher instance;

        private IComponentAttachedControl componentAttachedUIElement;

        private PlayerCharacterPrivateState privateState;

        public static IWorldObject CurrentInteractionOverWorldObject =>
            instance?.privateState.CurrentActionState?.TargetWorldObject;

        public void Setup(PlayerCharacterPrivateState setPrivateState)
        {
            this.privateState = setPrivateState;
            this.Setup();
        }

        public override void Update(double deltaTime)
        {
            // no-op
        }

        protected override void OnDisable()
        {
            this.ReleaseSubscriptions();

            if (ReferenceEquals(this, instance))
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            this.Setup();
            instance = this;
        }

        private void CurrentActionStateChanged(object obj)
        {
            this.RefreshAttachedComponent();
        }

        private void RefreshAttachedComponent()
        {
            this.componentAttachedUIElement?.Destroy();
            this.componentAttachedUIElement = null;

            var actionState = this.privateState.CurrentActionState;
            if (actionState is null
                || !actionState.IsDisplayingProgress)
            {
                return;
            }

            var targetWorldObject = actionState.TargetWorldObject;
            if (targetWorldObject is null)
            {
                // display over the character
                targetWorldObject = (IWorldObject)this.privateState.GameObject;
            }

            Vector2D offset = (0, 1.01);
            switch (targetWorldObject.ProtoWorldObject)
            {
                case IProtoStaticWorldObject protoStaticWorldObject:
                    offset += protoStaticWorldObject.SharedGetObjectCenterWorldOffset(targetWorldObject);
                    break;

                case IProtoCharacter protoCharacter:
                    offset += protoCharacter.SharedGetObjectCenterWorldOffset(targetWorldObject);
                    break;
            }

            this.componentAttachedUIElement = Client.UI.AttachControl(
                targetWorldObject,
                new ActionProgressControl()
                {
                    ActionState = actionState
                },
                positionOffset: offset,
                isFocusable: false);

            CannotInteractMessageDisplay.Hide();
            InteractionTooltip.Hide();
        }

        private void Setup()
        {
            if (this.privateState is null)
            {
                return;
            }

            this.ReleaseSubscriptions();
            this.privateState.ClientSubscribe(
                _ => _.CurrentActionState,
                this.CurrentActionStateChanged,
                this);

            this.RefreshAttachedComponent();
        }
    }
}