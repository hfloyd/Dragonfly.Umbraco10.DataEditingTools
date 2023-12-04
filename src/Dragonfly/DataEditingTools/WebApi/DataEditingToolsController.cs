namespace Dragonfly.DataEditingTools.WebApi
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Text;
	using System.Web;

	using System.Net;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.ViewFeatures;
	using System.Text.Json;
	using Serilog;

	using Umbraco.Cms.Core.Models;
	using Umbraco.Cms.Core.PropertyEditors;
	using Umbraco.Cms.Web.Common.Attributes;
	using Umbraco.Cms.Web.Common.UmbracoContext;
	using Umbraco.Cms.Core.Services;
	using Umbraco.Cms.Web.BackOffice;
	using Umbraco.Cms.Web.Common.Attributes;
	using Umbraco.Extensions;

	using Dragonfly.DataEditingTools.Helpers;
	using Dragonfly.DataEditingTools.Models;
	using Dragonfly.NetHelpers;
	using Dragonfly.NetHelperServices;
	using Dragonfly.NetModels;
	using Dragonfly.UmbracoHelpers;

	using Newtonsoft.Json;
	using Microsoft.Extensions.Logging;
	using Umbraco.Cms.Web.BackOffice.Controllers;

	// [IsBackOffice]
	// GET: /Umbraco/backoffice/Api/TransformationHelperApi <-- UmbracoAuthorizedApiController

	//  /umbraco/backoffice/Dragonfly/DataEditingTools/
	[PluginController("Dragonfly")]
	[IsBackOffice]
	public class DataEditingToolsController : UmbracoAuthorizedApiController
	{

		#region ctor & DI
		private readonly ILogger<DataEditingToolsController> _logger;
		private readonly DataEditingToolsService _dataEditingToolsService;
		private readonly IViewRenderService _viewRenderService;

		private readonly ConfigData _config;
		private readonly ServiceContext _services;

		public DataEditingToolsController(
			ILogger<DataEditingToolsController> logger,
			DependencyLoader dependencies,
			DataEditingToolsService dataEditingToolsService,
			IViewRenderService viewRenderService
			)
		{
			_logger = logger;
			_dataEditingToolsService = dataEditingToolsService;
			_viewRenderService = viewRenderService;

			_services = dependencies.Services;

			_config = _dataEditingToolsService.ConfigOptions();
		}

		#endregion
		private string RazorFilesPath()
		{
			return _dataEditingToolsService.GetViewsPath();
		}


		#region "Start" Pages

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/Start
		[HttpGet]
		public IActionResult Start()
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}Start.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var propertyEditors = ts.AllPropertyEditorTypes;
			var allDataTypes = ts.AllDataTypes;
			var allDocTypes = ts.AllDocTypes;
			var allContentCompositions = ts.GetAllCompositionDocTypes();
			var allContentNodes = ts.AllContent;
			var allMediaTypes = ts.AllMediaTypes;
			var allMediaNodes = ts.AllMedia;
			var allMediaCompositions = ts.GetAllCompositionMediaTypes();

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");
			viewData.Add("AllPropertyEditorTypes", propertyEditors);
			viewData.Add("AllDataTypes", allDataTypes);
			viewData.Add("AllDocTypes", allDocTypes);
			viewData.Add("AllContentCompositions", allContentCompositions);
			viewData.Add("AllContentNodes", allContentNodes);
			viewData.Add("AllMediaTypes", allMediaTypes);
			viewData.Add("AllMediaCompositions", allMediaCompositions);
			viewData.Add("AllMediaNodes", allMediaNodes);
			//viewData.Add("AllDocTypes", allDocTypes);
			//viewData.Add("AllDocTypes", allDocTypes);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/StartPropertyEditorType?PropertyEditorAlias=xx
		[HttpGet]
		public IActionResult StartPropertyEditorType(string PropertyEditorAlias)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}StartPropertyEditorType.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			IDataEditor selEditor = null;
			var allPropertyEditors = ts.AllPropertyEditorTypes;
			var matches = allPropertyEditors.Where(n => n.Alias == PropertyEditorAlias).ToList();
			selEditor = matches.Any() ? matches.First() : null;

			var allRelatedDataTypes = ts.AllDataTypes.Where(n => n.EditorAlias == PropertyEditorAlias).ToList();
			var dataTypeIds = allRelatedDataTypes.Select(n => n.Id).ToList();
			var allRelatedProperties = ts.AllDocTypeProperties.Where(n => dataTypeIds.Contains(n.Property.DataTypeId));
		
			//UPDATE STATUS MSG
			//returnStatusMsg.Message = "";


			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("SelectedPropEditor", selEditor);
			viewData.Add("RelatedDataTypes", allRelatedDataTypes);
			viewData.Add("RelatedProperties", allRelatedProperties);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/StartDataType?DataTypeId=xx
		[HttpGet]
		public IActionResult StartDataType(int DataTypeId)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}StartDataType.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			IDataType selDataType = null;
			var allDataTypes = ts.AllDataTypes;
			var matches = allDataTypes.Where(n => n.Id == DataTypeId).ToList();
			selDataType = matches.Any() ? matches.First() : null;

			var allRelatedProperties = ts.AllDocTypeProperties.Where(n => n.Property.DataTypeId == DataTypeId);

			//UPDATE STATUS MSG
			//returnStatusMsg.Message = "";


			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("SelectedDataType", selDataType);
			viewData.Add("RelatedProperties", allRelatedProperties);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/StartDocType?DocTypeId=xx
		[HttpGet]
		public IActionResult StartDocType(int DocTypeId)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}StartDocType.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			IContentType selDocType = null;
			var allDocTypes = ts.AllDocTypes;
			var matches = allDocTypes.Where(n => n.Id == DocTypeId).ToList();
			selDocType = matches.Any() ? matches.First() : null;

			var allRelatedProperties = ts.AllDocTypeProperties.Where(n => n.DocTypeAlias == selDocType.Alias);

			//var allCompositions = ts.GetAllCompositionDocTypes();
			var allCompositions = ts.GetAllCompositionsWithTypes();

			var allContentNodesOfType = ts.AllContent.Where(n => n.ContentTypeId == selDocType.Id);

			//UPDATE STATUS MSG
			//returnStatusMsg.Message = "";


			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("SelectedDocType", selDocType);
			viewData.Add("AllCompositions", allCompositions);
			viewData.Add("RelatedProperties", allRelatedProperties);
			viewData.Add("ContentNodesOfType", allContentNodesOfType);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}


		/// /umbraco/backoffice/Dragonfly/DataEditingTools/StartMediaType?MediaTypeId=xx
		[HttpGet]
		public IActionResult StartMediaType(int MediaTypeId)
		{
			var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}StartMediaType.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			IMediaType selMediaType = null;
			var allMediaTypes = ts.AllMediaTypes;
			var matches = allMediaTypes.Where(n => n.Id == MediaTypeId).ToList();
			selMediaType = matches.Any() ? matches.First() : null;

			var allRelatedProperties = ts.AllMediaTypeProperties.Where(n => n.DocTypeAlias == selMediaType.Alias);

			//var allCompositions = ts.GetAllCompositionMediaTypes();
			var allCompositions = ts.GetAllCompositionsWithTypes();

			var allMediaNodesOfType = ts.AllMedia.Where(n => n.ContentTypeId == selMediaType.Id);

			//UPDATE STATUS MSG
			//returnStatusMsg.Message = "";


			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("SelectedMediaType", selMediaType);
			viewData.Add("AllCompositions", allCompositions);
			viewData.Add("RelatedProperties", allRelatedProperties);
			viewData.Add("MediaNodesOfType", allMediaNodesOfType);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		#endregion


		#region Data-Editing Pages

		#region Prop-to-Prop

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/TransferContentPropertyToPropertySetup?DocTypeAlias=xx
		[HttpGet]
		public IActionResult TransferContentPropertyToPropertySetup(int DocTypeId, string PropertyFrom = "", string PropertyTo = "")
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}TransferPropertyToProperty.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var selDocType = _services.ContentTypeService.Get(DocTypeId);
			var availableProperties = ts.AllDocTypeProperties.Where(n => n.DocTypeAlias == selDocType.Alias);

			var affectedNodes = ts.AllContent.Where(n => n.ContentTypeId == selDocType.Id).ToList();
			var nodesList = DataEditingToolsService.ConvertToCsvIds(affectedNodes);
			var propertyAliases = availableProperties.Select(n => n.Property.Alias).ToList();

			var formInputs = new FormInputsPropertyToProperty();
			formInputs.PreviewOnly = true; //default
			formInputs.AvailablePropertiesCSV = string.Join(",", propertyAliases);
			formInputs.ContentNodeIdsCsv = nodesList;
			formInputs.NodeTypes = Enums.NodeType.Content;
			formInputs.TypeAlias = selDocType.Alias;
			formInputs.PropertyAliasFrom = PropertyFrom;
			formInputs.PropertyAliasTo = PropertyTo;

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			//viewData.Add("AvailableProperties", availableProperties);
			viewData.Add("SelectedType", selDocType);
			viewData.Add("AffectedNodes", affectedNodes);
			viewData.Add("FormInputs", formInputs);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/TransferMediaPropertyToPropertySetup?MediaTypeId=xx
		[HttpGet]
		public IActionResult TransferMediaPropertyToPropertySetup(int MediaTypeId, string PropertyFrom = "", string PropertyTo = "")
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}TransferPropertyToProperty.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var selType = _services.MediaTypeService.Get(MediaTypeId);
			var availableProperties = ts.AllMediaTypeProperties.Where(n => n.DocTypeAlias == selType.Alias);

			var affectedNodes = ts.AllMedia.Where(n => n.ContentTypeId == selType.Id);
			var nodesList = DataEditingToolsService.ConvertToCsvIds(affectedNodes);
			var propertyAliases = availableProperties.Select(n => n.Property.Alias).ToList();

			var formInputs = new FormInputsPropertyToProperty();
			formInputs.PreviewOnly = true; //default
			formInputs.AvailablePropertiesCSV = string.Join(",", propertyAliases);
			formInputs.ContentNodeIdsCsv = nodesList;
			formInputs.NodeTypes = Enums.NodeType.Media;
			formInputs.TypeAlias = selType.Alias;
			formInputs.PropertyAliasFrom = PropertyFrom;
			formInputs.PropertyAliasTo = PropertyTo;

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			//viewData.Add("AvailableProperties", availableProperties);
			viewData.Add("SelectedType", selType);
			viewData.Add("AffectedNodes", affectedNodes);
			viewData.Add("FormInputs", formInputs);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}


		/// /umbraco/backoffice/Dragonfly/DataEditingTools/TransferPropertyToProperty
		[HttpPost]
		public IActionResult TransferPropertyToProperty(FormInputsPropertyToProperty FormInputs)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}TransferPropertyToProperty.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			IContentType selDocType = null;
			IMediaType selMediaType = null;
			IEnumerable<IContent> affectedContentNodes = new List<IContent>();
			IEnumerable<IMedia> affectedMediaNodes = new List<IMedia>();
			var specialMessage = "";
			var specialMsgClass = "bg-info text-white";

			var resultSet = new PropToPropResultsSet();

			if (FormInputs != null)
			{
				if (string.IsNullOrEmpty(FormInputs.PropertyAliasFrom) || string.IsNullOrEmpty(FormInputs.PropertyAliasTo))
				{
					specialMessage = "Both a 'From' Property and a 'To' Property are required.";
					specialMsgClass = "bg-danger text-white";
					//returnStatusMsg.Success = false;
					//returnStatusMsg.Message =
					//    $"Form Inputs data was missing - Both a 'From' Property and a 'To' Property are required.";
				}
				else
				{
					//Update based on Node Type
					if (FormInputs.NodeTypes == Enums.NodeType.Content)
					{
						selDocType = _services.ContentTypeService.Get(FormInputs.TypeAlias);
						affectedContentNodes = ts.IdCsvToContents(FormInputs.ContentNodeIdsCsv);
						resultSet = ts.ProcessPropertyToProperty(FormInputs);

						if (resultSet.HasError)
						{
							specialMessage = resultSet.ErrorMessage;
							specialMsgClass = "bg-danger text-white";
						}
						else if (!FormInputs.PreviewOnly)
						{
							//Do Content updates
							foreach (var toUpdate in resultSet.Results.Where(n => n.ValidToTransfer))
							{
								var key = toUpdate.Key;
								toUpdate.ContentNode.SetValue(toUpdate.PropertyToAlias, toUpdate.PropertyToData);

								if (toUpdate.ContentNode.Published)
								{
									var saveResult = _services.ContentService.SaveAndPublish(toUpdate.ContentNode);
									resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
									resultSet.Results.Find(p => p.Key == key).SavePublishResult = saveResult;
								}
								else
								{
									var saveResult = _services.ContentService.Save(toUpdate.ContentNode);
									resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
									resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = saveResult;
								}
							}

							resultSet.DataUpdatedAndSaved = true;
						}
					}
					else if (FormInputs.NodeTypes == Enums.NodeType.Media)
					{
						selMediaType = _services.MediaTypeService.Get(FormInputs.TypeAlias);
						affectedMediaNodes = ts.IdCsvToMedias(FormInputs.ContentNodeIdsCsv);
						resultSet = ts.ProcessPropertyToProperty(FormInputs);

						if (resultSet.HasError)
						{
							specialMessage = resultSet.ErrorMessage;
							specialMsgClass = "bg-danger text-white";
						}
						else if (!FormInputs.PreviewOnly)
						{
							//Do Media updates
							foreach (var toUpdate in resultSet.Results.Where(n => n.ValidToTransfer))
							{
								var key = toUpdate.Key;
								toUpdate.MediaNode.SetValue(toUpdate.PropertyToAlias, toUpdate.PropertyToData);

								var saveResult = _services.MediaService.Save(toUpdate.MediaNode);
								resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
								resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = saveResult.Result;
							}

							resultSet.DataUpdatedAndSaved = true;
						}
					}
				}
			}
			else
			{
				returnStatusMsg.Success = false;
				returnStatusMsg.Message = $"Form Inputs data was missing.";
			}

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", specialMessage);
			viewData.Add("SpecialMessageClass", specialMsgClass);

			//viewData.Add("AvailableProperties", availableProperties);
			viewData.Add("FormInputs", FormInputs);
			viewData.Add("Results", resultSet);

			if (FormInputs.NodeTypes == Enums.NodeType.Content)
			{
				viewData.Add("SelectedType", selDocType);
				viewData.Add("AffectedNodes", affectedContentNodes);
			}
			else if (FormInputs.NodeTypes == Enums.NodeType.Media)
			{
				viewData.Add("SelectedType", selMediaType);
				viewData.Add("AffectedNodes", affectedMediaNodes);
			}

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		#endregion

		#region FindReplace

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/SetupReplaceInData?DocTypeAlias=xx&DataTypeId=xx
		[HttpGet]
		public IActionResult SetupReplaceInData(string DocTypeAlias, int DataTypeId)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}FindReplaceInRawData.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var selDocType = _services.ContentTypeService.Get(DocTypeAlias);
			var selDataType = _services.DataTypeService.GetDataType(DataTypeId);

			var affectedProperties = ts.AllDocTypeProperties.Where(n => n.DocTypeAlias == DocTypeAlias && n.Property.DataTypeId == DataTypeId);

			var affectedNodes = ts.AllContent.Where(n => n.ContentTypeId == selDocType.Id);
			var nodesList = DataEditingToolsService.ConvertToCsvIds(affectedNodes);
			var propertyAliases = affectedProperties.Select(n => n.Property.Alias).ToList();

			var formInputs = new FormInputsFindReplace();
			formInputs.PreviewOnly = true; //default
			formInputs.PropertyAliasesCsv = string.Join(",", propertyAliases);
			formInputs.ContentNodeIdsCsv = nodesList;

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("AffectedProperties", affectedProperties);
			viewData.Add("SelectedDocType", selDocType);
			viewData.Add("SelectedDataType", selDataType);
			viewData.Add("SelectedProperty", null);
			viewData.Add("AffectedContentNodes", affectedNodes);
			viewData.Add("FormInputs", formInputs);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/SetupReplaceInData?DocTypeAlias=xx&PropertyAlias=xx
		[HttpGet]
		public IActionResult SetupReplaceInData(string DocTypeAlias, string PropertyAlias)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}FindReplaceInRawData.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var selDocType = _services.ContentTypeService.Get(DocTypeAlias);

			var affectedProperties = ts.AllDocTypeProperties.Where(n => n.DocTypeAlias == DocTypeAlias && n.Property.Alias == PropertyAlias);
			var selProperty = affectedProperties.First().Property;
			var selDataType = _services.DataTypeService.GetDataType(selProperty.DataTypeId);

			var affectedNodes = ts.AllContent.Where(n => n.ContentTypeId == selDocType.Id);
			var nodesList = DataEditingToolsService.ConvertToCsvIds(affectedNodes);
			var propertyAliases = affectedProperties.Select(n => n.Property.Alias).ToList();

			var formInputs = new FormInputsFindReplace();
			formInputs.PreviewOnly = true; //default
			formInputs.PropertyAliasesCsv = string.Join(",", propertyAliases);
			formInputs.ContentNodeIdsCsv = nodesList;

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("AffectedProperties", affectedProperties);
			viewData.Add("SelectedDocType", selDocType);
			viewData.Add("SelectedProperty", selProperty);
			viewData.Add("SelectedDataType", selDataType);
			viewData.Add("AffectedContentNodes", affectedNodes);
			viewData.Add("FormInputs", formInputs);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/SetupReplaceInData?PropertyAlias=xx
		[HttpGet]
		public IActionResult SetupReplaceInData(string PropertyAlias)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}FindReplaceInRawData.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			IContent selDocType = null;//Services.ContentTypeService.Get(DocTypeAlias);

			var affectedProperties = ts.AllDocTypeProperties.Where(n => n.Property.Alias == PropertyAlias);
			var selProperty = affectedProperties.First().Property;
			var selDataType = _services.DataTypeService.GetDataType(selProperty.DataTypeId);

			var affectedNodes = ts.AllContent.Where(n => n.HasProperty(PropertyAlias));
			var nodesList = DataEditingToolsService.ConvertToCsvIds(affectedNodes);
			var propertyAliases = affectedProperties.Select(n => n.Property.Alias).ToList();

			var formInputs = new FormInputsFindReplace();
			formInputs.PreviewOnly = true; //default
			formInputs.PropertyAliasesCsv = string.Join(",", propertyAliases);
			formInputs.ContentNodeIdsCsv = nodesList;

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("AffectedProperties", affectedProperties);
			viewData.Add("SelectedDocType", selDocType);
			viewData.Add("SelectedProperty", selProperty);
			viewData.Add("SelectedDataType", selDataType);
			viewData.Add("AffectedContentNodes", affectedNodes);
			viewData.Add("FormInputs", formInputs);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}


		/// /umbraco/backoffice/Dragonfly/DataEditingTools/FindReplace
		[HttpPost]
		public IActionResult FindReplace(FormInputsFindReplace FormInputs)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}FindReplaceResults.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var resultSet = new FindReplaceResultsSet();

			if (FormInputs != null)
			{
				if (FormInputs.FindReplaceTypeOption == Enums.FindReplaceType.CustomTransformation && string.IsNullOrEmpty(FormInputs.CustomTransformationClass))
				{
					returnStatusMsg.Success = false;
					returnStatusMsg.Message =
						$"Custom Transformer option was selected, but no Transformation class was provided. Please check form inputs.";
				}

				resultSet = ts.ProcessFindReplace(FormInputs);

				if (!FormInputs.PreviewOnly)
				{
					//Do Content updates
					foreach (var toUpdate in resultSet.Results.Where(n => n.MatchFound))
					{
						//int index = resultSet.Results.FindIndex(n => n.Key == toUpdate.Key);
						var key = toUpdate.Key;
						toUpdate.ContentNode.SetValue(toUpdate.PropertyAlias, toUpdate.NewPropertyData);

						if (toUpdate.ContentNode.Published)
						{
							var saveResult = _services.ContentService.SaveAndPublish(toUpdate.ContentNode);
							resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
							resultSet.Results.Find(p => p.Key == key).SavePublishResult = saveResult;
						}
						else
						{
							var saveResult = _services.ContentService.Save(toUpdate.ContentNode);
							resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
							resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = saveResult;
						}
					}

					resultSet.DataUpdatedAndSaved = true;
				}
			}
			else
			{
				returnStatusMsg.Success = false;
				returnStatusMsg.Message =
					$"Form Inputs data was missing.";
			}

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("FormInputs", FormInputs);
			viewData.Add("FindReplaceResults", resultSet);
			//viewData.Add("SearchResultsSet", searchResults);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/ReplaceWithUdis
		[HttpPost]
		public IActionResult ReplaceWithUdis(FormInputsFindReplace FormInputs)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}ReplaceWithUdiResults.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var resultSet = new FindReplaceResultsSet();

			if (FormInputs != null)
			{
				resultSet = ts.ProcessFindReplaceIntUdi(FormInputs);

				if (!FormInputs.PreviewOnly)
				{
					//Do Content updates
					foreach (var toUpdate in resultSet.Results.Where(n => n.MatchFound))
					{
						var key = toUpdate.Key;
						toUpdate.ContentNode.SetValue(toUpdate.PropertyAlias, toUpdate.NewPropertyData);

						if (toUpdate.ContentNode.Published)
						{
							var saveResult = _services.ContentService.SaveAndPublish(toUpdate.ContentNode);
							resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
							resultSet.Results.Find(p => p.Key == key).SavePublishResult = saveResult;
						}
						else
						{
							var saveResult = _services.ContentService.Save(toUpdate.ContentNode);
							resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
							resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = saveResult;
						}
					}

					resultSet.DataUpdatedAndSaved = true;
				}
			}
			else
			{
				returnStatusMsg.Success = false;
				returnStatusMsg.Message =
					$"Form Inputs data was missing.";
			}

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("FormInputs", FormInputs);
			viewData.Add("FindReplaceResults", resultSet);
			//viewData.Add("SearchResultsSet", searchResults);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		#endregion

		#region Store Legacy Data

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/SetupStoreLegacyData
		[HttpGet]
		public IActionResult SetupStoreLegacyData()
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}StoreLegacyData.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			//var selDocType = Services.ContentTypeService.Get(DocTypeAlias);

			var contentProperties = ts.AllDocTypeProperties;
			var contentPropAliases = contentProperties.Select(n => n.Property.Alias).Distinct().ToList();

			var mediaProperties = ts.AllMediaTypeProperties;
			var mediaPropAliases = mediaProperties.Select(n => n.Property.Alias).Distinct().ToList();

			//var selProperty = affectedProperties.First().Property;
			//var selDataType = Services.DataTypeService.GetDataType(selProperty.DataTypeId);

			// var affectedNodes = ts.AllContent.Where(n => n.ContentTypeId == selDocType.Id);
			// var nodesList = ViewHelpers.ConvertToCsvIds(affectedNodes);


			var formInputs = new FormInputsStoreLegacyData();
			formInputs.PreviewOnly = true; //default
			formInputs.AvailableContentPropertiesCSV = string.Join(",", contentPropAliases);
			formInputs.AvailableMediaPropertiesCSV = string.Join(",", mediaPropAliases);

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			//viewData.Add("AffectedProperties", affectedProperties);
			//viewData.Add("SelectedDocType", selDocType);
			//viewData.Add("SelectedProperty", selProperty);
			//viewData.Add("SelectedDataType", selDataType);
			//viewData.Add("AffectedContentNodes", affectedNodes);
			viewData.Add("FormInputs", formInputs);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/DoStoreLegacyData
		[HttpPost]
		public IActionResult DoStoreLegacyData(FormInputsStoreLegacyData FormInputs)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}StoreLegacyData.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			//IContentType selDocType = null;
			IEnumerable<IContent> affectedContentNodes = new List<IContent>();
			IEnumerable<IMedia> affectedMediaNodes = new List<IMedia>();
			var specialMessage = "";
			var specialMsgClass = "bg-info text-white";

			var resultSet = new LegacyDataResultsSet();

			if (FormInputs != null)
			{
				//selDocType = Services.ContentTypeService.Get(FormInputs.DocTypeAlias);
				//affectedNodes = ts.IdCsvToContents(FormInputs.ContentNodeIdsCsv);

				if (string.IsNullOrEmpty(FormInputs.PropertyAliasContentNodeId) && string.IsNullOrEmpty(FormInputs.PropertyAliasMediaNodeId))
				{
					specialMessage = "You need to specify at least one property to update.";
					specialMsgClass = "bg-danger text-white";
					//returnStatusMsg.Success = false;
					//returnStatusMsg.Message =
					//    $"Form Inputs data was missing - Both a 'From' Property and a 'To' Property are required.";
				}
				else
				{
					resultSet = ts.ProcessStoreLegacyData(FormInputs);

					if (!FormInputs.PreviewOnly)
					{
						//Do Content updates
						foreach (var toUpdate in resultSet.Results.Where(n => n.ValidToTransfer & n.Type == Enums.NodeType.Content))
						{
							var key = toUpdate.Key;
							toUpdate.ContentNode.SetValue(toUpdate.IdPropertyAlias, toUpdate.IdData);

							if (toUpdate.ContentNode.Published)
							{
								//try Save & Publish
								try
								{
									var saveResult = _services.ContentService.SaveAndPublish(toUpdate.ContentNode);
									resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
									resultSet.Results.Find(p => p.Key == key).SavePublishResult = saveResult;
								}
								catch (Exception exSavePub)
								{
									_logger.LogWarning( exSavePub, "DoStoreLegacyData: Unable to Save & Publish content node #{0} - Attempting Save Only...", toUpdate.ContentNode.Id);
									//Try just saving
									try
									{
										var saveResult = _services.ContentService.Save(toUpdate.ContentNode);
										resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
										resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = saveResult;
									}
									catch (Exception exSave)
									{
										_logger.LogError( exSave, "DoStoreLegacyData: Unable to Save content node #{0} - FAILURE: {1}", toUpdate.ContentNode.Id, exSave);
										resultSet.Results.Find(p => p.Key == key).ContentUpdated = false;
										resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = null;
									}
								}
							}
							else
							{
								//Try just saving
								try
								{
									var saveResult = _services.ContentService.Save(toUpdate.ContentNode);
									resultSet.Results.Find(p => p.Key == key).ContentUpdated = true;
									resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = saveResult;
								}
								catch (Exception exSave)
								{
									_logger.LogError(exSave, "DoStoreLegacyData: Unable to Save content node #{0} - FAILURE: {1}", toUpdate.ContentNode.Id, exSave);
									resultSet.Results.Find(p => p.Key == key).ContentUpdated = false;
									resultSet.Results.Find(p => p.Key == key).SaveOnlyResult = null;
								}
							}
						}

						//Do Media updates
						foreach (var toUpdate in resultSet.Results.Where(n => n.ValidToTransfer & n.Type == Enums.NodeType.Media))
						{
							var key = toUpdate.Key;
							toUpdate.MediaNode.SetValue(toUpdate.IdPropertyAlias, toUpdate.IdData);

							try
							{
								_services.MediaService.Save(toUpdate.MediaNode);
								resultSet.Results.Find(p => p.Key == key).NodeUpdated = true;
							}
							catch (Exception exMediaSave)
							{
								_logger.LogError( "DoStoreLegacyData: Unable to Save media node #{0} - FAILURE: {1}", toUpdate.MediaNode.Id, exMediaSave);
								resultSet.Results.Find(p => p.Key == key).NodeUpdated = false;
							}
						}

						resultSet.DataUpdatedAndSaved = true;
					}
				}
			}
			else
			{
				returnStatusMsg.Success = false;
				returnStatusMsg.Message =
					$"Form Inputs data was missing.";
			}

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", specialMessage);
			viewData.Add("SpecialMessageClass", specialMsgClass);

			//viewData.Add("AvailableProperties", availableProperties);
			//viewData.Add("SelectedDocType", selDocType);
			//viewData.Add("AffectedContentNodes", affectedNodes);
			viewData.Add("FormInputs", FormInputs);
			viewData.Add("Results", resultSet);

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		#endregion

		#region Lookup Udi

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/SetupLookupUdi
		[HttpGet]
		public IActionResult SetupLookupUdi()
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}LookupUdi.cshtml";

			//Setup
			// var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY

			var formInputs = new FormInputsUdiLookup();
			formInputs.ObjectType = Enums.UmbracoObjectType.Unknown; //default

			//UPDATE STATUS MSG
			//returnStatusMsg.Success = true;

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", "");
			viewData.Add("SpecialMessageClass", "bg-info");

			viewData.Add("FormInputs", formInputs);
			viewData.Add("LookupResult", new UdiLookupResult(Enums.LookupStatus.NotSearchedYet));

			//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/LookupUdi
		[HttpPost]
		public IActionResult LookupUdi(FormInputsUdiLookup FormInputs)
		{
			//var returnSB = new StringBuilder();
			var returnStatusMsg = new StatusMessage(true); //assume success
			var pvPath = $"{RazorFilesPath()}LookupUdi.cshtml";

			//Setup
			var ts = _dataEditingToolsService;

			//GET DATA TO DISPLAY
			var specialMessage = "";
			var specialMsgClass = "bg-info text-white";

			UdiLookupResult lookupResult = new UdiLookupResult(Enums.LookupStatus.NotSearchedYet);

			if (FormInputs != null)
			{
				lookupResult = ts.LookupUdi(FormInputs);
			}
			else
			{
				returnStatusMsg.Success = false;
				returnStatusMsg.Message =
					$"Form Inputs data was missing.";
			}

			//VIEW DATA 
			var model = returnStatusMsg;
			var viewData = new Dictionary<string, object>();
			viewData.Add("SpecialMessage", specialMessage);
			viewData.Add("SpecialMessageClass", specialMsgClass);

			viewData.Add("FormInputs", FormInputs);
			viewData.Add("LookupResult", lookupResult);
//RENDER
			var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};
		}

		#endregion

		#endregion


		#region JSON Returns
		/// /umbraco/backoffice/Dragonfly/DataEditingTools/TestRaw
		[HttpGet]
		public IActionResult TestRaw()
		{
			var returnSB = new StringBuilder();
			var returnStatus = new StatusMessage();

			//GET DATA TO DISPLAY
			var ts = _dataEditingToolsService;
			var allContent = ts.AllContent;

			var docTypeToFix = "home";
			var contentOfType = allContent.Where(n => n.ContentType.Alias == docTypeToFix);


			//UPDATE STATUS MSG
			returnStatus.Success = true;
			returnStatus.Message = $"All Content of Type: {docTypeToFix}";
			returnStatus.MessageDetails = $"Number of Content Nodes: {contentOfType.Count()}";
			returnStatus.RelatedObject = contentOfType;

			//Return JSON
			return new JsonResult(returnStatus);
		}

		/// /umbraco/backoffice/Dragonfly/DataEditingTools/GetAllProperties
		[HttpGet]
		public IActionResult GetAllProperties()
		{
			//var returnSB = new StringBuilder();
			var returnStatus = new StatusMessage();

			//GET DATA TO DISPLAY
			var ts = _dataEditingToolsService;
			var allContent = ts.AllContent;

			var propEditor = "Umbraco.NestedContent";
			//var contentOfType = allContent.Where(n => n.ContentType.Alias == docTypeToFix);

			var allProps = ts.AllDocTypeProperties;

			var matchingProps = allProps.Where(n => n.Property.PropertyEditorAlias == propEditor);

			//UPDATE STATUS MSG
			returnStatus.Success = true;
			returnStatus.Message = $"All Properties of Property Editor Type: {propEditor}";
			returnStatus.MessageDetails = $"Number of Properties: {matchingProps.Count()}";
			returnStatus.RelatedObject = matchingProps;

			//Return JSON
			return new JsonResult(returnStatus);
		}
		#endregion


		#region Tests & Examples

		/// /umbraco/backoffice/Dragonfly/AuthorizedApi/Test
		[HttpGet]
		public bool Test()
		{
			//LogHelper.Info<PublicApiController>("Test STARTED/ENDED");
			return true;
		}

		/// /umbraco/backoffice/Dragonfly/AuthorizedApi/ExampleReturnHtml
		[HttpGet]
		public IActionResult ExampleReturnHtml()
		{
			//Setup
			var returnStatusMsg = new StatusMessage();
			var pvPath = RazorFilesPath() + "Start.cshtml";

			//GET DATA TO DISPLAY
			var status = new StatusMessage(true);

			//BUILD HTML
			var returnSB = new StringBuilder();
			returnSB.AppendLine("<h1>Hello! This is HTML</h1>");
			returnSB.AppendLine("<p>Use this type of return when you want to exclude &lt;XML&gt;&lt;/XML&gt; tags from your output and don\'t want it to be encoded automatically.</p>");
			var displayHtml = returnSB.ToString();

			//VIEW DATA 
			//var model = returnStatusMsg;
			//var viewData = new Dictionary<string, object>();
			//viewData.Add("StandardInfo", GetStandardViewInfo());
			//viewData.Add("Status", status);

			//RENDER
			//var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			//var displayHtml = htmlTask.Result;

			//RETURN AS HTML
			return new ContentResult()
			{
				Content = displayHtml,
				StatusCode = (int)HttpStatusCode.OK,
				ContentType = "text/html; charset=utf-8"
			};

		}

		/// /umbraco/backoffice/Dragonfly/AuthorizedApi/ExampleReturnJson
		[HttpGet]
		public IActionResult ExampleReturnJson()
		{
			//var testData1 = new TimeSpan(1, 1, 1, 1);
			var testData2 = new StatusMessage(true, "This is a test object so you can see JSON!");

			//Return JSON
			return new JsonResult(testData2);
		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/ExampleReturnCsv
		[HttpGet]
		public IActionResult ExampleReturnCsv()
		{
			var returnSB = new StringBuilder();
			var tableData = new StringBuilder();

			for (int i = 0; i < 10; i++)
			{
				tableData.AppendFormat(
					"\"{0}\",{1},\"{2}\",{3}{4}",
					"Name " + i,
					i,
					string.Format("Some text about item #{0} for demo.", i),
					DateTime.Now,
					Environment.NewLine);
			}
			returnSB.Append(tableData);

			//RETURN AS CSV FILE
			var fileName = "Example.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

		}

		/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestCSV
		[HttpGet]
		public IActionResult TestCsv()
		{
			var returnSB = new StringBuilder();

			var tableData = new StringBuilder();

			for (int i = 0; i < 10; i++)
			{
				tableData.AppendFormat(
					"\"{0}\",{1},\"{2}\",{3}{4}",
					"Name " + i,
					i,
					string.Format("Some text about item #{0} for demo.", i),
					DateTime.Now,
					Environment.NewLine);
			}
			returnSB.Append(tableData);


			//RETURN AS CSV FILE
			var fileName = "Test.csv";
			var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
			var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
			return File(result, "text/csv", fileName);

		}

		#endregion
	}
}
