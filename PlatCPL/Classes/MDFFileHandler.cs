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
		public MDFFileHandler()
		{
		}
		
		public bool realMdfFile(string filePath)
		{
			System.IO.FileStream file = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			IDBLOCK temp1 = new MDFFileHandler.IDBLOCK();
			bool res = temp1.readFile(file);
			BLOCK temp2 = new MDFFileHandler.BLOCK();
			res = temp2.readFile(file);
			return false;
		}
		
		public class BLOCK
		{
			public Int32 address = -1; // address of this block
			public string blockId = "HD";
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
			public virtual bool parseData()
			{
				return false;
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
				this.data = new byte[64];
				try
				{
					this.address = (int)file.Position;
					if(file.Read(data, 0, 64) != 64) return false;
					return parseData();
				}
				catch(Exception)
				{
					return false;
				}
			}
			public override bool parseData()
			{
				if( data.Length != 64 )return false;
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
		}
		public class HDBLOCK : BLOCK
		{
			public new string blockId = "HD";
			public new UInt16 blockSize = 0xA4;
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
		}
		public class TXBLOCK : BLOCK
		{
			public new string blockId = "TX";
			public new UInt16 blockSize = 0;
			public string text = ""; // new line indicated by CR and LF; end of text indicated by 0
		}
		public class PRBLOCK : BLOCK
		{
			public new string blockId = "PR";
			public new UInt16 blockSize = 0;
			public string text = ""; //end of text indicated by the size of the block
		}
		public class DGBLOCK : BLOCK
		{
			public new string blockId = "DG";
			public new UInt16 blockSize = 0x1C;
			public Int32 linkDGBLOCK = 0; // next data group block, null allowed
			public Int32 linkCGBLOCK = 0; // next channel group block, null allowed
			public Int32 linkReserved1 = 0; // reserved
			public Int32 linkDataRecords = 0; // data records, null allowed
			public UInt16 numChannelGroups = 0; // Number of channel groups
			public UInt16 numRecordIds = 0; // Number of record IDs in the data block (0, 1 ou 2)
			public UInt32 reserved2 = 0;
		}
		public class CGBLOCK : BLOCK
		{
			public new string blockId = "CG";
			public new UInt16 blockSize = 0x1A;
			public Int32 linkCGBLOCK = 0; // next data channel group block, null allowed
			public Int32 linkCNBLOCK = 0; // first channel block, null allowed
			public Int32 linkTXBLOCK = 0; // channel group comment text, null allowed
			public UInt16 recordId = 0; // record id
			public UInt16 numChannels = 0; // Number of channels
			public UInt16 recordSize = 0; // Data record size in bytes for each sample (without the record ID)
			public UInt32 numRecords = 0; // Number of records
		}
		public class CNBLOCK : BLOCK
		{
			public new string blockId = "CN";
			public new UInt16 blockSize = 0xDE;
			public Int32 linkCNBLOCK = 0; // next channel block of this channel group, null allowed
			public Int32 linkCCBLOCK = 0; // conversion formula of this signal, null allowed
			public Int32 linkReserved1 = 0; // reserved
			public Int32 linkReserved2 = 0; // reserved
			public Int32 linkTXBLOCK = 0; // channel comment of this signal, null allowed
			public UInt16 channelType = 0; // 0=data, 1=time
			public string signalName = "[xx.10.xx][xx.20.xx][xx.30.xx]12"; // 32 char
			public string signalDescription = ""; // 128 char
			public UInt16 numFirstBits = 0;
			public UInt16 numBits = 0;
			public UInt16 dataType = 2; // 2,3 = IEEE 754 floating-point format
			public UInt16 boolKnownImp = 0; // Value range – known implementation value
			public double minImpVal = 0; // Value range – minimum implementation value
			public double maxImpVal = 0; // Value range – maximum implementation value
			public double sampleRate = 0; // Rate in which the variable was sampled. Unit [s]
			public Int32 linkTXname = 0; // A2L signal name, null allowed
			public Int32 linkTXdisplayName = 0; // signal display name, null allowed
			public UInt16 byteOffset = 0; // default value: 0
		}
		public class CCBLOCK : BLOCK
		{
			public new string blockId = "CC";
			public new UInt16 blockSize = 0;
			public UInt16 boolKnownPhy = 0; // Value range – known physical value
			public double minPhyVal = 0; // Value range – minimum physical value
			public double maxPhyVal = 0; // Value range – maximum physical value
			public string physUnit = "[xx.10.xx][xx.20.xx]"; // 20 char
			public UInt16 convFormType = 0; // Conversion formula identifier
			public UInt16 numPar = 0; // number of parameters
		}
	}
}
