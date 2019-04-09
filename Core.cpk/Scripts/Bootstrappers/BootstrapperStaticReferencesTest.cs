namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BootstrapperStaticReferencesTest : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            if (TestReferencesHolder<string>.TestObject != null)
            {
                Logger.Error("Test object not null. Something wrong with reloading scripting assemblies.");
                return;
            }

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            TestReferencesHolder<string>.TestObject = "test value";
        }

        public static class TestReferencesHolder<T>
            where T : class
        {
            public static T TestObject { get; set; }
        }
    }
}