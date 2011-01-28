/*
 * Criado por SharpDevelop.
 * Usu�rio:
 * Data: 26/12/2008
 * Hora: 14:48
 * 
 * Para alterar este modelo use Ferramentas | Op��es | Codifica��o | Editar Cabe�alhos Padr�o.
 */

namespace PlatCPL
{
	/// <summary>
	/// Description of XML3.
	/// </summary>
	public class XML3
	{
		//Class based on Chilkat XML class: http://www.chilkatsoft.com/
		public string Tag;
		public string Content;
		public XML3[] children;
		public XML3[] attributes;
		public XML3 parent = null;
		
		public XML3()
		{
			Tag = "";
			Content = "";
			children = new XML3[0];
			attributes = new XML3[0];
		}
		public XML3(string name, string content)
		{
			Tag = name;
			if(content!=null)Content = content;
			else Content = "";
			children = new XML3[0];
			attributes = new XML3[0];
		}

		#region XML3 file I/O
		public bool LoadXmlFile(string path)
		{
			Tag = "";
			Content = "";
			children = new XML3[0];
			attributes = new XML3[0];
			System.IO.StreamReader fileStream = null;
			try
			{
				fileStream = new System.IO.StreamReader(path, System.Text.Encoding.Default, true);
			}
			catch(System.Exception exc)
			{
				System.Diagnostics.Trace.WriteLine(exc.Message);
				fileStream = null;
				return false;
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
				if(resultOK)
				{
					System.Diagnostics.Trace.WriteLine("XML OK!!");
					return true;
				}
				else
				{
					System.Diagnostics.Trace.WriteLine("ERROR[64]: there was a problem reading the XML file");
					return false;
				}
			}
			return false;
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
							state.actualTagElement = state.actualTagElement.parent;
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
						state.actualTagElement = state.actualTagElement.parent;
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
							if(nameLength>0)
							{
								state.actualAttribute = new XML3(new string(buffer, state.indexBeginName, nameLength), "");
								//TODO: add attribute
							}
							else
							{
								System.Diagnostics.Trace.WriteLine("ERROR[78]: invalid attribute name");
								return false;
							}
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
		public void SaveXml(string path)
		{
			SaveXml(path, System.Text.Encoding.Default);
		}
		public void SaveXml(string path, System.Text.Encoding encod)
		{
			if(Tag.Length==0)Tag="XML";
			System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
			settings.Indent = true;
			settings.Encoding = encod;
			settings.IndentChars = ("    ");
			System.Xml.XmlWriter arquivo = System.Xml.XmlWriter.Create(path, settings);
			arquivo.WriteStartDocument();
			saveElement(arquivo);
			arquivo.WriteEndDocument();
			arquivo.Close();
		}
		private void saveElement(System.Xml.XmlWriter file)
		{
			file.WriteStartElement(Tag);
			foreach(XML3 atrib in attributes)
			{
				try{file.WriteAttributeString(atrib.Tag,atrib.Content);}
				catch(System.Exception e)
				{
					//comm.porNoLog(e.Message);
					System.Diagnostics.Trace.WriteLine(e.Message);
				}
			}
			if(children.Length>0)
			{
				foreach(XML3 crianca in children) crianca.saveElement(file);
			}
			else file.WriteString(Content);
			file.WriteEndElement();
		}
		#endregion
		
		public int NumChildren{ get{ return children.Length; } }
		public int NumAttributes{ get{ return attributes.Length; } }
		
		#region XML3 contents modification
		public bool HasChildWithTag(string name)
		{
			foreach(XML3 crianca in children)
			{
				if(crianca.Tag == name)return true;
			}
			return false;
		}
		public string GetChildContent(string name)
		{
			foreach(XML3 crianca in children)
			{
				if(crianca.Tag == name)
				{
					return crianca.Content;
				}
			}
			return null;
		}
		public XML3 FindChild(string name)
		{
			return GetChildWithTag(name);
		}
		public XML3 GetChild(int index)
		{
			if(index>=0&&index<children.Length)return children[index];
			return null;
		}
		public XML3 GetChildWithTag(string name)
		{
			foreach(XML3 crianca in children)
			{
				if(crianca.Tag == name)
				{
					return crianca;
				}
			}
			return null;
		}
		public void RemoveChild(int index)
		{
			int j=index;
			for(int i=index+1;i<children.Length;i++)
			{
				children[j++]=children[i];
			}
			if(j==children.Length-1)System.Array.Resize<XML3>(ref children,j);
		}
		public void RemoveChild(string name)
		{
			int j=0;
			int i=0;
			for(;i<children.Length;i++)
			{
				if(i==j)
				{
					if(children[i].Tag!=name)j++;
				}
				else children[j++]=children[i];
			}
			if(j==children.Length-1)System.Array.Resize<XML3>(ref children,j);
		}
		public void RemoveAttribute(string name)
		{
			int j=0;
			int i=0;
			for(;i<attributes.Length;i++)
			{
				if(i==j)
				{
					if(attributes[i].Tag!=name)j++;
				}
				else attributes[j++]=attributes[i];
			}
			if(j==attributes.Length-1)System.Array.Resize<XML3>(ref attributes,j);
		}
		public void UpdateChildContent(string name, string content)
		{
			foreach(XML3 crianca in children)
			{
				if(crianca.Tag == name)
				{
					crianca.Content = content;
				}
			}
		}
		public void UpdateChildContentInt(string name, int content)
		{
			UpdateChildContent(name, content.ToString());
		}
		public int GetChildIntValue(string name)
		{
			foreach(XML3 crianca in children)
			{
				if(crianca.Tag == name)
				{
					try { return int.Parse(crianca.Content); }
					catch { break; }
				}
			}
			return 0;
		}
		public string GetAttrValue(string name)
		{
			foreach(XML3 atributo in attributes)
			{
				if(atributo.Tag == name)
				{
					return atributo.Content;
				}
			}
			return null;
		}
		public bool UpdateAttrValue(string name, string newValue)
		{
			foreach(XML3 atributo in attributes)
			{
				if(atributo.Tag == name)
				{
					atributo.Content = newValue;
					return true;
				}
			}
			return false;
		}
		public string GetAttributeName(int index)
		{
			if(index>0&&index<attributes.Length)return attributes[index].Tag;
			return null;
		}
		public string GetAttributeValue(int index)
		{
			if(index>0&&index<attributes.Length)return attributes[index].Content;
			return null;
		}
		public XML3 NewChild(string name, string content)
		{
			System.Array.Resize<XML3>(ref children, children.Length+1);
			children[children.Length-1] = new XML3(/*comm,*/ name, content);
			children[children.Length-1].parent = this;
			return children[children.Length-1];
		}
		public bool NewChild(XML3 child)
		{
			if(child==null)return false;
			System.Array.Resize<XML3>(ref children, children.Length+1);
			children[children.Length-1] = child;
			children[children.Length-1].parent = this;
			return true;
		}
		public void NewChildInt2(string name, int content)
		{
			NewChild(name, content.ToString());
		}
		public void AddAttribute(string attribute, string attValue)
		{
			System.Array.Resize<XML3>(ref attributes, attributes.Length+1);
			attributes[attributes.Length-1] = new XML3(/*comm,*/ attribute, attValue);
			attributes[attributes.Length-1].parent = this;
		}
		public void AddAttributeInt(string attribute, int attValue)
		{
			AddAttribute(attribute, attValue.ToString());
		}
		#endregion
		
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
			public PlatCPL.XML3 root;
			public PlatCPL.XML3 actualTagElement;
			public PlatCPL.XML3 actualAttribute;
			
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
				root = new PlatCPL.XML3("ROOT","");
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
}
