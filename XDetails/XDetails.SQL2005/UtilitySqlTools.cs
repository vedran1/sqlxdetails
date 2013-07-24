using Microsoft.SqlServer.Management.Smo.RegSvrEnum;                  // u Microsoft.SqlServer.RegSvrEnum.dll
using Microsoft.SqlServer.Management.Common;                          // u Microsoft.SqlServer.ConnectionInfo.dll
using Microsoft.SqlServer.Management.UI.VSIntegration;                // u SqlWorkbench.Interfaces.dll
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer; // u SqlWorkbench.Interfaces.dll
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;        // u Microsoft.SqlServer.SqlTools.VSIntegration.dll
using System.Text;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Data;
using System.Diagnostics;
using System;
using System.Windows.Forms;

namespace XDetails
{
	/// <summary>
	/// Singleton klasa, kreira se samo jednom po jednom pokretanju aplikacije (SSMS) i svima je dostupna.
	/// </summary>
	/// <remarks></remarks>
	public partial class UtilitySqlTools
	{
		private static readonly UtilitySqlTools currentInstance = new UtilitySqlTools();
		//kreiramo samo jednu instancu (shared)

		private UIConnectionInfo currentUIConnection;
		private SqlConnectionInfo currentConnection;

		/// <summary>
		/// Privatni konstruktor. Nitko ne može napraviti instancu ove klase, osim njene vlastite shared metode.
		/// </summary>
		/// <remarks></remarks>
		private UtilitySqlTools()
		{
		}

		public static UtilitySqlTools Current
		{
			get { return currentInstance; }
		}


		/// <summary>
		/// Vraca trenutno odabranu bazu u combo box-u. Ako prazan string ako nije nista odabrano.
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		public string GetActiveDatabaseName()
		{
			UIConnectionInfo uiConnInfo = GetActiveUIConnectionInfo();
			if (uiConnInfo != null)
			{
				return uiConnInfo.AdvancedOptions["DATABASE"];
			}
			return string.Empty;
		}

		/// <summary>
		/// Iz UIConnectionInfo builda SqlConnectionInfo kopirajući server, username, password, odabranu bazu.
		/// SqlConnectionInfo zna na temelju toga vratiti connectionstring i mozes ga koristiti za kreiranje sql konekcija.
		/// </summary>
		/// <param name="uiConnInfo"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		private SqlConnectionInfo CreateSqlConnectionInfo(UIConnectionInfo uiConnInfo)
		{
			SqlConnectionInfo sqlConnInfo = new SqlConnectionInfo();
			sqlConnInfo.ServerName = uiConnInfo.ServerName;
			sqlConnInfo.UserName = uiConnInfo.UserName;
			if (string.IsNullOrEmpty(uiConnInfo.Password))
			{
				sqlConnInfo.UseIntegratedSecurity = true;
			}
			else
			{
				sqlConnInfo.Password = uiConnInfo.Password;
			}
			sqlConnInfo.DatabaseName = uiConnInfo.AdvancedOptions["DATABASE"];
			sqlConnInfo.ConnectionTimeout = 3; // default je 15, jedinice su sekunde
			sqlConnInfo.QueryTimeout = 5; // max trajanje upita
			sqlConnInfo.ApplicationName = "XDetails"; // da se vidi u sesijama baze tko se spaja
			return sqlConnInfo;
		}

		public SqlConnectionInfo GetActiveWindowConnectionInfo()
		{
			SqlConnectionInfo info = null;
			try
			{
				UIConnectionInfo uiConnInfo = GetActiveUIConnectionInfo();
				//Dim uiConnInfo As UIConnectionInfo = Nothing
				//If ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo IsNot Nothing Then
				//	uiConnInfo = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo.UIConnectionInfo
				//End If
				if (uiConnInfo != null)
				{
					if (uiConnInfo == currentUIConnection)
					{
						return currentConnection;
					}
					else
					{
						info = CreateSqlConnectionInfo(uiConnInfo);
						currentConnection = info;
						currentUIConnection = uiConnInfo;
					}
				}

				if (info == null)
				{	//desiti ce se ako GetActiveUIConnectionInfo() vrati nothing
					INodeInformation[] nodes = GetObjectExplorerSelectedNodes();
					if (nodes.Length > 0)
					{
						info = nodes[0].Connection as SqlConnectionInfo;
					}
				}
				return info;
			}
			//catch (NullReferenceException e)
			catch (Exception e)
			{
				throw; // rethrows exception
				throw (new ApplicationException("GetActiveWindowConnectionInfo: unable to find connection info", e));
				//return null;
			}
		}


