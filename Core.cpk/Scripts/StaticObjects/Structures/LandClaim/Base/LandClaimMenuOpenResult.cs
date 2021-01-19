namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum LandClaimMenuOpenResult : byte
    {
        Success,

        FailCannotInteract,

        FailPlayerIsNotOwner
    }
}