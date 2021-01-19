namespace AtomicTorch.CBND.CoreMod.Systems.Notifications
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum NotificationColor : byte
    {
        Neutral,

        Good,

        Bad,

        Event
    }
}