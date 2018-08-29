#region

using System;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser.Processor
{
    internal partial class EditUser : Form
    {
        private readonly User newUser;

        public EditUser(User user)
        {
            newUser = user;
            InitializeComponent();

            foreach(var value in Enum.GetValues(typeof(Status)))
                cbStatus.Items.Add(value);

            txtName.Text = newUser.UserName;
            txtEmail.Text = newUser.Email;
            dtExpiration.Value = newUser.SubscriptionEnd;
            cbStatus.SelectedItem = newUser.Status;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            newUser.UserName = txtName.Text;
            newUser.Email = txtEmail.Text;
            newUser.SubscriptionEnd = dtExpiration.Value;
            newUser.Status = (Status) cbStatus.SelectedItem;
            DialogResult = DialogResult.OK;
        }
    }
}