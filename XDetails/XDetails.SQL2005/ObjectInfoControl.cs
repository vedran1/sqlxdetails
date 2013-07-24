using System;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Collections.Specialized;
using Microsoft.SqlServer.Management.Common;
using XDetails.Configuration;
using System.Diagnostics;


namespace XDetails
{

	public partial class ObjectInfoControl : UserControl
	{

		#region "Private Properties"
		private SqlConnectionInfo _sqlConnInfo;
		public SqlConnectionInfo SqlConnInfo
		{
			get { return _sqlConnInfo; }
			set { _sqlConnInfo = value; }
		}

		//private string _connStr;
		public string ConnectionString
		{
			get
			{
				if (_sqlConnInfo != null) return _sqlConnInfo.ConnectionString;
				else return null;
			}
			//set { _connStr = value; }
		}

		//private string _objectNameToSearchFor;
		//public string ObjectNameToSearchFor
		//{
		//   get { return _objectNameToSearchFor; }
		//   set { _objectNameToSearchFor = value; }
		//}

		//Private _useDbName As String
		//Public Property UseDbName() As String
		// Get
		// Return _useDbName
		// End Get
		// Set(ByVal value As String)
		// _useDbName = value
		// End Set
		//End Property
		#endregion



		#region "Db Object Info: InfoDbObjectBasic class"

		private class InfoDbObjectBasic
		{
			public int id;
			public string xtype;
			public string name;
			public string type_desc; //TODO: ovo obriši u budućnosti, nigdje se ne koristi i ne treba mi, jer imam u configu nazive.
			public string type_desc_cmd; // rijec koja se koristi u sql komandama koje barataju s ovim tipom objekata
			public string schema_name;
			public string db_name;
			public int db_id;

			//Constructor
			public InfoDbObjectBasic(int id, string xtype, string name, string schema_name, string db_name, int db_id)
			{
				this.id = id;
				this.name = name;
				this.xtype = xtype;
				this.schema_name = schema_name;
				this.db_name = db_name;
				this.db_id = db_id;

				switch (xtype)
				{
					case "C ": type_desc = "Check constraint"; type_desc_cmd = "CONSTRAINT";  break;
					case "D ": type_desc = "Default"; type_desc_cmd = "CONSTRAINT"; break;
					case "F ": type_desc = "Foreign key constraint"; type_desc_cmd = "CONSTRAINT"; break;
					case "L ": type_desc = "Log"; type_desc_cmd = "LOG"; break;
					case "FN": type_desc = "Scalar function"; type_desc_cmd = "FUNCTION"; break;
					case "IF": type_desc = "In-lined table-function"; type_desc_cmd = "FUNCTION"; break;
					case "P ": type_desc = "Stored procedure"; type_desc_cmd = "PROCEDURE"; break;
					case "PK": type_desc = "Primary key constraint"; type_desc_cmd = "CONSTRAINT"; break;
					case "R ": type_desc = "Rule"; type_desc_cmd = "RULE"; break;
					case "RF": type_desc = "Replication filter stored procedure"; type_desc_cmd = "PROCEDURE"; break;
					case "S ": type_desc = "System table"; type_desc_cmd = "TABLE"; break;
					case "TF": type_desc = "Table function"; type_desc_cmd = "FUNCTION"; break;
					case "TR": type_desc = "Trigger"; type_desc_cmd = "TRIGGER"; break;
					case "U ": type_desc = "User table"; type_desc_cmd = "TABLE"; break;
					case "UQ": type_desc = "Unique constraint"; type_desc_cmd = "CONSTRAINT"; break;
					case "V ": type_desc = "View"; type_desc_cmd = "VIEW"; break;
					case "X ": type_desc = "Extended stored procedure"; type_desc_cmd = "PROCEDURE"; break;
					default: type_desc = string.Format("Unknown SQL object type ({0})", xtype); break;
				}
			}
		}

