/*
 * Criado por SharpDevelop.
 * Usuário: 
 * Data: 26/12/2008
 * Hora: 14:48
 * 
 * Para alterar este modelo use Ferramentas | Opções | Codificação | Editar Cabeçalhos Padrão.
 */

namespace PlatCPL
{
	/// <summary>
	/// Description of GerenciamentoBancoDados.
	/// </summary>
	public class XML2
	{
		//Class based on Chilkat XML class: http://www.chilkatsoft.com/
		public string Tag;
		public string Content;
		public XML2[] criancas;
		public XML2[] atributos;
		public XML2 pai = null;
		//private InterfaceadorV2 comm;
		
		public XML2(/*InterfaceadorV2 comunicador*/)
		{
			//comm = comunicador;
			Tag = "";
			Content = "";
			criancas = new XML2[0];
			atributos = new XML2[0];
		}
		public XML2(/*InterfaceadorV2 comunicador,*/ string nome, string conteudo)
		{
			//comm = comunicador;
			Tag = nome;
			if(conteudo!=null)Content = conteudo;
			else Content = "";
			criancas = new XML2[0];
			atributos = new XML2[0];
		}
		public XML2(/*InterfaceadorV2 comunicador,*/ System.Xml.XmlNode no)
		{
			//comm = comunicador;
			carregarNo(no);
		}

		public void SaveXml(string nomeArquivo)
		{
			SaveXml(nomeArquivo, System.Text.Encoding.Default);
		}
		public void SaveXml(string nomeArquivo, System.Text.Encoding encod)
		{
			if(Tag.Length==0)Tag="XML2";
			System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
			settings.Indent = true;
			settings.Encoding = encod;
			settings.IndentChars = ("    ");
			System.Xml.XmlWriter arquivo = System.Xml.XmlWriter.Create(nomeArquivo, settings);
			arquivo.WriteStartDocument();
			salvarElemento(arquivo);
			arquivo.WriteEndDocument();
			arquivo.Close();
		}
		private void salvarElemento(System.Xml.XmlWriter arquivo)
		{
			arquivo.WriteStartElement(Tag);
			foreach(XML2 atrib in atributos)
			{
				try{arquivo.WriteAttributeString(atrib.Tag,atrib.Content);}
				catch(System.Exception e)
				{
					//comm.porNoLog(e.Message);
					System.Diagnostics.Trace.WriteLine(e.Message);
				}
			}
			if(criancas.Length>0)
			{
				foreach(XML2 crianca in criancas) crianca.salvarElemento(arquivo);
			}
			else arquivo.WriteString(Content);
			arquivo.WriteEndElement();
		}
		public bool LoadXmlFile(string nomeArquivo)
		{
			Tag = "";
			Content = "";
			criancas = new XML2[0];
			atributos = new XML2[0];
			System.Xml.XmlDocument conteudoXml = new System.Xml.XmlDocument();
			try { conteudoXml.Load(nomeArquivo); }
			catch(System.Exception e)
			{
				//comm.porNoLog("ERRO: não foi possível carregar o arquivo "+nomeArquivo+" => "+e.Message);
				System.Diagnostics.Trace.WriteLine("ERRO: não foi possível carregar o arquivo "+nomeArquivo+" => "+e.Message);
				return false;
			}
			//foreach(System.Xml.XmlNode cri in conteudoXml.ChildNodes)comm.porNoLog(cri.Name);
			if(conteudoXml.ChildNodes.Count==2&&conteudoXml.ChildNodes[0].Name=="xml")
			{
				carregarNo(conteudoXml.ChildNodes[1]);
				return true;
			}
			else if(conteudoXml.ChildNodes.Count==3&&conteudoXml.ChildNodes[0].Name=="xml"&&conteudoXml.ChildNodes[1].Name=="session"&&conteudoXml.ChildNodes[2].Name=="session")
			{
				carregarNo(conteudoXml.ChildNodes[2]);
				return true;
			}
			else
			{
				//comm.porNoLog("ERRO: arquivo XML em formato desconhecido.");
				System.Diagnostics.Trace.WriteLine("ERRO: arquivo XML em formato desconhecido.");
				return false;
			}
		}
		private void carregarNo(System.Xml.XmlNode no)
		{
			this.Tag = no.Name;
			atributos = new XML2[no.Attributes.Count];
			for(int i=0; i<no.Attributes.Count; i++)
			{
				atributos[i] = new XML2(/*comm,*/ no.Attributes[i].Name, no.Attributes[i].Value);
				atributos[i].pai = this;
			}
			if(no.ChildNodes.Count==1&&no.ChildNodes[0].Name=="#text")
			{
				criancas = new XML2[0];
				this.Content = no.InnerText;
			}
			else
			{
				this.Content = "";
				criancas = new XML2[no.ChildNodes.Count];
				for(int i=0; i<no.ChildNodes.Count; i++)
				{
					criancas[i] = new XML2(/*comm,*/ no.ChildNodes[i]);
					criancas[i].pai = this;
				}
			}
		}
		
