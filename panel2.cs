using System;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;

namespace sift {
    public partial class sift : Form {

        /// <summary>
        /// targetDataGrid消去
        /// </summary>
        private void clsTarget() {
            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            dtTarget.Clear();

            this.Cursor = preCursor;
        }

        /// <summary>
        /// コンテキストメニュー　ファイル名消去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearToolStripMenuItem_Click(object sender, EventArgs e) {
            // チェックをすべて消す
            checkAll(false);
            // DataGridを消去
            clsTarget();
            clsRename();
            // ボタンのアクティブ
            buttonEnable();
        }

        /// <summary>
        /// コンテキストメニュー　targetDataGridに表示されたファイル名をクリップボードへコピー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            targetFileNameCopyToClipboard();
        }

        private void targetFileNameCopyToClipboard() {
            // フォルダー構成を残す場合は、パス名も含めたファイル名をコピー
            string keepStructure = checkBox_keepfolder.Checked ? "path" : "file";

            System.Text.StringBuilder s = new System.Text.StringBuilder();
            for (int i = 0; i < targetDataGrid.Rows.Count; i++) {
                s.Append(targetDataGrid.Rows[i].Cells[keepStructure].Value.ToString() + Environment.NewLine);
            }
            if (s.Length > 0)
                Clipboard.SetText(s.ToString());
        }

        /// <summary>
        /// コンテキストメニュー targetDataGridから選択されたファイルを消す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeToolStripMenuItem_Click(object sender, EventArgs e) {
            String sId; // 選択されたデータのID
            int pos = targetDataGrid.FirstDisplayedScrollingRowIndex;

            using (var transaction = cn.BeginTransaction()) {
                using (SQLiteCommand cmd = cn.CreateCommand()) {
                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();

                    foreach (DataGridViewCell cell in targetDataGrid.SelectedCells) {
                        sId = targetDataGrid.Rows[cell.RowIndex].Cells["Id"].Value.ToString();
                        // sourceDataGridのチェックも外す
                        dbCheck(int.Parse(sId), 0);
                    }
                }
                transaction.Commit();
            }
            adapter.Fill(dtSource);
            checkBox1.Checked = false;
            targetRefresh();
            if (pos > targetDataGrid.Rows.Count)
                pos = targetDataGrid.Rows.Count - 1;
            if (pos > 0)
                targetDataGrid.FirstDisplayedScrollingRowIndex = pos;
        }

        /// <summary>
        /// ソートが実行されたとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void targetDataGrid_Sorted(object sender, EventArgs e) {
            System.ComponentModel.ListSortDirection order;
            DataGridViewColumn col = targetDataGrid.SortedColumn;
            if (targetDataGrid.SortOrder == SortOrder.Ascending)
                order = System.ComponentModel.ListSortDirection.Ascending;
            else
                order = System.ComponentModel.ListSortDirection.Descending;
            renameDataGrid.Sort(renameDataGrid.Columns[col.Name], order);
        }

        /// <summary>
        /// targetDataGrid再表示
        /// </summary>
        private void targetRefresh() {
            moveToTarget();
        }

        /// <summary>
        /// コンテキストメニュー　ソート解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void unsortToolStripMenuItem1_Click(object sender, EventArgs e) {
            dtTarget.DefaultView.Sort = string.Empty;
            dtRename.DefaultView.Sort = string.Empty;
        }

        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// コンテキストメニュー　Targetへクリップボードにあるファイル名のリストを貼り付け
        //  現在targetDataSelectに表示されているファイル名はクリアされる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// ///////////////////////////////////////////////////////////////////////
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) {
            targetFileNamePasteFromClipboard();
        }

        private void targetFileNamePasteFromClipboard() {
            // 選択されているものをすべて消去
            checkAll(false);

            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            int total = 0;
            int scale;

            // ボタン類の使用不可
            allButtons(false);

            // クリップボードの中の行数を確認
            using (StringReader reader = new StringReader(Clipboard.GetText())) {
                string line;
                while ((line = reader.ReadLine()) != null)
                    if (line.Length > 0)
                        total++;
            }
            // カウンター表示更新のタイミング
            scale = total / 500 + 1;

            // プログレスバーの初期設定
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = total;
            toolStripProgressBar1.Value = 0;

            // クリップボードのテキストをストリームとして読み込み、sourceDataGridのファイル名と突き合わせ
            using (StringReader reader = new StringReader(Clipboard.GetText())) {
                string line;
                int n = 0;

                using (var transaction = cn.BeginTransaction()) {
                    using (SQLiteCommand cmd = cn.CreateCommand()) {
                        cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                        cmd.ExecuteNonQuery();

                        // 最後の行まで、1行づつ読み込みを行う
                        while ((line = reader.ReadLine()) != null) {
                            if (line.Trim().Length > 0) {
                                fileQuery(line.Trim());
                                n++;
                                // カウンター表示更新
                                if ((n % scale) == 0) {
                                    processConter(n);
                                    toolStripProgressBar1.Value = n;
                                    Application.DoEvents();
                                }
                            }
                        }
                    }
                    transaction.Commit();
                }
                adapter.Fill(dtSource);
            }
            dtTarget.DefaultView.Sort = string.Empty;
            moveToTarget();

            // ボタン、カーソルを元に戻す
            allButtons(true);
            buttonEnable();
            this.Cursor = preCursor;
            processConter();
            toolStripProgressBar1.Value = 0;

            checkedCounter(targetDataGrid.Rows.Count);
            targetDataGrid.FirstDisplayedScrollingRowIndex = 0;

            MessageBox.Show(string.Format(Properties.Resources.messagebox3, targetDataGrid.Rows.Count, total));
        }

        // ////////////////////////////////////////////////////////////////
        /// <summary>
        /// パネル２からコピー実行（ファイル名変更なし）
        /// </summary>
        /// <returns></returns>
        // ////////////////////////////////////////////////////////////////
        private int copyScr2() {
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
        /// targetDataGridでのクリップボードへコピー、貼り付け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void targetDataGrid_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData == (Keys.Control | Keys.V)) {
                targetFileNamePasteFromClipboard();
            }
        }

        /// <summary>
        /// パネル１へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToScr1_Click(object sender, EventArgs e) {
            viewPanel(1);
            buttonEnable();
            checkedCounter(totalCount);
        }

        /// <summary>
        /// パネル３へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToScr3_Click(object sender, EventArgs e) {
            viewPanel(3);
            buttonEnable();
            checkedCounter(renameDataGrid.Rows.Count);
            // targetDataGridとrenameDataGridの位置そろえ
            int idx = targetDataGrid.FirstDisplayedScrollingRowIndex;
            renameDataGrid.FirstDisplayedScrollingRowIndex = idx;
            tbFormat.Focus();
        }
    }
}
