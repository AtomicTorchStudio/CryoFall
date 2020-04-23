namespace AtomicTorch.CBND.CoreMod.Systems.EmojiSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class EmojiSystem
    {
        public static IReadOnlyList<EmojisCategory> EmojiCategories { get; private set; }

        public readonly struct EmojiData
        {
            public readonly string TextId;

            public readonly string UnicodeId;

            public EmojiData(string unicodeId, string textId)
            {
                this.UnicodeId = unicodeId;
                this.TextId = textId;
            }
        }

        public class EmojisCategory
        {
            public readonly IReadOnlyList<EmojiData> Emojis;

            public EmojiData HeaderEmoji;

            public EmojisCategory(IReadOnlyList<EmojiData> emojis, EmojiData headerEmoji)
            {
                this.Emojis = emojis;
                this.HeaderEmoji = headerEmoji;
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                var categories = new List<EmojisCategory>();
                EmojiCategories = categories;

                foreach (var filePath in Api.Shared.FindFiles("Scripts/Systems/EmojiSystem/Presets/")
                                            .EnumerateAndDispose())
                {
                    if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var filterFileContent = Api.Shared.LoadTextFileContent(filePath);
                    var category = LoadEmojis(filterFileContent);
                    categories.Add(category);
                }

                Api.Logger.Important(
                    $"Emojis loaded: {EmojiCategories.Count} categories, {EmojiCategories.Sum(c => c.Emojis.Count)}emojis total");

                static EmojisCategory LoadEmojis(string filterFileContent)
                {
                    var emojiList = new List<EmojiData>();

                    var sb = new StringBuilder();
                    sb.Clear();
                    foreach (var c in filterFileContent)
                    {
                        switch (c)
                        {
                            case '\r':
                                // skip
                                break;

                            case '\n':
                                // newline
                                CommitEntry();
                                break;

                            default:
                                sb.Append(c);
                                break;
                        }
                    }

                    CommitEntry();

                    var headerEmoji = emojiList[0];
                    return new EmojisCategory(emojiList, headerEmoji);

                    void CommitEntry()
                    {
                        if (sb.Length == 0)
                        {
                            return;
                        }

                        try
                        {
                            // TODO: improve format to add text codes for emojis
                            var unicodeId = sb.ToString();
                            var textId = unicodeId;
                            var entry = new EmojiData(unicodeId, textId);
                            emojiList.Add(entry);
                        }
                        finally
                        {
                            sb.Clear();
                        }
                    }
                }
            }
        }
    }
}