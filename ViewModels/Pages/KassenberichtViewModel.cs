using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Data;
using System.Windows.Input;
using WPF_LoginForm.Model;


namespace WPF_LoginForm.ViewModels.Pages
{
    public class KassenberichtViewModel : ViewModelBase
    {

        //Singleton
        private static KassenberichtViewModel instance;

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

        //Constructor
        public KassenberichtViewModel()
        {
            _gruppeEinnahmen = new ObservableCollection<KassenbucheintragModel>();
            _gruppeAusgaben = new ObservableCollection<KassenbucheintragModel>();
            GruppeEinnahmenView = CollectionViewSource.GetDefaultView(GruppeEinnahmen);
            GruppeAusgabenView = CollectionViewSource.GetDefaultView(GruppeAusgaben);
            KassenberichtGruppieren();

        }

        //Fields
        private ObservableCollection<KassenbucheintragModel> _gruppeEinnahmen;
        private ObservableCollection<KassenbucheintragModel> _gruppeAusgaben;
        private ICollectionView _gruppeEinnahmenView;
        private ICollectionView _gruppeAusgabenView;

        private double _summeEinnahmen;
        private double _summeAusgaben;
        private double _summeEinnahmenAusgaben;
        private double _summeHandkasse;
        private double _summeAusschankkasse;
        private double _summeKasse;
        private double _summeKonto;
        private double _summeKasseKonto;
        private double _summeEinnahmenAusgabenGesamt;

        //Properties
        public double SummeEinnahmen
        {
            get { return _summeEinnahmen; }
            set { _summeEinnahmen = value; OnPropertyChanged(nameof(SummeEinnahmen)); }
        }

        public double SummeAusgaben
        {
            get { return _summeAusgaben; }
            set { _summeAusgaben = value; OnPropertyChanged(nameof(SummeAusgaben)); }
        }

        public double SummeEinnahmenAusgaben
        {
            get { return _summeEinnahmenAusgaben; }
            set { _summeEinnahmenAusgaben = value; OnPropertyChanged(nameof(SummeEinnahmenAusgaben)); }
        }

        public double SummeEinnahmenAusgabenGesamt
        {
            get { return SummeEinnahmenAusgaben + KassenbuchViewModel.Instance.Kassenstand[4]; }
            set { _summeEinnahmenAusgabenGesamt = value; OnPropertyChanged(nameof(SummeEinnahmenAusgabenGesamt)); }
        }

        public double SummeHandkasse
        {
            get { return KassenbuchViewModel.Instance.Kassenstand[2]; }
            set { _summeHandkasse = value; OnPropertyChanged(nameof(SummeHandkasse)); }
        }

        public double SummeAusschankkasse
        {
            get { return KassenbuchViewModel.Instance.Kassenstand[3]; }
            set { _summeAusschankkasse = value; OnPropertyChanged(nameof(SummeAusschankkasse)); }
        }

        public double SummeKasse
        {
            get { return _summeKasse; }
            set { _summeKasse = value; OnPropertyChanged(nameof(SummeKasse)); }
        }

        public double SummeKonto
        {
            get { return KassenbuchViewModel.Instance.Kassenstand[1]; }
            set { _summeKonto = value; OnPropertyChanged(nameof(SummeKonto)); }
        }

        public double SummeKasseKonto
        {
            get { return KassenbuchViewModel.Instance.Kassenstand[0]; }
            set { _summeKasseKonto = value; OnPropertyChanged(nameof(SummeKasseKonto)); }
        }



        public ICollectionView GruppeEinnahmenView
        {
            get { return _gruppeEinnahmenView; }
            set { _gruppeEinnahmenView = value; OnPropertyChanged(nameof(GruppeEinnahmenView)); }
        }

        public ICollectionView GruppeAusgabenView
        {
            get { return _gruppeAusgabenView; }
            set { _gruppeAusgabenView = value; OnPropertyChanged(nameof(GruppeAusgabenView)); }
        }

