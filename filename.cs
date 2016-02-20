using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;

namespace sift {
    public partial class sift : Form {

        /// <summary>
        /// DB作成処理
        /// </summary>
        private void dbMake() {
            using (SQLiteCommand cmd = cn.CreateCommand()) {
                try {
                    cmd.CommandText = "CREATE TABLE flist (Id INTEGER PRIMARY KEY AUTOINCREMENT,checked INTEGER,path TEXT,file TEXT, size INTEGER, date DATETIME);";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE INDEX checked_idx on flist(checked);";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE INDEX path_idx on flist(path);";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE INDEX file_idx on flist(file);";
                    cmd.ExecuteNonQuery();
                } catch {
                    MessageBox.Show("DB ERROR");
                }
            }
        }

        /// <summary>
        ///  DB削除
        /// </summary>
        /// <param name="fileName"></param>
        private void dbDelete(string fileName) {
            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = string.Format("DROP TABLE IF EXISTS {0};", fileName);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///  DBデータ消去
        /// </summary>
        private void dbClear() {
            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = "DELETE FROM flist;";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// DBチェック付与・消去
        /// </summary>
        /// <param name="id">db column</param>
        /// <param name="f">0:unchecked 1:checked</param>
        private void dbCheck(int id, int f) {
            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = string.Format("UPDATE flist SET checked = {0} WHERE Id = {1};", f, id);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// pathを検索してチェック付与
        /// </summary>
        /// <param name="s">search name</param>
        private void fileQuery(string s) {
            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = @"UPDATE flist SET checked = 1 WHERE path like @param;";
                cmd.Parameters.Add(new SQLiteParameter("@param", "%" + s + "%"));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///  DBチェックの件数
        /// </summary>
        /// <param name="f">cout true or false</param>
        /// <returns>number</returns>
        private int dbCount(Boolean f) {
            int n = 0;

            using (SQLiteCommand cmd = cn.CreateCommand()) {
                if (f)
                    cmd.CommandText = "SELECT COUNT(Id) FROM flist WHERE checked = 1;";
                else
                    cmd.CommandText = "SELECT COUNT(Id) FROM flist WHERE checked = 0;";

                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read())
                        n = int.Parse(reader[0].ToString());
                }
            }
            return n;
        }

        private int dbCount() {
            int n = 0;

            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = "SELECT COUNT(Id) FROM flist;";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read())
                        n = int.Parse(reader[0].ToString());
                }
            }
            return n;
        }

