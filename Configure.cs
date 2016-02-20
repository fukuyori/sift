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
        public string fastcopy;
        public Boolean fastcopyStructureIgnore;
        public Boolean HistoryClear;

        public Configure() {
            InitializeComponent();
        }

        private void properties_Load(object sender, EventArgs e) {
            HistoryClear = false;
            udHistory.Value = maxCompStr;
            udFile.Value = maxFileCount;
            if (fastcopy == "")
                label4.Text = Properties.Resources.fc_notuse;
            else if (fastcopyStructureIgnore)
                label4.Text = Properties.Resources.fc_ignore;
            else
                label4.Text = Properties.Resources.fc_use;
        }

        private void properties_FormClosed(object sender, FormClosedEventArgs e) {
        }

        /// <summary>
        /// OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, EventArgs e) {
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift");
            regkey.SetValue("HistoryCount", udHistory.Value.ToString());
            regkey.SetValue("FileCount", udFile.Value.ToString());
            maxCompStr = int.Parse(udHistory.Value.ToString());
            maxFileCount = int.Parse(udFile.Value.ToString());
            this.Close();
        }

        /// <summary>
        /// Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Use FastCopy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e) {

        }

        private void label4_Click(object sender, EventArgs e) {
            string prog = @"\FastCopy\FastCopy.exe";

            if (label4.Text == Properties.Resources.fc_notuse) {
                // FastCopyを使用する
                setProg(prog);
                fastcopyStructureIgnore = false;
                label4.Text = Properties.Resources.fc_use;
            } else if (label4.Text == Properties.Resources.fc_use) {
                // FastCopyをフォルダ構造無視の時だけ使用
                setProg(prog);
                fastcopyStructureIgnore = true;
                label4.Text = Properties.Resources.fc_ignore;
            } else {
                // FastCopyを使用しない
                fastcopy = "";
                fastcopyStructureIgnore = false;
                label4.Text = Properties.Resources.fc_notuse;
            }
        }

        private void setProg(string prog) {
            string progfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            // 64bit : Program Files (x86)
            // 32bit : Program Files
            if (System.IO.File.Exists(progfiles + prog)) {
                fastcopy = progfiles + prog;
                return;
            }

            // 64bit : Program Files
            if (progfiles.EndsWith("(x86)")) {
                progfiles = progfiles.Replace(" (x86)", "");
                if (System.IO.File.Exists(progfiles + prog)) {
                    fastcopy = progfiles + prog;
                    return;
                }
            }
            // FastCopy.exe が見つかりません
            MessageBox.Show(Properties.Resources.notfound);
        }

        /// <summary>
        /// Clear history
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHistory_Click(object sender, EventArgs e) {
            HistoryClear = true;
        }
    }
}
