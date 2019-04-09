namespace AtomicTorch.CBND.CoreMod
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientWorldObjectInteractHelper
    {
        public static IWorldObject CurrentlyInteractingWith;

        public static IStaticWorldObject ClientFindWorldObjectAtCurrentMousePosition()
        {
            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            return ClientComponentObjectInteractionHelper.FindStaticObjectAtCurrentMousePosition(
                       currentCharacter,
                       CollisionGroups.ClickArea)
                   // second try to find by default collider
                   ?? ClientComponentObjectInteractionHelper.FindStaticObjectAtCurrentMousePosition(
                       currentCharacter,
                       CollisionGroups.Default)
                   // try to find object in the pointed tile
                   ?? Api.Client.World.GetTile(Api.Client.Input.MouseWorldPosition.ToVector2Ushort())
                         .StaticObjects.OrderByDescending(o => o.ProtoStaticWorldObject.Kind)
                         .FirstOrDefault();
        }
    }
}