namespace CnMedicineTools
{
    partial class frmGetCnDrug
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btOpen = new System.Windows.Forms.Button();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btGetResult = new System.Windows.Forms.Button();
            this.tbCnDrug = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btOpen
            // 
            this.btOpen.Location = new System.Drawing.Point(518, 13);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(94, 23);
            this.btOpen.TabIndex = 0;
            this.btOpen.Text = "打开文件(&O)";
            this.btOpen.UseVisualStyleBackColor = true;
            this.btOpen.Click += new System.EventHandler(this.btOpen_Click);
            // 
            // tbFileName
            // 
            this.tbFileName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CnMedicineTools.Properties.Settings.Default, "药物文件全路径", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbFileName.Location = new System.Drawing.Point(13, 13);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(499, 21);
            this.tbFileName.TabIndex = 1;
            this.tbFileName.Text = global::CnMedicineTools.Properties.Settings.Default.药物文件全路径;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btGetResult
            // 
            this.btGetResult.Location = new System.Drawing.Point(517, 98);
            this.btGetResult.Name = "btGetResult";
            this.btGetResult.Size = new System.Drawing.Size(94, 23);
            this.btGetResult.TabIndex = 0;
            this.btGetResult.Text = "获取结果(&R)";
            this.btGetResult.UseVisualStyleBackColor = true;
            this.btGetResult.Click += new System.EventHandler(this.btGetResult_Click);
            // 
            // tbCnDrug
            // 
            this.tbCnDrug.Location = new System.Drawing.Point(12, 98);
            this.tbCnDrug.Multiline = true;
            this.tbCnDrug.Name = "tbCnDrug";
            this.tbCnDrug.Size = new System.Drawing.Size(499, 340);
            this.tbCnDrug.TabIndex = 1;
            // 
            // frmGetCnDrug
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tbCnDrug);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.btGetResult);
            this.Controls.Add(this.btOpen);
            this.Name = "frmGetCnDrug";
            this.Text = "frmGetCnDrug";
            this.Load += new System.EventHandler(this.frmGetCnDrug_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOpen;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btGetResult;
        private System.Windows.Forms.TextBox tbCnDrug;
    }
}