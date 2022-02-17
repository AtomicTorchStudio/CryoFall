namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelSkinMenu : BaseViewModel
    {
        public ViewModelSkinMenu()
        {
            Client.Microtransactions.SkinsDataReceived += this.SkinsDataReceivedHandler;
            this.Reset();
        }

        public bool IsEditor => Api.IsEditor;

        public bool IsFeaturedTabSelected { get; set; } = true;

        public bool IsListLoading { get; private set; }

        public IReadOnlyList<ViewModelSkin> SkinsAll { get; set; }

        public IReadOnlyList<ViewModelSkin> SkinsEquipmentToBuy { get; set; }

        public IReadOnlyList<ViewModelSkin> SkinsFeatured { get; set; }

        public IReadOnlyList<ViewModelSkin> SkinsOwned { get; set; }

        public IReadOnlyList<ViewModelSkin> SkinsWeaponsToBuy { get; set; }

        public void Reset()
        {
            Client.Microtransactions.RefreshSkinsData();
            this.ReloadList();
        }

        protected override void DisposeViewModel()
        {
            Client.Microtransactions.SkinsDataReceived -= this.SkinsDataReceivedHandler;
            base.DisposeViewModel();
        }

        private static void RandomizeStuff(List<ViewModelSkin> skins)
        {
            // apply random discount to 5 entries
            skins = skins.ToList();
            skins.Shuffle();

            for (var index = 0; index < Math.Min(5, skins.Count); index++)
            {
                skins[index].DiscountPercent = 50;
            }

            // apply random ownership for non-discounted items
            foreach (var skin in skins)
            {
                skin.IsOwned = true; // RandomHelper.RollWithProbability(0.35);
            }
        }

        private void ReloadList()
        {
            var service = Client.Microtransactions;
            List<ViewModelSkin> allSkins;

            if (!service.IsDataReceived)
            {
                this.IsListLoading = true;
                // reset skins
                allSkins = new List<ViewModelSkin>();
            }
            else
            {
                this.IsListLoading = false;
                allSkins = Api.FindProtoEntities<IProtoItemWithSkinData>()
                              .GroupBy(r => (IProtoItemWithSkinData)r.BaseProtoItem)
                              .Where(g => g.Key is not null
                                          && ((IProtoItemWithRecipeData)g.Key).ListedInRecipes.Count > 0
                                          && ((IProtoItemWithRecipeData)g.Key).ListedInRecipes[0].ListedInTechNodes
                                                                              .Count
                                          > 0)
                              .OrderByDescending(g => g.Key is IProtoItemWeapon) // weapons go first
                              .SelectMany(g => g.Select(i => new ViewModelSkin(i,
                                                                               /*new SkinEntry(
                                                                              (ushort)i.SkinId,
                                                                              SkinsPool.InAppPurchase,
                                                                              isOwned: false,
                                                                              priceCentsInPlayerCurrency: 199,
                                                                              0)))*/
                                                                               service.GetSkinData((ushort)i.SkinId)))
                                                .ToList())
                              .Where(s => s.IsSkinEntryExist)
                              .ToList();
            }

            allSkins.SortBy(g => (ushort)g.ProtoItemSkin.SkinId);

            //RandomizeStuff(allSkins);

            this.DisposeCollection(this.SkinsAll);
            this.SkinsAll = allSkins.ToArray();

            this.SkinsWeaponsToBuy = allSkins.Where(vm => vm.ProtoItemSkin.BaseProtoItem is IProtoItemWeapon
                                                          && !vm.IsOwned
                                                          && vm.SkinsPool == SkinsPool.InAppPurchase)
                                             .ToList();

            this.SkinsEquipmentToBuy = allSkins.Where(vm => vm.ProtoItemSkin.BaseProtoItem is not IProtoItemWeapon
                                                            && !vm.IsOwned
                                                            && vm.SkinsPool == SkinsPool.InAppPurchase)
                                               .ToList();

            this.SkinsFeatured = allSkins.Where(vm => vm.DiscountPercent > 0
                                                      && !vm.IsOwned
                                                      && vm.SkinsPool == SkinsPool.InAppPurchase)
                                         .ToList();

            this.SkinsOwned = allSkins.Where(vm => vm.IsOwned
                                                   && vm.SkinsPool != SkinsPool.Free
                                                   && vm.SkinsPool != SkinsPool.EventGranted)
                                      .ToList();
        }

        private void SkinsDataReceivedHandler()
        {
            this.ReloadList();
        }
    }
}