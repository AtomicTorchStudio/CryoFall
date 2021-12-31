// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Editor.Console.Debug
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleFindInvalidNames : BaseConsoleCommand
    {
        public override string Description => "Finds prototypes which have invalid names.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "debug.findInvalidNames";

        public string Execute()
        {
            var invalidList = Api.FindProtoEntities<IProtoEntity>()
                                 .Where(p => p is IProtoCharacter
                                                 or IProtoStatusEffect
                                                 or Recipe
                                                 or IProtoItem
                                                 or IProtoPerk
                                                 or IProtoQuest
                                                 or IProtoSkill
                                                 or IProtoStaticWorldObject
                                                 or TechNode
                                                 or TechGroup
                                                 or IProtoTile)
                                 .Where(p => !IsValidName(p.Name))
                                 .OrderBy(p => p.Id)
                                 .ToList();

            if (invalidList.Count == 0)
            {
                return "All names are valid";
            }

            return "Found prototypes with invalid names:"
                   + Environment.NewLine
                   + invalidList.Select(s => " * " + s.Id + " - \"" + s.Name + "\"")
                                .GetJoinedString(Environment.NewLine);
        }

        private static bool IsValidName(string name)
        {
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c))
                {
                    continue;
                }

                switch (c)
                {
                    case ' ':
                    case '-':
                    case '—':
                    case ':':
                    case ',':
                    case '.':
                    case '\'':
                        continue;
                }

                return false;
            }

            return true;
        }
    }
}