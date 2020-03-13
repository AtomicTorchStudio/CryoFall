// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.World
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleActivateBaseRaidblock : BaseConsoleCommand
    {
        public override string Description =>
            @"Activates raidblock status for the base where character is present now.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "world.activateBaseRaidblock";

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            using var tempAreas = Api.Shared.GetTempList<ILogicObject>();

            LandClaimSystem.SharedGetAreasInBounds(
                new RectangleInt(character.TilePosition.X, character.TilePosition.Y, 1, 1),
                tempAreas,
                addGracePadding: false);

            foreach (var logicObject in tempAreas.AsList())
            {
                LandClaimSystem.ServerSetRaidblock(logicObject, isShort: false);
            }

            return string.Empty;
        }
    }
}