using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace GridTest
{
	public partial class FormFast : Form
	{
		public FormFast()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			//SqlConnection con = new SqlConnection("Data Source=localhost;Initial Catalog=HRNET_KONZUM;Integrated Security=True");
			SqlConnection con = new SqlConnection(@"Data Source=SQL2K\SQL2000DEV;Initial Catalog=IntegraHRP_VIP;User Id=sa;Password=sa");
			con.Open();
			//SqlCommand cmd = new SqlCommand("SELECT top 100 * FROM Zaposleni");
			SqlDataAdapter a = new SqlDataAdapter("SELECT top 100 * FROM Zaposleni",con);
			DataTable table = new DataTable();
			a.Fill(table);
			
			dataGridView1.DataSource = table;
		}

		static SolidBrush BrushText = new SolidBrush(SystemColors.WindowText);
		static SolidBrush BrushBackground = new SolidBrush(SystemColors.Window);
		static SolidBrush BrushBackgroundHighlight = new SolidBrush(SystemColors.Highlight);
		static Pen PenBorder = new Pen(SystemColors.ActiveBorder);
		static Font FontText = SystemFonts.DefaultFont;

		private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			//return;
			DataGridView dgv = sender as DataGridView;
			if (dgv == null) return;
			if (e.RowIndex == -1 || e.ColumnIndex == -1 ) return; // header

			SolidBrush brushText, brushBackground;

			//DataGridViewCell cell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
			Rectangle rec = e.CellBounds;
			Graphics g = e.Graphics;
			//if (dgv.SelectedCells.Contains(cell))
			if ((e.State & DataGridViewElementStates.Selected) != 0)
			{
				//e.Paint(rec, e.PaintParts);
				//e.Handled = true;
				//return;
				brushText = BrushBackground;
				brushBackground = BrushBackgroundHighlight;
			}
			else
			{
				brushText = BrushText;
				brushBackground = BrushBackground;
			}


			//if (e.RowIndex == this.dataGridView1.CurrentCell.RowIndex) return; // are we painting selected row?

			//e.PaintBackground(e.ClipBounds, true); // usporava jako!

			g.FillRectangle(brushBackground, rec);
			//g.DrawLines(PenBorder, new Point[] { rec.Location, new Point(rec.Right-1, rec.Top), new Point(rec.Right-1, rec.Bottom) }); // jako usporava
			//g.DrawRectangle(pn, rec); // usporava jako!
			if (e.FormattedValue != null)
			{
				//g.DrawString(e.FormattedValue.ToString(), e.CellStyle.Font, brushText, rec.Location.X, rec.Location.Y+3 );
				g.DrawString(e.FormattedValue.ToString(), FontText, brushText, rec.Location.X, rec.Location.Y + 3);
			}
			e.Handled = true;
		}
	}
}
