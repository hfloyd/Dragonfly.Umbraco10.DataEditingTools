namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;


public class DocTypeProperty
{
	public string DocTypeAlias { get; set; }
	public string CompositionDocTypeAlias { get; set; }
	public string GroupName { get; set; }

	public IPropertyType Property { get; set; }
}

