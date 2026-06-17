using System;
using System.Drawing;
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
            if (AppState.Instance.DarkMode)
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
                AppState.Instance.ForestName = SetForestNameTextBox.Text;

                if (SaveForestCheckBox.Checked)
                {
                    try
                    {
                        SettingsService.SaveForestName(SetForestNameTextBox.Text);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"ERROR: Failed to write to the HKCU\\ADREPLSTATUS registry key!\n{ex.Message}\n";
                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Logger.Log(errorMessage);
                    }
                }

                Logger.Log($"Forest name set to: {AppState.Instance.ForestName}");

                Dispose();
            }
        }
    }
}