		/// <summary>
		/// Returns connection string of active window.
		/// </summary>
		/// <returns>Nothing if there is no active sql window, or unable to get connection info</returns>
		/// <remarks></remarks>
		public string GetActiveWindowConnectionString()
		{
			SqlConnectionInfo connInfo;
			try
			{
				connInfo = this.GetActiveWindowConnectionInfo();
				if (connInfo != null)
				{
					return connInfo.ConnectionString;
				}
			}
			catch (Exception e)
			{
				throw (e);
				throw (new ApplicationException("GetActiveWindowConnectionString: unable to find connection info", e));
			}
			return null;
		}

		public static void CreateNewDocument(string s)
		{
			ServiceCache.ScriptFactory.CreateNewBlankScript(ScriptType.Sql);
			EnvDTE.TextDocument doc = (EnvDTE.TextDocument)ServiceCache.ExtensibilityModel.Application.ActiveDocument.Object(null);
			doc.StartPoint.CreateEditPoint().Insert(s);
		}



		/// <summary>
		/// Executes given sql in internally opened connection and returns resulting data table (just one).
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		/// <remarks>
		/// 	Parametri:
		///  command - SQL SELECT komanda ili naziv storane procedure koja u sebi radi select.
		///  parameters - nazivi i vrijednosti parametara sql upita odnosno procedure.
		///    Svi trebaju početi sa "@".
		///    Sve vrijednosti su String tipa. Ukoliko se radi o broju ili datumu (tj. ne-stringu),
		///    moraš iza @ nadodati oznaku tipa u uglatim zagradama.
		///    Ovo su moguće oznake tipova:
		///       @[d] = datum
		///       @[n] = broj (decimal, isto što i numeric sql tip)
		///    Ukoliko je vrijednost prazan string, to se prevodi u database NULL.
		///    Ukoliko si naveo parametre koji se NE KORISTE,
		///      u slučaju select-a oni se ignoriraju i ne smetaju,
		///      ali u slučaju procedure javiti će grešku za svaki parametar viška.
		///      U oba slučaja ako je dano manje parametaara nego select ili procedura u sebi imaju, javlja se greška.
		/// </remarks>
		public DataTable GetTable(string sql, string connStr, NameValueCollection parameters)
		{
			SqlConnection conn = null;
			//postavljam na nothing da mi compiler ne baca upozorenja da nisam postavio varijablu
			SqlCommand command = null;
			SqlDataReader reader = null;
			DataTable table = null;
			try
			{
				conn = new SqlConnection(connStr);
				conn.Open();

				command = new SqlCommand(sql, conn);
				command.CommandTimeout = 1000;
				command.CommandType = CommandType.Text;

				//Napuni parametre
				if ((parameters != null))
				{
					foreach (string paramName in parameters.Keys)
					{
						SqlParameter param = new SqlParameter();
						param.IsNullable = true;
						// Ako je naveden tip parametra
						if (paramName.StartsWith("@["))
						{
							param.ParameterName = paramName.Remove(1, 3);
							//makni oznaku tipa
							switch (paramName.Substring(1, 3).ToUpper())
							{
								case "[D]":
									//datum oblika ddmmyyyy
									param.SqlDbType = SqlDbType.DateTime;
									if (string.IsNullOrEmpty(parameters[paramName]))
									{
										param.Value = SqlDateTime.Null;
									}
									else
									{
										//param.Value = ParseDate(parameters(paramName)) ' za ddmmyyyy oblik
										//SqlDateTime.Parse(parameters(paramName))
										param.SqlDbType = SqlDbType.DateTime;
										param.Value = DateTime.Parse(parameters[paramName]);
									}

									break;
								case "[N]":
									//broj
									param.SqlDbType = SqlDbType.Decimal;
									//Decimal je isto što i Numeric sql tip
									if (string.IsNullOrEmpty(parameters[paramName]))
									{
										param.Value = SqlDecimal.Null;
									}
									else
									{
										param.Value = SqlDecimal.Parse(parameters[paramName]);
									}

									break;
								default:
									Debug.Fail("Nepoznata oznaka tipa podatka u nazivu parametra: " + paramName);
									Debug.Assert(false);
									//TODO: raise exception!
									break;
							}
						}
						else
						{
							// nije naveden tip parametra, dakle radi se o string tipu (default)
							param.ParameterName = paramName;
							param.Value = parameters[paramName];
						}
						//da li je naveden tip parametra

						command.Parameters.Add(param);
						// parameters.Keys
					}
				}
				// Not parameters Is Nothing


				reader = command.ExecuteReader();

				table = new DataTable();
				table.Load(reader);
				return table;
			}
			catch (SqlException exception)
			{
				//MessageBox.Show("Greška: " + exception.Message);
				Debug.WriteLine("SQL Error: " + exception.Message);
				throw;
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
					reader.Dispose();
				}
				if (command != null) command.Dispose();
				if (conn != null)
				{
					conn.Close();
					//zatvaranje konekcije koja nije otvorena nece proizvesti gresku. Close() mozes prozivati koliko god hoces puta.
					conn.Dispose();
				}
			}

