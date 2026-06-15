using BrightIdeasSoftware;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ADReplStatus
{
    public partial class ADReplStatusForm : Form
    {
        private readonly AppState _state = AppState.Instance;
        private readonly ReplicationService _replicationService = new ReplicationService();

        public ADReplStatusForm()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ProgressPercentLabel.Visible = true;
            ProgressPercentLabel.Text = "0%";
            Text = $"AD Replication Status Tool - {_state.ForestName}";
            _state.DCs.Clear();

            foreach (var control in Controls)
            {
                if (control is Button button)
                {
                    button.Enabled = false;
                }
            }

            backgroundWorker1.RunWorkerAsync();
        }

        private void ADReplStatusForm_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(RefreshButton, "Refresh Replication Status");
            toolTip1.SetToolTip(EnableLoggingButton, "Enable Logging");
            toolTip1.SetToolTip(SetForestButton, "Manually Set Forest");
            toolTip1.SetToolTip(AlternateCredsButton, "Provide Alternate Credentials");
            toolTip1.SetToolTip(ErrorsOnlyButton, "Show Errors Only");

            try
            {
                SettingsService.LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occured while trying to read app settings from the registry!\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (_state.DarkMode)
            {
                SetDarkMode();
            }
            else
            {
                SetLightMode();
            }

            if (string.IsNullOrEmpty(_state.ForestName))
            {
                try
                {
                    _state.ForestName = SettingsService.DetectForestName();
                }
                catch
                {
                    MessageBox.Show("Unable to detect AD forest. You will need to manually enter the AD forest you wish to scan using the 'Manually Set Forest' button.\nThis happens on non-domain joined computers as well as hybrid or Azure AD domain-joined machines.", "Forest Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ADReplStatusForm_Resize(object sender, EventArgs e)
        {
            treeListView1.Top = 68;
            treeListView1.Left = 12;
            treeListView1.Width = Width - 40;
            treeListView1.Height = Height - 119;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var reporter = new BackgroundWorkerProgressReporter(backgroundWorker1);
            var results = _replicationService.DiscoverReplicationStatus(reporter);
            if (results != null)
            {
                _state.DCs = results;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            treeListView1.CanExpandGetter = x => x is ADREPLDC;

            treeListView1.ChildrenGetter = x => ((ADREPLDC)x).ReplicationPartners;

            DCNameColumn.AspectGetter = x =>
            {
                switch (x) { case ADREPLDC dc: return dc.Name; case ReplicationNeighbor n: return n.SourceServer; default: return null; }
            };
            DomainNameColumn.AspectGetter = x =>
            {
                switch (x) { case ADREPLDC dc: return dc.DomainName; default: return null; }
            };
            SiteColumn.AspectGetter = x =>
            {
                switch (x) { case ADREPLDC dc: return dc.Site; default: return null; }
            };
            DiscoveryIssuesColumn.AspectGetter = x =>
            {
                switch (x) { case ADREPLDC dc: return dc.DiscoveryIssues; default: return null; }
            };
            IsGCColumn.AspectGetter = x =>
            {
                switch (x) { case ADREPLDC dc: return dc.IsGC; default: return null; }
            };
            IsRODCColumn.AspectGetter = x =>
            {
                switch (x) { case ADREPLDC dc: return dc.IsRODC; default: return null; }
            };
            SourceServerColumn.AspectGetter = x =>
            {
                switch (x) { case ReplicationNeighbor n: return n.SourceServer; default: return null; }
            };
            PartitionNameColumn.AspectGetter = x =>
            {
                switch (x) { case ReplicationNeighbor n: return n.PartitionName; default: return null; }
            };
            ConsecutiveFailureCountColumn.AspectGetter = x =>
            {
                switch (x) { case ReplicationNeighbor n: return n.ConsecutiveFailureCount; default: return null; }
            };
            LastSuccessfulSyncColumn.AspectGetter = x =>
            {
                switch (x) { case ReplicationNeighbor n: return n.LastSuccessfulSync; default: return null; }
            };
            LastSyncResultColumn.AspectGetter = x =>
            {
                switch (x) { case ReplicationNeighbor n: return n.LastSyncResult; default: return null; }
            };
            LastSyncMessageColumn.AspectGetter = x =>
            {
                switch (x) { case ReplicationNeighbor n: return n.LastSyncMessage; default: return null; }
            };

            treeListView1.SetObjects(_state.DCs);

            ProgressPercentLabel.Visible = false;

            foreach (var control in Controls)
            {
                if (control is Button button)
                {
                    button.Enabled = true;
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Logger.Log(e.UserState.ToString());

            if (e.UserState.ToString().StartsWith("ERROR:"))
            {
                MessageBox.Show(e.UserState.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.UserState.ToString().Equals("UPDATEPERCENT"))
            {
                ProgressPercentLabel.Text = $"{e.ProgressPercentage}%";
            }
        }

        private void EnableLoggingButton_Click(object sender, EventArgs e)
        {
            _state.LoggingEnabled = !_state.LoggingEnabled;

            if (_state.LoggingEnabled)
            {
                toolTip1.SetToolTip(EnableLoggingButton, "Disable Logging");
                EnableLoggingButton.BackColor = SystemColors.ControlDark;

                DateTime now = DateTime.Now;
                _state.LogfileName = $"adreplstatus_{now.Month}.{now.Day}.{now.Year}.{now.Hour}.{now.Minute}.{now.Second}.log";

                Logger.Log("Logging enabled.");
            }
            else
            {
                toolTip1.SetToolTip(EnableLoggingButton, "Enable Logging");
                EnableLoggingButton.BackColor = _state.DarkMode ? Color.FromArgb(32, 32, 32) : SystemColors.Control;

                Logger.Log("Logging disabled.");
            }
        }

        private void SetForestButton_Click(object sender, EventArgs e)
        {
            Logger.Log("SetForestName button was clicked.");

            using (var setForestNameForm = new SetForestNameForm())
            {
                setForestNameForm.ShowDialog();
            }
        }

        private void AlternateCredsButton_Click(object sender, EventArgs e)
        {
            Logger.Log("AlternateCreds button was clicked.");

            using (var alternateCredsForm = new AlternateCredsForm())
            {
                alternateCredsForm.ShowDialog();
            }
        }

        private void treeListView1_FormatRow(object sender, FormatRowEventArgs e)
        {
            switch (e.Model)
            {
                case ADREPLDC dc:
                    if (dc.DiscoveryIssues)
                    {
                        e.Item.BackColor = Color.Red;
                        e.Item.ForeColor = Color.White;
                    }
                    else if (_state.DarkMode)
                    {
                        e.Item.ForeColor = Color.White;
                    }
                    break;

                case ReplicationNeighbor neighbor:
                    if (neighbor.ConsecutiveFailureCount > 0)
                    {
                        e.Item.BackColor = Color.Red;
                        e.Item.ForeColor = Color.White;
                    }
                    else if (_state.DarkMode)
                    {
                        e.Item.ForeColor = Color.White;
                    }
                    break;
            }
        }

        private void ErrorsOnlyButton_Click(object sender, EventArgs e)
        {
            _state.ErrorsOnly = !_state.ErrorsOnly;

            if (_state.ErrorsOnly)
            {
                toolTip1.SetToolTip(ErrorsOnlyButton, "Show Everything");
                ErrorsOnlyButton.BackColor = SystemColors.ControlDark;
                treeListView1.ExpandAll();

                treeListView1.ModelFilter = new ModelFilter(x =>
                {
                    switch (x)
                    {
                        case ADREPLDC adrepldc:
                            return adrepldc.DiscoveryIssues;

                        case ReplicationNeighbor rn:
                            return rn.ConsecutiveFailureCount > 0;

                        default:
                            return false;
                    }
                });
            }
            else
            {
                toolTip1.SetToolTip(ErrorsOnlyButton, "Show Errors Only");

                ErrorsOnlyButton.BackColor = _state.DarkMode
                    ? Color.FromArgb(32, 32, 32)
                    : SystemColors.Control;

                treeListView1.ModelFilter = null;
            }
        }

        private void DCNameColumn_RightClick(object sender, CellRightClickEventArgs e)
        {
            try
            {
                if (e.Column.Text == "DC Name")
                {
                    if (treeListView1.SelectedItem.Text != "")
                    {
                        ContextMenuStrip diagnosticMenu = new ContextMenuStrip();
                        diagnosticMenu.ItemClicked += diagnosticMenuSelector;

                        diagnosticMenu.Items.Add(new ToolStripMenuItem("Ping"));
                        diagnosticMenu.Items.Add(new ToolStripMenuItem("Initiate RDP connection"));
                        diagnosticMenu.Items.Add(new ToolStripMenuItem("Enter PowerShell session"));
                        diagnosticMenu.Items.Add(new ToolStripMenuItem("Port Tester"));

                        e.MenuStrip = diagnosticMenu;
                    }
                }
            }
            catch
            {
            }
        }

        private void diagnosticMenuSelector(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.ToString())
            {
                case "Ping":
                    Logger.Log("Diagnostic ping menu opened.");
                    diagnosticPing(sender, e);
                    break;

                case "Initiate RDP connection":
                    diagnosticRdp(sender, e);
                    break;

                case "Enter PowerShell session":
                    diagnosticPSSession(sender, e);
                    break;

                case "Port Tester":
                    diagnosticNetworkTester(sender, e);
                    break;
            }
        }

        private void diagnosticPing(object sender, ToolStripItemClickedEventArgs e)
        {
            string destination = treeListView1.SelectedItem.Text;

            if (destination != "")
            {
                using (var dialog = new Form())
                {
                    dialog.Text = "Ping Test";
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.MaximizeBox = false;
                    dialog.MinimizeBox = false;
                    dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dialog.ShowInTaskbar = false;
                    dialog.Width = 290;
                    dialog.Height = 150;

                    var ipv4Button = new Button
                    {
                        Text = "IPv4",
                        Location = new Point(10, 20)
                    };
                    ipv4Button.Click += (s, ev) => RunPing(destination, AddressFamily.InterNetwork, dialog);

                    var ipv6Button = new Button
                    {
                        Text = "IPv6",
                        Location = new Point(180, 20)
                    };
                    ipv6Button.Click += (s, ev) => RunPing(destination, AddressFamily.InterNetworkV6, dialog);

                    var statusTextBox = new TextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        Location = new Point(10, 60),
                        Width = dialog.Width - 45,
                        Height = dialog.Height - 110,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
                    };

                    dialog.Controls.Add(ipv4Button);
                    dialog.Controls.Add(ipv6Button);
                    dialog.Controls.Add(statusTextBox);

                    if (_state.DarkMode)
                    {
                        dialog.BackColor = Color.FromArgb(32, 32, 32);
                        foreach (var control in dialog.Controls)
                        {
                            switch (control)
                            {
                                case Label label:
                                    label.BackColor = Color.FromArgb(32, 32, 32);
                                    label.ForeColor = Color.White;
                                    break;

                                case TextBox textBox:
                                    textBox.BackColor = Color.FromArgb(32, 32, 32);
                                    textBox.ForeColor = Color.White;
                                    break;

                                case Button btn:
                                    btn.BackColor = Color.FromArgb(32, 32, 32);
                                    btn.ForeColor = Color.White;
                                    break;

                                case CheckBox checkBox:
                                    checkBox.BackColor = Color.FromArgb(32, 32, 32);
                                    checkBox.ForeColor = Color.White;
                                    break;

                                case RadioButton radioButton:
                                    radioButton.BackColor = Color.FromArgb(32, 32, 32);
                                    radioButton.ForeColor = Color.White;
                                    break;

                                case ListBox listBox:
                                    listBox.BackColor = Color.FromArgb(32, 32, 32);
                                    listBox.ForeColor = Color.White;
                                    break;
                            }
                        }
                    }

                    dialog.ShowDialog(this);
                }
            }
        }

        private async void RunPing(string destination, AddressFamily addressFamily, Form dialog)
        {
            try
            {
                if (!IPAddress.TryParse(destination, out IPAddress address))
                {
                    var entry = await Dns.GetHostEntryAsync(destination);
                    address = entry.AddressList.FirstOrDefault(a => a.AddressFamily == addressFamily) ?? throw new Exception($"No {addressFamily} address found for {destination}");
                }

                using (var p = new Ping())
                {
                    var reply = await p.SendPingAsync(address, 5000, new byte[1], new PingOptions(64, true));
                    switch (reply.Status)
                    {
                        case IPStatus.Success:
                            {
                                string protocol = addressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
                                var statusTextBox = (TextBox)dialog.Controls[2];
                                statusTextBox.Clear();
                                statusTextBox.AppendText($"Ping to {destination} using {protocol} ({reply.Address}) successful.\n");

                                Logger.Log(statusTextBox.Text);
                                break;
                            }

                        default:
                            throw new Exception(reply.Status.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                dialog.Invoke(new Action(() =>
                {
                    string errorMessage = $"Ping failed!\n{ex.Message}\n";
                    var statusTextBox = (TextBox)dialog.Controls[2];
                    statusTextBox.Clear();
                    statusTextBox.AppendText($"{errorMessage}\n");
                    Logger.Log(errorMessage);
                }));
            }
        }

        private void diagnosticRdp(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                Logger.Log($"Initiating RDP connection to {treeListView1.SelectedItem.Text}.");

                string args = $"/v {treeListView1.SelectedItem.Text}";
                Process.Start("mstsc.exe", args);
            }
            catch (Exception ex)
            {
                string errorMessage = $"ERROR: RDP to {treeListView1.SelectedItem.Text} failed!\n{ex.Message}\n";
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(errorMessage);
            }
        }

        private void diagnosticPSSession(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                Logger.Log($"Initiating remote powershell session to {treeListView1.SelectedItem.Text}.");

                string powershellArgs = $"-NoExit $Cred = Get-Credential;Enter-PSSession -ComputerName {treeListView1.SelectedItem.Text} -Credential $Cred";
                Process.Start("powershell.exe", powershellArgs);
            }
            catch (Exception ex)
            {
                string errorMessage = $"ERROR: Enter-PsSession -ComputerName {treeListView1.SelectedItem.Text} failed!\n{ex.Message}\n";
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(errorMessage);
            }
        }

        private void diagnosticNetworkTester(object sender, ToolStripItemClickedEventArgs e)
        {
            _state.Target = treeListView1.SelectedItem.Text;

            Logger.Log("Port Tester button was clicked.");

            using (var portTesterForm = new PortTester())
            {
                portTesterForm.ShowDialog();
            }
        }

        private void DarkModeButton_Click(object sender, EventArgs e)
        {
            _state.DarkMode = !_state.DarkMode;

            if (_state.DarkMode)
            {
                SetDarkMode();
            }
            else
            {
                SetLightMode();
            }

            try
            {
                SettingsService.SaveDarkMode(_state.DarkMode);
            }
            catch (Exception ex)
            {
                string errorMessage = $"ERROR: Failed to write to the HKCU\\ADREPLSTATUS registry key!\n{ex.Message}\n";
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log(errorMessage);
            }
        }

        private void SetDarkMode()
        {
            toolTip1.SetToolTip(DarkModeButton, "Light Mode");

            BackColor = Color.FromArgb(32, 32, 32);

            foreach (var control in Controls)
            {
                if (control is Button btn)
                    btn.BackColor = Color.FromArgb(32, 32, 32);

                if (control is Label label)
                {
                    label.BackColor = Color.FromArgb(32, 32, 32);
                    label.ForeColor = Color.White;
                }
            }

            if (_state.LoggingEnabled)
                EnableLoggingButton.BackColor = SystemColors.ControlDark;

            if (_state.ErrorsOnly)
                ErrorsOnlyButton.BackColor = SystemColors.ControlDark;

            treeListView1.BackColor = Color.FromArgb(32, 32, 32);

            foreach (OLVColumn item in treeListView1.Columns)
            {
                var headerstyle = new HeaderFormatStyle();
                headerstyle.SetBackColor(Color.FromArgb(32, 32, 32));
                headerstyle.SetForeColor(Color.White);
                item.HeaderFormatStyle = headerstyle;
            }
        }

        private void SetLightMode()
        {
            toolTip1.SetToolTip(DarkModeButton, "Dark Mode");

            BackColor = SystemColors.Control;

            foreach (var control in Controls)
            {
                if (control is Button btn)
                    btn.BackColor = SystemColors.Control;

                if (control is Label label)
                {
                    label.BackColor = SystemColors.Control;
                    label.ForeColor = SystemColors.ControlText;
                }
            }

            if (_state.LoggingEnabled)
                EnableLoggingButton.BackColor = SystemColors.ControlDark;

            if (_state.ErrorsOnly)
                ErrorsOnlyButton.BackColor = SystemColors.ControlDark;

            treeListView1.BackColor = SystemColors.Window;

            foreach (OLVColumn item in treeListView1.Columns)
            {
                var headerstyle = new HeaderFormatStyle();
                headerstyle.SetBackColor(SystemColors.Window);
                headerstyle.SetForeColor(SystemColors.ControlText);
                item.HeaderFormatStyle = headerstyle;
            }
        }

        private void SetDcButton_Click(object sender, EventArgs e)
        {
            Logger.Log("SetUserDomainController button was clicked.");

            using (var setUserDCForm = new SetUserDomainControllerForm())
            {
                setUserDCForm.ShowDialog();
            }
        }
    }

    internal class BackgroundWorkerProgressReporter : IProgressReporter
    {
        private readonly BackgroundWorker _worker;

        public BackgroundWorkerProgressReporter(BackgroundWorker worker)
        {
            _worker = worker;
        }

        public void ReportProgress(int percent, string message)
        {
            _worker.ReportProgress(percent, message);
        }
    }
}
