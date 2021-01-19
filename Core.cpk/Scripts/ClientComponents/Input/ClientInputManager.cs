namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.ClientOptions.Controls;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static partial class ClientInputManager
    {
        private const string InputSettingsStorageFilePath = "Options/Input";

        public static readonly IInputClientService RawInput = Api.Client.Input;

        private static readonly InputMappingSnapshot CurrentInputMappingSnapshot
            = new();

        private static readonly Func<InputKey, bool, bool> DelegateRawInputIsKeyDown = RawInput.IsKeyDown;

        private static readonly Func<InputKey, bool, bool> DelegateRawInputIsKeyHeld = RawInput.IsKeyHeld;

        private static readonly Func<InputKey, bool, bool> DelegateRawInputIsKeyUp = RawInput.IsKeyUp;

        private static readonly Dictionary<IWrappedButton, ButtonMapping> MappingButtonToKeys
            = new();

        private static readonly HashSet<Type> RegisteredButtonEnums = new();

        private static readonly Dictionary<IWrappedButton, ButtonInfoAttribute> RegisteredButtons
            = new();

        private static readonly IClientStorage Storage;

        private static bool isFrozen;

        static ClientInputManager()
        {
            // create storage
            var storage = Api.Client.Storage.GetStorage(InputSettingsStorageFilePath);
            storage.RegisterType(typeof(InputKey));
            storage.RegisterType(typeof(ButtonMapping));
            storage.RegisterType(typeof(InputMappingSnapshot));
            Storage = storage;

            // register default buttons enum
            RegisterButtonsEnum<GameButton>();

            // create input updater (updated every frame)
            Api.Client.Scene.CreateSceneObject(nameof(ClientComponentInputManagerUpdater))
               .AddComponent<ClientComponentInputManagerUpdater>();
        }

        public static event Action<IWrappedButton> ButtonKeyMappingUpdated;

        public static Vector2D MouseWorldPosition => RawInput.MouseWorldPosition;

        public static Dictionary<IWrappedButton, ButtonMapping> CloneMapping()
        {
            return new(MappingButtonToKeys);
        }

        public static void ConsumeAllButtons()
        {
            RawInput.ConsumeAllKeys();
        }

        public static void ConsumeButton<TButton>(TButton button)
            where TButton : struct, Enum
        {
            ConsumeButton(WrappedButton<TButton>.GetWrappedButton(button));
        }

        public static void ConsumeButton(IWrappedButton button)
        {
            if (!MappingButtonToKeys.TryGetValue(button, out var mapping))
            {
                return;

                //throw new Exception("The button is not mapped, it cannot be consumed");
            }

            RawInput.ConsumeKey(mapping.PrimaryKey);
            RawInput.ConsumeKey(mapping.SecondaryKey);
        }

        public static IEnumerable<IWrappedButton> GetButtonForKey(InputKey key, string buttonCategory)
        {
            foreach (var pair in MappingButtonToKeys)
            {
                var mapping = pair.Value;
                if (mapping.PrimaryKey != key
                    && mapping.SecondaryKey != key)
                {
                    continue;
                }

                // found mapping to this key
                var foundButton = pair.Key;
                if (!IsSameCategory(foundButton, buttonCategory))
                {
                    // different category
                    continue;
                }

                yield return foundButton;
            }
        }

        public static ButtonInfoAttribute GetButtonInfo(IWrappedButton button)
        {
            return RegisteredButtons.Find(button);
        }

        public static InputKey GetKeyForButton<TButton>(TButton button)
            where TButton : struct, Enum
        {
            return GetKeyForButton(
                WrappedButton<TButton>.GetWrappedButton(button));
        }

        public static InputKey GetKeyForButton(IWrappedButton button)
        {
            var mapping = GetMappingForAbstractButton(button);
            return mapping.PrimaryKey != InputKey.None
                       ? mapping.PrimaryKey
                       : mapping.SecondaryKey;
        }

        public static IReadOnlyDictionary<IWrappedButton, ButtonInfoAttribute> GetKnownButtons()
        {
            return RegisteredButtons;
        }

        public static ButtonMapping GetMappingForAbstractButton(IWrappedButton button)
        {
            return MappingButtonToKeys.Find(button);
        }

        public static bool IsButtonDown<TButton>(TButton button, bool evenIfHandled = false)
            where TButton : struct, Enum
        {
            return WrapAndCheck(button, DelegateRawInputIsKeyDown, evenIfHandled);
        }

        public static bool IsButtonHeld<TButton>(TButton button, bool evenIfHandled = false)
            where TButton : struct, Enum
        {
            return WrapAndCheck(button, DelegateRawInputIsKeyHeld, evenIfHandled);
        }

        public static bool IsButtonUp<TButton>(TButton button, bool evenIfHandled = false)
            where TButton : struct, Enum
        {
            return WrapAndCheck(button, DelegateRawInputIsKeyUp, evenIfHandled);
        }

        public static void RegisterButtonsEnum<TButton>()
            where TButton : struct, Enum
        {
            if (isFrozen)
            {
                throw new Exception(
                    "Cannot register new button enums - "
                    + nameof(ClientInputManager)
                    + " is in a frozen state."
                    + Environment.NewLine
                    + "It's possible to register new button enums only during the bootstrappers initialization time (before any button or input context were accessed).");
            }

            var enumType = typeof(TButton);
            ValidateButtonType(enumType);

            if (!RegisteredButtonEnums.Add(enumType))
            {
                throw new Exception("Already registered buttons enum: " + enumType);
            }

            foreach (var button in EnumExtensions.GetValues<TButton>())
            {
                var memberInfo = enumType.ScriptingGetMember(button.ToString())[0];
                var buttonInfo = memberInfo.GetCustomAttribute<ButtonInfoAttribute>();
                if (buttonInfo is null)
                {
                    throw new Exception(
                        $"There is no {nameof(ButtonInfoAttribute)} on button {button} (enum {enumType.FullName})");
                }

                var description = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                if (description is null)
                {
                    throw new Exception(
                        $"There is no {nameof(DescriptionAttribute)} on button {button} (enum {enumType.FullName})");
                }

                buttonInfo.Title = description.Description;

                var wrappedButton = WrappedButton<TButton>.GetWrappedButton(button);
                RegisteredButtons.Add(wrappedButton, buttonInfo);
                //ApplyDefaultMapping(wrappedButton, buttonInfo);
            }
        }

        public static void ResetMappingToDefault()
        {
            Api.Logger.Important("Input manager: reset mapping to default");

            MappingButtonToKeys.Clear();

            foreach (var pair in RegisteredButtons)
            {
                ApplyDefaultMapping(pair.Key, pair.Value);
                ButtonKeyMappingUpdated?.Invoke(pair.Key);
            }

            Freeze();
        }

        public static void Save()
        {
            CurrentInputMappingSnapshot.Update(RegisteredButtonEnums, MappingButtonToKeys);
            Storage.Save(CurrentInputMappingSnapshot);
        }

        public static void SetAbstractButtonMapping(
            IWrappedButton buttonToRebind,
            ButtonMapping mapping,
            bool writeToLog = true)
        {
            if (!RegisteredButtons.TryGetValue(buttonToRebind, out var buttonToRebindInfo))
            {
                throw new Exception("Unknown button: " + buttonToRebind);
            }

            var buttonCategory = buttonToRebindInfo.Category;

            foreach (var pair in MappingButtonToKeys.ToList())
            {
                var existingButton = pair.Key;
                if (existingButton.Equals(buttonToRebind))
                {
                    continue;
                }

                var existingMapping = pair.Value;

                if (existingMapping.PrimaryKey != InputKey.None
                    && (existingMapping.PrimaryKey == mapping.PrimaryKey
                        || existingMapping.PrimaryKey == mapping.SecondaryKey))
                {
                    if (IsSameCategory(existingButton, buttonCategory))
                    {
                        // the key is already used - release this mapping
                        ReleaseAbstractButtonMapping(existingButton, existingMapping.PrimaryKey);
                    }
                }

                if (existingMapping.SecondaryKey != InputKey.None
                    && (existingMapping.SecondaryKey == mapping.PrimaryKey
                        || existingMapping.SecondaryKey == mapping.SecondaryKey))
                {
                    if (IsSameCategory(existingButton, buttonCategory))
                    {
                        // the key is already used - release this mapping
                        ReleaseAbstractButtonMapping(existingButton, existingMapping.SecondaryKey);
                    }
                }
            }

            if (writeToLog)
            {
                Api.Logger.Important($"Setting button mapping: {buttonToRebind}; mapping: {mapping}");
            }

            MappingButtonToKeys[buttonToRebind] = mapping;

            ButtonKeyMappingUpdated?.Invoke(buttonToRebind);
        }

        public static void SetMapping(
            Dictionary<IWrappedButton, ButtonMapping> mapping,
            bool writeToLog = true)
        {
            MappingButtonToKeys.GetDiff(mapping, out var added, out var removed);

            foreach (var pair in removed)
            {
                MappingButtonToKeys.Remove(pair.Key);
            }

            foreach (var pair in added)
            {
                SetAbstractButtonMapping(pair.Key, pair.Value, writeToLog);
            }
        }

        internal static bool IsButtonDown(IWrappedButton wrappedButton, bool evenIfHandled = false)
        {
            return Check(wrappedButton, DelegateRawInputIsKeyDown, evenIfHandled);
        }

        internal static bool IsButtonHeld(IWrappedButton wrappedButton, bool evenIfHandled = false)
        {
            return Check(wrappedButton, DelegateRawInputIsKeyHeld, evenIfHandled);
        }

        internal static bool IsButtonUp(IWrappedButton wrappedButton, bool evenIfHandled = false)
        {
            return Check(wrappedButton, DelegateRawInputIsKeyUp, evenIfHandled);
        }

        private static void ApplyDefaultMapping(IWrappedButton button, ButtonInfoAttribute info)
        {
            MappingButtonToKeys.Add(button, info.DefaultButtonMapping);
        }

        private static bool Check(
            IWrappedButton wrappedButton,
            Func<InputKey, bool, bool> checkFunc,
            bool evenIfHandled)
        {
            if (!isFrozen)
            {
                LoadButtonsMapping();
            }

            if (!MappingButtonToKeys.TryGetValue(
                    wrappedButton,
                    out var mapping))
            {
                // the key is not mapped
                var buttonType = wrappedButton.WrappedButtonType;
                ValidateButtonType(buttonType);

                if (!RegisteredButtonEnums.Contains(buttonType))
                {
                    throw new Exception(
                        $"The button enum type is not registered: {buttonType.FullName}. Please register it in your mod initialization code by calling {nameof(ClientInputManager)}.{nameof(RegisterButtonsEnum)}<{buttonType.Name}>()");
                }

                return false;
            }

            if (mapping.PrimaryKey != InputKey.None
                && checkFunc(mapping.PrimaryKey, evenIfHandled))
            {
                // check successful
                return true;
            }

            if (mapping.SecondaryKey != InputKey.None
                && checkFunc(mapping.SecondaryKey, evenIfHandled))
            {
                // check successful
                return true;
            }

            return false;
        }

        private static void Freeze()
        {
            isFrozen = true;
            Api.GetProtoEntity<ControlsOptionsCategory>().SaveMappingIfRequired();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Initialize()
        {
            // necessary to invoke the static constructor
        }

        private static bool IsSameCategory(
            IWrappedButton foundButton,
            string buttonCategory)
        {
            return RegisteredButtons[foundButton].Category == buttonCategory;
        }

        private static void LoadButtonsMapping()
        {
            if (!Storage.TryLoad<InputMappingSnapshot>(out var snapshot))
            {
                // cannot load the input
                ResetMappingToDefault();
                return;
            }

            CurrentInputMappingSnapshot.Update(snapshot);
            SetMapping(
                CurrentInputMappingSnapshot.GetMapping(RegisteredButtonEnums, RegisteredButtons),
                writeToLog: false);

            Freeze();
        }

        private static void ReleaseAbstractButtonMapping(
            IWrappedButton buttonToReset,
            InputKey keyToRemove)
        {
            if (!RegisteredButtons.TryGetValue(buttonToReset, out _))
            {
                throw new Exception("Unknown button: " + buttonToReset);
            }

            var mapping = MappingButtonToKeys[buttonToReset];
            if (mapping.PrimaryKey == keyToRemove)
            {
                mapping = new ButtonMapping(InputKey.None, mapping.SecondaryKey);
            }

            if (mapping.SecondaryKey == keyToRemove)
            {
                mapping = new ButtonMapping(mapping.PrimaryKey, InputKey.None);
            }

            Api.Logger.Important(
                $"Releasing button mapping: {buttonToReset}; key to remove: {keyToRemove}; mapping: {mapping}");
            MappingButtonToKeys[buttonToReset] = mapping;

            ButtonKeyMappingUpdated?.Invoke(buttonToReset);
        }

        private static void ValidateButtonType(Type type)
        {
            if (type == typeof(InputKey))
            {
                throw new Exception(
                    $"You should never use {nameof(InputKey)} type with {nameof(ClientInputManager)}. Use a special buttons enum instead - your own or {nameof(GameButton)}");
            }

            if (!type.IsEnum)
            {
                throw new Exception("Type of button must be an enum type: " + type);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool WrapAndCheck<TButton>(
            TButton button,
            Func<InputKey, bool, bool> checkFunc,
            bool evenIfHandled)
            where TButton : struct, Enum
        {
            return Check(
                WrappedButton<TButton>.GetWrappedButton(button),
                checkFunc,
                evenIfHandled);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Initialize();
            }
        }
    }
}