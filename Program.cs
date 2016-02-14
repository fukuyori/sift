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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new sift());
            //if (args.Length > 0) {
            //    System.Threading.Thread.CurrentThread.CurrentUICulture =
            //    new CultureInfo(args[0], false);
            //}

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Form f1 = new sift();
            //Application.Run(f1);
        }
    }
}
