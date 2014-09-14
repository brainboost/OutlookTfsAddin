using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NetOffice.OutlookApi;
using NetOffice.OutlookApi.Enums;
using Action = NetOffice.OutlookApi.Action;
using Attachment = NetOffice.OutlookApi.Attachment;

namespace OutlookTfs
{
    public partial class OutlookTfsAddIn
    {
        // the prompt and action name
        private const string CreateNewPrompt = "New TFS WorkItem";
        private const string TfsServer = "http://vtom2010:8080/tfs/main";
        private const string TfsProject = "HELM";

        private Explorer _explorer;

        private void OutlookTfsAddInStartup(object sender, EventArgs e)
        {
            // cache the explorer object
            _explorer = (Explorer) Application.ActiveExplorer();

            // when an email selection changes this event will fire
            _explorer.SelectionChangeEvent += ExplorerSelectionChange;
        }

        // event fired when any selection changes.
        void ExplorerSelectionChange()
        {
            foreach (object selectedItem in _explorer.Selection)
            {
                // we only want to deal with selected mail items
                var item = selectedItem as MailItem;
                if (item != null)
                {
                    // see if the action already exists on mail item
                    Action newAction = item.Actions[CreateNewPrompt];

                    // and create it if it does not
                    if (newAction == null)
                    {
                        newAction = item.Actions.Add();
                        newAction.Name = CreateNewPrompt;
                        newAction.ShowOn = OlActionShowOn.olMenu;
                        newAction.Enabled = true;
                        item.Save();
                    }

                    // add the event handler for our action
                    item.CustomActionEvent += ItemCustomAction;
                }
            }
        }

        void ItemCustomAction(object action, object response, ref bool cancel)
        {
            try
            {
                var mailAction = (Action)action;
                switch (mailAction.Name)
                {
                    // only process the action we know about
                    case CreateNewPrompt:
                        try
                        {
                            var mailItem = _explorer.Selection[1] as MailItem;
                            if (mailItem != null)
                            {
                                var form = new CreateWorkItem();
                                var dialogRes = form.ShowDialog();
                                if (dialogRes == DialogResult.Cancel)
                                    return;
                                var tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsServer));
                                var store = (WorkItemStore)tfs.GetService(typeof(WorkItemStore));

                                WorkItemTypeCollection workItemTypes = store.Projects[TfsProject].WorkItemTypes;
                                WorkItemType wit = workItemTypes["bug"];
                                var workItem = new WorkItem(wit)
                                                   {
                                                       Title = mailItem.Subject,
                                                       Description = mailItem.Body,
                                                       //IterationPath = "Iteration 3",
                                                       AreaPath = "HELM",
                                                   };
                                //if (MessageBox.Show(mailItem.Body, "Text", MessageBoxButtons.OKCancel) ==
                                //    DialogResult.Cancel)
                                //    return;
                                var assigned = workItem.Fields["Assigned To"];
                                assigned.Value = tfs.AuthorizedIdentity.DisplayName;

                                foreach (Attachment mailattach in mailItem.Attachments)
                                {
                                    var file = Path.Combine(Environment.CurrentDirectory, mailattach.FileName);
                                    mailattach.SaveAsFile(file);

                                    workItem.Attachments.Add(
                                        new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(file,
                                                                                                        mailattach
                                                                                                            .DisplayName));
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
                        finally
                        {
                            cancel = true;
                        }
                        break;

                }
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void OutlookTfsAddInShutdown(object sender, EventArgs e)
        {
            MessageBox.Show(CreateNewPrompt);

        }


        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            Startup += OutlookTfsAddInStartup;
            Shutdown += OutlookTfsAddInShutdown;
        }

        #endregion
    }
}
