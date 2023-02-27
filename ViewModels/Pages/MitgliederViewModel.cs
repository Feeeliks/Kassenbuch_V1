using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class MitgliederViewModel : ViewModelBase
    {
        private static MitgliederViewModel instance;

        public MitgliederViewModel()
        {

        }

        public static MitgliederViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MitgliederViewModel();
                }
                return instance;
            }
        }
    }
}
