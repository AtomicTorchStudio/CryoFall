namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ConsolePlayerSetMobMode : BaseConsoleCommand
    {
        public override string Description
            => "Toggles mob mode (player mimicking a particular mob).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setMobMode";

        public string Execute(bool isEnabled, [CurrentCharacterIfNull] ICharacter character)
        {
            var containerHotbar = character.SharedGetPlayerContainerHotbar();
            this.RemoveAllEquipment(character);

            if (isEnabled)
            {
                PlayerCharacterMob.ServerSwitchToMobMode(character);
                Server.Items.CreateItem<ItemWeaponScorpionClaws>(containerHotbar);
                PlayerCharacter.SharedSelectHotbarSlotId(character, 0);
            }
            else
            {
                PlayerCharacterMob.ServerSwitchToPlayerMode(character);
            }

            return null;
        }

        private void RemoveAllEquipment(ICharacter character)
        {
            DestroyAllItems(character.SharedGetPlayerContainerEquipment().Items.ToList());
            DestroyAllItems(character.SharedGetPlayerContainerHotbar().Items.ToList());

            void DestroyAllItems(List<IItem> list)
            {
                foreach (var item in list)
                {
                    Server.Items.DestroyItem(item);
                }
            }
        }
    }
}