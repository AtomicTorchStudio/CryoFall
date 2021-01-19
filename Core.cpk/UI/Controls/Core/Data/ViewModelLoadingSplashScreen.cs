namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelLoadingSplashScreen : BaseViewModel
    {
        public ViewModelLoadingSplashScreen()
        {
            this.RandomizeInfo();
        }

        public bool DisplayStructureInfos { get; set; }

        public List<ViewModelStructureInfo> StructureInfos { get; private set; }

        public void RandomizeInfo()
        {
            var infos = this.StructureInfos;
            this.StructureInfos = null;
            if (infos is not null)
            {
                this.DisposeCollection(infos);
            }

            infos = new List<ViewModelStructureInfo>();

            if (!this.DisplayStructureInfos)
            {
                this.StructureInfos = infos;
                return;
            }

            var availableStructures = StructuresHelper.LoadingSplashScreenStructures
                                                      .ToList();

            // filter categories
            for (var index = 0; index < availableStructures.Count; index++)
            {
                var structure = availableStructures[index];
                switch (structure.Category)
                {
                    case StructureCategoryFood:
                    case StructureCategoryBuildings:
                    case StructureCategoryDecorations:
                    case StructureCategoryOther:
                        // ignore these categories
                        availableStructures.RemoveAt(index);
                        index--;
                        break;
                }
            }

            do
            {
                var randomStructure = availableStructures.TakeByRandom();
                var hasSameOrSimilarStructure = false;
                foreach (var existingEntry in infos)
                {
                    if (existingEntry.ProtoStructure == randomStructure
                        || existingEntry.ProtoStructure.Category == randomStructure.Category)
                    {
                        hasSameOrSimilarStructure = true;
                        break;
                    }
                }

                if (hasSameOrSimilarStructure)
                {
                    continue;
                }

                infos.Add(new ViewModelStructureInfo(randomStructure));
            }
            while (infos.Count < 3);

            this.StructureInfos = infos;
        }

        public class ViewModelStructureInfo : BaseViewModel
        {
            public readonly IProtoObjectStructure ProtoStructure;

            public ViewModelStructureInfo(IProtoObjectStructure protoStructure)
            {
                this.ProtoStructure = protoStructure;

                if (IsDesignTime)
                {
                    this.Icon = Brushes.BlueViolet;
                    return;
                }

                if (protoStructure is not null)
                {
                    this.Icon = Client.UI.GetTextureBrush(this.ProtoStructure.Icon);
                }
            }

            public string Description => this.ProtoStructure.Description;

            public Brush Icon { get; }

            public string Title => this.ProtoStructure.Name;

            public override string ToString()
            {
                return this.ProtoStructure.ToString();
            }
        }
    }
}