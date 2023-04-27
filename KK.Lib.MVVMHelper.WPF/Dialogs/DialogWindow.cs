using System.Windows;
using System.Windows.Data;

namespace KK.Lib.MVVMHelper.Dialogs;

internal class DialogWindow : Window, IDialogWindow
{
    public DialogWindow()
    {
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is IDialogAware dataContext)
        {
            Binding titleBinding = new(nameof(IDialogAware.Title))
            {
                Source = dataContext,
            };
            _ = SetBinding(TitleProperty, titleBinding);
        }
    }
}
