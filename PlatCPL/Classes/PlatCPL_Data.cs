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
		public System.Collections.Generic.List<XML3> children;
		public System.Collections.Generic.List<XML3> attributes;
		//public XML3[] children;
		//public XML3[] attributes;
		public XML3 parent = null;
		private PlatCPL.InterCommunication comm;
		
		public XML3(PlatCPL.InterCommunication parentComm)
		{
			comm = parentComm;
			Tag = "";
			comm.msgDebug(" > new XML3: "+this.Tag);
			Content = "";
			children = new System.Collections.Generic.List<XML3>();
			attributes = new System.Collections.Generic.List<XML3>();
			//children = new XML3[0];
			//attributes = new XML3[0];
		}
		public XML3(string name, string content, PlatCPL.InterCommunication parentComm)
		{
			comm = parentComm;
			Tag = name;
			comm.msgDebug(" > new XML3: "+this.Tag);
			if(content!=null)Content = content;
			else Content = "";
			children = new System.Collections.Generic.List<XML3>();
			attributes = new System.Collections.Generic.List<XML3>();
			//children = new XML3[0];
			//attributes = new XML3[0];
		}
		public XML3(string name, PlatCPL.InterCommunication parentComm)
		{
			comm = parentComm;
			Tag = name;
			comm.msgDebug(" > new XML3: "+this.Tag);
			Content = "";
			children = new System.Collections.Generic.List<XML3>();
			attributes = new System.Collections.Generic.List<XML3>();
			//children = new XML3[0];
			//attributes = new XML3[0];
		}

		#region XML3 file I/O
		public bool LoadXmlFile(string path)
		{
			Tag = "";
			Content = "";
			children = new System.Collections.Generic.List<XML3>();
			attributes = new System.Collections.Generic.List<XML3>();
			//children = new XML3[0];
			//attributes = new XML3[0];
			System.IO.StreamReader fileStream = null;
			try
			{
				fileStream = new System.IO.StreamReader(path, System.Text.Encoding.Default, true);
			}
			catch(System.Exception exc)
			{
				comm.msgError("ERROR[78]: there was a problem opening the XML file\n"+exc.Message);
				fileStream = null;
				return false;
			}
			if(fileStream != null)
			{
				State_XMLfile state = new State_XMLfile(comm);
				char[] buffer = new char[4000]; // page = 4096 bytes
				int bytesRead = fileStream.ReadBlock(buffer, 0, buffer.Length);
				bool resultOK = false;
				while(bytesRead > 0)
				{
					//for(int i=0; i<bytesRead; i++) comm.infoMsg(" - "+(i+1)+") '"+(buffer[i])+"' ["+((int)buffer[i])+"]");
					resultOK = readXMLstream(buffer, bytesRead, state);
					if(resultOK)
						bytesRead = fileStream.ReadBlock(buffer, 0, buffer.Length);
					else
						break;
				}
				fileStream.Close();
				if(resultOK)
				{
					if(state.root.NumChildren==1)
					{
						//case of simple document
						this.Tag = state.root.children[0].Tag;
						this.Content = state.root.children[0].Content;
						this.children = state.root.children[0].children;
						this.attributes = state.root.children[0].attributes;
						return true;
					}
					if(state.root.NumChildren==2 && state.root.children[0].Tag=="")
					{
						//case of <? ...><tag > document
						this.Tag = state.root.children[1].Tag;
						this.Content = state.root.children[1].Content;
						this.children = state.root.children[1].children;
						this.attributes = state.root.children[1].attributes;
						return true;
					}
					comm.msgError("ERROR[18]: xml with an unknown configuration of root elements");
					return false;
				}
				else
				{
					comm.msgError("ERROR[64]: there was a problem parsing the XML file");
					return false;
				}
			}
			comm.msgError("ERROR[27]: there was a problem opening the XML file");
			return false;
		}
		private bool readXMLstream(char[] buffer, int dataLength, State_XMLfile state)
		{
			for(int i=0; i<dataLength; )
			{
				//TODO: HTML tags with no close: <BR>
				if(state.actualAreaDbg == State_XMLfile.Area.InTopLevelWaitingForRootElement)
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
						state.actualAreaDbg = State_XMLfile.Area.WaitingForTagName;
						state.closingTag = false;
						i++;
					}
					else
					{
						comm.msgError("ERROR[80]: invalid character");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForTagName)
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
									comm.msgError("ERROR[13]: tag with text and children");
									return false;
								}
								state.actualTagHasChildren[state.level] = true;
							}
							state.level++;
							if(state.level >= State_XMLfile.MAX_LEVELS)
							{
								comm.msgError("ERROR[83]: XML file too deep");
								return false;
							}
							state.actualTagHasAttributes[state.level] = false;
							state.actualTagHasChildren[state.level] = false;
							state.actualTagHasText[state.level] = false;
							comm.msgDebug(" >[a] '"+state.actualTagElement.Tag+"' gets a new child");
							state.actualTagElement = state.actualTagElement.NewChild("","");
							//state.actualTagName[state.level] = "";
						}
						state.indexBeginName = i;
						state.actualAreaDbg = State_XMLfile.Area.InTagName;
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
							comm.msgError("ERROR[46]: '<?' tag should be in root level");
							return false;
						}
						state.level++;
						state.actualTagHasAttributes[state.level] = false;
						state.actualTagHasChildren[state.level] = false;
						state.actualTagHasText[state.level] = false;
						comm.msgDebug(" >[b] '"+state.actualTagElement.Tag+"' gets a new '?' child");
						state.actualTagElement = state.actualTagElement.NewChild("","");
						state.indexBeginName = -1;
						state.actualAreaDbg = State_XMLfile.Area.InSpecialTagI;
						i++;
					}
					else if(buffer[i] == '!')
					{
						//TODO: accept ! tags
						if(state.level>=0)
						{
							comm.msgError("ERROR[67]: '<!' tag should be in root level");
							return false;
						}
						state.level++;
						state.actualTagHasAttributes[state.level] = false;
						state.actualTagHasChildren[state.level] = false;
						state.actualTagHasText[state.level] = false;
						comm.msgDebug(" >[c] '"+state.actualTagElement.Tag+"' gets a new '!' child");
						state.actualTagElement = state.actualTagElement.NewChild("!","");
						state.indexBeginName = -1;
						state.actualAreaDbg = State_XMLfile.Area.InSpecialTagE;
						i++;
					}
					else
					{
						comm.msgError("ERROR[81]: invalid character for tag name");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InTagName)
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
									comm.msgError("ERROR[61]: invalid tag name");
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
									comm.msgError("ERROR[73]: invalid tag name");
									return false;
								}
							}
							state.indexBeginName = -1;
							if(buffer[i] == '>')
								state.actualAreaDbg = State_XMLfile.Area.InTagContents;
							else if(buffer[i] == '/')
								state.actualAreaDbg = State_XMLfile.Area.WaitingForTagClose;
							else
								state.actualAreaDbg = State_XMLfile.Area.WaitingForAttributeName;
							comm.msgDebug(" >[d] actual tag (level "+state.level+") is: '"+state.actualTagElement.Tag+"'");
							i++;
						}
						else
						{
							comm.msgError("ERROR[86]: invalid character in tag name");
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
									comm.msgError("ERROR[61]: invalid tag name");
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
									comm.msgError("ERROR[73]: invalid tag name");
									return false;
								}
							}
							if(/*state.actualTagName[state.level]*/state.actualTagElement.Tag.Length == state.indexNameBuffer)
							{
								for(int j=0; j<state.actualTagElement.Tag.Length; j++)
								{
									if(state.actualTagElement.Tag[j] != state.nameBuffer[j])
									{
										comm.msgError("ERROR[50]: invalid closing tag name");
										return false;
									}
								}
							}
							else
							{
								comm.msgError("ERROR[50]: invalid closing tag name");
								return false;
							}
							comm.msgDebug(" >[e] finished with tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
							state.indexBeginName = -1;
							state.indexNameBuffer = -1;
							state.level--;
							state.actualTagElement = state.actualTagElement.parent;
							comm.msgDebug(" >[f] going back to tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
							state.closingTag = false;
							if(state.level>=0)state.actualAreaDbg = State_XMLfile.Area.InTagContents;
							else state.actualAreaDbg = State_XMLfile.Area.InTopLevelWaitingForRootElement;
							i++;
						}
						else
						{
							comm.msgError("ERROR[86]: invalid character in tag name");
							return false;
						}
					}
					
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForTagClose)
				{
					#region WaitingForTagClose: next character should be '>'
					// case: <tag attrib="content"/>
					if(buffer[i] == '>')
					{
						state.indexBeginName = -1;
						state.indexNameBuffer = -1;
						state.level--;
						state.actualTagElement = state.actualTagElement.parent;
						comm.msgDebug(" >[g] going back to tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
						state.closingTag = false;
						if(state.level>=0)state.actualAreaDbg = State_XMLfile.Area.InTagContents;
						else state.actualAreaDbg = State_XMLfile.Area.InTopLevelWaitingForRootElement;
						i++;
					}
					else
					{
						comm.msgError("ERROR[47]: invalid character in attributes area");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeName)
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
							comm.msgError("ERROR[99]: invalid character in tag name");
							return false;
						}
						else
						{
							state.indexBeginName = -1;
							state.actualAreaDbg = State_XMLfile.Area.InTagContents;
							i++;
						}
					}
					else if(buffer[i] == '/')
					{
						state.indexBeginName = -1;
						state.actualAreaDbg = State_XMLfile.Area.WaitingForTagClose;
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
						state.actualAreaDbg = State_XMLfile.Area.InAttributeName;
						i++;
					}
					else
					{
						// > STOP or ...
						comm.msgError("ERROR[04]: invalid character for attribute name");
						return false;
						// > CONTINUE
						//state.indexBeginName = i;
						//state.actualTagHasAttributes[state.level] = true;
						//state.actualArea = State_XMLfile.Area.InAttributeName;
						//i++;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeSpaceOrEnd)
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
						state.actualAreaDbg = State_XMLfile.Area.WaitingForAttributeName;
						//i++; it is just a verification, leave the treatment to the WaitingForAttributeName function
					}
					else
					{
						comm.msgError("ERROR[87]: invalid character in attributes area");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InAttributeName)
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
								state.actualAttribute = new XML3(new string(buffer, state.indexBeginName, nameLength), "", comm);
								//TODO: add attribute
							}
							else
							{
								comm.msgError("ERROR[78]: invalid attribute name");
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
						state.actualAreaDbg = State_XMLfile.Area.WaitingForAttributeEqual;
						// i++; not necessary
					}
					else
					{
						comm.msgError("ERROR[68]: invalid character in attribute name");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeEqual)
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
						state.actualAreaDbg = State_XMLfile.Area.WaitingForAttributeValue;
						i++;
					}
					else
					{
						comm.msgError("ERROR[94]: invalid character in tag attributes");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeValue)
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
						state.actualAreaDbg = State_XMLfile.Area.InAttributeValueString;
						i++;
					}
					else if( buffer[i]>='0' && buffer[i]<='9' )
					{
						state.indexBeginName = i;
						state.actualAreaDbg = State_XMLfile.Area.InAttributeValueNumber;
						i++;
					}
					else
					{
						comm.msgError("ERROR[20]: invalid character for attribute value");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InAttributeValueString)
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
						state.actualAreaDbg = State_XMLfile.Area.WaitingForAttributeSpaceOrEnd;
						i++;
					}
					else
					{
						comm.msgError("ERROR[95]: invalid character inside text content of the attribute");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InAttributeValueNumber)
				{
					#region InAttributeValueNumber: waiting for 'space' or '>'
					//TODO: attribute number
					comm.msgError("ERROR[88]: not implemented");
					return false;
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InTagContents)
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
						state.actualAreaDbg = State_XMLfile.Area.WaitingForTagName;
						i++;
					}
					else
					{
						state.indexBeginName = i;
						state.indexNameBuffer = 0;
						state.actualAreaDbg = State_XMLfile.Area.InText;
						state.actualTagHasText[state.level] = true;
						i++;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InText)
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
						//Text finished, next step: store it
						if(state.indexBeginName>=0)
						{
							//The beginning of the text is in the buffer
							int textLength = i-state.indexBeginName;
							if(textLength>0)
							{
								//text is not null
								state.actualTagElement.Content +=
									new string(buffer, state.indexBeginName, textLength);
							}
							else
							{
								//text = ""
								comm.msgError("ERROR[03]: invalid text");
								return false;
							}
						}
						else
						{
							//The beginning was already read in some previous buffer, read the rest now
							if(i>0)
							{
								//Name is not null
								state.actualTagElement.Content += new string(buffer, 0, i);
							}
							else if(state.actualTagElement.Content.Length == 0)
							{
								//text = ""
								comm.msgError("ERROR[18]: invalid text");
								return false;
							}
						}
						state.indexBeginName = -1;
						state.actualAreaDbg = State_XMLfile.Area.WaitingForTagName;
						i++;
					}
					else
					{
						comm.msgError("ERROR[26]: invalid character inside text content of the tag");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InSpecialTagI)
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
						comm.msgDebug(" >[h] finished with tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
						state.actualTagElement = state.actualTagElement.parent;
						state.level--;
						comm.msgDebug(" >[i] going back to tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
						if(state.level>=0)
							return false;
						else
							state.actualAreaDbg = State_XMLfile.Area.InTopLevelWaitingForRootElement;
						i++;
					}
					else
					{
						comm.msgError("ERROR[52]: invalid character inside '?' tag");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InSpecialTagE)
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
						comm.msgDebug(" >[j] finished with tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
						state.actualTagElement = state.actualTagElement.parent;
						state.level--;
						comm.msgDebug(" >[k] going back to tag (level "+state.level+"): '"+state.actualTagElement.Tag+"'");
						if(state.level>=0)
							return false;
						else
							state.actualAreaDbg = State_XMLfile.Area.InTopLevelWaitingForRootElement;
						i++;
					}
					else
					{
						comm.msgError("ERROR[77]: invalid character inside '!' tag");
						return false;
					}
					#endregion
				}
				else if(state.actualAreaDbg == State_XMLfile.Area.InSpecialTagC)
				{
					#region Waiting for -->
					comm.msgError("ERROR[98]: not implemented");
					return false;
					#endregion
				}
				else
				{
					comm.msgError("ERROR[94]: invalid state");
					return false;
				}
				if( i==dataLength )
				{
					//End of buffer
					break;
				}
			}
			#region Handle the end of the buffer
			if(state.actualAreaDbg == State_XMLfile.Area.InTopLevelWaitingForRootElement)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForTagName)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InTagName)
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
			else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForTagClose)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeName)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeSpaceOrEnd)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InAttributeName)
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
			else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeEqual)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.WaitingForAttributeValue)
			{
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InAttributeValueString)
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
			else if(state.actualAreaDbg == State_XMLfile.Area.InAttributeValueNumber)
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
			else if(state.actualAreaDbg == State_XMLfile.Area.InTagContents)
			{
				//TODO: store the spaces in case of <tag>     Text  </tag>
				//nothing to do
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InText)
			{
				#region Waiting for </end>
				if(state.indexBeginName<0)state.indexBeginName=0;
				int i = buffer.Length;
				int textLength = i-state.indexBeginName;
				if(textLength>0)
				{
					//text is not null
					state.actualTagElement.Content +=
						new string(buffer, state.indexBeginName, textLength);
				}
				state.indexBeginName = -1;
				#endregion
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InSpecialTagI)
			{
				//TODO: check the contents of the '?' tag
				//nothing to do for now
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InSpecialTagE)
			{
				//TODO: check the contents of the '!' tag
				//nothing to do for now
			}
			else if(state.actualAreaDbg == State_XMLfile.Area.InSpecialTagC)
			{
				comm.msgError("ERROR[64]: state not implemented");
				return false;
			}
			else
			{
				comm.msgError("ERROR[69]: invalid state");
				return false;
			}
			#endregion
			if(state.indexBeginName>=0)
			{
				//The End Of Buffer treatment failed
				comm.msgError("ERROR[75]: internal error, some data was lost");
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
					comm.msgError(e.Message);
				}
			}
			if(children.Count>0)
			{
				foreach(XML3 crianca in children) crianca.saveElement(file);
			}
			else file.WriteString(Content);
			file.WriteEndElement();
		}
		#endregion
		
		public int NumChildren{ get{ return children.Count; } }
		public int NumAttributes{ get{ return attributes.Count; } }
		
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
			if(index>=0&&index<children.Count)return children[index];
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
			children.RemoveAt(index);
			/*int j=index;
			for( int i=index+1; i<children.Count; i++)
			{
				children[j++]=children[i];
			}
			if(j==children.Length-1)System.Array.Resize<XML3>(ref children,j);*/
		}
		public bool RemoveChild(string name)
		{
			for( int i=0; i<children.Count; i++)
			{
				if(children[i].Tag == name)
				{
					return children.Remove(children[i]);
				}
			}
			return false;
			/*int j=0;
			int i=0;
			for( ; i<children.Length; i++)
			{
				if(i==j)
				{
					if(children[i].Tag!=name)j++;
				}
				else children[j++]=children[i];
			}
			if(j==children.Length-1)System.Array.Resize<XML3>(ref children,j);*/
		}
		public bool RemoveAttribute(string name)
		{
			for( int i=0; i<attributes.Count; i++)
			{
				if(attributes[i].Tag == name)
				{
					return attributes.Remove(attributes[i]);
				}
			}
			return false;
			/*int j=0;
			int i=0;
			for(;i<attributes.Length;i++)
			{
				if(i==j)
				{
					if(attributes[i].Tag!=name)j++;
				}
				else attributes[j++]=attributes[i];
			}
			if(j==attributes.Length-1)System.Array.Resize<XML3>(ref attributes,j);*/
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
			if( index>0 && index<attributes.Count )return attributes[index].Tag;
			return null;
		}
		public string GetAttributeValue(int index)
		{
			if( index>0 && index<attributes.Count )return attributes[index].Content;
			return null;
		}
		public XML3 NewChild(string name, string content)
		{
			XML3 newChild = new XML3(name, content, comm);
			newChild.parent = this;
			children.Add(newChild);
			return newChild;
			/*System.Array.Resize<XML3>(ref children, children.Length+1);
			children[children.Length-1] = new XML3(name, content);
			children[children.Length-1].parent = this;
			return children[children.Length-1];*/
		}
		public bool NewChild(XML3 child)
		{
			if(child==null)return false;
			child.parent = this;
			children.Add(child);
			/*System.Array.Resize<XML3>(ref children, children.Length+1);
			children[children.Length-1] = child;
			children[children.Length-1].parent = this;*/
			return true;
		}
		public void NewChildInt2(string name, int content)
		{
			NewChild(name, content.ToString());
		}
		public bool AddAttribute(string attribute, string attValue)
		{
			XML3 newAttribute = new XML3(attribute, attValue, comm);
			newAttribute.parent = this;
			attributes.Add(newAttribute);
			return true;
			/*System.Array.Resize<XML3>(ref attributes, attributes.Length+1);
			attributes[attributes.Length-1] = new XML3(attribute, attValue);
			attributes[attributes.Length-1].parent = this;*/
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
			public Area actualAreaDbg
			{
				get { return actualArea; }
				set { comm.msgDebug(" new state: "+value); actualArea = value; }
			}
			public PlatCPL.XML3 root;
			public PlatCPL.XML3 actualTagElement;
			public PlatCPL.XML3 actualAttribute;
			public PlatCPL.InterCommunication comm;
			
			public State_XMLfile(InterCommunication parentComm)
			{
				comm = parentComm;
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
				actualAreaDbg = Area.InTopLevelWaitingForRootElement;
				closingTag = false;
				root = new PlatCPL.XML3("ROOT","", comm);
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
