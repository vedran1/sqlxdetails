using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestiramGUI
{
	public partial class FormHyperlinks : Form
	{
		public FormHyperlinks()
		{
			InitializeComponent();
		}

		private void FormHyperlinks_Load(object sender, EventArgs e)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("prva");
			dt.Columns.Add("druga");
			dt.Rows.Add(1, "aaaaaaaaaaaaaaaaaa");
			dt.Rows.Add(2, "bbbbbbbbbbbbbbbb");
			dt.Rows.Add(3, "cccccccccccccccc");
			dataGridView1.DataSource = dt;

			DataGridViewLinkColumn col = new DataGridViewLinkColumn();

			//col.UseColumnTextForLinkValue = true;
			//col.HeaderText = "linkam";
			//col.DataPropertyName = "druga";
			//col.ActiveLinkColor = Color.White;
			//col.LinkBehavior = LinkBehavior.SystemDefault;
			//col.LinkColor = Color.Blue;
			//col.TrackVisitedState = true;
			//col.VisitedLinkColor = Color.YellowGreen;

			const int COLNUM = 1;
			col.Name = "MoNoLinkLabel";
			col.HeaderText = "MO No";
			col.Text = "prva";
			col.DataPropertyName = dataGridView1.Columns[COLNUM].DataPropertyName; // datatable column name
			//col.Width = 90;
			col.LinkColor = Color.MediumBlue; //SystemColors.WindowText;
			col.LinkBehavior = LinkBehavior.HoverUnderline;
			//col.ActiveLinkColor = Color.White;
			//col.VisitedLinkColor = Color.Purple;
			col.TrackVisitedState = false;
			col.SortMode = DataGridViewColumnSortMode.Automatic; // bez ovoga kolona nije sortabilna za korisnika
			col.DisplayIndex = COLNUM; // određuje poredak te kolone

			//dataGridView1.Columns[COLNUM].Visible = false; // skrivamo kolonu koju smo zamijenili
			dataGridView1.Columns.Add(col);

			//namjerno skrivam i kolonu "prva" u kojoj je id
			//dataGridView1.Columns["prva"].Visible = false;
		}

		/// <summary>
		/// Ovaj event opaljuje se kad klikneš vrijednost bilo kojeg polja - ne mora biti link.
		/// Opaljuje se i kod sortiranja, ali tada ima e.RowIndex = -1.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if( e.RowIndex == -1 )return; // desava se npr. kod sortiranja
			if( !(sender is DataGridView) ) return;
			DataGridView dg = (DataGridView)sender;

			//Event opaljuje na sve tipove ćelija, a nas zanima da reagiramo samo na linkove
			DataGridViewLinkCell cell = dg.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewLinkCell; // biti će null ako nije DataGridViewLinkCell
			if( cell == null ) return; // ne zanimaju nas ćelije koje nisu linkovi (iako bi mogli i njih hendlati jednako)

			MessageBox.Show
			(	string.Format
				(	"Clicked cell value: {0}\nid value: {1}",
					dg.Rows[e.RowIndex].Cells[e.ColumnIndex].Value,
					dg.Rows[e.RowIndex].Cells["prva"].Value
				)
			);
			//e.ColumnIndex + e.RowIndex;
		}
	}
}
