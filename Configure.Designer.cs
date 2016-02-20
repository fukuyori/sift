namespace sift {
    partial class Configure {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configure));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnHistory = new System.Windows.Forms.Button();
            this.udHistory = new System.Windows.Forms.NumericUpDown();
            this.udFile = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.udHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udFile)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnHistory
            // 
            resources.ApplyResources(this.btnHistory, "btnHistory");
            this.btnHistory.Name = "btnHistory";
            this.btnHistory.UseVisualStyleBackColor = true;
            this.btnHistory.Click += new System.EventHandler(this.btnHistory_Click);
            // 
            // udHistory
            // 
            resources.ApplyResources(this.udHistory, "udHistory");
            this.udHistory.Name = "udHistory";
            this.udHistory.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // udFile
            // 
            resources.ApplyResources(this.udFile, "udFile");
            this.udFile.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.udFile.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.udFile.Name = "udFile";
            this.udFile.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.BackColor = System.Drawing.SystemColors.Window;
            this.label4.Name = "label4";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // Configure
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ControlBox = false;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.udFile);
            this.Controls.Add(this.udHistory);
            this.Controls.Add(this.btnHistory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Configure";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.properties_FormClosed);
            this.Load += new System.EventHandler(this.properties_Load);
            ((System.ComponentModel.ISupportInitialize)(this.udHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnHistory;
        private System.Windows.Forms.NumericUpDown udHistory;
        private System.Windows.Forms.NumericUpDown udFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}