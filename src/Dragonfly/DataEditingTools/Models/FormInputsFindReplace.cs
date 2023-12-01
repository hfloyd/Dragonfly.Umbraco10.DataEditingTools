namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FormInputsFindReplace
{
	public string ContentNodeIdsCsv { get; set; }

	public Enums.FindReplaceType FindReplaceTypeOption { get; set; }

	public string CustomTransformationClass { get; set; }
	public string Find { get; set; }
	public string Replace { get; set; }

	public bool PreviewOnly { get; set; }
	public bool FullPropertyReplace { get; set; }

	public bool FullPropertyIsMultiple { get; set; }
	public string PropertyAliasesCsv { get; set; }
}
