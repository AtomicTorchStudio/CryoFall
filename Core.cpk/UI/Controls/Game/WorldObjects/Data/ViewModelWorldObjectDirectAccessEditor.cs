namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWorldObjectDirectAccessEditor : BaseViewModel
    {
        private readonly IObjectWithAccessModePrivateState privateState;

        private readonly IStaticWorldObject worldObject;

        private WorldObjectDirectAccessMode selectedAccessMode;

        public ViewModelWorldObjectDirectAccessEditor(IStaticWorldObject worldObject, bool canSetAccessMode)
        {
            this.worldObject = worldObject;
            this.CanSetAccessMode = canSetAccessMode;

            if (!(worldObject.ProtoGameObject is IProtoObjectWithAccessMode protoObjectWithAccessMode))
            {
                throw new Exception("This world object doesn't have an access mode");
            }

            IEnumerable<WorldObjectDirectAccessMode> accessModes
                = EnumExtensions.GetValues<WorldObjectDirectAccessMode>();
            if (!protoObjectWithAccessMode.IsClosedAccessModeAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectDirectAccessMode.Closed);
            }

            if (!protoObjectWithAccessMode.IsEveryoneAccessModeAvailable)
            {
                accessModes = accessModes.ExceptOne(WorldObjectDirectAccessMode.OpensToEveryone);
            }

            this.AccessModes = accessModes
                               .Select(e => new ViewModelEnum<WorldObjectDirectAccessMode>(e))
                               .OrderBy(vm => vm.Order)
                               .ToArray();

            this.privateState = worldObject.GetPrivateState<IObjectWithAccessModePrivateState>();

            this.privateState.ClientSubscribe(
                _ => _.DirectAccessMode,
                _ => this.RefreshAccessMode(),
                this);

            this.RefreshAccessMode();
        }

        public ViewModelEnum<WorldObjectDirectAccessMode>[] AccessModes { get; }

        public bool CanSetAccessMode { get; }

        public ViewModelEnum<WorldObjectDirectAccessMode> SelectedAccessMode
        {
            get => new(this.selectedAccessMode);
            set => this.SetSelectedAccessMode(value.Value, sendToServer: true);
        }

        private void RefreshAccessMode()
        {
            this.SetSelectedAccessMode(this.privateState.DirectAccessMode,
                                       sendToServer: false);
        }

        private void SetSelectedAccessMode(WorldObjectDirectAccessMode mode, bool sendToServer)
        {
            if (this.selectedAccessMode == mode)
            {
                return;
            }

            this.selectedAccessMode = mode;
            this.NotifyPropertyChanged(nameof(this.SelectedAccessMode));

            if (sendToServer)
            {
                WorldObjectAccessModeSystem.ClientSetDirectAccessMode(this.worldObject, mode);
            }
        }
    }
}