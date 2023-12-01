namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

public static partial class Enums
{
	public enum FindReplaceType
	{
		TextToText,
		IntsToUdis,
		CustomTransformation
	}

	public static Dictionary<FindReplaceType, string> FindReplaceTypesWithDisplayText()
	{
		var dict = new Dictionary<FindReplaceType, string>();

		dict.Add(FindReplaceType.TextToText, "Simple Text-to-Text");
		dict.Add(FindReplaceType.IntsToUdis, "Replace Integer Ids with UDIs");
		dict.Add(FindReplaceType.CustomTransformation, "Custom Transformation (select)");
		return dict;
	}

	public static IEnumerable<SelectListItem> FindReplaceTypesSelectList()
	{
		var dict = FindReplaceTypesWithDisplayText();
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

public class FindReplaceResultsSet
{
	public Enums.FindReplaceType Type { get; set; }
	public FormInputsFindReplace FormInputs { get; set; }

	public List<FindReplaceResult> Results { get; set; }
	public IEnumerable<string> PropertyAliases { get; set; }
	public bool DataUpdatedAndSaved { get; set; }

	public bool HasError { get; set; }
	public string ErrorMessage { get; set; }

	public FindReplaceResultsSet()
	{
		Results = new List<FindReplaceResult>();
	}
}



public class FindReplaceResult
{
	public IContent ContentNode { get; set; }

	public string PropertyAlias { get; set; }

	public object OriginalPropertyData { get; set; }

	public object NewPropertyData { get; set; }
	public bool DataFormatIsNotValidForReplace { get; set; }
	public bool MatchFound { get; set; }
	public bool ContentUpdated { get; set; }

	public Guid Key { get; set; }
	public string CurrentDataFormat { get; set; }
	public string Status { get; set; }
	public List<string> FindStrings { get; set; }
	public List<string> ReplaceStrings { get; set; }
	public OperationResult SaveOnlyResult { get; set; }
	public PublishResult SavePublishResult { get; set; }
	public FindReplaceResult()
	{
		Key = Guid.NewGuid();
		FindStrings = new List<string>();
		ReplaceStrings = new List<string>();
	}
}

