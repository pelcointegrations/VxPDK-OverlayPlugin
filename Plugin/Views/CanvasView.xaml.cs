using PluginNs.ViewModels;
using System.Windows.Controls;

namespace PluginNs.Views
{
    /// <summary>
    /// Interaction logic for CanvasView.xaml
    /// </summary>
    partial class CanvasView : UserControl
    {
        public CanvasView(CanvasViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
