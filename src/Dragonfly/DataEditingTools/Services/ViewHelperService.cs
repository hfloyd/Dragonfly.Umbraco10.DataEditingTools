namespace Dragonfly.DataEditingTools
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	using Dragonfly.DataEditingTools.Models;
	using Dragonfly.NetHelpers;
	using Dragonfly.NetHelperServices;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Html;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc.Rendering;
	using Microsoft.Extensions.Logging;
	using Umbraco.Cms.Core;
	using Umbraco.Cms.Core.Models;
	using Umbraco.Cms.Core.Services;
	using Umbraco.Cms.Core.Web;
	using Umbraco.Cms.Web.Common;


	public class ViewHelperService
	{
		#region CTOR /DI

		private readonly UmbracoHelper _umbracoHelper;
		private readonly ILogger _logger;

		private readonly IUmbracoContextAccessor _umbracoContextAccessor;
		private readonly IUmbracoContext _umbracoContext;
		private readonly ServiceContext _services;
		private readonly FileHelperService _FileHelperService;
		private readonly DependencyLoader _Dependencies;

		private bool _HasUmbracoContext;
		private readonly IWebHostEnvironment _HostingEnvironment;
		private readonly HttpContext _Context;
		private readonly DataEditingToolsService _dataEditingToolsService;
		private readonly ConfigData _config;
		private readonly CustomTransformerService _CustomTransformerService;

		public ViewHelperService(
			ILogger<ViewHelperService> logger,
			DependencyLoader dependencies,
			DataEditingToolsService dataEditingToolsService,
			CustomTransformerService customTransformerService
			)
		{
			_logger = logger;

			_Dependencies = dependencies;
			_HostingEnvironment = dependencies.HostingEnvironment;
			_umbracoHelper = dependencies.UmbHelper;
			_FileHelperService = dependencies.DragonflyFileHelperService;
			_Context = dependencies.Context;

			_services = dependencies.Services;

			_umbracoContextAccessor = dependencies.UmbracoContextAccessor;
			_HasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out _umbracoContext);
			_dataEditingToolsService = dataEditingToolsService;
			_CustomTransformerService = customTransformerService;
			_config = _dataEditingToolsService.ConfigOptions();
		}

		public string PackageDisplayTitle
		{
			get
			{
				return PackageInfo.DisplayTitle;

			}
		}

        public Version PackageVersion 	{
            get
            {
                return PackageInfo.Version;
            }
        }

        #endregion

		public string GetViewsPath()
		{
			var path = _dataEditingToolsService.GetViewsPath();
			return path;
		}
		public IEnumerable<DocTypeProperty> GetPropertiesOnNestedContent(int DataTypeId)
		{
			var ts = _dataEditingToolsService;
			return ts.GetPropertiesOnNestedContent(DataTypeId);
		}
		public IEnumerable<DocTypeProperty> GetPropertiesOnNestedContent(IDataType DataType)
		{
			var ts = _dataEditingToolsService;
			return ts.GetPropertiesOnNestedContent(DataType);
		}
		public IEnumerable<IDataType> GetNestedContentDataTypes(string DocTypeAlias)
		{
			var ts = _dataEditingToolsService;
			return ts.GetNestedContentDataTypes(DocTypeAlias);
		}
		public bool IsEligibleForIdToUdiReplace(string PropertyEditorAlias)
		{
			var eligiblePropEditors = new List<string>()
			{
				"Umbraco.ContentPicker",
				"Umbraco.MediaPicker",
				"Umbraco.MemberPicker",
				"Umbraco.MultiUrlPicker",
				"Umbraco.MultiNodeTreePicker"
			};

			return eligiblePropEditors.Contains(PropertyEditorAlias);
		}
		public string GetDataTypeFolderPath(IDataType DataType, string PathDelim = "/", string RootFolderName = "")
		{
			var folderNames = new List<string>();

			var folderIds = DataType.Path.Split(',');
			foreach (var id in folderIds)
			{
				int folderId;
				var isInt = int.TryParse(id, out folderId);
				if (isInt)
				{
					var folderContainer = _services.DataTypeService.GetContainer(folderId);
					var folderName = folderContainer != null ? folderContainer.Name : RootFolderName;
					folderNames.Add(folderName);
				}
				else
				{
					folderNames.Add("UnknownFolder");
				}
			}

			return string.Join(PathDelim, folderNames);
		}
		public string GetDocTypeFolderPath(IContentType ContentType, string PathDelim = "/", string RootFolderName = "")
		{
			var folderNames = new List<string>();

			var folderIds = ContentType.Path.Split(',');
			foreach (var id in folderIds)
			{
				var folderId = Convert.ToInt32(id);
				var folderContainer = _services.ContentTypeService.GetContainer(folderId);
				var folderName = folderContainer != null ? folderContainer.Name : RootFolderName;
				folderNames.Add(folderName);
			}

			return string.Join(PathDelim, folderNames);
		}
		public string GetMediaTypeFolderPath(IMediaType MediaType, string PathDelim = "/", string RootFolderName = "")
		{
			var folderNames = new List<string>();

			var folderIds = MediaType.Path.Split(',');
			foreach (var id in folderIds)
			{
				var folderId = Convert.ToInt32(id);
				var folderContainer = _services.MediaTypeService.GetContainer(folderId);
				var folderName = folderContainer != null ? folderContainer.Name : RootFolderName;
				folderNames.Add(folderName);
			}

			return string.Join(PathDelim, folderNames);
		}
		public string ConvertToCsvIds(IEnumerable<IContent> AffectedContentNodes)
		{
			return DataEditingToolsService.ConvertToCsvIds(AffectedContentNodes);

			//var csv = "";

			//var ids = AffectedContentNodes.Select(n => n.Id);
			//csv = string.Join(",", ids);

			//return csv;
		}

		public string ConvertToCsvIds(IEnumerable<IMedia> AffectedMediaNodes)
		{
			return DataEditingToolsService.ConvertToCsvIds(AffectedMediaNodes);
			//var csv = "";

			//var ids = AffectedMediaNodes.Select(n => n.Id);
			//csv = string.Join(",", ids);

			//return csv;
		}
		public HtmlString Highlight(string OriginalText, IEnumerable<string> TextStringsToHighlight)
		{
			var newText = "";

			//Check for HTML tags to encode
			var isHtml = false;
			if (OriginalText.Contains("<"))
			{
				isHtml = true;
			}

			if (isHtml)
			{
				newText = OriginalText.HtmlEncode();
				foreach (var str in TextStringsToHighlight)
				{
					newText = Highlight(newText, str.HtmlEncode()).ToString();
				}
			}
			else
			{
				newText = OriginalText;
				foreach (var str in TextStringsToHighlight)
				{
					newText = Highlight(newText, str).ToString();
				}
			}

			return new HtmlString(newText);
		}
		public HtmlString Highlight(string OriginalText, string TextStringToHighlight)
		{
			if (!string.IsNullOrEmpty(OriginalText))
			{
				if (!string.IsNullOrEmpty(TextStringToHighlight))
				{
					var markedUp = $"<mark>{TextStringToHighlight}</mark>";
					var newText = OriginalText.Replace(TextStringToHighlight, markedUp);
					return new HtmlString(newText);
				}
				else
				{
					return new HtmlString(OriginalText);
				}
			}
			else
			{
				return new HtmlString("");
			}
		}

		#region Custom Transformations

		//public static IEnumerable<SelectListItem> CustomTransformersAssembliesSelectList(string DefaultValue, string DefaultText)
		//{
		//    var list = CustomTransformerHelpers.GetAssemblyNames();
		//    var options = list.Select(i => new SelectListItem
		//    {
		//        Value = i.ToString(),
		//        Text = i.ToString()
		//    });

		//    //if default option needed... (string DefaultValue, string DefaultText)
		//    if (!string.IsNullOrEmpty(DefaultValue))
		//    {
		//        var defaultSelect = Enumerable.Repeat(new SelectListItem
		//        {
		//            Value = DefaultValue,
		//            Text = DefaultText
		//        }, count: 1);

		//        return defaultSelect.Concat(options);
		//    }

		//    return options;
		//}

		public IEnumerable<SelectListItem> CustomFindReplaceTransformersAllClassesSelectList(string DefaultValue, string DefaultSelectText, string NoOptionsText)
		{
			var list = _CustomTransformerService.GetAllTransformerClassNames(Enums.CustomTransformerTypes.ICustomFindReplaceDataTransformer);
			var options = list.Select(i => new SelectListItem
			{
				Value = i.ToString(),
				Text = i.ToString()
			});

			if (!options.Any())
			{
				var defaultSelect = Enumerable.Repeat(new SelectListItem
				{
					Value = DefaultValue,
					Text = NoOptionsText
				}, count: 1);

				return defaultSelect;
			}
			else
			{
				//if default option needed... (string DefaultValue, string DefaultText)
				if (!string.IsNullOrEmpty(DefaultSelectText))
				{
					var defaultSelect = Enumerable.Repeat(new SelectListItem
					{
						Value = DefaultValue,
						Text = DefaultSelectText
					}, count: 1);

					return defaultSelect.Concat(options);
				}
				else //no default to add
				{
					return options;
				}
			}
		}

		public IEnumerable<SelectListItem> CustomPropToPropTransformersAllClassesSelectList(string DefaultValue, string DefaultSelectText, string NoOptionsText)
		{
			var list = _CustomTransformerService.GetAllTransformerClassNames(Enums.CustomTransformerTypes.ICustomPropToPropDataTransformer);
			var options = list.Select(i => new SelectListItem
			{
				Value = i.ToString(),
				Text = i.ToString()
			});

			if (!options.Any())
			{
				var defaultSelect = Enumerable.Repeat(new SelectListItem
				{
					Value = DefaultValue,
					Text = NoOptionsText
				}, count: 1);

				return defaultSelect;
			}
			else
			{
				//if default option needed... (string DefaultValue, string DefaultText)
				if (!string.IsNullOrEmpty(DefaultSelectText))
				{
					var defaultSelect = Enumerable.Repeat(new SelectListItem
					{
						Value = DefaultValue,
						Text = DefaultSelectText
					}, count: 1);

					return defaultSelect.Concat(options);
				}
				else //no default to add
				{
					return options;
				}
			}
		}

		#endregion

		#region Date/Time
		//public enum TimezoneFormatOption
		//{
		//    Full,
		//    Abbreviated
		//}
		//public static string FormatUtcDateTime(DateTime dt, TimezoneFormatOption TzFormat = TimezoneFormatOption.Full, string LocalTimezone = "", string DateFormat = "ddd MMM d, yyyy", string TimeFormat = "hh:mm tt" )
		//{
		//    //var dateFormat = "ddd MMM d, yyyy";
		//    //var timeFormat = "hh:mm tt";
		//    if (LocalTimezone == "")
		//    {
		//        var configTz = Config.GetConfig().LocalTimezone;
		//        LocalTimezone = configTz!=""? configTz: TimeZoneInfo.Local.Id;
		//    }
		//    TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(LocalTimezone);
		//    DateTime localizedDT = TimeZoneInfo.ConvertTimeFromUtc(dt, timeZone);

		//    var zoneFullName = timeZone.IsDaylightSavingTime(localizedDT) ? timeZone.DaylightName : timeZone.StandardName;
		//    var zoneAbbrevName = zoneFullName.Abbreviate();
		//    var zoneInfo = TzFormat == TimezoneFormatOption.Abbreviated ? zoneAbbrevName : zoneFullName;

		//    var stringDate = string.Format("{0} at {1} ({2})", localizedDT.ToString(DateFormat), localizedDT.ToString(TimeFormat), zoneInfo);

		//    return stringDate;
		//}
		#endregion



	}
}
