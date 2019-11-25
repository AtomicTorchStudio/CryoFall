namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using static CharacterMoveModes;
    using static ClientComponents.Input.ClientInputManager;

    public class ComponentPlayerInputUpdater : ClientComponent
    {
        private const bool IsAllowMovementWithOpenedWindow = true;

        private static readonly IClientStorage StorageRunMode
            = Client.Storage.GetStorage("Gameplay/RunToggle");

        private ICharacter character;

        private CharacterInput characterInput;

        private ClientInputContext inputContext;

        private bool runToggle;

        public void Setup(ICharacter character)
        {
            this.character = character;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.inputContext = ClientInputContext
                                .Start("Player character Movement")
                                .HandleAll(this.UpdateInput);

            StorageRunMode.TryLoad(out this.runToggle);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.inputContext?.Stop();
            this.inputContext = null;
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private static bool CheckIsInputSuppressed()
        {
            if (ClientCurrentCharacterHelper.PublicState?.IsDead ?? true)
            {
                // dead character
                return true;
            }

            if (!MainMenuOverlay.IsHidden)
            {
                // main menu displayed - suppress input
                return true;
            }

            if (!IsAllowMovementWithOpenedWindow
                && WindowsManager.OpenedWindowsCount > 1)
            {
                // any window opened and we don't allow movement with the opened window
                return true;
            }

            return false;
        }

        private float GetRotationAngleRad()
        {
            var mouseWorldPosition = Api.Client.Input.MouseWorldPosition;
            var deltaPositionToMouseCursor = this.character.Position
                                             + (0, this.character.ProtoCharacter.CharacterWorldWeaponOffsetRanged)
                                             - mouseWorldPosition;

            //var mouseScreenPosition = Api.Client.Input.MouseScreenPosition;
            //var screenSize = Api.Client.Rendering.ViewportSize;
            //var screenCenterPosition = (screenSize.X * 0.5f, screenSize.Y * 0.5f);
            //var deltaPositionToMouseCursor = (screenCenterPosition.X - mouseScreenPosition.X,
            //                                              mouseScreenPosition.Y - screenCenterPosition.Y);

            var rotationAngleRad = Math.Abs(Math.PI
                                            + Math.Atan2(deltaPositionToMouseCursor.Y,
                                                         deltaPositionToMouseCursor.X));

            return (float)rotationAngleRad;
        }

        private void SetInput(CharacterMoveModes moveModes, float rotationAngleRad)
        {
            this.characterInput.MoveModes = moveModes;
            this.characterInput.RotationAngleRad = rotationAngleRad;

            //// uncomment visualize rotation angle correctness with Physics Visualizer
            //var fromPosition = this.character.Position
            //                   + (0, this.character.ProtoCharacter.CharacterWorldWeaponOffsetRanged);
            //var toPosition = fromPosition + (30, 0).RotateRad(rotationAngleRad);
            //Client.Characters.CurrentPlayerCharacter.PhysicsBody.PhysicsSpace.TestLine(
            //    fromPosition,
            //    toPosition,
            //    collisionGroup: null);

            if (!this.characterInput.HasChanged)
            {
                return;
            }

            this.characterInput.SetChanged(false);

            var command = new CharacterInputUpdate(
                this.characterInput.MoveModes,
                this.characterInput.RotationAngleRad);

            ((PlayerCharacter)this.character.ProtoCharacter)
                .ClientSetInput(command);
        }

        private void UpdateInput()
        {
            var move = None;
            var angleRad = this.characterInput.RotationAngleRad;

            if (CheckIsInputSuppressed()
                || !Api.Client.CurrentGame.IsSuccessfullyProcessingPhysics)
            {
                // set default input
                this.SetInput(move, angleRad);
                return;
            }

            if (IsButtonHeld(GameButton.MoveUp))
            {
                move |= Up;
            }

            if (IsButtonHeld(GameButton.MoveDown))
            {
                move |= Down;
            }

            if (IsButtonHeld(GameButton.MoveLeft))
            {
                move |= Left;
            }

            if (IsButtonHeld(GameButton.MoveRight))
            {
                move |= Right;
            }

            if (IsButtonDown(GameButton.RunToggle))
            {
                this.runToggle = !this.runToggle;
                StorageRunMode.Save(this.runToggle, writeToLog: false);
            }

            if (this.runToggle)
            {
                move |= ModifierRun;
            }

            if (IsButtonHeld(GameButton.RunTemporary))
            {
                move ^= ModifierRun;
            }

            ConsumeButton(GameButton.MoveUp);
            ConsumeButton(GameButton.MoveDown);
            ConsumeButton(GameButton.MoveLeft);
            ConsumeButton(GameButton.MoveRight);
            ConsumeButton(GameButton.RunToggle);
            ConsumeButton(GameButton.RunTemporary);

            if ((move & Up) != 0
                && (move & Down) != 0)
            {
                // cannot move up and down simultaneously
                move &= ~(Up | Down);
            }

            if ((move & Left) != 0
                && (move & Right) != 0)
            {
                // cannot move left and right simultaneously
                move &= ~(Left | Right);
            }

            if ((move & (Left | Up | Right | Down))
                == None)
            {
                // cannot run when not moving
                move = None;
            }

            angleRad = this.GetRotationAngleRad();

            this.SetInput(move, angleRad);
        }
    }
}