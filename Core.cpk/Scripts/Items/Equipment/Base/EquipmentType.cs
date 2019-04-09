namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public enum EquipmentType : byte
    {
        Head = 0,

        Chest = 1,

        Legs = 2,

        // two implants (next commented code is intentional)
        Implant = 3,
        //Implant = 4,

        // five device items
        Device = 5,
        //Device = 6,
        //Device = 7,
        //Device = 8,
        //Device = 9,

        // special case (Head+Chest+Legs)
        FullBody = 100
    }
}