namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ReadOnlyMineralDropItemsConfig
    {
        public readonly IReadOnlyDropItemsList Stage1;

        public readonly IReadOnlyDropItemsList Stage2;

        public readonly IReadOnlyDropItemsList Stage3;

        public readonly IReadOnlyDropItemsList Stage4;

        public ReadOnlyMineralDropItemsConfig(MineralDropItemsConfig config)
        {
            this.Stage1 = config.Stage1;
            this.Stage2 = config.Stage2;
            this.Stage3 = config.Stage3;
            this.Stage4 = config.Stage4;
        }

        public IReadOnlyDropItemsList GetForStage(int damageStage)
        {
            switch (damageStage)
            {
                case 1:
                    return this.Stage1;
                case 2:
                    return this.Stage2;
                case 3:
                    return this.Stage3;
                case 4:
                    return this.Stage4;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(damageStage),
                        "Damage stage must be in range from 1 to 4, provided value is " + damageStage);
            }
        }
    }
}