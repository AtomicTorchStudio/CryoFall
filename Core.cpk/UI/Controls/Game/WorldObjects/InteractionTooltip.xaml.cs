namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class InteractionTooltip : BaseUserControl
    {
        public static readonly DependencyProperty TextBrushProperty =
            DependencyProperty.Register("TextBrush",
                                        typeof(Brush),
                                        typeof(InteractionTooltip),
                                        new PropertyMetadata(default(Brush)));

        public static readonly Brush TextBrushCanInteract =
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xCC, 0x00));

        public static readonly Brush TextBrushCannotInteract =
            new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        private static bool? lastCanInteract;

        private static InteractionTooltip lastTooltip;

        private static IWorldObject lastWorldObject;

        private IComponentAttachedControl attachedControl;

        private ClientInputContext clientInputContext;

        public Brush TextBrush
        {
            get => (Brush)this.GetValue(TextBrushProperty);
            set => this.SetValue(TextBrushProperty, value);
        }

        public static void Hide()
        {
            HideInternal();
            lastWorldObject = null;
            lastCanInteract = null;
        }

        public static void ShowOn(IWorldObject worldObject, string message, bool canInteract)
        {
            if (lastWorldObject == worldObject
                && lastCanInteract == canInteract)
            {
                return;
            }

            CannotInteractMessageDisplay.Hide();

            HideInternal();
            lastWorldObject = worldObject;
            lastCanInteract = canInteract;

            if (string.IsNullOrEmpty(message)
                || worldObject == null)
            {
                return;
            }

            Vector2D positionOffset;
            switch (worldObject)
            {
                case ICharacter _:
                    goto default;

                case IStaticWorldObject _:
                case IDynamicWorldObject _:
                    positionOffset = worldObject.ProtoWorldObject.SharedGetObjectCenterWorldOffset(worldObject);
                    break;

                default:
                    positionOffset = (0, 0);
                    break;
            }

            positionOffset += (0, 1.025);

            lastTooltip = new InteractionTooltip();
            lastTooltip.Setup(message, canInteract);

            lastTooltip.attachedControl = Api.Client.UI.AttachControl(
                worldObject,
                lastTooltip,
                positionOffset: positionOffset,
                isFocusable: false);
        }

        public void Setup(string message, bool canInteract)
        {
            this.DataContext = message;
            this.TextBrush = canInteract
                                 ? TextBrushCanInteract
                                 : TextBrushCannotInteract;
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.clientInputContext =
                ClientInputContext.Start("InteractionTooltip hide on interaction")
                                  .HandleAll(
                                      () =>
                                      {
                                          if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem,
                                                                              evenIfHandled: true)
                                              || ClientInputManager.IsButtonDown(GameButton.ActionInteract,
                                                                                 evenIfHandled: true))
                                          {
                                              HideInternal();
                                          }
                                      });
        }

        protected override void OnUnloaded()
        {
            if (ReferenceEquals(lastTooltip, this))
            {
                lastTooltip = null;
            }

            this.clientInputContext.Stop();
            this.clientInputContext = null;
        }

        private static void HideInternal()
        {
            lastTooltip?.attachedControl?.Destroy();
            lastTooltip = null;
        }
    }
};