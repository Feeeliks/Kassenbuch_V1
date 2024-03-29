﻿using System;
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
    /// Interaktionslogik für KassenbuchView.xaml
    /// </summary>
    public partial class KassenbuchView : UserControl
    {
        
        //private HomeViewModel _HomeViewModel;

        public KassenbuchView()
        {
            InitializeComponent();
            DataContext = new CombinedViewModel();
        }
       
        private void btnHideColumns_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 9; i <= 16; i++)
            {
                DataGridColumn column = dgKassenbuch.Columns[i];
                column.Visibility = column.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }

            //Infotext.Visibility = Infotext.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            
            if (Infozeile.Height.Value == 0)
            {
                Infozeile.Height = new GridLength(50); // Setze die Höhe auf den gewünschten Wert
            }
            else
            {
                Infozeile.Height = new GridLength(0); // Blende die Zeile aus
            }
        }
    }
}
