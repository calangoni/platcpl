/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 13/03/2011
 * Hora: 10:57
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;

namespace PlatCPL
{
	/// <summary>
	/// Description of Constants.
	/// </summary>
	public static class C
	{
		public static class S //System constants
		{
			public static string InterfacesNamespace = "PlatCPL.Interfaces";
			public static string Database4Name = "PlatCPLv4";
			public static string DefaultFoldersTag = "DefaultFolder";
			public static string DefaultFolderTag = "default";
		}
		public static class M //Intercommunication messages
		{
			/*public static string ErrorMsg = "E";
			public static string WarnMsg = "W";
			public static string InfoMsg = "I";
			public static string AddApp = "ADDAPP";
			public static string LoadLocalString = "LoadLocalString";
			public static string SaveLocalString = "SaveLocalString";
			public static string LoadGlobalString = "LoadGlobalString";
			public static string LoadSystemConstant = "LoadSystemConstant";
			public static string SelectFolder = "SelectFolder";
			public static string SelectFile = "SelectFile";
			public static string SelectFiles = "SelectFiles";
			public static string CreateFile = "CreateFile";*/

			public static string TempFolder = "TempFolder";
			public static string ProgramUserFolder = "ProgramUserFolder";
		}
		public static class PB //PhoneBook constants
		{
			public static string OtherFields = "OtherFields";
		}
	}
	public static class E
	{
		public enum MsgType
		{
			Error,
			Info,
			Warn
		}
	}
}
