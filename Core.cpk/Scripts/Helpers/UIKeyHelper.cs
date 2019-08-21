namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System.Windows.Input;

    public static class UIKeyHelper
    {
        public static bool IsModifier(this Key key)
        {
            return key == Key.LeftShift
                   || key == Key.RightShift
                   || key == Key.LeftCtrl
                   || key == Key.RightCtrl
                   || key == Key.LeftAlt
                   || key == Key.RightAlt
                   || key == Key.LWin // Win keys are not supported by NoesisGUI yet
                   || key == Key.RWin;
        }
    }
}