using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Mapping;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_LoginForm.Model;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Windows.Forms;

namespace WPF_LoginForm.ViewModels.Pages
{
        public class HomeViewModel : ViewModelBase
        {
            //Fields
            private static HomeViewModel instance;
            private string _aktuellesProjekt;
            private string _vorherigesProjekt;
            private string _createTableName;
            private List<string> _projects;

            //Singleton
            public static HomeViewModel Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new HomeViewModel();
                    }
                    return instance;
                }
            }

        //Properties
            public ExportViewModel Export { get; set; }
            public EinstellungenViewModel Einstellungen { get; set; }

            public MitgliederViewModel Mitglieder { get; set; }

            public string CreateTableName
            {
                get { return _createTableName; }
                set
                {
                    _createTableName = value;
                    OnPropertyChanged(nameof(CreateTableName));
                }
            }

            public List<string> Projects
            {
                get { return _projects; }
                set { _projects = value; OnPropertyChanged(nameof(Projects)); }
            }

            public string AktuellesProjekt
            {
                get { return _aktuellesProjekt; }
                set
                {
                    _aktuellesProjekt = value;
                    OnPropertyChanged(nameof(AktuellesProjekt));
                    KassenbuchViewModel.Instance.LoadKassenbuch(AktuellesProjekt);
                    MitgliederViewModel.Instance.LoadMitglieder();
                    EinstellungenViewModel.Instance.LoadKassenpruefer();
                }
            }

            public string VorherigesProjekt
            {
                get { return Convert.ToString(Convert.ToInt32(_aktuellesProjekt) - 1); }
                set
                {
                    _vorherigesProjekt = value;
                    OnPropertyChanged(nameof(VorherigesProjekt));
                }
            }

            //Commands
            public ICommand CreateTableCommand { get; }

            //Methods
            public void LoadProjects()
            {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand("SELECT Name FROM Projekte", connection))
                {
                    SQLiteDataReader reader = command.ExecuteReader();
                    _projects = new List<string>();
                    while (reader.Read())
                    {
                        string name = Convert.ToString(reader["Name"]);
                        _projects.Add(name);
                    }
                }
            }
            OnPropertyChanged(nameof(Projects));
            }

            private bool TableExists(SQLiteConnection connection, string tableName)
            {
                string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }

            public void ExecuteCreateTable(object obj)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();

                    string sourceTableName = (int.Parse(CreateTableName) + 1).ToString();
                    string targetTableName = CreateTableName + "_Mitglieder";

                    string sql1;

                    if (TableExists(connection, sourceTableName))
                    {
                        // Kopiere Daten aus der Quelltabelle und ändere den Bezahlstatus
                        sql1 = "INSERT INTO \"" + targetTableName + "\" (Vorname, Nachname, Mitgliedsstatus, Mitgliedsbeitrag, Bezahlstatus) " +
                            "SELECT Vorname, Nachname, Mitgliedsstatus, Mitgliedsbeitrag, 'offen' FROM \"" + sourceTableName + "\"";
                    }
                    else
                    {
                        // Erstelle eine leere Tabelle mit den angegebenen Spalten
                        sql1 = "CREATE TABLE IF NOT EXISTS \"" + targetTableName + "\" (" +
                            "Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                            "Vorname TEXT NOT NULL, " +
                            "Nachname TEXT NOT NULL, " +
                            "Mitgliedsstatus TEXT NOT NULL, " +
                            "Mitgliedsbeitrag REAL NOT NULL, " +
                            "Bezahlstatus TEXT NOT NULL DEFAULT 'offen')";
                    }

                    string sql2 = "CREATE TABLE IF NOT EXISTS \"" + CreateTableName + "_Kassenbuch\" (" +
                        "Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                        "Datum TEXT NOT NULL, " +
                        "Position TEXT NOT NULL, " +
                        "Kontobeleg TEXT, " +
                        "Belegnummer INTEGER NOT NULL, " +
                        "Betrag REAL NOT NULL, " +
                        "KontoEinnahme REAL, " +
                        "KontoAusgabe REAL, " +
                        "KasseEinnahme REAL, " +
                        "KasseAusgabe REAL, " +
                        "Steuer1Einnahme REAL, " +
                        "Steuer1Ausgabe REAL, " +
                        "Steuer2Einnahme REAL, " +
                        "Steuer2Ausgabe REAL, " +
                        "Steuer3Einnahme REAL, " +
                        "Steuer3Ausgabe REAL, " +
                        "Steuer4Einnahme REAL, " +
                        "Steuer4Ausgabe REAL, " +
                        "Gruppe TEXT NOT NULL)";

                    string sql3 = "INSERT INTO Projekte (Name) VALUES (@Name)";

                    string sql4 = "CREATE TABLE IF NOT EXISTS \"" + CreateTableName + "_Kassenbestand\" (" +
                        "Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                        "Gesamtbestand REAL NOT NULL, " +
                        "Kontobestand REAL NOT NULL, " +
                        "Handkassenbestand REAL NOT NULL, " +
                        "Ausschankkassenbestand REAL NOT NULL);" +
                        "INSERT INTO \"" + CreateTableName + "_Kassenbestand\" (Gesamtbestand, Kontobestand, Handkassenbestand, Ausschankkassenbestand) VALUES (0.00, 0.00, 0.00, 350.00)";

                    string sql5 = "CREATE TABLE IF NOT EXISTS \"" + CreateTableName + "_Kassenbericht\" (" +
                        "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                        "Gruppe TEXT NOT NULL, " +
                        "Betrag REAL NOT NULL, " +
                        "EinnahmeAusgabe TEXT NOT NULL)";

                    using (SQLiteCommand command = new SQLiteCommand(sql1, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(sql2, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(sql3, connection))
                    {
                        command.Parameters.AddWithValue("@Name", CreateTableName);
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(sql4, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (SQLiteCommand command = new SQLiteCommand(sql5, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Projekt erfolgreich erstellt", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadProjects();
                EinstellungenViewModel.Instance.LoadProjekte();   
            }

            private bool CanExecuteCreateTable(object obj)
            {
                // Verbindung zur SQLite-Datenbank herstellen
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();

                    // SQL-Abfrage vorbereiten
                    string sql = "SELECT COUNT(*) FROM Projekte WHERE Name=@Name";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        // Parameter hinzufügen
                        command.Parameters.AddWithValue("@Name", CreateTableName);

                        // Anzahl der gefundenen Datensätze abrufen
                        long count = (long)command.ExecuteScalar();

                        // Wenn mindestens ein Datensatz gefunden wurde, ist die Texteingabe in der Tabelle vorhanden
                        if (count > 0)
                        {
                            return false;
                        }
                        else
                            return true;
                    }
                }
            }

            //Constructor
            public HomeViewModel()
            {
                Export = ExportViewModel.Instance;
                Einstellungen = EinstellungenViewModel.Instance;
                LoadProjects();
                CreateTableCommand = new ViewModelCommand(ExecuteCreateTable, CanExecuteCreateTable);
            }
        }
}