			//return null;
		}

		//Cache za verziju baze, da ne mora svaki puta ići upitom na bazu i provjeravati. 
		//Ključ je connection string, a vrijednost je 10 za sql2008, 9 za 2005, i 8 za 2000.
		private Dictionary<string, int> _DbVersionCache = new Dictionary<string, int>();

		/// <summary>
		/// Returns 8 for sql2000, 9 for sql2005, and 10 for sql2008. Has caching mechanism inside.
		/// </summary>
		/// <returns></returns>
		/// <remarks>Ima caching mehanizam u sebi</remarks>
		public int GetDbVersion(string connStr)
		{
			//Postoji li vrijednost vec u cache-u?
			if (_DbVersionCache.ContainsKey(connStr))
			{
				return _DbVersionCache[connStr];
			}

			//Verziju procitaj upitom nad bazom
			string sql = "select version=CONVERT(float,CONVERT(VARCHAR(2),SERVERPROPERTY('productversion')))";
			DataTable table = GetTable(sql, connStr, null);
			int version = Convert.ToInt32(table.Rows[0]["version"]);
			_DbVersionCache.Add(connStr, version);
			//Dodaj u cache
			return version;
		}

		/// <summary>
		///	Returns full object name at cursor. Object name can consist of multiple parts connected with dot.
		///	E.g. "[master].dbo.[my function]"
		/// </summary>
		/// <param name="lineText"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		public string GetObjectNameAtPosition
			(string textLine, // Linija teksta u kojoj tražiš naziv objekta
				int cursorPos // 0-based pozicija slova koje se nalazi desno od kursora
			)
		{
			int pos, start, end; //0-based location of opening and closing brackets
			string word; // excluding quotes
			string fullName = ""; // e.g. dbo.[lalala]
			int start1, end1; //start and end of a word with cursor

			GetWordAtCursor(textLine, cursorPos, out start1, out end1, out word);
			if (word == "") return String.Empty; // there is no word at cursor
			//fullName = textLine.Substring(start1, end1 - start1 + 1); //initial word at cursor
			fullName = word; //initial word at cursor

			//*** To the left of the cursor
			pos = start1 - 1; start = start1;
			while (start > 0 && textLine[start - 1] == '.')
			{
				GetWordAtCursor(textLine, pos, out start, out end, out word);
				//fullName = textLine.Substring(start, end - start + 1) + "." + fullName;
				fullName = word + "." + fullName;
				pos = start - 1;
			}

			//*** To the right of the cursor
			pos = end1 + 2; end = end1;
			while (end < textLine.Length - 1 && textLine[end + 1] == '.')
			{
				GetWordAtCursor(textLine, pos, out start, out end, out word);
				//fullName += "." + textLine.Substring(start, end - start + 1);
				fullName += "." + word;
				pos = end + 2;
			}

			//fullName.Remove(fullName.Length - 1); //remove last character (dot)

			return fullName;
		}

		/// <summary>
		/// Tells whether character is allowed within sql server identifier (object names).
		/// Rules for sql identifiers: http://msdn.microsoft.com/en-us/library/ms175874%28v=sql.90%29.aspx
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool CharIsAllowedInSqlIdentifier(char c)
		{
			return Char.IsLetterOrDigit(c) // unicode slovo ili znamenka
			|| c.ToString().IndexOfAny("@#_$".ToCharArray()) > -1;
		}

