using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Attachment = Microsoft.Office.Interop.Outlook.Attachment;
using MessageBox = System.Windows.MessageBox;

namespace OutlookTfs
{
    public class Presenter : IPresenter
    {
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
            ViewModel.Title = mailItem.Subject;
            ViewModel.Comment = mailItem.Body;
            ViewModel.Attachments = new ObservableCollection<AttachModel>();
            foreach (Attachment mailattach in mailItem.Attachments)
            {
                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), mailattach.DisplayName);
                mailattach.SaveAsFile(file);
                ViewModel.Attachments.Add(new AttachModel
                {
                    Path = file,
                    Comment = mailattach.DisplayName,
                    Chosen = true
                });
            }

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
                    Title = ViewModel.Title,
                    Description = ViewModel.Comment,
                    IterationPath = ViewModel.Iteration,
                    AreaPath = ViewModel.AreaPath,
                };
                workItem["Priority"] = ViewModel.Priority;
                var assigned = workItem.Fields["Assigned To"];
                if (assigned != null)
                    assigned.Value = ViewModel.AssignedTo;
                // create file attachments
                foreach (var attach in ViewModel.Attachments.Where(a => a.Chosen))
                {
                    workItem.Attachments.Add(
                        new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(attach.Path, attach.Comment));
                }
                var validationResult = workItem.Validate();
                if (validationResult.Count == 0)
                {
                    workItem.Save();
                    if (MessageBox.Show(string.Format("Created bug {0}", workItem.Id)) == MessageBoxResult.OK)
                        Dispose();
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
                    var store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));
                    Project selectedProject = null;
                    if (store != null && store.Projects != null)
                    {
                        WorkItemTypeCollection workItemTypes = store.Projects[proj.Name].WorkItemTypes;
                        ViewModel.ItemTypes = new ObservableCollection<string>(workItemTypes
                            .Cast<WorkItemType>()
                            .Select(w => w.Name));
                        var ims = tfs.GetService<IIdentityManagementService>();
                        var members = ims.ReadIdentity(GroupWellKnownDescriptors.EveryoneGroup, MembershipQuery.Expanded,
                                ReadIdentityOptions.None)
                                .Members;
                        var nodeMembers = ims.ReadIdentities(members, MembershipQuery.Expanded, ReadIdentityOptions.TrueSid)
                            .Where(m => m.IsActive && !m.IsContainer)
                            .ToArray();
                        ViewModel.Users = new ObservableCollection<string>(nodeMembers.Select(g => g.DisplayName));
                        selectedProject = store.Projects.Cast<Project>().FirstOrDefault(p => p.Name == proj.Name);
                    }
                    if (selectedProject == null) return;
                    var iters = new ObservableCollection<string> { proj.Name };
                    var areas = new ObservableCollection<string> { proj.Name };
                    // Area paths
                    if (selectedProject.AreaRootNodes != null)
                    {
                        foreach (Node area in selectedProject.AreaRootNodes)
                        {
                            areas.Add(area.Path);
                            foreach (Node item in area.ChildNodes)
                            {
                                areas.Add(item.Path);
                            }
                        }
                    }
                    ViewModel.Areas = areas;

                    // Iterations
                    if (selectedProject.IterationRootNodes != null)
                    {
                        foreach (Node node in selectedProject.IterationRootNodes)
                        {
                            iters.Add(node.Path);
                            foreach (Node item in node.ChildNodes)
                            {
                                iters.Add(item.Path);
                            }
                        }
                    }
                    ViewModel.Iterations = iters;
                    if (iters.Count > 0)
                        ViewModel.Iteration = iters[0];
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

        public void Dispose()
        {
            ((Window)View).Close();
        }
    }
}