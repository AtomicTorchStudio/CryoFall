namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Home.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ViewModelMenuHome : BaseViewModel
    {
        private const string AtomicTorchForumsRssFeed =
            "http://forums.atomictorch.com/index.php?action=.xml;type=rss2&c=3&limit=15";

        private const string AtomicTorchNewsRssFeed = "http://atomictorch.com/blog/rss";

        private ServerViewModelsProvider serverViewModelsProvider;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public ViewModelMenuHome()
        {
            if (IsDesignTime)
            {
                // some test RSS content for XAML design preview
                var rssFeedEntries = new List<RssFeedEntry>()
                {
                    new RssFeedEntry("Test title", "Test description", "Lurler", string.Empty, DateTime.Now),
                    new RssFeedEntry(
                        "Test title 2",
                        "Test description 2",
                        "Lurler",
                        string.Empty,
                        DateTime.Today.AddDays(-1)),
                    new RssFeedEntry(
                        "Test title 2",
                        "Test description 3",
                        "Lurler",
                        string.Empty,
                        DateTime.Today.AddDays(-2)),
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

        public SuperObservableCollection<RssFeedEntry> ForumsItemsList { get; }
            = new SuperObservableCollection<RssFeedEntry>();

        public ViewModelServersList HistoryServers { get; }

        public SuperObservableCollection<RssFeedEntry> NewsItemsList { get; }
            = new SuperObservableCollection<RssFeedEntry>();

        public Visibility VisibilityLoadingForumsRssFeed { get; set; }

        public Visibility VisibilityLoadingNewsRssFeed { get; set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.serverViewModelsProvider = null;
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