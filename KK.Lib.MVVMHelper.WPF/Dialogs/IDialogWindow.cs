using System.ComponentModel;
using System.Windows;

namespace KK.Lib.MVVMHelper.Dialogs;

public interface IDialogWindow
{
    object Content { get; set; }

    Window Owner { get; set; }

    void Show();

    bool? ShowDialog();

    void Close();

    object DataContext { get; set; }

    event RoutedEventHandler Loaded;

    event EventHandler Closed;

    event CancelEventHandler Closing;
}
