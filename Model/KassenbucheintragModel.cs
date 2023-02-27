using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.Model
{
    public class KassenbucheintragModel
    {
        public int Id { get; set; }
        public string Datum { get; set; }
        public int Buchungsnummer { get; set; }
        public string Position { get; set; }
        public bool KontoKasse { get; set; }
        public bool EinnahmeAusgabe { get; set; }
        public int Steuerklasse { get; set; }
    }
}
