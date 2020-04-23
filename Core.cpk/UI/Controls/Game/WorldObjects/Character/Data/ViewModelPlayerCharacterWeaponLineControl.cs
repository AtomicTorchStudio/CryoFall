namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Character.Data
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelPlayerCharacterWeaponLineControl : BaseViewModel
    {
        private readonly Grid layoutRoot;

        private ICharacter character;

        private PlayerCharacterPrivateState characterPrivateState;

        private double screenHeight;

        private double screenWidth;

        public ViewModelPlayerCharacterWeaponLineControl()
        {
            this.layoutRoot = Client.UI.LayoutRoot;
            this.layoutRoot.SizeChanged += this.LayoutRootSizeChanged;

            ClientUpdateHelper.UpdateCallback += this.Update;

            this.RefreshCenter();
        }

        public ICharacter Character
        {
            get => this.character;
            set
            {
                if (this.character == value)
                {
                    return;
                }

                this.character = value;
                this.characterPrivateState = this.character.GetPrivateState<PlayerCharacterPrivateState>();
            }
        }

        public float ScreenCenterX { get; private set; }

        public float ScreenCenterY { get; private set; }

        public float ScreenTargetX { get; private set; }

        public float ScreenTargetY { get; private set; }

        public Visibility Visibility { get; private set; }

        private void LayoutRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshCenter();
        }

        private void RefreshCenter()
        {
            this.screenWidth = this.layoutRoot.ActualWidth;
            this.screenHeight = this.layoutRoot.ActualHeight;
            this.ScreenCenterX = (float)(this.screenWidth / 2);
            this.ScreenCenterY = (float)(this.screenHeight / 2);
        }

        private void Update()
        {
            if (this.character == null
                || this.character.IsDestroyed)
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            var weaponState = this.characterPrivateState.WeaponState;
            if (!(weaponState.ProtoWeapon is IProtoItemWeaponRanged))
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            this.Visibility = Visibility.Visible;

            var weaponCache = weaponState.WeaponCache;
            if (weaponCache is null)
            {
                // calculate new weapon cache
                WeaponSystem.SharedRebuildWeaponCache(this.character, weaponState);
                weaponCache = weaponState.WeaponCache;
            }

            var range = weaponCache.RangeMax;
            var angleRad = this.characterPrivateState.Input.RotationAngleRad;

            var relativeTargetPosition = new Vector2D(range * Math.Cos(angleRad),
                                                      range * Math.Sin(angleRad));

            var absoluteTargetPosition = relativeTargetPosition
                                         + this.character.Position
                                         + (0, this.character.ProtoCharacter.CharacterWorldWeaponOffsetRanged);
            var screenTargetPosition = Api.Client.Input.WorldToScreenPosition(absoluteTargetPosition);
            var result = this.layoutRoot.PointFromScreen(new Point(screenTargetPosition.X, screenTargetPosition.Y));

            this.ScreenTargetX = (float)result.X;
            this.ScreenTargetY = (float)result.Y;
        }
    }
}