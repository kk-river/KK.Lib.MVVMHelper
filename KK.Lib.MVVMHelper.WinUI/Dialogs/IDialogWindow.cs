using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogWindow
{
    object Content { get; set; }

    XamlRoot XamlRoot { get; set; }

    public event RoutedEventHandler Loaded;
    public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs> Closing;
    public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> Closed;

    public IAsyncOperation<ContentDialogResult> ShowAsync();

    public void Hide();
}
