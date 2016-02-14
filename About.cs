using System;
using System.Windows.Forms;

namespace sift {
    public partial class About : Form {
        public About() {
            InitializeComponent();
        }

        private void about_Load(object sender, EventArgs e) {
            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            version.Text = ver.ProductVersion;
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
