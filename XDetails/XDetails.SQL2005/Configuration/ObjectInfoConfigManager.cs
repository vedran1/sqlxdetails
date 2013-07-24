using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace XDetails.Configuration
{

	/// <summary>
	/// Klasa koja služi za dohvat konfiguracije iz XML datoteke "objectInfo.config".
	/// Klasa učitava podatke iz XML-a u konstruktoru, 
	/// i drži ih cijelo vrijeme cachirane u singletonu (instancira se samo jednom).
	/// </summary>
	public class ObjectInfoConfigManager
	{
		/// <summary>
		/// Samo ovdje navedeni Key-evi dopušteni su u xml config datoteci.
		/// Koristim enum, radije nego stringove kako bi mi još u compile-timeu mogao javiti grešku
		/// ako nije na ovom popisu dozvoljenih naziva.
		/// </summary>
		public enum AllKeysEnum
		{
			U, S, V,
			P, RF, X,
			FN, IF, TF,
			TR, C, D,
			F, L, PK, R, UQ
		}
		public static string ConvertEnumToStr(AllKeysEnum keyEnum)
		{
			// pretvori enum u string
			return Enum.GetName(typeof(AllKeysEnum), keyEnum).PadRight(2, ' ');
		}
		public static AllKeysEnum ConvertStrToEnum(string keyStr)
		{
			return (AllKeysEnum)Enum.Parse(typeof(AllKeysEnum), keyStr.TrimEnd(' '));
		}

		// singleton; konstruktor nitko ne moze pozvati jer je private. Svi koriste Current() statičku metodu da dobiju objekt (singleton).
		private static ObjectInfoConfigManager _currentInstance = new ObjectInfoConfigManager();
		public static ObjectInfoConfigManager Current
		{
			get { return _currentInstance; }
		}

		private List<DbObjectInfo> _dbObjectInfos;
		public List<DbObjectInfo> DbObjectInfos
		{
			get { return _dbObjectInfos; } //može se samo ćitati
		}

		//privatni konstruktor
		private ObjectInfoConfigManager()
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
			_dbObjectInfos = DeserializeConfigFromXml();
		}

		/// <summary>
		/// Za jedan tip database objekta, npr. table-valued funkciju,
		/// daje skup sql upita za prikaz informacija o objektu.
		/// </summary>
		public class DbObjectInfo
		{
			[XmlAttribute] // Biti ce serijaliziran kao atribut elementa DbObjectInfo
			public String Key { get; set; }
			[XmlAttribute] // Biti ce serijaliziran kao atribut elementa DbObjectInfo
			public String Name { get; set; }

			/// <summary>
			/// sql koji vraća DataTable. Dakle, SELECT ili execute neke procedure.
			/// </summary>
			//[XmlArray(ElementName="Tab")]  
			//[XmlArrayItem(ElementName = "Tab")] [XmlArray]
			[XmlArray("Tabs")]
			[XmlArrayItem("Tab")]
			public TabItem[] Tabs { get; set; } // Oprez! Moze biti null, što znači da nema tabova!
		}

		/// <summary>
		/// Jedan "tab" odnosno stranica (page) sa informacijama o objektu nastalim jednim SQL upitom.
		/// Npr. "Columns" tab ispod sebe ima upit koji daje sve atribute tablice.
		/// </summary>
		public class TabItem
		{
			[XmlAttribute] // Biti ce serijaliziran kao atribut elementa TabItem
			public string Title { get; set; }
			[XmlAttribute] // Biti ce serijaliziran kao atribut elementa TabItem
			public string SqlKey { get; set; } // id preko kojeg se može doći do konretnog upita koristeći SqlConfigManager.
			[XmlAttribute] // Biti ce serijaliziran kao atribut elementa TabItem
			public string View { get; set; } // grid ili text. Ovisno o tome kreirati će se DataGridView ili TextBox kontrola.
		}

		/// <summary>
		/// Cita sql.config datoteku i vraca ga kao niz SqlQuery objekata.
		/// </summary>
		private List<DbObjectInfo> DeserializeConfigFromXml()
		{
			XmlSerializer deserializer = new XmlSerializer(typeof(List<DbObjectInfo>));

			//string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
			//string baseDir2 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			//string appPath = Path.GetDirectoryName(Application.ExecutablePath);
			//string path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Connect)).CodeBase);
			string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // setup koristi ovu mapu kao "application path" mapu
			string path = assemblyPath + @"\objectInfo.config";

			TextReader textReader = null;
			List<DbObjectInfo> dbObjectInfos = null;
			try
			{
				//TextReader textReader = new StreamReader(@"D:\temp\objectInfo.config"); // TODO: Napravi da je u istoj mapi kao i dll, ili nekak posloži da mapa nije apsolutna, u neku usersku mapu stavi prilikom setupa ili nešto takvo.
				textReader = new StreamReader(path);
				dbObjectInfos = (List<DbObjectInfo>)deserializer.Deserialize(textReader); // baca System.InvalidCastException ako cast ne uspije
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
			return dbObjectInfos;
		}

		/// <summary>
		/// Za zadani sql object tip, vraća objekt DbObjectInfo, koji predstavlja skup sql upita za prikaz informacija o objektu.
		/// </summary>
		/// <returns></returns>
		public DbObjectInfo GetDbObjectInfo(ObjectInfoConfigManager.AllKeysEnum dbObjectType)
		{
			// pretvori enum u string
			string key = ConvertEnumToStr(dbObjectType);
			// Daje prvog koji zadovoljava uvjet da ima isti ključ i verziju
			DbObjectInfo objectInfo = DbObjectInfos.Find(q => q.Key == key);
			if (objectInfo == null) return null;
			return objectInfo;
		}

	}

}