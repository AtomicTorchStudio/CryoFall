namespace AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientTimeOfDayVisibilityHelper
    {
        public static bool ClientIsObservable(IWorldObject worldObject)
        {
            if (!TimeOfDaySystem.IsNight)
            {
                return true;
            }

            if (ClientComponentLightingRenderer.AdditionalAmbientLight > 0
                || ClientComponentLightingRenderer.AdditionalAmbightLightAdditiveFraction > 0)
            {
                // assume night vision or artificial retina implant
                return true;
            }

            if (ClientCurrentCharacterHelper.Character?.ProtoCharacter
                    is PlayerCharacterSpectator)
            {
                // don't apply this limitation to the spectator player
                return true;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(ClientCurrentCharacterHelper.Character))
            {
                // don't apply this limitation to the player in creative mode
                return true;
            }

            if (worldObject is ICharacter character
                && !character.IsNpc
                && PartySystem.ClientIsPartyMember(character.Name))
            {
                // party members are always visible in night
                return true;
            }

            // it's night, perform the distance check to closest light source
            Vector2D position;
            switch (worldObject)
            {
                case IDynamicWorldObject dynamicWorldObject:
                    position = dynamicWorldObject.Position;
                    break;
                case IStaticWorldObject staticWorldObject:
                    position = staticWorldObject.TilePosition.ToVector2D()
                               + staticWorldObject.ProtoStaticWorldObject.Layout.Center;
                    break;
                default:
                    position = worldObject.TilePosition.ToVector2D();
                    break;
            }

            foreach (var lightSource in ClientLightSourceManager.AllLightSources)
            {
                var distanceToLightSqr = position.DistanceSquaredTo(lightSource.SceneObject.Position);
                var lightRadiusSqr = lightSource.LogicalLightRadiusSqr;
                if (distanceToLightSqr <= lightRadiusSqr
                    && lightRadiusSqr > 0)
                {
                    // lighted object
                    return true;
                }
            }

            return false;
        }
    }
}