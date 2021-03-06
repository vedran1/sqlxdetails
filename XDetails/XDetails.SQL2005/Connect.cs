using System;
using Extensibility; // definira IDTExtensibility2 interface (OnConnection, OnStartupComplete itd)
using EnvDTE; // definira IDTCommandTarget interface (Exec() i QueryStatus()), DTE interface
//using EnvDTE80; // definira većinu interface-a koji završavaju sa 2 (DTE2, Window2, ...)
using Microsoft.VisualStudio.CommandBars;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using PostSharp.Extensibility;
using XDetails.Properties;

#if DEBUG
[assembly: Trace(AttributeTargetTypes = "XDetails.*",
	AttributeTargetTypeAttributes = MulticastAttributes.All,
	AttributeTargetMemberAttributes = MulticastAttributes.NonAbstract,
	AttributePriority = 1)]
// Nemoj trace-ati tracersku klasu jer ces dobiti gresku: "Cannot apply aspects to the type XDetails.Properties.TraceAttribute because it is itself an aspect.
// AttributePriority koristim za poredak, da budem siguran da je iskljucim tek nakon sto je ukljucena.
[assembly: Trace(AttributeTargetTypes = "XDetails.Properties.TraceAttribute",
	AttributeExclude = true, AttributePriority = 2)]
#endif

