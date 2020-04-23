namespace AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public static class ServerPlayerMuteSystem
    {
        private const string DatabaseKeyMutedTillDictionary = "MutedTillDictionary";

        private static readonly IDatabaseService Database = Api.Server.Database;

        private static Dictionary<string, double> mutedTillDictionary;

        /// <summary>
        /// Please use only for logging purposes.
        /// </summary>
        public static IEnumerable<string> GetMuteList()
        {
            return mutedTillDictionary.Keys.OrderBy(n => n);
        }

        public static bool IsMuted(string playerName, out double secondsRemains)
        {
            if (!mutedTillDictionary.TryGetValue(playerName, out var mutedTillTime))
            {
                // not muted
                secondsRemains = 0;
                return false;
            }

            secondsRemains = mutedTillTime - Api.Server.Game.FrameTime;
            return secondsRemains > 1;
        }

        public static void Mute(ICharacter character, int minutes)
        {
            if (character.IsNpc)
            {
                throw new Exception("Cannot mute NPC character");
            }

            Api.Assert(minutes > 0, "Minutes should be > 0");

            mutedTillDictionary[character.Name] = Api.Server.Game.FrameTime + 60 * minutes;
            Api.Logger.Important($"Player muted: {character}. Mute duration: {minutes} minutes");
        }

        public static bool Unmute(ICharacter character)
        {
            if (!mutedTillDictionary.Remove(character.Name))
            {
                return false;
            }

            Api.Logger.Important("Player unmuted: " + character);
            return true;
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                if (!Database.TryGet(nameof(ServerPlayerAccessSystem),
                                     DatabaseKeyMutedTillDictionary,
                                     out mutedTillDictionary))
                {
                    mutedTillDictionary = new Dictionary<string, double>();
                    Database.Set(nameof(ServerPlayerAccessSystem),
                                 DatabaseKeyMutedTillDictionary,
                                 mutedTillDictionary);
                }
            }
        }
    }
}