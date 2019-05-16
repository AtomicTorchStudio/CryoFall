namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System.Windows;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public interface IProtoOption
    {
        string Id { get; }

        bool IsHidden { get; }

        bool IsModified { get; }

        string Name { get; }

        IProtoOption OrderAfterOption { get; }

        string ShortId { get; }

        ProtoOptionsCategory Category { get; }

        void Apply();

        void ApplyAbstractValue(object value);

        void Cancel();

        void CreateControl(out FrameworkElement labelControl, out FrameworkElement optionControl);

        object GetAbstractValue();

        void RegisterValueType(IClientStorage storage);

        void Reset(bool apply);
    }
}