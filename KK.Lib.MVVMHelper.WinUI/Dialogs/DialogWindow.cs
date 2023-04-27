using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace KK.Lib.MVVMHelper.Dialogs;

public class DialogWindow : ContentDialog, IDialogWindow
{
    public DialogWindow()
    {
        BindingOperations.SetBinding(this, TitleProperty, new Binding() { Source = DataContext, Path = new("Title"), });

        PrimaryButtonText = "Close";
    }

    event RoutedEventHandler IDialogWindow.Loaded
    {
        add => Loaded += value;
        remove => Loaded -= value;
    }

    event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> IDialogWindow.Closing
    {
        add => Closing += value;
        remove => Closing -= value;
    }

    event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> IDialogWindow.Closed
    {
        add => Closed += value;
        remove => Closed -= value;
    }
}
