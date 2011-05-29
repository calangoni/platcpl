﻿/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 13/03/2011
 * Hora: 10:45
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;

namespace PlatCPL
{
	/// <summary>
	/// Description of ApplicationManager.
	/// </summary>
	public class PlatCPLv4
	{
		public System.Collections.Generic.List<string> apps;
		private System.Collections.Generic.List<PcAppHandler> openWindows;
		public LogManager log;
		public PlatCPLDatabaseManager database;
		public System.Windows.Forms.OpenFileDialog fileOpenDialog;
		public System.Windows.Forms.SaveFileDialog fileSaveDialog;
		public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		//public string programFolder;
		public string programUserFolder;
		public string tempFolder;
		public string xmlDatabaseFile;
		
		public PlatCPLv4()
		{
			//Initialize the variables
			openWindows = new System.Collections.Generic.List<PcAppHandler>();
			fileOpenDialog = new System.Windows.Forms.OpenFileDialog();
			fileSaveDialog = new System.Windows.Forms.SaveFileDialog();
			folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			//Define default folders and files
			findSystemFilesAndFolders();
			//Prepare log screen
			log = new LogManager();
			//Prepare platform database
			database = new PlatCPLDatabaseManager(xmlDatabaseFile);
			//Find all applications
			apps = PlatCPL.ThirdParty.GetAllClasses(C.S.InterfacesNamespace);
			foreach(string interfaceClass in apps) log.showMessage(E.MsgType.Info, "App: "+interfaceClass);
		}
		
