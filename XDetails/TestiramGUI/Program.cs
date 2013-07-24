using System;
using System.Windows.Forms;
using GridTest;
using XDetails;

namespace TestiramGUI
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			RegistrationForm kupto = new RegistrationForm();
			kupto.ShowDialog();
			//return;

			TextInputForm tif = new TextInputForm();
			tif.Question = "Pitanje je sad?";
			tif.Answer = "Ponuđeni odgovor";
			DialogResult dialogResult = tif.ShowDialog();
			if (dialogResult == DialogResult.OK) MessageBox.Show("Upisano: "+tif.Answer);
			else MessageBox.Show("Pritisnut je Cancel");

			FormFast ff = new FormFast();
			ff.Show();

			Application.Run(new FormHyperlinks());
		}
	}
}