		/// <summary>
		/// U zadanoj bazi traži TOCNO JEDAN objekt preko navedenog potpunog naziva, u bilo kojoj shemi.
		/// Zadan je object name or schema.name or database.schema.name or database..name,
		/// That is, every form of object identifier that OBJECT_ID() can receive.
		/// 
		/// Ime objekta je jedinstveno unutar sheme. ID objekta je jedinstven unutar baze.
		/// U istoj bazi može postojati više objekta istog imena, dokle god su u različitim shemama.
		/// 
		/// TODO:
		/// Treba najprije vidjeti da li je zadan samo naziv ili naziv i shema, ili naziv i shema i baza objekta,
		/// i onda prema tome i tražiti ID i ostale podatke tog objekta. 
		/// Ako je zadan naziv i shema, tražiš u trenutnoj bazi i zadanoj shemi.
		/// Ako je zadan naziv, tražiš u trenutnoj shemi trenutne baze.
		/// </summary>
		/// <param name="connStr"></param>
		/// <param name="objName"></param>
		/// <returns></returns>
		private InfoDbObjectBasic GetDbObjectBasicInfo(string connStr, string objName)
		{
			int version = UtilitySqlTools.Current.GetDbVersion(this.ConnectionString);
			string sql = SqlConfigManager.Current.GetSql("GetDbObjectBasicInfo", version);
			NameValueCollection par = new NameValueCollection();
			par["@objName"] = objName;
			DataTable table = UtilitySqlTools.Current.GetTable(sql, connStr, par);
			if( table == null ) return null;
			if ( table.Rows.Count == 1 )
			{
				return new InfoDbObjectBasic
				(	(int)table.Rows[0]["id"],
					(string)table.Rows[0]["xtype"],
					(string)table.Rows[0]["name"],
					(string)table.Rows[0]["schema_name"],
					(string)table.Rows[0]["db_name"],
					System.Convert.ToInt32(table.Rows[0]["db_id"]) // vrijednost je tipa "short" pa bi sirovi cast u int javio grešku "Specified cast is not valid"
				);
			}

			//nema objekta s tim nazivom
			return null;
		}

		/// <summary>
		/// Pretrazivanje po dijelu imena. Moze dati puno redaka ili niti jedan ili jedan.
		/// Daje atribute: [Object name], [Schema], [Type], [Create date], a od SQL2005 na dalje daje i [Modify date].
		/// TODO: Ovo cu prepraviti da uzme listu svih objekata u bazi, i nju filtriraš "as you type".
		/// TODO: Neka uzima sql ovisan o verziji baze, a ne da hardkodiraš select.
		/// </summary>
		/// <param name="connStr"></param>
		/// <param name="strFilter"></param>
		/// <returns></returns>
		private DataTable GetObjects(string connStr, string strFilter)
		{
			int version = UtilitySqlTools.Current.GetDbVersion(this.ConnectionString);
			string sql = SqlConfigManager.Current.GetSql("GetAllObjects", version);
			NameValueCollection par = new NameValueCollection();
			// tu je bitan poredak. Npr. ako prvo zamijenim _ sa \_ te zatim \ sa \\, nebu dobro!
			par["@objname"] = "%" + strFilter.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_") + "%";
			DataTable table = UtilitySqlTools.Current.GetTable(sql, connStr, par);
			return table;
		}

		#endregion


		private class DbObjectInfoTabPage : TabPage
		{
			internal ObjectInfoConfigManager.TabItem _tabConfig; //informacije o tabu pročitane iz xml konfiguracije
			internal InfoDbObjectBasic _basicInfo; //osnovne informacije o objektu kojeg ćemo opisivati
			internal string _connStr; //konekcija na koju se spajam da nadjem objekt
			private bool _bDataIsLoaded; // da li su već kreirane kontrole (datagrid ili textbox) na ovom tabu i podaci napunjeni?
			// Postavlja se nakon prvog punjenja podataka (databindinga)

