using System.Windows;

namespace OutlookTfs
{
    /// <summary>
    /// Interaction logic for NewWorkItem.xaml
    /// </summary>
    public partial class NewWorkItem : Window, IView
    {
        public NewWorkItem()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            //var rd = new ResourceDictionary
            //{
            //    Source = new Uri("/OutlookTfs;component/NewWorkItem.xaml", UriKind.Relative)
            //};

            //Resources.MergedDictionaries.Add(rd);
        }
    }
}
