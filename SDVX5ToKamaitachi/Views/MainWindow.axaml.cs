using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SDVX5ToKamaitachi.ViewModels;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace SDVX5ToKamaitachi.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated((_) =>
            {
                ViewModel!.GetFile.RegisterHandler(GetFile);
            });
        }

        private async Task GetFile(InteractionContext<Unit, string> ctx)
        {
            var dialog = new OpenFileDialog
            {
                Filters = new()
                {
                    new FileDialogFilter()
                    {
                        Name = "e-amusement CSV",
                        Extensions = new() { "csv" }
                    }
                },
                AllowMultiple = false
            };
            var file = await dialog.ShowAsync((Window)VisualRoot).ConfigureAwait(false);
            ctx.SetOutput(file.FirstOrDefault());
        }
    }
}
