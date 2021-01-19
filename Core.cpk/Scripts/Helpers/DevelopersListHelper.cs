namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System.Collections.Generic;

    public static class DevelopersListHelper
    {
        private static readonly HashSet<string> DeveloperNames
            = new()
            {
                "ai_enabled",
                "Lurler",
                "AtomicTorchStudio"
            };

        public static bool IsDeveloper(string name)
        {
            return DeveloperNames.Contains(name);
        }
    }
}