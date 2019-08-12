namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System.Collections.Generic;

    public static class DevelopersListHelper
    {
        private static readonly HashSet<string> DeveloperNames
            = new HashSet<string>()
            {
                "ai_enabled",
                "Lurler",
                "AtomicTorch"
            };

        public static bool IsDeveloper(string name)
        {
            return DeveloperNames.Contains(name);
        }
    }
}