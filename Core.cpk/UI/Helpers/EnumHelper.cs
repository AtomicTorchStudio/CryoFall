namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Linq;

    public static class EnumHelper
    {
        public static ViewModelEnum<T>[] EnumValuesToViewModel<T>()
            where T : struct, Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select(e => new ViewModelEnum<T>(e))
                       .OrderBy(vm => vm.Order)
                       .ToArray();
        }
    }
}