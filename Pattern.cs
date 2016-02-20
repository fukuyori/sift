using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace sift {
    public partial class Pattern : Form {
        string fileName;
        string fmt = "";
        int idx = 0;

        public Pattern(string s, int width, int n) {
            InitializeComponent();
            fileName = s;
            fmt = "D" + width.ToString();
            idx = n;
        }

        private void Pattern_Load(object sender, EventArgs e) {
            StringBuilder lines = new StringBuilder();
            List<string> target;

            label1.Text = fileName;

            Rename rn = new Rename();
            target = rn.ShowPattern(fileName);

            listView1.View = View.List;
            listView1.GridLines = true;

            listView1.Items.Add(string.Format("{0,6} = {1}", "<0>", fileName));
            for (int i = 0; i < target.Count; i++) {
                listView1.Items.Add(string.Format("{0,6} = {1}","<" + (i+1).ToString() + ">", target[i]));
            }
            listView1.Items.Add(string.Format("{0,6} = {1}", "<f>", Path.GetFileNameWithoutExtension(fileName)));
            listView1.Items.Add(string.Format("{0,6} = {1}", "<e>", Path.GetExtension(fileName)));
            listView1.Items.Add(string.Format("{0,6} = {1}", "<n>", (idx + 1).ToString(fmt)));
        }
    }
}
