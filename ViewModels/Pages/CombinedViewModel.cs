using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_LoginForm.Model;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class CombinedViewModel : ViewModelBase
    {
        //Fields
        public HomeViewModel Home { get; set; }
        public KassenbuchViewModel Kassenbuch { get; set; }
        public KassenberichtViewModel Kassenbericht { get; set; }
        public ExportViewModel Export { get; set; }
        public MitgliederViewModel Mitglieder { get; set; }
        public EinstellungenViewModel Einstellungen { get; set; }

        //Constructor
        public CombinedViewModel()
        {
            Home = HomeViewModel.Instance;
            Kassenbuch = KassenbuchViewModel.Instance;
            Kassenbericht = KassenberichtViewModel.Instance;
            Export = ExportViewModel.Instance;
            Mitglieder = MitgliederViewModel.Instance;
            Einstellungen = EinstellungenViewModel.Instance;
        }
    }
}
