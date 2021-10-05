namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Launchpad.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowLaunchpad : BaseViewModel
    {
        private readonly ObjectLaunchpadPrivateState privateState;

        private readonly ProtoObjectLaunchpad protoObjectLaunchpad;

        private readonly ObjectLaunchpadPublicState publicState;

        private readonly IStaticWorldObject worldObject;

        public ViewModelWindowLaunchpad(IStaticWorldObject worldObject)
        {
            this.worldObject = worldObject;
            this.protoObjectLaunchpad = (ProtoObjectLaunchpad)worldObject.ProtoGameObject;

            this.privateState = worldObject.GetPrivateState<ObjectLaunchpadPrivateState>();
            this.publicState = worldObject.GetPublicState<ObjectLaunchpadPublicState>();
            this.publicState.ClientSubscribe(_ => _.LaunchServerFrameTime,
                                             _ => this.NotifyPropertyChanged(nameof(this.IsLaunched)),
                                             this);

            this.publicState.ClientSubscribe(_ => _.LaunchedByPlayerName,
                                             _ => this.NotifyPropertyChanged(nameof(this.LaunchedByPlayerName)),
                                             this);

            this.Tasks = this.protoObjectLaunchpad
                             .TasksList
                             .Select((task, taskIndex) =>
                                         new ViewModelLaunchpadTask(worldObject, task, taskIndex, this.privateState))
                             .ToArray();

            this.privateState.ClientSubscribe(_ => _.TaskCompletionState,
                                              _ => this.NotifyPropertyChanged(nameof(this.IsUpgradeAvailable)),
                                              this);

            if (this.protoObjectLaunchpad.ConfigUpgrade.Entries.Count > 0)
            {
                this.ViewModelStructureUpgrade =
                    new ViewModelStructureUpgrade(this.protoObjectLaunchpad.ConfigUpgrade.Entries[0]);
            }
        }

        public BaseCommand CommandLaunchRocket
            => new ActionCommand(this.ExecuteCommandLaunchRocket);

        public BaseCommand CommandLaunchUpgradeToNextStage
            => new ActionCommand(this.ExecuteCommandLaunchUpgradeToNextStage);

        public BaseCommand CommandResetLaunchpad
            => new ActionCommand(this.ExecuteCommandResetLaunchpad);

        public byte CurrentStageIndex
            => GetStageIndex(this.protoObjectLaunchpad);

        public bool IsLaunched
            => this.publicState.LaunchServerFrameTime > 0;

        public bool IsUpgradeAvailable
            => this.privateState.TaskCompletionState.All(task => task)
               && this.ViewModelStructureUpgrade is not null;

        public string LaunchedByPlayerName
            => this.publicState.LaunchedByPlayerName is not null
                   ? string.Format(CoreStrings.WindowLaunchpad_RocketLaunchedByPlayer_Format,
                                   this.publicState.LaunchedByPlayerName)
                   : null;

        // the last number of the class name is the stage number
        public byte MaxStageIndex => Api.FindProtoEntities<ProtoObjectLaunchpad>()
                                        .Select(GetStageIndex)
                                        .Maximum(i => i);

        public IReadOnlyList<ViewModelLaunchpadTask> Tasks { get; }

        public ViewModelStructureUpgrade ViewModelStructureUpgrade { get; }

        private static byte GetStageIndex(ProtoObjectLaunchpad proto)
        {
            var name = proto.ShortId;
            return byte.Parse(name[name.Length - 1].ToString());
        }

        private void ExecuteCommandLaunchRocket()
        {
            ((ObjectLaunchpadStage5)this.worldObject.ProtoGameObject)
                .ClientLaunchRocket(this.worldObject);
        }

        private void ExecuteCommandLaunchUpgradeToNextStage()
        {
            ((ProtoObjectLaunchpad)this.worldObject.ProtoGameObject)
                .ClientUpgradeToNextStage(this.worldObject);
        }

        private void ExecuteCommandResetLaunchpad()
        {
            ((ObjectLaunchpadStage5)this.worldObject.ProtoGameObject)
                .ClientResetLaunchpad(this.worldObject);
        }
    }
}