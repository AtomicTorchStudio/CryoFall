namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting
{
    using System.Collections.Generic;

    public static class ClientLightSourceManager
    {
        private static readonly List<BaseClientComponentLightSource> allLightSources
            = new List<BaseClientComponentLightSource>();

        /// <summary>
        /// Please do NOT modify! It's exposed as List to ensure faster iteration only.
        /// </summary>
        public static readonly List<BaseClientComponentLightSource> AllLightSources
            = allLightSources;

        public static void Register(BaseClientComponentLightSource lightSource)
        {
            allLightSources.Add(lightSource);
        }

        public static void Unregister(BaseClientComponentLightSource lightSource)
        {
            allLightSources.Remove(lightSource);
        }
    }
}