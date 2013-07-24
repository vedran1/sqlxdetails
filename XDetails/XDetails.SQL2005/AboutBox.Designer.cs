namespace XDetails
{
	partial class AboutBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
			this.okButton = new System.Windows.Forms.Button();
			this.labelCopyright = new System.Windows.Forms.Label();
			this.labelVersion = new System.Windows.Forms.Label();
			this.labelProductName = new System.Windows.Forms.Label();
			this.logoPictureBox = new System.Windows.Forms.PictureBox();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.RegisterButton = new System.Windows.Forms.Button();
			this.RegisteredTo = new System.Windows.Forms.RichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(329, 236);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 24;
			this.okButton.Text = "&OK";
			// 
			// labelCopyright
			// 
			this.labelCopyright.AutoSize = true;
			this.labelCopyright.Location = new System.Drawing.Point(124, 49);
			this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelCopyright.Name = "labelCopyright";
			this.labelCopyright.Size = new System.Drawing.Size(198, 13);
			this.labelCopyright.TabIndex = 21;
			this.labelCopyright.Text = "Copyright (c) 2010-2011 Vedran Kesegić";
			this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelVersion
			// 
			this.labelVersion.AutoSize = true;
			this.labelVersion.Location = new System.Drawing.Point(124, 30);
			this.labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(42, 13);
			this.labelVersion.TabIndex = 0;
			this.labelVersion.Text = "Version";
			this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProductName
			// 
			this.labelProductName.AutoSize = true;
			this.labelProductName.Location = new System.Drawing.Point(124, 11);
			this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelProductName.Name = "labelProductName";
			this.labelProductName.Size = new System.Drawing.Size(143, 13);
			this.labelProductName.TabIndex = 19;
			this.labelProductName.Text = "SQL XDetails for SQL Server";
			this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// logoPictureBox
			// 
			this.logoPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("logoPictureBox.Image")));
			this.logoPictureBox.Location = new System.Drawing.Point(12, -1);
			this.logoPictureBox.Name = "logoPictureBox";
			this.logoPictureBox.Size = new System.Drawing.Size(103, 274);
			this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.logoPictureBox.TabIndex = 12;
			this.logoPictureBox.TabStop = false;
			// 
			// richTextBox1
			// 
			this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox1.Location = new System.Drawing.Point(127, 189);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.Size = new System.Drawing.Size(277, 41);
			this.richTextBox1.TabIndex = 26;
			this.richTextBox1.Text = "Visit www.sqlxdetails.com\nfor tutorials and downloading new versions.";
			this.richTextBox1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextBox1_LinkClicked);
			// 
			// RegisterButton
			// 
			this.RegisterButton.Location = new System.Drawing.Point(247, 237);
			this.RegisterButton.Name = "RegisterButton";
			this.RegisterButton.Size = new System.Drawing.Size(75, 23);
			this.RegisterButton.TabIndex = 27;
			this.RegisterButton.Text = "Register";
			this.RegisterButton.UseVisualStyleBackColor = true;
			this.RegisterButton.Click += new System.EventHandler(this.RegisterButton_Click);
			// 
			// RegisteredTo
			// 
			this.RegisteredTo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.RegisteredTo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.RegisteredTo.Location = new System.Drawing.Point(127, 85);
			this.RegisteredTo.Name = "RegisteredTo";
			this.RegisteredTo.ReadOnly = true;
			this.RegisteredTo.Size = new System.Drawing.Size(271, 98);
			this.RegisteredTo.TabIndex = 28;
			this.RegisteredTo.Text = "Registration info will go here";
			// 
			// AboutBox
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(416, 272);
			this.Controls.Add(this.RegisteredTo);
			this.Controls.Add(this.RegisterButton);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.logoPictureBox);
			this.Controls.Add(this.labelProductName);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.labelVersion);
			this.Controls.Add(this.labelCopyright);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutBox";
			this.Padding = new System.Windows.Forms.Padding(9);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "AboutBox";
			this.Load += new System.EventHandler(this.AboutBox_Load);
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label labelCopyright;
		private System.Windows.Forms.Label labelVersion;
		private System.Windows.Forms.Label labelProductName;
		private System.Windows.Forms.PictureBox logoPictureBox;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button RegisterButton;
		private System.Windows.Forms.RichTextBox RegisteredTo;

	}
}
