using System;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;

namespace sift {
    public partial class sift : Form {

        /// <summary>
        /// renameDataGrid消去
        /// </summary>
        private void clsRename() {
            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            dtRename.Clear();

            this.Cursor = preCursor;
        }

        /// <summary>
        /// コンテキストメニュー　renameDataGridに表示されたファイル名をクリップボードへコピー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyAllToolStripMenuItem1_Click(object sender, EventArgs e) {
            renameFileNameCopyToClipboard();
        }

        private void renameFileNameCopyToClipboard() {
            // フォルダー構成を残す場合は、パス名も含めたファイル名をコピー
            int keepStructure = checkBox_keepfolder.Checked ? 0 : 1;

            System.Text.StringBuilder s = new System.Text.StringBuilder();
            for (int i = 0; i < renameDataGrid.Rows.Count; i++) {
                s.Append(renameDataGrid.Rows[i].Cells[keepStructure].Value.ToString() + Environment.NewLine);
            }
            Clipboard.SetText(s.ToString());
        }

        /// <summary>
        /// コンテキストメニュー　Renameへの貼り付け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e) {
            renameFileNamePasteFromClipboard();
        }

        private void renameFileNamePasteFromClipboard() {
            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            int total = 0;

            // ボタン類使用不可 
            allButtons(false);

            clsRename();
            dtRename.DefaultView.Sort = string.Empty;

            // クリップボードの中の行数を確認
            using (StringReader reader = new StringReader(Clipboard.GetText())) {
                string line;
                while ((line = reader.ReadLine()) != null)
                    if (line.Length > 0)
                        total++;
            }


            // プログレスバーの初期設定
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = targetDataGrid.Rows.Count;
            toolStripProgressBar1.Value = 0;
            DataRow data_row;
            int n = 0;
            int n1 = 0;

            // クリップボードのテキストをストリームとして読み込む
            using (StringReader reader = new StringReader(Clipboard.GetText())) {
                string line;

                // 最後の行まで、1行づつ読み込みを行う
                while ((line = reader.ReadLine()) != null && n < targetDataGrid.Rows.Count) {
                    if (line.Trim().Length > 0) {
                        data_row = dtRename.NewRow();
                        data_row["Id"] = targetDataGrid.Rows[n].Cells["Id"].Value;
                        data_row["size"] = targetDataGrid.Rows[n].Cells["size"].Value;
                        data_row["date"] = targetDataGrid.Rows[n].Cells["date"].Value;
                        try {

                            data_row["file"] = Path.GetFileName(line.Trim());
                            if (checkBox_keepfolder.Checked)
                                data_row["path"] = destPath.Text + line.Trim();
                            else
                                data_row["path"] = destPath.Text + Path.GetFileName(line.Trim());
                            dtRename.Rows.Add(data_row);
                            // カウンター表示
                            n++;
                            processConter(n);
                            toolStripProgressBar1.Value = n;
                            Application.DoEvents();
                        } catch { // ファイル名が適切でない場合は読み飛ばし
                        }
                    }
                }
                n1 = n;

                // クリップボードの件数がtargetDataGridの件数より多い
                while (line != null) {
                    if (line.Trim().Length > 0)
                        n++;
                    line = reader.ReadLine();
                }

                // クリップボードの件数がtargetDataGridの件数より少ない場合、targetDataGridをコピー
                while (n < targetDataGrid.Rows.Count) {
                    data_row = dtRename.NewRow();
                    data_row["Id"] = targetDataGrid.Rows[n].Cells["Id"].Value;
                    data_row["size"] = targetDataGrid.Rows[n].Cells["size"].Value;
                    data_row["date"] = targetDataGrid.Rows[n].Cells["date"].Value;
                    data_row["file"] = targetDataGrid.Rows[n].Cells["file"].Value;
                    data_row["path"] = targetDataGrid.Rows[n].Cells["file"].Value;
                    dtRename.Rows.Add(data_row);
                    // カウンター表示
                    n++;
                    processConter(n);
                    toolStripProgressBar1.Value = n;
                    Application.DoEvents();
                }
            }
            renameDataGrid.DataSource = dtRename;

            // ボタン、カーソルを元に戻す
            allButtons(true);
            buttonEnable();
            this.Cursor = preCursor;
            processConter();
            toolStripProgressBar1.Value = 0;

            checkedCounter(renameDataGrid.Rows.Count);
            targetDataGrid.FirstDisplayedScrollingRowIndex = 0;

            if (n > targetDataGrid.Rows.Count)
                MessageBox.Show(string.Format(Properties.Resources.messagebox6, n - targetDataGrid.Rows.Count));
            else if (n1 < targetDataGrid.Rows.Count)
                MessageBox.Show(string.Format(Properties.Resources.messagebox5, targetDataGrid.Rows.Count - n1));
            else
                MessageBox.Show(string.Format(Properties.Resources.messagebox7, n));
        }

        /// <summary>
        /// コンテキストメニュー　renameDataGrid再表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e) {
            targetRefresh();
        }

        /// <summary>
        /// コンテキストメニュー renameDataGridから選択されたファイルを消す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeToolStripMenuItem1_Click(object sender, EventArgs e) {
            String sId; // 選択されたデータのID
            int pos = renameDataGrid.FirstDisplayedScrollingRowIndex;

            using (var transaction = cn.BeginTransaction()) {
                using (SQLiteCommand cmd = cn.CreateCommand()) {
                    cmd.CommandText = "PRAGMA journal_mode = MEMORY";
                    cmd.ExecuteNonQuery();

                    foreach (DataGridViewCell cell in renameDataGrid.SelectedCells) {
                        sId = renameDataGrid.Rows[cell.RowIndex].Cells["Id"].Value.ToString();
                        // sourceDataGridのチェックも外す
                        dbCheck(int.Parse(sId), 0);
                    }
                }
                transaction.Commit();
            }
            adapter.Fill(dtSource);
            checkBox1.Checked = false;
            targetRefresh();
            if (pos > renameDataGrid.Rows.Count)
                pos = renameDataGrid.Rows.Count - 1;
            if (pos > 0)
                renameDataGrid.FirstDisplayedScrollingRowIndex = pos;
        }

        /// <summary>
        /// コンテキストメニュー　パターン表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showPatternToolStripMenuItem_Click(object sender, EventArgs e) {
            if (renameDataGrid.SelectedCells.Count > 0) {
                DataGridViewCell cell = renameDataGrid.SelectedCells[0];
                Pattern pattern = new Pattern(targetDataGrid.Rows[cell.RowIndex].Cells["file"].Value.ToString(), targetDataGrid.Rows.Count.ToString().Length, cell.RowIndex);
                pattern.ShowDialog();
            }
        }

        // //////////////////////////////////////////////////////////////
        /// <summary>
        /// パネル３からコピー実行（ファイル名変更あり）
        /// </summary>
        /// <returns></returns>
        // //////////////////////////////////////////////////////////////
        private int copyScr3() {
            String inf;
            string outf = "";
            int cnt = 0;

            allButtons(false);

            // マウスカーソル変更
            Cursor preCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            // プログレスバー設定
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = dbCount(true);
            toolStripProgressBar1.Value = 0;

            for (int i = 0; i < targetDataGrid.Rows.Count; i++) {
                // 入力ファイル名
                inf = sourcePath.Text + "\\" + targetDataGrid.Rows[i].Cells["path"].Value.ToString();


                if (destPath.Text.Length > 0) {
                    // 出力ファイル名
                    if (checkBox_keepfolder.Checked)
                        outf = destPath.Text + "\\" + renameDataGrid.Rows[i].Cells["path"].Value.ToString();
                    else
                        outf = destPath.Text + "\\" + renameDataGrid.Rows[i].Cells["file"].Value.ToString();
                    // コピー実行
                    if (doCopy(inf, outf))
                        cnt++;
                    else
                        break;
                } else {
                    // 出力ファイル名
                    outf = sourcePath.Text + "\\" + renameDataGrid.Rows[i].Cells["path"].Value.ToString();
                    // ファイル名変更
                    if (doMove(inf, outf))
                        cnt++;
                    else
                        break;
                }

                processConter(cnt);
                toolStripProgressBar1.Value = cnt;
                Application.DoEvents();
            }

            processConter();
            toolStripProgressBar1.Value = 0;

            if (destPath.Text.Length == 0) 
                sourceRefresh(sourcePath.Text);

            this.Cursor = preCursor;
            allButtons(true);
            buttonEnable();
            return cnt;
        }

        private Boolean doMove(string inf, string outf) {
            try {
                File.Move(inf, outf);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// renameDataGridでのクリップボードへコピー、貼り付け
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void renameDataGrid_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyData == (Keys.Control | Keys.V)) {
                renameFileNamePasteFromClipboard();
            }
        }

        /// <summary>
        ///  名前変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFormat_KeyDown(object sender, KeyEventArgs e) {
            // リターンキーを押されたとき
            if (e.KeyCode == Keys.Return) {
                // 文字が入力されていること
                if (tbFormat.Text.Length > 0) {
             
                    Rename rn = new Rename(tbFormat.Text);
                    rn.Setup(renameDataGrid.Rows.Count.ToString().Length);

                    // 名前変更処理
                    for (int i = 0; i < renameDataGrid.Rows.Count; i++) {
                        renameDataGrid.Rows[i].Cells["file"].Value =
                            rn.convert(targetDataGrid.Rows[i].Cells["file"].Value.ToString());
                        renameDataGrid.Rows[i].Cells["path"].Value =
                            Path.GetDirectoryName(targetDataGrid.Rows[i].Cells["path"].Value.ToString())
                            + "\\" + renameDataGrid.Rows[i].Cells["file"].Value;
                    }
                }
            }
        }

        /// <summary>
        /// パネル２へ移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnToScr2From3_Click(object sender, EventArgs e) {
            viewPanel(2);
            buttonEnable();
            checkedCounter(targetDataGrid.Rows.Count);
            // targetDataGridとrenameDataGridの位置そろえ
            int idx = renameDataGrid.FirstDisplayedScrollingRowIndex;
            targetDataGrid.FirstDisplayedScrollingRowIndex = idx;
        }
    }
}