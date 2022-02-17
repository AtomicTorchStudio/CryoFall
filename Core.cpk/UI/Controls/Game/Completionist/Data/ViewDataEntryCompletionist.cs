namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewDataEntryCompletionist
    {
        private ViewModelWindowCompletionist.CompletionistEntryState state;

        internal ViewDataEntryCompletionist(
            IProtoEntity prototype,
            ActionCommandWithParameter commandClaimReward)
        {
            this.Prototype = prototype;
            this.CommandClaimReward = commandClaimReward;
            this.State = ViewModelWindowCompletionist.CompletionistEntryState.Undiscovered;
            this.RefreshIconTexture();
        }

        public bool CanClaimReward =>
            this.State == ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable;

        public ActionCommandWithParameter CommandClaimReward { get; }

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.IconTexture);

        public ITextureResource IconTexture { get; private set; }

        public bool IsRewardClaimed => this.State == ViewModelWindowCompletionist.CompletionistEntryState.RewardClaimed;

        public bool IsUndiscovered => this.State == ViewModelWindowCompletionist.CompletionistEntryState.Undiscovered;

        public string NotificationMessage
            => string.Format(this.NotificationMessageFormat, this.Title);

        public string NotificationMessageFormat
            => this.Prototype switch
            {
                IProtoItemFish     => ViewModelWindowCompletionist.Notification_FishDiscovered_MessageFormat,
                IProtoItemFood     => ViewModelWindowCompletionist.Notification_FoodDiscovered_MessageFormat,
                IProtoCharacterMob => ViewModelWindowCompletionist.Notification_CreatureDiscovered_MessageFormat,
                IProtoObjectLoot   => ViewModelWindowCompletionist.Notification_LootDiscovered_MessageFormat,
                IProtoEvent        => ViewModelWindowCompletionist.Notification_EventParticipated_MessageFormat,
                _                  => throw new Exception("Unknown prototype: " + this.Prototype)
            };

        public IProtoEntity Prototype { get; }

        public string Title => this.Prototype.Name;

        public FrameworkElement TooltipControl
            => this.State != ViewModelWindowCompletionist.CompletionistEntryState.Undiscovered
               && this.Prototype is IProtoItem protoItem
                   ? ItemTooltipControl.Create(protoItem)
                   : null;

        internal ViewModelWindowCompletionist.CompletionistEntryState State
        {
            get => this.state;
            set
            {
                if (this.state == value)
                {
                    return;
                }

                this.state = value;
                // state changed, most likely we need a new icon texture
                this.RefreshIconTexture();
            }
        }

        private ITextureResource GetIconTexture()
        {
            ITextureResource iconTexture;
            try
            {
                iconTexture = GetPrototypeIconTexture();
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
                iconTexture = TextureResource.NoTexture;
            }

            if (!this.IsUndiscovered)
            {
                return iconTexture;
            }

            var originalIcon = iconTexture;
            iconTexture = new ProceduralTexture(
                "Completionist icon locked " + this.Prototype.ShortId,
                proceduralTextureRequest =>
                    CompletionistLockedEntryIconHelper.CreateIconForLockedEntry(
                        proceduralTextureRequest,
                        originalIcon),
                isTransparent: true,
                isUseCache: true,
                dependsOn: new[] { originalIcon });

            return iconTexture;

            ITextureResource GetPrototypeIconTexture()
            {
                return this.Prototype switch
                {
                    IProtoItemFood protoItem => protoItem.Icon,
                    IProtoItemFish protoItemFish => protoItemFish.Icon,
                    IProtoCharacterMob protoCharacterMob => protoCharacterMob.Icon,
                    IProtoStaticWorldObject protoStaticWorldObject => protoStaticWorldObject.Icon,
                    IProtoEvent protoEvent => protoEvent.Icon,
                    _ => throw new Exception("Unknown prototype: " + this.Prototype)
                };
            }
        }

        private void RefreshIconTexture()
        {
            this.IconTexture = this.GetIconTexture();
        }
    }
}