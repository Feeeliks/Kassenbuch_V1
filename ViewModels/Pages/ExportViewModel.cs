using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class ExportViewModel : ViewModelBase
    {
        private static ExportViewModel instance;

        public ExportViewModel()
        {

        }

        public static ExportViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExportViewModel();
                }
                return instance;
            }
        }
    }
}
