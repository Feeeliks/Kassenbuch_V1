using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_LoginForm.Model;
using Xceed.Wpf.Toolkit.Primitives;
using WPF_LoginForm.View.Pages;
using System.Runtime.ConstrainedExecution;
using Xceed.Wpf.Toolkit;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class KassenbuchViewModel : ViewModelBase
    {
        private static KassenbuchViewModel instance;

        //Constructor
        public KassenbuchViewModel()
        {
            LoadPositionen();
            AddCommand = new ViewModelCommand(ExecuteAddCommand, CanExecuteAddCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
        }

        public static KassenbuchViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KassenbuchViewModel();
                }
                return instance;
            }
        }

        //Fields
        private List<PositionModel> _positionenListe;
        private ObservableCollection<PositionModel> _comboBoxSource;
        private ObservableCollection<KassenbucheintragModel> _aktuellesKassenbuch;
        private ICollectionView _aktuellesKassenbuchView;
        private string _datum;
        private bool _isEinnahmeSelected;
        private bool _isAusgabeSelected;
        private PositionModel _position;
        private double? _betrag;
        private bool _isKontoSelected;
        private bool _isKasseSelected;
        private int? _belegnummer;
        private int _steuerklasse;
        private KassenbucheintragModel _selectedItem;
        private ObservableCollection<double> _kassenstand = new ObservableCollection<double>(new double[8]);
        private string _errorMessage;

        //Properties

        public ICollectionView AktuellesKassenbuchView
        {   
            get { return _aktuellesKassenbuchView; }
            set { _aktuellesKassenbuchView = value; OnPropertyChanged(nameof(AktuellesKassenbuchView)); }
        }

        public ObservableCollection<KassenbucheintragModel> AktuellesKassenbuch
        {
            get { return _aktuellesKassenbuch; }
            set { _aktuellesKassenbuch = value; OnPropertyChanged(nameof(AktuellesKassenbuch)); }
        }

        public List<PositionModel> PositionenListe
        {
            get { return _positionenListe; }
            set { _positionenListe = value; OnPropertyChanged(nameof(PositionenListe)); }
        }
        
        public ObservableCollection<PositionModel> ComboBoxSource
        {
            get => _comboBoxSource;
            set
            {
                _comboBoxSource = value;
                OnPropertyChanged(nameof(ComboBoxSource));
            }
        }

        public string Datum
        {
            get => _datum;
            set
            {
                _datum = value;
                OnPropertyChanged(nameof(Datum));
            }
        }

        public bool IsEinnahmeSelected
        {
            get => _isEinnahmeSelected;
            set
            {
                _isEinnahmeSelected = value;
                OnPropertyChanged(nameof(IsEinnahmeSelected));
                //UpdateComboBox();
            }
        }

        public bool IsAusgabeSelected
        {
            get => _isAusgabeSelected;
            set
            {
                _isAusgabeSelected = value;
                OnPropertyChanged(nameof(IsAusgabeSelected));
                //UpdateComboBox();
            }
        }

        public PositionModel Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        public string PositionName { get => Position.Name; }

        public double? Betrag
        {
            get => _betrag;
            set
            {
                if (_betrag != value)
                {
                    _betrag = value;
                    OnPropertyChanged(nameof(Betrag));
                }
            }
        }

        public bool IsKontoSelected
        {
            get => _isKontoSelected;
            set
            {
                _isKontoSelected = value;
                OnPropertyChanged(nameof(IsKontoSelected));
                //UpdateComboBox();
            }
        }

        public bool IsKasseSelected
        {
            get => _isKasseSelected;
            set
            {
                _isKasseSelected = value;
                OnPropertyChanged(nameof(IsKasseSelected));
                //UpdateComboBox();
            }
        }

        public int? Belegnummer
        {
            get => _belegnummer;
            set
            {
                if (_belegnummer != value)
                {
                    _belegnummer = value;
                    OnPropertyChanged(nameof(Belegnummer));
                }
            }
        }

        public int Steuerklasse
        {
            get => _steuerklasse;
            set
            {
                _steuerklasse = value;
                OnPropertyChanged(nameof(Steuerklasse));
            }
        }

        public KassenbucheintragModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<double> Kassenstand
        {
            get => _kassenstand;
            set
            {
                _kassenstand = value;
                OnPropertyChanged(nameof(Kassenstand));
            }
        }

        public String ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(IsErrorMessageVisible)); // Auslösen der Eigenschaft, die die Sichtbarkeit der Grid.Row steuert
            }
        }

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        //Commands
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }


        //Methods
        
        private bool CanExecuteDeleteCommand(object obj)
        {
            return true;
        }

        private void ExecuteDeleteCommand(object obj)
        {
            if (SelectedItem != null)
            {
                ErrorMessage = "";

                int recordId = SelectedItem.Id;
                string tabellenname = HomeViewModel.Instance.AktuellesProjekt;
                string sql = "DELETE FROM \"" + tabellenname + "_Kassenbuch\" WHERE Id = @id";
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", recordId);
                        command.ExecuteNonQuery();
                    }
                }
                LoadKassenbuch(tabellenname);
            }

            else
            {
                ErrorMessage = "* Die zu löschende Zeile muss ausgewählt werden!";
                return;
            }
        }

        private bool CanExecuteAddCommand(object obj)
        {
            return true;
        }

        private void ExecuteAddCommand(object obj) //STEUERKLASSEN KLÄREN
        {
            // Überprüfen, ob alle Textfelder ausgefüllt sind
            if (string.IsNullOrEmpty(Datum) || Position == null || Betrag == 0.00 || Betrag == null || Belegnummer == 0 || Belegnummer == null || /*(IsEinnahmeSelected == false && IsAusgabeSelected == false) ||*/ (IsKontoSelected == false && IsKasseSelected == false))
            {
                ErrorMessage = "* Alle Felder müssen ausgefüllt sein!";
                //System.Windows.Forms.MessageBox.Show("Alle Felder müssen ausgefüllt werden!","Eingabefehler",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            else
                ErrorMessage = "";

            foreach (PositionModel position in PositionenListe)
            {
                if (position.Name == _position.Name)
                {
                    _steuerklasse = position.Steuerklasse;
                    break;
                }
            }

            foreach (PositionModel position in PositionenListe)
            {
                if (position.Name == _position.Name)
                {
                    if (position.EinnahmeAusgabe == "Einnahme")
                    {
                        _isEinnahmeSelected = true;
                        _isAusgabeSelected = false;
                        break;
                    }

                    if (position.EinnahmeAusgabe == "Ausgabe")
                    {
                        _isEinnahmeSelected = false;
                        _isAusgabeSelected = true;
                        break;
                    }
                }
            }

            string tabellenname = HomeViewModel.Instance.AktuellesProjekt;
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();
                string sql = "INSERT INTO \"" + tabellenname + "_Kassenbuch\" (Datum, Position, Kontobeleg, Belegnummer, Betrag, KontoEinnahme, KontoAusgabe, KasseEinnahme, KasseAusgabe, Steuer1Einnahme, Steuer1Ausgabe, Steuer2Einnahme, Steuer2Ausgabe, Steuer3Einnahme, Steuer3Ausgabe, Steuer4Einnahme, Steuer4Ausgabe, Gruppe) VALUES (@datum, @position, @kontobeleg, @belegnummer, @betrag, @kontoEinnahme, @kontoAusgabe, @kasseEinnahme, @kasseAusgabe, @steuer1Einnahme, @steuer1Ausgabe, @steuer2Einnahme, @steuer2Ausgabe, @steuer3Einnahme, @steuer3Ausgabe, @steuer4Einnahme, @steuer4Ausgabe, @gruppe)";

                // SQLiteCommand-Objekt erstellen
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    // Parameter für den SQL-Befehl erstellen
                    command.Parameters.AddWithValue("@datum", Datum);
                    command.Parameters.AddWithValue("@position", Position.Name);
                    command.Parameters.AddWithValue("@belegnummer", Belegnummer);
                    command.Parameters.AddWithValue("@betrag", Betrag);
                    command.Parameters.AddWithValue("@gruppe", Position.KassenberichtGruppe);

                    if (IsEinnahmeSelected)
                    {
                        if (IsKontoSelected)
                        {
                            command.Parameters.AddWithValue("@kontoEinnahme", Betrag);
                            command.Parameters.AddWithValue("@kontoAusgabe", 0.00);
                            command.Parameters.AddWithValue("@kasseEinnahme", 0.00);
                            command.Parameters.AddWithValue("@kasseAusgabe", 0.00);
                            command.Parameters.AddWithValue("@kontobeleg", "B");
                        }
                        
                        else if (IsKasseSelected)
                        {
                            command.Parameters.AddWithValue("@kontoEinnahme", 0.00);
                            command.Parameters.AddWithValue("@kontoAusgabe", 0.00);
                            command.Parameters.AddWithValue("@kasseEinnahme", Betrag);
                            command.Parameters.AddWithValue("@kasseAusgabe", 0.00);
                            command.Parameters.AddWithValue("@kontobeleg", "");
                        }

                        switch (Steuerklasse)
                        {
                            case 0:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 1:
                                command.Parameters.AddWithValue("@steuer1Einnahme", Betrag);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 2:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", Betrag);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 3:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", Betrag);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 4:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", Betrag);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;
                        }
                    }

                    else if (IsAusgabeSelected)
                    {
                        if (IsKontoSelected)
                        {
                            command.Parameters.AddWithValue("@kontoEinnahme", 0.00);
                            command.Parameters.AddWithValue("@kontoAusgabe", Betrag);
                            command.Parameters.AddWithValue("@kasseEinnahme", 0.00);
                            command.Parameters.AddWithValue("@kasseAusgabe", 0.00);
                            command.Parameters.AddWithValue("@kontobeleg", "B");
                        }
                        
                        else if (IsKasseSelected)
                        {
                            command.Parameters.AddWithValue("@kontoEinnahme", 0.00);
                            command.Parameters.AddWithValue("@kontoAusgabe", 0.00);
                            command.Parameters.AddWithValue("@kasseEinnahme", 0.00);
                            command.Parameters.AddWithValue("@kasseAusgabe", Betrag);
                            command.Parameters.AddWithValue("@kontobeleg", "");
                        }

                        switch (Steuerklasse)
                        {
                            case 0:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 1:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", Betrag);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 2:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", Betrag);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 3:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", Betrag);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", 0.00);
                                break;

                            case 4:
                                command.Parameters.AddWithValue("@steuer1Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer1Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer2Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer2Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer3Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer3Ausgabe", 0.00);
                                command.Parameters.AddWithValue("@steuer4Einnahme", 0.00);
                                command.Parameters.AddWithValue("@steuer4Ausgabe", Betrag);
                                break;
                        }

                    }

                    // SQL-Befehl ausführen
                    command.ExecuteNonQuery();
                }
            }

            LoadKassenbuch(HomeViewModel.Instance.AktuellesProjekt);

            Belegnummer++;
        }

        public void LoadPositionen()
        {
            _positionenListe = new List<PositionModel>();

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT Name, EinnahmeAusgabe, Steuerklasse, KassenberichtGruppe FROM Positionen", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _positionenListe.Add(new PositionModel
                            {
                                Name = reader["Name"].ToString(),
                                EinnahmeAusgabe = reader["EinnahmeAusgabe"].ToString(),
                                Steuerklasse = Convert.ToInt32(reader["Steuerklasse"]),
                                KassenberichtGruppe = reader["KassenberichtGruppe"].ToString()
                            });
                        }
                        //UpdateComboBox();
                    }
                }
            }
        }

        public void WriteKassenbestand()
        {
            string tabellennameNeu = HomeViewModel.Instance.AktuellesProjekt;
            string tabellennameAlt = HomeViewModel.Instance.VorherigesProjekt;
            string sqlCountNeu = "SELECT COUNT(*) FROM \"" + tabellennameNeu + "_Kassenbuch\"";
            string sqlCountAlt = "SELECT COUNT(*) FROM \"" + tabellennameAlt + "_Kassenbuch\"";
            string sqlKontoPlus = "SELECT SUM([KontoEinnahme]) FROM \"" + tabellennameNeu + "_Kassenbuch\"";
            string sqlKontoMinus = "SELECT SUM([KontoAusgabe]) FROM \"" + tabellennameNeu + "_Kassenbuch\"";
            string sqlKassePlus = "SELECT SUM([KasseEinnahme]) FROM \"" + tabellennameNeu + "_Kassenbuch\"";
            string sqlKasseMinus = "SELECT SUM([KasseAusgabe]) FROM \"" + tabellennameNeu + "_Kassenbuch\"";
            string sqlAltBestand = "SELECT Gesamtbestand, Kontobestand, Handkassenbestand, Ausschankkassenbestand FROM \"" + tabellennameAlt + "_Kassenbestand\"";
            string sqlUpdate = "INSERT OR REPLACE INTO \"" + tabellennameNeu + "_Kassenbestand\" ([ID], [Gesamtbestand], [Kontobestand], [Handkassenbestand], [Ausschankkassenbestand]) VALUES (1, @gesamtbestand, @kontobestand, @handkassenbestand, @ausschankkassenbestand)";
            double[] kassenbestandAlt = new double[4];

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                var commandCountNeu = new SQLiteCommand(sqlCountNeu, connection);
                int rowCountNeu = Convert.ToInt32(commandCountNeu.ExecuteScalar());

                int rowCountAlt;
                var commandCountAlt = new SQLiteCommand(sqlCountAlt, connection);
                try { rowCountAlt = Convert.ToInt32(commandCountAlt.ExecuteScalar()); } 
                catch (Exception ex) { rowCountAlt = 0; }

                if (rowCountNeu > 0)
                {
                    var command1 = new SQLiteCommand(sqlKontoPlus, connection);
                    var kontoPlus = Convert.ToDouble(command1.ExecuteScalar());

                    var command2 = new SQLiteCommand(sqlKontoMinus, connection);
                    var kontoMinus = Convert.ToDouble(command2.ExecuteScalar());

                    var command3 = new SQLiteCommand(sqlKassePlus, connection);
                    var kassePlus = Convert.ToDouble(command3.ExecuteScalar());

                    var command4 = new SQLiteCommand(sqlKasseMinus, connection);
                    var kasseMinus = Convert.ToDouble(command4.ExecuteScalar());

                    if (rowCountAlt > 0)
                    {
                        using (var command5 = new SQLiteCommand(sqlAltBestand, connection))
                        {
                            using (var reader = command5.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    kassenbestandAlt[0] = reader.GetDouble(0);    // Gesamtbestand
                                    kassenbestandAlt[1] = reader.GetDouble(1);    // Kontobestand
                                    kassenbestandAlt[2] = reader.GetDouble(2);    // Handkassenbestand
                                    kassenbestandAlt[3] = reader.GetDouble(3);    // Ausschankkassenbestand
                                }
                            }
                        }
                    }

                    else
                    {
                        kassenbestandAlt[0] = 0.00;    // Gesamtbestand
                        kassenbestandAlt[1] = 0.00;    // Kontobestand
                        kassenbestandAlt[2] = 0.00;    // Handkassenbestand
                        kassenbestandAlt[3] = 0.00;    // Ausschankkassenbestand
                    }

                    var kontoBestand = kontoPlus - kontoMinus + kassenbestandAlt[1];
                    var kasseBestand = kassePlus - kasseMinus + kassenbestandAlt[2];
                    var gesamtBestand = kontoBestand + kasseBestand + 350.00;

                    var command6 = new SQLiteCommand(sqlUpdate, connection);
                    command6.Parameters.AddWithValue("@gesamtbestand", gesamtBestand);
                    command6.Parameters.AddWithValue("@kontobestand", kontoBestand);
                    command6.Parameters.AddWithValue("@handkassenbestand", kasseBestand);
                    command6.Parameters.AddWithValue("@ausschankkassenbestand", 350.00);
                    command6.ExecuteNonQuery();
                }

            }

        }

        public void LoadKassenbestand()
        {
            string tabellennameNeu = HomeViewModel.Instance.AktuellesProjekt;
            string tabellennameAlt = HomeViewModel.Instance.VorherigesProjekt;
            string sqlNeu = "SELECT Gesamtbestand, Kontobestand, Handkassenbestand, Ausschankkassenbestand FROM \"" + tabellennameNeu + "_Kassenbestand\"";
            string sqlAlt = "SELECT Gesamtbestand, Kontobestand, Handkassenbestand, Ausschankkassenbestand FROM \"" + tabellennameAlt + "_Kassenbestand\"";
            
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (var command = new SQLiteCommand(sqlNeu, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        int i = 0;
                        while (reader.Read())
                        {
                            Kassenstand[i] = reader.GetDouble(0);     // Gesamtkassenbestand
                            Kassenstand[i + 1] = reader.GetDouble(1); // Kontobestand
                            Kassenstand[i + 2] = reader.GetDouble(2); // Handkassenbestand
                            Kassenstand[i + 3] = reader.GetDouble(3); // Ausschankkassenbestand
                            i += 4;
                        }
                    }
                    KassenberichtViewModel.Instance.SummeKasse = Kassenstand[2] + Kassenstand[3];
                }

                try
                {
                    using (var command = new SQLiteCommand(sqlAlt, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int i = 4;
                            while (reader.Read())
                            {
                                Kassenstand[i] = reader.GetDouble(0);     // Gesamtkassenbestand
                                Kassenstand[i + 1] = reader.GetDouble(1); // Kontobestand
                                Kassenstand[i + 2] = reader.GetDouble(2); // Handkassenbestand
                                Kassenstand[i + 3] = reader.GetDouble(3); // Ausschankkassenbestand
                                i += 4;
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    int i = 4;
                    Kassenstand[i] = 0.00;     // Gesamtkassenbestand
                    Kassenstand[i + 1] = 0.00; // Kontobestand
                    Kassenstand[i + 2] = 0.00; // Handkassenbestand
                    Kassenstand[i + 3] = 0.00; // Ausschankkassenbestand
                }

                connection.Close();
            }
        }

        public void LoadKassenbuch(string aktuellesProjekt)
        {
            _aktuellesKassenbuch = new ObservableCollection<KassenbucheintragModel>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM [{aktuellesProjekt}_Kassenbuch]", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            KassenbucheintragModel eintrag = new KassenbucheintragModel();

                            eintrag.Id = reader.GetInt32(0);
                            eintrag.Datum = reader.GetString(1);
                            eintrag.Position = reader.GetString(2);
                            eintrag.Kontobeleg = reader.GetString(3);
                            eintrag.Belegnummer = reader.GetInt32(4);
                            eintrag.Betrag = reader.GetDouble(5);
                            eintrag.KontoEinnahme = reader.GetDouble(6);
                            eintrag.KontoAusgabe = reader.GetDouble(7);
                            eintrag.KasseEinnahme = reader.GetDouble(8);
                            eintrag.KasseAusgabe = reader.GetDouble(9);
                            eintrag.Steuer1Einnahme = reader.GetDouble(10);
                            eintrag.Steuer1Ausgabe = reader.GetDouble(11);
                            eintrag.Steuer2Einnahme = reader.GetDouble(12);
                            eintrag.Steuer2Ausgabe = reader.GetDouble(13);
                            eintrag.Steuer3Einnahme = reader.GetDouble(14);
                            eintrag.Steuer3Ausgabe = reader.GetDouble(15);
                            eintrag.Steuer4Einnahme = reader.GetDouble(16);
                            eintrag.Steuer4Ausgabe = reader.GetDouble(17);
                            eintrag.Gruppe = reader.GetString(18);

                            _aktuellesKassenbuch.Add(eintrag);
                        }

                        AktuellesKassenbuchView = CollectionViewSource.GetDefaultView(_aktuellesKassenbuch);

                        AktuellesKassenbuchView.GroupDescriptions.Add(new PropertyGroupDescription("MonthGroup"));
                        AktuellesKassenbuchView.GroupDescriptions.Add(new PropertyGroupDescription("KontobelegGroup"));

                    }
                }
                connection.Close();

                WriteKassenbestand();
                LoadKassenbestand();
                KassenberichtViewModel.Instance.KassenberichtGruppieren();
            }
        }

        /*
        private void UpdateComboBox()
        {
            if (_positionenListe == null)
                return;

            List<PositionModel> sortedList;

            if (IsEinnahmeSelected)
                sortedList = _positionenListe.Where(p => p.EinnahmeAusgabe == "Einnahme").OrderBy(p => p.Name).ToList();
            else if (IsAusgabeSelected)
                sortedList = _positionenListe.Where(p => p.EinnahmeAusgabe == "Ausgabe").OrderBy(p => p.Name).ToList();
            else
                sortedList = _positionenListe.OrderBy(p => p.Name).ToList();

            ComboBoxSource = new ObservableCollection<PositionModel>(sortedList);
        }
        */
    }
}