			//Konstruktor
			public DbObjectInfoTabPage(string text) : base(text) { }


			//Kreira gridview ili textbox na tabpage-u i popunjava ga odgovarajućim podacima iz baze.
			//Jako moćno.
			public void CreateControls()
			{
				DbObjectInfoTabPage tabPage = this;
				//Prvo nadji opis ovog tab-a iz konfiguracije, tj. TabItem
				ObjectInfoConfigManager.TabItem tab = tabPage._tabConfig;

				DataTable table = null;
				if(RegistrationHelper.CheckRegistrationData() == false 
					&& tabPage._tabConfig.Title != "Columns"
					&& tabPage._tabConfig.Title != "Source"
				)
				{
					table = new DataTable();
					table.Columns.Add("Visit www.sqlxdetails.com to find how!");
					table.Rows.Add("See valuable data instead of this message by small amount needed to buy this product. Support further development! Thank you!");
				}
				else
				{
					//Nadji SQL, za odgovarajuću verziju sql servera
					//string sql = SqlConfigManager.Current.GetSql(SqlConfigManager.ConvertStrToEnum(tab.SqlKey), version);
					int version = UtilitySqlTools.Current.GetDbVersion(_connStr);
					string sql = SqlConfigManager.Current.GetSql(tab.SqlKey, version);
					if (sql != null) // u konfiguraciji postoji upit za tu verziju sql servera
					{
						//Svaki token $(db_name) zamijeni sa imenom baze u kojoj je objekt.
						//Kasnije ćeš isti taj podatak dodavati kao parametar, ali parametar ne možeš koristiti npr. za ime tablice,
						// npr. "select * from $(db_name).dbo.sysobjects" pa mi treba i ovako kao zamjenjivi token.
						sql = sql.Replace("$(db_name)", _basicInfo.db_name);

						NameValueCollection par = new NameValueCollection();
						par["@id"] = _basicInfo.id.ToString();
						par["@schema_name"] = _basicInfo.schema_name;
						par["@db_name"] = _basicInfo.db_name; //UtilitySqlTools.Current.GetActiveDatabaseName();
						par["@db_id"] = _basicInfo.db_id.ToString(); //UtilitySqlTools.Current.GetActiveDatabaseName();
						par["@name"] = _basicInfo.name;
						table = UtilitySqlTools.Current.GetTable(sql, _connStr, par);
						// Micanje constrainata je potrebno jer se neki constrainti automatski generiraju koji mi poslije smetaju.
						// Npr. za tablicu koja ima sql tip timestamp, i kad klikem na "Data" tab da selektira podatke te tablice,
						// za timestamp tip se izgenerira constraint! A ja želim maknuti kolonu timestamp tipa iz ispisa,
						// jer se ona vidi kao Byte[], i nju datagrid ne zna prikazati, isto kao niti blob.
						table.Constraints.Clear();
					}
				}
				if (table == null) return;

				Control control = null;
				if (tab.View == "text")
				{
					int m = 3; // margina
					//Dodaj gumb
					Button button = new Button();
					button.Name = "btnEdit";
					button.Anchor = AnchorStyles.Left | AnchorStyles.Top;
					button.Location = new Point(m, m);
					button.Text = "Edit";
					button.TextAlign = ContentAlignment.MiddleLeft;
					button.AutoSize = true;
					button.FlatStyle = FlatStyle.Popup;

					button.Click += this.ButtonClick; //button.Click += new System.EventHandler(this.ButtonClick);

					tabPage.Controls.Add(button);

					//text
					TextBox textBox = new TextBox();
					textBox.Name = "txtSource";
					control = textBox;
					if (table.Rows.Count > 0)
					{
						foreach (DataRow row in table.Rows)
						{
							textBox.Text += (string) row[0];
						}
						//textBox.Text = (string)table.Rows[0][0];
					}
					else textBox.Text = "(source is not available)";
					
					//Layout characteristics
					textBox.Multiline = true;

					//textBox.Dock = DockStyle.Fill;
					textBox.Location = new Point(button.Left, button.Bottom + m);
					textBox.Width = tabPage.ClientSize.Width-2*m;
					textBox.Height = tabPage.ClientSize.Height - button.Bottom - 2*m;
					textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

					textBox.ScrollBars = ScrollBars.Both;
					textBox.WordWrap = false;
					textBox.ReadOnly = true;

					//Dodaj grid ili textbox u tabpage
					tabPage.Controls.Add(control);
				}
				else
				{	// grid
					DataGridView gridView = new DataGridView();
					control = gridView;
					//Layout characteristics
					gridView.Dock = DockStyle.Fill;
					gridView.ReadOnly = true;
					gridView.AllowUserToAddRows = false;
					gridView.AllowUserToDeleteRows = false;
					gridView.AllowUserToOrderColumns = true;
					gridView.AllowUserToResizeColumns = true;
					//gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
					//gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
					gridView.RowHeadersVisible = false;
					//gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

					//Ako imamo koju BLOB kolonu, nju grid ne zna prikazati pa ćeš ja kasnije sakriti.
					//No, da bi ipak nešto prikazali, morati ćemo stvoriti istoimenu kolonu sa običnim string podatkom.
					for (int i = 0; i < table.Columns.Count; i++) //foreach ne mogu koristiti jer mi javlja grešku da ne smijem mijenjati kolekciju koju foreach-am.
					{
						DataColumn col = table.Columns[i];
						if (col.DataType.Name == "Byte[]") // tipovi timestamp, varbinary, varbinary(max), image
						{
							table.Columns.Remove(col);
							DataColumn newCol = new DataColumn(col.ColumnName, typeof(String), "'(binary data)'");
							table.Columns.Add(newCol);
						}
					}

					gridView.DataSource = table;

					//Nakon što si postavio DataSource, još uvijek grid nema columns (gridView.Column.Count=0),
					//iako tablica (DataTable) koju grid prikazuje itekako ima i retke i stupce.
					//DataGridView će dobiti svoje retke i stupce tek kad završi DataBindingComplete.
					//Zato ćemo sada dinamički nakačiti event handler na taj događaj
					//gridView.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(this.gridView_DataBindingComplete);
					gridView.DataBindingComplete += gridView_DataBindingComplete;
					gridView.CellDoubleClick += gridView_CellDoubleClick;

					//Dodaj grid ili textbox u tabpage
					tabPage.Controls.Add(control);
				}
			}

