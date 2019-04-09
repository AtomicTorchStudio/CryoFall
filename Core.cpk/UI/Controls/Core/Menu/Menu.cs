namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class Menu : BaseViewModel
    {
        private static readonly List<Menu> RegisteredMenus = new List<Menu>();

        [ViewModelNotAutoDisposeField]
        private readonly IMenu menu;

        private bool isSelected;

        protected Menu(IMenu menu)
        {
            this.menu = menu;
            menu.IsOpenedChanged += this.UpdateIsSelected;

            this.CommandToggle = new ActionCommand(this.Toggle);
            this.isSelected = this.menu.IsOpened;

            RegisteredMenus.Add(this);
            Api.Logger.Info("Menu registered: " + menu.GetType().Name);
        }

        [ViewModelNotAutoDisposeField]
        public BaseCommand CommandToggle { get; }

        public bool IsSelected
        {
            get => this.isSelected;
            protected set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                this.NotifyThisPropertyChanged();

                if (!this.isSelected)
                {
                    return;
                }

                // close all other menus
                foreach (var otherMenu in RegisteredMenus)
                {
                    if (otherMenu != this)
                    {
                        otherMenu.Close();
                    }
                }
            }
        }

        public static void CloseAll()
        {
            foreach (var menu in RegisteredMenus)
            {
                menu.Close();
            }
        }

        public static TMenu Find<TMenu>()
            where TMenu : IMenu
        {
            foreach (var menu in RegisteredMenus)
            {
                if (menu.menu is TMenu result)
                {
                    return result;
                }
            }

            throw new Exception("No menu registered: " + typeof(TMenu).Name);
        }

        public static bool IsOpened<TMenu>()
            where TMenu : IMenu
        {
            foreach (var menu in RegisteredMenus)
            {
                if (menu.menu is TMenu)
                {
                    return menu.IsSelected;
                }
            }

            Logger.Info("No menu registered: " + typeof(TMenu).Name);
            return false;
        }

        public static bool IsOpenedAny()
        {
            foreach (var menu in RegisteredMenus)
            {
                if (menu.IsSelected)
                {
                    return true;
                }
            }

            return false;
        }

        public static void Open<TMenu>()
            where TMenu : IMenu
        {
            foreach (var menu in RegisteredMenus)
            {
                if (menu.menu is TMenu)
                {
                    if (!menu.IsSelected)
                    {
                        menu.Toggle();
                    }

                    return;
                }
            }

            throw new Exception("No menu registered: " + typeof(TMenu).Name);
        }

        public static Menu Register<TMenu>(TMenu instance)
            where TMenu : IMenu, new()
        {
            return new Menu(instance);
        }

        public static Menu Register<TMenu>()
            where TMenu : IMenu, new()
        {
            var gameMenu = new TMenu();
            gameMenu.InitMenu();
            return new Menu(gameMenu);
        }

        public static void Toggle<TMenu>()
            where TMenu : IMenu
        {
            foreach (var menu in RegisteredMenus)
            {
                if (menu.menu is TMenu)
                {
                    menu.Toggle();
                    return;
                }
            }

            throw new Exception("No menu registered: " + typeof(TMenu).Name);
        }

        public void Close()
        {
            if (this.menu.IsOpened)
            {
                this.menu.Toggle();
            }
        }

        public virtual void ToggleCurrentMenu()
        {
            if (this.IsDisposed)
            {
                Logger.Error("Menu disposed, should not toggle: " + this.menu);
            }

            this.Toggle();
        }

        public override string ToString()
        {
            return this.menu.GetType().ToString();
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.Close();
            this.menu.IsOpenedChanged -= this.UpdateIsSelected;
            RegisteredMenus.Remove(this);
            this.menu.Dispose();
            Api.Logger.Info("Menu unregistered: " + this.menu);
        }

        protected virtual void Toggle()
        {
            this.menu.Toggle();
        }

        protected virtual void UpdateIsSelected()
        {
            this.IsSelected = this.menu.IsOpened;
        }
    }
}