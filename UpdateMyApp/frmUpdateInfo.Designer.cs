namespace UpdateMyApp
{
    partial class frmUpdateInfo
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
            this.components = new System.ComponentModel.Container();
            this.lbSize = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.timerCheckUpdate = new System.Windows.Forms.Timer(this.components);
            this.picLoad = new System.Windows.Forms.PictureBox();
            this.pictureIcon = new System.Windows.Forms.PictureBox();
            this.lbLinkUpdate = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.picLoad)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // lbSize
            // 
            this.lbSize.AutoSize = true;
            this.lbSize.Location = new System.Drawing.Point(146, 87);
            this.lbSize.Name = "lbSize";
            this.lbSize.Size = new System.Drawing.Size(35, 13);
            this.lbSize.TabIndex = 1;
            this.lbSize.Text = "label1";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(164, 117);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(259, 117);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // timerCheckUpdate
            // 
            this.timerCheckUpdate.Interval = 1;
            this.timerCheckUpdate.Tick += new System.EventHandler(this.timerCheckUpdate_Tick);
            // 
            // picLoad
            // 
            this.picLoad.Image = global::UpdateMyApp.Properties.Resources.loading_7528_256;
            this.picLoad.Location = new System.Drawing.Point(176, 21);
            this.picLoad.Name = "picLoad";
            this.picLoad.Size = new System.Drawing.Size(64, 64);
            this.picLoad.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picLoad.TabIndex = 4;
            this.picLoad.TabStop = false;
            // 
            // pictureIcon
            // 
            this.pictureIcon.Image = global::UpdateMyApp.Properties.Resources.smile;
            this.pictureIcon.Location = new System.Drawing.Point(10, 10);
            this.pictureIcon.Name = "pictureIcon";
            this.pictureIcon.Size = new System.Drawing.Size(48, 48);
            this.pictureIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureIcon.TabIndex = 0;
            this.pictureIcon.TabStop = false;
            // 
            // lbLinkUpdate
            // 
            this.lbLinkUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbLinkUpdate.Location = new System.Drawing.Point(83, 30);
            this.lbLinkUpdate.Name = "lbLinkUpdate";
            this.lbLinkUpdate.Size = new System.Drawing.Size(55, 13);
            this.lbLinkUpdate.TabIndex = 0;
            this.lbLinkUpdate.TabStop = true;
            this.lbLinkUpdate.Text = "linkLabel1";
            // 
            // frmUpdateInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 146);
            this.Controls.Add(this.lbLinkUpdate);
            this.Controls.Add(this.picLoad);
            this.Controls.Add(this.lbSize);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.pictureIcon);
            this.Name = "frmUpdateInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Software Update";
            this.Load += new System.EventHandler(this.frmUpdateInfo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picLoad)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureIcon;
        private System.Windows.Forms.Label lbSize;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.PictureBox picLoad;
        private System.Windows.Forms.Timer timerCheckUpdate;
        public System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.LinkLabel lbLinkUpdate;
    }
}