		public int NumChildren{ get{ return criancas.Length; } }
		public int NumAttributes{ get{ return atributos.Length; } }

		public bool HasChildWithTag(string nome)
		{
			foreach(XML2 crianca in criancas)
			{
				if(crianca.Tag == nome)return true;
			}
			return false;
		}
		public string GetChildContent(string nome)
		{
			foreach(XML2 crianca in criancas)
			{
				if(crianca.Tag == nome)
				{
					return crianca.Content;
				}
			}
			return null;
		}
		public XML2 FindChild(string nome)
		{
			return GetChildWithTag(nome);
		}
		public XML2 GetChild(int posicao)
		{
			if(posicao>=0&&posicao<criancas.Length)return criancas[posicao];
			return null;
		}
		public XML2 GetChildWithTag(string nome)
		{
			foreach(XML2 crianca in criancas)
			{
				if(crianca.Tag == nome)
				{
					return crianca;
				}
			}
			return null;
		}
		public void RemoveChild(int index)
		{
			int j=index;
			for(int i=index+1;i<criancas.Length;i++)
			{
				criancas[j++]=criancas[i];
			}
			if(j==criancas.Length-1)System.Array.Resize<XML2>(ref criancas,j);
		}
		public void RemoveChild(string nome)
		{
			int j=0;
			int i=0;
			for(;i<criancas.Length;i++)
			{
				if(i==j)
				{
					if(criancas[i].Tag!=nome)j++;
				}
				else criancas[j++]=criancas[i];
			}
			if(j==criancas.Length-1)System.Array.Resize<XML2>(ref criancas,j);
		}
		public void RemoveAttribute(string nome)
		{
			int j=0;
			int i=0;
			for(;i<atributos.Length;i++)
			{
				if(i==j)
				{
					if(atributos[i].Tag!=nome)j++;
				}
				else atributos[j++]=atributos[i];
			}
			if(j==atributos.Length-1)System.Array.Resize<XML2>(ref atributos,j);
		}
		public void UpdateChildContent(string nome, string conteudo)
		{
			foreach(XML2 crianca in criancas)
			{
				if(crianca.Tag == nome)
				{
					crianca.Content = conteudo;
				}
			}
		}
		public void UpdateChildContentInt(string nome, int conteudo)
		{
			UpdateChildContent(nome, conteudo.ToString());
		}
		public int GetChildIntValue(string nome)
		{
			foreach(XML2 crianca in criancas)
			{
				if(crianca.Tag == nome)
				{
					try { return int.Parse(crianca.Content); }
					catch { break; }
				}
			}
			return 0;
		}
		public string GetAttrValue(string nome)
		{
			foreach(XML2 atributo in atributos)
			{
				if(atributo.Tag == nome)
				{
					return atributo.Content;
				}
			}
			return null;
		}
		public bool UpdateAttrValue(string nome, string novoValor)
		{
			foreach(XML2 atributo in atributos)
			{
				if(atributo.Tag == nome)
				{
					atributo.Content = novoValor;
					return true;
				}
			}
			return false;
		}
		public string GetAttributeName(int posicao)
		{
			if(posicao>0&&posicao<atributos.Length)return atributos[posicao].Tag;
			return null;
		}
		public string GetAttributeValue(int posicao)
		{
			if(posicao>0&&posicao<atributos.Length)return atributos[posicao].Content;
			return null;
		}
		public XML2 NewChild(string nome, string conteudo)
		{
			System.Array.Resize<XML2>(ref criancas, criancas.Length+1);
			criancas[criancas.Length-1] = new XML2(/*comm,*/ nome, conteudo);
			criancas[criancas.Length-1].pai = this;
			return criancas[criancas.Length-1];
		}
		public bool NewChild(XML2 crianca)
		{
			if(crianca==null)return false;
			System.Array.Resize<XML2>(ref criancas, criancas.Length+1);
			criancas[criancas.Length-1] = crianca;
			criancas[criancas.Length-1].pai = this;
			return true;
		}
		public void NewChildInt2(string nome, int conteudo)
		{
			NewChild(nome, conteudo.ToString());
		}
		public void AddAttribute(string atributo, string valor)
		{
			System.Array.Resize<XML2>(ref atributos, atributos.Length+1);
			atributos[atributos.Length-1] = new XML2(/*comm,*/ atributo, valor);
			atributos[atributos.Length-1].pai = this;
		}
		public void AddAttributeInt(string atributo, int valor)
		{
			AddAttribute(atributo, valor.ToString());
		}
	}
}