		/// <summary>
		/// Inspects whether cursor at given position in text line touches brackets (or double quotes or single quotes),
		/// or is inside them, and returns the word between these brackets.
		/// Brackets and quotes can be: [], "", ''
		/// If there are no brackets, return text left and right of cursor position,
		/// limited by space, tab, dot, or text end.
		/// </summary>
		/// <param name="textLine">line of text that you are inspecting</param>
		/// <param name="cursorPos">0-based position of the first character on the right of cursor caret.</param>
		public void GetWordAtCursor
			(string textLine,
				int cursorPos,
				out int o_start, // start and and 0-based index of brackets
				out int o_end,
				out string o_word // word between brackets (excluding brackets)
			)
		{
			o_start = -1;
			o_end = -1;
			o_word = string.Empty;

			int start, end; //0-based location of opening and closing brackets
			//string word; // excluding quotes

			if (textLine.Length == 0) return; // empty text, empty result.

			//***First, check if you are inside of or touching []
			if (cursorPos >= textLine.Length) //If cursor is at end of the line, put it before last character.
				start = textLine.Substring(0, cursorPos).LastIndexOf('[');
			else
				start = textLine.Substring(0, cursorPos + 1).LastIndexOf('[');

			if (start > -1)
			{
				end = textLine.IndexOf(']', start);
				if (end >= cursorPos - 1) // we are in [] brackets!
				{
					o_start = start;
					o_end = end;
					o_word = textLine.Substring(start + 1, end - start - 1);
					return;
				}
			}

			//***Check if you are in double quotes ""
			int qoutesCount = textLine.Length - textLine.Replace("\"", "").Length;
			end = -1; // start searching from beginning
			if (qoutesCount > 0 && qoutesCount % 2 == 0) //number of " in line should be even
			{
				//Go from pair to pair of quotes, and see if cursorPos is within or touching them
				while (true)
				{
					start = textLine.IndexOf('"', end + 1);
					if (start == -1) break; // no more quotes
					end = textLine.IndexOf('"', start + 1);
					if (cursorPos >= start && cursorPos <= end + 1)
					{
						o_start = start;
						o_end = end;
						o_word = textLine.Substring(start + 1, end - start - 1);
						return;
					}
				}
			}

			//***Check if you are in single quotes ''
			qoutesCount = textLine.Length - textLine.Replace("'", "").Length;
			end = -1; // start searching from beginning
			if (qoutesCount > 0 && qoutesCount % 2 == 0) //number of " in line should be even
			{
				//Go from pair to pair of quotes, and see if cursorPos is within or touching them
				while (true)
				{
					start = textLine.IndexOf('\'', end + 1);
					if (start == -1) break; // no more quotes
					end = textLine.IndexOf('\'', start + 1);
					if (cursorPos >= start && cursorPos <= end + 1)
					{
						o_start = start;
						o_end = end;
						o_word = textLine.Substring(start + 1, end - start - 1);
						return;
					}
				}
			}

			//*** There are no brackets or quotes of any kind. Read from cursor out until space, tab, or line end.
			start = cursorPos;
			end = cursorPos; //pocetak i kraj rijeci koju smo obuhvatili (0-based)
			if (end > textLine.Length - 1) end = textLine.Length - 1; //ne idi preko kraja linije

			//char[] spaceChars = " \t.()-".ToCharArray(); // praznine
			
			//Ako lijevo od kursora ima ne-razmaka, obuhvati ih sve dok ne naidjes na razmak ili kraj
			//while (start > 0 && textLine[start - 1].ToString().IndexOfAny(spaceChars) == -1)
			while (start > 0 && CharIsAllowedInSqlIdentifier(textLine[start - 1]) )
			{
				start--;
			}

			//Ako desno od kursora ima ne-razmaka, obuhvati ih sve dok ne naidjes na razmak ili kraj
			if (end < textLine.Length - 1 && CharIsAllowedInSqlIdentifier(textLine[end]) )
			{
				while (end < textLine.Length - 1 && CharIsAllowedInSqlIdentifier(textLine[end + 1]) )
				{
					end++;
				}
			}
			if (end <= textLine.Length - 1 && !CharIsAllowedInSqlIdentifier(textLine[end]) )
			{
				end--;
			}

			//Debug.WriteLine(string.Format("wordStart: {0}, wordEnd: {1}", start, end));
			if (start <= end && end <= textLine.Length - 1)
			{
				//imamo riječ!
				o_start = start;
				o_end = end;
				o_word = textLine.Substring(start, end - start + 1);
			}
		} // TouchingBrackets


		/// <summary>
		/// UIConnectionInfo sadrzi podatke o konekciji aktivnog prozora (server, username, pwd),
		/// trenutno odabranu bazu (u .AdvancedOptions("DATABASE")), i mnoge druge info. 
		/// Ali nema connection string, iako ga mozemo napraviti iz tih info.
		/// </summary>
		/// <returns>null - ukoliko ne može doći do informacija (moram istražiti kada je to)</returns>
		/// <remarks></remarks>
		public UIConnectionInfo GetActiveUIConnectionInfo()
		{
			CurrentlyActiveWndConnectionInfo wndConnInfo = Microsoft.SqlServer.Management.UI.VSIntegration.ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo;
			//wndConnInfo = (CurrentlyActiveWndConnectionInfo)ServiceCache.ServiceProvider.GetService(typeof(UIConnectionInfo));
			if (wndConnInfo != null)
			{
				UIConnectionInfo uiConnInfo = default(UIConnectionInfo);
				//uiConnInfo = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo.UIConnectionInfo;
				uiConnInfo = wndConnInfo.UIConnectionInfo;
				return uiConnInfo;
			}
			return null;
		}
	
	} // class
} // namespace