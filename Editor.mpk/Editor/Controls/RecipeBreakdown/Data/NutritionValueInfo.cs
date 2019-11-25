namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Items.Food;

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class NutritionValueInfo
    {
        public double FoodRestore { get; set; }

        public bool HasAny
            => this.HealthRestore != 0
               || this.StaminaRestore != 0
               || this.FoodRestore != 0
               || this.WaterRestore != 0;

        public double HealthRestore { get; set; }

        public double StaminaRestore { get; set; }

        public double WaterRestore { get; set; }

        // ReSharper disable once UseStringInterpolation
        public override string ToString()
        {
            return string.Format("H{0:0.##} S{1:0.##} F{2:0.##} W{3:0.##}",
                                 this.HealthRestore,
                                 this.StaminaRestore,
                                 this.FoodRestore,
                                 this.WaterRestore);
        }

        public void TryAdd(ProtoItemWithCountFractional entry)
        {
            if (!(entry.ProtoItem is IProtoItemFood protoItemFood))
            {
                return;
            }

            this.FoodRestore += protoItemFood.FoodRestore * entry.Count;
            this.HealthRestore += protoItemFood.HealthRestore * entry.Count;
            this.StaminaRestore += protoItemFood.StaminaRestore * entry.Count;
            this.WaterRestore += protoItemFood.WaterRestore * entry.Count;
        }
    }
}