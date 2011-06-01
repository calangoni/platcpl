/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 10/04/2011
 * Hora: 9:40
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;

namespace PlatCPL.Classes
{
	/// <summary>
	/// Description of PhoneContacts.
	/// </summary>
	public class PhoneContact
	{
		public string name;
		public string category;
		public System.Collections.Generic.List<ContactNumberInfo> numbers;
		public XML3 otherFields;
		private PlatCPL.InterCommunication comm;
		
		public PhoneContact(string contactName, PlatCPL.InterCommunication parentComm)
		{
			comm = parentComm;
			name = contactName;
			category = "";
			numbers = new System.Collections.Generic.List<ContactNumberInfo>();
			otherFields = new XML3(C.PB.OtherFields, "", comm);
		}
		
		public bool addNumber(string number)
		{
			return addNumber(number, "");
		}
		public bool addNumber(string number, string type)
		{
			ContactNumberInfo newNumber = new ContactNumberInfo(number, type);
			numbers.Add(newNumber);
			return true;
		}
		public bool addPersField(string fieldName, string fieldContent)
		{
			otherFields.NewChild(fieldName, fieldContent);
			return true;
		}
	}
	public class ContactNumberInfo
	{
		public string number;
		public string type;
		public string name;
		
		public ContactNumberInfo(string contactNumber)
		{
			number = contactNumber;
			type = "";
			name = "";
		}
		public ContactNumberInfo(string contactNumber, string numberType)
		{
			number = contactNumber;
			type = numberType;
			name = "";
		}
	}
}
