using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GoldenStandard.Views
{
    public partial class EditProductView : UserControl
    {
        public EditProductView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}