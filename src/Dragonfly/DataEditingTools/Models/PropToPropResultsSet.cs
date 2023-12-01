namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;


public static partial class Enums
{
	public enum PropToPropType
	{
		DirectCopy,
		IntsToUdis,
		CustomTransformation
	}

	public static Dictionary<PropToPropType, string> PropToPropTypesWithDisplayText()
	{
		var dict = new Dictionary<PropToPropType, string>();

		dict.Add(PropToPropType.DirectCopy, "Direct Copy");
		dict.Add(PropToPropType.IntsToUdis, "Transform Integer Ids To UDIs");
		dict.Add(PropToPropType.CustomTransformation, "Custom Transformation (select)");
		return dict;
	}

	public static IEnumerable<SelectListItem> PropToPropTypesSelectList()
	{
		var dict = PropToPropTypesWithDisplayText();
		var options = dict.Select(d => new SelectListItem
		{
			Value = d.Key.ToString(),
			Text = d.Value.ToString()
		});

		//if default option needed... (string DefaultValue, string DefaultText)
		//if (!string.IsNullOrEmpty(DefaultValue))
		//{
		//    var defaultSelect = Enumerable.Repeat(new SelectListItem
		//    {
		//        Value = DefaultId,
		//        Text = DefaultText
		//    }, count: 1);

		//    return defaultSelect.Concat(options);
		//}

		return options;
	}

}
public class PropToPropResultsSet
{
	public Enums.PropToPropType Type { get; set; }
	public FormInputsPropertyToProperty FormInputs { get; set; }

	public List<PropToPropResult> Results { get; set; }
	public IEnumerable<string> PropertyAliases { get; set; }
	public bool DataUpdatedAndSaved { get; set; }
	public bool HasError { get; set; }
	public string ErrorMessage { get; set; }

	public PropToPropResultsSet()
	{
		Results = new List<PropToPropResult>();
	}
}

public class PropToPropResult
{
	public IContent ContentNode { get; set; }

	public string PropertyFromAlias { get; set; }

	public object PropertyFromData { get; set; }

	public string PropertyFromDataFormat { get; set; }
	public string PropertyToAlias { get; set; }
	public object PropertyToData { get; set; }
	public string PropertyToDataFormat { get; set; }
	public bool DataFormatIsNotValidForTransfer { get; set; }
	public bool ValidToTransfer { get; set; }
	public bool ContentUpdated { get; set; }
	public string Status { get; set; }
	public Guid Key { get; set; }
	public OperationResult SaveOnlyResult { get; set; }
	public PublishResult SavePublishResult { get; set; }
	public IMedia MediaNode { get; set; }

	public PropToPropResult()
	{
		Key = Guid.NewGuid();
	}
}