        public ObservableCollection<KassenbucheintragModel> GruppeEinnahmen
        {
            get { return _gruppeEinnahmen; }
            set { _gruppeEinnahmen = value; OnPropertyChanged(nameof(GruppeEinnahmen)); }
        }

        public ObservableCollection<KassenbucheintragModel> GruppeAusgaben
        {
            get { return _gruppeAusgaben; }
            set { _gruppeAusgaben = value; OnPropertyChanged(nameof(GruppeAusgaben)); }
        }
            
        //Methods
        public void KassenberichtGruppieren()
        {
            if (KassenbuchViewModel.Instance.AktuellesKassenbuch == null) { return; }

            GruppeEinnahmen.Clear();
            GruppeAusgaben.Clear();

            SummeEinnahmen = 0.00;
            SummeAusgaben = 0.00;
            SummeEinnahmenAusgaben = 0.00;

            foreach (var eintrag in KassenbuchViewModel.Instance.AktuellesKassenbuch)
            {
                if (eintrag.EinnahmeAusgabeGroup == "Einnahme")
                {
                    var gruppe = GruppeEinnahmen.FirstOrDefault(g => g.Gruppe == eintrag.Gruppe);
                    if (gruppe == null)
                    {
                        gruppe = new KassenbucheintragModel { Gruppe = eintrag.Gruppe, Betrag = 0 };
                        GruppeEinnahmen.Add(gruppe);
                    }
                    gruppe.Betrag += eintrag.Betrag;
                    SummeEinnahmen += eintrag.Betrag;
                }
                else if (eintrag.EinnahmeAusgabeGroup == "Ausgabe")
                {
                    var gruppe = GruppeAusgaben.FirstOrDefault(g => g.Gruppe == eintrag.Gruppe);
                    if (gruppe == null)
                    {
                        gruppe = new KassenbucheintragModel { Gruppe = eintrag.Gruppe, Betrag = 0 };
                        GruppeAusgaben.Add(gruppe);
                    }
                    gruppe.Betrag += eintrag.Betrag;
                    SummeAusgaben += eintrag.Betrag;
                }
            }

            SummeEinnahmenAusgaben =  SummeEinnahmen - SummeAusgaben;

            // Herstellen der Verbindung zur Datenbank
            string tabellenname = HomeViewModel.Instance.AktuellesProjekt;
            string connectionString = "Data Source=database.db;Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Löschen der vorhandenen Daten in der Tabelle
                string clearTableQuery = "DELETE FROM \"" + tabellenname + "_Kassenbericht\"";
                SQLiteCommand clearTableCommand = new SQLiteCommand(clearTableQuery, connection);
                clearTableCommand.ExecuteNonQuery();

                // Schreiben der Daten in die Tabelle
                foreach (var eintrag in GruppeEinnahmen.Concat(GruppeAusgaben))
                {
                    string insertQuery = $"INSERT INTO \"" + tabellenname + "_Kassenbericht\" (Gruppe, Betrag, EinnahmeAusgabe) VALUES ('{eintrag.Gruppe}', '{eintrag.Betrag}', '{eintrag.EinnahmeAusgabeGroup}')";
                    SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection);
                    insertCommand.ExecuteNonQuery();
                }
            }

            GruppeEinnahmenView = CollectionViewSource.GetDefaultView(GruppeEinnahmen);
            GruppeAusgabenView = CollectionViewSource.GetDefaultView(GruppeAusgaben);

            GruppeEinnahmenView.Refresh();
            GruppeAusgabenView.Refresh();

            GruppeEinnahmenView.SortDescriptions.Clear();
            GruppeEinnahmenView.SortDescriptions.Add(new SortDescription("Gruppe", ListSortDirection.Ascending));

            GruppeAusgabenView.SortDescriptions.Clear();
            GruppeAusgabenView.SortDescriptions.Add(new SortDescription("Gruppe", ListSortDirection.Ascending));
        }

        //Commands

    }
}
