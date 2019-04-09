namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Linq.Expressions;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class ViewModelExtensions
    {
        public static void SubscribePropertyChange<TViewModel, TValue>(
            this TViewModel viewModel,
            Expression<Func<TViewModel, TValue>> propertyGetFunc,
            Action<TValue> callback)
            where TViewModel : BaseViewModel
        {
            var property = TypeHelper.GetProperty(propertyGetFunc);
            // TODO: we don't have a way to unsubscribe from it
            viewModel.PropertyChanged += (sender, e) =>
                                         {
                                             if (e.PropertyName != property.Name)
                                             {
                                                 return;
                                             }

                                             // property changed!
                                             var value = property.GetValue(viewModel);
                                             callback((TValue)value);
                                         };
        }

        public static void SubscribePropertyChange<TViewModel, TValue>(
            this TViewModel viewModel,
            Expression<Func<TViewModel, TValue>> propertyGetFunc,
            Action callback)
            where TViewModel : BaseViewModel
        {
            viewModel.SubscribePropertyChange(propertyGetFunc, _ => callback());
        }
    }
}