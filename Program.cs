using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace sift {
    static class Program {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            //二重起動をチェックする
            if (System.Diagnostics.Process.GetProcessesByName(
                System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1) {
                //すでに起動していると判断する
                MessageBox.Show(Properties.Resources.messagebox4);
                return;
            }

            if (args.Length > 0) {
                System.Threading.Thread.CurrentThread.CurrentUICulture =
                new CultureInfo(args[0], false);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form f1 = new sift();
            Application.Run(f1);
        }
    }
}
