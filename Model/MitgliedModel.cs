using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.Model
{
    public class MitgliedModel
    {
        public int Id { get; set; }
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string Mitgliedsstatus { get; set; }
        public double Mitgliedsbeitrag { get; set; }
        public string Bezahlstatus { get; set; }
    }
}
