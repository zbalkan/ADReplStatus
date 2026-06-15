using System;
using System.Drawing;
using System.Windows.Forms;

namespace ADReplStatus
{
    public partial class AlternateCredsForm : Form
    {
        public AlternateCredsForm()
        {
            InitializeComponent();
        }

        private void AlternateCredsButton_Click(object sender, EventArgs e)
        {
            if (UsernameTextBox.Text.Length > 0 && PasswordTextBox.Text.Length > 0)
            {
                ADReplStatusForm.gUsername = UsernameTextBox.Text;

                ADReplStatusForm.gPassword = PasswordTextBox.Text;

                Dispose();

                if (ADReplStatusForm.gLoggingEnabled)
                {
                    System.IO.File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] Using alternate identity: {ADReplStatusForm.gUsername}\n");
                }
            }
        }

        private void AlternateCredsForm_Load(object sender, EventArgs e)
        {
            if (ADReplStatusForm.gDarkMode)
            {
                BackColor = Color.FromArgb(32, 32, 32);

                foreach (var control in Controls)
                {
                    switch (control)
                    {
                        case Label _:
                            ((Label)control).BackColor = Color.FromArgb(32, 32, 32);

                            ((Label)control).ForeColor = Color.White;
                            break;

                        case TextBox _:
                            ((TextBox)control).BackColor = Color.FromArgb(32, 32, 32);

                            ((TextBox)control).ForeColor = Color.White;
                            break;

                        case Button _:
                            ((Button)control).BackColor = Color.FromArgb(32, 32, 32);

                            ((Button)control).ForeColor = Color.White;
                            break;
                    }
                }
            }
        }
    }
}