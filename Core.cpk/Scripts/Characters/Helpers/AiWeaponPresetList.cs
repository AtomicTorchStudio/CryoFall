namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Collections.Generic;

    public class AiWeaponPresetList
    {
        private readonly List<AiWeaponPreset> entries
            = new();

        public AiWeaponPresetList Add(AiWeaponPreset preset)
        {
            this.entries.Add(preset);
            return this;
        }

        public IReadOnlyList<AiWeaponPreset> ToReadReadOnly()
        {
            return this.entries.ToArray();
        }
    }
}