			private void gridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
			{
				DataGridView gv = sender as DataGridView;
				if (gv == null) return;
				if (e.RowIndex == -1 || e.ColumnIndex == -1) return; // header

				//if(gv.Columns[e.ColumnIndex].Name != "Description") return; // react only on description column

				if (gv.Columns["Column name"] == null) return; // if there is no "Column name" column, exit because we need the name of db table column

				string dbTableColumnDescription = gv[e.ColumnIndex, e.RowIndex].Value.ToString();
				string dbTableColumnName = gv[gv.Columns["Column name"].Index, e.RowIndex].Value.ToString();

#if DEBUG
				MessageBox.Show(string.Format("Tekst: >{0}< Kolona: {1}",
					dbTableColumnDescription, dbTableColumnName)
				);
#endif
				
			}

			private void ButtonClick(Object sender, EventArgs e)
			{
				if (!(sender is Button && e is MouseEventArgs))
					return;
				TextBox txtSource = (TextBox)this.Controls["txtSource"];
				string cmdText = string.Format(
@"IF OBJECT_ID('[{0}].[{1}]','{2}') IS NOT NULL BEGIN BEGIN TRAN DROP {3} [{0}].[{1}] END
GO
{4}
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRAN ELSE IF @@TRANCOUNT>0 COMMIT TRAN
GO
", _basicInfo.schema_name, _basicInfo.name, _basicInfo.xtype, _basicInfo.type_desc_cmd, // 0-3
 txtSource.Text // 4
 );
				UtilitySqlTools.CreateNewDocument(cmdText);
			}

