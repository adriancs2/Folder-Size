namespace FolderSize
{
    partial class Form1
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colnSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colnSizeNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btClearCache = new System.Windows.Forms.Button();
            this.btRefresh = new System.Windows.Forms.Button();
            this.btBack = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colnName,
            this.colnSize,
            this.colnSizeNo});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 87);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(429, 393);
            this.dataGridView1.TabIndex = 0;
            // 
            // colnName
            // 
            this.colnName.HeaderText = "Folder";
            this.colnName.Name = "colnName";
            this.colnName.ReadOnly = true;
            this.colnName.Width = 200;
            // 
            // colnSize
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.colnSize.DefaultCellStyle = dataGridViewCellStyle1;
            this.colnSize.HeaderText = "Size";
            this.colnSize.Name = "colnSize";
            this.colnSize.ReadOnly = true;
            // 
            // colnSizeNo
            // 
            this.colnSizeNo.HeaderText = "MB";
            this.colnSizeNo.Name = "colnSizeNo";
            this.colnSizeNo.ReadOnly = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Select Parent Folder";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Total: 0";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btClearCache);
            this.panel1.Controls.Add(this.btRefresh);
            this.panel1.Controls.Add(this.btBack);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(429, 87);
            this.panel1.TabIndex = 3;
            // 
            // btClearCache
            // 
            this.btClearCache.Location = new System.Drawing.Point(327, 9);
            this.btClearCache.Name = "btClearCache";
            this.btClearCache.Size = new System.Drawing.Size(90, 23);
            this.btClearCache.TabIndex = 6;
            this.btClearCache.Text = "Clear Cache";
            this.btClearCache.UseVisualStyleBackColor = true;
            this.btClearCache.Click += new System.EventHandler(this.btClearCache_Click);
            // 
            // btRefresh
            // 
            this.btRefresh.Location = new System.Drawing.Point(260, 9);
            this.btRefresh.Name = "btRefresh";
            this.btRefresh.Size = new System.Drawing.Size(65, 23);
            this.btRefresh.TabIndex = 5;
            this.btRefresh.Text = "Refresh";
            this.btRefresh.UseVisualStyleBackColor = true;
            this.btRefresh.Click += new System.EventHandler(this.btRefresh_Click);
            // 
            // btBack
            // 
            this.btBack.Location = new System.Drawing.Point(150, 9);
            this.btBack.Name = "btBack";
            this.btBack.Size = new System.Drawing.Size(110, 23);
            this.btBack.TabIndex = 3;
            this.btBack.Text = "Up One Level";
            this.btBack.UseVisualStyleBackColor = true;
            this.btBack.Click += new System.EventHandler(this.btBack_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "(Double click each row folder to enter)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 480);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple Folder Size Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colnSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn colnSizeNo;
        private System.Windows.Forms.Button btBack;
        private System.Windows.Forms.Button btRefresh;
        private System.Windows.Forms.Button btClearCache;
        private System.Windows.Forms.Label label2;
    }
}
