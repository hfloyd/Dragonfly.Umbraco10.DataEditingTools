namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FormInputsPropertyToProperty
{
	public Enums.PropToPropType PropToPropTypeOption { get; set; }
	public string CustomTransformationClass { get; set; }
	public string AvailablePropertiesCSV { get; set; }
	public Enums.NodeType NodeTypes { get; set; }
	public string TypeAlias { get; set; }
	public string ContentNodeIdsCsv { get; set; }
	public string PropertyAliasFrom { get; set; }
	public string PropertyAliasTo { get; set; }

	public bool PreviewOnly { get; set; }

	public bool OverwriteExistingData { get; set; }

}