			/// <summary>
			/// Tu stavi kood koji treba pozvati tek kada su kolone DataGridView-a kreirane.
			/// Ovo opaljuje jednom za svaki gridview u svom tab-u, te kod svakog sortiranja.
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			private void gridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
			{
				if (_bDataIsLoaded) return;
				if (!(sender is DataGridView)) return;
				//MessageBox.Show("gridView_DataBindingComplete");
				DataGridView gridView = (DataGridView)sender;

				foreach (DataGridViewColumn col in gridView.Columns)
				{
					if (col.ValueType.Name == "Decimal" || col.ValueType.Name == "Single" || col.ValueType.Name == "Double")
					{
						col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; //poravnaj desno (inače bi bio lijevo)
					}
					if (col.ValueType.Name.Contains("Int")) // Int16, Int32, Int64, UInt16, UInt32, UInt64
					{
						col.DefaultCellStyle.Format = "n0"; // brojčani format kakv je u locals-ima ali sa 0 decimala. Locale možeš mijenjati na DataTable objektu.
						col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; //poravnaj desno (inače bi bio lijevo)
					}
					else if (col.ValueType.Name == "Byte[]")
					{
						//TODO: Umjesto skrivanja kolone, napravi da može prikazati binarne podatke (BLOB-ove), bez da baci grešku.
						//Npr. umjesto blob podataka neka prikazuje tekst "(BLOB)"
						col.Visible = false; // sakrij je da ti ne baca grešku jer binarne podatke pokušava prikazati kao sliku
						//DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
						//c.DataPropertyName = col.DataPropertyName;
						//c.HeaderText = col.HeaderText;
						//gridView.Columns.Insert(col.Index, c);
						//gridView.Columns.Remove(col);
						//col.DefaultCellStyle.NullValue = null;
						//col.ValueType = typeof(String);
					}
					else if (col.ValueType.Name == "Boolean")
					{
						col.SortMode = DataGridViewColumnSortMode.Automatic; // bez ovoga se boolean stupci nece dati sortirati

						//Bez ovoga ne mozes razlikovati null od false vrijednosti
						DataGridViewCheckBoxColumn cb = (DataGridViewCheckBoxColumn)col; 
						cb.ThreeState = true;
					}
				}

				//gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader); //Ovo treba pozvati tek nakon što su gridview kolone generirane (DataBindingComplete)
				gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells); //Ovo treba pozvati tek nakon što su gridview kolone generirane (DataBindingComplete)
				_bDataIsLoaded = true;
			}

		}

		public ObjectInfoControl()
		{
			InitializeComponent();
		}


