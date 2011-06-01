/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 25/05/2011
 * Hora: 22:08
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PlatCPL
{
	/// <summary>
	/// Description of PlatCPLv4.
	/// </summary>
	public partial class I_PlatCPLv4 : UserControl
	{
		PlatCPLv4 platform;
		InterCommunication comm;
		
		public I_PlatCPLv4()
		{
			InitializeComponent();
			platform = new PlatCPLv4();
			comm = platform.comm;
			fillComboBox();
			//Select user applications
			selecionarPerfil("");
			this.Dock = DockStyle.Fill;
		}
		public I_PlatCPLv4(PlatCPLv4 existingPlatform)
		{
			InitializeComponent();
			platform = existingPlatform;
			fillComboBox();
		}

		private bool selecionarPerfil(string perfil)
		{
			return false;
		}
		private void fillComboBox()
		{
			comboBox1.SuspendLayout();
			comboBox1.Items.Clear();
			comboBox1.Items.Add("");
			foreach(string app in platform.apps)comboBox1.Items.Add(app);
			comboBox1.SelectedIndex = 0;
			comboBox1.ResumeLayout();
			ComboBox1SelectedIndexChanged(null, null);
		}
		void ComboBox1SelectedIndexChanged(object sender, System.EventArgs e)
		{
			//foreach(System.Windows.Forms.Control contr in this.panel1.Controls) contr.Dispose();
			//this.panel1.Controls.Clear();
			if(this.comboBox1.SelectedItem != null)
			{
				string appName = (string)this.comboBox1.SelectedItem;
				if( appName.Length > 0 )
				{
					bool alreadyOpened = platform.isAppOpen(appName);
					if( !alreadyOpened )
					{
						PcAppHandler appControl = platform.getApplicationInstance(appName);
						if( appControl != null )
						{
							System.Windows.Forms.TabPage appPage = new System.Windows.Forms.TabPage(appName);

							System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
							/*panel.Location = new System.Drawing.Point(0,2);
							panel.Size = new System.Drawing.Size( appPage.Size.Width-2, appPage.Size.Height-3 );
							panel.Anchor = (System.Windows.Forms.AnchorStyles)
								(System.Windows.Forms.AnchorStyles.Top |
								 System.Windows.Forms.AnchorStyles.Left |
								 System.Windows.Forms.AnchorStyles.Right |
								 System.Windows.Forms.AnchorStyles.Bottom);*/
							panel.Dock = System.Windows.Forms.DockStyle.Fill;
							panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
							
							this.tabControl1.TabPages.Add(appPage);
							appPage.Controls.Add(panel);
							panel.Controls.Add( appControl );
						}
					}
				}
			}
		}
	}
}