using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;

namespace OutlookTfs
{
    /// <summary>
    /// AppViewModel base class
    /// </summary>
    public class AppViewModel : INotifyPropertyChanged
    {
        private TfsTeamProjectCollection _tfsConnection;
        private ProjectInfo _tfsProjects;
        private string _tfsServer;
        private string _itemType = "Bug";
        private ObservableCollection<string> _itemTypes;
        private ObservableCollection<string> _users;
        private string _project;
        private string _areaPath;
        private string _iteration;
        private string _assignedTo;

        public string ItemType
        {
            get { return _itemType; }
            set
            {
                if (_itemType == value) return;
                _itemType = value;
                OnPropertyChanged("ItemType");
            }
        }

        public ObservableCollection<string> ItemTypes
        {
            get { return _itemTypes ?? (_itemTypes = new ObservableCollection<string> { _itemType }); }
            set
            {
                _itemTypes = value;
                OnPropertyChanged("ItemTypes");
            }
        }

        public string TfsServer
        {
            get { return _tfsServer; }
            set
            {
                if (_tfsServer == value) return;
                _tfsServer = value;
                OnPropertyChanged("TfsServer");
            }
        }

        public TfsTeamProjectCollection TfsConnection
        {
            get { return _tfsConnection; }
            set
            {
                if (value != null && _tfsConnection != null && _tfsConnection.Name == value.Name) return;
                _tfsConnection = value;
                OnPropertyChanged("TfsConnection");
            }
        }

        public ProjectInfo TfsProject
        {
            get { return _tfsProjects; }
            set
            {
                if (_tfsProjects != null && value != null && _tfsProjects.Name == value.Name) return;
                _tfsProjects = value;
                OnPropertyChanged("TfsProject");
            }
        }

        public string AreaPath
        {
            get { return _areaPath; }
            set
            {
                if (_areaPath == value) return;
                _areaPath = value;
                OnPropertyChanged("AreaPath");
            }
        }
        
        public string Iteration
        {
            get { return _iteration; }
            set
            {
                if (_iteration == value) return;
                _iteration = value;
                OnPropertyChanged("Iteration");
            }
        }

        //public string Project
        //{
        //    get { return _project; }
        //    set
        //    {
        //        if (_project == value) return;
        //        _project = value;
        //        OnPropertyChanged("Project");
        //    }
        //}
        public ObservableCollection<string> Users
        {
            get { return _users ?? (_users = new ObservableCollection<string>()); }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }

        public string AssignedTo
        {
            get { return _assignedTo; }
            set
            {
                if (_assignedTo == value) return;
                _assignedTo = value;
                OnPropertyChanged("AssignedTo");
            }
        }

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when property changed
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Commands
        public ICommand OkCommand { get; set; }

        public ICommand ChangeConnectionCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        #endregion
    }
}