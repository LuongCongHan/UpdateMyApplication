namespace UpdateMyApp
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.lbCurrentVersion = new System.Windows.Forms.Label();
            this.btnCheckUpdate = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnCheckInternet = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnBatTimer = new System.Windows.Forms.Button();
            this.btnTatTimer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbCurrentVersion
            // 
            this.lbCurrentVersion.AutoSize = true;
            this.lbCurrentVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbCurrentVersion.Location = new System.Drawing.Point(55, 83);
            this.lbCurrentVersion.Name = "lbCurrentVersion";
            this.lbCurrentVersion.Size = new System.Drawing.Size(66, 24);
            this.lbCurrentVersion.TabIndex = 0;
            this.lbCurrentVersion.Text = "label1";
            // 
            // btnCheckUpdate
            // 
            this.btnCheckUpdate.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCheckUpdate.Location = new System.Drawing.Point(210, 59);
            this.btnCheckUpdate.Name = "btnCheckUpdate";
            this.btnCheckUpdate.Size = new System.Drawing.Size(158, 72);
            this.btnCheckUpdate.TabIndex = 1;
            this.btnCheckUpdate.Text = "Check Update Version";
            this.btnCheckUpdate.UseVisualStyleBackColor = true;
            this.btnCheckUpdate.Click += new System.EventHandler(this.btnCheckUpdate_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(26, 161);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Click me!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCheckInternet
            // 
            this.btnCheckInternet.Location = new System.Drawing.Point(225, 160);
            this.btnCheckInternet.Name = "btnCheckInternet";
            this.btnCheckInternet.Size = new System.Drawing.Size(126, 23);
            this.btnCheckInternet.TabIndex = 3;
            this.btnCheckInternet.Text = "Kiểm tra Internet";
            this.btnCheckInternet.UseVisualStyleBackColor = true;
            this.btnCheckInternet.Click += new System.EventHandler(this.btnCheckInternet_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnBatTimer
            // 
            this.btnBatTimer.Location = new System.Drawing.Point(115, 13);
            this.btnBatTimer.Name = "btnBatTimer";
            this.btnBatTimer.Size = new System.Drawing.Size(75, 23);
            this.btnBatTimer.TabIndex = 4;
            this.btnBatTimer.Text = "Bật timer";
            this.btnBatTimer.UseVisualStyleBackColor = true;
            this.btnBatTimer.Click += new System.EventHandler(this.btnBatTimer_Click);
            // 
            // btnTatTimer
            // 
            this.btnTatTimer.Location = new System.Drawing.Point(247, 12);
            this.btnTatTimer.Name = "btnTatTimer";
            this.btnTatTimer.Size = new System.Drawing.Size(75, 23);
            this.btnTatTimer.TabIndex = 4;
            this.btnTatTimer.Text = "Tắt Timer";
            this.btnTatTimer.UseVisualStyleBackColor = true;
            this.btnTatTimer.Click += new System.EventHandler(this.btnTatTimer_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 211);
            this.Controls.Add(this.btnTatTimer);
            this.Controls.Add(this.btnBatTimer);
            this.Controls.Add(this.btnCheckInternet);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnCheckUpdate);
            this.Controls.Add(this.lbCurrentVersion);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "Update My App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbCurrentVersion;
        private System.Windows.Forms.Button btnCheckUpdate;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnCheckInternet;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnBatTimer;
        private System.Windows.Forms.Button btnTatTimer;
    }
}

