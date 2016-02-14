using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;

namespace sift {
    public partial class sift : Form {

        /// <summary>
        /// sourceGridData 消去
        /// </summary>
        private void clsSource() {
            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            // DataGridViewの消去
            dtSource.Rows.Clear();
            // DBの消去
            dbClear();

            this.Cursor = preCursor;
        }

        private void sourceDataGrid_CurrentCellChanged(object sender, EventArgs e) {
                adapter.Update(dtSource);
        }

        // /////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///  sourceDataGridにチェックのあるファイルをtargetDataGrid, renameDataGridへ表示
        /// </summary>
        // /////////////////////////////////////////////////////////////////////////////
        private void moveToTarget() {
            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            DataRow data_row;

            clsTarget();
            clsRename();

            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = "SELECT Id, checked, path, file,size,date FROM flist WHERE checked = 1;";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    for (int i = 0; reader.Read(); i++) {
                        data_row = dtTarget.NewRow();
                        data_row["path"] = reader["path"].ToString();
                        data_row["file"] = reader["file"].ToString();
                        data_row["Id"] = reader["Id"].ToString();
                        data_row["size"] = reader["size"].ToString();
                        data_row["date"] = reader["date"].ToString();
                        dtTarget.Rows.Add(data_row);

                        data_row = dtRename.NewRow();
                        data_row["path"] = reader["path"].ToString();
                        data_row["file"] = reader["file"].ToString();
                        data_row["Id"] = reader["Id"].ToString();
                        data_row["size"] = reader["size"].ToString();
                        data_row["date"] = reader["date"].ToString();
                        dtRename.Rows.Add(data_row);
                    }
                }
            }
            targetDataGrid.DataSource = dtTarget;
            renameDataGrid.DataSource = dtRename;
            clearFocus();
            this.Cursor = preCursor;
        }

        /// <summary>
        /// コンテキストメニュー　表示されているファイル名をクリップボードへコピー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e) {
            sourceFileNameCopyToClipboard();
        }

        private void sourceFileNameCopyToClipboard() {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            for (int i = 0; i < sourceDataGrid.Rows.Count; i++) {
                s.Append(sourceDataGrid.Rows[i].Cells["path"].Value.ToString() + Environment.NewLine);
            }
            Clipboard.SetText(s.ToString());
        }

        /// <summary>
        /// コンテキストメニュー　sourceGridViewの再表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e) {
            sourceRefresh(sourcePath.Text);
        }

        /// <summary>
        /// コンテキストメニュー 消去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearAllToolStripMenuItem2_Click(object sender, EventArgs e) {
            checkAll(false);
        }

        /// <summary>
        /// コンテキストメニュー　 ソート解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void unsortToolStripMenuItem_Click(object sender, EventArgs e) {
            dtSource.DefaultView.Sort = string.Empty;
        }

        /// <summary>
        /// コンテキストメニュー　選択範囲をチェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void markToolStripMenuItem_Click(object sender, EventArgs e) {
            String sId; // 選択されたデータのID

            using (var transaction = cn.BeginTransaction()) {
                using (SQLiteCommand cmd = cn.CreateCommand()) {
                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();

                    foreach (DataGridViewCell cell in sourceDataGrid.SelectedCells) {
                        sId = sourceDataGrid.Rows[cell.RowIndex].Cells["Id"].Value.ToString();
                        // sourceDataGridのチェックも外す
                        dbCheck(int.Parse(sId), 1);
                    }
                }
                transaction.Commit();
            }
            adapter.Fill(dtSource);
            checkBox1.Checked = false;
        }

        /// <summary>
        /// コンテキストメニュー　選択範囲をチェック外し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void unmarkToolStripMenuItem_Click(object sender, EventArgs e) {
            String sId; // 選択されたデータのID

            using (var transaction = cn.BeginTransaction()) {
                using (SQLiteCommand cmd = cn.CreateCommand()) {
                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();

                    foreach (DataGridViewCell cell in sourceDataGrid.SelectedCells) {
                        sId = sourceDataGrid.Rows[cell.RowIndex].Cells["Id"].Value.ToString();
                        // sourceDataGridのチェックも外す
                        dbCheck(int.Parse(sId), 0);
                    }
                }
                transaction.Commit();
            }
            adapter.Fill(dtSource);
            checkBox1.Checked = false;
        }

        // /////////////////////////////////////////////////////////
        /// <summary>
        /// パネル１からコピー実行
        ///  sourceDataGridでチェックされているファイルをコピー処理
        /// </summary>
        /// <returns></returns>
        // /////////////////////////////////////////////////////////
        private int copyScr1() {
            String inf, outf;
            int cnt = 0;

            allButtons(false);

            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            // プログレスバー設定
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = dbCount(true);
            toolStripProgressBar1.Value = 0;

            using (SQLiteCommand cmd = cn.CreateCommand()) {
                cmd.CommandText = "SELECT path, file FROM flist WHERE checked = 1";
                using (SQLiteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        // 入力ファイル名
                        inf = sourcePath.Text + "\\" + reader["path"].ToString();
                        // 出力ファイル名
                        if (checkBox_keepfolder.Checked)
                            outf = destPath.Text + "\\" + reader["path"].ToString();
                        else
                            outf = destPath.Text + "\\" + reader["file"].ToString();

                        // コピー実行
                        if (doCopy(inf, outf)) {
                            cnt++;
                            processConter(cnt);
                            toolStripProgressBar1.Value = cnt;
                            Application.DoEvents();
                        }
                    }
                }
            }
            processConter();
            toolStripProgressBar1.Value = 0;
            this.Cursor = preCursor;
            allButtons(true);
            buttonEnable();
            return cnt;
        }

        /// <summary>
        /// コピー処理
        /// </summary>
        /// <param name="inf"></param>
        /// <param name="outf"></param>
        /// <returns>コピーしたらtrue しなかったらfalse</returns>
        private bool doCopy(string inf, string outf) {

            // フォルダが存在しない場合は作成する
            if (!Directory.Exists(Path.GetDirectoryName(outf)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outf));

            // コピー先に同盟ファイルがあるかチェック
            if (File.Exists(outf)) {

                // コピー先に同名のファイルが存在するとき
                if (checkBox_overwrite.Checked) {

                    // 上書きする場合
                    if (checkBox_newfile.Checked) {
                        // sinがsoutより新しい場合は上書き
                        if (checkNewer(inf, outf)) {
                            File.Delete(outf);
                            File.Copy(inf, outf);
                            return true;
                        } else
                            return false;
                    } else {
                        // 無条件に上書き
                        File.Delete(outf);
                        File.Copy(inf, outf);
                        return true;
                    }
                } else {
                    // 上書きしない場合、名前が重複する場合は、名前を変える
                    String pathname = System.IO.Path.GetDirectoryName(outf);
                    String filename = System.IO.Path.GetFileNameWithoutExtension(outf);
                    String extname = System.IO.Path.GetExtension(outf);
                    int n = 1;
                    while (File.Exists(String.Format("{0}\\{1}({2}){3}", pathname, filename, n, extname))) {
                        n++;
                    }
                    outf = String.Format("{0}\\{1}({2}){3}", pathname, filename, n, extname);
                    File.Copy(inf, outf);
                    return true;
                }
            } else {

                // コピー先には同名のファイルがない場合
                File.Copy(inf, outf);
                return true;
            }
        }

        /// <summary>
        /// チェックボックスで全チェック、全チェック外し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            adapter.Update(dtSource);

            if (checkBox1.Checked)
                checkAll(true);
            else if (dbCount(false) == 0)
                checkAll(false);
        }

        /// <summary>
        /// ボタン表示処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel_VisibleChanged(object sender, EventArgs e) {
            buttonEnable();
        }

        /// <summary>
        /// パネル２へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToScr2Frm1_Click(object sender, EventArgs e) {
            adapter.Update(dtSource);
            moveToTarget();
            viewPanel(2);
            buttonEnable();
            checkedCounter(targetDataGrid.Rows.Count);
        }
    }
}
