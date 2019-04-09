namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class NameEntry
    {
        public NameEntry(
            string name,
            BaseCommand commandRemove,
            Visibility removeButtonVisibility)
        {
            this.Name = name;
            this.CommandRemove = commandRemove;
            this.RemoveButtonVisibility = removeButtonVisibility;
        }

        public BaseCommand CommandRemove { get; }

        public string Name { get; }

        public Visibility RemoveButtonVisibility { get; }
    }
}