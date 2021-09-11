namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    public delegate bool DropItemRollFunctionDelegate(
        DropItemContext context,
        double probabilityMultiplier,
        out double resultProbability);
}