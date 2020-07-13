namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip
{
    using System.Windows;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ConstructionRelocationTooltip : BaseUserControl
    {
        public static readonly DependencyProperty CanInteractProperty =
            DependencyProperty.Register(nameof(CanInteract),
                                        typeof(bool),
                                        typeof(ConstructionRelocationTooltip),
                                        new PropertyMetadata(default(bool)));

        public bool CanInteract
        {
            get => (bool)this.GetValue(CanInteractProperty);
            set => this.SetValue(CanInteractProperty, value);
        }

        public IStaticWorldObject WorldObject { get; private set; }

        public static IComponentAttachedControl CreateAndAttach(IStaticWorldObject worldObject)
        {
            var control = new ConstructionRelocationTooltip();
            control.WorldObject = worldObject;

            var positionOffset = worldObject.ProtoStaticWorldObject.SharedGetObjectCenterWorldOffset(worldObject);
            positionOffset += (0, 1.125);

            return Api.Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: positionOffset,
                isFocusable: true);
        }

        protected override void OnLoaded()
        {
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.Update();
        }

        protected override void OnUnloaded()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private void Update()
        {
            this.CanInteract = this.WorldObject.ProtoWorldObject.SharedIsInsideCharacterInteractionArea(
                Api.Client.Characters.CurrentPlayerCharacter,
                this.WorldObject,
                writeToLog: false);
        }
    }
}