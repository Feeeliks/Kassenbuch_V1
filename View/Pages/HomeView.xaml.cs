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
    /// Interaktionslogik für HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            DataContext = HomeViewModel.Instance;
        }

        private void cboxOpenProject_SelectionChanged (object sender, RoutedEventArgs e)
        {
            HomeViewModel.Instance.AktuellesProjekt = ((ComboBoxItem)cboxAktuellesProjekt.SelectedItem).Content.ToString();
        }
    }
}
