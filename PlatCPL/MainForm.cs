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
		
		private void loadXML()
		{
			System.Windows.Forms.OpenFileDialog window = new System.Windows.Forms.OpenFileDialog();
			if(window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				string fileXMLname = window.FileName;
				System.IO.StreamReader fileStream = null;
				try
				{
					fileStream = new System.IO.StreamReader(fileXMLname, System.Text.Encoding.Default, true);
				}
				catch(System.Exception exc)
				{
					System.Diagnostics.Trace.WriteLine(exc.Message);
					fileStream = null;
				}
				if(fileStream != null)
				{
					State_XMLfile state = new State_XMLfile();
					char[] buffer = new char[4000]; // page = 4096 bytes
					int bytesRead = fileStream.ReadBlock(buffer, 0, buffer.Length);
					bool resultOK = false;
					while(bytesRead > 0)
					{
						//for(int i=0; i<bytesRead; i++) System.Diagnostics.Trace.WriteLine(" - "+(i+1)+") '"+(buffer[i])+"' ["+((int)buffer[i])+"]");
						resultOK = readXMLstream(buffer, bytesRead, state);
						if(resultOK)
							bytesRead = fileStream.ReadBlock(buffer, 0, buffer.Length);
						else
							break;
					}
					fileStream.Close();
					if(resultOK) System.Diagnostics.Trace.WriteLine("XML OK!!");
					else System.Diagnostics.Trace.WriteLine("ERROR[64]: there was a problem reading the XML file");
				}
			}
		}
		private bool readXMLstream(char[] buffer, int dataLength, State_XMLfile state)
		{
			for(int i=0; i<dataLength; )
			{
				//TODO: HTML tags with no close: <BR>
				if(state.actualArea == State_XMLfile.Area.InTopLevelWaitingForRootElement)
				{
					#region InTopLevelWaitingForRootElement: ignore spaces until '<'
					while( i<dataLength &&
					      (buffer[i] == ' ' ||
					       buffer[i] == '\r' ||
					       buffer[i] == '\n' ||
					       buffer[i] == '\t' )) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					if(buffer[i] == '<')
					{
						state.actualArea = State_XMLfile.Area.WaitingForTagName;
						state.closingTag = false;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[80]: invalid character");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.WaitingForTagName)
				{
					#region WaitingForTagName: no spaces allowed, next character should be tag name, '!', '?' or '/' (if its a closing tag)
					// > First character of tag name
					if((buffer[i] >= 'A' && buffer[i] <= 'Z') ||
					   (buffer[i] >= 'a' && buffer[i] <= 'z') ||
					   //(buffer[i] >= '0' && buffer[i] <= '9') || //accept numbers
					   (buffer[i] >= 0x80) || //accept non ASCII characters
					   (buffer[i] == '_') )
					{
						if(!state.closingTag)
						{
							if(state.level>=0)
							{
								if(state.actualTagHasText[state.level])
								{
									System.Diagnostics.Trace.WriteLine("ERROR[13]: tag with text and children");
									return false;
								}
								state.actualTagHasChildren[state.level] = true;
							}
							state.level++;
							if(state.level >= State_XMLfile.MAX_LEVELS)
							{
								System.Diagnostics.Trace.WriteLine("ERROR[83]: XML file too deep");
								return false;
							}
							state.actualTagHasAttributes[state.level] = false;
							state.actualTagHasChildren[state.level] = false;
							state.actualTagHasText[state.level] = false;
							state.actualTagElement = state.actualTagElement.NewChild("","");
							//state.actualTagName[state.level] = "";
						}
						state.indexBeginName = i;
						state.actualArea = State_XMLfile.Area.InTagName;
						i++;
					}
					else if(buffer[i] == '/')
					{
						//TODO: verify if the buffer has the space needed
						state.indexNameBuffer = 0;
						state.indexBeginName = -1;
						//state.actualArea = State_XMLfile.Area.WaitingForTagName;
						state.closingTag = true;
						i++;
					}
					else if(buffer[i] == '?')
					{
						//TODO: accept ? tags
						if(state.level>=0)
						{
							System.Diagnostics.Trace.WriteLine("ERROR[46]: '<?' tag should be in root level");
							return false;
						}
						state.level++;
						state.actualTagHasAttributes[state.level] = false;
						state.actualTagHasChildren[state.level] = false;
						state.actualTagHasText[state.level] = false;
						state.actualTagElement = state.actualTagElement.NewChild("","");
						state.indexBeginName = -1;
						state.actualArea = State_XMLfile.Area.InSpecialTagI;
						i++;
					}
					else if(buffer[i] == '!')
					{
						//TODO: accept ! tags
						if(state.level>=0)
						{
							System.Diagnostics.Trace.WriteLine("ERROR[67]: '<!' tag should be in root level");
							return false;
						}
						state.level++;
						state.actualTagHasAttributes[state.level] = false;
						state.actualTagHasChildren[state.level] = false;
						state.actualTagHasText[state.level] = false;
						state.actualTagElement = state.actualTagElement.NewChild("","");
						state.indexBeginName = -1;
						state.actualArea = State_XMLfile.Area.InSpecialTagE;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[81]: invalid character for tag name");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InTagName)
				{
					#region InTagName: read tag name characters until 'space', '>' or '/>'
					while( i<dataLength &&
					      ((buffer[i] >= 'A' && buffer[i] <= 'Z') ||
					       (buffer[i] >= 'a' && buffer[i] <= 'z') ||
					       (buffer[i] >= '0' && buffer[i] <= '9') || //accept numbers
					       (buffer[i] >= 0x80) || //accept non ASCII characters
					       (buffer[i] == '_') ||
					       (buffer[i] == '.') ||(buffer[i] == '-') ||(buffer[i] == ':')) //accept some symbols
					     ) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					if(!state.closingTag)
					{
						//If it is not a closing tag, it may have attributes or just close the tag without any contents
						//Next may be '>', '/>' or 'space'
						if(buffer[i] == '>' ||
						   buffer[i] == '/' ||
						   buffer[i] == ' ' ||
						   buffer[i] == '\r' ||
						   buffer[i] == '\n' ||
						   buffer[i] == '\t' )
						{
							//Name really finished, next step: store the name
							if(state.indexBeginName>=0)
							{
								//The beginning of the name is in the buffer
								int nameLength = i-state.indexBeginName;
								if(nameLength>0)
								{
									//name is not null
									state.actualTagElement.Tag +=
										new string(buffer, state.indexBeginName, nameLength);
								}
								else
								{
									//name = ""
									System.Diagnostics.Trace.WriteLine("ERROR[61]: invalid tag name");
									return false;
								}
							}
							else
							{
								//The beginning was already read in some previous buffer, read the rest now
								if(i>0)
								{
									//Name is not null
									state.actualTagElement.Tag += new string(buffer, 0, i);
								}
								else if(state.actualTagElement.Tag.Length == 0)
								{
									//Name = ""
									System.Diagnostics.Trace.WriteLine("ERROR[73]: invalid tag name");
									return false;
								}
							}
							state.indexBeginName = -1;
							if(buffer[i] == '>')
								state.actualArea = State_XMLfile.Area.InTagContents;
							else if(buffer[i] == '/')
								state.actualArea = State_XMLfile.Area.WaitingForTagClose;
							else
								state.actualArea = State_XMLfile.Area.WaitingForAttributeName;
							i++;
						}
						else
						{
							System.Diagnostics.Trace.WriteLine("ERROR[86]: invalid character in tag name");
							return false;
						}
					}
					else
					{
						//If it is a closing tag, it can't have attributes, it should close right away
						//Next should be '>'
						if(buffer[i] == '>')
						{
							if(state.indexBeginName>=0)
							{
								int nameLength = i-state.indexBeginName;
								if(nameLength>0)
								{
									//TODO: verify if the buffer has the space needed
									for(int j=0; j<nameLength; j++)
										state.nameBuffer[state.indexNameBuffer++] =	buffer[state.indexBeginName+j];
								}
								else
								{
									System.Diagnostics.Trace.WriteLine("ERROR[61]: invalid tag name");
									return false;
								}
							}
							else
							{
								if(i>0)
								{
									//TODO: verify if the buffer has the space needed
									for(int j=0; j<i; j++)
										state.nameBuffer[state.indexNameBuffer++] =
											buffer[0+j];
								}
								else if(/*state.actualTagName[state.level]*/state.actualTagElement.Tag.Length == 0)
								{
									System.Diagnostics.Trace.WriteLine("ERROR[73]: invalid tag name");
									return false;
								}
							}
							if(/*state.actualTagName[state.level]*/state.actualTagElement.Tag.Length == state.indexNameBuffer)
							{
								for(int j=0; j<state.actualTagElement.Tag.Length; j++)
								{
									if(state.actualTagElement.Tag[j] != state.nameBuffer[j])
									{
										System.Diagnostics.Trace.WriteLine("ERROR[50]: invalid closing tag name");
										return false;
									}
								}
							}
							else
							{
								System.Diagnostics.Trace.WriteLine("ERROR[50]: invalid closing tag name");
								return false;
							}
							state.indexBeginName = -1;
							state.indexNameBuffer = -1;
							System.Diagnostics.Trace.WriteLine(" - finished with tag: "+state.actualTagElement.Tag);
							state.level--;
							state.actualTagElement = state.actualTagElement.pai;
							state.closingTag = false;
							if(state.level>=0)state.actualArea = State_XMLfile.Area.InTagContents;
							else state.actualArea = State_XMLfile.Area.InTopLevelWaitingForRootElement;
							i++;
						}
						else
						{
							System.Diagnostics.Trace.WriteLine("ERROR[86]: invalid character in tag name");
							return false;
						}
					}
					
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.WaitingForTagClose)
				{
					#region WaitingForTagClose: next character should be '>'
					// case: <tag attrib="content"/>
					if(buffer[i] == '>')
					{
						state.indexBeginName = -1;
						state.indexNameBuffer = -1;
						System.Diagnostics.Trace.WriteLine(" - finished with tag: "+state.actualTagElement.Tag);
						state.level--;
						state.actualTagElement = state.actualTagElement.pai;
						state.closingTag = false;
						if(state.level>=0)state.actualArea = State_XMLfile.Area.InTagContents;
						else state.actualArea = State_XMLfile.Area.InTopLevelWaitingForRootElement;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[47]: invalid character in attributes area");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeName)
				{
					#region WaitingForAttributeName: ignore spaces until attribute name, '>' or '/>'
					// spaces are allowed only if there are attributes: <tag > not allowed
					while( i<dataLength &&
					      (buffer[i] == ' ' ||
					       buffer[i] == '\r' ||
					       buffer[i] == '\n' ||
					       buffer[i] == '\t' )) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					// > First character of attribute name or >
					if(buffer[i] == '>')
					{
						if(!state.actualTagHasAttributes[state.level])
						{
							System.Diagnostics.Trace.WriteLine("ERROR[99]: invalid character in tag name");
							return false;
						}
						else
						{
							state.indexBeginName = -1;
							state.actualArea = State_XMLfile.Area.InTagContents;
							i++;
						}
					}
					else if(buffer[i] == '/')
					{
						state.indexBeginName = -1;
						state.actualArea = State_XMLfile.Area.WaitingForTagClose;
						i++;
					}
					else if((buffer[i] >= 'A' && buffer[i] <= 'Z') ||
					        (buffer[i] >= 'a' && buffer[i] <= 'z') ||
					        //(buffer[i] >= '0' && buffer[i] <= '9') || //accept numbers
					        (buffer[i] >= 0x80) || //accept non ASCII characters
					        (buffer[i] == '_') )
					{
						state.indexBeginName = i;
						state.actualTagHasAttributes[state.level] = true;
						state.actualArea = State_XMLfile.Area.InAttributeName;
						i++;
					}
					else
					{
						// > STOP or ...
						System.Diagnostics.Trace.WriteLine("ERROR[04]: invalid character for attribute name");
						return false;
						// > CONTINUE
						//state.indexBeginName = i;
						//state.actualTagHasAttributes[state.level] = true;
						//state.actualArea = State_XMLfile.Area.InAttributeName;
						//i++;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeSpaceOrEnd)
				{
					#region WaitingForAttributeSpaceOrEnd: there should be at least one space or '>' or '/>'
					// case not allowed: <tag attrib1="content"attrib2="content">
					if(buffer[i] == ' ' ||
					   buffer[i] == '\r' ||
					   buffer[i] == '\n' ||
					   buffer[i] == '\t' ||
					   buffer[i] == '>' ||
					   buffer[i] == '/')
					{
						state.actualArea = State_XMLfile.Area.WaitingForAttributeName;
						//i++; it is just a verification, leave the treatment to the WaitingForAttributeName function
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[87]: invalid character in attributes area");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InAttributeName)
				{
					#region InAttributeName: waiting for 'space' or '='
					while( i<dataLength &&
					      ((buffer[i] >= 'A' && buffer[i] <= 'Z') ||
					       (buffer[i] >= 'a' && buffer[i] <= 'z') ||
					       (buffer[i] >= '0' && buffer[i] <= '9') || //accept numbers
					       (buffer[i] >= 0x80) || //accept non ASCII characters
					       (buffer[i] == '_') ||
					       (buffer[i] == '.') ||(buffer[i] == '-') ||(buffer[i] == ':')) //accept some symbols
					     ) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					//Next may be 'space' or '='
					if(buffer[i] == '=' ||
					   buffer[i] == ' ' ||
					   //buffer[i] == '\r' ||
					   //buffer[i] == '\n' ||
					   buffer[i] == '\t' )
					{
						if(state.indexBeginName>=0)
						{
							int nameLength = i-state.indexBeginName;
							//TODO: add attribute
							//state.actualTagName[state.level] += new string(buffer, state.indexBeginName, nameLength);
						}
						else
						{
							//TODO: add attribute
							// if(i>0)
							//state.elements[state.level] += new string(buffer, 0, i);
						}
						state.indexBeginName = -1;
						state.actualArea = State_XMLfile.Area.WaitingForAttributeEqual;
						// i++; not necessary
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[68]: invalid character in attribute name");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeEqual)
				{
					#region WaitingForAttributeEqual: ignore spaces until '='
					while( i<dataLength &&
					      (buffer[i] == ' ' ||
					       buffer[i] == '\r' ||
					       buffer[i] == '\n' ||
					       buffer[i] == '\t' )) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					//Next should be '='
					if(buffer[i] == '=')
					{
						state.actualArea = State_XMLfile.Area.WaitingForAttributeValue;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[94]: invalid character in tag attributes");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeValue)
				{
					#region WaitingForAttributeValue: ignore spaces until '"' or 'number'
					//TODO: there are property values like prop=va.lue (maybe only in HTML, I don't remember)
					while( i<dataLength &&
					      (buffer[i] == ' ' ||
					       buffer[i] == '\r' ||
					       buffer[i] == '\n' ||
					       buffer[i] == '\t' )) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					//Next should be '"' or 'number'
					if(buffer[i] == '"')
					{
						state.indexBeginName = i;
						state.actualArea = State_XMLfile.Area.InAttributeValueString;
						i++;
					}
					else if( buffer[i]>='0' && buffer[i]<='9' )
					{
						state.indexBeginName = i;
						state.actualArea = State_XMLfile.Area.InAttributeValueNumber;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[20]: invalid character for attribute value");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InAttributeValueString)
				{
					#region InAttributeValueString: waiting for '"' (\" allowed?)
					//TODO: read attribute string. \" allowed?
					while( i<dataLength && (buffer[i]!='"') ) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					if(buffer[i]=='"')
					{
						//TODO: store the text
						state.actualArea = State_XMLfile.Area.WaitingForAttributeSpaceOrEnd;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[95]: invalid character inside text content of the attribute");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InAttributeValueNumber)
				{
					#region InAttributeValueNumber: waiting for 'space' or '>'
					//TODO: attribute number
					System.Diagnostics.Trace.WriteLine("ERROR[88]: not implemented");
					return false;
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InTagContents)
				{
					#region InTagContents: waiting for 'text', <child> or </end>
					//TODO: store spaces in case of being text. Example: <tag>   TEXT   </tag>
					while( i<dataLength &&
					      (buffer[i] == ' ' ||
					       buffer[i] == '\r' ||
					       buffer[i] == '\n' ||
					       buffer[i] == '\t' )) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					//Next should be '"' or 'number'
					if(buffer[i] == '<')
					{
						state.actualArea = State_XMLfile.Area.WaitingForTagName;
						i++;
					}
					else
					{
						state.indexBeginName = i;
						state.actualArea = State_XMLfile.Area.InText;
						state.actualTagHasText[state.level] = true;
						i++;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InText)
				{
					#region Waiting for </end>
					while( i<dataLength && (buffer[i]!='<' && buffer[i]!='>') ) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					if(buffer[i]=='<')
					{
						//TODO: store the text
						state.actualArea = State_XMLfile.Area.WaitingForTagName;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[26]: invalid character inside text content of the tag");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InSpecialTagI)
				{
					#region Waiting for ?>
					//FIXME: search for '?>', not just '>'
					while( i<dataLength && (buffer[i]!='<' && buffer[i]!='>') ) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					if(buffer[i] == '>')
					{
						state.level--;
						if(state.level>=0)
							return false;
						else
							state.actualArea = State_XMLfile.Area.InTopLevelWaitingForRootElement;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[52]: invalid character inside '?' tag");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InSpecialTagE)
				{
					#region Waiting for >
					//FIXME: verify the contents, not just search for '>'
					while( i<dataLength && (buffer[i]!='<' && buffer[i]!='>') ) i++;
					if( i==dataLength )
					{
						//End of buffer
						break;
					}
					if(buffer[i] == '>')
					{
						state.level--;
						if(state.level>=0)
							return false;
						else
							state.actualArea = State_XMLfile.Area.InTopLevelWaitingForRootElement;
						i++;
					}
					else
					{
						System.Diagnostics.Trace.WriteLine("ERROR[77]: invalid character inside '!' tag");
						return false;
					}
					#endregion
				}
				else if(state.actualArea == State_XMLfile.Area.InSpecialTagC)
				{
					#region Waiting for -->
					System.Diagnostics.Trace.WriteLine("ERROR[98]: not implemented");
					return false;
					#endregion
				}
				else
				{
					System.Diagnostics.Trace.WriteLine("ERROR[94]: invalid state");
					return false;
				}
				if( i==dataLength )
				{
					//End of buffer
					break;
				}
			}
			#region Handle the end of the buffer
			if(state.actualArea == State_XMLfile.Area.InTopLevelWaitingForRootElement)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.WaitingForTagName)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.InTagName)
			{
				#region InTagName: read tag name characters until 'space', '>' or '/>'
				if(state.indexBeginName<0)state.indexBeginName=0;
				int i = buffer.Length;
				int nameLength = i-state.indexBeginName;
				if(nameLength>0)
				{
					//name is not null
					if(!state.closingTag)
					{
						state.actualTagElement.Tag +=
							new string(buffer, state.indexBeginName, nameLength);
					}
					else
					{
						//TODO: verify if the buffer has the needed space
						for(int j=0; j<nameLength; j++)
							state.nameBuffer[state.indexNameBuffer++] =	buffer[state.indexBeginName+j];
					}
				}
				state.indexBeginName = -1;
				#endregion
			}
			else if(state.actualArea == State_XMLfile.Area.WaitingForTagClose)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeName)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeSpaceOrEnd)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.InAttributeName)
			{
				#region InAttributeName: waiting for 'space' or '='
				if(state.indexBeginName<0)state.indexBeginName=0;
				int i = buffer.Length;
				int nameLength = i-state.indexBeginName;
				if(nameLength>0)
				{
					//name is not null
					//TODO: store the attribute name
				}
				state.indexBeginName = -1;
				#endregion
			}
			else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeEqual)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.WaitingForAttributeValue)
			{
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.InAttributeValueString)
			{
				#region InAttributeValueString: waiting for '"' (\" allowed?)
				if(state.indexBeginName<0)state.indexBeginName=0;
				int i = buffer.Length;
				int textLength = i-state.indexBeginName;
				if(textLength>0)
				{
					//TODO: store the text
				}
				state.indexBeginName = -1;
				#endregion
			}
			else if(state.actualArea == State_XMLfile.Area.InAttributeValueNumber)
			{
				#region InAttributeValueNumber: waiting for 'space' or '>'
				if(state.indexBeginName<0)state.indexBeginName=0;
				int i = buffer.Length;
				int valueLength = i-state.indexBeginName;
				if(valueLength>0)
				{
					//TODO: store the value
				}
				state.indexBeginName = -1;
				#endregion
			}
			else if(state.actualArea == State_XMLfile.Area.InTagContents)
			{
				//TODO: store the spaces in case of <tag>     Text  </tag>
				//nothing to do
			}
			else if(state.actualArea == State_XMLfile.Area.InText)
			{
				#region Waiting for </end>
				if(state.indexBeginName<0)state.indexBeginName=0;
				int i = buffer.Length;
				int textLength = i-state.indexBeginName;
				if(textLength>0)
				{
					//TODO: store the text
				}
				state.indexBeginName = -1;
				#endregion
			}
			else if(state.actualArea == State_XMLfile.Area.InSpecialTagI)
			{
				//TODO: check the contents of the '?' tag
				//nothing to do for now
			}
			else if(state.actualArea == State_XMLfile.Area.InSpecialTagE)
			{
				//TODO: check the contents of the '!' tag
				//nothing to do for now
			}
			else if(state.actualArea == State_XMLfile.Area.InSpecialTagC)
			{
				System.Diagnostics.Trace.WriteLine("ERROR[64]: state not implemented");
				return false;
			}
			else
			{
				System.Diagnostics.Trace.WriteLine("ERROR[69]: invalid state");
				return false;
			}
			#endregion
			if(state.indexBeginName>=0)
			{
				//The End Of Buffer treatment failed
				System.Diagnostics.Trace.WriteLine("ERROR[75]: internal error, some data was lost");
				return false;
			}
			return true;
		}
		
		void Button1Click(object sender, System.EventArgs e) //Button Read XML
		{
			loadXML();
		}
	}

	public class State_XMLfile
	{
		public const int MAX_LEVELS = 20;
		public const int MAX_CHARS = 250;
		public bool[] actualTagHasAttributes;
		public bool[] actualTagHasChildren;
		public bool[] actualTagHasText;
		//public string[] actualTagName;
		public char[] nameBuffer;
		public bool closingTag;
		public int indexNameBuffer;
		public int level;
		public int indexBeginName;
		public Area actualArea;
		public PlatCPL.XML2 root;
		public PlatCPL.XML2 actualTagElement;
		
		public State_XMLfile()
		{
			reset();
		}
		public void reset()
		{
			actualTagHasAttributes = new bool[MAX_LEVELS];
			actualTagHasChildren = new bool[MAX_LEVELS];
			actualTagHasText = new bool[MAX_LEVELS];
			//actualTagName = new string[MAX_LEVELS];
			nameBuffer = new char[MAX_CHARS];
			level = -1;
			indexBeginName = -1;
			actualArea = Area.InTopLevelWaitingForRootElement;
			closingTag = false;
			root = new PlatCPL.XML2("ROOT","");
			actualTagElement = root;
		}
		
		public enum Area
		{
			InTopLevelWaitingForRootElement,
			WaitingForTagName,
			WaitingForTagClose,
			InTagName,
			WaitingForAttributeName, //after some spaces, //may be a "\n>"
			WaitingForAttributeSpaceOrEnd,
			InAttributeName,
			WaitingForAttributeEqual, // if after attribute name comes a 'space'
			WaitingForAttributeValue, // " or 'number'
			InAttributeValueString,
			InAttributeValueNumber,
			InTagContents, //Waiting for "text" or <child> or </end>
			InText, //waiting for </end>
			InSpecialTagI, // '<?xxx' tag, waiting for '?>'
			InSpecialTagE, // '<!xxx' tag, waiting for '>'
			InSpecialTagC, // '<!--' tag, waiting for '-->'
		}
	}
}
