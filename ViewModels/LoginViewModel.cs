using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_LoginForm.Model;
using WPF_LoginForm.Repositories;
using WPF_LoginForm.ViewModels;

namespace WPF_LoginForm.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        //Fields
        private string _username;
        private SecureString _password;
        private string _errorMessage;
        private bool _istViewVisible = true;

        private IUserRepository userRepository;

        //Properties
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public String ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsViewVisible
        {
            get { return _istViewVisible; }
            set
            {
                _istViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        //-> Commands
        public ICommand LoginCommand { get; }
        public ICommand RecoverPasswordCommand { get; }
        public ICommand ShowPasswordCommand { get; }
        public ICommand RememberPasswordCommand { get; }

        //Constructor
        public LoginViewModel()
        {
            userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new ViewModelCommand(p => ExecuteRecoverPassCommand("", ""));
        }

        private void ExecuteRecoverPassCommand(string username, string email) // TODO: Passwort Reset
                                                                              // Fügen Sie eine neue Seite oder ein neues Fenster hinzu, auf der / in dem Benutzer ihr Passwort zurücksetzen können.
                                                                              // Auf dieser Seite sollte ein Formular enthalten sein, in dem Benutzer ihre E-Mail-Adresse eingeben können.
                                                                              // Sobald das Formular abgeschickt wurde, senden Sie eine E-Mail an die angegebene Adresse mit einem temporären Passwort oder einem Link zum Zurücksetzen des Passworts.
                                                                              // Speichern Sie das temporäre Passwort in der SQLite-Datenbank für den Benutzer, dessen E-Mail-Adresse angegeben wurde.
                                                                              // Wenn der Benutzer auf den Link in der E-Mail klickt oder sich mit dem temporären Passwort anmeldet, leiten Sie sie zu einer Seite weiter, auf der sie ihr Passwort ändern können.
                                                                              // Nach erfolgreicher Änderung des Passworts sollte das temporäre Passwort in der Datenbank gelöscht werden.
                                                                              // Stellen Sie sicher, dass alle erforderlichen Überprüfungen und Sicherheitsmaßnahmen (z. B. Überprüfung der E-Mail-Adresse, Überprüfung des temporären Passworts usw.) in der Anwendung implementiert sind.
         
        {
            throw new NotImplementedException();
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3 || Password == null || Password.Length < 3)
                validData = false;
            else
                validData = true;
            return validData;
        }

        private void ExecuteLoginCommand(object obj)
        {
            var isValidUser = userRepository.AuthenticateUser(new System.Net.NetworkCredential(Username, Password));
            if (isValidUser)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(Username), null);
                IsViewVisible = false;
            }
            else
            {
                ErrorMessage = "* Ungültiger Username oder Passwort";
            }
        }
    }
}
