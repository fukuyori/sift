using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace sift {
    public partial class Configure : Form {
        public int maxCompStr, maxFileCount;
        public Boolean HistoryClear;

        public Configure() {
            InitializeComponent();
        }

        private void properties_Load(object sender, EventArgs e) {
            HistoryClear = false;
            udHistory.Value = maxCompStr;
            udFile.Value = maxFileCount;
        }

        private void properties_FormClosed(object sender, FormClosedEventArgs e) {
        }

        private void btnOk_Click(object sender, EventArgs e) {
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift");
            regkey.SetValue("HistoryCount", udHistory.Value.ToString());
            regkey.SetValue("FileCount", udFile.Value.ToString());
            maxCompStr = int.Parse(udHistory.Value.ToString());
            maxFileCount = int.Parse(udFile.Value.ToString());
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void btnHistory_Click(object sender, EventArgs e) {
            HistoryClear = true;
        }
    }
}
