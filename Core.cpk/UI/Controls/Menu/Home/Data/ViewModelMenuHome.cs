namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Home.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.UpdatesHistory;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuHome : BaseViewModel
    {
        private const string AtomicTorchForumsRssFeed =
            "http://forums.atomictorch.com/index.php?action=.xml;type=rss2&c=3&limit=15";

        private const string AtomicTorchNewsRssFeed = //"http://atomictorch.com/blog/rss";
            "https://steamcommunity.com/games/829590/rss/";

        private ServerViewModelsProvider serverViewModelsProvider;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public ViewModelMenuHome()
        {
            if (IsDesignTime)
            {
                // some test RSS content for XAML design preview
                var rssFeedEntries = new List<RssFeedEntry>()
                {
                    new("Test title", "Test description", "Lurler", string.Empty, DateTime.Now),
                    new(
                        "Test title 2",
                        "Test description 2",
                        "Lurler",
                        string.Empty,
                        DateTime.Today.AddDays(-1)),
                    new(
                        "Test title 2",
                        "Test description 3",
                        "Lurler",
                        string.Empty,
                        DateTime.Today.AddDays(-2))
                };

                this.OnNewsRssFeedResult(rssFeedEntries);
                this.OnForumsRssFeedResult(rssFeedEntries);
                return;
            }

            this.serverViewModelsProvider = ServerViewModelsProvider.Instance;
            var serversProvider = Client.MasterServer.ServersProvider;

            this.HistoryServers = new ViewModelServersList(
                new MultiplayerMenuServersController(serversProvider.History,
                                                     this.serverViewModelsProvider),
                this.OnSelectedServerChanged);
            this.HistoryServers.IsActive = true;

            Client.Core.RequestRssFeed(
                AtomicTorchNewsRssFeed,
                this.OnNewsRssFeedResult);

            Client.Core.RequestRssFeed(
                AtomicTorchForumsRssFeed,
                this.OnForumsRssFeedResult);
        }

        public BaseCommand CommandShowFeaturesSlideshow
            => new ActionCommand(this.ExecuteCommandShowFeaturesSlideshow);

        public SuperObservableCollection<RssFeedEntry> ForumsItemsList { get; }
            = new();

        public ViewModelServersList HistoryServers { get; }

        public SuperObservableCollection<RssFeedEntry> NewsItemsList { get; }
            = new();

        public string UpdateReleaseDateText => UpdatesHistoryEntries.Entries.FirstOrDefault()?
                                                                    .DateValue
                                                                    .ToString("MMMM yyyy",
                                                                              CultureInfo.CurrentUICulture)
                                                                    .ToUpperInvariant();

        public string UpdateTitle => UpdatesHistoryEntries.Entries.FirstOrDefault()?.Title;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.serverViewModelsProvider = null;
        }

        private void ExecuteCommandShowFeaturesSlideshow()
        {
            FeaturesSlideshow.IsDisplayed = true;
        }

        private void OnForumsRssFeedResult(List<RssFeedEntry> rssFeedEntries)
        {
            this.ForumsItemsList.ClearAndAddRange(rssFeedEntries);
        }

        private void OnNewsRssFeedResult(List<RssFeedEntry> rssFeedEntries)
        {
            this.NewsItemsList.ClearAndAddRange(rssFeedEntries);
        }

        private void OnSelectedServerChanged(ViewModelServerInfoListEntry viewModelServerInfoListEntry)
        {
            //this.SelectedServer = viewModelServerInfoListEntry;
        }
    }
}