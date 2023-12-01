
namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

public class CompositionsWithTypes
{
	public Enums.NodeType Type { get; set; }
	public string ContentTypeAlias { get; set; }

	public IEnumerable<IContentTypeComposition> Compositions { get; set; }

	public CompositionsWithTypes()
	{
		Compositions = new List<IContentTypeComposition>();
	}
}

