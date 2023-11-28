using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_LoginForm.Model;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class EinstellungenViewModel : ViewModelBase
    {
        private static EinstellungenViewModel instance;

        public EinstellungenViewModel()
        {

            ChangeCommand = new ViewModelCommand(ExecuteChangeCommand, CanExecuteChangeCommand);
            ChangeCommand2 = new ViewModelCommand(ExecuteChangeCommand2, CanExecuteChangeCommand2);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            DeleteCommand2 = new ViewModelCommand(ExecuteDeleteCommand2, CanExecuteDeleteCommand2);
            LoadPositionen();
            LoadProjekte();
        }

        //Singleton
        public static EinstellungenViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EinstellungenViewModel();
                }
                return instance;
            }
        }

        //Commands
        public ICommand ChangeCommand { get; }
        public ICommand ChangeCommand2 { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DeleteCommand2 { get; }

        //Fields
        private ObservableCollection<KassenprueferModel> _aktuelleKassenpruefer;
        private List<PositionModel> _positionenListe;
        private PositionModel _aktuellePosition;
        private List<ProjectModel> _projektListe;
        private ProjectModel _aktuellesProjekt;
        private string _errorMessage;

        //Properties
        public ObservableCollection<KassenprueferModel> AktuelleKassenpruefer
        {
            get { return _aktuelleKassenpruefer; }
            set { _aktuelleKassenpruefer = value; OnPropertyChanged(nameof(AktuelleKassenpruefer)); }
        }

        public List<PositionModel> PositionenListe
        {
            get { return _positionenListe; }
            set { _positionenListe = value; OnPropertyChanged(nameof(PositionenListe)); }
        }

        public PositionModel AktuellePosition
        {
            get { return _aktuellePosition; }
            set { _aktuellePosition = value; OnPropertyChanged(nameof(AktuellePosition)); }
        }

        public List<ProjectModel> ProjektListe
        {
            get { return _projektListe; }
            set { _projektListe = value; OnPropertyChanged(nameof(ProjektListe)); }
        }

        public ProjectModel AktuellesProjekt
        {
            get { return _aktuellesProjekt; }
            set { _aktuellesProjekt = value; OnPropertyChanged(nameof(AktuellesProjekt)); }
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

        //Methods
        public bool CanExecuteChangeCommand(object obj)
        {
            return true;
        }

        public bool CanExecuteChangeCommand2(object obj)
        {
            return true;
        }

        public bool CanExecuteDeleteCommand(object obj)
        {
            return true;
        }

        public bool CanExecuteDeleteCommand2(object obj)
        {
            return true;
        }

        public void ExecuteChangeCommand(object obj)
        {
            foreach (var kassenpruefer in AktuelleKassenpruefer)
            {
                // Überprüfe, ob alle Eigenschaften des Kassenprüfers ausgefüllt sind
                if (string.IsNullOrEmpty(kassenpruefer.Vorname) ||
                    string.IsNullOrEmpty(kassenpruefer.Nachname) ||
                    string.IsNullOrEmpty(kassenpruefer.Strasse) ||
                    string.IsNullOrEmpty(kassenpruefer.Ort))
                {
                    ErrorMessage = "* Alle Felder müssen ausgefüllt werden!";
                    return;
                }
            }

            ErrorMessage = "";

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();
                string sql = "UPDATE Pruefer SET Vorname = @vorname, Nachname = @nachname, Strasse = @strasse, Ort = @ort WHERE Id = @id";

                // SQLiteCommand-Objekt erstellen
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.Add("@id", DbType.Int32);
                    command.Parameters.Add("@vorname", DbType.String);
                    command.Parameters.Add("@nachname", DbType.String);
                    command.Parameters.Add("@strasse", DbType.String);
                    command.Parameters.Add("@ort", DbType.String);

                    // Prüfer 1
                    command.Parameters["@id"].Value = 1;
                    command.Parameters["@vorname"].Value = AktuelleKassenpruefer[0].Vorname;
                    command.Parameters["@nachname"].Value = AktuelleKassenpruefer[0].Nachname;
                    command.Parameters["@strasse"].Value = AktuelleKassenpruefer[0].Strasse;
                    command.Parameters["@ort"].Value = AktuelleKassenpruefer[0].Ort;
                    command.ExecuteNonQuery();

                    // Prüfer 2
                    command.Parameters["@id"].Value = 2;
                    command.Parameters["@vorname"].Value = AktuelleKassenpruefer[1].Vorname;
                    command.Parameters["@nachname"].Value = AktuelleKassenpruefer[1].Nachname;
                    command.Parameters["@strasse"].Value = AktuelleKassenpruefer[1].Strasse;
                    command.Parameters["@ort"].Value = AktuelleKassenpruefer[1].Ort;
                    command.ExecuteNonQuery();

                    // Prüfer 3
                    command.Parameters["@id"].Value = 3;
                    command.Parameters["@vorname"].Value = AktuelleKassenpruefer[2].Vorname;
                    command.Parameters["@nachname"].Value = AktuelleKassenpruefer[2].Nachname;
                    command.Parameters["@strasse"].Value = AktuelleKassenpruefer[2].Strasse;
                    command.Parameters["@ort"].Value = AktuelleKassenpruefer[2].Ort;
                    command.ExecuteNonQuery();
                }
            }
            LoadKassenpruefer();
        }

        public void ExecuteChangeCommand2(object obj)
        {
            if (AktuellePosition == null)
            {
                ErrorMessage = "* Die zu bearbeitende Position muss ausgewählt werden!";
                return;
            }

            if (AktuellePosition.Steuerklasse < 0 || AktuellePosition.Steuerklasse > 4)
            {
                ErrorMessage = "* Die Steuerklasse muss zwischen 0 und 4 liegen!";
                return;
            }

            if (AktuellePosition.EinnahmeAusgabe != "Einnahme" && AktuellePosition.EinnahmeAusgabe != "Ausgabe")
            {
                ErrorMessage = "* Die Art muss entweder 'Einnahme' oder 'Ausgabe' sein!";
                return;
            }

            ErrorMessage = "";
            
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();
                string sql = "INSERT OR REPLACE INTO Positionen (Name, EinnahmeAusgabe, Steuerklasse, KassenberichtGruppe) VALUES (@name, @einnahmeausgabe, @steuerklasse, @kassenberichtgruppe)";

                // SQLiteCommand-Objekt erstellen
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", AktuellePosition.Name);
                    command.Parameters.AddWithValue("@einnahmeausgabe", AktuellePosition.EinnahmeAusgabe);
                    command.Parameters.AddWithValue("@steuerklasse", AktuellePosition.Steuerklasse);
                    command.Parameters.AddWithValue("@kassenberichtgruppe", AktuellePosition.KassenberichtGruppe);
                    command.ExecuteNonQuery();
                }
            }
            KassenbuchViewModel.Instance.LoadPositionen();
            LoadPositionen();
        }

        public void ExecuteDeleteCommand(object obj)
        {
            if (AktuellePosition == null)
            {
                ErrorMessage = "* Die zu löschende Position muss ausgewählt werden!";
                return;
            }

            ErrorMessage = "";

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();
                string sql = "DELETE FROM Positionen WHERE Name = @name";

                // SQLiteCommand-Objekt erstellen
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", AktuellePosition.Name);
                    command.ExecuteNonQuery();
                }
            }
            KassenbuchViewModel.Instance.LoadPositionen();
            LoadPositionen();
        }

        public void ExecuteDeleteCommand2(object obj) //Geht noch nicht!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        {
            if (AktuellesProjekt == null)
            {
                ErrorMessage = "* Das zu löschende Projekt muss ausgewählt werden!";
                return;
            }

            // Benutzer um Bestätigung bitten
            DialogResult result = MessageBox.Show("Möchten Sie das Projekt wirklich löschen?", "Bestätigung", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                ErrorMessage = "";

                List<string> tablesToDelete = new List<string>();

                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tableName = reader.GetString(0);

                            if (tableName.StartsWith(AktuellesProjekt.Name.ToString()))
                            {
                                tablesToDelete.Add(tableName);
                            }
                        }

                        using (SQLiteTransaction transaction = connection.BeginTransaction())
                        {
                            foreach (string tableNameToDelete in tablesToDelete)
                            {
                                using (SQLiteCommand deleteCommand = new SQLiteCommand($"DROP TABLE IF EXISTS [{tableNameToDelete}];", connection))
                                {
                                    deleteCommand.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                    }
                    
                    // Eintrag aus Projekte-Tabelle löschen
                    string deleteEntrySQL = "DELETE FROM Projekte WHERE Name = @name";
                    using (SQLiteCommand deleteEntryCommand = new SQLiteCommand(deleteEntrySQL, connection))
                    {
                        deleteEntryCommand.Parameters.AddWithValue("@name", AktuellesProjekt.Name.ToString());
                        deleteEntryCommand.ExecuteNonQuery();
                    }
                }
                HomeViewModel.Instance.LoadProjects();
                LoadProjekte();
            }
        }

        public void LoadKassenpruefer()
        {
            _aktuelleKassenpruefer = new ObservableCollection<KassenprueferModel>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM Pruefer", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            KassenprueferModel eintrag = new KassenprueferModel();
                            eintrag.Id = reader.GetInt32(0);
                            eintrag.Vorname = reader.GetString(1);
                            eintrag.Nachname = reader.GetString(2);
                            eintrag.Strasse = reader.GetString(3);
                            eintrag.Ort = reader.GetString(4);

                            AktuelleKassenpruefer.Add(eintrag);
                        }
                    }
                }
                connection.Close();
            }
        }

        public void LoadPositionen()
        {
            PositionenListe = new List<PositionModel>();

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
                            PositionenListe.Add(new PositionModel
                            {
                                Name = reader["Name"].ToString(),
                                EinnahmeAusgabe = reader["EinnahmeAusgabe"].ToString(),
                                Steuerklasse = Convert.ToInt32(reader["Steuerklasse"]),
                                KassenberichtGruppe = reader["KassenberichtGruppe"].ToString()
                            });
                        }
                    }
                }
            }
            PositionenListe.Sort((x, y) => string.Compare(x.Name, y.Name));

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(PositionenListe));
            });
        }

        public void LoadProjekte()
        {
            _projektListe = new List<ProjectModel>();

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT Id, Name FROM Projekte", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _projektListe.Add(new ProjectModel
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = Convert.ToInt32(reader["Name"])
                            });
                        }
                    }
                }
            }
            _projektListe.Sort((projekt1, projekt2) => projekt1.Name.CompareTo(projekt2.Name));
        }
    }
}
