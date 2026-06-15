using System;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADReplStatus
{
    public partial class PortTester : Form
    {
        private bool g_checkAll_rbtn_isChecked = false;
        private bool g_tnc_firstRun_onMulti = false;

        public PortTester()
        {
            InitializeComponent();
            target_txtbox.Text = $"{ADReplStatusForm.gTarget}";
            portProtocolList();
        }

        private void portOverride_btn_CheckedChanged(object sender, EventArgs e)
        {
            if (!port_label.Visible)
            {
                port_label.Visible = true;
                port_txtbox.Visible = true;
                manualTest_btn.Visible = true;
            }
            else
            {
                port_label.Visible = false;
                port_txtbox.Visible = false;
                manualTest_btn.Visible = false;
            }
        }

        private void portProtocolList()
        {
            protocolTesterListBox.CheckOnClick = true;

            const string rpcEpmString = "Remote Procedure Call // EndpointMapper";
            const string ldapString = "LDAP";
            const string ldapSSLString = "LDAP SSL";
            const string dnsString = "Domain Name Service";
            const string globalCatalogLDAPString = "Global Catalog LDAP";
            const string globalCatalogLDAPSSLString = "Global Catalog LDAP SSL";
            const string kerberosString = "Kerberos authentication";
            const string smbString = "SMB, NetLogon, SamR";

            protocolTesterListBox.Items.AddRange(new string[] { rpcEpmString, ldapString, ldapSSLString, dnsString, globalCatalogLDAPString, globalCatalogLDAPSSLString, kerberosString, smbString });
        }

        private async void manualTest_btn_Click(object sender, EventArgs e)
        {
            switch (port_txtbox.Text)
            {
                case "":
                case null:
                    {
                        const string errorMessage = "When using the manual test method you MUST provide a port to test with!";
                        MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
            }

            try
            {
                Int32 port = Int32.Parse(port_txtbox.Text);
                await testNetConnection(target_txtbox.Text, port, false);
            }
            catch
            {
                //Do nothing, the exception should've already been caught in testNetConnection
            }
        }

        private async Task testNetConnection(string target, Int32 port, bool isMulti)
        {
            if (g_tnc_firstRun_onMulti && isMulti)
            {
                results_txtbox.Text = "";
            }
            else if (!isMulti)
            {
                results_txtbox.Text = "";
            }

            try
            {
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                results_txtbox.AppendText($"Testing TCP connection to {target} on port {port}:{Environment.NewLine}");
                await client.ConnectAsync(target, port);

                if (ADReplStatusForm.gLoggingEnabled)
                {
                    System.IO.File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] Connection to {target} was successful on {port}{Environment.NewLine}");
                }

                string successMessage = $"Connection successful! {Environment.NewLine}===========================";
                results_txtbox.AppendText(successMessage);
                results_txtbox.AppendText($"{Environment.NewLine}Local IP Address: {client.LocalEndPoint}{Environment.NewLine}");
                results_txtbox.AppendText($"{Environment.NewLine}Remote IP Address: {client.RemoteEndPoint}{Environment.NewLine}{Environment.NewLine}");

                new Thread(() => client.Disconnect(true));
            }
            catch (Exception ex)
            {
                string errorMessage = $"ERROR: Connection to {target} using port {port} failed!{Environment.NewLine}{Environment.NewLine}{ex.Message}{Environment.NewLine}";
                results_txtbox.AppendText($"{Environment.NewLine}{errorMessage}{Environment.NewLine}");

                if (ADReplStatusForm.gLoggingEnabled)
                {
                    System.IO.File.AppendAllText(ADReplStatusForm.gLogfileName, $"[{DateTime.Now}] {errorMessage}\n");
                }
            }
        }

        private void selectAll_rbtn_CheckedChanged(object sender, EventArgs e)
        {
            g_checkAll_rbtn_isChecked = selectAll_rbtn.Checked;
        }

        private void selectAll_rbtn_Click(object sender, EventArgs e)
        {
            if (selectAll_rbtn.Checked && !g_checkAll_rbtn_isChecked)
            {
                selectAll_rbtn.Checked = false;
                for (int i = 0; i < protocolTesterListBox.Items.Count; i++)
                {
                    protocolTesterListBox.SetItemChecked(i, false);
                }
            }
            else
            {
                selectAll_rbtn.Checked = true;
                g_checkAll_rbtn_isChecked = false;
                for (int i = 0; i < protocolTesterListBox.Items.Count; i++)
                {
                    protocolTesterListBox.SetItemChecked(i, true);
                }
            }
        }

        private async void runTest_btn_Click(object sender, EventArgs e)
        {
            const Int32 rpcEpmPort = 135;
            const Int32 ldapPort = 389;
            const Int32 ldapSSLPort = 636;
            const Int32 globalCatalogLDAPPort = 3268;
            const Int32 globalCatalogLDAPSSLPort = 3269;
            const Int32 kerberosPort = 88;
            const Int32 dnsPort = 53;
            const Int32 smbPort = 445;

            try
            {
                g_tnc_firstRun_onMulti = true;
                foreach (var item in protocolTesterListBox.CheckedItems)
                {
                    string protocolname = item.ToString();
                    Int32 selectedPort;
                    switch (protocolname)
                    {
                        case "Remote Procedure Call // EndpointMapper":
                            selectedPort = rpcEpmPort;
                            break;

                        case "LDAP":
                            selectedPort = ldapPort;
                            break;

                        case "LDAP SSL":
                            selectedPort = ldapSSLPort;
                            break;

                        case "Domain Name Service":
                            selectedPort = dnsPort;
                            break;

                        case "Global Catalog LDAP":
                            selectedPort = globalCatalogLDAPPort;
                            break;

                        case "Global Catalog LDAP SSL":
                            selectedPort = globalCatalogLDAPSSLPort;
                            break;

                        case "Kerberos authentication":
                            selectedPort = kerberosPort;
                            break;

                        case "SMB, NetLogon, SamR":
                            selectedPort = smbPort;
                            break;

                        default:
                            return;
                    }
                    await testNetConnection(target_txtbox.Text, selectedPort, true);
                    g_tnc_firstRun_onMulti = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                g_tnc_firstRun_onMulti = false;
            }
        }

        private void exportResults_btn_Click(object sender, EventArgs e)
        {
            if (results_txtbox.Text.Length == 0)
            {
                new Thread(() => MessageBox.Show("Cannot export an empty results report!", "No results available", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)).Start();
            }
            else
            {
                try
                {
                    System.IO.File.WriteAllLines("PortTester_Results.txt", results_txtbox.Lines);
                    new Thread(() => MessageBox.Show($"Results exported to: {Application.StartupPath}\\PortTester_Results.txt", "Successfl export", MessageBoxButtons.OK, MessageBoxIcon.Information)).Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void PortTester_Load(object sender, EventArgs e)
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

                        case CheckBox _:
                            ((CheckBox)control).BackColor = Color.FromArgb(32, 32, 32);

                            ((CheckBox)control).ForeColor = Color.White;
                            break;

                        case RadioButton _:
                            ((RadioButton)control).BackColor = Color.FromArgb(32, 32, 32);

                            ((RadioButton)control).ForeColor = Color.White;
                            break;

                        case ListBox _:
                            ((ListBox)control).BackColor = Color.FromArgb(32, 32, 32);

                            ((ListBox)control).ForeColor = Color.White;
                            break;
                    }
                }
            }
        }
    }
}
