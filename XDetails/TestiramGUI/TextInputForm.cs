using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestiramGUI
{
	public partial class TextInputForm : Form
	{
		public string Question { get; set; }
		public string Answer { get; set; }

		public TextInputForm()
		{
			InitializeComponent();
		}

		private void TextInputForm_Load(object sender, EventArgs e)
		{
			this.Text = Question;
			this.textBox1.Text = Answer;
		}

		private void TextInputForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Answer = this.textBox1.Text;
		}
	}
}
