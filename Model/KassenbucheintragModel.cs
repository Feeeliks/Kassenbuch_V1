using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_LoginForm.Model
{
    /*public class KassenbucheintragModel
    {
        public int Id { get; set; }
        public string Datum { get; set; }
        public int Buchungsnummer { get; set; }
        public string Position { get; set; }
        public bool KontoKasse { get; set; }
        public bool EinnahmeAusgabe { get; set; }
        public int Steuerklasse { get; set; }
    }*/

    public class KassenbucheintragModel
    {
        public int Id { get; set; }
        public string Datum { get; set; }
        public string Position { get; set; }
        public string Kontobeleg { get; set; }
        public int Belegnummer { get; set; }
        public double Betrag { get; set; }
        public double KontoEinnahme { get; set; }
        public double KontoAusgabe { get; set; }
        public double KasseEinnahme { get; set; }
        public double KasseAusgabe { get; set; }
        public double Steuer1Einnahme { get; set; }
        public double Steuer1Ausgabe { get; set; }
        public double Steuer2Einnahme { get; set; }
        public double Steuer2Ausgabe { get; set; }
        public double Steuer3Einnahme { get; set; }
        public double Steuer3Ausgabe { get; set; }
        public double Steuer4Einnahme { get; set; }
        public double Steuer4Ausgabe { get; set; }
        public string Gruppe { get; set; }

        public string MonthGroup
        {
            get 
            { 
                return (DateTime.ParseExact(Datum, "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("MMMM", new CultureInfo("de-DE")));
            }
        }

        public string KontobelegGroup
        {
            get { return Kontobeleg == "B" ? "Konto" : "Kasse"; }
        }

        public string EinnahmeAusgabeGroup
        {
            get { return (KasseEinnahme > 0 || KontoEinnahme > 0) ? "Einnahme" : "Ausgabe"; }
        }

    }
}
