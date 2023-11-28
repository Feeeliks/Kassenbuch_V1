using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_LoginForm.Model;
using Xceed.Wpf.Toolkit.Primitives;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WPF_LoginForm.ViewModels.Pages
{
    public class MitgliederViewModel : ViewModelBase
    {
        //Constructor
        public MitgliederViewModel()
        {
            StatusListe.Add("Aktiv (Ü18)");
            StatusListe.Add("Aktiv (Ü18, Ausbildung)");
            StatusListe.Add("Aktiv (U18)");
            StatusListe.Add("Aktiv (U12)");
            StatusListe.Add("Passiv");
            StatusListe.Add("Ehrenmitglied");
            AddCommand = new ViewModelCommand(ExecuteAddCommand, CanExecuteAddCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand, CanExecuteDeleteCommand);
            PayCommand = new ViewModelCommand(ExecutePayCommand, CanExecutePayCommand);
        }

        //Singleton
        public static MitgliederViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MitgliederViewModel();
                }
                return instance;
            }
        }

        //Fields
        private static MitgliederViewModel instance;

        private ObservableCollection<MitgliedModel> _aktuelleMitglieder;
        private ICollectionView _aktuelleMitgliederView;

        private List<string> _statusListe = new List<string>();

        private string _vorname;
        private string _nachname;
        private string _mitgliedsstatus;

        private string _errorMessage;

        private MitgliedModel _selectedItem;

        //Properties
        public ObservableCollection<MitgliedModel> AktuelleMitglieder
        {
            get { return _aktuelleMitglieder; }
            set { _aktuelleMitglieder = value; OnPropertyChanged(nameof(AktuelleMitglieder)); }
        }

        public ICollectionView AktuelleMitgliederView
        {
            get { return _aktuelleMitgliederView; }
            set { _aktuelleMitgliederView = value; OnPropertyChanged(nameof(AktuelleMitgliederView)); }
        }

        public List<string> StatusListe
        {
            get { return _statusListe; }
            set { _statusListe = value; OnPropertyChanged(nameof(StatusListe)); }
        }

        public string Vorname
        {
            get { return _vorname; }
            set { _vorname = value; OnPropertyChanged(nameof(Vorname)); }
        }

        public string Nachname
        {
            get { return _nachname; }
            set { _nachname = value; OnPropertyChanged(nameof(Nachname)); }
        }

        public string Mitgliedsstatus
        {
            get { return _mitgliedsstatus; }
            set { _mitgliedsstatus = value; OnPropertyChanged(nameof(Mitgliedsstatus)); }
        }

        public MitgliedModel SelectedItem1
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem1)); }
        }

        public string ErrorMessage
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
        public void LoadMitglieder()
        {
            _aktuelleMitglieder = new ObservableCollection<MitgliedModel>();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand($"SELECT * FROM [{HomeViewModel.Instance.AktuellesProjekt}_Mitglieder]", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MitgliedModel eintrag = new MitgliedModel();
                            eintrag.Id = reader.GetInt32(0);
                            eintrag.Vorname = reader.GetString(1);
                            eintrag.Nachname = reader.GetString(2);
                            eintrag.Mitgliedsstatus = reader.GetString(3);
                            eintrag.Mitgliedsbeitrag = reader.GetDouble(4);
                            eintrag.Bezahlstatus = reader.GetString(5);

                            _aktuelleMitglieder.Add(eintrag);
                        }

                        AktuelleMitgliederView = CollectionViewSource.GetDefaultView(_aktuelleMitglieder);

                        AktuelleMitgliederView.GroupDescriptions.Add(new PropertyGroupDescription("Bezahlstatus"));
                    }
                }
                connection.Close();
            }
        }

        public bool CanExecuteAddCommand(object obj)
        {
            // Alles vernünftig ausgefüllt?
            return true;
        }

        public bool CanExecuteDeleteCommand(object obj)
        {
            return true;
        }
        
        public bool CanExecutePayCommand(object obj)
        {
            return true;
        }

        public void ExecuteAddCommand(object obj)
        {
            // Überprüfen, ob alle Textfelder ausgefüllt sind
            if (string.IsNullOrEmpty(Vorname) || string.IsNullOrEmpty(Nachname) || string.IsNullOrEmpty(Mitgliedsstatus))
            {
                ErrorMessage = "* Alle Felder müssen ausgefüllt sein!";
                //System.Windows.Forms.MessageBox.Show("Alle Felder müssen ausgefüllt werden!","Eingabefehler",MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            else
                ErrorMessage = "";

            string tabellenname = HomeViewModel.Instance.AktuellesProjekt + "_Mitglieder";
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();
                string sql = "INSERT INTO \"" + tabellenname + "\" (Vorname, Nachname, Mitgliedsstatus, Mitgliedsbeitrag, Bezahlstatus) VALUES (@vorname, @nachname, @mitgliedsstatus, @mitgliedsbeitrag, @bezahlstatus)";

                // SQLiteCommand-Objekt erstellen
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    // Parameter für den SQL-Befehl erstellen
                    command.Parameters.AddWithValue("@vorname", Vorname);
                    command.Parameters.AddWithValue("@nachname", Nachname);
                    command.Parameters.AddWithValue("@mitgliedsstatus", Mitgliedsstatus);

                    switch(Mitgliedsstatus)
                    {
                        case "Aktiv (Ü18)": { command.Parameters.AddWithValue("@mitgliedsbeitrag", 90.00); break; }
                        case "Aktiv (Ü18, Ausbildung)": { command.Parameters.AddWithValue("@mitgliedsbeitrag", 66.00); break; }
                        case "Aktiv (U18)": { command.Parameters.AddWithValue("@mitgliedsbeitrag", 54.00); break; }
                        case "Aktiv (U12)": { command.Parameters.AddWithValue("@mitgliedsbeitrag", 54.00); break; }
                        case "Passiv": { command.Parameters.AddWithValue("@mitgliedsbeitrag", 54.00); break; }
                        case "Ehrenmitglied": { command.Parameters.AddWithValue("@mitgliedsbeitrag", 0.00); break; }
                    }

                    command.Parameters.AddWithValue("@bezahlstatus", "offen");
                    
                    // SQL-Befehl ausführen
                    command.ExecuteNonQuery();

                    LoadMitglieder();
                }
            }
        }

        public void ExecuteDeleteCommand(object obj)
        {
            if (SelectedItem1 != null)
            {
                ErrorMessage = "";

                int recordId = SelectedItem1.Id;
                string tabellenname = HomeViewModel.Instance.AktuellesProjekt + "_Mitglieder";
                string sql = "DELETE FROM \"" + tabellenname + "\" WHERE Id = @id";
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", recordId);
                        command.ExecuteNonQuery();
                    }
                }
                LoadMitglieder();
            }

            else
            {
                ErrorMessage = "* Die zu löschende Zeile muss ausgewählt werden!";
                return;            
            }
        }
        
        public void ExecutePayCommand(object obj)
        {
            if(SelectedItem1!=null)
            {
                ErrorMessage = "";

                int recordId = SelectedItem1.Id;
                string tabellenname = HomeViewModel.Instance.AktuellesProjekt + "_Mitglieder";
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();

                    // Erstelle SQL-Befehl, um Bezahlstatus zu ändern
                    string sql = $"UPDATE \"" + tabellenname + "\" SET Bezahlstatus='bezahlt' WHERE Id= \"" + recordId + "\"";

                    // SQLiteCommand-Objekt erstellen
                    using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                    {
                        // SQL-Befehl ausführen
                        command.ExecuteNonQuery();
                        LoadMitglieder();
                    }
                }
            }

            else
            {
                ErrorMessage = "* Es muss eine Zeile ausgewählt werden!";
                return;
            }
        }

        //Commands
        public ICommand PayCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
    }
}
