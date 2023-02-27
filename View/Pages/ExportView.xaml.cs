using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_LoginForm.ViewModels.Pages;

namespace WPF_LoginForm.View.Pages
{
    /// <summary>
    /// Interaktionslogik für ExportView.xaml
    /// </summary>
    public partial class ExportView : UserControl
    {
        private HomeViewModel _HomeViewModel;

        public ExportView()
        {
            InitializeComponent();
            _HomeViewModel = HomeViewModel.Instance;
            DataContext = _HomeViewModel;
        }
    }
}
