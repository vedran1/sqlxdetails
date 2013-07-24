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

/////////////////////////////////////////////////
// Neke metode moraju RAZLIČITO izgledati na različitim verzijama SSMS-a.
// Riješio sam to parcijalnom klasom. Takvu metodu izdvojio sam u zasebni cs file, iako pripada istoj klasi kao i prije.
// Projekti za specifičnu verziju SSMS-a koriste specifični file, a ostali koriste generički file sa generičkom verzijom te iste metode.
namespace XDetails
{
	/// <summary>
	/// Singleton klasa, kreira se samo jednom po jednom pokretanju aplikacije (SSMS) i svima je dostupna.
	/// </summary>
	/// <remarks></remarks>
	public partial class UtilitySqlTools
	{
		private INodeInformation[] GetObjectExplorerSelectedNodes()
		{
			//Ovo radi svugdje osim na Sql2012
			Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.IObjectExplorerService objExplorer = Microsoft.SqlServer.Management.UI.VSIntegration.ServiceCache.GetObjectExplorer();

			//Za SQL2012
			//Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer.IObjectExplorerService objExplorer = (IObjectExplorerService)Microsoft.SqlServer.Management.UI.VSIntegration.ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));

			//Ovo ne radi nigdje, jer ne znam u kojem dll-u je definiran ObjectExplorerService - njega kad nadjes referenciraj i trebalo bi raditi.
			//ObjectExplorerService objExplorerService = (ObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService)); 

			int arraySize = 0;
			INodeInformation[] nodes = null;
			objExplorer.GetSelectedNodes(out arraySize, out nodes);
			return nodes;
		}

	}
}