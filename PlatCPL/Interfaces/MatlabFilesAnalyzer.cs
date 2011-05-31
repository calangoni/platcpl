/*
 * Created by SharpDevelop.
 * User: LB07270
 * Date: 30/5/2011
 * Time: 11:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace PlatCPL.Interfaces
{
	/// <summary>
	/// Description of MatlabFilesAnalyzer.
	/// </summary>
	public partial class MatlabFilesAnalyzer : PcAppHandler
	{
		System.Drawing.Bitmap graphicImage;
		public MatlabFilesAnalyzer()
		{
			InitializeComponent();
		}
		public override bool PC_Initialize()
		{
			graphicImage = new System.Drawing.Bitmap(panel1.Size.Width, panel1.Size.Height);
			return true;
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			string[] matFilesNames = selectFiles("Mat files (*.mat)|*.mat", "Select files");
			foreach(string fileName in matFilesNames)
			{
				listView1.Items.Add(System.IO.Path.GetFileNameWithoutExtension(fileName)).SubItems.Add(fileName);
				addVariable(fileName);
			}
		}
		
		public bool addVariable(string matFileName)
		{
			if (matFileName!=null && matFileName.Length>0)
			{
				richTextBox1.Text = richTextBox1.Text + "Attempting to read the file '" + matFileName + "'...";
				try
				{
					csmatio.io.MatFileReader mfr = new csmatio.io.MatFileReader(matFileName);
					richTextBox1.Text += "Done!\nMAT-file contains the following:\n";
					richTextBox1.Text += mfr.MatFileHeader.ToString() + "\n";
					foreach (csmatio.types.MLArray mla in mfr.Data)
					{
						richTextBox1.Text = richTextBox1.Text + mla.ContentToString().Replace('\n',' ') + "\n";
						VariableInfo infovar = new VariableInfo(System.IO.Path.GetFileNameWithoutExtension(matFileName), mla);
						if(infovar.data!=null)
						{
							System.Windows.Forms.ListViewItem item = listView2.Items.Add(infovar.name);
							item.SubItems.Add("");
							item.SubItems.Add("");
							item.SubItems.Add("");
							item.SubItems.Add("");
							item.Tag = infovar;
							return true;
							//infoMsg(" - lines: "+infovar.data.Length);
							//infoMsg(" - columns: "+infovar.data[0].Length);
						}
					}
				}
				catch (System.IO.IOException exc)
				{
					richTextBox1.Text = richTextBox1.Text + "Invalid MAT-file!\n";
					System.Windows.Forms.MessageBox.Show("Invalid binary MAT-file! Please select a valid binary MAT-file.\n"+exc.Message,
					                                     "Invalid MAT-file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
				}
			}
			return false;
		}
		
		void TextBox1KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			System.Windows.Forms.TextBox textbox = (System.Windows.Forms.TextBox)sender;
			if(e.KeyCode == System.Windows.Forms.Keys.Enter)
			{
				double timeVal = 0;
				if(double.TryParse(textbox.Text, out timeVal))
				{
					string timeStrVal = timeVal.ToString();
					for(int i=0; i<listView2.Items.Count; i++)
					{
						listView2.Items[i].SubItems[3].Text = timeStrVal;
						listView2.Items[i].SubItems[4].Text = ((VariableInfo)listView2.Items[i].Tag).GetInterpStrVal(timeVal);
					}
				}
				else
				{
					for(int i=0; i<listView2.Items.Count; i++)
					{
						listView2.Items[i].SubItems[3].Text = "-";
						listView2.Items[i].SubItems[4].Text = "-";
					}
				}
			}
		}
		
		void B_Redraw(object sender, EventArgs e)
		{
			if(panel1.Size.Width<50)return;
			if(panel1.Size.Height<50)return;
			graphicImage = new System.Drawing.Bitmap(panel1.Width, panel1.Height);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(graphicImage);
			g.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Tomato, 5), 30, 20, 80, 100);
			panel1.Invalidate();
		}
		
		void Panel1Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			System.Windows.Forms.Panel panel = (System.Windows.Forms.Panel)sender;
			panel.SuspendLayout();
			e.Graphics.DrawImage(graphicImage, 0, 0);
			panel.ResumeLayout();
		}
	}
	
	public class VariableInfo
	{
		public double[][] data;
		public string name;
		
		public VariableInfo(string variableName, csmatio.types.MLArray matlabArray)
		{
			name = variableName;
			if(matlabArray.IsDouble)
			{
				csmatio.types.MLDouble temp = (csmatio.types.MLDouble)matlabArray;
				data = temp.GetArray();
				if(data.Length!=2) data=null;
				else if(data[0].Length<2) data=null;
				//infoMsg(" - lines: "+dados.Length);
				//infoMsg(" - columns: "+dados[0].Length);
			}
		}
		public string GetInterpStrVal(double timeVal)
		{
			if(data==null)return "-";
			int i;
			for(i=0; i<data[0].Length; i++)
			{
				if(timeVal<=data[0][i])break;
			}
			if(timeVal==data[0][i])return data[1][i].ToString();
			if(i>=data[0].Length)return "-";
			if(i<=0)return "-";
			double result = (timeVal-data[0][i-1])/(data[0][i]-data[0][i-1])*(data[1][i]-data[1][i-1])+data[1][i-1];
			return result.ToString();
		}
	}
}
