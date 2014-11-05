using System;
using System.Windows.Forms;
using Microsoft.TeamFoundation.Client;

namespace OutlookTfs
{
    public partial class CreateWorkItem : Form
    {
        public CreateWorkItem()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            var tfsPp = new TeamProjectPicker(TeamProjectPickerMode.MultiProject, false, new UICredentialsProvider());
            tfsPp.ShowDialog();
           // tfsPp.SelectedTeamProjectCollection
        }
    }
}
