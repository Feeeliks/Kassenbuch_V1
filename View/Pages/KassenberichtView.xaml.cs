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
    /// Interaktionslogik für KassenberichtView.xaml
    /// </summary>
    public partial class KassenberichtView : UserControl
    {
        private HomeViewModel _HomeViewModel;

        public KassenberichtView()
        {
            InitializeComponent();
            _HomeViewModel = HomeViewModel.Instance;
            DataContext = _HomeViewModel;
        }
    }
}
