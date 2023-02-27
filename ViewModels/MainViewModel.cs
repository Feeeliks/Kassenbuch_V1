using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPF_LoginForm.Model;
using WPF_LoginForm.Repositories;
using WPF_LoginForm.ViewModels;
using WPF_LoginForm.ViewModels.Pages;
using System.Windows;
using FontAwesome.Sharp;
using System.Windows.Input;

namespace WPF_LoginForm.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        //Fields
        private UserAccountModel _currentUserAccount;
        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;

        private IUserRepository userRepository;

        //Properties
        public UserAccountModel CurrentUserAccount
        {
            get { return _currentUserAccount; }
            set
            {
                _currentUserAccount = value;
                OnPropertyChanged(nameof(CurrentUserAccount));
            }
        }

        public ViewModelBase CurrentChildView
        {
            get { return _currentChildView; }
            set 
            { 
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public IconChar Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        //--> Commands
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowKassenbuchViewCommand { get; }
        public ICommand ShowKassenberichtViewCommand { get; }
        public ICommand ShowMitgliederViewCommand { get; }
        public ICommand ShowExportViewCommand { get; }
        public ICommand ShowEinstellungenViewCommand { get; }

        //Constructor
        public MainViewModel()
        {
            userRepository = new UserRepository();
            CurrentUserAccount = new UserAccountModel();

            //Initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowKassenbuchViewCommand = new ViewModelCommand(ExecuteShowKassenbuchViewCommand);
            ShowKassenberichtViewCommand = new ViewModelCommand(ExecuteShowKassenberichtViewCommand);
            ShowMitgliederViewCommand = new ViewModelCommand(ExecuteShowMitgliederViewCommand);
            ShowExportViewCommand = new ViewModelCommand(ExecuteShowExportViewCommand);
            ShowEinstellungenViewCommand = new ViewModelCommand(ExecuteShowEinstellungenViewCommand);

            //Default view
            ExecuteShowHomeViewCommand(null);

            LoadCurrentUserData();

        }

        private void ExecuteShowEinstellungenViewCommand(object obj)
        {
            CurrentChildView = new EinstellungenViewModel();
            Caption = "Einstellungen";
            Icon = IconChar.Sun;
        }

        private void ExecuteShowMitgliederViewCommand(object obj)
        {
            CurrentChildView = new MitgliederViewModel();
            Caption = "Mitglieder";
            Icon = IconChar.UserGroup;
        }

        private void ExecuteShowExportViewCommand(object obj)
        {
            CurrentChildView = new ExportViewModel();
            Caption = "Export";
            Icon = IconChar.CircleDown;
        }

        private void ExecuteShowKassenberichtViewCommand(object obj)
        {
            CurrentChildView = new KassenberichtViewModel();
            Caption = "Kassenbericht";
            Icon = IconChar.PieChart;
        }

        private void ExecuteShowKassenbuchViewCommand(object obj)
        {
            CurrentChildView = new KassenbuchViewModel();
            Caption = "Kassenbuch";
            Icon = IconChar.Book;
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new HomeViewModel();
            Caption = "Startseite";
            Icon = IconChar.Home;
        }

        private void LoadCurrentUserData()
        {
            if (userRepository != null && Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity != null)
            {
                var user = userRepository.GetByUsername(Thread.CurrentPrincipal.Identity.Name);
                if (user != null)
                {
                    CurrentUserAccount.Username = user.Username;
                    CurrentUserAccount.DisplayName = $"{user.Name} {user.LastName}";
                    CurrentUserAccount.ProfilePicture = null;
                }
                else
                {
                    CurrentUserAccount.DisplayName = "Invalid user, not logged in";
                    //Hide child views
                }
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
    }
}
