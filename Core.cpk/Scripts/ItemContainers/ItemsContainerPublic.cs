namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    /// <summary>
    /// This container prototype will not perform the "is container in the character private scope" validation.
    /// </summary>
    public class ItemsContainerPublic : ItemsContainerDefault
    {
        protected override bool IsValidateContainerInPrivateScope => false;
    }
}