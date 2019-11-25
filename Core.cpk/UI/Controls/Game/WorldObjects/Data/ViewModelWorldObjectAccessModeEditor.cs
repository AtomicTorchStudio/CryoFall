namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWorldObjectAccessModeEditor : BaseViewModel
    {
        private readonly IObjectWithAccessModePrivateState privateState;

        private readonly IWorldObject worldObject;

        private WorldObjectAccessMode selectedAccessMode;

        public ViewModelWorldObjectAccessModeEditor(IWorldObject worldObject, bool canSetAccessMode)
        {
            this.worldObject = worldObject;
            this.CanSetAccessMode = canSetAccessMode;

            if (!(worldObject.ProtoGameObject is IProtoObjectWithAccessMode protoObjectWithAccessMode))
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            var accessModes = Enum.GetValues(typeof(WorldObjectAccessMode))
                                  .Cast<WorldObjectAccessMode>();

            if (!protoObjectWithAccessMode.IsClosedAccessModeAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectAccessMode.Closed);
            }

            this.AccessModes = accessModes
                               .Select(e => new ViewModelEnum<WorldObjectAccessMode>(e))
                               .OrderBy(vm => vm.Order)
                               .ToArray();

            this.privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();

            this.privateState.ClientSubscribe(
                _ => _.AccessMode,
                _ => this.RefreshAccessMode(),
                this);

            this.RefreshAccessMode();
        }

        public ViewModelEnum<WorldObjectAccessMode>[] AccessModes { get; }

        public bool CanSetAccessMode { get; }

        public ViewModelEnum<WorldObjectAccessMode> SelectedAccessMode
        {
            get => new ViewModelEnum<WorldObjectAccessMode>(this.selectedAccessMode);
            set => this.SetSelectedAccessMode(value.Value, sendToServer: true);
        }

        private void RefreshAccessMode()
        {
            this.SetSelectedAccessMode(this.privateState.AccessMode,
                                       sendToServer: false);
        }

        private void SetSelectedAccessMode(WorldObjectAccessMode mode, bool sendToServer)
        {
            if (this.selectedAccessMode == mode)
            {
                return;
            }

            this.selectedAccessMode = mode;
            this.NotifyPropertyChanged(nameof(this.SelectedAccessMode));

            if (sendToServer)
            {
                WorldObjectAccessModeSystem.ClientSetMode(this.worldObject, mode);
            }
        }
    }
}