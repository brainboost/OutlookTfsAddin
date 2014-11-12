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
        private ProjectInfo[] _tfsProjects;
        private string _tfsServer;
        private string _itemType = "bug";
        private ObservableCollection<string> _itemTypes;

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
            get { return _itemTypes ?? (_itemTypes = new ObservableCollection<string> {"bug", "task"}); }
            set { _itemTypes = value; }
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

        public ProjectInfo[] TfsProjects
        {
            get { return _tfsProjects; }
            set
            {
                if (_tfsProjects == value) return;
                _tfsProjects = value;
                OnPropertyChanged("TfsProjects");
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