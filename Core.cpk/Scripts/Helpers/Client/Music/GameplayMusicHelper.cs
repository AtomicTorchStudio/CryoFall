namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Music
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.AmbientSound;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Misc;
    using AtomicTorch.CBND.CoreMod.Playlists;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.ClientMusic;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class GameplayMusicHelper
    {
        private static bool isInitialized;

        public static void Init()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            Api.Client.CurrentGame.ConnectionStateChanged += Refresh;
            Api.Client.Core.IsCompilingChanged += Refresh;

            Refresh();

            TimerCallback();
        }

        private static bool IsBaseMusicShouldPlay(ICharacter character)
        {
            using var tempListAreasNearby = Api.Shared.GetTempList<ILogicObject>();
            using var tempListOwnedAreaGroupsNearby = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(
                new RectangleInt(character.TilePosition.X, character.TilePosition.Y, 1, 1).Inflate(2),
                tempListAreasNearby,
                addGracePadding: true);

            // find owned bases (area groups) nearby
            foreach (var area in tempListAreasNearby.AsList())
            {
                if (LandClaimSystem.SharedIsOwnedArea(area, character))
                {
                    var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
                    tempListOwnedAreaGroupsNearby.AddIfNotContains(areasGroup);
                }
            }

            if (tempListOwnedAreaGroupsNearby.Count == 0)
            {
                return false;
            }

            var allAreas = (List<ILogicObject>)LandClaimSystem.SharedEnumerateAllAreas();

            // check every owned base whether it has at least single T2 or higher tier land claim
            foreach (var areasGroup in tempListOwnedAreaGroupsNearby.AsList())
            {
                foreach (var area in allAreas)
                {
                    var areaPublicState = LandClaimArea.GetPublicState(area);
                    if (areaPublicState.LandClaimAreasGroup == areasGroup
                        && areaPublicState.LandClaimTier > 1)
                    {
                        // found an area on the base with land claim tier > 1
                        return true;
                    }
                }
            }

            return false;
        }

        private static void Refresh()
        {
            var shouldPlay = Api.Client.CurrentGame.ConnectionState == ConnectionState.Connected
                             && !Api.Client.Core.IsCompiling
                             && !Api.Client.Core.IsCompilationFailed;

            if (!shouldPlay)
            {
                if (!(ClientMusicSystem.CurrentPlaylist is PlaylistMainMenu))
                {
                    // stop playing current playlist
                    ClientMusicSystem.CurrentPlaylist = null;
                }

                return;
            }

            var character = ClientCurrentCharacterHelper.Character;
            if (character == null
                || !LandClaimSystem.ClientIsOwnedAreasReceived)
            {
                ClientMusicSystem.CurrentPlaylist = null;
                return;
            }

            if (ClientRaidblockWatcher.IsNearOrInsideBaseUnderRaidblock)
            {
                // near or inside a base under raid
                ClientMusicSystem.CurrentPlaylist = Api.GetProtoEntity<PlaylistBaseRaid>();
                return;
            }

            if (IsBaseMusicShouldPlay(character))
            {
                // inside or nearby an owned land claim area
                ClientMusicSystem.CurrentPlaylist = Api.GetProtoEntity<PlaylistPlayersBase>();
                return;
            }


            if (ComponentAmbientSoundManager.Instance?.IsMusicSuppressedByAmbient() 
                ?? false)
            {
                ClientMusicSystem.CurrentPlaylist = null;
                return;
            }

            ClientMusicSystem.CurrentPlaylist = Api.GetProtoEntity<PlaylistGameplay>();
        }

        private static void TimerCallback()
        {
            ClientTimersSystem.AddAction(delaySeconds: 0.5, TimerCallback);

            Refresh();
        }
    }
}