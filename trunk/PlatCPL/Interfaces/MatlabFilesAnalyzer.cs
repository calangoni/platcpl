/*
 * Created by SharpDevelop.
 * User: LB07270
 * Date: 30/5/2011
 * Time: 11:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using PlatCPL.Classes;
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
			graphicImage = new System.Drawing.Bitmap(pictureBox1.Width, pictureBox1.Height);
			return true;
		}
		
		void B_Load(object sender, EventArgs e) // Load
		{
			string[] matFilesNames = comm.selectFilesNN("Mat files (*.mat)|*.mat", "Select files");
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
							item.SubItems.Add(infovar.minTime.ToString());
							item.SubItems.Add(infovar.maxTime.ToString());
							item.SubItems.Add("-");
							item.SubItems.Add("-");
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
			if(pictureBox1.Width<50)return;
			if(pictureBox1.Height<50)return;
			if(pictureBox1.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
				graphicImage = new System.Drawing.Bitmap(pictureBox1.Width-2, pictureBox1.Height-2);
			else graphicImage = new System.Drawing.Bitmap(pictureBox1.Width, pictureBox1.Height);
			System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(graphicImage);
			
			System.Drawing.Pen penBlack = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
			int numGraphs = 5;
			int graphSpacing = 5;
			int totalDeadSpace = (numGraphs+1)*graphSpacing;
			int graphsHeight = (int)System.Math.Floor( (double)(graphicImage.Height-totalDeadSpace)/numGraphs );
			int graphsWidth = graphicImage.Width - 2*graphSpacing;
			int gy;
			int gx;
			if(graphsHeight>10)
			{
				for(int i=0; i<numGraphs; i++)
				{
					gy = (i+1)*graphSpacing + i*graphsHeight;
					gx = graphSpacing;
					g.DrawRectangle(penBlack, gx-1, gy-1, graphsWidth+2, graphsHeight+2);
				}
			}
			//Draw main border
			g.DrawRectangle(penBlack, 0, 0, graphicImage.Width-1, graphicImage.Height-1);

			pictureBox1.Image = graphicImage;
		}
		
		void B_Export(object sender, EventArgs e)
		{
			System.Windows.Forms.ListViewItem item;
			System.Windows.Forms.ListView listView;
			listView = listView2;
			List<double> time = new List<double>();
			int index;
			
			if ( listView.Items.Count == 0 )
			{
				return;
			}
			
			#region Create The Time Axis 
			for ( int i = 0 ; i < listView.Items.Count ; i++ )
			{
				item = listView.Items[ i ];
				if ( item.Tag == null )
				{
					continue;
				}
				else if ( item.Tag is VariableInfo )
				{
					VariableInfo vi = (VariableInfo)item.Tag;
					for ( int j = 0 ; j < vi.time.Length ; j++ )
					{
						if ( ( index = time.BinarySearch( vi.time[ j ] ) ) < 0 ) // insert the value in the axis.
						{
							index = Math.Abs( index );
							if ( index > time.Count )
							{
								index = time.Count;
							}
							time.Insert( index , vi.time[ j ] );
							
						}
						else 
						{
							// do nothing, the value already exists in the axis
						}
					}
				}
			}
			#endregion
			
			#region Create The Data with the new time axis
			List<WorkedData> workedDataList = new List<WorkedData>();
			for ( int i = 0 ; i < listView.Items.Count ; i++ )
			{
				item = listView.Items[ i ];
				if ( item.Tag == null )
				{
					continue;
				}
				else if ( item.Tag is VariableInfo )
				{
					VariableInfo vi = (VariableInfo)item.Tag;
					WorkedData workedDataItem = new WorkedData( time , item.Text );
					if ( workedDataItem.CreateData( vi ) )
					{
						workedDataList.Add( workedDataItem );
					}
					else
					{
						//do nothing;
					}
				}
			}
			#endregion
			
			#region Write the dat file
			MDFFileHandler datManager = new MDFFileHandler();
			if(datManager.fillMdfData(workedDataList, time))
			{
				if(datManager.exportToMdfFile(comm.createFileNN(".dat")))
				{
					return;
				}
			}
			#endregion
			
			#region Write the ascii File
			string path = comm.createFile("", ".ascii");
			System.IO.StreamWriter fileOutput = null;
			if ( path != null && path.Length > 0 )
			{
				try
				{
					fileOutput = new System.IO.StreamWriter( path );
				}
				catch
				{
					//error
				}
			}
			
			if( fileOutput != null )
			{
				// line 1
				fileOutput.WriteLine( "ETASAsciiItemFile" + "\t" + "record" + "\t" + "CrLf" + "\t" + "Tab" );
				// -------
				
				// line 2
				fileOutput.Write( "sampleCount" + "\t" + time.Count.ToString() );
				for ( int i = 15 ; i > time.Count.ToString().Length ; i-- )
				{
					fileOutput.Write( " " );
				}
				fileOutput.WriteLine();
				//-------
				
				// line 3
				fileOutput.Write( "time" + "\t" );
				for ( int i = 0 ; i < workedDataList.Count ; i++ )
				{
					fileOutput.Write( workedDataList[ i ].LabelName  + "\t" );
				}
				fileOutput.WriteLine();
				//------
				
				// line 4
				fileOutput.Write( "f8" + "\t" );
				for ( int i = 0 ; i < workedDataList.Count ; i++ )
				{
					fileOutput.Write( "f8" + "\t" );
				}
				fileOutput.WriteLine();
				// ------
				
				// line 5
				fileOutput.Write( "\"s\"" + "\t" );
				for ( int i = 0 ; i < workedDataList.Count ; i++ )
				{
					fileOutput.Write( "\"" + ( i+1 ).ToString() + "\"" + "\t" );
				}
				fileOutput.WriteLine();
				// ------
				
				// Data
				for( int i = 0 ; i < time.Count ; i++ )
				{
					fileOutput.Write( time[ i ].ToString() + "\t" );
					for ( int j = 0 ; j < workedDataList.Count ; j++ )
					{
						fileOutput.Write( workedDataList[ j ].Data[ i ].ToString() + "\t" );
					}
					fileOutput.WriteLine();
				}
				// ------
				fileOutput.Close();
			}
			#endregion
		}
		
		void B_Add(object sender, EventArgs e) // add
		{
			
		}
		
		void B_Remove(object sender, EventArgs e) // remove
		{
			
		}
		
		void B_Test(object sender, EventArgs e) // test
		{
			/*byte[] bytes;
			bytes = new byte[]{ 0,0,0,0,0,0,0,128 }; // 0
			comm.msgDebug(" ["+BitConverter.ToDouble(bytes,0).ToString() +"]");
			bytes = new byte[]{ 123,20,174,71,225,122,132,63 }; // 0.01
			comm.msgDebug(" ["+BitConverter.ToDouble(bytes,0).ToString() +"]");
			bytes = new byte[]{ 0,0,0,0,0,0,240,63 }; // 1
			comm.msgDebug(" ["+BitConverter.ToDouble(bytes,0).ToString() +"]");
			bytes = new byte[]{ 22,86,231,158,175,3,210,60 }; // 0.000000000000001
			comm.msgDebug(" ["+BitConverter.ToDouble(bytes,0).ToString() +"]");
			double val = 0.000000000000001;
			byte[] testDouble = BitConverter.GetBytes(val);
			comm.msgDebug("["+testDouble[0]+"]["+testDouble[1]+"]["+testDouble[2]+"]["+testDouble[3]+"]["+testDouble[4]+"]["+testDouble[5]+"]["+testDouble[6]+"]["+testDouble[7]+"]");
			val = 1;
			testDouble = BitConverter.GetBytes(val);
			comm.msgDebug("["+testDouble[0]+"]["+testDouble[1]+"]["+testDouble[2]+"]["+testDouble[3]+"]["+testDouble[4]+"]["+testDouble[5]+"]["+testDouble[6]+"]["+testDouble[7]+"]");*/
			
			MDFFileHandler manager = new MDFFileHandler();
			string fileName = comm.selectFileNN("*.dat|*.dat","Select file");
			bool result = false;
			if(fileName.Length>0)
			{
				result = manager.readMdfFile(fileName);
				string line;
				for(int i=0; i<30; i++)
				{
					MDFFileHandler.HDBLOCK headerBlock = (MDFFileHandler.HDBLOCK)manager.links[1].block;
					line = " (";
					line += headerBlock.DGblock.DATAblock.getDoubleValue(100*(i+1), 0).ToString();
					line += ") - (";
					line += headerBlock.DGblock.DATAblock.getDoubleValue(100*(i+1), 1).ToString();
					line += ")";
					comm.msgDebug(line);
				}
			}
		}
	}
	
	public class VariableInfo
	{
		public double maxTime;
		public double minTime;
		public double[] time;
		public double[] data;
		public string name;
		
		public VariableInfo(string variableName, csmatio.types.MLArray matlabArray)
		{
			name = variableName;
			if(matlabArray.IsDouble)
			{
				csmatio.types.MLDouble temp = (csmatio.types.MLDouble)matlabArray;
				double[][] tempData = temp.GetArray();
				if(tempData.Length!=2)
				{
					time = null;
					data = null;
					maxTime = 0;
					minTime = 0;
				}
				else if(tempData.Length<2)
				{
					time = null;
					data = null;
					maxTime = 0;
					minTime = 0;
				}
				else
				{
					time = tempData[0];
					data = tempData[1];
					maxTime = time[time.Length-1];
					minTime = time[0];
				}
			}
		}
		public string GetInterpStrVal(double timeVal)
		{
			if(data==null||time==null)return "-";
			int i;
			for(i=0; i<time.Length; i++)
			{
				if(timeVal<=time[i])break;
			}
			if(timeVal==time[i])return data[i].ToString();
			if(i>=time.Length)return "-";
			if(i<=0)return "-";
			double result = (timeVal-time[i-1])/(time[i]-time[i-1])*(data[i]-data[i-1])+data[i-1];
			return result.ToString();
		}
		public double GetInterpVal(double timeVal)
		{
			//if(data==null||time==null)return 0;
			int i;
			for(i=0; i<time.Length; i++)
			{
				if(timeVal<=time[i])break;
			}
			if(timeVal==time[i])return data[i];
			if(i>=time.Length)return data[data.Length-1];
			if(i<=0)return data[0];
			double result = (timeVal-time[i-1])/(time[i]-time[i-1])*(data[i]-data[i-1])+data[i-1];
			return result;
		}
	}
}
