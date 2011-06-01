/*
 * Created by SharpDevelop.
 * User: pm05724
 * Date: 1/6/2011
 * Time: 11:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using PlatCPL.Interfaces;
namespace PlatCPL.Classes
{
	/// <summary>
	/// Description of WorkedData.
	/// </summary>
	public class WorkedData
	{
		List<double> data;
		List<double> time;
		string labelName;
		public WorkedData( List<double> time , string labelName)
		{
			this.data = new List<double>();
			this.time = time;
			this.labelName = labelName;
		}
		
		public bool CreateData( VariableInfo vi  )
		{
			if ( vi == null )
			{
				return false;
			}
			else if ( vi.data == null )
			{
				return false;
			}
			else
			{
				for ( int i = 0 ; i < this.time.Count ; i++ )
				{
					this.data.Add( vi.GetInterpVal( this.time[ i ] ) );
				}
				return true;
			}
		}
		
		public string LabelName
		{
			get { return this.labelName; }
		}
		
		public List<double> Data
		{
			get { return this.data; }
		}
		
	}
}