		public void DisplayObjectByName(string objectName)
		{
			//if (string.IsNullOrEmpty(this.ConnectionString))
			//{	// nemam connection string. Probaj ga dobiti.
			//   _sqlConnInfo = UtilitySqlTools.Current.GetActiveWindowConnectionInfo(); // može baciti exception
			//}
			if (string.IsNullOrEmpty(this.ConnectionString))
			{	// nemam connection string. Bilo bi dobro ispisati nekakvu poruku korisniku.
				MessageBox.Show("ObjectInfoControl_Load: Unable to get connection string");
				return;
			}

			string _objectNameToSearchFor = objectName;
			InfoDbObjectBasic i = GetDbObjectBasicInfo(this.ConnectionString, _objectNameToSearchFor);

			if (i == null) this.DatabaseLabel.Text = string.Format("{0}.{1}", this.SqlConnInfo.ServerName, this.SqlConnInfo.DatabaseName);
			else this.DatabaseLabel.Text = string.Format("{0}.{1}", this.SqlConnInfo.ServerName, i.db_name);

			//Ocisti tab page-ove od prijasnjeg objekta ili search rezultata
			this.tabControl1.TabPages.Clear();

			//Ako objekt toga imena ne postoji
			if (i == null)
			{
				//Ispisi listu objekata koji sadrze taj naziv.
				//TODO: Malo pametnije bi bilo ovdje vidjeti koliko smo objekata dobili.
				//Ako smo dobili 1, odmah prikazati stanje za njega.
				//Ako smo dobili 0, onda neka ispise sve objekte umjesto niti jednog.
				//Ako smo dobili 2 ili više, onda neka ispise tu listu, da korisnik odabere objekt.

				this.ObjectNameLabel.Text = string.Format("Searching for '{0}'", _objectNameToSearchFor);
				TabPage tabPage = new TabPage("Find object");

				DataGridView gridView = new DataGridView();
				gridView.Dock = DockStyle.Fill;
				gridView.ReadOnly = true;
				gridView.AllowUserToAddRows = false;
				gridView.AllowUserToDeleteRows = false;
				gridView.AllowUserToOrderColumns = true;
				gridView.AllowUserToResizeColumns = true;
				gridView.RowHeadersVisible = false;
				gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
				//gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
				gridView.DataSource = GetObjects(this.ConnectionString, _objectNameToSearchFor);

				gridView.DataBindingComplete += gridView_DataBindingComplete; //kolone gridview-a tek u tom eventu imamo kreirane
				gridView.CellContentClick += gridView_CellContentClick;


				tabPage.Controls.Add(gridView);
				this.tabControl1.TabPages.Add(tabPage);
			}
			else
			{

				//Dodaj page-ove kako piše u konfiguraciji
				ObjectInfoConfigManager.AllKeysEnum tip = ObjectInfoConfigManager.ConvertStrToEnum(i.xtype);
				ObjectInfoConfigManager.DbObjectInfo info = ObjectInfoConfigManager.Current.GetDbObjectInfo(tip);
				//this.BasicInfoLabel.Text = string.Format("{0}: [{1}].[{2}]", i.type_desc, i.schema_name, i.name);
				if (info == null) // Tip kojeg nemaš u konfiguraciji
				{
					this.ObjectNameLabel.Text = string.Format("Unknown type {0}: [{1}].[{2}]", i.xtype, i.schema_name, i.name);
					return; // nemaš šta dalje ispisivati
				}
				//Ispiši osnovne podatke
				this.ObjectNameLabel.Text = string.Format("{1}.{2} ({0})", info.Name, i.schema_name, i.name);

				//Kreiraj tab-ove
				if (info.Tabs == null) return; // nema tab-ova za taj tip objekta
				foreach (ObjectInfoConfigManager.TabItem tab in info.Tabs)
				{

					//Console.WriteLine("Title={0}, SqlKey={1}", tab.Title, tab.SqlKey);
					DbObjectInfoTabPage tabPage = new DbObjectInfoTabPage(tab.Title);

					// Bitno je pohraniti neki identifikator tab-a po kojem ćeš kasnije 
					// moći pronaći sql i generirati datagrid ili textbox za prikaz.
					// A to "kasnije" je netom prije prvog prikaza tog pojedinog tab-a,
					// a ne da odmah sada sve tabove popunjavaš podacima.
					tabPage._tabConfig = tab;
					tabPage._basicInfo = i;
					tabPage._connStr = this.SqlConnInfo.ConnectionString;

					tabPage.Enter += tabPage_Enter; //tabPage.Enter += new EventHandler(tabPage_Enter);

					//Dodaj TabPage u listu page-va (tj. u TabControl)
					this.tabControl1.TabPages.Add(tabPage);



				} // foreach

				//Kod promjene odabranog page-a se proziva.
				//Inicijalni prikaz ne opaljuje ovaj event uopće.
				//this.tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);

				//this.tabControl1.Selected += new TabControlEventHandler(tabControl1_Selected);
				//private void tabControl1_Selected(object sender, EventArgs e)
				//{
				//    MessageBox.Show("Selected!");
				//}

				//Onaj prvi tab koji ce biti prikazan, njemu odmah kreiraj kontrole (ostalima će biti kreirane kontrole tek kad se klikne na njih)
				DbObjectInfoTabPage tPage = this.tabControl1.SelectedTab as DbObjectInfoTabPage;
				if (tPage != null) { tPage.CreateControls(); } //recimo, search prozor ima tab koji nije od klase DbObjectInfoTabPage.
			} // kad imaš nadjen konkretan db objekt

		}

