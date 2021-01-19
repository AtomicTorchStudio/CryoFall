namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelStructuresSelectList : BaseViewModel
    {
        public ViewModelStructuresSelectList(IEnumerable<IProtoObjectStructure> structures)
        {
            var structuresViewModels = structures.Select(
                staticWorldObjectType => new ViewModelStructure(staticWorldObjectType));

            this.Items = new ObservableCollection<ViewModelStructure>(structuresViewModels);
        }

        public ViewModelStructuresSelectList()
        {
            // generate some test data - for XAML design-time only
            this.Items = new ObservableCollection<ViewModelStructure>(
                new List<ViewModelStructure>()
                {
                    new(new ObjectCampfire()),
                    new(new ObjectWallWood()),
                    new(new ObjectStove())
                });
        }

        public ObservableCollection<ViewModelStructure> Items { get; }
    }
}