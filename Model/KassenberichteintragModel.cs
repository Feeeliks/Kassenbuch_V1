using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.Model
{
    class KassenberichteintragModel
    {
        public int Id { get; set; }
        public string Gruppe { get; set; }
        public double Betrag { get; set; }
        public string EinnahmeAusgabe { get; set; }
    }
}
