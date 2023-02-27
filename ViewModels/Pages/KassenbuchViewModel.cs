using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF_LoginForm.Model;

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
        private bool _isEinnahmeSelected;
        private bool _isAusgabeSelected;
        private ObservableCollection<PositionModel> _comboBoxSource;

        //Properties
        public ObservableCollection<KassenbucheintragModel> AktuellesKassenbuch { get; set; }

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

        public bool IsEinnahmeSelected
        {
            get => _isEinnahmeSelected;
            set
            {
                _isEinnahmeSelected = value;
                OnPropertyChanged(nameof(IsEinnahmeSelected));
                UpdateComboBox();
            }
        }

        public bool IsAusgabeSelected
        {
            get => _isAusgabeSelected;
            set
            {
                _isAusgabeSelected = value;
                OnPropertyChanged(nameof(IsAusgabeSelected));
                UpdateComboBox();
            }
        }

        //Commands
        public ICommand AddCommand { get; }


        //Methods
        private bool CanExecuteAddCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void ExecuteAddCommand(object obj)
        {
            throw new NotImplementedException();
        }


        public void LoadPositionen()
        {
            _positionenListe = new List<PositionModel>();

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT Name, EinnahmeAusgabe, Steuerklasse FROM Positionen", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _positionenListe.Add(new PositionModel
                            {
                                Name = reader["Name"].ToString(),
                                EinnahmeAusgabe = reader["EinnahmeAusgabe"].ToString(),
                                Steuerklasse = Convert.ToInt32(reader["Steuerklasse"])
                            });
                        }
                        UpdateComboBox();
                    }
                }
            }
        }

        /*public void LoadKassenbuch() ///////////////////////////////
        {
            string tableName = $"{CurrentProject.Name}_Kassenbuch";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                string query = "SELECT * FROM {tableName}";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            KassenbucheintragModel eintrag = new KassenbucheintragModel();

                            eintrag.Id = reader.GetInt32(0);
                            eintrag.Datum = reader.GetString(1);
                            eintrag.Buchungsnummer = reader.GetInt32(2);
                            eintrag.Position = reader.GetString(3);
                            eintrag.Datum = reader.GetString(1);


                            eintrag.Datum = reader.GetDateTime(0);
                            eintrag.Beschreibung = reader.GetString(1);
                            eintrag.Betrag = reader.GetDecimal(2);
                            // ...
                            AktuellesKassenbuch.Add(eintrag);
                        }
                    }
                }

                connection.Close();
            }
        }*/

        private void UpdateComboBox()
        {
            if (_positionenListe == null)
                return;

            if (IsEinnahmeSelected)
                ComboBoxSource = new ObservableCollection<PositionModel>(_positionenListe.Where(p => p.EinnahmeAusgabe == "Einnahme"));
            else if (IsAusgabeSelected)
                ComboBoxSource = new ObservableCollection<PositionModel>(_positionenListe.Where(p => p.EinnahmeAusgabe == "Ausgabe"));
            else
                ComboBoxSource = new ObservableCollection<PositionModel>(_positionenListe);
        }
    }
}
