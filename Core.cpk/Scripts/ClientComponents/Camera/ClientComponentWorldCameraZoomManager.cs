namespace AtomicTorch.CBND.CoreMod.ClientComponents.Camera
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Please note that we're using exponential zoom formula.
    /// It allows us to linearly zoom-in/out and smoothly interpolate zoom logarithmic
    /// value and then convert it to the actual camera zoom value with Math.Exp.
    /// </summary>
    public class ClientComponentWorldCameraZoomManager : ClientComponent
    {
        public const double ZoomAnimationSpeed = 6;

        private const double KeyPressRepeatInterval = 0.15;

        private const double StorageLastZoomValueSaveDelay = 1;

        private static readonly ICoreClientService Core = Api.Client.Core;

        private static readonly IInputClientService Input = Api.Client.Input;

        private static readonly IRenderingClientService Rendering = Client.Rendering;

        private static readonly IClientStorage StorageLastZoomValue
            = Client.Storage.GetStorage("Gameplay/CameraLastZoomValue");

        private static readonly Interval<double> ZoomDefaultBounds
            = new Interval<double>(0.5, 1.0);

        private static readonly Interval<double> ZoomDefaultBoundsLog
            = new Interval<double>(Math.Log(ZoomDefaultBounds.Min), Math.Log(ZoomDefaultBounds.Max));

        private static readonly double ZoomDefaultValueLog
            = (ZoomDefaultBoundsLog.Max + ZoomDefaultBoundsLog.Min) / 2;

        private static readonly double ZoomStepSizeLog
            = (ZoomDefaultBoundsLog.Max - ZoomDefaultBoundsLog.Min) / 8;

        private static ClientComponentWorldCameraZoomManager instance;

        private ClientInputContext clientInputContext;

        private double currentZoomLog;

        private int lastButtonDelta;

        private double lastButtonPressTime;

        private double? saveZoomValueAfterTime;

        // all zoom values are stored as logarithmic
        private double targetZoomLog;

        private Interval<double> zoomBoundsLinear;

        private Interval<double> zoomBoundsLog;

        public ClientComponentWorldCameraZoomManager()
            : base(isLateUpdateEnabled: false)
        {
            if (instance != null)
            {
                throw new Exception("Instance already exist");
            }

            if (!StorageLastZoomValue.TryLoad(out double zoomValue))
            {
                zoomValue = ZoomDefaultValueLog;
            }

            this.targetZoomLog = this.currentZoomLog = zoomValue;
            this.ZoomBounds = ZoomDefaultBounds;
        }

        public static ClientComponentWorldCameraZoomManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Client.Scene.CreateSceneObject(nameof(ClientComponentWorldCameraZoomManager))
                                     .AddComponent<ClientComponentWorldCameraZoomManager>();
                }

                return instance;
            }
        }

        public static bool IsProcessingMouseWheel { get; set; }

        public Interval<double> ZoomBounds
        {
            get => this.zoomBoundsLinear;
            set
            {
                this.zoomBoundsLinear = value;
                this.zoomBoundsLog = new Interval<double>(Math.Log(value.Min), Math.Log(value.Max));
                var oldTargetZoomLog = this.targetZoomLog;
                this.ClampTargetZoom();

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (oldTargetZoomLog != this.targetZoomLog)
                {
                    // force the current zoom to match the target zoom as the target zoom changed during the clamp call
                    this.currentZoomLog = this.targetZoomLog;
                }
            }
        }

        public void Update()
        {
            if (Math.Abs(this.currentZoomLog - this.targetZoomLog)
                > 0.001f)
            {
                // interpolate current zoom value
                this.currentZoomLog = MathHelper.Lerp(this.currentZoomLog,
                                                      this.targetZoomLog,
                                                      ZoomAnimationSpeed * Core.DeltaTime);
            }
            else
            {
                // delta is too small - snap to the target zoom value
                this.currentZoomLog = this.targetZoomLog;
            }

            this.ClampTargetZoom();
            Rendering.WorldCameraZoom = (float)Math.Exp(this.currentZoomLog);

            if (!this.saveZoomValueAfterTime.HasValue
                || Core.ClientRealTime < this.saveZoomValueAfterTime)
            {
                return;
            }

            this.saveZoomValueAfterTime = null;
            StorageLastZoomValue.Save(this.targetZoomLog, writeToLog: false);
            //Logger.Dev("Zoom value saved to storage: " + this.targetZoomLog);
        }

        protected override void OnDisable()
        {
            Rendering.WorldCameraUpdateCallback -= this.Update;
            this.clientInputContext.Stop();
        }

        protected override void OnEnable()
        {
            Rendering.WorldCameraUpdateCallback += this.Update;
            this.clientInputContext =
                ClientInputContext.Start(nameof(ClientComponentWorldCameraZoomManager))
                                  .HandleAll(() =>
                                             {
                                                 if (IsProcessingMouseWheel
                                                     && WindowsManager.OpenedWindowsCount == 0)
                                                 {
                                                     this.ApplyZoomDelta(Input.MouseScrollDeltaValue);
                                                 }

                                                 var delta = 0;
                                                 if (ClientInputManager.IsButtonHeld(GameButton.CameraZoomIn))
                                                 {
                                                     delta = -1;
                                                 }

                                                 if (ClientInputManager.IsButtonHeld(GameButton.CameraZoomOut))
                                                 {
                                                     delta = 1;
                                                 }

                                                 var time = Core.ClientRealTime;
                                                 if (this.lastButtonDelta == delta
                                                     && time <= this.lastButtonPressTime + KeyPressRepeatInterval)
                                                 {
                                                     return;
                                                 }

                                                 this.lastButtonDelta = delta;
                                                 this.lastButtonPressTime = time;
                                                 this.ApplyZoomDelta(delta);
                                             });
        }

        private void ApplyZoomDelta(double delta)
        {
            if (delta == 0)
            {
                return;
            }

            delta *= ZoomStepSizeLog;
            this.targetZoomLog -= delta;
            this.ClampTargetZoom();

            this.saveZoomValueAfterTime = Core.ClientRealTime + StorageLastZoomValueSaveDelay;
            //Logger.Dev("Camera zoom changed to: " + Math.Exp(this.targetZoomLog).ToString("F4"));
        }

        private void ClampTargetZoom()
        {
            this.targetZoomLog = MathHelper.Clamp(this.targetZoomLog,
                                                  this.zoomBoundsLog.Min,
                                                  this.zoomBoundsLog.Max);
        }
    }
}