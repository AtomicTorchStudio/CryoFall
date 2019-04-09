namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System.Windows;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public interface IProtoOption : IProtoEntity
    {
        bool IsHidden { get; }

        bool IsModified { get; }

        IProtoOption OrderAfterOption { get; }

        void Apply();

        void ApplyAbstractValue(object value);

        void Cancel();

        void CreateControl(out FrameworkElement labelControl, out FrameworkElement optionControl);

        object GetAbstractValue();

        void RegisterValueType(IClientStorage storage);

        void Reset(bool apply);
    }
}