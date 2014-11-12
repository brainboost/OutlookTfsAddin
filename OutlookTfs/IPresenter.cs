using Microsoft.Office.Interop.Outlook;

namespace OutlookTfs
{
    public interface IPresenter
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="mailItem"></param>
        void Initialize(MailItem mailItem);

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        IView View { get; set; }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        AppViewModel ViewModel { get; set; }
    }
}