/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 26/12/2010
 * Hora: 15:21
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Windows.Forms;

namespace PlatCPL
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : System.Windows.Forms.Form
	{

		public MainForm()
		{
			InitializeComponent();
			this.Controls.Add( new PlatCPL.I_PlatCPLv4() );
		}
	}
}
