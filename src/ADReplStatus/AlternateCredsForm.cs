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
                var state = AppState.Instance;
                state.Username = UsernameTextBox.Text;
                state.Password = PasswordTextBox.Text;

                Dispose();

                Logger.Log($"Using alternate identity: {state.Username}");
            }
        }

        private void AlternateCredsForm_Load(object sender, EventArgs e)
        {
            if (AppState.Instance.DarkMode)
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
