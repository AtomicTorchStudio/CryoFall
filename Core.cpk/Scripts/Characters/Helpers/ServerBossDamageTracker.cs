namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ServerBossDamageTracker
    {
        private readonly ListDictionary<ICharacter, double> damageByCharacter
            = new ListDictionary<ICharacter, double>();

        [NonSerialized]
        private double timeAccumulated;

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

        public void Update(double deltaTime)
        {
            this.timeAccumulated += deltaTime;
            if (this.timeAccumulated < 1)
            {
                // wait until a full second is accumulated
                return;
            }

            this.timeAccumulated -= 1;

            // Diminish the accumulated dealt damage value each second.
            // This way players that have made most of their damage more recently
            // are ranked higher than those that dealt their damage long ago
            // (it will prevent the exploit when a player almost kills the boss (maybe even several times)
            // and walk away so when another group of players defeat the boss
            // they surprisingly get very little loot compared to that player).
            var list = this.damageByCharacter.List;
            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index];
                var damage = entry.Value * 0.99; // reduce by 1% every second
                list[index] = new KeyValuePair<ICharacter, double>(entry.Key, damage);
            }
        }
    }
}