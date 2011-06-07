/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 30/05/2011
 * Hora: 17:43
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PlatCPL
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			this.Controls.Add( new PlatCPL.I_PlatCPLv4() );
		}
	}
}