namespace XDetails
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		// Constants for command properties
		//Izbjegavaj konstante jer se one ne mogu obfuscirati!
		private readonly static string MY_ADDIN_NAME = "SQL XDetails";

		private readonly static string MY_COMMAND_NAME = "XDetailsCommandGetInfo";
		private readonly static string MY_COMMAND_CAPTION = "XDetails";
		private readonly static string MY_COMMAND_TOOLTIP = "View database object details at the cursor";

		private readonly static string ABOUT_COMMAND_NAME = "XDetailsCommandAbout";
		private readonly static string ABOUT_COMMAND_CAPTION = "About";
		private readonly static string ABOUT_COMMAND_TOOLTIP = "About SQL XDetails";

		private readonly DateTime _expirationDate = new DateTime(2011, 4, 1);

		private readonly static string XDETAILS_WINDOW_KIND_ID = "{5B7F8C1C-65B9-2ACA-1AC3-12ACBBAF21D5}"; // ObjectKind je GUID svijek sa svim UPPER slovima!

		private EnvDTE.AddIn _addInInstance;
		private EnvDTE.DTE _DTE; // application object
		//private EnvDTE80.DTE2 _DTE; // application object
		//private DTE2 _DTE2;

		//private CommandEvents _CommandEvents;
		//private CommandBarControl _CommandBarControl;

		// Buttons that will be created on built-in commandbars of Visual Studio
		// We must keep them at class level to remove them when the add-in is unloaded
		private CommandBarButton myStandardCommandBarButton;
		private CommandBarButton myToolsCommandBarButton = null;
		private CommandBarButton myCodeWindowCommandBarButton = null;

		// CommandBars that will be created by the add-in
		// We must keep them at class level to remove them when the add-in is unloaded
		private CommandBar myTemporaryToolbar=null;
		private CommandBarPopup myTemporaryCommandBarPopup1 = null;
		private CommandBarPopup myTemporaryCommandBarPopup2;

		//Private _outputWindowPane As OutputWindowPane
		//Private _textEditorEvents As EnvDTE.TextEditorEvents
		//Private _textDocumentKeyPressEvents As EnvDTE80.TextDocumentKeyPressEvents

		//#Region "Object Explorer fields"
		// Dim provider As IObjectExplorerEventProvider
		// Dim cmdQueryExecute As Command = Nothing 'Command cmdQueryExecute = default(Command);
		// Dim commandEventsQueryExecute As CommandEvents = Nothing ' CommandEvents commandEventsQueryExecute = default(CommandEvents);
		//#End Region

		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(new ThreadExceptionHandler().ApplicationThreadException);
		}

		#region IDTExtensibility2 Members

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		void IDTExtensibility2.OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		void IDTExtensibility2.OnBeginShutdown(ref Array custom)
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		void IDTExtensibility2.OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
		{
			try
			{
				_addInInstance = (AddIn)AddInInst;
				//_DTE2 = (DTE2)_addInInstance.DTE;
				_DTE = (DTE)_addInInstance.DTE;
				//_DTE = (DTE2)_addInInstance.DTE;
				//ovako kaze Mladen da radi svugdje
				//_DTE = CType(ServiceCache.ExtensibilityModel, EnvDTE.DTE) 'Ovako kaze poljak sa dev2dev da treba za ssms2008 i 2005

				switch (ConnectMode)
				{
					case ext_ConnectMode.ext_cm_UISetup:

						// Do nothing for this add-in with temporary user interface
						break;

					case ext_ConnectMode.ext_cm_Startup:

						// The add-in was marked to load on startup
						// Do nothing at this point because the IDE may not be fully initialized
						// Visual Studio will call OnStartupComplete when fully initialized
						break;

					case ext_ConnectMode.ext_cm_AfterStartup:

						// The add-in was loaded by hand after startup using the Add-In Manager
						// Initialize it in the same way that when is loaded on startup
						AddTemporaryUI();
						break;
				}
			}
			catch (System.Exception e)
			{
				System.Windows.Forms.MessageBox.Show(e.ToString());
			}


			//List all commands
			//For Each com As Command In _DTE2.Commands
			// Debug.WriteLine(String.Format("Name={0} | GUID={1} | ID={2}", com.Name, com.Guid, com.ID))
			//Next


			//Dim outputWindow As OutputWindow = CType(_DTE.Windows.Item(Constants.vsWindowKindOutput).Object, OutputWindow)
			//_outputWindowPane = outputWindow.OutputWindowPanes.Add("DTE Event Information - C# Event Watcher")
			//Nemam pojma zašto se taj output window vise ne vidi i ne mogu ga nikako prikazati?

			//Retrieve the event objects from the automation model
			//_textEditorEvents = events.TextEditorEvents
			//_textDocumentKeyPressEvents = (CType(events, EnvDTE80.Events2)).TextDocumentKeyPressEvents
			//Connect to each delegate exposed from each object retrieved above
			//AddHandler _textDocumentKeyPressEvents.AfterKeyPress, AddressOf AfterKeyPress



			//Name=Edit.DoubleClick | GUID={1496A755-94DE-11D0-8C3F-00C04FC2AAE2} | ID=134
			//Events: AfterExecute, BeforeExecute
			//Ne koristi se nigdje, pa ne vidim svrhu.
			//_CommandEvents = _DTE.Events.CommandEvents("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}", 134)



			//----Ovo dole ne briši jer možeš naučiti neke trikove, npr. kako napraviti gumb sa grafikom

			//If connectMode = ext_ConnectMode.ext_cm_UISetup Then

			// Dim commands As Commands2 = CType(_DTE2.Commands, Commands2)
			// Dim toolsMenuName As String
			// Try

			// 'If you would like to move the command to a different menu, change the word "Tools" to the
			// ' English version of the menu. This code will take the culture, append on the name of the menu
			// ' then add the command to that menu. You can find a list of all the top-level menus in the file
			// ' CommandBar.resx.
			// Dim resourceManager As System.Resources.ResourceManager = New System.Resources.ResourceManager("SSMSAddinTest3.CommandBar", System.Reflection.Assembly.GetExecutingAssembly())

			// Dim cultureInfo As System.Globalization.CultureInfo = New System.Globalization.CultureInfo(_DTE2.LocaleID)
			// If (cultureInfo.TwoLetterISOLanguageName = "zh") Then
			// Dim parentCultureInfo As System.Globalization.CultureInfo = cultureInfo.Parent
			// toolsMenuName = resourceManager.GetString(String.Concat(parentCultureInfo.Name, "Tools"))
			// Else
			// toolsMenuName = resourceManager.GetString(String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools"))
			// End If

			// Catch e As Exception
			// 'We tried to find a localized version of the word Tools, but one was not found.
			// ' Default to the en-US word, which may work for the current culture.
			// toolsMenuName = "Tools"
			// End Try

			// 'Place the command on the tools menu.
			// 'Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
			// Dim commandBars As CommandBars = CType(_DTE2.CommandBars, CommandBars)
			// Dim menuBarCommandBar As CommandBar = commandBars.Item("MenuBar")

			// 'Find the Tools command bar on the MenuBar command bar:
			// Dim toolsControl As CommandBarControl = menuBarCommandBar.Controls.Item(toolsMenuName)
			// Dim toolsPopup As CommandBarPopup = CType(toolsControl, CommandBarPopup)

			// Try
			// 'Add a command to the Commands collection:
			// Dim command As Command = commands.AddNamedCommand2(_addInInstance, "SSMSAddinTest3", "SSMSAddinTest3", "Executes the command for SSMSAddinTest3", True, 59, Nothing, CType(vsCommandStatus.vsCommandStatusSupported, Integer) + CType(vsCommandStatus.vsCommandStatusEnabled, Integer), vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton)

			// 'Find the appropriate command bar on the MenuBar command bar:
			// command.AddControl(toolsPopup.CommandBar, 1)
			// Catch argumentException As System.ArgumentException
			// 'If we are here, then the exception is probably because a command with that name
			// ' already exists. If so there is no need to recreate the command and we can
			// ' safely ignore the exception.
			// End Try

			//End If
			//EnvDTE.Events events = _DTE.Events;
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		void IDTExtensibility2.OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
		{
			/*
			//Checks whether the control in the tools menu is there if so delete the menu item.
			try
			{
				if ((_CommandBarControl != null))
				{
					_CommandBarControl.Delete(null); // upitno je što treba biti parametar ovdje
				}
			}
			catch
			{
				//If the delegate handlers have been connected, then disconnect them here.
				// This needs to be done, otherwise the handler may still fire since they
				// have not been garbage collected.
				//If _textDocumentKeyPressEvents IsNot Nothing Then
				// RemoveHandler _textDocumentKeyPressEvents.AfterKeyPress, AddressOf AfterKeyPress
				//End If
			}*/

			try
			{
				switch (RemoveMode)
				{
					case ext_DisconnectMode.ext_dm_HostShutdown:
					case ext_DisconnectMode.ext_dm_UserClosed:

						if ((myStandardCommandBarButton != null))
						{
							myStandardCommandBarButton.Delete(true);
						}

						if ((myCodeWindowCommandBarButton != null))
						{
							myCodeWindowCommandBarButton.Delete(true);
						}

						if ((myToolsCommandBarButton != null))
						{
							myToolsCommandBarButton.Delete(true);
						}

						if ((myTemporaryToolbar != null))
						{
							myTemporaryToolbar.Delete();
						}

						if ((myTemporaryCommandBarPopup1 != null))
						{
							myTemporaryCommandBarPopup1.Delete(true);
						}

						if ((myTemporaryCommandBarPopup2 != null))
						{
							myTemporaryCommandBarPopup2.Delete(true);
						}

						//Makni komandu, kako bi je opet mogao kreirati s možda nekom drugom ikonicom.
						//TODO: Nju bi po pravilima trebalo maknuti prilikom deinstalacije addina,
						//da se ne mora komanda svaki put kreirati jer oduzima vrijeme.
						//Tu je primjer kako: http://www.mztools.com/articles/2005/MZ2005002.aspx
						try
						{
							Command myCommand = _DTE.Commands.Item(_addInInstance.ProgID + "." + MY_COMMAND_NAME, -1);
							if (myCommand != null)
							{
								//myCommand.Bindings = null; //inace ce ti slova ostati vezana uz nepostojeću komandu
								myCommand.Delete();
							}
						} catch{}

						try
						{
							Command aboutCommand = _DTE.Commands.Item(_addInInstance.ProgID + "." + ABOUT_COMMAND_NAME, -1);
							if (aboutCommand != null)
							{
								aboutCommand.Delete();
							}
						}
						catch { }
						
						break;
				}
			}
			catch (System.Exception e)
			{
				System.Windows.Forms.MessageBox.Show(e.ToString());
			}

		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		void IDTExtensibility2.OnStartupComplete(ref Array custom)
		{
			AddTemporaryUI();

			/*
						//MessageBox.Show("bbb");

						Command myCommand = null;

						// 1. Check whether the command exists
						// try to retrieve the command, in case it was already created
						try
						{
							string commandFullName = _addInInstance.ProgID + "." + COMMAND_NAME;
							myCommand = _DTE.Commands.Item(commandFullName, 0); // ovu 0 sam dodao jer bez neke brojke kompajler baca grešku
						}
						catch
						{
						}
						// this just means the command wasn't found
						//Ako komanda već postoji, moze biti da nije dodana u menu (desilo mi se). Zato treba nastaviti.


						// 2. Create the command if necessary
						if (myCommand == null)
						{
							Array a = null;
							myCommand = _DTE.Commands.AddNamedCommand
								(_addInInstance, MY_COMMAND_NAME, "MySSMSAddin MenuItem", "Tooltip for your command",
									true, 0, ref a,
									(int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled)
								);
						}

						//Zakači poziv komande preko tipkovnice
						object[] bindings = (object[])myCommand.Bindings;
						if (bindings != null)
						{
							bindings = new object[1];
							//bindings(0) = DirectCast("Global::Ctrl+1, Ctrl+2", Object)
							bindings[0] = (object)"Global::Ctrl+Alt+M";

							myCommand.Bindings = (object)bindings;
						}

						// 3. Get the name of the tools menu (may not be called "Tools" if we're not in English
						string toolsMenuName = null;
						try
						{
							// If you would like to move the command to a different menu, change the word "Tools" to the
							// English version of the menu. This code will take the culture, append on the name of the menu
							// then add the command to that menu. You can find a list of all the top-level menus in the file
							// CommandBar.resx.
							System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("MySSMSAddin.CommandBar", System.Reflection.Assembly.GetExecutingAssembly());
							System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(_DTE2.LocaleID);
							toolsMenuName = resourceManager.GetString(string.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools"));
						}
						catch (Exception)
						{
							//We tried to find a localized version of the word Tools, but one was not found.
							// Default to the en-US word, which may work for the current culture.
							toolsMenuName = "Tools";
						}

						// 4. Get the Tools menu
						CommandBars commandBars = (CommandBars)_DTE.CommandBars;
						CommandBar toolsCommandBar = commandBars[toolsMenuName];

						// 5. Create the command bar control for the command
						try
						{
							//Find the appropriate command bar on the MenuBar command bar:
							_CommandBarControl = (CommandBarControl)myCommand.AddControl(toolsCommandBar, toolsCommandBar.Controls.Count + 1);
							_CommandBarControl.Caption = "MySSMSAddin";
						}
						catch (System.ArgumentException)
						{
						}
						//If we are here, then the exception is probably because a command with that name
						// already exists. If so there is no need to recreate the command and we can
						// safely ignore the exception.	
			*/

		}


		public void AddTemporaryUI()
		{
			// Constants for names of built-in commandbars of Visual Studio
			const string VS_STANDARD_COMMANDBAR_NAME = "Standard";
			const string VS_MENUBAR_COMMANDBAR_NAME = "MenuBar";
			const string VS_TOOLS_COMMANDBAR_NAME = "Tools";
			//const string VS_CODE_WINDOW_COMMANDBAR_NAME = "Code Window";

			// Constants for names of commandbars created by the add-in
			//const string MY_TEMPORARY_COMMANDBAR_POPUP1_NAME = "MyTemporaryCommandBarPopup1";
			const string MY_TEMPORARY_COMMANDBAR_POPUP2_NAME = "MyTemporaryCommandBarPopup2";

			// Constants for captions of commandbars created by the add-in
			//const string MY_TEMPORARY_COMMANDBAR_POPUP1_CAPTION = "SQL XDetails"; // "My sub menu"
			const string MY_TEMPORARY_COMMANDBAR_POPUP2_CAPTION = "SQL XDetails"; // "My main menu"
			//const string MY_TEMPORARY_TOOLBAR_CAPTION = "SQL XDetails"; // "My toolbar"


			// Buttons that will be created on a toolbars/commandbar popups created by the add-in
			// We don't need to keep them at class level to remove them when the add-in is unloaded 
			// because we will remove the whole toolbars/commandbar popups
			//CommandBarButton myToolBarButton = null;
			//CommandBarButton myCommandBarPopup1Button = null;
			

			// Other variables
			CommandBarControl toolsCommandBarControl;
			//Array contextUIGuids = Array.CreateInstance(typeof(Object), 5);
			object[] contextUIGuids = new object[] { };
			object[] contextUIGuids2 = new object[] { };
			try
			{
				// ------------------------------------------------------------------------------------
				// The only command that will be created. We will create several buttons from it
				Command myCommand = null;
				// Try to retrieve the command, just in case it was already created, ignoring the 
				// exception that would happen if the command was not created yet.
				try
				{	
					myCommand = _DTE.Commands.Item(_addInInstance.ProgID + "." + MY_COMMAND_NAME, -1);
				}	catch{}

				Command aboutCommand = null;
				try
				{
					aboutCommand = _DTE.Commands.Item(_addInInstance.ProgID + "." + ABOUT_COMMAND_NAME, -1);
				}
				catch { }

				//EnvDTE.Commands commands = (EnvDTE.Commands)_DTE.Commands;
				// Add the command if it does not exist

				//Ovo će ti biti samo jednom pri prvoj instalaciji na računalo.
				//Nakon toga je komanda upisana u registry 
				//i svaki puta će ti preskakati ovaj dio kooda jer komanda već postoji.
				if (myCommand == null)
				{
					//Ovdje je popis ID-jeva svih ikona raspoloživih u Microsoft.VisualStudio.Commandbars.dll-u:
					// http://www.kebabshopblues.co.uk/2007/01/04/visual-studio-2005-tools-for-office-commandbarbutton-faceid-property/
					// Zanimljiviji ID-jevi:
					//8
					//25
					//351
					//487 - i
					//1000
					//1089 - ?
					//1100
					//1215 - baza
					//1382 - lampa
					//1714
					//1954 - i

					//myCommand = _DTE.Commands.AddNamedCommand
					//(_addInInstance, MY_COMMAND_NAME, MY_COMMAND_CAPTION, MY_COMMAND_TOOLTIP,
					//    true, 59, ref contextUIGuids,
					//    (int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled)
					//);

					myCommand = _DTE.Commands.AddNamedCommand
					(_addInInstance, MY_COMMAND_NAME, MY_COMMAND_CAPTION, MY_COMMAND_TOOLTIP,
						true, 1954, ref contextUIGuids,
						(int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled)
					);

					//myCommand = _DTE.Commands.AddNamedCommand
					//(_addInInstance, MY_COMMAND_NAME, MY_COMMAND_CAPTION, MY_COMMAND_TOOLTIP,
					//    false, 1, ref contextUIGuids,
					//    (int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled)
					//);

					//myCommand = commands.AddNamedCommand2
					//(_addInInstance, MY_COMMAND_NAME, MY_COMMAND_CAPTION, MY_COMMAND_TOOLTIP,
					//    false, 1, ref contextUIGuids,
					//    (int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled),
					//    (int)vsCommandStyle.vsCommandStylePictAndText,
					//    vsCommandControlType.vsCommandControlTypeButton
					//);

					//Zakači poziv komande preko tipkovnice
					object[] bindings = (object[])myCommand.Bindings;
					if (bindings != null)
					{
						bindings = new object[1];
						//bindings(0) = DirectCast("Global::Ctrl+1, Ctrl+2", Object)
						//bindings[0] = (object)"Global::Ctrl+Alt+M";
						bindings[0] = (object)"Global::Alt+1";

						myCommand.Bindings = (object)bindings;
					}
				}


				if( aboutCommand == null )
				{
					aboutCommand = _DTE.Commands.AddNamedCommand
					(_addInInstance, ABOUT_COMMAND_NAME, ABOUT_COMMAND_CAPTION, ABOUT_COMMAND_TOOLTIP,
						true, 1089, ref contextUIGuids2,
						(int)(vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled)
					);
				}

				// ------------------------------------------------------------------------------------

				// Retrieve the collection of commandbars
				// Note:
				// - In VS.NET 2002/2003 (which uses the Office.dll reference) 
				//   DTE.CommandBars returns directly a CommandBars type, so a cast 
				//   to CommandBars is redundant
				// - In VS 2005 or higher (which uses the new Microsoft.VisualStudio.CommandBars.dll reference) 
				//   DTE.CommandBars returns an Object type, so we do need a cast to CommandBars
				// The collection of Visual Studio commandbars
				CommandBars commandBars = (CommandBars)_DTE.CommandBars;

				// Built-in commandbars of Visual Studio
				// Retrieve some built-in commandbars
				CommandBar standardCommandBar = commandBars[VS_STANDARD_COMMANDBAR_NAME];
				CommandBar menuCommandBar = commandBars[VS_MENUBAR_COMMANDBAR_NAME];
				CommandBar toolsCommandBar = commandBars[VS_TOOLS_COMMANDBAR_NAME];
				//CommandBar codeCommandBar = commandBars[VS_CODE_WINDOW_COMMANDBAR_NAME];

				// ------------------------------------------------------------------------------------

				// Create the buttons from the commands
				// Note:
				// - In VS.NET 2002/2003 (which uses the Office.dll reference) 
				//   Command.AddControl returns directly a CommandBarControl type, so a cast 
				//   to CommandBarControl is redundant
				// - In VS 2005 or higher (which uses the new Microsoft.VisualStudio.CommandBars.dll reference) 
				//   Command.AddControl returns an Object type, so we do need a cast to CommandBarControl

				// ------------------------------------------------------------------------------------
				// Button on the "Standard" toolbar
				// ------------------------------------------------------------------------------------
				myStandardCommandBarButton = (CommandBarButton)myCommand.AddControl(standardCommandBar,
				   standardCommandBar.Controls.Count + 1);

				// Change some button properties
				myStandardCommandBarButton.Caption = MY_COMMAND_CAPTION;
				myStandardCommandBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption; // It could be also msoButtonIconAndCaption, or msoButtonIcon
				myStandardCommandBarButton.BeginGroup = true; // Separator line above button
				myStandardCommandBarButton.TooltipText = MY_COMMAND_TOOLTIP;


				//// ------------------------------------------------------------------------------------
				//// Button on the "Tools" menu
				//// ------------------------------------------------------------------------------------

				//// Add a button to the built-in "Tools" menu
				//myToolsCommandBarButton = (CommandBarButton)myCommand.AddControl(toolsCommandBar,
				//   toolsCommandBar.Controls.Count + 1);

				//// Change some button properties
				//myToolsCommandBarButton.Caption = MY_COMMAND_CAPTION;
				//myToolsCommandBarButton.BeginGroup = true; // Separator line above button

				//// ------------------------------------------------------------------------------------
				//// Button on the "Code Window" context menu
				//// ------------------------------------------------------------------------------------

				//// Add a button to the built-in "Code Window" context menu
				//myCodeWindowCommandBarButton = (CommandBarButton)myCommand.AddControl(codeCommandBar,
				//   codeCommandBar.Controls.Count + 1);

				//// Change some button properties
				//myCodeWindowCommandBarButton.Caption = MY_COMMAND_CAPTION;
				//myCodeWindowCommandBarButton.BeginGroup = true; // Separator line above button

				//// ------------------------------------------------------------------------------------
				//// New toolbar
				//// ------------------------------------------------------------------------------------

				//// Add a new toolbar 
				//myTemporaryToolbar = commandBars.Add(MY_TEMPORARY_TOOLBAR_CAPTION,
				//   MsoBarPosition.msoBarTop, System.Type.Missing, true);

				//// Add a new button on that toolbar
				//myToolBarButton = (CommandBarButton)myCommand.AddControl(myTemporaryToolbar,
				//   myTemporaryToolbar.Controls.Count + 1);

				//// Change some button properties
				//myToolBarButton.Caption = MY_COMMAND_CAPTION;
				//myToolBarButton.Style = MsoButtonStyle.msoButtonIconAndCaption; // It could be also msoButtonIcon
				//myToolBarButton.TooltipText = MY_COMMAND_TOOLTIP;

				//// Make visible the toolbar
				//myTemporaryToolbar.Visible = true;

				//// ------------------------------------------------------------------------------------
				//// New submenu under the "Tools" menu
				//// ------------------------------------------------------------------------------------

				//// Add a new commandbar popup 
				//myTemporaryCommandBarPopup1 = (CommandBarPopup)toolsCommandBar.Controls.Add(
				//   MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing,
				//   toolsCommandBar.Controls.Count + 1, true);

				//// Change some commandbar popup properties
				//myTemporaryCommandBarPopup1.CommandBar.Name = MY_TEMPORARY_COMMANDBAR_POPUP1_NAME;
				//myTemporaryCommandBarPopup1.Caption = MY_TEMPORARY_COMMANDBAR_POPUP1_CAPTION;

				//// Add a new button on that commandbar popup
				//myCommandBarPopup1Button = (CommandBarButton)myCommand.AddControl(
				//   myTemporaryCommandBarPopup1.CommandBar, myTemporaryCommandBarPopup1.Controls.Count + 1);

				//// Change some button properties
				//myCommandBarPopup1Button.Caption = MY_COMMAND_CAPTION;

				//// Make visible the commandbar popup
				//myTemporaryCommandBarPopup1.Visible = true;

				// ------------------------------------------------------------------------------------
				// New main menu
				// ------------------------------------------------------------------------------------

				// Calculate the position of a new commandbar popup to the right of the "Tools" menu
				toolsCommandBarControl = (CommandBarControl)toolsCommandBar.Parent;
				int position = toolsCommandBarControl.Index + 1;

				// Add a new commandbar popup 
				myTemporaryCommandBarPopup2 = (CommandBarPopup)menuCommandBar.Controls.Add(
				   MsoControlType.msoControlPopup, System.Type.Missing, System.Type.Missing, position, true);

				// Change some commandbar popup properties
				myTemporaryCommandBarPopup2.CommandBar.Name = MY_TEMPORARY_COMMANDBAR_POPUP2_NAME;
				myTemporaryCommandBarPopup2.Caption = MY_TEMPORARY_COMMANDBAR_POPUP2_CAPTION;

				// Add a new button on that commandbar popup
				CommandBarButton myCommandBarPopup2Button = (CommandBarButton)myCommand.AddControl
				(myTemporaryCommandBarPopup2.CommandBar,
					myTemporaryCommandBarPopup2.Controls.Count + 1
				);
				// Change some button properties
				myCommandBarPopup2Button.Caption = MY_COMMAND_CAPTION;
				myCommandBarPopup2Button.TooltipText = MY_COMMAND_TOOLTIP;

				// Add a new button on that commandbar popup
				CommandBarButton aboutCommandBarPopup2Button = (CommandBarButton)aboutCommand.AddControl
				(myTemporaryCommandBarPopup2.CommandBar,
					myTemporaryCommandBarPopup2.Controls.Count + 1
				);
				// Change some button properties
				aboutCommandBarPopup2Button.Caption = ABOUT_COMMAND_CAPTION;
				aboutCommandBarPopup2Button.TooltipText = ABOUT_COMMAND_TOOLTIP;

				// Make visible the commandbar popup
				myTemporaryCommandBarPopup2.Visible = true;
			}
			catch (System.Exception e)
			{
				System.Windows.Forms.MessageBox.Show(e.ToString());
			}
		}


		#endregion

		#region IDTCommandTarget Members

		void IDTCommandTarget.Exec(string cmdName, vsCommandExecOption executeOption, ref object variantIn, ref object variantOut, ref bool handled)
		{
			//handled = False
			//If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
			// If commandName = "SSMSAddinTest3.Connect.SSMSAddinTest3" Then
			// handled = True
			// Exit Sub
			// End If
			//End If

			handled = false;
			if (executeOption != vsCommandExecOption.vsCommandExecOptionDoDefault) return;

			if (cmdName == _addInInstance.ProgID + "." + MY_COMMAND_NAME) // e.g. If(commandName == "SSMSAddinTest3.Connect.SSMSAddinTest3")
			{
				handled = true;

				////Expiration by date
				//if (System.DateTime.Today >= _expirationDate)
				//{
				//    AboutBox ab = new AboutBox();
				//    ab.labelExpired.Visible = true;
				//    ab.ShowDialog();
				//    return;
				//} 

				//Sada za probu ovdje pokusaj naci aktivan sql text editor prozor, i riječ u kojoj je kursor
				string sWordAtCursor = GetWordAtCursor();
				Debug.WriteLine(string.Format("Riječ: >{0}<", sWordAtCursor));
				if (string.IsNullOrEmpty(sWordAtCursor)) return;
				//izadji van ako nije nista odabrano

				// get windows2 interface.
				//Windows2 interface je bolji od Windows, jer Windows baca grešku kad drugi put pokušaš kreirati prozor (prvi put uspije).
				EnvDTE80.Windows2 windowManager2 = (EnvDTE80.Windows2)_DTE.Windows;
				//EnvDTE.Windows windowManager = (EnvDTE.Windows)_DTE.Windows;

				// create the window
				object MyControl = null;
				ObjectInfoControl w = null;

				Window toolWindow = null;
				//Provjeri postoji li već prozor
				//try
				//{	// ako ne postoji, baca neki exception koji namespace tu nije bitan
				//   toolWindow = windowManager2.Item(MY_ADDIN_NAME);
				//}
				//catch { }
				foreach (Window ww in windowManager2)
				{
					if (ww.ObjectKind == XDETAILS_WINDOW_KIND_ID) // ObjectKind je GUID svijek sa svim UPPER slovima!
					{
						toolWindow = ww;
						w = toolWindow.Object as ObjectInfoControl;
						break;
					}
				}

				if (toolWindow == null)
				{
					//Pazi! Tu moraš staviti točno ime klase prozora (namespace.klasa, mislim). Npr: SSMSAddinTest3.MyAddinWindow
					//Ovaj CLSID nemam pojma od kuda mu (nema ga u registryju niti listi komandi), ali s njim radi. Možda je to bilo koji unique CLSID (neki novi).
					// SQL Orcchy: sql server database Object seaRCH and info addin
					System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly(); // get current assembly
					string asmLocation = asm.Location;
					string sFullName = typeof(ObjectInfoControl).FullName;
					//MessageBox.Show(string.Format("asm Location: {0}, FullName: {1}", asmLocation, sFullName)); // debug


					//Window toolWindow = windowManager2.CreateToolWindow2
					//    (_addInInstance, asmLocation, sFullName,
					//        sWordAtCursor, "{5B7F8C1C-65B9-2aca-1Ac3-12AcBbAF21d5}", ref MyControl
					//    );

					//Ovo kreira ObjectInfoControl objekt i pokrece ObjectInfoControl_Load(), bar na sql2012
					// i zatim sprema instancu u MyControl
					//Problem je sto u toj _Load proceduri vec mora imati saznanje o sql konekciji, a ne možeš mu to ovdje dojaviti nekim parametrom.
					//U verzijama 2008R2 i starijima tu kreira objekt ali NE POKREĆE ObjectInfoControl_Load(),
					// pa nemaš problem što kreirani objekt još ne zna db konekciju.
					toolWindow = windowManager2.CreateToolWindow2
						(_addInInstance, asmLocation, sFullName,
						 sWordAtCursor+" - "+MY_ADDIN_NAME,
						 XDETAILS_WINDOW_KIND_ID, ref MyControl //SPREMA kreirani objekt u MyControl!
						);

					////Kreiraj novi prozor samo ako je potrebno
					//if (toolWindow == null)
					//{
					//    toolWindow = windowManager.CreateToolWindow
					//        (_addInInstance, typeof(DbDetector.ObjectInfoControl).FullName,
					//            MY_ADDIN_NAME, "{6D84A0E7-B090-4E47-8B1D-F84799989524}", ref MyControl
					//        );
					//}
					w = (ObjectInfoControl)MyControl;

				}

				//Dobio si instancu svoje klase. Ona postoji i iscrtava se unutar addin-ovog prozora.
				//Mozes je dobiti i iz "toolWindow.Object", takodjer trebas castati.
				//TODO: Sada dojavi nekako tom prozoru što da prikaže
				//Npr. postavi mu neku varijablu sa Set operatorom ili prozoveš neku njegovu proceduru.
				//Ili naprednije, pošalješ mu message, tako da se izvodi u procesu prozora, kako i treba.

				//string sConnectionString;
				Microsoft.SqlServer.Management.Common.SqlConnectionInfo sqlConnInfo;
				try
				{
					//sConnectionString = UtilitySqlTools.Current.GetActiveWindowConnectionString();
					sqlConnInfo = UtilitySqlTools.Current.GetActiveWindowConnectionInfo();
				}
				catch (Exception e)
				{
					MessageBox.Show("Exec: " + e.Message);
					return;
				}
				//w.ConnectionString = sConnectionString;
				w.SqlConnInfo = sqlConnInfo;
				w.DisplayObjectByName(sWordAtCursor);
				//w.ObjectNameToSearchFor = sWordAtCursor;
				//ovo će opaliti neki event. Load sigurno opaljuje nakon ovoga (iskustveno).
				toolWindow.Visible = true;


			//For Each doc As Document In _DTE.Documents
			// Debug.WriteLine(doc.Name)
			//Next


			//'Pronadji konekciju
			//'Dim scriptFactory As Editors.IScriptFactory = ServiceCache.ScriptFactory
			//'scriptFactory.CurrentlyActiveWndConnectionInfo.UIConnectionInfo.ToString()
			//'-----------------------TODO: Tu nastavi s konekcijom!!! Trebaju ti dev2dev upute!
			//Dim sb As New StringBuilder()
			//sb.AppendLine("print '123'")
			//Dim conn As SqlConnection = Nothing 'postavljam na nothing da mi compiler ne baca upozorenja da nisam postavio varijablu
			//Dim command As SqlCommand = Nothing
			//Try
			// conn = UtilitySqlTools.Current.GetOpenedConnection()
			// command = New SqlCommand(sb.ToString(), conn)
			// command.CommandTimeout = 1000
			// command.CommandType = CommandType.Text
			// command.ExecuteNonQuery()
			// Debug.WriteLine("Uspio!")
			//Catch exception As Exception
			// Debug.WriteLine("Greška!")
			//Finally
			// If command IsNot Nothing Then command.Dispose()
			// If conn IsNot Nothing Then
			// conn.Close() 'zatvaranje konekcije koja nije otvorena nece proizvesti gresku. Close() mozes prozivati koliko god hoces puta.
			// conn.Dispose()
			// End If
			//End Try
			// commandName = _addInInstance.ProgID & "." & COMMAND_NAME

			// executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault
			}
			else if (cmdName == _addInInstance.ProgID + "." + ABOUT_COMMAND_NAME)
			{
				handled = true;
				AboutBox ab = new AboutBox();
				ab.ShowDialog();
			}

		} // Exec

		void IDTCommandTarget.QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
		{
			//called when the command's availability is updated
			if (NeededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if
				(	CmdName == _addInInstance.ProgID + "." + ABOUT_COMMAND_NAME 
					||
					CmdName == _addInInstance.ProgID + "." + MY_COMMAND_NAME //e.g. "SSMSAddinTest3.Connect.SSMSAddinTest3"
					&& _DTE.ActiveWindow.Type == vsWindowType.vsWindowTypeDocument // Gumb omogući samo ako si unutar sql editor prozora
				)
				{
					//status = enabled + supported. Bez "supported" ti se komanda neće uopće ni prikazati na meniju.
					StatusOption = (vsCommandStatus)(vsCommandStatus.vsCommandStatusEnabled | vsCommandStatus.vsCommandStatusSupported);
				}
				else
				{
					//status = unsupported
					StatusOption = vsCommandStatus.vsCommandStatusUnsupported;
				}
			}
		}

		#endregion

		#region "Helpers"

		/// <summary>
		/// Finds active sql editor window, and gets selected text or word (possibly db object name) at the text cursor location.
		/// </summary>
		/// <returns>Returns selected text or word at the cursor of active sql edir widow.</returns>
		/// <remarks>If there is no active sql editor, or no word at cursor, returns empty string. Never returns null.</remarks>
		private string GetWordAtCursor()
		{
			string sWordAtCursor = string.Empty;
			TextSelection sel = (TextSelection)_DTE.ActiveDocument.Selection;
			Debug.WriteLine(string.Format("Line: {0}, Col: {1}, Text: {2}", sel.CurrentLine, sel.CurrentColumn, sel.Text));

			//Ako je korisnik nešto odabrao, onda točno to i tražiš.
			if (sel.Text != string.Empty) return sel.Text;

			EditPoint ep = ((TextPoint)sel.ActivePoint).CreateEditPoint();
			string lineText = ep.GetLines(ep.Line, ep.Line + 1);
			//linija teksta u kojoj je kursor
			Debug.WriteLine(string.Format("Linija u kojoj je kursor: {0}", lineText));

			//Pronadji rijec u kojoj je kursor, naravno, ukoliko je kursor uopce u nekoj rijeci.
			//Jer, kursor moze biti usred niza razmaka - tada nema rijeci i ne vracaj nista.
			//Moze biti na kraju linije, na pocetku linije - pitanje jel ta linija uopce ima printabilnih znakova.
			//Moze biti na kraju rijeci, na pocetku rijeci.
			//Moze cak biti u rijeci koja je omedjena zagradama [], i tada su zagrade granice.
			if (lineText.Length >= 1) // isplati se traziti rijec ako je linija dugacka barem jedan znak
			{
				//Debug.WriteLine(String.Format("Slovo lijevo: '{0}' i desno: '{1}' od kursora.", lineText.Substring(sel.CurrentColumn - 2, 1), lineText.Substring(sel.CurrentColumn - 1, 1)))
				int pos = sel.CurrentColumn - 1; //pozicija slova desno od kursora, 0-based
				pos = ep.LineCharOffset - 1; //pozicija slova desno od kursora, 0-based
				sWordAtCursor = UtilitySqlTools.Current.GetObjectNameAtPosition(lineText, pos);
			}
			//Ako su zagrade lijevo i desno od kursora
			//Dim otvorenaZagrada As Integer = lineText.Substring(0, pos).LastIndexOf("[")
			//If lineText.IndexOf("[") > -1 Then

			//End If

			// lineText.Length >= 2

			return sWordAtCursor;
		}

		#endregion

		/// <summary>
		/// Handles any thread exceptions
		/// </summary>
		public class ThreadExceptionHandler
		{
			public void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
			{
				MessageBox.Show("Please report this error: " + e.Exception.Message, Connect.MY_ADDIN_NAME + ": exception occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

}