		private void ObjectInfoControl_Load(object sender, EventArgs e)
		{
#if DEBUG
			//Reload xml config every time AddIn window is shown - only if compiled for debug.
			ObjectInfoConfigManager.Current.LoadFromXml();
			SqlConfigManager.Current.LoadFromXml();
#endif
		} // ObjectInfoControl_Load()


		//Tu trebaš staviti kood koji želiš da se izvrši prije nego se prikaže tab.
		//Opaljuje i prije prvog prikaza, samo za tab koji će se upravo prikazati.
		private void tabPage_Enter(object sender, EventArgs e)
		{
			//MessageBox.Show("tabPage_Enter!");
			if (sender is DbObjectInfoTabPage)
			{
				DbObjectInfoTabPage tabPage = (DbObjectInfoTabPage)sender;
				if (tabPage.Controls.Count == 0) //ako još nisu generirane kontrole - kreiraj ih. To radiš samo jednom, tj. pri prvom prikazu.
				{
					tabPage.CreateControls();
				}
			}
		}


		/// <summary>
		/// Tu stavi kood koji treba pozvati tek kada su kolone DataGridView-a kreirane.
		/// Ovo opaljuje jednom za svaki gridview u svom tab-u, te kod svakog sortiranja.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void gridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			//if (_bDataIsLoaded) return;

			if (!(sender is DataGridView)) return;
			//MessageBox.Show("gridView_DataBindingComplete");
			DataGridView gridView = (DataGridView)sender;
			if (gridView.Columns["hiperlink:Object name"] != null) return; //vec smo tu bili i dodali hiperlink kolonu

			foreach (DataGridViewColumn col in gridView.Columns)
			{
				if (col.ValueType.Name == "Decimal" || col.ValueType.Name == "Single" || col.ValueType.Name == "Double")
				{
					col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; //poravnaj desno (inače bi bio lijevo)
				}
				if (col.ValueType.Name.Contains("Int")) // Int16, Int32, Int64, UInt16, UInt32, UInt64
				{
					col.DefaultCellStyle.Format = "n0"; // brojčani format kakv je u locals-ima ali sa 0 decimala. Locale možeš mijenjati na DataTable objektu.
					col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; //poravnaj desno (inače bi bio lijevo)
				}
				else if (col.ValueType.Name == "Byte[]")
				{
					//TODO: Umjesto skrivanja kolone, napravi da može prikazati binarne podatke (BLOB-ove), bez da baci grešku.
					//Npr. umjesto blob podataka neka prikazuje tekst "(BLOB)"
					col.Visible = false; // sakrij je da ti ne baca grešku jer binarne podatke pokušava prikazati kao sliku
					//DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
					//c.DataPropertyName = col.DataPropertyName;
					//c.HeaderText = col.HeaderText;
					//gridView.Columns.Insert(col.Index, c);
					//gridView.Columns.Remove(col);
					//col.DefaultCellStyle.NullValue = null;
					//col.ValueType = typeof(String);
				}
				else if (col.ValueType.Name == "Boolean")
				{
					col.SortMode = DataGridViewColumnSortMode.Automatic; // bez ovoga se boolean stupci nece dati sortirati

					//Bez ovoga ne mozes razlikovati null od false vrijednosti
					DataGridViewCheckBoxColumn cb = (DataGridViewCheckBoxColumn)col;
					cb.ThreeState = true;
				}
			}


