namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ServerBossDamageTracker
    {
        private readonly Dictionary<ICharacter, double> damageByCharacter
            = new Dictionary<ICharacter, double>();

        public IReadOnlyList<(ICharacter Character, double Damage)> GetDamageByCharacter()
        {
            var result = this.damageByCharacter
                             .Select(p => (Character: p.Key, Damage: p.Value))
                             .ToList();
            result.SortByDesc(p => p.Damage);
            return result;
        }

        public void RegisterDamage(ICharacter character, double damageApplied)
        {
            if (character is null
                || character.IsNpc)
            {
                return;
            }

            var newDamageValue = damageApplied;
            if (this.damageByCharacter.TryGetValue(character, out var currentValue))
            {
                newDamageValue += currentValue;
            }

            this.damageByCharacter[character] = newDamageValue;
        }
    }
}