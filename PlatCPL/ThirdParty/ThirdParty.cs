/*
 * Criado por SharpDevelop.
 * Usuário: Langoni
 * Data: 13/03/2011
 * Hora: 11:26
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */
using System;
//using System.Reflection;
//using System.Collections.Generic;

namespace PlatCPL
{
	/// <summary>
	/// Description of ThirdParty.
	/// </summary>
	public class ThirdParty
	{
		/// <summary>
		/// Method to populate a list with all the class
		/// in the namespace provided by the user
		/// </summary>
		/// <param name="nameSpace">The namespace the user wants searched</param>
		/// <returns></returns>
		public static System.Collections.Generic.List<string> GetAllClasses(string nameSpace)
		{
			//create an Assembly and use its GetExecutingAssembly Method
			//http://msdn2.microsoft.com/en-us/library/system.reflection.assembly.getexecutingassembly.aspx
			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
			//create a list for the namespaces
			System.Collections.Generic.List<string> namespaceList = new System.Collections.Generic.List<string>();
			//create a list that will hold all the classes
			//the suplied namespace is executing
			System.Collections.Generic.List<string> returnList = new System.Collections.Generic.List<string>();
			//loop through all the "Types" in the Assembly using
			//the GetType method:
			//http://msdn2.microsoft.com/en-us/library/system.reflection.assembly.gettypes.aspx
			foreach (Type type in asm.GetTypes())
			{
				if (type.Namespace == nameSpace)
					namespaceList.Add(type.Name);
			}
			//now loop through all the classes returned above and add
			//them to our classesName list
			foreach (String className in namespaceList)
				returnList.Add(className);
			//return the list
			return returnList;
		}
	}
}
