namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public enum EquipmentType : byte
    {
        Head = 0,

        Armor = 1,

        // three implants (next commented code is intentional)
        Implant = 2,
        //Implant = 3,
        //Implant = 4,

        // five device items
        Device = 5,
        //Device = 6,
        //Device = 7,
        //Device = 8,
        //Device = 9,

        // special case (Head+Armor)
        FullBody = 100
    }
}