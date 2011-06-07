/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 31/05/2011
 * Hora: 18:28
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;

namespace PlatCPL.Classes
{
	/// <summary>
	/// Description of MDFFileHandler.
	/// </summary>
	public class MDFFileHandler
	{
		public System.Collections.Generic.List<Link> links;
		public MDFFileHandler()
		{
			links = null;
		}
		
		public bool readMdfFile(string filePath)
		{
			links = new System.Collections.Generic.List<MDFFileHandler.Link>();

			System.IO.FileStream file = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			
			IDBLOCK identBlock = new MDFFileHandler.IDBLOCK();
			if(identBlock.readFile(file)&&identBlock.parseData(links))
			{
				links.Add(new Link(0, identBlock));
				links.Add(new Link(64,"HD")); //HDBLOCK
				BLOCK newBlock;
				bool addedBlock = true;
				do
				{
					addedBlock = false;
					for(int i=0; i<links.Count; i++)
					{
						if( links[i].address==file.Position && links[i].block==null )
						{
							if(links[i].blockType=="DATA")
							{
								newBlock = new DATABLOCK((DGBLOCK)links[i].referenceBlock);
								if(!((DATABLOCK)newBlock).DGblock.resolveCGLink(links))
									break;
							}
							else newBlock = new BLOCK();
							if(newBlock.readFile(file))
							{
								newBlock = newBlock.getParsedBlock(links);
								if(newBlock!=null)
								{
									if(newBlock.blockId==links[i].blockType)
									{
										links[i].block = newBlock;
										addedBlock = true;
									}
								}
								else break;
							}
							else break;
						}
					}
				}
				while(addedBlock);
				if(file.Position==file.Length)
				{
					file.Close();
					HDBLOCK headerBlock = (HDBLOCK)links[1].block;
					bool res = headerBlock.resolveLinks(links);
					return res;
				}
			}
			file.Close();
			return false;
		}
		public bool fillMdfData(System.Collections.Generic.List<WorkedData> workedDataList, System.Collections.Generic.List<double> time)
		{
			links = new System.Collections.Generic.List<MDFFileHandler.Link>();
			int currentAddress = 0;
			
			IDBLOCK identBlock = new MDFFileHandler.IDBLOCK();
			identBlock.fillData(currentAddress);
			links.Add(new Link(currentAddress, identBlock));
			currentAddress += identBlock.blockSize;
			
			HDBLOCK headerBlock = new MDFFileHandler.HDBLOCK();
			headerBlock.fillData(currentAddress, 1, System.DateTime.Now.ToString("dd:MM:yyyy"), System.DateTime.Now.ToString("HH:mm:ss"),
			                     "Author: PlatCPL", "Organization: PlatCPL", "Project: PlatCPL", "Measure: PlatCPL");
			links.Add(new Link(currentAddress, headerBlock));
			currentAddress += headerBlock.blockSize;
			
			TXBLOCK textBlock = new MDFFileHandler.TXBLOCK();
			textBlock.fillData(currentAddress, "-");
			links.Add(new Link(currentAddress, textBlock));
			currentAddress += textBlock.blockSize;
			headerBlock.linkTXBLOCK = textBlock.address;
			
			DGBLOCK dataGBlock = new MDFFileHandler.DGBLOCK();
			dataGBlock.fillData(currentAddress, 1);
			links.Add(new Link(currentAddress, dataGBlock));
			currentAddress += dataGBlock.blockSize;
			headerBlock.linkDGBLOCK = dataGBlock.address;
			
			CGBLOCK chanGBlock = new MDFFileHandler.CGBLOCK();
			chanGBlock.fillData(currentAddress, 0, (ushort)(workedDataList.Count+1), (ushort)(8*(workedDataList.Count+1)), (uint)(time.Count));
			links.Add(new Link(currentAddress, chanGBlock));
			currentAddress += chanGBlock.blockSize;
			dataGBlock.linkCGBLOCK = chanGBlock.address;

			//time channel
			CNBLOCK channBlock = new MDFFileHandler.CNBLOCK();
			double sampleRate = (time[time.Count-1]-time[0])/time.Count;
			channBlock.fillData(currentAddress, 1, "time", 0, 8*8, 3, sampleRate);
			links.Add(new Link(currentAddress, channBlock));
			currentAddress += channBlock.blockSize;
			chanGBlock.linkCNBLOCK = channBlock.address;
			CNBLOCK lastChannBlock = channBlock;

			CCBLOCK convBlock = new MDFFileHandler.CCBLOCK();
			convBlock.fillData(currentAddress, 1, time[0], time[time.Count-1], "s", 65535, 0, new byte[0]);
			links.Add(new Link(currentAddress, convBlock));
			currentAddress += convBlock.blockSize;
			channBlock.linkCCBLOCK = convBlock.address;
			
			//test second time channel (as data)
			/*channBlock = new MDFFileHandler.CNBLOCK();
			channBlock.fillData(currentAddress, 0, "time2", 8*8, 2, sampleRate);
			links.Add(new Link(currentAddress, channBlock));
			currentAddress += channBlock.blockSize;
			lastChannBlock.linkCNBLOCK = channBlock.address;
			lastChannBlock = channBlock;

			convBlock = new MDFFileHandler.CCBLOCK();
			convBlock.fillData(currentAddress, 1, time[0], time[time.Count-1], "s", 65535, 0, new byte[0]);
			links.Add(new Link(currentAddress, convBlock));
			currentAddress += convBlock.blockSize;
			channBlock.linkCCBLOCK = convBlock.address;*/
			
			string signalName;
			ushort firstBit = 8*8;
			for(int i=0; i<workedDataList.Count; i++)
			{
				channBlock = new MDFFileHandler.CNBLOCK();
				signalName = workedDataList[i].LabelName;
				if(signalName.Length>32) signalName = signalName.Substring(signalName.Length-32);
				channBlock.fillData(currentAddress, 0, signalName, firstBit, 8*8, 3, sampleRate);
				firstBit += 8*8;
				links.Add(new Link(currentAddress, channBlock));
				currentAddress += channBlock.blockSize;
				lastChannBlock.linkCNBLOCK = channBlock.address;
				lastChannBlock = channBlock;
				
				convBlock = new MDFFileHandler.CCBLOCK();
				//convBlock.fillData(currentAddress, 1, workedDataList[i].minVal, workedDataList[i].maxVal, "U"+i, 0, 0, new byte[]{22,86,231,158,175,3,210,60,  22,86,231,158,175,3,210,60});//0,0,0,0,0,0,240,63});
				//convBlock.fillData(currentAddress, 1, workedDataList[i].minVal, workedDataList[i].maxVal, "U"+i, 65535, 0, new byte[]{});
				convBlock.fillData(currentAddress, 0, 0, 0, "U"+i, 0, 2, new byte[]{0,0,0,0,0,0,0,128,   0,0,0,0,0,0,240,63});
				links.Add(new Link(currentAddress, convBlock));
				currentAddress += convBlock.blockSize;
				channBlock.linkCCBLOCK = convBlock.address;
			}
			
			DATABLOCK dataBlock = new MDFFileHandler.DATABLOCK(dataGBlock);
			dataBlock.fillUnParsedData(currentAddress, workedDataList, time);
			links.Add(new Link(currentAddress, dataBlock));
			currentAddress += dataBlock.blockSize;
			dataGBlock.linkDataRecords = dataBlock.address;
			
			return true;
		}
		public bool exportToMdfFile(string filePath)
		{
			for(int i=0; i<links.Count; i++)
			{
				if(links[i].block is IDBLOCK) ((IDBLOCK)links[i].block).unParseData();
				else if(links[i].block is HDBLOCK) ((HDBLOCK)links[i].block).unParseData();
				else if(links[i].block is TXBLOCK) ((TXBLOCK)links[i].block).unParseData();
				else if(links[i].block is PRBLOCK) ((PRBLOCK)links[i].block).unParseData();
				else if(links[i].block is DGBLOCK) ((DGBLOCK)links[i].block).unParseData();
				else if(links[i].block is CGBLOCK) ((CGBLOCK)links[i].block).unParseData();
				else if(links[i].block is CNBLOCK) ((CNBLOCK)links[i].block).unParseData();
				else if(links[i].block is CCBLOCK) ((CCBLOCK)links[i].block).unParseData();
				else if(links[i].block is DATABLOCK) { }
				else return false;
			}
			System.IO.FileStream fileOutput = null;
			if ( filePath != null && filePath.Length > 0 )
			{
				try
				{
					fileOutput = new System.IO.FileStream( filePath, System.IO.FileMode.Create );
				}
				catch
				{
					//error
				}
			}
			
			if( fileOutput != null )
			{
				try
				{
					for(int i=0; i<links.Count; i++) fileOutput.Write(links[i].block.data, 0, links[i].block.data.Length);
					fileOutput.Close();
					return true;
				}
				catch(Exception)
				{
					fileOutput.Close();
				}
			}
			return false;
		}
		
