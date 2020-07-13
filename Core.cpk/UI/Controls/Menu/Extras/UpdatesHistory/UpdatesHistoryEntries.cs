namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.UpdatesHistory
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class UpdatesHistoryEntries
    {
        public static readonly IReadOnlyList<Entry> Entries = new[]
        {
            new Entry("A27—Total Overhaul Update",
                      new DateTime(2020, month: 7, day: 16),
                      "Updates/A27.jpg"),

            new Entry("A26—Pragmium War Update",
                      new DateTime(2020, month: 4, day: 22),
                      "Updates/A26.jpg"),

            new Entry("A25—Balance Update",
                      new DateTime(2020, month: 2, day: 1),
                      "Updates/A25.jpg"),

            new Entry("A24—Mechanized Update",
                      new DateTime(2019, month: 11, day: 25),
                      "Updates/A24.jpg"),

            new Entry("Halloween Event",
                      new DateTime(2019, month: 10, day: 25),
                      "Updates/A23-halloween.jpg"),

            new Entry("A23—Electricity Update",
                      new DateTime(2019, month: 8, day: 12),
                      "Updates/A23.jpg"),

            new Entry("A22—PvE Update",
                      new DateTime(2019, month: 5, day: 16),
                      "Updates/A22.jpg"),

            new Entry("A21—Survivors Update",
                      new DateTime(2019, month: 4, day: 27),
                      "Updates/A21.jpg"),

            new Entry("Early Access Release",
                      new DateTime(2019, month: 4, day: 3),
                      "Updates/A20.jpg")
        };

        public class Entry
        {
            public Entry(string title, DateTime date, string textureImagePath)
            {
                this.Title = title;
                this.TextureImagePath = textureImagePath;
                this.Date = date.ToString("MMMM yyyy", CultureInfo.CurrentUICulture);
                this.DateValue = date;
            }

            public string Date { get; }

            public DateTime DateValue { get; }

            public TextureBrush Image
                => Api.Client.UI.GetTextureBrush(new TextureResource(this.TextureImagePath));

            public string TextureImagePath { get; }

            public string Title { get; }
        }
    }
}