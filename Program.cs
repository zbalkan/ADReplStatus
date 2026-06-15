using System;
using System.Windows.Forms;

namespace ADReplStatus
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (!System.IO.File.Exists("ObjectListView.dll"))
            {
                MessageBox.Show("Could not find ObjectListView.dll. Make sure the file resides in the same directory as this executable.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ADReplStatusForm());
        }
    }
}