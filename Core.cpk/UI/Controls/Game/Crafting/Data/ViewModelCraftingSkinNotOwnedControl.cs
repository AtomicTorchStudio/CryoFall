namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelCraftingSkinNotOwnedControl : BaseViewModel
    {
        private IProtoItemWithSkinData protoItemSkin;

        public string BaseProtoItemName => this.protoItemSkin?.BaseProtoItem.Name;

        public BaseCommand CommandBrowseSkins
            => new ActionCommand(ExecuteCommandBrowseSkins);

        public BaseCommand CommandNavigateToSupporterPackPage
            => new ActionCommand(ExecuteCommandNavigateToSupporterPackPage);

        public bool IsBrowseSkinsButtonVisible
            => this.SkinsPool == SkinsPool.InAppPurchase;

        public bool IsOwned { get; private set; } = true;

        public bool IsSupporterPackSkin => this.SkinsPool == SkinsPool.SupporterPack;

        public string MessageHowToAccess
        {
            get
            {
                switch (this.SkinsPool)
                {
                    case SkinsPool.NotAvailable:
                    case SkinsPool.EventGranted:
                        return CoreStrings.Skin_AccessLimitedEdition;

                    case SkinsPool.Free:
                        // not possible as free skins are always available
                        return null;

                    case SkinsPool.InAppPurchase:
                        // it's available in store so no need for a message here (only a button to browse store)
                        return null;

                    case SkinsPool.SupporterPack:
                        return CoreStrings.Skin_AccessSupporterPack;

                    case SkinsPool.EarnByPlaying:
                        return CoreStrings.Skin_AccessUnlockByPlaying;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string Name => this.protoItemSkin?.Name;

        public IProtoItemWithSkinData ProtoItemSkin
        {
            get => this.protoItemSkin;
            set
            {
                if (this.protoItemSkin == value)
                {
                    return;
                }

                this.protoItemSkin = value;
                if (value is null)
                {
                    // no skin selected
                    this.IsOwned = true; // we don't want to display the "not owned" UI in this case
                    this.SkinsPool = SkinsPool.NotAvailable;
                }
                else
                {
                    var skinData = Api.Client.Microtransactions.GetSkinData((ushort)this.protoItemSkin.SkinId);
                    this.IsOwned = skinData.IsOwned;
                    this.SkinsPool = skinData.Pool;
                }

                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.Name));
                this.NotifyPropertyChanged(nameof(this.BaseProtoItemName));
                this.NotifyPropertyChanged(nameof(this.MessageHowToAccess));
                this.NotifyPropertyChanged(nameof(this.IsBrowseSkinsButtonVisible));
                this.NotifyPropertyChanged(nameof(this.IsSupporterPackSkin));
            }
        }

        public SkinsPool SkinsPool { get; private set; }

        private static void ExecuteCommandBrowseSkins()
        {
            SkinsMenuOverlay.IsDisplayed = true;
        }

        private static void ExecuteCommandNavigateToSupporterPackPage()
        {
            Api.Client.SteamApi.OpenBuySupporterPackPage();
        }
    }
}