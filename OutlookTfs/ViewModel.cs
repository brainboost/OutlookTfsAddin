using System.ComponentModel;
using System.Windows.Input;

namespace OutlookTfs
{
    /// <summary>
    /// ViewModel base class
    /// </summary>
    public class ViewModel : IViewModel, INotifyPropertyChanged
    {
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

        public ICommand Command { get; set; }
    }
}