namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;

public interface ICustomFindReplaceTransformer : IDiscoverable
{
	/// <summary>
	/// Allows your custom Transformer to test that the provided property is valid for conversion with this Transformer
	/// </summary>
	/// <param name="ContentType"></param>
	/// <param name="DataType"></param>
	/// <returns></returns>
	bool IsValidForData(int NodeId, string ContentTypeAlias, IPropertyType PropType, object OriginalData, out string NotValidReasonMsg);

	/// <summary>
	/// Does the Conversion
	/// </summary>
	/// <param name="OriginalData"></param>
	/// <returns></returns>
	object ConvertOriginalData(object OriginalData, out string ConversionErrorMsg);
}

