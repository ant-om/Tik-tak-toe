using System.Net.Sockets;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using AvaloniaApplication1.ViewModels;
using ReactiveUI;

namespace AvaloniaApplication1.Views;

public partial class ConnectionMenuWindow : ReactiveWindow<ConnectionMenuWindowViewModel>
{
    public ConnectionMenuWindow()
    {
        InitializeComponent();
        this.WhenActivated(action =>
            action(ViewModel!.ShowGameWindow.RegisterHandler(OpenGameWindow)));
    }

    private async Task OpenGameWindow(IInteractionContext<(Socket, CellState), Unit> context)
    {
        await new GameWindow
        {
            DataContext = new GameWindowViewModel(context.Input.Item1, context.Input.Item2)
        }.ShowDialog(this);
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.PhysicalKey != PhysicalKey.Enter) return;
        var vm = (ConnectionMenuWindowViewModel)DataContext!;
        vm.IsDialogOpen = false;
        vm.Connect();
    }
}