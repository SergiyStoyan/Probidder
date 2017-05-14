namespace Cliver.Foreclosures
{
    partial class SettingsForm
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
            this.bOk = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.TicketKey = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TicketModifierKey1 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.TicketModifierKey2 = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DbRefreshPeriodInSecs = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.DbRefreshRetryPeriodInSecs = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.NextDbRefreshTime = new System.Windows.Forms.DateTimePicker();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // bOk
            // 
            this.bOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOk.Location = new System.Drawing.Point(116, 290);
            this.bOk.Name = "bOk";
            this.bOk.Size = new System.Drawing.Size(75, 23);
            this.bOk.TabIndex = 0;
            this.bOk.Text = "OK";
            this.bOk.UseVisualStyleBackColor = true;
            this.bOk.Click += new System.EventHandler(this.bOk_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.Location = new System.Drawing.Point(197, 290);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 9;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
            // 
            // TicketKey
            // 
            this.TicketKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TicketKey.FormattingEnabled = true;
            this.TicketKey.Location = new System.Drawing.Point(10, 38);
            this.TicketKey.Name = "TicketKey";
            this.TicketKey.Size = new System.Drawing.Size(75, 21);
            this.TicketKey.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Key:";
            // 
            // TicketModifierKey1
            // 
            this.TicketModifierKey1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TicketModifierKey1.FormattingEnabled = true;
            this.TicketModifierKey1.Location = new System.Drawing.Point(91, 38);
            this.TicketModifierKey1.Name = "TicketModifierKey1";
            this.TicketModifierKey1.Size = new System.Drawing.Size(75, 21);
            this.TicketModifierKey1.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(92, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Modifier Key1:";
            // 
            // TicketModifierKey2
            // 
            this.TicketModifierKey2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TicketModifierKey2.FormattingEnabled = true;
            this.TicketModifierKey2.Location = new System.Drawing.Point(173, 38);
            this.TicketModifierKey2.Name = "TicketModifierKey2";
            this.TicketModifierKey2.Size = new System.Drawing.Size(75, 21);
            this.TicketModifierKey2.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(174, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Modifier Key2:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TicketModifierKey1);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.TicketModifierKey2);
            this.groupBox1.Controls.Add(this.TicketKey);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(258, 69);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "New Entry Key Combination";
            // 
            // DbRefreshPeriodInSecs
            // 
            this.DbRefreshPeriodInSecs.Location = new System.Drawing.Point(18, 35);
            this.DbRefreshPeriodInSecs.Name = "DbRefreshPeriodInSecs";
            this.DbRefreshPeriodInSecs.Size = new System.Drawing.Size(138, 20);
            this.DbRefreshPeriodInSecs.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(128, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Db Refresh Period (secs):";
            // 
            // DbRefreshRetryPeriodInSecs
            // 
            this.DbRefreshRetryPeriodInSecs.Location = new System.Drawing.Point(18, 76);
            this.DbRefreshRetryPeriodInSecs.Name = "DbRefreshRetryPeriodInSecs";
            this.DbRefreshRetryPeriodInSecs.Size = new System.Drawing.Size(138, 20);
            this.DbRefreshRetryPeriodInSecs.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Db Refresh Retry Period (secs):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Next Db Refresh Time:";
            // 
            // NextDbRefreshTime
            // 
            this.NextDbRefreshTime.Location = new System.Drawing.Point(18, 117);
            this.NextDbRefreshTime.Name = "NextDbRefreshTime";
            this.NextDbRefreshTime.Size = new System.Drawing.Size(200, 20);
            this.NextDbRefreshTime.TabIndex = 24;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.NextDbRefreshTime);
            this.groupBox2.Controls.Add(this.DbRefreshPeriodInSecs);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.DbRefreshRetryPeriodInSecs);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 90);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(260, 150);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "DataBase";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(284, 325);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bOk);
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bOk;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.ComboBox TicketKey;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox TicketModifierKey1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox TicketModifierKey2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox DbRefreshPeriodInSecs;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox DbRefreshRetryPeriodInSecs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker NextDbRefreshTime;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}