namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientFactionEmblemDataCache
    {
        private static readonly Dictionary<string, FactionEmblem> CacheEmblems;

        static ClientFactionEmblemDataCache()
        {
            if (Api.IsServer)
            {
                return;
            }

            Api.Client.CurrentGame.ConnectionStateChanged += CurrentGameOnConnectionStateChanged;
            FactionSystem.ClientFactionDissolved += FactionSystemOnClientFactionDissolved;
            CacheEmblems = new Dictionary<string, FactionEmblem>();
        }

        public static async Task<FactionEmblem> GetFactionEmblemAsync(string clanTag)
        {
            if (CacheEmblems.TryGetValue(clanTag, out var emblem))
            {
                return emblem;
            }

            emblem = await FactionSystem.ClientGetFactionEmblemAsync(clanTag);
            CacheEmblems[clanTag] = emblem;
            return emblem;
        }

        public static bool TryGetValue(string clanTag, out FactionEmblem emblem)
        {
            return CacheEmblems.TryGetValue(clanTag, out emblem);
        }

        private static void CurrentGameOnConnectionStateChanged()
        {
            CacheEmblems.Clear();
        }

        private static void FactionSystemOnClientFactionDissolved(string clanTag)
        {
            CacheEmblems.Remove(clanTag);
        }
    }
}