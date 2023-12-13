namespace Dragonfly.DataEditingTools.Transformers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dragonfly.DataEditingTools.Models;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

internal class ConvertToMultiUrlPickerValueEditor : ICustomPropToPropDataTransformer
{
	#region CTOR/DI

	private readonly ILogger _logger;
	private readonly ServiceContext _services;

	//public ConvertToNewUrlPicker()
	//{
	//}

	public ConvertToMultiUrlPickerValueEditor(
		ILogger<ConvertToMultiUrlPickerValueEditor> logger,
		ServiceContext services
	)
	{
		_logger = logger;
		_services = services;
	}

	#endregion

	#region Implementation of ICustomPropToPropDataTransformer

	public bool IsValidForData(int NodeId, string ContentTypeAlias,
		IPropertyType FromPropertyType, object FromPropertyData,
		IPropertyType ToPropertyType, object ToPropertyData,
		out string NotValidReasonMsg)
	{
		if (ToPropertyType.PropertyEditorAlias != "Umbraco.MultiUrlPicker")
		{
			NotValidReasonMsg = "ToPropertyType is not using PropertyEditor 'Umbraco.MultiUrlPicker'";
			return false;
		}
		else
		{
			//VALID
			NotValidReasonMsg = "";
			return true;
		}

	}

	public object TransformData(object FromPropData, out string ConversionErrorMsg)
	{
		var newMultiLinkData = new List<MultiUrlPickerLinkModel>();
		ConversionErrorMsg = "";

		var oldLinkDataJson = FromPropData.ToString();

		if (oldLinkDataJson.Contains("\"id\":\""))
		{
			//string Id model
			try
			{
				var gibeJson = oldLinkDataJson.Replace("\"id\":0", "\"id\":\"0\"");
				var oldGibeModelStr = JsonSerializer.Deserialize<GibeLinkPickerModelStringId>(gibeJson);
				if (oldGibeModelStr != null)
				{
					var newLinkData = new MultiUrlPickerLinkModel();
					newLinkData.Name = oldGibeModelStr.Name;
					newLinkData.Target = oldGibeModelStr.Target;

					if (oldGibeModelStr.Id != "" && oldGibeModelStr.Id != "0")
					{
						var idInt = int.Parse(oldGibeModelStr.Id);
						newLinkData.Udi = LookupNodeUdiString(idInt);
					}
					else
					{
						//External URL
						newLinkData.Url = oldGibeModelStr.Url.ToString();
					}

					newMultiLinkData.Add(newLinkData);
				}
				else
				{
					ConversionErrorMsg = "Old Data not in GibeLinkPicker (string) format.";
				}
			}
			catch (Exception e)
			{
				ConversionErrorMsg = $"Unable to deserialize to GibeLinkPicker (string) format: {e.Message}";
			}
		}
		else //if (oldLinkDataJson.Contains())
		{
			//int Id model
			try
			{
				var gibeJson = oldLinkDataJson;
				var oldGibeModelInt = JsonSerializer.Deserialize<GibeLinkPickerModelIntId>(gibeJson);
				if (oldGibeModelInt != null)
				{
					var newLinkData = new MultiUrlPickerLinkModel();
					newLinkData.Name = oldGibeModelInt.Name;
					newLinkData.Target = oldGibeModelInt.Target;

					if (oldGibeModelInt.Id != 0)
					{
						newLinkData.Udi = LookupNodeUdiString(oldGibeModelInt.Id);
					}
					else
					{
						//External URL
						newLinkData.Url = oldGibeModelInt.Url.ToString();
					}

					newMultiLinkData.Add(newLinkData);
				}
				else
				{
					ConversionErrorMsg = "Old Data not in GibeLinkPicker (int) format.";
				}
			}
			catch (Exception e)
			{
				ConversionErrorMsg = $"Unable to deserialize to GibeLinkPicker (int) format: {e.Message}";
			}
		}


		return JsonSerializer.Serialize(newMultiLinkData);
	}

	#endregion


	private string LookupNodeUdiString(int IdInt)
	{
		//Look for Content
		var matchingContent = _services.ContentService.GetById(IdInt);
		if (matchingContent != null)
		{
			return matchingContent.GetUdi().ToString();
		}
		else
		{
			//Look for Media
			var matchingMedia = _services.MediaService.GetById(IdInt);
			if (matchingMedia != null)
			{
				return matchingMedia.GetUdi().ToString();
			}
			else
			{
				return "";
			}
		}

	}
}

internal class MultiUrlPickerLinkModel
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("target")]
	public string Target { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("url")]
	public string Url { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("udi")]
	public string Udi { get; set; }
}

internal class GibeLinkPickerModelStringId
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("url")]
	public Uri Url { get; set; }

	[JsonPropertyName("target")]
	public string Target { get; set; }

	[JsonPropertyName("hashtarget")]
	public string Hashtarget { get; set; }
}

internal class GibeLinkPickerModelIntId
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("url")]
	public Uri Url { get; set; }

	[JsonPropertyName("target")]
	public string Target { get; set; }

	[JsonPropertyName("hashtarget")]
	public string Hashtarget { get; set; }
}




