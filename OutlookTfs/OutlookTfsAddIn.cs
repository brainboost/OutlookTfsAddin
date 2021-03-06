﻿using System;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using Microsoft.TeamFoundation.Client;
using Action = Microsoft.Office.Interop.Outlook.Action;

namespace OutlookTfs
{
    public partial class OutlookTfsAddIn
    {
        // the prompt and action name
        private const string CreateNewPrompt = "New TFS WorkItem";
        private const string TfsServer = "http://vtom2010:8080/tfs/main";

        private Explorer _explorer;
        private SimpleContainer _container;

        private void OutlookTfsAddInStartup(object sender, EventArgs e)
        {
            _explorer = Application.ActiveExplorer();
            _container = new SimpleContainer() 
                .RegisterSingle(_explorer)
                .Register<TfsConnection>(container => TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(TfsServer)))
                .Register<IView>(container => new NewWorkItem())
                .Register<AppViewModel>(container => new AppViewModel())
                .Register<IPresenter>(container => new Presenter
                {
                    View = container.Create<IView>(),
                    ViewModel = container.Create<AppViewModel>()
                });
            // when an email selection changes this event will fire
            _explorer.SelectionChange += ExplorerSelectionChange;
        }

        // event fired when any selection changes.
        public void ExplorerSelectionChange()
        {
            foreach (var selectedItem in _explorer.Selection)
            {
                // we only want to deal with selected mail items
                var item = selectedItem as MailItem;
                if (item != null)
                {
                    // see if the action already exists on mail item
                    var newAction = item.Actions[CreateNewPrompt];

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
                    item.CustomAction += ItemCustomAction;
                }
            }
        }

        public void ItemCustomAction(object action, object response, ref bool cancel)
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
                                var form = _container.Create<IPresenter>();
                                form.Initialize(mailItem);
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
