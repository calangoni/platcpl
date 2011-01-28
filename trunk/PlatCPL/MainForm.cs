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
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
		}
		
		void Button1Click(object sender, System.EventArgs e) //Button Read XML
		{
			System.Windows.Forms.OpenFileDialog window = new System.Windows.Forms.OpenFileDialog();
			if(window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string fileXMLname = window.FileName;
				if(fileXMLname != null && fileXMLname.Length > 0)
				{
					XML3 test = new XML3();
					test.LoadXmlFile(fileXMLname);
				}
			}
		}
	}

}
