namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemBonePants : ProtoItemEquipmentLegs
    {
        public override string Description => GetProtoEntity<ItemBoneJacket>().Description;

        public override ushort DurabilityMax => 500;

        public override string Name => "Bone pants";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.3,
                heat: 0.2,
                cold: 0.2,
                chemical: 0.1,
                electrical: 0.15,
                radiation: 0.1,
                psi: 0);
        }

        // we can play extra custom sound on footstep!
        //protected override ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootstepsOverride()
        //{
        //    return SoundPreset.CreateFromFolder<GroundSoundMaterial>("Items/Equipment/Bone/ItemBonePants/Movement");
        //}
    }
}