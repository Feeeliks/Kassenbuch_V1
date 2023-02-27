using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Input;
using WPF_LoginForm.Model;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class KassenberichtViewModel : ViewModelBase
    {

        //Singleton
        private static KassenberichtViewModel instance;

        public KassenberichtViewModel() { }

        public static KassenberichtViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KassenberichtViewModel();
                }
                return instance;
            }
        }
    }
}
