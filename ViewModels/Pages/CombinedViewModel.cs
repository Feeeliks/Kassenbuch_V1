using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_LoginForm.Model;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class CombinedViewModel
    {
        public HomeViewModel Home { get; set; }
        public KassenbuchViewModel Kassenbuch { get; set; }

        public CombinedViewModel()
        {
            Home = HomeViewModel.Instance;
            Kassenbuch = KassenbuchViewModel.Instance;
        }
    }
}
