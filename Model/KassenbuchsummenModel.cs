using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_LoginForm.ViewModels.Pages;

namespace WPF_LoginForm.Model
{
    public class KassenbuchsummenModel
    {
        public string Monat { get; set; }
        public double SummeBetrag { get; set; }
        public double SummeKontoEin { get; set; }
        public double SummeKontoAus { get; set; }
        public double SummeKasseEin { get; set; }
        public double SummeKasseAus { get; set; }
        public double SummeSteuer1Ein { get; set; }
        public double SummeSteuer1Aus { get; set; }
        public double SummeSteuer2Ein { get; set; }
        public double SummeSteuer2Aus { get; set; }
        public double SummeSteuer3Ein { get; set; }
        public double SummeSteuer3Aus { get; set; }
        public double SummeSteuer4Ein { get; set; }
        public double SummeSteuer4Aus { get; set; }

        public double SummeKonto
        {
            get { return SummeKontoEin - SummeKontoAus + KassenbuchViewModel.Instance.Kassenstand[5]; }
        }

        public double SummeKasse
        {
            get { return SummeKasseEin - SummeKasseAus + KassenbuchViewModel.Instance.Kassenstand[6]; }
        }

        public double SummeSteuer1
        {
            get { return SummeSteuer1Ein - SummeSteuer1Aus; }
        }

        public double SummeSteuer2
        {
            get { return SummeSteuer2Ein - SummeSteuer2Aus; }
        }

        public double SummeSteuer3
        {
            get { return SummeSteuer3Ein - SummeSteuer3Aus; }
        }

        public double SummeSteuer4
        {
            get { return SummeSteuer4Ein - SummeSteuer4Aus; }
        }
    }
}
