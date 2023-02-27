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
            private object _aktuellesProjekt;
            private string _createTableName;
            private List<ProjectModel> _projects;

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
            public string CreateTableName
            {
                get { return _createTableName; }
                set
                {
                    _createTableName = value;
                    OnPropertyChanged(nameof(CreateTableName));
                }
            }

            public List<ProjectModel> Projects
            {
                get { return _projects; }
                set { _projects = value; OnPropertyChanged(nameof(Projects)); }
            }

            public object AktuellesProjekt
            {
                get { return _aktuellesProjekt; }
                set
                {
                    _aktuellesProjekt = value;
                    OnPropertyChanged(nameof(AktuellesProjekt));
                
            }
            }

            //Commands
            public ICommand CreateTableCommand { get; }

            //Methods
            private void LoadProjects()
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Projekte", connection))
                    {
                        SQLiteDataReader reader = command.ExecuteReader();
                        _projects = new List<ProjectModel>();
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["ID"]);
                            int name = Convert.ToInt32(reader["Name"]);
                            _projects.Add(new ProjectModel { ID = id, Name = name });
                        }
                    }
                }
            }

            public int PreviousProjectName
            {
                get
                {
                    ProjectModel project = (ProjectModel)_aktuellesProjekt;
                    return project.PreviousProject();
                }
            }

            public void ExecuteCreateTable(object obj)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=database.db;Version=3;"))
                {
                    connection.Open();

                    string sql1 = "CREATE TABLE IF NOT EXISTS \"" + CreateTableName + "_Mitglieder\" (" +
                        "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                        "Geburtsdatum TEXT NOT NULL, " +
                        "Nachname TEXT NOT NULL, " +
                        "Vorname TEXT NOT NULL, " +
                        "Mitgliedsstatus TEXT NOT NULL, " +
                        "Beitrag TEXT NOT NULL, " +
                        "Zahlstatus TEXT NOT NULL)";

                    string sql2 = "CREATE TABLE IF NOT EXISTS \"" + CreateTableName + "_Kassenbuch\" (" +
                        "ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE, " +
                        "Datum TEXT NOT NULL, " +
                        "Buchungsnummer INTEGER NOT NULL, " +
                        "Position TEXT NOT NULL, " +
                        "KontoKasse TEXT NOT NULL, " +
                        "EinnahmeAusgabe TEXT NOT NULL, " +
                        "Steuerklasse INTEGER NOT NULL)";

                    string sql3 = "INSERT INTO Projekte (Name) VALUES (@Name)";

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
                }

                MessageBox.Show("Projekt erfolgreich erstellt", "Erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                CreateTableCommand = new ViewModelCommand(ExecuteCreateTable, CanExecuteCreateTable);
                LoadProjects();
                _aktuellesProjekt = _projects[Projects.Count - 1];
            }
        }
}
