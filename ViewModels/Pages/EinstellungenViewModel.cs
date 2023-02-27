using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class EinstellungenViewModel : ViewModelBase
    {
        private static EinstellungenViewModel instance;

        public EinstellungenViewModel()
        {

        }

        public static EinstellungenViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EinstellungenViewModel();
                }
                return instance;
            }
        }
    }
}
