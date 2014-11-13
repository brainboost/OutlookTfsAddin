using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using Microsoft.Office.Interop.Outlook;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MessageBox = System.Windows.Forms.MessageBox;

namespace OutlookTfs
{
    public class Presenter : IPresenter
    {
        private readonly IContainer _container;
        private MailItem _mailItem;

        public Presenter(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        public IView View { get; set; }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public AppViewModel ViewModel { get; set; }

        /// <summary>
        /// Can be called by container
        /// </summary>
        /// <param name="mailItem"></param>
        public virtual void Initialize(MailItem mailItem)
        {
            _mailItem = mailItem;
            ViewModel.OkCommand = new DelegateCommand(OkExecuteMethod, OkCanExecuteMethod);
            ViewModel.ChangeConnectionCommand = new DelegateCommand(ChangeConnectionExecuteMethod,
                ChangeConnectionCanExecuteMethod);
            ViewModel.CloseCommand = new DelegateCommand(wnd =>
            {
                var window = wnd as Window;
                if (window != null) window.Close();
            }, o => true);
            // Initialize the View (loads merged resource dictionary)
            View.Initialize();

            // Configure data context to use specified viewmodel. 
            View.DataContext = ViewModel;

            ((Window)View).ShowDialog();
        }

        /// <summary>
        /// Handles the execute command by resolving the provided command parameter 
        /// </summary>
        public virtual void OkExecuteMethod(object executeCommandParam)
        {
            var tfs = ViewModel.TfsConnection;
            var proj = ViewModel.TfsProject;
            var store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
            if (store != null && store.Projects != null)
            {
                WorkItemTypeCollection workItemTypes = store.Projects[proj.Name].WorkItemTypes;
                WorkItemType wit = workItemTypes[ViewModel.ItemType];
                var workItem = new WorkItem(wit)
                {
                    Title = _mailItem.Subject,
                    Description = _mailItem.Body,
                    IterationPath = ViewModel.Iteration,
                    AreaPath = ViewModel.AreaPath,
                    //IterationId = ViewModel.Iteration
                };
                var assigned = workItem.Fields["Assigned To"];
                if (assigned != null)
                    assigned.Value = ViewModel.AssignedTo;
                // create file attachments
                foreach (Microsoft.Office.Interop.Outlook.Attachment mailattach in _mailItem.Attachments)
                {
                    var file = Path.Combine(Environment.CurrentDirectory, mailattach.DisplayName);
                    mailattach.SaveAsFile(file);

                    workItem.Attachments.Add(
                        new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(file, mailattach.DisplayName));
                }
                var validationResult = workItem.Validate();

                if (validationResult.Count == 0)
                {
                    workItem.Save();
                    MessageBox.Show(string.Format("Created bug {0}", workItem.Id));
                }
                else
                {
                    var tt = new StringBuilder();
                    foreach (var res in validationResult)
                        tt.AppendLine(res.ToString());

                    MessageBox.Show(tt.ToString());
                }
            }
        }

        public virtual void ChangeConnectionExecuteMethod(object executeCommandParam)
        {
            var tfsPp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false) { AcceptButtonText = "OK" };
            if (tfsPp.ShowDialog() == DialogResult.Cancel) return;
            var tfs = tfsPp.SelectedTeamProjectCollection;
            ViewModel.TfsConnection = tfs;
            ViewModel.AssignedTo = tfs.AuthorizedIdentity.DisplayName;
            if (tfsPp.SelectedProjects != null && tfsPp.SelectedProjects.Length > 0)
            {
                var proj = tfsPp.SelectedProjects[0];
                if (proj != null)
                {
                    ViewModel.TfsProject = proj;
                    ViewModel.AreaPath = proj.Name;
                    var store = (WorkItemStore) tfs.GetService(typeof (WorkItemStore));
                    if (store != null && store.Projects != null)
                    {
                        WorkItemTypeCollection workItemTypes = store.Projects[proj.Name].WorkItemTypes;
                        ViewModel.ItemTypes = new ObservableCollection<string>(workItemTypes
                            .Cast<WorkItemType>()
                            .Select(w => w.Name));
                    }
                    var ims = tfs.GetService<IIdentityManagementService>();
                   // String[] searchValues = { "Team Foundation Valid Users" };
                    //var users = ims.ReadIdentities(IdentitySearchFactor.AccountName, searchValues, MembershipQuery.Expanded, ReadIdentityOptions.ExtendedProperties);
                    //Identity SIDS = ims.ReadIdentity(SearchFactor.Sid, "Team Foundation Valid Users", QueryMembership.Expanded); 
                    //Identity[] UserId = ims.ReadIdentities(SearchFactor.Sid, SIDS.Members, QueryMembership.None); 
                    TeamFoundationIdentity[] groups = ims.ListApplicationGroups(string.Empty, ReadIdentityOptions.None); 
                    ViewModel.Users = new ObservableCollection<string>(groups.Select(g => g.DisplayName));
                }
            }
        }

        /// <summary>
        /// Handles the can execute method.
        /// </summary>
        /// <param name="para">The param</param>
        /// <returns></returns>
        public virtual bool OkCanExecuteMethod(object para)
        {
            return true;
        }

        public virtual bool ChangeConnectionCanExecuteMethod(object para)
        {
            return true;
        }
    }
}