		public PcAppHandler getApplicationInstance(string appName)
		{
			if( appName!=null && appName.Length>0 )
			{
				for( int i=0; i<openWindows.Count; i++)
				{
					if(openWindows[i].GetType().Name == appName)
					{
						// There is already an instance of this app
						return openWindows[i];
					}
				}
				// This app was note yet instanced, try to do it
				string appFullName = C.S.InterfacesNamespace + "." + appName;
				PcAppHandler appControl = null;
				try
				{
					System.Type appType = System.Type.GetType( appFullName );
					appControl = (PlatCPL.PcAppHandler) System.Activator.CreateInstance(appType);
				}
				catch(Exception) { appControl = null; }
				if( appControl != null )
				{
					InterCommunication appComm = new InterCommunication( this, openWindows.Count, appName );
					appControl.commParent = appComm;
					appControl.Dock = System.Windows.Forms.DockStyle.Fill;
					appControl.MinimumSize = appControl.Size;
					openWindows.Add( appControl );
					return appControl;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
		public bool isAppOpen(string appName)
		{
			if( appName!=null && appName.Length>0 )
			{
				for( int i=0; i<openWindows.Count; i++)
				{
					if(openWindows[i].GetType().Name == appName)
					{
						// There is already an instance of this app
						return true;
					}
				}
			}
			return false;
		}
		public void findSystemFilesAndFolders()
		{
			System.Reflection.Assembly assemb = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.FileInfo program = new System.IO.FileInfo(assemb.Location);
			string programFolder = program.Directory.FullName;
			programUserFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "\\PlatCPL";
			if(!System.IO.Directory.Exists(programUserFolder)) programUserFolder=programFolder;
			else if(System.IO.File.Exists(programFolder+"\\Base4.xml")) programUserFolder=programFolder;
			xmlDatabaseFile = programUserFolder+"\\Base4.xml";
			tempFolder = System.IO.Path.GetTempPath();
		}
	}
	public class LogManager
	{
		public void showMessage(E.MsgType type, string message)
		{
			if(type == E.MsgType.Error) System.Diagnostics.Trace.WriteLine("ERROR: "+message);
			else if(type == E.MsgType.Warn) System.Diagnostics.Trace.WriteLine("WARNING: "+message);
			else if(type == E.MsgType.Info) System.Diagnostics.Trace.WriteLine(message);
			else System.Diagnostics.Trace.WriteLine(message);
		}
	}
	public class PlatCPLDatabaseManager
	{
		private string XMLfileName;
		private XML3 XMLdatabase;

		public PlatCPLDatabaseManager(string XMLfilePath)
		{
			XMLfileName = "";
			XMLdatabase = new XML3(C.S.Database4Name);
			if( XMLfilePath!=null && XMLfilePath.Length>0 )
			{
				try
				{
					System.IO.FileInfo dbFileInfo = new System.IO.FileInfo(XMLfilePath);
					if(dbFileInfo.Exists)XMLdatabase.LoadXmlFile(dbFileInfo.FullName);
					if( XMLdatabase.Tag==C.S.Database4Name && XMLdatabase.Content=="" )
					{
						XMLfileName = XMLfilePath;
					}
					else
					{
						XMLfileName = "";
						XMLdatabase = new XML3(C.S.Database4Name);
					}
				}
				catch(Exception)
				{
					XMLfileName = "";
					XMLdatabase = new XML3(C.S.Database4Name);
				}
			}
		}

		public bool saveString(string[] databasePath, string content)
		{
			if(databasePath.Length<2)return false;
			if(databasePath[0] != C.S.Database4Name)return false;
			XML3 currentChild = XMLdatabase;
			XML3 lastChild = XMLdatabase;
			for(int i=1; i<databasePath.Length; i++)
			{
				lastChild = currentChild;
				currentChild = lastChild.FindChild(databasePath[i]);
				if(currentChild==null)
				{
					currentChild = lastChild.NewChild(databasePath[i], "");
				}
			}
			currentChild.Content = content;
			if(XMLfileName.Length>0)XMLdatabase.SaveXml(XMLfileName);
			return true;
		}
		public string loadString(string[] databasePath)
		{
			if(databasePath.Length<2)return null;
			if(databasePath[0] != C.S.Database4Name)return null;
			XML3 currentChild = XMLdatabase;
			for(int i=1; i<databasePath.Length; i++)
			{
				currentChild = currentChild.FindChild(databasePath[i]);
				if(currentChild==null) return null;
			}
			return currentChild.Content;
		}
	}
	public class InterCommunication
	{
		private PlatCPL.PlatCPLv4 platform;
		private int appWindowId;
		private string appIdent;

		public InterCommunication(PlatCPLv4 platv4, int windowId, string appName)
		{
			platform = platv4;
			appWindowId = windowId;
			appIdent = appName;
		}
		
		public void showMessage(E.MsgType type, string message)
		{
			platform.log.showMessage(type, message);
		}
		public string loadSystemConstant(string constantIdentifier)
		{
			if( constantIdentifier == C.M.ProgramUserFolder ) return platform.programUserFolder;
			else
			{
				platform.log.showMessage(E.MsgType.Error, "[12] Invalid constant identifier: "+constantIdentifier);
				return null;
			}
		}
		public string loadLocalString(string stringIdentifier)
		{
			return platform.database.loadString(new string[] { C.S.Database4Name, appIdent, stringIdentifier });
		}
		public bool saveLocalString(string stringIdentifier, string content)
		{
			return platform.database.saveString(new string[] { C.S.Database4Name, appIdent, stringIdentifier }, content);
		}

		public string[] selectFile(string defaultFolderId, string filter, bool allowMultiple, string title)
		{
			platform.fileOpenDialog.FileName = "";
			platform.fileOpenDialog.Multiselect = allowMultiple;
			if(filter.Length>0 && (!filter.Contains("|")))filter = filter+"|"+filter;
			platform.fileOpenDialog.Filter = filter;
			platform.fileOpenDialog.Title = title;
			platform.fileOpenDialog.InitialDirectory = "";
			if(defaultFolderId==null || defaultFolderId.Length>0)defaultFolderId = C.S.DefaultFolderTag;
			string defaultFolder = platform.database.loadString(new string[] { C.S.Database4Name, appIdent, C.S.DefaultFoldersTag, defaultFolderId });
			if(defaultFolder!=null) platform.fileOpenDialog.InitialDirectory = defaultFolder;
			if(platform.fileOpenDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				platform.database.saveString(
					new string[] { C.S.Database4Name, appIdent, C.S.DefaultFoldersTag, defaultFolderId },
					System.IO.Path.GetDirectoryName(platform.fileOpenDialog.FileName) );
				return platform.fileOpenDialog.FileNames;
			}
			else return new string[0];
		}
		public string selectFolder(string defaultFolderId, string forcedInitialFolder, string title)
		{
			if(defaultFolderId==null || defaultFolderId.Length>0)defaultFolderId = C.S.DefaultFolderTag;
			if(forcedInitialFolder!=null && forcedInitialFolder.Length>0) platform.folderBrowserDialog.SelectedPath = forcedInitialFolder;
			else
			{
				platform.folderBrowserDialog.SelectedPath = "";
				string defaultFolder = platform.database.loadString(new string[] { C.S.Database4Name, appIdent, C.S.DefaultFoldersTag, defaultFolderId });
				if(defaultFolder!=null) platform.folderBrowserDialog.SelectedPath = defaultFolder;
			}
			if(platform.folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				platform.database.saveString(
					new string[] { C.S.Database4Name, appIdent, C.S.DefaultFoldersTag, defaultFolderId },
					platform.folderBrowserDialog.SelectedPath );
				return platform.folderBrowserDialog.SelectedPath;
			}
			else return null;
		}
		public string createFile(string defaultFolderId, string extension)
		{
			string filter = extension;
			if(extension==null || extension.Length==0)filter="*.*";
			else if(extension.StartsWith("."))filter="*"+extension;
			else if(!extension.StartsWith("*."))filter="*."+extension;
			filter = "("+filter+")|"+filter;
			platform.fileSaveDialog.InitialDirectory = "";
			platform.fileSaveDialog.FileName = "";
			platform.fileSaveDialog.Filter = filter;
			if(defaultFolderId==null || defaultFolderId.Length>0)defaultFolderId = C.S.DefaultFolderTag;
			string defaultFolder = platform.database.loadString(new string[] { C.S.Database4Name, appIdent, C.S.DefaultFoldersTag, defaultFolderId });
			if(defaultFolder!=null) platform.fileSaveDialog.InitialDirectory = defaultFolder;
			if(platform.fileSaveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				platform.database.saveString(
					new string[] { C.S.Database4Name, appIdent, C.S.DefaultFoldersTag, defaultFolderId },
					System.IO.Path.GetDirectoryName(platform.fileOpenDialog.FileName) );
				return platform.fileSaveDialog.FileName;
			}
			return null;
		}
		
		public object command(string message, object[] parameters)
		{
			platform.log.showMessage(E.MsgType.Error, "[28] not implemented!");
			return null;
		}
	}
}
