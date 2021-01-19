namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.GameApi.Data.Items;

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class FoodNutritionValueData
    {
        public double FoodRestore { get; set; }

        public bool HasAny
            => this.StaminaRestore != 0
               || this.FoodRestore != 0
               || this.WaterRestore != 0;

        public double StaminaRestore { get; set; }

        public double WaterRestore { get; set; }

        public void Add(IProtoItemFood protoItemFood, double count)
        {
            this.FoodRestore += protoItemFood.FoodRestore * count;
            this.StaminaRestore += protoItemFood.StaminaRestore * count;
            this.WaterRestore += protoItemFood.WaterRestore * count;
        }

        // ReSharper disable once UseStringInterpolation
        public override string ToString()
        {
            return string.Format("S{0:0.##} F{1:0.##} W{2:0.##}",
                                 this.StaminaRestore,
                                 this.FoodRestore,
                                 this.WaterRestore);
        }

        public void TryAdd(IProtoItem protoItem, double count)
        {
            if (protoItem is IProtoItemFood protoItemFood)
            {
                this.Add(protoItemFood, count);
            }
        }
    }
}