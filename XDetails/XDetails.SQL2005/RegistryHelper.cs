using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace XDetails
{
	//Source of this code is: http://www.codeproject.com/Articles/51326/Net-Compilation-registry-accessing-and-application.aspx?msg=3426476
	//and GetWindowsProductId part is from: http://www.pinvoke.net/default.aspx/advapi32/RegOpenKeyEx.html
	//Something different here: http://social.msdn.microsoft.com/Forums/en/windowscompatibility/thread/ff7a9023-4764-4f15-b3f8-f872d42cba75
	static class RegistryHelper
	{
/* stari kood za čitanje registryja. Ne radi dobro sa sql2012.
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegOpenKeyEx")]
		static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int sam,
			out IntPtr phkResult);

		[Flags]
		public enum eRegWow64Options : int
		{
			None = 0x0000,
			KEY_WOW64_64KEY = 0x0100,
			KEY_WOW64_32KEY = 0x0200,
			// Add here any others needed, from the table of the previous chapter
		}

		[Flags]
		public enum eRegistryRights : int
		{
			ReadKey = 131097,
			WriteKey = 131078,
		}

		public static RegistryKey OpenSubKey(RegistryKey pParentKey, string pSubKeyName,
											 bool pWriteable,
											 eRegWow64Options pOptions)
		{
			if (pParentKey == null || GetRegistryKeyHandle(pParentKey).Equals(System.IntPtr.Zero))
				throw new System.Exception("OpenSubKey: Parent key is not open");

			eRegistryRights Rights = eRegistryRights.ReadKey;
			if (pWriteable)
				Rights = eRegistryRights.WriteKey;

			System.IntPtr SubKeyHandle;
			System.Int32 Result = RegOpenKeyEx(GetRegistryKeyHandle(pParentKey), pSubKeyName, 0,
											  (int)Rights | (int)pOptions, out SubKeyHandle);
			if (Result != 0)
			{
				System.ComponentModel.Win32Exception W32ex =
					new System.ComponentModel.Win32Exception();
				throw new System.Exception("OpenSubKey: Exception encountered opening key",
					W32ex);
			}

			return PointerToRegistryKey(SubKeyHandle, pWriteable, false);
		}

		private static System.IntPtr GetRegistryKeyHandle(RegistryKey pRegisteryKey)
		{
			Type Type = Type.GetType("Microsoft.Win32.RegistryKey");
			FieldInfo Info = Type.GetField("hkey", BindingFlags.NonPublic | BindingFlags.Instance);

			SafeHandle Handle = (SafeHandle)Info.GetValue(pRegisteryKey);
			IntPtr RealHandle = Handle.DangerousGetHandle();

			return Handle.DangerousGetHandle();
		}

		private static RegistryKey PointerToRegistryKey(IntPtr hKey, bool pWritable,
			bool pOwnsHandle)
		{
			// Create a SafeHandles.SafeRegistryHandle from this pointer - this is a private class
			BindingFlags privateConstructors = BindingFlags.Instance | BindingFlags.NonPublic;
			Type safeRegistryHandleType = typeof(
				SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType(
				"Microsoft.Win32.SafeHandles.SafeRegistryHandle");

			Type[] safeRegistryHandleConstructorTypes = new Type[] { typeof(System.IntPtr),
        typeof(System.Boolean) };
			ConstructorInfo safeRegistryHandleConstructor =
				safeRegistryHandleType.GetConstructor(privateConstructors,
				null, safeRegistryHandleConstructorTypes, null);
			Object safeHandle = safeRegistryHandleConstructor.Invoke(new Object[] { hKey,
        pOwnsHandle });

			// Create a new Registry key using the private constructor using the
			// safeHandle - this should then behave like 
			// a .NET natively opened handle and disposed of correctly
			Type registryKeyType = typeof(Microsoft.Win32.RegistryKey);
			Type[] registryKeyConstructorTypes = new Type[] { safeRegistryHandleType,
        typeof(Boolean) };
			ConstructorInfo registryKeyConstructor =
				registryKeyType.GetConstructor(privateConstructors, null,
				registryKeyConstructorTypes, null);
			RegistryKey result = (RegistryKey)registryKeyConstructor.Invoke(new Object[] {
        safeHandle, pWritable });
			return result;
		}
*/

		// ne radi na sql2012 (dotnet 4), uvijek vraća null
		public static string GetWindowsProductId()
		{
			string productID;
			productID = Read64bitRegistryFrom32bitApp.RegistryWOW6432.GetRegKey64(Read64bitRegistryFrom32bitApp.RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductId");
			if (!String.IsNullOrEmpty(productID)) return productID;
			
			productID = Read64bitRegistryFrom32bitApp.RegistryWOW6432.GetRegKey32(Read64bitRegistryFrom32bitApp.RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductId");
			return productID;

			//stari kood, ne radi dobro na sql2012
			//RegistryKey currentVersion = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Microsoft").OpenSubKey("Windows NT").OpenSubKey("CurrentVersion");
			//productID = (string)currentVersion.GetValue("ProductId") ?? string.Empty;
			//if (productID == string.Empty)
			//{
			//   RegistryKey software = OpenSubKey(Registry.LocalMachine, "SOFTWARE", false, eRegWow64Options.KEY_WOW64_64KEY);
			//   RegistryKey microsoft = OpenSubKey(software, "Microsoft", false, eRegWow64Options.KEY_WOW64_64KEY);
			//   RegistryKey windowsNT = OpenSubKey(microsoft, "Windows NT", false, eRegWow64Options.KEY_WOW64_64KEY);
			//   currentVersion = OpenSubKey(windowsNT, "CurrentVersion", false, eRegWow64Options.KEY_WOW64_64KEY);
			//   productID = (string)currentVersion.GetValue("ProductId") ?? string.Empty;
			//}
			//return productID;
		}

		//ovo radi samo na dotnet 4
		//static string GetWindowsProductId()
		//{
		//   RegistryKey localMachine = null;
		//   if (Environment.Is64BitOperatingSystem)
		//   {
		//      localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
		//   }
		//   else
		//   {
		//      localMachine = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
		//   }
		//   RegistryKey windowsNTKey = localMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion");
		//   return windowsNTKey.GetValue("ProductId").ToString();
		//}
		
	}
}