		public class BLOCK
		{
			public Int32 address = -1; // address of this block
			public string blockId = "--";
			public UInt16 blockSize = 0;
			public byte[] data;

			public virtual bool readFile(System.IO.FileStream file)
			{
				this.data = new byte[4];
				try
				{
					this.address = (int)file.Position;
					if(file.Read(data, 0, 4) != 4) return false;
					this.blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					this.blockSize = BitConverter.ToUInt16(data, 2);
					Array.Resize<byte>(ref data, blockSize);
					if(file.Read(data, 4, data.Length-4) != data.Length-4) return false;
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public virtual bool parseData(System.Collections.Generic.List<Link> links)
			{
				return false;
			}
			public virtual BLOCK getParsedBlock(System.Collections.Generic.List<Link> links)
			{
				BLOCK temp = null;
				if(this.blockId=="HD")temp = new MDFFileHandler.HDBLOCK();
				if(this.blockId=="TX")temp = new MDFFileHandler.TXBLOCK();
				if(this.blockId=="PR")temp = new MDFFileHandler.PRBLOCK();
				if(this.blockId=="DG")temp = new MDFFileHandler.DGBLOCK();
				if(this.blockId=="CG")temp = new MDFFileHandler.CGBLOCK();
				if(this.blockId=="CN")temp = new MDFFileHandler.CNBLOCK();
				if(this.blockId=="CC")temp = new MDFFileHandler.CCBLOCK();
				if(temp!=null)
				{
					temp.address = this.address;
					temp.blockId = this.blockId;
					temp.blockSize = this.blockSize;
					temp.blockSize = this.blockSize;
					temp.blockSize = this.blockSize;
					temp.data = this.data;
					if(temp.parseData(links))return temp;
				}
				return null;
			}
		}
		public class IDBLOCK : BLOCK
		{
			public string fileId     = "MDF     ";                       // 8 char
			public string version    = "3.00    ";                       // 8 char
			public string programId  = "PlatCPL ";                       // 8 char
			public UInt16 endianType = 0;                                // 0 = little endian
			public UInt16 floatType  = 0;                                // 0 = IEEE 754
			public UInt16 versionN   = 300;                              // version number
			public UInt16 reserved1  = 0;                                // value not defined, picking 0
			public string reserved2  = "xx";                             // 2 char, value not defined
			public string reserved3  = "[xx.10.xx][xx.20.xx][xx.30.xx]"; // 30 char, value not defined

			public override bool readFile(System.IO.FileStream file)
			{
				if( file.Position != 0 ) return false;
				this.blockSize = 64;
				this.blockId = "ID";
				this.data = new byte[this.blockSize];
				try
				{
					this.address = (int)file.Position;
					if(file.Read(data, 0, this.blockSize) != this.blockSize) return false;
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					fileId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 8));
					version = new string(System.Text.Encoding.ASCII.GetChars(data, 8, 8));
					programId = new string(System.Text.Encoding.ASCII.GetChars(data, 16, 8));
					endianType = BitConverter.ToUInt16(data, 24);
					floatType = BitConverter.ToUInt16(data, 26);
					versionN = BitConverter.ToUInt16(data, 28);
					reserved1 = BitConverter.ToUInt16(data, 30);
					reserved2 = new string(System.Text.Encoding.ASCII.GetChars(data, 32, 2));
					reserved3 = new string(System.Text.Encoding.ASCII.GetChars(data, 34, 30));
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool fillData(Int32 currentAddress)
			{
				address = currentAddress; // address of this block
				blockId = "ID";
				blockSize = 64;
				
				fileId     = "MDF     ";                       // 8 char
				version    = "3.00    ";                       // 8 char
				programId  = "PlatCPL ";                       // 8 char
				endianType = 0;                                // 0 = little endian
				floatType  = 0;                                // 0 = IEEE 754
				versionN   = 300;                              // version number
				reserved1  = 0;                                // value not defined, picking 0
				reserved2  = "\0\0";                             // 2 char, value not defined
				reserved3  = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0"; // 30 char, value not defined

				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(fileId).CopyTo(data, 0);
					System.Text.Encoding.ASCII.GetBytes(version).CopyTo(data, 8);
					System.Text.Encoding.ASCII.GetBytes(programId).CopyTo(data, 16);
					BitConverter.GetBytes(endianType).CopyTo(data, 24);
					BitConverter.GetBytes(floatType).CopyTo(data, 26);
					BitConverter.GetBytes(versionN).CopyTo(data, 28);
					BitConverter.GetBytes(reserved1).CopyTo(data, 30);
					System.Text.Encoding.ASCII.GetBytes(reserved2).CopyTo(data, 32);
					System.Text.Encoding.ASCII.GetBytes(reserved3).CopyTo(data, 34);
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		public class HDBLOCK : BLOCK
		{
			//public string blockId = "HD";
			//public UInt16 blockSize = 0xA4;
			public Int32 linkDGBLOCK = 0; // first file group block
			public Int32 linkTXBLOCK = 0; // measurement file comment text, null allowed
			public Int32 linkPRBLOCK = 0; // program block, null allowed
			public UInt16 nDataGroups = 0; //number of data groups
			public string recordStartDate = "DD:MM:YYYY"; // 10 char
			public string recordStartTime = "HH:MM:SS"; // 8 char
			public string author       = "[xx.10.xx][xx.20.xx][xx.30.xx]12"; // 32 char
			public string organization = "[xx.10.xx][xx.20.xx][xx.30.xx]12"; // 32 char
			public string project      = "[xx.10.xx][xx.20.xx][xx.30.xx]12"; // 32 char
			public string measureId    = "[xx.10.xx][xx.20.xx][xx.30.xx]12"; // 32 char

			public DGBLOCK DGblock = null;
			public TXBLOCK TXblock = null;
			public PRBLOCK PRblock = null;
			
			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					linkDGBLOCK = BitConverter.ToInt32(data, 4);
					linkTXBLOCK = BitConverter.ToInt32(data, 8);
					linkPRBLOCK = BitConverter.ToInt32(data, 12);
					nDataGroups = BitConverter.ToUInt16(data, 16);
					recordStartDate = new string(System.Text.Encoding.ASCII.GetChars(data, 18, 10));
					recordStartTime = new string(System.Text.Encoding.ASCII.GetChars(data, 28, 8));
					author = new string(System.Text.Encoding.ASCII.GetChars(data, 36, 32));
					organization = new string(System.Text.Encoding.ASCII.GetChars(data, 68, 32));
					project = new string(System.Text.Encoding.ASCII.GetChars(data, 100, 32));
					measureId = new string(System.Text.Encoding.ASCII.GetChars(data, 132, 32));

					if(this.blockId != "HD") return false;
					if(this.blockSize != 0xA4) return false;
					if(this.address != 64) return false;

					if(linkDGBLOCK!=0)links.Add(new Link(linkDGBLOCK,"DG"));
					if(linkTXBLOCK!=0)links.Add(new Link(linkTXBLOCK,"TX"));
					if(linkPRBLOCK!=0)links.Add(new Link(linkPRBLOCK,"PR"));
					
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool fillData(Int32 currentAddress, UInt16 nnDataGroups, string nrecordStartDate, string nrecordStartTime, string nauthor, string norganization, string nproject, string nmeasureId)
			{
				address = currentAddress; // address of this block
				blockId = "HD";
				blockSize = 0xA4;
				
				linkDGBLOCK = 0; // first file group block
				linkTXBLOCK = 0; // measurement file comment text, null allowed
				linkPRBLOCK = 0; // program block, null allowed
				nDataGroups = nnDataGroups; //number of data groups
				recordStartDate = nrecordStartDate; // 10 char
				recordStartTime = nrecordStartTime; // 8 char
				author       = nauthor; // 32 char
				organization = norganization; // 32 char
				project      = nproject; // 32 char
				measureId    = nmeasureId; // 32 char

				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					BitConverter.GetBytes(linkDGBLOCK).CopyTo(data, 4);
					BitConverter.GetBytes(linkTXBLOCK).CopyTo(data, 8);
					BitConverter.GetBytes(linkPRBLOCK).CopyTo(data, 12);
					BitConverter.GetBytes(nDataGroups).CopyTo(data, 16);
					System.Text.Encoding.ASCII.GetBytes(recordStartDate).CopyTo(data, 18);
					System.Text.Encoding.ASCII.GetBytes(recordStartTime).CopyTo(data, 28);
					System.Text.Encoding.ASCII.GetBytes(author).CopyTo(data, 36);
					for(int i=(36+author.Length); i<(36+32); i++)data[i]=0;
					System.Text.Encoding.ASCII.GetBytes(organization).CopyTo(data, 68);
					for(int i=(68+organization.Length); i<(68+32); i++)data[i]=0;
					System.Text.Encoding.ASCII.GetBytes(project).CopyTo(data, 100);
					for(int i=(100+project.Length); i<(100+32); i++)data[i]=0;
					System.Text.Encoding.ASCII.GetBytes(measureId).CopyTo(data, 132);
					for(int i=(132+measureId.Length); i<(132+32); i++)data[i]=0;
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool resolveLinks(System.Collections.Generic.List<Link> links)
			{
				foreach(Link link in links)
				{
					if(link.address==linkDGBLOCK && link.blockType=="DG") DGblock = (DGBLOCK)link.block;
					if(link.address==linkTXBLOCK && link.blockType=="TX") TXblock = (TXBLOCK)link.block;
					if(link.address==linkPRBLOCK && link.blockType=="PR") PRblock = (PRBLOCK)link.block;
				}
				if(DGblock==null)return false;
				return DGblock.resolveLinks(links);
			}
		}
		public class TXBLOCK : BLOCK
		{
			//public string blockId = "TX";
			
			public string text = ""; // new line indicated by CR and LF; end of text indicated by 0

			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					text = new string(System.Text.Encoding.ASCII.GetChars(data, 4, data.Length-4));

					if(this.blockId != "TX") return false;

					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool fillData(Int32 currentAddress, string newText)
			{
				address = currentAddress; // address of this block
				blockId = "TX";
				
				text = newText;

				blockSize = (UInt16)(2 + 2 + newText.Length + 1);
				
				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					System.Text.Encoding.ASCII.GetBytes(text).CopyTo(data, 4);
					for(int i=(2+2+text.Length); i<data.Length; i++)data[i]=0;
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		public class PRBLOCK : BLOCK
		{
			//public string blockId = "PR";
			
			public string text = ""; //end of text indicated by the size of the block

			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					text = new string(System.Text.Encoding.ASCII.GetChars(data, 4, data.Length-4));

					if(this.blockId != "PR") return false;

					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool fillData(Int32 currentAddress, string newText)
			{
				address = currentAddress; // address of this block
				blockId = "PR";
				
				text = newText;

				blockSize = (UInt16)(2 + 2 + newText.Length + 1);
				
				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					System.Text.Encoding.ASCII.GetBytes(text).CopyTo(data, 4);
					for(int i=(2+2+text.Length); i<data.Length; i++)data[i]=0;
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		public class DGBLOCK : BLOCK
		{
			//public string blockId = "DG";
			//public UInt16 blockSize = 0x1C;
			public Int32 linkDGBLOCK = 0; // next data group block, null allowed
			public Int32 linkCGBLOCK = 0; // next channel group block, null allowed
			public Int32 linkReserved1 = 0; // reserved
			public Int32 linkDataRecords = 0; // data records, null allowed
			public UInt16 numChannelGroups = 0; // Number of channel groups
			public UInt16 numRecordIds = 0; // Number of record IDs in the data block (0, 1 ou 2)
			public UInt32 reserved2 = 0;
			
			public CGBLOCK CGblock = null;
			public DATABLOCK DATAblock = null;

			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					linkDGBLOCK = BitConverter.ToInt32(data, 4);
					linkCGBLOCK = BitConverter.ToInt32(data, 8);
					linkReserved1 = BitConverter.ToInt32(data, 12);
					linkDataRecords = BitConverter.ToInt32(data, 16);
					numChannelGroups = BitConverter.ToUInt16(data, 20);
					numRecordIds = BitConverter.ToUInt16(data, 22);
					reserved2 = BitConverter.ToUInt32(data, 24);

					if(this.blockId != "DG") return false;
					if(this.blockSize != 0x1C) return false;

					if(linkDGBLOCK!=0)links.Add(new Link(linkDGBLOCK,"DG"));
					if(linkCGBLOCK!=0)links.Add(new Link(linkCGBLOCK,"CG"));
					if(linkDataRecords!=0)links.Add(new Link(linkDataRecords,"DATA", this));
					
					if(linkCGBLOCK>linkDataRecords)
						return false;
					
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool resolveCGLink(System.Collections.Generic.List<Link> links)
			{
				foreach(Link link in links)
				{
					if(link.address==linkCGBLOCK && link.blockType=="CG") CGblock = (CGBLOCK)link.block;
					if(link.address==linkDataRecords && link.blockType=="DATA") DATAblock = (DATABLOCK)link.block;
				}
				if(CGblock==null)return false;
				return true;
				//return CGblock.resolveLinks(links);
			}
			public bool resolveLinks(System.Collections.Generic.List<Link> links)
			{
				if(resolveCGLink(links))return CGblock.resolveLinks(links);
				else return false;
			}
			public bool fillData(Int32 currentAddress, UInt16 numberOfChannelGroups)
			{
				address = currentAddress; // address of this block
				blockId = "DG";
				blockSize = 0x1C;
				
				linkDGBLOCK = 0; // next data group block, null allowed
				linkCGBLOCK = 0; // next channel group block, null allowed
				linkReserved1 = 0; // reserved
				linkDataRecords = 0; // data records, null allowed
				numChannelGroups = numberOfChannelGroups; // Number of channel groups
				numRecordIds = 0; // Number of record IDs in the data block (0, 1 ou 2)
				reserved2 = 0;
				
				CGblock = null;

				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					BitConverter.GetBytes(linkDGBLOCK).CopyTo(data, 4);
					BitConverter.GetBytes(linkCGBLOCK).CopyTo(data, 8);
					BitConverter.GetBytes(linkReserved1).CopyTo(data, 12);
					BitConverter.GetBytes(linkDataRecords).CopyTo(data, 16);
					BitConverter.GetBytes(numChannelGroups).CopyTo(data, 20);
					BitConverter.GetBytes(numRecordIds).CopyTo(data, 22);
					BitConverter.GetBytes(reserved2).CopyTo(data, 24);
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		public class CGBLOCK : BLOCK
		{
			//public string blockId = "CG";
			//public UInt16 blockSize = 0x1A;
			public Int32 linkCGBLOCK = 0; // next data channel group block, null allowed
			public Int32 linkCNBLOCK = 0; // first channel block, null allowed
			public Int32 linkTXBLOCK = 0; // channel group comment text, null allowed
			public UInt16 recordId = 0; // record id
			public UInt16 numChannels = 0; // Number of channels
			public UInt16 recordSize = 0; // Data record size in bytes for each sample (without the record ID)
			public UInt32 numRecords = 0; // Number of records

			public CNBLOCK CNblock = null;
			
			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					linkCGBLOCK = BitConverter.ToInt32(data, 4);
					linkCNBLOCK = BitConverter.ToInt32(data, 8);
					linkTXBLOCK = BitConverter.ToInt32(data, 12);
					recordId = BitConverter.ToUInt16(data, 16);
					numChannels = BitConverter.ToUInt16(data, 18);
					recordSize = BitConverter.ToUInt16(data, 20);
					numRecords = BitConverter.ToUInt32(data, 22);

					if(this.blockId != "CG") return false;
					if(this.blockSize != 0x1A) return false;

					if(linkCGBLOCK!=0)links.Add(new Link(linkCGBLOCK,"CG"));
					if(linkCNBLOCK!=0)links.Add(new Link(linkCNBLOCK,"CN"));
					if(linkTXBLOCK!=0)links.Add(new Link(linkTXBLOCK,"TX"));
					
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool resolveLinks(System.Collections.Generic.List<Link> links)
			{
				foreach(Link link in links)
				{
					if(link.address==linkCNBLOCK && link.blockType=="CN")
					{
						CNblock = (CNBLOCK)link.block;
						break;
					}
				}
				if(CNblock==null)return false;
				return CNblock.resolveLinks(links, numChannels-1);
			}
			public bool fillData(Int32 currentAddress, UInt16 nrecordId, UInt16 nnumChannels, UInt16 nrecordSize, UInt32 nnumRecords)
			{
				address = currentAddress; // address of this block
				blockId = "CG";
				blockSize = 0x1A;
				
				linkCGBLOCK = 0; // next data channel group block, null allowed
				linkCNBLOCK = 0; // first channel block, null allowed
				linkTXBLOCK = 0; // channel group comment text, null allowed
				recordId = nrecordId; // record id
				numChannels = nnumChannels; // Number of channels
				recordSize = nrecordSize; // Data record size in bytes for each sample (without the record ID)
				numRecords = nnumRecords; // Number of records

				CNblock = null;

				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					BitConverter.GetBytes(linkCGBLOCK).CopyTo(data, 4);
					BitConverter.GetBytes(linkCNBLOCK).CopyTo(data, 8);
					BitConverter.GetBytes(linkTXBLOCK).CopyTo(data, 12);
					BitConverter.GetBytes(recordId).CopyTo(data, 16);
					BitConverter.GetBytes(numChannels).CopyTo(data, 18);
					BitConverter.GetBytes(recordSize).CopyTo(data, 20);
					BitConverter.GetBytes(numRecords).CopyTo(data, 22);
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		public class CNBLOCK : BLOCK
		{
			//public string blockId = "CN";
			//public UInt16 blockSize = 0xE4;
			public Int32 linkCNBLOCK = 0; // next channel block of this channel group, null allowed
			public Int32 linkCCBLOCK = 0; // conversion formula of this signal, null allowed
			public Int32 linkReserved1 = 0; // reserved
			public Int32 linkReserved2 = 0; // reserved
			public Int32 linkTXBLOCK = 0; // channel comment of this signal, null allowed
			public UInt16 channelType = 0; // 0=data, 1=time
			public string signalName = "[xx.10.xx][xx.20.xx][xx.30.xx]12"; // 32 char
			public string signalDescription = ""; // 128 char
			public UInt16 numFirstBit = 0;
			public UInt16 numBits = 0;
			public UInt16 dataType = 2; // 2,3 = IEEE 754 floating-point format
			public UInt16 boolKnownImp = 0; // Value range – known implementation value
			public double minImpVal = 0; // Value range – minimum implementation value
			public double maxImpVal = 0; // Value range – maximum implementation value
			public double sampleRate = 0; // Rate in which the variable was sampled. Unit [s]
			public Int32 linkTXname = 0; // A2L signal name, null allowed
			public Int32 linkTXdisplayName = 0; // signal display name, null allowed
			public UInt16 byteOffset = 0; // default value: 0

			public CNBLOCK CNblock = null;
			public CCBLOCK CCblock = null;

			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					linkCNBLOCK = BitConverter.ToInt32(data, 4);
					linkCCBLOCK = BitConverter.ToInt32(data, 8);
					linkReserved1 = BitConverter.ToInt32(data, 12);
					linkReserved2 = BitConverter.ToInt32(data, 16);
					linkTXBLOCK = BitConverter.ToInt32(data, 20);
					channelType = BitConverter.ToUInt16(data, 24);
					signalName = new string(System.Text.Encoding.ASCII.GetChars(data, 26, 32));
					signalDescription = new string(System.Text.Encoding.ASCII.GetChars(data, 58, 128));
					numFirstBit = BitConverter.ToUInt16(data, 186);
					numBits = BitConverter.ToUInt16(data, 188);
					dataType = BitConverter.ToUInt16(data, 190);
					boolKnownImp = BitConverter.ToUInt16(data, 192);
					minImpVal = BitConverter.ToDouble(data, 194);
					maxImpVal = BitConverter.ToDouble(data, 202);
					sampleRate = BitConverter.ToDouble(data, 210);
					linkTXname = BitConverter.ToInt32(data, 218);
					linkTXdisplayName = BitConverter.ToInt32(data, 222);
					byteOffset = BitConverter.ToUInt16(data, 226);

					if(this.blockId != "CN") return false;
					if(this.blockSize != 0xE4) return false;

					if(linkCNBLOCK!=0)links.Add(new Link(linkCNBLOCK,"CN"));
					if(linkCCBLOCK!=0)links.Add(new Link(linkCCBLOCK,"CC"));
					if(linkTXBLOCK!=0)links.Add(new Link(linkTXBLOCK,"TX"));
					if(linkTXname!=0)links.Add(new Link(linkTXname,"TX"));
					if(linkTXdisplayName!=0)links.Add(new Link(linkTXdisplayName,"TX"));
					
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool resolveLinks(System.Collections.Generic.List<Link> links, int remainingCNs)
			{
				if( linkCNBLOCK==0 || remainingCNs==0 )
				{
					if( linkCNBLOCK==0 && remainingCNs==0 ) return true;
					else return false;
				}
				foreach(Link link in links)
				{
					if(link.address==linkCNBLOCK && link.blockType=="CN") CNblock = (CNBLOCK)link.block;
					if(link.address==linkCCBLOCK && link.blockType=="CC") CCblock = (CCBLOCK)link.block;
				}
				if(CNblock==null)return false;
				return CNblock.resolveLinks(links, remainingCNs-1);
			}
			public bool fillData(Int32 currentAddress, UInt16 nchannelType, string nsignalName, UInt16 nnumFirstBit, UInt16 nnumBits, UInt16 ndataType, double nsampleRate)
			{
				address = currentAddress; // address of this block
				blockId = "CN";
				blockSize = 0xE4;
				
				linkCNBLOCK = 0; // next channel block of this channel group, null allowed
				linkCCBLOCK = 0; // conversion formula of this signal, null allowed
				linkReserved1 = 0; // reserved
				linkReserved2 = 0; // reserved
				linkTXBLOCK = 0; // channel comment of this signal, null allowed
				channelType = nchannelType; // 0=data, 1=time
				signalName = nsignalName; // 32 char
				signalDescription = ""; // 128 char
				numFirstBit = nnumFirstBit;
				numBits = nnumBits;
				dataType = ndataType; // 2,3 = IEEE 754 floating-point format
				boolKnownImp = 0; // Value range – known implementation value
				minImpVal = 0; // Value range – minimum implementation value
				maxImpVal = 0; // Value range – maximum implementation value
				sampleRate = nsampleRate; // Rate in which the variable was sampled. Unit [s]
				linkTXname = 0; // A2L signal name, null allowed
				linkTXdisplayName = 0; // signal display name, null allowed
				byteOffset = 0; // default value: 0

				CNblock = null;

				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					BitConverter.GetBytes(linkCNBLOCK).CopyTo(data, 4);
					BitConverter.GetBytes(linkCCBLOCK).CopyTo(data, 8);
					BitConverter.GetBytes(linkReserved1).CopyTo(data, 12);
					BitConverter.GetBytes(linkReserved2).CopyTo(data, 16);
					BitConverter.GetBytes(linkTXBLOCK).CopyTo(data, 20);
					BitConverter.GetBytes(channelType).CopyTo(data, 24);
					System.Text.Encoding.ASCII.GetBytes(signalName).CopyTo(data, 26);
					for(int i=(26+signalName.Length); i<(26+32); i++)data[i]=0;
					System.Text.Encoding.ASCII.GetBytes(signalDescription).CopyTo(data, 58);
					for(int i=(58+signalDescription.Length); i<(58+128); i++)data[i]=0;
					BitConverter.GetBytes(numFirstBit).CopyTo(data, 186);
					BitConverter.GetBytes(numBits).CopyTo(data, 188);
					BitConverter.GetBytes(dataType).CopyTo(data, 190);
					BitConverter.GetBytes(boolKnownImp).CopyTo(data, 192);
					BitConverter.GetBytes(minImpVal).CopyTo(data, 194);
					BitConverter.GetBytes(maxImpVal).CopyTo(data, 202);
					BitConverter.GetBytes(sampleRate).CopyTo(data, 210);
					BitConverter.GetBytes(linkTXname).CopyTo(data, 218);
					BitConverter.GetBytes(linkTXdisplayName).CopyTo(data, 222);
					BitConverter.GetBytes(byteOffset).CopyTo(data, 226);
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		public class CCBLOCK : BLOCK
		{
			//public string blockId = "CC";
			
			public UInt16 boolKnownPhy = 0; // Value range – known physical value
			public double minPhyVal = 0; // Value range – minimum physical value
			public double maxPhyVal = 0; // Value range – maximum physical value
			public string physUnit = "[xx.10.xx][xx.20.xx]"; // 20 char
			public UInt16 convFormType = 0; // Conversion formula identifier
			public UInt16 numPar = 0; // number of parameters
			public byte[] parameters = new byte[0];

			public override bool parseData(System.Collections.Generic.List<Link> links)
			{
				if( data.Length != this.blockSize )return false;
				try
				{
					blockId = new string(System.Text.Encoding.ASCII.GetChars(data, 0, 2));
					blockSize = BitConverter.ToUInt16(data, 2);
					if( data.Length != this.blockSize )return false;
					boolKnownPhy = BitConverter.ToUInt16(data, 4);
					minPhyVal = BitConverter.ToDouble(data, 6);
					maxPhyVal = BitConverter.ToDouble(data, 14);
					physUnit = new string(System.Text.Encoding.ASCII.GetChars(data, 22, 20));
					convFormType = BitConverter.ToUInt16(data, 42);
					numPar = BitConverter.ToUInt16(data, 44);
					parameters = new byte[data.Length-46];
					for(int i=0; i<parameters.Length; i++) parameters[i] = data[46+i];

					if(this.blockId != "CC") return false;

					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public bool fillData(Int32 currentAddress, UInt16 nboolKnownPhy, double nminPhyVal, double nmaxPhyVal, string nphysUnit, UInt16 nconvFormType, UInt16 nnumPar, byte[] nparameters)
			{
				address = currentAddress; // address of this block
				blockId = "CC";
				blockSize = (ushort)(46 + nparameters.Length);
				
				boolKnownPhy = nboolKnownPhy; // Value range – known physical value
				minPhyVal = nminPhyVal; // Value range – minimum physical value
				maxPhyVal = nmaxPhyVal; // Value range – maximum physical value
				physUnit = nphysUnit; // 20 char
				convFormType = nconvFormType; // Conversion formula identifier
				numPar = nnumPar; // number of parameters
				parameters = nparameters;

				return true;
				//return unParseData();
			}
			public bool unParseData()
			{
				if(data==null)data = new byte[this.blockSize];
				if( data.Length != this.blockSize ) Array.Resize<byte>(ref data, this.blockSize);
				try
				{
					System.Text.Encoding.ASCII.GetBytes(blockId).CopyTo(data, 0);
					BitConverter.GetBytes(blockSize).CopyTo(data, 2);
					BitConverter.GetBytes(boolKnownPhy).CopyTo(data, 4);
					BitConverter.GetBytes(minPhyVal).CopyTo(data, 6);
					BitConverter.GetBytes(maxPhyVal).CopyTo(data, 14);
					System.Text.Encoding.ASCII.GetBytes(physUnit).CopyTo(data, 22);
					for(int i=(22+physUnit.Length); i<(22+20); i++)data[i]=0;
					BitConverter.GetBytes(convFormType).CopyTo(data, 42);
					BitConverter.GetBytes(numPar).CopyTo(data, 44);
					parameters.CopyTo(data, 46);
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}
		
		public class DATABLOCK : BLOCK
		{
			//public string blockId = "DATA";
			public DGBLOCK DGblock;
			public UInt32 dataBlockSize = 0;
			
			public DATABLOCK(DGBLOCK refBlock)
			{
				DGblock = refBlock;
			}
			public override bool readFile(System.IO.FileStream file)
			{
				if( DGblock==null ) return false;
				if( DGblock.numChannelGroups != 1 ) return false;
				if( DGblock.numRecordIds != 0 ) return false;
				if( DGblock.CGblock==null ) return false;
				
				this.blockId = "DATA";
				this.blockSize = 0;
				this.dataBlockSize = DGblock.CGblock.numRecords*DGblock.CGblock.recordSize;
				this.data = new byte[this.dataBlockSize];
				try
				{
					this.address = (int)file.Position;
					if(file.Read(data, 0, data.Length) != data.Length) return false;
					return true;
				}
				catch(Exception)
				{
					return false;
				}
			}
			public override bool parseData(System.Collections.Generic.List<MDFFileHandler.Link> links)
			{
				this.blockId = "DATA";
				return true;
			}
			public override BLOCK getParsedBlock(System.Collections.Generic.List<Link> links)
			{
				if(this.parseData(links))return this;
				return null;
			}
			public bool fillUnParsedData(Int32 currentAddress, System.Collections.Generic.List<WorkedData> workedDataList, System.Collections.Generic.List<double> time)
			{
				address = currentAddress; // address of this block
				blockId = "DATA";
				blockSize = 0;
				UInt32 dataSampleSize = (uint)( (8) * (workedDataList.Count+1) );
				dataBlockSize = (uint)( dataSampleSize * (time.Count) );
				data = new byte[dataBlockSize];
				UInt32 currentPosition = 0;
				int j;
				for(int i=0; i<time.Count; i++)
				{
					BitConverter.GetBytes(time[i]).CopyTo(data, currentPosition);
					currentPosition += 8;
					for(j=0; j<workedDataList.Count; j++)
					{
						BitConverter.GetBytes(workedDataList[j].Data[i]).CopyTo(data, currentPosition);
						currentPosition += 8;
					}
				}
				
				if(currentPosition != dataBlockSize)return false;
				else return true;
			}
			public double getDoubleValue(int dataRecordNumber, int channelNumber)
			{
				int position = DGblock.CGblock.recordSize*dataRecordNumber + 8*channelNumber;
				if(position<data.Length-8) return BitConverter.ToDouble(data, position);
				return 0;
			}
		}
		
		public class Link
		{
			public Int32 address = -1;
			public BLOCK block = null;
			public string blockType = "";
			public BLOCK referenceBlock = null;
			
			public Link(Int32 newLinkAddress, string blockTypeLink)
			{
				address = newLinkAddress;
				block = null;
				blockType = blockTypeLink;
				referenceBlock = null;
			}
			public Link(Int32 newLinkAddress, BLOCK newBlock)
			{
				address = newLinkAddress;
				block = newBlock;
				blockType = newBlock.blockId;
				referenceBlock = null;
			}
			public Link(Int32 newLinkAddress, string blockTypeLink, BLOCK refBlock)
			{
				address = newLinkAddress;
				block = null;
				blockType = blockTypeLink;
				referenceBlock = refBlock;
			}
		}
	}
}
