namespace XDetails
{

	partial class ObjectInfoControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.ObjectNameLabel = new System.Windows.Forms.Label();
			this.DatabaseLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Location = new System.Drawing.Point(3, 44);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(456, 218);
			this.tabControl1.TabIndex = 1;
			// 
			// ObjectNameLabel
			// 
			this.ObjectNameLabel.AutoEllipsis = true;
			this.ObjectNameLabel.AutoSize = true;
			this.ObjectNameLabel.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
			this.ObjectNameLabel.Location = new System.Drawing.Point(3, 3);
			this.ObjectNameLabel.Name = "ObjectNameLabel";
			this.ObjectNameLabel.Size = new System.Drawing.Size(213, 17);
			this.ObjectNameLabel.TabIndex = 2;
			this.ObjectNameLabel.Text = "schema.objectname - type";
			// 
			// DatabaseLabel
			// 
			this.DatabaseLabel.AutoEllipsis = true;
			this.DatabaseLabel.AutoSize = true;
			this.DatabaseLabel.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.DatabaseLabel.Location = new System.Drawing.Point(3, 24);
			this.DatabaseLabel.Name = "DatabaseLabel";
			this.DatabaseLabel.Size = new System.Drawing.Size(121, 17);
			this.DatabaseLabel.TabIndex = 2;
			this.DatabaseLabel.Text = "server.database";
			// 
			// ObjectInfoControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.DatabaseLabel);
			this.Controls.Add(this.ObjectNameLabel);
			this.Controls.Add(this.tabControl1);
			this.Name = "ObjectInfoControl";
			this.Size = new System.Drawing.Size(462, 265);
			this.Load += new System.EventHandler(this.ObjectInfoControl_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Label ObjectNameLabel;
		private System.Windows.Forms.Label DatabaseLabel;

	}

}