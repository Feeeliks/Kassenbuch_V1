using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.Model
{
    public class ProjectModel
    {
        public int ID { get; set; }
        public int Name { get; set; }

        public int PreviousProject()
        {
            return this.Name - 1;
        }
    }
}
