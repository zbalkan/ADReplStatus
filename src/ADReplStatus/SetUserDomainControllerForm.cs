using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADReplStatus
{
    public partial class SetUserDomainControllerForm : Form
    {
        public SetUserDomainControllerForm()
        {
            InitializeComponent();
        }

        private void SetForestNameButton_Click(object sender, EventArgs e)
        {
            var state = AppState.Instance;

            if (state.UseUserDomainController)
            {
                if (SetUserDomainControllerTextBox.Text.Length < 1)
                {
                    Logger.Log($"Clearing user specified domain controller and disabling global. Previous value:{state.UserDomainController}");

                    state.UseUserDomainController = false;
                    state.UserDomainController = string.Empty;
                }
                else
                {
                    Logger.Log($"Changing user specified domain controller to {SetUserDomainControllerTextBox.Text}");

                    state.UserDomainController = SetUserDomainControllerTextBox.Text;
                }

                Close();
                return;
            }

            if (SetUserDomainControllerTextBox.Text.Length < 1)
            {
                return;
            }

            Logger.Log($"Setting user specified domain controller to {SetUserDomainControllerTextBox.Text} and enabling global.");

            state.UseUserDomainController = true;
            state.UserDomainController = SetUserDomainControllerTextBox.Text;

            Close();
        }

        private void SetUserDomainControllerForm_Load(object sender, EventArgs e)
        {
            var state = AppState.Instance;

            if (state.DarkMode)
            {
                BackColor = Color.FromArgb(32, 32, 32);

                SetUserDomainControllerLabel.BackColor = Color.FromArgb(32, 32, 32);
                SetUserDomainControllerLabel.ForeColor = Color.White;

                SetUserDomainControllerTextBox.BackColor = Color.FromArgb(32, 32, 32);
                SetUserDomainControllerTextBox.ForeColor = Color.White;

                SetUserDomainControllerButton.BackColor = Color.FromArgb(32, 32, 32);
                SetUserDomainControllerButton.ForeColor = Color.White;
            }

            SetUserDomainControllerTextBox.Text = state.UseUserDomainController
                ? state.UserDomainController
                : string.Empty;
        }

        private void SetUserDomainControllerTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetForestNameButton_Click(this, EventArgs.Empty);
            }
        }
    }
}