        // sourceDataGridのチェックボックスをすべて変更
        private void checkAll(Boolean f) {
            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            using (var transaction = cn.BeginTransaction()) {
                using (SQLiteCommand cmd = cn.CreateCommand()) {
                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();

                    if (f)
                        cmd.CommandText = "UPDATE flist SET checked = 1;";
                    else
                        cmd.CommandText = "UPDATE flist SET checked = 0;";

                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            adapter.Fill(dtSource);

            checkBox1.Checked = f;

            this.Cursor = preCursor;
        }

        // ////////////////////////////////////////////////////////
        /// <summary>
        /// ファイルを検索して再表示
        /// </summary>
        /// <param name="path"></param>
        // ////////////////////////////////////////////////////////
        private enum SearchMode {None, Stop, Continue};
        private SearchMode sm = SearchMode.None;

        private void sourceRefresh(String path) {
            // DataGridの内容消去
            clsSource();
            checkBox1.Checked = false;
            clsTarget();
            clsRename();

            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            // 項目無効
            allButtons(false);

            totalCount = 0;

            // ファイル名の取得
            using (var transaction = cn.BeginTransaction()) {
                using (SQLiteCommand cmd = cn.CreateCommand()) {
                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();
                    sm = SearchMode.None;
                    recursiveFileList(path, ref totalCount);
                }
                transaction.Commit();
            }

            adapter = new SQLiteDataAdapter("SELECT Id, checked, path, file, size, date FROM flist;", cn);

            SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);

            if (adapter != null)
                adapter.Fill(dtSource);

            // カーソルを元に戻す
            this.Cursor = preCursor;
            // 項目有効
            allButtons(true);
            // ボタン表示チェック
            buttonEnable();
            // 件数表示
            checkedCounter(totalCount);
            // パネル1を表示
            viewPanel(1);
        }

        /// <summary>
        /// ファイルシステム検索
        /// </summary>
        IEnumerable<string> files;
        IEnumerable<string> dirs;

        /// <summary>
        /// 再帰的にファイル名を取得
        /// </summary>
        /// <param name="path"></param>
        /// <param name="totalCount"></param>
        private void recursiveFileList(String path, ref int totalCount) {
            System.IO.FileInfo fi;
            files = getFileName(path);
            long size;

            if (sm == SearchMode.Stop)
                return;

            foreach (String file in files) {
                // ファイルの属性を調べる
                try {
                    fi = new System.IO.FileInfo(file);
                    if ((fi.Attributes & System.IO.FileAttributes.Hidden) != System.IO.FileAttributes.Hidden) {
                        // ファイル名全部表示
                        using (SQLiteCommand cmd = cn.CreateCommand()) {
                            size = ((fi.Length * 10 / 1024 % 10) > 0) ? 1 : 0;
                            size += fi.Length / 1024;

                            cmd.CommandText = "INSERT INTO flist (checked,path,file,size,date) values(0, @path, @file, @size, @date);";
                            cmd.Parameters.Add(new SQLiteParameter("@path", file.Substring(sourcePath.Text.Length)));
                            cmd.Parameters.Add(new SQLiteParameter("@file", Path.GetFileName(file)));
                            cmd.Parameters.Add(new SQLiteParameter("@size", size));
                            cmd.Parameters.Add(new SQLiteParameter("@date", fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")));

                            //MessageBox.Show(cmd.CommandText);
                            cmd.ExecuteNonQuery();
                            totalCount++;
                        }

                    }
                } catch {
                    // 情報が取得できないファイルは、ほっとく
                }
            }

            // 件数表示
            checkedCounter(totalCount);
            if (totalCount > MAXFILECOUNT && sm == SearchMode.None) {
                DialogResult result = MessageBox.Show(string.Format(Properties.Resources.messagebox2, MAXFILECOUNT,System.Environment.NewLine),
                    "Question",
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question, 
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    sm = SearchMode.Stop;
                else if (result == DialogResult.Yes)
                    sm = SearchMode.Continue;
            }
               
            Application.DoEvents();

            if (!radioButton1.Checked) {
                dirs = getDirectoryName(path);
                foreach (String dir in dirs) {
                    recursiveFileList(dir + "\\", ref totalCount);
                }
            }
        }

        /// <summary>
        /// 再帰的にファイル名を取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<string> getFileName(String path) {
            files = Enumerable.Empty<string>();
            try {
                files = System.IO.Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
            } catch {
                // アクセス権限がない
                return files;
            }
            return files;
        }

        /// <summary>
        /// ディレクトリ名を取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<string> getDirectoryName(String path) {
            dirs = Enumerable.Empty<string>();

            try {
                dirs = System.IO.Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
            } catch {
                // アクセス権限がない
                return dirs;
            }
            return dirs;
        }

        /// <summary>
        /// ＤＢを検索して追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectAdd_Click(object sender, EventArgs e) {
            selectAdd(tbQuery.Text);
        }

        /// <summary>
        /// エンターキーで選択ファイル追加処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbQuery_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Return) {
                selectAdd(tbQuery.Text);
            }
        }

        /// <summary>
        /// 選択ファイル追加処理
        /// </summary>
        /// <param name="s"></param>
        private void selectAdd(String s) {
            allButtons(false);

            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            // ファイル名の取得
            fileAddQuery(tbQuery.Text);
            checkBox1.Checked = false;
            // カーソルを元に戻す
            this.Cursor = preCursor;

            allButtons(true);
            buttonEnable();
        }

        /// <summary>
        /// DB更新
        /// 文字をfileとpathから検索してチェック付与
        /// </summary>
        /// <param name="s"></param>
        private void fileAddQuery(string s) {
            if (tbQuery.Text.Length > 0) {
                using (var transaction = cn.BeginTransaction()) {
                    using (SQLiteCommand cmd = cn.CreateCommand()) {
                        cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "UPDATE flist SET checked = 1 WHERE path like @param;";
                        cmd.Parameters.Add(new SQLiteParameter("@param", s));
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                if (adapter != null)
                    adapter.Fill(dtSource);
            }
        }

        /// <summary>
        ///  選択ファイル除外ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectSub_Click(object sender, EventArgs e) {
            selectSub(tbQuery.Text);
        }

        /// <summary>
        /// 選択ファイル除外処理
        /// </summary>
        /// <param name="s"></param>
        private void selectSub(String s) {
            allButtons(false);

            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            // ファイル名の取得
            fileSubQuery(tbQuery.Text);

            checkBox1.Checked = false;
            // カーソルを元に戻す
            this.Cursor = preCursor;

            // ボタン表示チェック
            allButtons(true);
            buttonEnable();
        }

        /// <summary>
        /// DB更新 除外
        /// </summary>
        /// <param name="s"></param>
        private void fileSubQuery(string s) {
            if (tbQuery.Text.Length > 0) {
                using (var transaction = cn.BeginTransaction()) {
                    using (SQLiteCommand cmd = cn.CreateCommand()) {
                        cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "UPDATE flist SET checked = 0 WHERE path like @param;";
                        cmd.Parameters.Add(new SQLiteParameter("@param", s));
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                if (adapter != null)
                    adapter.Fill(dtSource);
            }
        }
    }
}