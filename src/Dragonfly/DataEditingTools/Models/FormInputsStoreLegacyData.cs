namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FormInputsStoreLegacyData
{
	public string AvailableContentPropertiesCSV { get; set; }
	//public string DocTypeAlias { get; set; }
	//public string ContentNodeIdsCsv { get; set; }
	public string PropertyAliasContentNodeId { get; set; }

	public string AvailableMediaPropertiesCSV { get; set; }
	public string PropertyAliasMediaNodeId { get; set; }

	public bool PreviewOnly { get; set; }

	public bool OverwriteExistingData { get; set; }

}