			///*
			//Dodaj hyperlink kolonu. Ona uzima podatak iz "link:XY" kolone, i zamjenjuje XY kolonu. link:XY se također ne smije vidjeti.
			DataGridViewLinkColumn linkCol = new DataGridViewLinkColumn();
			DataGridViewColumn linkTargetCol = gridView.Columns["link:Object name"]; //ima link target vrijednosti
			DataGridViewColumn replaceCol = gridView.Columns["Object name"]; //ima "display" vrijednosti. Nju cemo zamijeniti sa hyperlinkom i nestat ce.
			//int COLNUM = linkTargetCol.Index; // kolona koju mijenjamo
			linkCol.Name = "hiperlink:" + replaceCol.Name;
			linkCol.HeaderText = replaceCol.HeaderText;
			linkCol.Text = "prva";
			linkCol.DataPropertyName = replaceCol.DataPropertyName; // datatable column name
			linkCol.LinkColor = Color.MediumBlue; //SystemColors.WindowText;
			linkCol.LinkBehavior = LinkBehavior.HoverUnderline;
			linkCol.TrackVisitedState = false;
			linkCol.SortMode = DataGridViewColumnSortMode.Automatic; // bez ovoga kolona nije sortabilna za korisnika
			linkCol.DisplayIndex = replaceCol.Index; // određuje poredak te kolone
			gridView.Columns.Add(linkCol);

			gridView.Columns.Remove(replaceCol); //Obrisi kolonu koja nam vise ne treba jer smo napravili njenu hyperlink kopiju
			linkTargetCol.Visible = false; // sakrij polje koje nosi id na koji ce voditi link
			//*/	
			
			//gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader); //Ovo treba pozvati tek nakon što su gridview kolone generirane (DataBindingComplete)
			gridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells); //Ovo treba pozvati tek nakon što su gridview kolone generirane (DataBindingComplete)
			//_bDataIsLoaded = true;
		}

		/// <summary>
		/// Ovaj event opaljuje se kad klikneš vrijednost bilo kojeg polja - ne mora biti link.
		/// Opaljuje se i kod sortiranja, ali tada ima e.RowIndex = -1.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void gridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1) return; // desava se npr. kod sortiranja
			if (!(sender is DataGridView)) return;
			DataGridView dg = (DataGridView)sender;

			//Event opaljuje na sve tipove ćelija, a nas zanima da reagiramo samo na linkove
			DataGridViewLinkCell cell = dg.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewLinkCell; // biti će null ako nije DataGridViewLinkCell
			if (cell == null) return; // ne zanimaju nas ćelije koje nisu linkovi (iako bi mogli i njih hendlati jednako)

			string NazivIdKolone = "link:"+dg.Columns[e.ColumnIndex].HeaderText;
			string VrijednostIdKolone = dg.Rows[e.RowIndex].Cells[NazivIdKolone].Value.ToString();

			//MessageBox.Show
			//(string.Format
			//   ("Clicked cell value: {0}, a id vrijednost je: {1}",
			//      dg.Rows[e.RowIndex].Cells[e.ColumnIndex].Value,
			//      VrijednostIdKolone
			//   )
			//);

			//Prozovi da se prikaže
			ObjectInfoControl oic = dg.Parent.Parent.Parent as ObjectInfoControl;
			if (oic != null)
			{
				//Debug.WriteLine("Unutar gridView_CellContentClick: ObjectInfoControl.InvokeRequired=" + oic.InvokeRequired.ToString());
				//oic.Invoke(new MethodInvoker(delegate { oic.DisplayObjectByName(VrijednostIdKolone); }));
				oic.DisplayObjectByName(VrijednostIdKolone);
			}
		}
	}

}