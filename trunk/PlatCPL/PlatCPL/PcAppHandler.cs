/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 13/03/2011
 * Hora: 10:43
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;

namespace PlatCPL
{
	/// <summary>
	/// Description of InterCommunication.
	/// </summary>
	
	public class PcAppHandler : System.Windows.Forms.UserControl //PlatCPL application handler
	{
		public PlatCPL.InterCommunication commParent;
		public string applicationShowName;

		public PcAppHandler()
		{
			commParent = null;
			applicationShowName = this.GetType().Name;
		}
		public void setComments(string appShowName)
		{
			if(appShowName!=null)applicationShowName = appShowName;
		}
		
		public object command(string message, object parameters)
		{
			return null;
		}
		public void msgError(string message)
		{
			commParent.showMessage(E.MsgType.Error, message);
		}
		public void msgWarn(string message)
		{
			commParent.showMessage(E.MsgType.Warn, message);
		}
		public void msgInfo(string message)
		{
			commParent.showMessage(E.MsgType.Info, message);
		}
		public void msgDebug(string message)
		{
			commParent.showMessage(E.MsgType.Debug, message);
		}
		public string loadLocalStringNN(string stringIdentifier) // Load application string not null
		{
			string result = commParent.loadLocalString(stringIdentifier);
			if(result==null)return "";
			return result;
		}
		public bool saveLocalString(string stringIdentifier, string content)
		{
			return commParent.saveLocalString(stringIdentifier, content);
		}
		public string loadSystemConstant(string constantIdentifier)
		{
			string result = commParent.loadSystemConstant(constantIdentifier);
			if(result==null)return "";
			else return result;
		}
		public string selectFolder()
		{
			return selectFolder("");
		}
		public string selectFolder(string folderId)
		{
			string result = commParent.selectFolder(folderId, "", "Select Folder");
			if(result==null)return "";
			else return result;
		}
		public string selectFile(string filtro, string titulo)
		{
			return selectFile(filtro, titulo, "");
		}
		public string selectFile(string filtro, string titulo, string identificadorTipo)
		{
			string[] resultado = commParent.selectFile(identificadorTipo, filtro, false, titulo);
			if(resultado.Length==0)return "";
			else return resultado[0];
		}
		public string[] selectFiles(string filtro, string titulo)
		{
			return selectFiles(filtro, titulo, "");
		}
		public string[] selectFiles(string filter, string titulo, string identificadorTipo)
		{
			return commParent.selectFile(identificadorTipo, filter, true, titulo);
		}
		public string createFile(string extension)
		{
			string resultado = commParent.createFile("", extension);
			if(resultado==null)return "";
			else return resultado;
		}

		public virtual bool PC_Initialize()
		{
			return true;
		}
	}
}
