/*
 * Created by SharpDevelop.
 * User: Langoni
 * Date: 08/03/2011
 * Time: 15:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PlatCPL.Interfaces
{
	/// <summary>
	/// Description of PhoneBook.
	/// </summary>
	public partial class PhoneBook : PcAppHandler
	{
		public PhoneBook()
		{
			InitializeComponent();
		}

		public void B_loadP2Kfile(object sender, EventArgs e)
		{
			string filePath = comm.selectFileNN("CSV file (*.csv)|*.csv","Select file");
			System.IO.StreamReader fileStream = new System.IO.StreamReader(filePath, System.Text.Encoding.Default, true);
			string line = fileStream.ReadLine();
			string[] lineParts, nameParts;
			System.Collections.Generic.List<string> categories = new System.Collections.Generic.List<string>();
			while(line != null)
			{
				lineParts = line.Split(';');
				if( lineParts.Length > 0 )
				{
					if(lineParts[0] == "CATEGORY")
					{
						if(lineParts.Length != 5)comm.msgError("ERROR 36");
						else categories.Add(lineParts[2]);
					}
					else if(lineParts[0] == "PHONE")
					{
						if(lineParts[4] == "0") lineParts[4] = "Work";
						if(lineParts[4] == "1") lineParts[4] = "Home";
						if(lineParts[4] == "2") lineParts[4] = "Main"; //General
						if(lineParts[4] == "3") lineParts[4] = "Mobile";
						if(lineParts[4] == "4") lineParts[4] = "Home Fax";
						if(lineParts[4] == "5") lineParts[4] = "Pager";
						if(lineParts[3].Length==13 && lineParts[3].StartsWith("+55") && lineParts[3][5]!='0')
						{
							lineParts[3] = lineParts[3].Substring(0,3) + " (" + lineParts[3].Substring(3,2) + ") " + lineParts[3].Substring(5,4) + "-" + lineParts[3].Substring(9,4);
						}
						if(lineParts[3].Length==11 && lineParts[3].StartsWith("0") && lineParts[3][3]!='0')
						{
							lineParts[3] = "(" + lineParts[3].Substring(1,2) + ") " + lineParts[3].Substring(3,4) + "-" + lineParts[3].Substring(7,4);
						}
						if(lineParts[3].Length==8 && lineParts[3][0]!='0')
						{
							lineParts[3] = lineParts[3].Substring(0,4) + "-" + lineParts[3].Substring(4,4);
						}
						nameParts = lineParts[2].Split(' ');
						for(int i=0; i<nameParts.Length; i++)
						{
							int state = 0;
							for(int j=0; j<nameParts[i].Length; j++)
							{
								if(state==0)
								{
									if(nameParts[i][j]>='A' && nameParts[i][j]<='Z')state = 1;
									else break;
								}
								else if(state==1)
								{
									if(nameParts[i][j]>='a' && nameParts[i][j]<='z')state = 2;
									else break;
								}
								else if(state==2)
								{
									if(nameParts[i][j]>='a' && nameParts[i][j]<='z'){}
									else if(nameParts[i][j]>='A' && nameParts[i][j]<='Z')
									{
										state = 3;
										nameParts[i] = nameParts[i].Substring(0, j) + " " + nameParts[i].Substring(j);
										break;
									}
									else break;
								}
								else break;
							}
							if(state == 3)break;
						}
						lineParts[2] = nameParts[0];
						for(int i=1; i<nameParts.Length; i++) lineParts[2] += " "+nameParts[i];
						comm.msgInfo(lineParts[2]+","+lineParts[2]+",,,,,,,,,,,,,,,,,,,,,,,,,* My Contacts,"+lineParts[4]+","+lineParts[3]+",,,,,,,,,,");
					}
					else comm.msgError("ERROR 42");
				}
				line = fileStream.ReadLine();
			}
		}
	}
}
