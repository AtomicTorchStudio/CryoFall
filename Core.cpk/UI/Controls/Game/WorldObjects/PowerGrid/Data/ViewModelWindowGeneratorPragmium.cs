namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.PowerGrid.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowGeneratorPragmium : BaseViewModel
    {
        public const string ReactorNameFormat = "Reactor {0}";

        public ViewModelWindowGeneratorPragmium(
            IStaticWorldObject worldObjectGenerator,
            ObjectGeneratorPragmiumPrivateState privateState,
            Action closeCallback)
        {
            privateState.ClientSubscribe(_ => _.ReactorStates,
                                         _ => closeCallback(),
                                         this);

            var reactorStates = privateState.ReactorStates;
            var reactors = new ViewModelPragmiumReactor[reactorStates.Length];

            for (byte index = 0; index < reactorStates.Length; index++)
            {
                var reactorPrivateState = reactorStates[index];
                reactors[index] = new ViewModelPragmiumReactor(worldObjectGenerator, index, reactorPrivateState);
            }

            this.Reactors = reactors;
        }

        public IReadOnlyList<ViewModelPragmiumReactor> Reactors { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientContainersExchangeManager.Unregister(this);
        }
    }
}