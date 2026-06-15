using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ADReplStatus
{
    public partial class SetForestNameForm : Form
    {
        public SetForestNameForm()
        {
            InitializeComponent();
        }

        private void SetForestNameForm_Load(object sender, EventArgs e)
        {
            if (ADReplStatusForm.gDarkMode)
            {
                BackColor = Color.FromArgb(32, 32, 32);

                EnterForestNameLabel.BackColor = Color.FromArgb(32, 32, 32);

                EnterForestNameLabel.ForeColor = Color.White;

                SetForestNameTextBox.BackColor = Color.FromArgb(32, 32, 32);

                SetForestNameTextBox.ForeColor = Color.White;

                SetForestNameButton.BackColor = Color.FromArgb(32, 32, 32);

                SetForestNameButton.ForeColor = Color.White;

                SaveForestCheckBox.ForeColor = Color.White;
            }
        }

        private void SetForestNameButton_Click(object sender, EventArgs e)
        {
            if (SetForestNameTextBox.Text.Length > 0)
            {
                ADReplStatusForm.gForestName = SetForestNameTextBox.Text;

                if (SaveForestCheckBox.Checked)
                {
                    try
                    {
                        var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\ADREPLSTATUS", true);

                        if (key != null)
                        {
                            key.SetValue("ForestName", SetForestNameTextBox.Text);

                            key.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"ERROR: Failed to write to the HKCU\\ADREPLSTATUS registry key!\n{ex.Message}\n";

                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        if (ADReplStatusForm.gLoggingEnabled)
                        {
                            File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] {errorMessage}\n");
                        }
                    }
                }

                if (ADReplStatusForm.gLoggingEnabled)
                {
                    File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] Forest name set to: {ADReplStatusForm.gForestName}\n");
                }

                Dispose();
            }
        }
    }
}