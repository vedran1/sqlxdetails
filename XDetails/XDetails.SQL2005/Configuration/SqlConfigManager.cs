using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace XDetails.Configuration
{

	/// <summary>
	/// Klasa koja služi za dohvat SQL upita iz XML datoteke "sql.config".
	/// Trebaš samo znati ključ i verziju baze i pozvati GetSql().
	/// Klasa učitava podatke iz XML-a u konstruktoru, 
	/// i drži ih cijelo vrijeme cachirane u singletonu (instancira se samo jednom).
	/// </summary>
	public class SqlConfigManager
	{
		//Ne želim koristiti enum kod mogućih upita, kako bih mogao dinamički dodavati upite u konfiguraciju bez recompile-a!

		///// <summary>
		///// Samo ovdje navedeni Key-evi dopušteni su u sql.config xml-u.
		///// Koristim enum, radije nego stringove kako bi mi još u compile-timeu mogao javiti grešku
		///// ako nije na ovom popisu dozvoljenih naziva.
		///// </summary>
		//public enum AllKeysEnum
		//{
		//    GetSqlServerVersion,
		//    GetDbObjectBasicInfo,
		//    GetAllObjects,
		//    GetSourceCode,
		//    GetTableColumns,
		//    GetTableForeignKeys,
		//    GetTableReferencedBy
		//}
		//public static string ConvertEnumToStr(AllKeysEnum keyEnum)
		//{
		//    // pretvori enum u string
		//    return Enum.GetName(typeof(AllKeysEnum), keyEnum);
		//}
		//public static AllKeysEnum ConvertStrToEnum(string keyStr)
		//{
		//    return (AllKeysEnum)Enum.Parse(	typeof(AllKeysEnum), keyStr );
		//}


		// singleton; konstruktor nitko ne moze pozvati jer je private. Svi koriste Current() statičku metodu da dobiju objekt (singleton).
		private static SqlConfigManager _currentInstance = new SqlConfigManager();
		public static SqlConfigManager Current
		{
			get { return _currentInstance; }
		}

		private List<SqlQuery> _queries;
		public List<SqlQuery> Queries
		{
			get { return _queries; } //može se samo ćitati
		}

		//privatni konstruktor
		private SqlConfigManager()
		{
			// Ucitaj sql upite iz datoteke
			LoadFromXml();
		}

		/// <summary>
		/// Učitava konfiguracijske podatke iz xml datoteke.
		/// Služi za inicijalno punjenje, 
		/// ali i da možeš osvježiti memoriju ako je u međuvremenu promijenjena XML konfiguracija.
		/// </summary>
		public void LoadFromXml()
		{
			_queries = DeserializeConfigFromXml();
		}

		/// <summary>
		/// Predstavlja jedan Sql upit. Može imati različite varijante na različitim verzijama sql servera.
		/// </summary>
		public class SqlQuery
		{
			/// <summary>
			/// Kljuc sql statementa.
			/// </summary>
			[XmlAttribute] // Biti ce serijaliziran kao atribut elementa SqlQuery
			public String Key { get; set; }

			/// <summary>
			/// sql koji vraća DataTable. Dakle, SELECT ili execute neke procedure.
			/// </summary>
			public String Sql2000 { get; set; }
			public String Sql2005 { get; set; }
			public String Sql2008 { get; set; }
		}

		/// <summary>
		/// Cita sql.config datoteku i vraca ga kao niz SqlQuery objekata.
		/// </summary>
		private List<SqlQuery> DeserializeConfigFromXml()
		{
			XmlSerializer deserializer = new XmlSerializer(typeof(List<SqlQuery>));


			//string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
			//TextReader textReader = new StreamReader(@"D:\temp\sql.config"); // TODO: Napravi da je u istoj mapi kao i dll, ili nekak posloži da mapa nije apsolutna, u neku usersku mapu stavi prilikom setupa ili nešto takvo.
			//List<SqlQuery> queries = (List<SqlQuery>)deserializer.Deserialize(textReader); // baca System.InvalidCastException ako cast ne uspije
			//textReader.Close();

			string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // setup koristi ovu mapu kao "application path" mapu
			string path = assemblyPath + @"\sql.config";

			TextReader textReader = null;
			List<SqlQuery> queries = null;
			try
			{
				//TextReader textReader = new StreamReader(@"D:\temp\objectInfo.config"); // TODO: Napravi da je u istoj mapi kao i dll, ili nekak posloži da mapa nije apsolutna, u neku usersku mapu stavi prilikom setupa ili nešto takvo.
				textReader = new StreamReader(path);
				queries = (List<SqlQuery>)deserializer.Deserialize(textReader); // baca System.InvalidCastException ako cast ne uspije
			}
			catch (Exception e)
			{
				//Logiraj grešku, ispiši nešto korisniku, tipa: "no mogu otvoriti konfiguracijsku datoteku"
				MessageBox.Show(e.Message);
			}
			finally
			{
				if (textReader != null) textReader.Close();
			}

			return queries;
		}

		/// <summary>
		/// Vraća sql za zadanu verziju sql servera. Ukoliko takav ne postoji, vraća prazan string.
		/// </summary>
		/// <returns></returns>
		public string GetSql(string key, int dbVersion)
		{
			// pretvori enum u string
			//string key = Enum.GetName(typeof(SqlConfigManager.AllKeysEnum), keyInt);

			// Daje prvog koji zadovoljava uvjet da ima isti ključ i verziju
			SqlQuery query = Queries.Find(q => q.Key == key);
			if (query == null) return null; // nema sql-a za tu verziju
			if (dbVersion >= 10) return query.Sql2008 ?? query.Sql2005 ?? query.Sql2000; // ?? daje prvi koji nije null
			if (dbVersion >= 9) return query.Sql2005 ?? query.Sql2000;
			if (dbVersion >= 8) return query.Sql2000;
			return null;
		}

	}
}