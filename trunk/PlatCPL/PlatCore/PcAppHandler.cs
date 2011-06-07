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
		public PlatCPL.InterCommunication comm;
		public string applicationShowName;

		public PcAppHandler()
		{
			comm = null;
			applicationShowName = this.GetType().Name;
		}
		public void setComments(string appShowName)
		{
			if(appShowName!=null)applicationShowName = appShowName;
		}

		public virtual bool PC_Initialize()
		{
			return true;
		}
	}
}
