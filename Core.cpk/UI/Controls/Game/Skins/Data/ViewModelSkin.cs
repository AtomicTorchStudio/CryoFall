namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSkin : BaseViewModel
    {
        private readonly SkinEntry skinEntry;

        private byte discountPercent;

        public ViewModelSkin(IProtoItemWithSkinData protoItemSkin, SkinEntry skinEntry)
        {
            this.skinEntry = skinEntry;
            this.ProtoItemSkin = protoItemSkin;
            this.IsOwned = Client.Microtransactions.IsSkinOwned((ushort)protoItemSkin.SkinId);
            this.PriceOriginalInPlayerCurrency = skinEntry.PriceCentsInPlayerCurrency / 100m;
            this.DiscountPercent = skinEntry.DiscountPercent;
            this.SkinsPool = skinEntry.Pool;
        }

        public string AvailableToCraftIn
        {
            get
            {
                var techGroup =
                    ((IProtoItemWithRecipeData)this.ProtoItemSkin.BaseProtoItem)?
                    .ListedInRecipes.FirstOrDefault()?
                    .ListedInTechNodes.FirstOrDefault()?
                    .Group;

                if (techGroup is null)
                {
                    return null;
                }

                return string.Format(CoreStrings.Item_AvailableInTechTier,
                                     ViewModelTechTier.GetTierText(techGroup.Tier));
            }
        }

        public string BaseItemName => this.ProtoItemSkin.BaseProtoItem.Name;

        public BaseCommand CommandBuy
            => new ActionCommand(this.ExecuteCommandBuy);

        public byte DiscountPercent
        {
            get => this.discountPercent;
            set
            {
                if (this.discountPercent == value)
                {
                    return;
                }

                this.discountPercent = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.DiscountText));
                this.NotifyPropertyChanged(nameof(this.PriceWithDiscountInPlayerCurrency));
                this.NotifyPropertyChanged(nameof(this.PriceWithDiscountInPlayerCurrencyText));
            }
        }

        public string DiscountText => !this.IsOwned
                                      && this.DiscountPercent > 0
                                          ? $"-{this.DiscountPercent}%"
                                          : null;

        public TextureBrush IconBaseItem
            => Api.Client.UI.GetTextureBrush(this.ProtoItemSkin.BaseProtoItem.Icon);

        public TextureBrush IconSkin
            => Api.Client.UI.GetTextureBrush(this.ProtoItemSkin.Icon);

        public bool IsOwned { get; set; }

        public bool IsSkinEntryExist => this.skinEntry.Id > 0;

        public decimal PriceOriginalInPlayerCurrency { get; }

        public string PriceOriginalInPlayerCurrencyText
            => ToPrice(this.PriceOriginalInPlayerCurrency);

        public string PriceWithDiscountInPlayerCurrencyText
            => ToPrice(this.PriceWithDiscountInPlayerCurrency);

        public IProtoItemWithSkinData ProtoItemSkin { get; }

        public ushort SkinId => (ushort)this.ProtoItemSkin.SkinId;

        public string SkinIdName => this.ProtoItemSkin.SkinId.ToString();

        public string SkinName => this.ProtoItemSkin.Name;

        public SkinsPool SkinsPool { get; set; }

        private decimal PriceWithDiscountInPlayerCurrency
            => Client.Microtransactions.ApplyDiscountToPrice(
                this.PriceOriginalInPlayerCurrency,
                this.DiscountPercent,
                Client.Microtransactions.CurrencyGranularity);

        public override string ToString()
        {
            return this.ProtoItemSkin.ToString();
        }

        private static string ToPrice(decimal price)
        {
            return Client.Microtransactions.FormatPrice(price);
        }

        private void ExecuteCommandBuy()
        {
            if (!Client.Microtransactions.AreSkinsSupported)
            {
                throw new Exception("Skins are not supported in current version of the client");
            }

            if (!Api.Client.SteamApi.IsSteamOverlayEnabled)
            {
                DialogWindow.ShowMessage(
                    CoreStrings.SteamOverlayDisabled_Title,
                    CoreStrings.SteamOverlayDisabled_Message,
                    closeByEscapeKey: true);
                return;
            }

            Api.Client.SteamApi.InitMicrotransaction((ushort)this.ProtoItemSkin.SkinId,
                                                     string.Format(CoreStrings.Skins_SkinNameFormat,
                                                                   this.ProtoItemSkin.Name,
                                                                   this.ProtoItemSkin.BaseProtoItem.Name));
        }
    }
}