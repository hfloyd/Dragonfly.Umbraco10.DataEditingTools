namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;


public interface ICustomPropToPropDataTransformer : IDiscoverable
{
	bool IsValidForData(int NodeId, string ContentTypeAlias, IPropertyType FromPropertyType, object FromPropertyData, IPropertyType ToPropertyType, object ToPropertyData, out string NotValidReasonMsg);
	object TransformData(object FromPropData, out string ConversionErrorMsg);
}

