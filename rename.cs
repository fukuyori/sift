using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;

namespace sift {
    class Rename {
        private string commands, fmt;
        private int serial;
        private List<string> listTarget;
        private List<Int32> listCommand;
        private enum TypeChar { NONE, CHARACTER, NUMERIC, SYMBOL };
        private static string symbols = @" !#$%&'()+-,.;=@[]^_`{}~";
        private static string numerics = "0123456789";

        /// <summary>
        /// 初期化 
        /// </summary>
        /// <param name="cmd">変換ルール</param>
        public Rename(string cmd) {
            commands = cmd;
            listCommand = new List<Int32>();
            listTarget = new List<string>();

            analyzeCommand(commands);
        }

        public Rename() {
            commands = "";
            listCommand = new List<Int32>();
            listTarget = new List<string>();
        }

        /// <summary>
        /// 連番の初期化
        /// </summary>
        /// <param name="n"></param>
        public void Setup(int n) {
            serial = 0;
            fmt = "D" + n.ToString();
        }

        /// <summary>
        /// 連番文字作成
        /// </summary>
        /// <returns></returns>
        private string serialString() {
            serial++;
            return serial.ToString(fmt);
        }

        /// <summary>
        /// 入力文字分解結果
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<string> ShowPattern(string s) {
            analyzeIns(s);
            return listTarget;
        }

        /// <summary>
        /// 文字列変換
        /// </summary>
        /// <param name="s"></param>
        /// <returns>変換後文字列</returns>
        public string convert(string s) {
            analyzeIns(s);
            string outs = commands;
            foreach (int n in listCommand) {
                if (n > 0 && n <= listTarget.Count) {
                    Regex r = new Regex(string.Format("<{0}>", n));
                    outs = r.Replace(outs, listTarget[n - 1]);
                }
            }
            // <0> は、元のファイル名全体
            if (outs.Contains("<0>")) {
                Regex r = new Regex("<0>");
                outs = r.Replace(outs, s);
            }
            // <f> は、元のファイル名の拡張子のないもの
            if (outs.Contains("<f>")) {
                Regex r = new Regex("<f>");
                outs = r.Replace(outs, Path.GetFileNameWithoutExtension(s));
            }
            // <e> は、元のファイル名の拡張子
            if (outs.Contains("<e>")) {
                Regex r = new Regex("<e>");
                outs = r.Replace(outs, Path.GetExtension(s));
            }
            // <n>は、連番
            if (outs.Contains("<n>")) {
                Regex r = new Regex("<n>");
                outs = r.Replace(outs, serialString());
            }
            return outs;
        }

        /// <summary>
        /// 変換ルール解析
        /// </summary>
        /// <param name="cmd"></param>
        private void analyzeCommand(string cmd) {
            StringBuilder s = new StringBuilder();
            listCommand.Clear();
            int n;

            // <>で数字を囲む部分を抽出
            Regex reg = new Regex("<(?<num>\\d+?)>");

            // 抽出したものを command配列に追加
            for (Match m = reg.Match(cmd); m.Success; m = m.NextMatch()) {
                n = Convert.ToInt32(m.Groups["num"].Value);
                if (!listCommand.Contains(n))
                    listCommand.Add(n);
            }
        }

        /// <summary>
        /// 入力文字分解
        /// </summary>
        /// <param name="ins"></param>
        private void analyzeIns(string ins) {
            StringBuilder sb = new StringBuilder();
            TypeChar preType = TypeChar.NONE;
            listTarget.Clear();
            TextElementEnumerator charEnum = StringInfo.GetTextElementEnumerator(ins);

            while (true) {
                // 次文字がなければ終了
                if (charEnum.MoveNext() == false) {
                    // 最後のワード
                    if (preType != TypeChar.SYMBOL)
                        listTarget.Add(sb.ToString());
                    break;
                }
                // 最初の文字の種別をセット
                if (preType == TypeChar.NONE)
                    preType = checkIns(charEnum.Current.ToString());

                if (checkIns(charEnum.Current.ToString()) == preType)
                    sb.Append(charEnum.Current.ToString());
                else {
                    if (preType != TypeChar.SYMBOL)
                        listTarget.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(charEnum.Current.ToString());
                    preType = checkIns(charEnum.Current.ToString());
                }
            }
        }

        /// <summary>
        /// 文字タイプ判定
        /// </summary>
        /// <param name="s">入力文字</param>
        /// <returns>TypeChar</returns>
        private TypeChar checkIns(string s) {
            if (numerics.Contains(s))
                return TypeChar.NUMERIC;
            else if (symbols.Contains(s))
                return TypeChar.SYMBOL;
            else
                return TypeChar.CHARACTER;
        }
    }
}