using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (ADReplStatusForm.gUseUserDomainController)
            {
                //The user cleared out the input box and clicked set
                if (SetUserDomainControllerTextBox.Text.Length < 1)
                {
                    if (ADReplStatusForm.gLoggingEnabled)
                    {
                        System.IO.File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] Clearing user specified domain controller and disabling global. Previous value:{ADReplStatusForm.gUserDomainController}\n");
                    }

                    ADReplStatusForm.gUseUserDomainController = false;
                    ADReplStatusForm.gUserDomainController = string.Empty;
                }
                else
                {
                    if (ADReplStatusForm.gLoggingEnabled)
                    {
                        System.IO.File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] Changing user specified domain controller to {SetUserDomainControllerTextBox.Text}\n");
                    }

                    ADReplStatusForm.gUserDomainController = SetUserDomainControllerTextBox.Text;
                }

                this.Close();
                return;
            }

            if (SetUserDomainControllerTextBox.Text.Length < 1)
            {
                return;
            }

            if (ADReplStatusForm.gLoggingEnabled)
            {
                System.IO.File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] Setting user specified domain controller to {SetUserDomainControllerTextBox.Text} and enabling global.\n");
            }

            ADReplStatusForm.gUseUserDomainController = true;
            ADReplStatusForm.gUserDomainController = SetUserDomainControllerTextBox.Text;

            this.Close();
        }

        private void SetUserDomainControllerForm_Load(object sender, EventArgs e)
        {
            if (ADReplStatusForm.gDarkMode)
            {
                this.BackColor = Color.FromArgb(32, 32, 32);

                SetUserDomainControllerLabel.BackColor = Color.FromArgb(32, 32, 32);
                SetUserDomainControllerLabel.ForeColor = Color.White;

                SetUserDomainControllerTextBox.BackColor = Color.FromArgb(32, 32, 32);
                SetUserDomainControllerTextBox.ForeColor = Color.White;

                SetUserDomainControllerButton.BackColor = Color.FromArgb(32, 32, 32);
                SetUserDomainControllerButton.ForeColor = Color.White;
            }

            SetUserDomainControllerTextBox.Text = ADReplStatusForm.gUseUserDomainController
                ? ADReplStatusForm.gUserDomainController
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