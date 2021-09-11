namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Launchpad.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelLaunchpadTask : BaseViewModel
    {
        private readonly int index;

        private readonly ObjectLaunchpadPrivateState privateState;

        private readonly ProtoObjectLaunchpad.LaunchpadTask task;

        private readonly IStaticWorldObject worldObject;

        public ViewModelLaunchpadTask(
            IStaticWorldObject worldObject,
            ProtoObjectLaunchpad.LaunchpadTask task,
            int index,
            ObjectLaunchpadPrivateState privateState)
        {
            this.worldObject = worldObject;
            this.task = task;
            this.index = index;
            this.privateState = privateState;
            privateState.ClientSubscribe(_ => _.TaskCompletionState,
                                         _ => this.NotifyPropertyChanged(nameof(this.IsCompleted)),
                                         this);
        }

        public BaseCommand CommandComplete
            => new ActionCommand(this.ExecuteCommandComplete);

        public TextureBrush Icon => Client.UI.GetTextureBrush(this.task.Icon);

        public IReadOnlyList<ProtoItemWithCount> InputItems => this.task.InputItems;

        public bool IsCompleted => this.privateState.TaskCompletionState[this.index];

        public string Name => this.task.Name;

        private void ExecuteCommandComplete()
        {
            ((ProtoObjectLaunchpad)this.worldObject.ProtoGameObject)
                .ClientCompleteTask(this.worldObject, this.index);
        }
    }
}