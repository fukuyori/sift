using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace sift {
    public partial class sift : Form {

        /// <summary>
        /// ＤＢ設定
        /// </summary>
        SQLiteDataAdapter adapter;
        SQLiteConnection cn;
        string dataPath;
        string dbc;

        /// <summary>
        /// DataSet, DataTable設定
        /// </summary>
        DataSet dsSource = new DataSet("source");
        DataSet dsTarget = new DataSet("target");
        DataSet dsRename = new DataSet("rename");
        DataTable dtSource = new DataTable("source");
        DataTable dtTarget = new DataTable("target");
        DataTable dtRename = new DataTable("rename");


        /// <summary>
        /// オートコンプリート設定
        /// </summary>
        AutoCompleteStringCollection sourceCompList, destCompList;
        private int MAXCOMPSTR = 20; // 履歴保存数
        private int MAXFILECOUNT = 10000; // 最大取扱ファイル数
        private string currentSourcePath = "";
        private int totalCount = 0; // データベースのレコード数

        // FastCopy
        private string FastCopyPath = "";
        private Boolean FastCopyStructureIgnore = false;

        /// <summary>
        /// 初期化
        /// </summary>
        public sift() {
            InitializeComponent();
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e) {
            // ボタン設定
            btnToScr2Frm1.Enabled = false;
            btnToScr3.Enabled = false;

            // パネル設定
            panel1.Dock = DockStyle.Fill;
            panel2.Dock = DockStyle.Fill;
            panel3.Dock = DockStyle.Fill;
            panel4.Dock = DockStyle.Top;
            viewPanel(1);
            this.Height = 800;
            this.Width = 600;

            checkedCounter(totalCount);

            // DB作成
            dataPath = System.IO.Path.GetTempPath() + @"\sift.db";
            // ＤＢファイル使用中の場合は、多重起動のメッセージを出して終了
            if (System.IO.File.Exists(dataPath)) {
                if (IsFileLocked(dataPath)) {
                    MessageBox.Show(Properties.Resources.messagebox4);
                    this.Close();
                    return;
                }
                System.IO.File.Delete(dataPath);
            }
            dbc = "Data Source=" + dataPath;
            cn = new SQLiteConnection(dbc);
            cn.Open();
            dbMake();

            // DataGirdView設定
            dsSource.Tables.Add(dtSource);
            dtSource.Columns.Add("Id", Type.GetType("System.Int32"));
            dtSource.PrimaryKey = new DataColumn[] { dtSource.Columns["Id"] };
            dtSource.Columns.Add("checked", Type.GetType("System.Boolean"));
            dtSource.Columns.Add("path", Type.GetType("System.String"));
            dtSource.Columns.Add("file", Type.GetType("System.String"));
            dtSource.Columns.Add("size", Type.GetType("System.Int32"));
            dtSource.Columns.Add("date", Type.GetType("System.DateTime"));
            sourceDataGrid.DataSource = dtSource;
            sourceDataGrid.Columns["Id"].Visible = false;
            sourceDataGrid.Columns["checked"].HeaderText = "";
            sourceDataGrid.Columns["checked"].Width = 20;
            sourceDataGrid.Columns["path"].HeaderText = "File Name";
            sourceDataGrid.Columns["path"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            sourceDataGrid.Columns["path"].ReadOnly = true;
            sourceDataGrid.Columns["path"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);
            sourceDataGrid.Columns["file"].Visible = false;
            sourceDataGrid.Columns["size"].HeaderText = "Size(KB)";
            sourceDataGrid.Columns["size"].Width = 60;
            sourceDataGrid.Columns["size"].ReadOnly = true;
            sourceDataGrid.Columns["size"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            sourceDataGrid.Columns["size"].DefaultCellStyle.Format = "#,0";
            sourceDataGrid.Columns["size"].DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
            sourceDataGrid.Columns["date"].HeaderText = "Date";
            sourceDataGrid.Columns["date"].Width = 120;
            sourceDataGrid.Columns["date"].ReadOnly = true;
            sourceDataGrid.Columns["date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            sourceDataGrid.Columns["date"].DefaultCellStyle.Format = "yyyy/MM/dd HH:mm";
            sourceDataGrid.Columns["date"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);

            dsTarget.Tables.Add(dtTarget);
            dtTarget.Columns.Add("path", Type.GetType("System.String"));
            dtTarget.Columns.Add("file", Type.GetType("System.String"));
            dtTarget.Columns.Add("Id", Type.GetType("System.Int32"));
            dtTarget.PrimaryKey = new DataColumn[] { dtTarget.Columns["Id"] };
            dtTarget.Columns.Add("size", Type.GetType("System.Int32"));
            dtTarget.Columns.Add("date", Type.GetType("System.DateTime"));

            targetDataGrid.DataSource = dtTarget;
            targetDataGrid.Columns["Id"].Visible = false;
            targetDataGrid.Columns["path"].Visible = false;
            targetDataGrid.Columns["path"].HeaderText = "File Name";
            targetDataGrid.Columns["path"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            targetDataGrid.Columns["path"].ReadOnly = true;
            targetDataGrid.Columns["path"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);
            targetDataGrid.Columns["file"].Visible = true;
            targetDataGrid.Columns["file"].HeaderText = "File Name";
            targetDataGrid.Columns["file"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            targetDataGrid.Columns["file"].ReadOnly = true;
            targetDataGrid.Columns["file"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);
            targetDataGrid.Columns["size"].HeaderText = "Size(KB)";
            targetDataGrid.Columns["size"].Width = 60;
            targetDataGrid.Columns["size"].ReadOnly = true;
            targetDataGrid.Columns["size"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            targetDataGrid.Columns["size"].DefaultCellStyle.Format = "#,0";
            targetDataGrid.Columns["size"].DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
            targetDataGrid.Columns["date"].HeaderText = "Date";
            targetDataGrid.Columns["date"].Width = 120;
            targetDataGrid.Columns["date"].ReadOnly = true;
            targetDataGrid.Columns["date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            targetDataGrid.Columns["date"].DefaultCellStyle.Format = "yyyy/MM/dd HH:mm";
            targetDataGrid.Columns["date"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);

            dsRename.Tables.Add(dtRename);
            dtRename.Columns.Add("path", Type.GetType("System.String"));
            dtRename.Columns.Add("file", Type.GetType("System.String"));
            dtRename.Columns.Add("Id", Type.GetType("System.Int32"));
            dtRename.PrimaryKey = new DataColumn[] { dtRename.Columns["Id"] };
            dtRename.Columns.Add("size", Type.GetType("System.Int32"));
            dtRename.Columns.Add("date", Type.GetType("System.DateTime"));

            renameDataGrid.DataSource = dtRename;
            renameDataGrid.Columns["Id"].Visible = false;
            renameDataGrid.Columns["path"].Visible = false;
            renameDataGrid.Columns["path"].HeaderText = "File Name";
            renameDataGrid.Columns["path"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            renameDataGrid.Columns["path"].ReadOnly = true;
            renameDataGrid.Columns["path"].SortMode = DataGridViewColumnSortMode.NotSortable;
            renameDataGrid.Columns["path"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);
            renameDataGrid.Columns["path"].ReadOnly = false;
            renameDataGrid.Columns["file"].Visible = true;
            renameDataGrid.Columns["file"].HeaderText = "File Name";
            renameDataGrid.Columns["file"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            renameDataGrid.Columns["file"].ReadOnly = true;
            renameDataGrid.Columns["file"].SortMode = DataGridViewColumnSortMode.NotSortable;
            renameDataGrid.Columns["file"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);
            renameDataGrid.Columns["file"].ReadOnly = false;
            renameDataGrid.Columns["size"].HeaderText = "Size(KB)";
            renameDataGrid.Columns["size"].Width = 60;
            renameDataGrid.Columns["size"].SortMode = DataGridViewColumnSortMode.NotSortable;
            renameDataGrid.Columns["size"].ReadOnly = true;
            renameDataGrid.Columns["size"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            renameDataGrid.Columns["size"].DefaultCellStyle.Format = "#,0";
            renameDataGrid.Columns["size"].DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);
            renameDataGrid.Columns["date"].HeaderText = "Date";
            renameDataGrid.Columns["date"].Width = 120;
            renameDataGrid.Columns["date"].SortMode = DataGridViewColumnSortMode.NotSortable;
            renameDataGrid.Columns["date"].ReadOnly = true;
            renameDataGrid.Columns["date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            renameDataGrid.Columns["date"].DefaultCellStyle.Format = "yyyy/MM/dd HH:mm";
            renameDataGrid.Columns["date"].DefaultCellStyle.Padding = new Padding(2, 0, 0, 0);

            // source,target,rename DataGridViewの初期設定
            changeDataGrid(checkBox_keepfolder.Checked);

            // オートコンプリート設定
            sourceCompList = new AutoCompleteStringCollection();
            sourcePath.AutoCompleteMode = AutoCompleteMode.Suggest;
            sourcePath.AutoCompleteCustomSource = sourceCompList;
            destCompList = new AutoCompleteStringCollection();
            destPath.AutoCompleteMode = AutoCompleteMode.Suggest;
            destPath.AutoCompleteCustomSource = destCompList;
            // フォルダパスをレジストリから取得
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift\History");
            for (int i = 0; i < MAXCOMPSTR; i++) {
                if (regkey.GetValue("source" + i, null) != null) {
                    sourceCompList.Add((string)regkey.GetValue("source" + i, null));
                    sourcePath.Items.Add((string)regkey.GetValue("source" + i, null));
                }
                if (regkey.GetValue("dest" + i, null) != null) {
                    destCompList.Add((string)regkey.GetValue("dest" + i, null));
                    destPath.Items.Add((string)regkey.GetValue("dest" + i, null));
                }
            }

            regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift");
            // 履歴保存数をレジストリから取得
            MAXCOMPSTR = (int)regkey.GetValue("HistoryCount", MAXCOMPSTR);
            // 最大取扱ファイル数をレジストリから取得
            MAXFILECOUNT = (int)regkey.GetValue("FileCount", MAXFILECOUNT);
            // 検索範囲 0:Top Folder 1:サブフォルダーも含む
            if ((int)regkey.GetValue("SearchOption", 0) == 1) {
                radioButton1.Checked = false;
                radioButton2.Checked = true;
            }
            // フォルダー構成のままコピー
            if ((int)regkey.GetValue("KeepFolder", 0) == 1)
                checkBox_keepfolder.Checked = true;
            // 上書き
            if ((int)regkey.GetValue("Overwrite", 0) == 1) {
                checkBox_overwrite.Checked = true;
            } else {
                checkBox_newfile.Enabled = false;
            }
            // 最新で上書き
            if ((int)regkey.GetValue("NewFile", 0) == 1)
                checkBox_newfile.Checked = true;
            // FastCopy
            if ((string)regkey.GetValue("FastCopy", "") != "")
                FastCopyPath = (string)regkey.GetValue("FastCopy", "");
            // FastCopy ignore mode
            if ((int)regkey.GetValue("FastCopy_Structure_Ignore", 0) == 1)
                FastCopyStructureIgnore = true;

            this.sourcePath.Focus();
        }

        /// <summary>
        /// ファイルの使用中をチェック
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileLocked(string path) {
            FileStream stream = null;

            try {
                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            } catch {
                return true;
            } finally {
                if (stream != null) {
                    stream.Close();
                }
            }

            return false;
        }

        /// <summary>
        /// targetDataGridとrenameDataGridの表示をパス付、パスなしの切り替え
        /// </summary>
        /// <param name="b"></param>
        private void changeDataGrid(Boolean b) {
            if (b) {
                // パス付
                targetDataGrid.Columns["path"].Visible = true;
                targetDataGrid.Columns["file"].Visible = false;
                renameDataGrid.Columns["path"].Visible = true;
                renameDataGrid.Columns["file"].Visible = false;
            } else {
                // パスなし
                targetDataGrid.Columns["path"].Visible = false;
                targetDataGrid.Columns["file"].Visible = true;
                renameDataGrid.Columns["path"].Visible = false;
                renameDataGrid.Columns["file"].Visible = true;
            }
            clearFocus();
        }

        /// <summary>
        /// 終了時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {

            //// フォルダパスをレジストリに登録
            Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift\History");

            for (int i = 0; i < MAXCOMPSTR; i++) {
                if (sourceCompList != null && sourceCompList.Count > i)
                    regkey.SetValue("source" + i, sourceCompList[i].ToString());
                if (destCompList != null && destCompList.Count > i)
                    regkey.SetValue("dest" + i, destCompList[i].ToString());
            }
            // 設定をレジストリに保存
            regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift");
            regkey.SetValue("HistoryCount", MAXCOMPSTR);
            regkey.SetValue("FileCount", MAXFILECOUNT);
            regkey.SetValue("SearchOption", radioButton1.Checked ? 0 : 1);
            regkey.SetValue("KeepFolder", checkBox_keepfolder.Checked ? 1 : 0);
            regkey.SetValue("Overwrite", checkBox_overwrite.Checked ? 1 : 0);
            regkey.SetValue("NewFile", checkBox_newfile.Checked ? 1 : 0);
            regkey.SetValue("FastCopy", FastCopyPath);
            regkey.SetValue("FastCopy_Structure_Ignore", FastCopyStructureIgnore ? 1 : 0);

            dbDelete("flist");
            cn.Close();
            cn.Dispose();
        }

        /// <summary>
        /// レジストリの履歴全消去
        /// </summary>
        private void regClear() {
            Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Sift");
            regkey.DeleteSubKey("History");
        }

        /// <summary>
        /// 入力フォルダーの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sourceFolderSelect(object sender, EventArgs e) {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "入力フォルダを指定してください。";
            //ルートフォルダを指定する
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //最初に選択するフォルダを指定する
            if (sourcePath.Text.Length > 0)
                fbd.SelectedPath = sourcePath.Text;
            else
                fbd.SelectedPath = fbd.RootFolder.ToString();
            //ユーザーが新しいフォルダを作成できない
            fbd.ShowNewFolderButton = false;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK) {
                // パスの最後に\をつける
                if (!fbd.SelectedPath.Substring(fbd.SelectedPath.Length - 1, 1).Equals("\\"))
                    fbd.SelectedPath = fbd.SelectedPath + "\\";
                if (sourcePath.Text != fbd.SelectedPath) {
                    //選択されたフォルダを表示する
                    sourcePath.Text = fbd.SelectedPath;
                    currentSourcePath = fbd.SelectedPath;
                    addSourceFolder(fbd.SelectedPath);
                    sourceRefresh(fbd.SelectedPath);
                }
            }
            fbd.Dispose();
            this.destPath.Focus();
        }

        /// <summary>
        /// 入力フォルダー　オートコンプリート項目追加
        /// </summary>
        /// <param name="s"></param>
        private void addSourceFolder(String s) {
            // オートコンプリート追加
            string newItem = s.Trim();
            if (!String.IsNullOrEmpty(newItem)) {
                if (sourceCompList.Contains(newItem)) {
                    // 既に登録済の場合は、いったん削除
                    sourceCompList.RemoveAt(sourceCompList.IndexOf(newItem));
                    sourcePath.Items.RemoveAt(sourcePath.Items.IndexOf(newItem));
                }
                sourceCompList.Insert(0, newItem);
                sourcePath.Items.Insert(0, newItem);
                sourcePath.Text = newItem;
                // 履歴が規定値以上の場合は、残りを削除
                renumSourceFolder();
            }
        }

        /// <summary>
        ///  規定値以上の履歴は削除
        /// </summary>
        private void renumSourceFolder() {
            if (sourceCompList.Count > MAXCOMPSTR) {
                for (int i = MAXCOMPSTR; i < sourceCompList.Count;) {
                    sourceCompList.RemoveAt(i);
                    sourcePath.Items.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 出力フォルダーの選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void destFolderSelect(object sender, EventArgs e) {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "出力フォルダを指定してください。";
            //ルートフォルダを指定する
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //最初に選択するフォルダを指定する
            if (destPath.Text.Length > 0)
                fbd.SelectedPath = destPath.Text;
            else
                fbd.SelectedPath = fbd.RootFolder.ToString();
            //ユーザーが新しいフォルダを作成できるようにする
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK) {
                // パスの最後に\をつける
                if (!fbd.SelectedPath.Substring(fbd.SelectedPath.Length - 1, 1).Equals("\\"))
                    fbd.SelectedPath = fbd.SelectedPath + "\\";
                //選択されたフォルダを表示する
                destPath.Text = fbd.SelectedPath;
                addDestFolder(fbd.SelectedPath);
            }
            fbd.Dispose();
            sourcePath.Focus();
        }

        /// <summary>
        /// 出力フォルダー　オートコンプリート項目追加
        /// </summary>
        /// <param name="s"></param>
        private void addDestFolder(String s) {
            // オートコンプリート追加
            string newItem = s.Trim();
            if (!String.IsNullOrEmpty(newItem)) {
                if (destCompList.Contains(newItem)) {
                    // 既に登録済の場合は、いったん削除
                    destCompList.RemoveAt(destCompList.IndexOf(newItem));
                    destPath.Items.RemoveAt(destPath.Items.IndexOf(newItem));
                }
                destCompList.Insert(0, newItem);
                destPath.Items.Insert(0, newItem);
                destPath.Text = newItem;
                // 履歴が規定値以上の場合は、残りを削除
                renumDestFolder();
            }
        }

        /// <summary>
        /// 規定値以上の履歴は削除
        /// </summary>
        private void renumDestFolder() {
            if (destCompList.Count > MAXCOMPSTR) {
                for (int i = MAXCOMPSTR; i < destCompList.Count;) {
                    destCompList.RemoveAt(i);
                    destPath.Items.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 入力フォルダー入力項目から移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sourcePath_Leave(object sender, EventArgs e) {
            // 存在しないフォルダなら履歴へ登録。存在しないなら、クリア
            if (System.IO.Directory.Exists(sourcePath.Text)) {
                // Pathの最後に\をつける
                if (!sourcePath.Text.Substring(sourcePath.Text.Length - 1, 1).Equals("\\"))
                    sourcePath.Text = sourcePath.Text + "\\";
                if (sourcePath.Text != currentSourcePath) {
                    currentSourcePath = sourcePath.Text;
                    addSourceFolder(sourcePath.Text);
                    sourceRefresh(sourcePath.Text);
                }
            } else {
                sourcePath.Text = "";
                currentSourcePath = "";
            }
            buttonEnable();
        }

        /// <summary>
        /// 入力フォルダー項目でEnterキーをおした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sourcePath_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Return) {
                this.destPath.Focus();
            }
        }

        /// <summary>
        /// 入力フォルダーでドロップダウンから選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sourcePath_SelectedIndexChanged(object sender, EventArgs e) {
            this.destPath.Focus();
        }

        /// <summary>
        /// 出力フォルダー入力項目から移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void destPath_Leave(object sender, EventArgs e) {
            // 存在しないフォルダなら履歴へ登録。存在しないなら、クリア
            if (System.IO.Directory.Exists(destPath.Text)) {
                // パスの最後に\をつける
                if (!destPath.Text.Substring(destPath.Text.Length - 1, 1).Equals("\\"))
                    destPath.Text = destPath.Text + "\\";
                addDestFolder(destPath.Text);
            } else
                destPath.Text = "";
            buttonEnable();
        }

        /// <summary>
        /// 出力フォルダー項目でEnterキーをおした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void destPath_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Return) {
                this.sourcePath.Focus();
            }
        }

        /// <summary>
        /// 出力フォルダーをドロップダウンで選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void destPath_SelectedIndexChanged(object sender, EventArgs e) {
            this.sourcePath.Focus();
        }

        /// <summary>
        /// 子ディレクトリまで対象にするか
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            sourceRefresh(sourcePath.Text);
        }

        /// <summary>
        /// 上書きの可否
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_overwrite_CheckedChanged(object sender, EventArgs e) {
            if (checkBox_overwrite.Checked)
                checkBox_newfile.Enabled = true;
            else
                checkBox_newfile.Enabled = false;
        }

        /// <summary>
        /// フォルダー構成を残すか、残さないか
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_keepfolder_CheckedChanged(object sender, EventArgs e) {
            changeDataGrid(checkBox_keepfolder.Checked);
        }

        /// <summary>
        /// 更新日付の比較
        /// </summary>
        /// <param name="m"></param>
        /// <param name="t"></param>
        /// <returns>
        /// mとtのファイルでmが新しい場合は、true。tが新しい場合は false
        /// ファイルが見つからない場合は false
        /// </returns>
        private Boolean checkNewer(String m, String t) {
            if (!System.IO.File.Exists(m) && !System.IO.File.Exists(t))
                return false;
            if (System.IO.File.GetLastWriteTime(m) > System.IO.File.GetLastWriteTime(t))
                return true;
            else
                return false;
        }

        /// <summary>
        /// フォーカスを消す
        /// </summary>
        private void clearFocus() {
            sourceDataGrid.ClearSelection();
            targetDataGrid.ClearSelection();
            renameDataGrid.ClearSelection();
        }

        /// <summary>
        /// ボタン選択表示
        /// </summary>
        private void buttonEnable() {
            // パネル１
            if (sourceDataGrid.Rows.Count > 0)
                btnToScr2Frm1.Enabled = true;
            else
                btnToScr2Frm1.Enabled = false;

            // パネル２
            if (targetDataGrid.Rows.Count > 0)
                btnToScr3.Enabled = true;
            else
                btnToScr3.Enabled = false;
        }

        /// <summary>
        /// コピー処理振り分け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e) {
            int cnt = 0;
            adapter.Update(dtSource);

            if (panel1.Visible.Equals(true))
                if (sourcePath.Text.Length > 0 && destPath.Text.Length > 0 && !sourcePath.Text.Equals(destPath.Text))
                    cnt = copyScr1();
            if (panel2.Visible.Equals(true))
                if (sourcePath.Text.Length > 0 && destPath.Text.Length > 0 && !sourcePath.Text.Equals(destPath.Text))
                    cnt = copyScr2();
            if (panel3.Visible.Equals(true))
                if (sourcePath.Text.Length > 0 && !sourcePath.Text.Equals(destPath.Text))
                    cnt = copyScr3();

            if (cnt > 0)
                MessageBox.Show(string.Format(Properties.Resources.messagebox1, cnt), "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        /// <summary>
        /// 件数カウント
        /// </summary>
        /// <param name="n"></param>
        private void checkedCounter(int n) {
            toolStripStatusLabel1.Text = string.Format("Total: {0,8:#,0}", n);
        }

        /// <summary>
        /// 処理中件数カウント
        /// </summary>
        /// <param name="n"></param>
        private void processConter(int n) {
            toolStripStatusLabel2.Text = string.Format("Processing: {0,8:#,0}", n);
        }
        private void processConter() {
            toolStripStatusLabel2.Text = "";
        }

        /// <summary>
        /// 作業中にボタンを操作できないように
        /// </summary>
        /// <param name="flg"></param>
        private void allButtons(Boolean flg) {
            sourcePath.Enabled = flg;
            destPath.Enabled = flg;
            button1.Enabled = flg;
            button2.Enabled = flg;
            button3.Enabled = flg;
            button4.Enabled = flg;
            btnStart.Enabled = flg;
            btnToScr1.Enabled = flg;
            btnToScr2Frm1.Enabled = flg;
            btnToScr2From3.Enabled = flg;
            btnToScr3.Enabled = flg;
            tbQuery.Enabled = flg;
            radioButton1.Enabled = flg;
            radioButton2.Enabled = flg;
            checkBox1.Enabled = flg;
            checkBox_keepfolder.Enabled = flg;
            checkBox_newfile.Enabled = flg;
            checkBox_overwrite.Enabled = flg;
            sourceDataGrid.Enabled = flg;
            targetDataGrid.Enabled = flg;
            renameDataGrid.Enabled = flg;
            tbFormat.Enabled = flg;
        }

        /// <summary>
        /// プロパティー設定ダイアログ表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e) {
            Configure prop = new Configure();
            prop.maxCompStr = MAXCOMPSTR;
            prop.maxFileCount = MAXFILECOUNT;
            prop.fastcopy = FastCopyPath;
            prop.fastcopyStructureIgnore = FastCopyStructureIgnore;

            prop.ShowDialog();
            MAXCOMPSTR = prop.maxCompStr;
            renumSourceFolder();
            renumDestFolder();
            MAXFILECOUNT = prop.maxFileCount;
            FastCopyPath = prop.fastcopy;
            FastCopyStructureIgnore = prop.fastcopyStructureIgnore;

            if (prop.HistoryClear) {
                regClear();
                sourceCompList.Clear();
                destCompList.Clear();
                sourcePath.Items.Clear();
                destPath.Items.Clear();
                sourcePath.Text = "";
                destPath.Text = "";
            }

            prop.Dispose();
        }

        /// <summary>
        /// メニュー　終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Close();
        }

        /// <summary>
        /// バージョン情報表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            About about = new About();
            about.ShowDialog();
        }

        /// <summary>
        /// パネル選択
        /// </summary>
        /// <param name="n"></param>
        private void viewPanel(int n) {
            switch (n) {
                case 1:
                    panel1.Visible = true;
                    panel2.Visible = false;
                    panel3.Visible = false;
                    break;
                case 2:
                    panel1.Visible = false;
                    panel2.Visible = true;
                    panel3.Visible = false;
                    break;
                case 3:
                    panel1.Visible = false;
                    panel2.Visible = false;
                    panel3.Visible = true;
                    break;
            }
        }
    }
}