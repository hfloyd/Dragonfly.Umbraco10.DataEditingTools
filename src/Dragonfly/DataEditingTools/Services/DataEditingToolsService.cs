namespace Dragonfly.DataEditingTools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Text.RegularExpressions;
using Dragonfly.DataEditingTools.Models;
using Dragonfly.UmbracoHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.UmbracoContext;

using Umbraco.Extensions;



public class DataEditingToolsService
{

    private readonly ConfigData _config;

    #region CTOR & DI

    private readonly ILogger<DataEditingToolsService> _logger;
    private readonly IServiceProvider _Provider;

    private readonly IConfiguration _AppSettingsConfig;
    private readonly IWebHostEnvironment _HostingEnvironment;

    //private readonly AppCaches _appCaches;
    //private readonly UmbracoContext _umbracoContext;
    //private readonly UmbracoHelper _umbracoHelper;
    private readonly ServiceContext _services;
    private readonly IPublishedContentQuery _publishedContentQuery;

    public DataEditingToolsService(
            ILogger<DataEditingToolsService> logger,
            IServiceProvider provider,
            DependencyLoader Dependencies
            )
    {
        _logger = logger;
         _Provider= provider;
        _AppSettingsConfig = Dependencies.AppSettingsConfig;
        _HostingEnvironment = Dependencies.HostingEnvironment;
        _services = Dependencies.Services;

        //Config Data
        _config = ConfigReaderHelper.GetConfig(_AppSettingsConfig);

    }

    #endregion

    #region Config Methods

    public ConfigData ConfigOptions()
    {
        return _config;
    }

    public string GetViewsPath()
    {
        var path = GetAppPluginsPath().EnsureEndsWith("/") + "RazorViews/";
        return path;
    }

    public string GetAppPluginsPath(bool ReturnMapped = false)
    {
        var path = ConfigOptions().AppPluginsPath;

        if (ReturnMapped)
        {
            return ConfigReaderHelper.MapPath(path, false, _HostingEnvironment);
        }
        else
        {
            return path;
        }
    }
    public string GetAppDataPath(bool ReturnMapped = false)
    {
        var path = ConfigOptions().DefaultDataPath;

        if (ReturnMapped)
        {
            return ConfigReaderHelper.MapPath(path, false, _HostingEnvironment);
        }
        else
        {
            return path;
        }
    }

    #endregion

    #region  Public Properties

    private List<IContent> _allContent = new List<IContent>();
    public IEnumerable<IContent> AllContent
    {
        get
        {
            if (_allContent.Any())
            {
                return _allContent;
            }
            else
            {
                FillAllContent();
                return _allContent;
            }
        }
    }


    private List<IContentType> _allDocTypes = new List<IContentType>();
    public IEnumerable<IContentType> AllDocTypes
    {
        get
        {
            if (_allDocTypes.Any())
            {
                return _allDocTypes;
            }
            else
            {
                FillDocTypesList();
                return _allDocTypes;
            }
        }
    }


    private List<DocTypeProperty> _allDocTypeProperties = new List<DocTypeProperty>();
    public IEnumerable<DocTypeProperty> AllDocTypeProperties
    {
        get
        {
            if (_allDocTypeProperties.Any())
            {
                return _allDocTypeProperties;
            }
            else
            {
                FillPropertiesList();
                return _allDocTypeProperties;
            }
        }
    }


    private List<IDataType> _allDataTypes = new List<IDataType>();
    public IEnumerable<IDataType> AllDataTypes
    {
        get
        {
            if (_allDataTypes.Any())
            {
                return _allDataTypes;
            }
            else
            {
                FillDataTypesList();
                return _allDataTypes;
            }
        }
    }


    private IEnumerable<IDataEditor> _allPropertyEditorTypes = new List<IDataEditor>();
    public IEnumerable<IDataEditor> AllPropertyEditorTypes
    {
        get
        {
            if (_allPropertyEditorTypes.Any())
            {
                return _allPropertyEditorTypes;
            }
            else
            {
                FillPropertyEditorsList();
                return _allPropertyEditorTypes;
            }
        }
    }


    private List<IMediaType> _allMediaTypes = new List<IMediaType>();
    public IEnumerable<IMediaType> AllMediaTypes
    {
        get
        {
            if (_allMediaTypes.Any())
            {
                return _allMediaTypes;
            }
            else
            {
                FillMediaTypesList();
                return _allMediaTypes;
            }
        }
    }


    private List<IMedia> _allMedia = new List<IMedia>();
    public IEnumerable<IMedia> AllMedia
    {
        get
        {
            if (_allMedia.Any())
            {
                return _allMedia;
            }
            else
            {
                FillAllMedia();
                return _allMedia;
            }
        }
    }


    private List<DocTypeProperty> _allMediaTypeProperties = new List<DocTypeProperty>();
    public IEnumerable<DocTypeProperty> AllMediaTypeProperties
    {
        get
        {
            if (_allMediaTypeProperties.Any())
            {
                return _allMediaTypeProperties;
            }
            else
            {
                FillMediaPropertiesList();
                return _allMediaTypeProperties;
            }
        }
    }

    #endregion


    #region Lookup Data

    public IEnumerable<IContentTypeComposition> GetAllCompositionDocTypes()
    {
        var docTypes = AllDocTypes;

        var comps = new List<IContentTypeComposition>();
        foreach (var type in docTypes)
        {
            comps.AddRange(type.ContentTypeComposition.ToList());
        }

        return comps.DistinctBy(n => n.Id);
    }

    public IEnumerable<IContentTypeComposition> GetAllCompositionMediaTypes()
    {
        var mediaTypes = AllMediaTypes;

        var comps = new List<IContentTypeComposition>();
        foreach (var type in mediaTypes)
        {
            comps.AddRange(type.ContentTypeComposition.ToList());
        }

        return comps.DistinctBy(n => n.Id);
    }

    public IEnumerable<CompositionsWithTypes> GetAllCompositionsWithTypes()
    {
        var comps = new List<CompositionsWithTypes>();

        //Content
        var docTypes = AllDocTypes;
        foreach (var type in docTypes)
        {
            var compInfo = new CompositionsWithTypes();
            compInfo.Type = Enums.NodeType.Content;
            compInfo.ContentTypeAlias = type.Alias;
            compInfo.Compositions = type.ContentTypeComposition.ToList();

            comps.Add(compInfo);
        }

        //Media
        var mediaTypes = AllMediaTypes;
        foreach (var type in mediaTypes)
        {
            var compInfo = new CompositionsWithTypes();
            compInfo.Type = Enums.NodeType.Media;
            compInfo.ContentTypeAlias = type.Alias;
            compInfo.Compositions = type.ContentTypeComposition.ToList();

            comps.Add(compInfo);
        }

        return comps;
    }

    public IEnumerable<IDataType> GetNestedContentDataTypes()
    {
        var ncTypes = AllDataTypes.Where(n => n.EditorAlias == "Umbraco.NestedContent");
        return ncTypes;
    }

    public IEnumerable<IDataType> GetNestedContentDataTypes(string DocTypeAlias)
    {
        var ncTypes = GetNestedContentDataTypes();

        var matches = new List<IDataType>();
        foreach (var type in ncTypes)
        {
            var config = type.Configuration as NestedContentConfiguration;
            var ctypes = config.ContentTypes.Select(c => c.Alias);
            if (ctypes.Contains(DocTypeAlias))
            {
                matches.Add(type);
            }
        }

        return matches;
    }

    public IEnumerable<DocTypeProperty> GetPropertiesOnNestedContent(int DataTypeId)
    {
        var dType = _services.DataTypeService.GetDataType(DataTypeId);
        return GetPropertiesOnNestedContent(dType);
    }

    public IEnumerable<DocTypeProperty> GetPropertiesOnNestedContent(IDataType DataType)
    {
        var propsList = new List<DocTypeProperty>();

        if (DataType.EditorAlias != "Umbraco.NestedContent")
        {
            throw new ArgumentException(
                $"Provided DataType must be Umbraco.NestedContent. (Argument DataType: {DataType.Id} = {DataType.Name} ({DataType.EditorAlias}))");
        }

        var config = DataType.Configuration as NestedContentConfiguration;
        //var config = JsonConvert.DeserializeObject<NestedContentConfiguration>(configJson.ToString());

        foreach (var contentType in config.ContentTypes)
        {
            var docTypeAlias = contentType.Alias;
            var groupName = contentType.TabAlias;

            //get all related props
            var relatedProps =
                AllDocTypeProperties.Where(n => n.DocTypeAlias == docTypeAlias && n.GroupName == groupName);

            propsList.AddRange(relatedProps);
        }

        return propsList;
    }

    #endregion

    #region Process Data - Property to Property

    public PropToPropResultsSet ProcessPropertyToProperty(FormInputsPropertyToProperty FormInputs)
    {
        if (string.IsNullOrEmpty(FormInputs.PropertyAliasFrom) ||
            string.IsNullOrEmpty(FormInputs.PropertyAliasTo) || string.IsNullOrEmpty(FormInputs.TypeAlias))
        {
            var errMsg = $"ProcessPropertyToProperty: Form Inputs for TypeAlias ({FormInputs.TypeAlias}),  'To' Property ({FormInputs.PropertyAliasTo}), and 'From' Property ( {FormInputs.PropertyAliasFrom}) are all required.";
            throw new Exception(errMsg);
        }

        switch (FormInputs.PropToPropTypeOption)
        {
            case Enums.PropToPropType.DirectCopy:
                if (FormInputs.NodeTypes == Enums.NodeType.Content)
                {
                    return ProcessPropToPropDirectCopyContent(FormInputs);
                }
                else if (FormInputs.NodeTypes == Enums.NodeType.Media)
                {
                    return ProcessPropToPropDirectCopyMedia(FormInputs);
                }
                else
                {
                    throw new NotImplementedException(
                        "No Property to Property Direct Copy function available for " +
                        FormInputs.NodeTypes.ToString());
                }

            case Enums.PropToPropType.IntsToUdis:
                if (FormInputs.NodeTypes == Enums.NodeType.Content)
                {
                    return ProcessPropToPropIntToUdi(FormInputs);
                }
                //else if (FormInputs.NodeTypes == Enums.NodeType.Media)
                //{
                //    return ProcessPropToPropDirectCopyMedia(FormInputs);
                //}
                else
                {
                    throw new NotImplementedException(
                        "No Property to Property Direct Copy function available for " +
                        FormInputs.NodeTypes.ToString());
                }

            case Enums.PropToPropType.CustomTransformation:
                if (FormInputs.NodeTypes == Enums.NodeType.Content)
                {
                    return ProcessPropToPropCustomTransformer(FormInputs);
                }
                //else if (FormInputs.NodeTypes == Enums.NodeType.Media)
                //{
                //    return ProcessPropToPropCustomTransformer(FormInputs);
                //}
                else
                {
                    throw new NotImplementedException(
                        "No Property to Property Direct Copy function available for " +
                        FormInputs.NodeTypes.ToString());
                }

            default:
                if (FormInputs.NodeTypes == Enums.NodeType.Content)
                {
                    return ProcessPropToPropDirectCopyContent(FormInputs);
                }
                else if (FormInputs.NodeTypes == Enums.NodeType.Media)
                {
                    return ProcessPropToPropDirectCopyMedia(FormInputs);
                }
                else
                {
                    throw new NotImplementedException(
                        "No Property to Property Direct Copy function available for " +
                        FormInputs.NodeTypes.ToString());
                }
        }
    }

    public PropToPropResultsSet ProcessPropToPropDirectCopyContent(FormInputsPropertyToProperty FormInputs)
    {
        //SETUP
        var resultSet = new PropToPropResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.PropToPropType.DirectCopy;

        var results = new List<PropToPropResult>();

        //TODO: HLF - Add checking for valid matching transfer types - throw error if data cannot be transferred between different types
        var fromDocTypeProp = AllDocTypeProperties.Where(n =>
            n.DocTypeAlias == FormInputs.TypeAlias && n.Property.Alias == FormInputs.PropertyAliasFrom).First();
        var fromPropDataType = _services.DataTypeService.GetDataType(fromDocTypeProp.Property.DataTypeId);
        var fromPropDbType = fromPropDataType.DatabaseType;

        var toDocTypeProp = AllDocTypeProperties.Where(n =>
            n.DocTypeAlias == FormInputs.TypeAlias && n.Property.Alias == FormInputs.PropertyAliasTo).First();
        var toPropDataType = _services.DataTypeService.GetDataType(toDocTypeProp.Property.DataTypeId);
        var toPropDbType = toPropDataType.DatabaseType;

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var fromPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasFrom).ToList();
            var fromProp = fromPropMatches.Any() ? fromPropMatches.First() : null;

            var toPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasTo).ToList();
            var toProp = toPropMatches.Any() ? toPropMatches.First() : null;

            if (fromProp == null)
            {
                var errMsg = $"No property Matching the 'From' Property of {FormInputs.PropertyAliasFrom} exists on Content Node {node.Id}";
                throw new Exception(errMsg);
            }
            //else if (toProp == null)
            //{
            //    var errMsg = $"No property Matching the 'To' Property of {FormInputs.PropertyAliasTo} exists on Content Node {node.Id}";
            //    throw new Exception(errMsg);
            //}
            else
            {
                var result = new PropToPropResult();
                result.ContentNode = node;
                result.PropertyFromAlias = fromProp.Alias;
                result.PropertyToAlias = FormInputs.PropertyAliasTo;

                var fromPropData = fromProp.GetValue();
                result.PropertyFromData = fromPropData;
                result.PropertyFromDataFormat = fromPropData != null ? fromPropData.GetType().ToString() : "NULL";

                var originalToData = toProp != null ? toProp.GetValue() : null;
                result.PropertyToDataFormat = originalToData != null ? originalToData.GetType().ToString() : "NULL";

                if (fromPropData != null)
                {
                    if (toPropDbType == ValueStorageType.Ntext || toPropDbType == ValueStorageType.Nvarchar)
                    {
                        var stringToData = originalToData != null ? originalToData.ToString() : null;
                        if (string.IsNullOrEmpty(stringToData) || FormInputs.OverwriteExistingData)
                        {
                            result.PropertyToData = fromPropData.ToString();
                            result.ValidToTransfer = true;
                        }
                        else
                        {
                            result.PropertyToData = originalToData;
                        }
                    }
                    else if (toPropDbType == ValueStorageType.Integer)
                    {
                        var stringToData = originalToData != null ? originalToData.ToString() : null;
                        if (string.IsNullOrEmpty(stringToData) || FormInputs.OverwriteExistingData)
                        {
                            int intFrom;
                            var fromIsInt = Int32.TryParse(fromPropData.ToString(), out intFrom);

                            if (fromIsInt)
                            {
                                result.PropertyToData = intFrom;
                                result.ValidToTransfer = true;
                            }
                            else
                            {
                                result.DataFormatIsNotValidForTransfer = true;
                                result.PropertyToData = originalToData;
                            }
                        }
                        else
                        {
                            result.PropertyToData = originalToData;
                        }
                    }
                    else //TODO: Support other datatypes - decimal, datetime, etc.
                    {
                        result.DataFormatIsNotValidForTransfer = true;
                        result.PropertyToData = originalToData;
                    }
                }

                results.Add(result);
            }
        }

        //WRAP UP
        resultSet.Results = results;
        return resultSet;
    }

    public PropToPropResultsSet ProcessPropToPropDirectCopyMedia(FormInputsPropertyToProperty FormInputs)
    {
        //SETUP
        var resultSet = new PropToPropResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.PropToPropType.DirectCopy;

        var results = new List<PropToPropResult>();


        //TODO: HLF - Add checking for valid matching transfer types - throw error if data cannot be transferred between different types
        var fromDocTypeProp = AllMediaTypeProperties.Where(n =>
            n.DocTypeAlias == FormInputs.TypeAlias && n.Property.Alias == FormInputs.PropertyAliasFrom).First();
        var fromPropDataType = _services.DataTypeService.GetDataType(fromDocTypeProp.Property.DataTypeId);
        var fromPropDbType = fromPropDataType.DatabaseType;

        var toDocTypeProp = AllMediaTypeProperties.Where(n =>
            n.DocTypeAlias == FormInputs.TypeAlias && n.Property.Alias == FormInputs.PropertyAliasTo).First();
        var toPropDataType = _services.DataTypeService.GetDataType(toDocTypeProp.Property.DataTypeId);
        var toPropDbType = toPropDataType.DatabaseType;

        //get content
        var nodes = IdCsvToMedias(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var fromPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasFrom).ToList();
            var fromProp = fromPropMatches.Any() ? fromPropMatches.First() : null;

            var toPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasTo).ToList();
            var toProp = toPropMatches.Any() ? toPropMatches.First() : null;

            if (fromProp == null)
            {
                var errMsg = $"No property Matching the 'From' Property of {FormInputs.PropertyAliasFrom} exists on Media Node {node.Id}";
                throw new Exception(errMsg);
            }
            //else if (toProp == null)
            //{
            //    var errMsg = $"No property Matching the 'To' Property of {FormInputs.PropertyAliasTo} exists on Content Node {node.Id}";
            //    throw new Exception(errMsg);
            //}
            else
            {
                var result = new PropToPropResult();
                result.MediaNode = node;
                result.PropertyFromAlias = fromProp.Alias;
                result.PropertyToAlias = FormInputs.PropertyAliasTo;

                var fromPropData = fromProp.GetValue();
                result.PropertyFromData = fromPropData;
                result.PropertyFromDataFormat = fromPropData != null ? fromPropData.GetType().ToString() : "NULL";

                var originalToData = toProp != null ? toProp.GetValue() : null;
                result.PropertyToDataFormat = originalToData != null ? originalToData.GetType().ToString() : "NULL";

                if (fromPropData != null)
                {
                    if (toPropDbType == ValueStorageType.Ntext || toPropDbType == ValueStorageType.Nvarchar)
                    {
                        var stringToData = originalToData != null ? originalToData.ToString() : null;
                        if (string.IsNullOrEmpty(stringToData) || FormInputs.OverwriteExistingData)
                        {
                            result.PropertyToData = fromPropData.ToString();
                            result.ValidToTransfer = true;
                        }
                        else
                        {
                            result.PropertyToData = originalToData;
                        }
                    }
                    else if (toPropDbType == ValueStorageType.Integer)
                    {
                        var stringToData = originalToData != null ? originalToData.ToString() : null;
                        if (string.IsNullOrEmpty(stringToData) || FormInputs.OverwriteExistingData)
                        {
                            int intFrom;
                            var fromIsInt = Int32.TryParse(fromPropData.ToString(), out intFrom);

                            if (fromIsInt)
                            {
                                result.PropertyToData = intFrom;
                                result.ValidToTransfer = true;
                            }
                            else
                            {
                                result.DataFormatIsNotValidForTransfer = true;
                                result.PropertyToData = originalToData;
                            }
                        }
                        else
                        {
                            result.PropertyToData = originalToData;
                        }
                    }
                    else //TODO: Support other datatypes - decimal, datetime, etc.
                    {
                        result.DataFormatIsNotValidForTransfer = true;
                        result.PropertyToData = originalToData;
                    }
                }

                results.Add(result);
            }
        }

        //WRAP UP
        resultSet.Results = results;
        return resultSet;
    }

    public PropToPropResultsSet ProcessPropToPropIntToUdi(FormInputsPropertyToProperty FormInputs)
    {
        //SETUP
        var resultSet = new PropToPropResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.PropToPropType.IntsToUdis;

        var results = new List<PropToPropResult>();

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var fromPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasFrom).ToList();
            var fromProp = fromPropMatches.Any() ? fromPropMatches.First() : null;

            var toPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasTo).ToList();
            var toProp = toPropMatches.Any() ? toPropMatches.First() : null;

            if (fromProp == null)
            {
                var errMsg =
                    $"No property Matching the 'From' Property of {FormInputs.PropertyAliasFrom} exists on Media Node {node.Id}";
                throw new Exception(errMsg);
            }
            else
            {
                var result = new PropToPropResult();
                result.ContentNode = node;
                result.PropertyFromAlias = fromProp.Alias;
                result.PropertyToAlias = FormInputs.PropertyAliasTo;

                var fromPropData = fromProp.GetValue();
                result.PropertyFromData = fromPropData;
                result.PropertyFromDataFormat = fromPropData != null ? fromPropData.GetType().ToString() : "NULL";

                var originalToData = toProp != null ? toProp.GetValue() : null;
                result.PropertyToDataFormat = originalToData != null ? originalToData.GetType().ToString() : "NULL";

                if (fromPropData != null)
                {
                    if (toProp.ValueStorageType == ValueStorageType.Ntext ||
                        toProp.ValueStorageType == ValueStorageType.Nvarchar)
                    {
                        int intOriginalData = 0;

                        if (fromPropData is int)
                        {
                            intOriginalData = Convert.ToInt32(fromPropData);
                        }
                        else if (fromPropData is string)
                        {
                            var isInteger = Int32.TryParse(fromPropData.ToString(), out intOriginalData);
                        }
                        else //some other type, can't process
                        {
                            result.DataFormatIsNotValidForTransfer = true;
                            result.PropertyToData = originalToData;
                        }

                        if (intOriginalData != 0)
                        {
                            //Look for matching Content node
                            var lookupNodeContent = _services.ContentService.GetById(intOriginalData);

                            if (lookupNodeContent != null)
                            {
                                var newStringToData = lookupNodeContent.GetUdi().ToString();
                                if (originalToData == null || FormInputs.OverwriteExistingData)
                                {
                                    result.PropertyToData = newStringToData;
                                    result.ValidToTransfer = true;
                                }
                                else
                                {
                                    result.PropertyToData = originalToData;
                                }
                            }
                            else //Look for Media node
                            {
                                var lookupNodeMedia = _services.MediaService.GetById(intOriginalData);

                                if (lookupNodeMedia != null)
                                {
                                    var newStringToData = lookupNodeMedia.GetUdi().ToString();
                                    if (originalToData == null || FormInputs.OverwriteExistingData)
                                    {
                                        result.PropertyToData = newStringToData;
                                        result.ValidToTransfer = true;
                                    }
                                    else
                                    {
                                        result.PropertyToData = originalToData;
                                    }
                                }
                                else //No matching node, skip update
                                {
                                    result.PropertyToData = originalToData;
                                }
                            }
                        }
                        else //original data is not an int
                        {
                            result.DataFormatIsNotValidForTransfer = true;
                            result.PropertyToData = originalToData;
                        }
                    }
                    else
                    {
                        result.DataFormatIsNotValidForTransfer = true;
                        result.PropertyToData = originalToData;
                    }
                }
                else
                {
                    result.DataFormatIsNotValidForTransfer = true;
                    result.PropertyToData = originalToData;
                }

                results.Add(result);
            }
        }

        //WRAP UP
        resultSet.Results = results;
        return resultSet;
    }

    public PropToPropResultsSet ProcessPropToPropCustomTransformer(FormInputsPropertyToProperty FormInputs)
    {
        //SETUP
        var resultSet = new PropToPropResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.PropToPropType.CustomTransformation;

        var results = new List<PropToPropResult>();

        //Test for Valid Custom option
        if (string.IsNullOrEmpty(FormInputs.CustomTransformationClass))
        {
            //Error - return empty
            resultSet.HasError = true;
            resultSet.ErrorMessage = "No Custom Transformer Class Provided";
            resultSet.Results = results;
            return resultSet;
        }

        ICustomPropToPropDataTransformer? customTransformer = ReflectionHelper.GetAssemblyTypeInstance(_Provider,FormInputs.CustomTransformationClass) as ICustomPropToPropDataTransformer;

        if (customTransformer == null)
        {
            //Error - return empty
            resultSet.HasError = true;
            resultSet.ErrorMessage = $"Unable to get an instance of Transformer '{FormInputs.CustomTransformationClass}'";
            resultSet.Results = results;
            return resultSet;
        }

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var fromPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasFrom).ToList();
            var fromProp = fromPropMatches.Any() ? fromPropMatches.First() : null;

            var toPropMatches = node.Properties.Where(n => n.Alias == FormInputs.PropertyAliasTo).ToList();
            var toProp = toPropMatches.Any() ? toPropMatches.First() : null;

            var result = new PropToPropResult();
            result.ContentNode = node;
            result.PropertyToAlias = FormInputs.PropertyAliasTo;

            if (fromProp == null)
            {
                var errMsg =
                    $"No property Matching the 'From' Property of {FormInputs.PropertyAliasFrom} exists on Content Node {node.Id}";
                result.Status = errMsg;
            }
            else
            {
                result.PropertyFromAlias = fromProp.Alias;

                var fromPropData = fromProp.GetValue();
                result.PropertyFromData = fromPropData;
                result.PropertyFromDataFormat = fromPropData != null ? fromPropData.GetType().ToString() : "NULL";

                var originalToData = toProp != null ? toProp.GetValue() : null;
                result.PropertyToDataFormat = originalToData != null ? originalToData.GetType().ToString() : "NULL";

                //Check that Transformer is valid for this data
                var notValidReasonMsg = "";
                if (customTransformer.IsValidForData(node.Id, node.ContentType.Alias, fromProp.PropertyType, fromPropData, toProp.PropertyType, originalToData, out notValidReasonMsg))
                {
                    var conversionError = "";
                    var newData = customTransformer.TransformData(fromPropData, out conversionError);

                    if (string.IsNullOrEmpty(conversionError))
                    {
                        result.ValidToTransfer = true;
                        result.PropertyToData = newData;
                    }
                    else
                    {
                        result.Status = conversionError;
                        result.PropertyToData = originalToData;
                    }
                }
                else
                {
                    //Transformer not valid for this
                    result.DataFormatIsNotValidForTransfer = true;
                    result.PropertyToData = originalToData;
                    result.Status = notValidReasonMsg;
                }
            }

            results.Add(result);
        }

        //WRAP UP
        resultSet.Results = results;
        return resultSet;
    }

    #endregion

    #region Process Data - Find/Replace
    public FindReplaceResultsSet ProcessFindReplace(FormInputsFindReplace FormInputs)
    {
        switch (FormInputs.FindReplaceTypeOption)
        {
            case Enums.FindReplaceType.TextToText:
                return ProcessFindReplaceText(FormInputs);

            case Enums.FindReplaceType.IntsToUdis:
                return ProcessFindReplaceIntUdi(FormInputs);

            case Enums.FindReplaceType.CustomTransformation:
                return ProcessFindReplaceCustomTransformer(FormInputs);
            default:
                return ProcessFindReplaceText(FormInputs);
        }
    }

    private FindReplaceResultsSet ProcessFindReplaceCustomTransformer(FormInputsFindReplace FormInputs)
    {
        var resultSet = new FindReplaceResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.FindReplaceType.CustomTransformation;

        var aliases = CsvToEnumerable(FormInputs.PropertyAliasesCsv);
        resultSet.PropertyAliases = aliases;

        var results = new List<FindReplaceResult>();

        //Test for Valid Custom option
        if (string.IsNullOrEmpty(FormInputs.CustomTransformationClass))
        {
            //Error - return empty
            resultSet.HasError = true;
            resultSet.ErrorMessage = "No Custom Transformer Class Provided";
            resultSet.Results = results;
            return resultSet;
        }

        ICustomFindReplaceTransformer? customTransformer = ReflectionHelper.GetAssemblyTypeInstance(_Provider,FormInputs.CustomTransformationClass) as ICustomFindReplaceTransformer;

        if (customTransformer == null)
        {
            //Error - return empty
            resultSet.HasError = true;
            resultSet.ErrorMessage = $"Unable to get an instance of Transformer '{FormInputs.CustomTransformationClass}'";
            resultSet.Results = results;
            return resultSet;
        }

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var propsData = node.Properties.Where(n => aliases.Contains(n.Alias));

            foreach (var propertyData in propsData)
            {
                var originalData = propertyData.GetValue();

                var result = new FindReplaceResult();
                result.ContentNode = node;
                result.PropertyAlias = propertyData.Alias;
                result.OriginalPropertyData = originalData;

                //var testPropertyType = AssemblyHelpers.TestSerializable(typeof(PropertyType));
                //var testObject = AssemblyHelpers.TestSerializable(typeof(object));
                //var testIenumString = AssemblyHelpers.TestSerializable(typeof(IEnumerable<string>));
                //var testListString = AssemblyHelpers.TestSerializable(typeof(List<string>));

                //Check that Transformer is valid for this data
                var notValidReasonMsg = "";
                if (customTransformer.IsValidForData(node.Id, node.ContentType.Alias, propertyData.PropertyType, originalData, out notValidReasonMsg))
                {
                    if (originalData != null)
                    {
                        var conversionError = "";
                        var newData = customTransformer.ConvertOriginalData(originalData, out conversionError);

                        result.NewPropertyData = newData;
                        result.Status = conversionError;
                        result.MatchFound = true;
                    }
                }
                else
                {
                    //Transformer not valid for this
                    result.DataFormatIsNotValidForReplace = true;
                    result.NewPropertyData = originalData;
                    result.Status = notValidReasonMsg;
                }

                results.Add(result);
            }
        }


        resultSet.Results = results;
        return resultSet;
    }

    public FindReplaceResultsSet ProcessFindReplaceText(FormInputsFindReplace FormInputs)
    {
        var resultSet = new FindReplaceResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.FindReplaceType.TextToText;

        var aliases = CsvToEnumerable(FormInputs.PropertyAliasesCsv);
        resultSet.PropertyAliases = aliases;

        var results = new List<FindReplaceResult>();

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var propsData = node.Properties.Where(n => aliases.Contains(n.Alias));

            foreach (var propertyData in propsData)
            {
                var originalData = propertyData.GetValue();

                var result = new FindReplaceResult();
                result.ContentNode = node;
                result.PropertyAlias = propertyData.Alias;
                result.OriginalPropertyData = originalData;
                result.FindStrings.Add(FormInputs.Find);
                result.ReplaceStrings.Add(FormInputs.Replace);

                if (originalData is string)
                {
                    var stringData = originalData.ToString();
                    if (stringData.Contains(FormInputs.Find))
                    {
                        result.MatchFound = true;
                        result.NewPropertyData = stringData.Replace(FormInputs.Find, FormInputs.Replace);
                    }
                    else
                    {
                        result.NewPropertyData = originalData;
                    }

                }
                else
                {
                    result.DataFormatIsNotValidForReplace = true;
                    result.NewPropertyData = originalData;
                }

                results.Add(result);
            }
        }

        resultSet.Results = results;
        return resultSet;
    }

    public FindReplaceResultsSet ProcessFindReplaceIntUdi(FormInputsFindReplace FormInputs)
    {
        //Determine Type of Replacement
        if (FormInputs.FullPropertyReplace)
        {
            if (FormInputs.FullPropertyIsMultiple)
            {
                return ProcessFR_FullPropMultiple(FormInputs);
            }
            else
            {
                return ProcessFR_FullPropSingle(FormInputs);
            }
        }
        else
        {
            return ProcessFR_SearchInText(FormInputs);
        }

    }

    private FindReplaceResultsSet ProcessFR_SearchInText(FormInputsFindReplace FormInputs)
    {
        var resultSet = new FindReplaceResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.FindReplaceType.IntsToUdis;

        //Setup
        var aliases = CsvToEnumerable(FormInputs.PropertyAliasesCsv);
        resultSet.PropertyAliases = aliases;

        //Make some alterations to the provided Find and Replace values
        var formFindString = FormInputs.Find;
        var formReplaceString = FormInputs.Replace;

        //If the fields are empty, assume full-field replacement 
        if (string.IsNullOrEmpty(formFindString))
        {
            formFindString = "~ID~";
        }
        else if (formFindString.Contains(@"\"))
        {
            //escape special chars in Find for regex
            formFindString = formFindString.Replace(@"\", @"\\");
        }

        if (string.IsNullOrEmpty(formReplaceString))
        {
            formReplaceString = "~UDI~";
        }

        var results = new List<FindReplaceResult>();

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var propsData = node.Properties.Where(n => aliases.Contains(n.Alias));

            foreach (var propertyData in propsData)
            {
                var originalData = propertyData.GetValue();

                var result = new FindReplaceResult();
                result.ContentNode = node;
                result.PropertyAlias = propertyData.Alias;
                result.OriginalPropertyData = originalData;
                result.CurrentDataFormat = originalData != null ? originalData.GetType().ToString() : "NULL";

                if (originalData is string)
                {
                    //Since we are looping, this value with continually be updated with each match
                    var newData = originalData.ToString();

                    //Use Regex to locate the "Find" substring
                    var constructRegex = formFindString.Replace("~ID~", @"\d+");
                    Regex regexFindString = new Regex(constructRegex);

                    var findMatches = regexFindString.Matches(originalData.ToString());
                    if (findMatches.Count > 0)
                    {
                        //Loop through all matches replacing values
                        foreach (Match findMatch in findMatches)
                        {
                            var findString = findMatch.Value;
                            result.FindStrings.Add(findString);

                            //Get the INT ID
                            Regex regexId = new Regex(@"\d+");
                            Match idMatch = regexId.Match(findString);
                            if (idMatch.Success)
                            {
                                string foundId = idMatch.Value;

                                int intOriginalId;
                                var isInteger = Int32.TryParse(foundId, out intOriginalId);
                                if (isInteger)
                                {
                                    //Get the new UDI
                                    var lookupContentNode = _services.ContentService.GetById(intOriginalId);
                                    if (lookupContentNode != null)
                                    {
                                        //Create the Replace string
                                        var replaceString =
                                            formReplaceString.Replace("~UDI~", lookupContentNode.GetUdi().ToString());
                                        result.ReplaceStrings.Add(replaceString);

                                        //UPDATE THE DATA
                                        newData = newData.Replace(findString, replaceString);
                                    }
                                    else
                                    {
                                        //Try Media
                                        var lookupMediaNode = _services.MediaService.GetById(intOriginalId);
                                        if (lookupMediaNode != null)
                                        {
                                            //Create the Replace string
                                            var replaceString =
                                                formReplaceString.Replace("~UDI~", lookupMediaNode.GetUdi().ToString());
                                            result.ReplaceStrings.Add(replaceString);

                                            //UPDATE THE DATA
                                            newData = newData.Replace(findString, replaceString);
                                        }
                                        else
                                        {
                                            //no node found, add a message and continue
                                            var msg = $"Unable to find a matching node for Id {intOriginalId}";
                                            result.Status = result.Status + "; " + msg;
                                            result.ReplaceStrings.Add($"[{msg}]");
                                        }
                                    }
                                }
                                else
                                {
                                    //Not a valid int, add a message and continue
                                    var msg = $"{foundId} is not a valid Integer Id ({findString})";
                                    result.Status = result.Status + "; " + msg;
                                    result.ReplaceStrings.Add($"[{msg}]");
                                }
                            }
                            else
                            {
                                //Unable to get Int ID, add a message and continue
                                var msg = $"Unable to get a valid Integer Id for {findString}";
                                result.Status = result.Status + "; " + msg;
                                result.ReplaceStrings.Add($"[{msg}]");
                            }
                        }

                        //All done replacing...
                        result.Status = $"OK";
                        result.MatchFound = true;
                        result.NewPropertyData = newData;
                    }
                    else
                    {
                        //No matches found
                        result.Status = $"No Match";
                        result.NewPropertyData = originalData;
                    }
                }
                else
                {
                    //Original Data is not a string value
                    result.Status = $"Original Data is not a string value";
                    result.DataFormatIsNotValidForReplace = true;
                    result.NewPropertyData = originalData;
                }

                results.Add(result);
            }
        }

        resultSet.Results = results;
        return resultSet;
    }

    private FindReplaceResultsSet ProcessFR_FullPropSingle(FormInputsFindReplace FormInputs)
    {
        //TODO: Simplify this
        var resultSet = new FindReplaceResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.FindReplaceType.IntsToUdis;

        //Setup
        var aliases = CsvToEnumerable(FormInputs.PropertyAliasesCsv);
        resultSet.PropertyAliases = aliases;

        ////Make some alterations to the provided Find and Replace values
        //var formFindString = FormInputs.Find;
        //var formReplaceString = FormInputs.Replace;

        ////If the fields are empty, assume full-field replacement 
        //if (string.IsNullOrEmpty(formFindString))
        //{
        //    formFindString = "~ID~";
        //}
        //else if (formFindString.Contains(@"\"))
        //{
        //    //escape special chars in Find for regex
        //    formFindString = formFindString.Replace(@"\", @"\\");
        //}

        //if (string.IsNullOrEmpty(formReplaceString))
        //{
        //    formReplaceString = "~UDI~";
        //}


        var results = new List<FindReplaceResult>();

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var propsData = node.Properties.Where(n => aliases.Contains(n.Alias));

            foreach (var propertyData in propsData)
            {
                var originalData = propertyData.GetValue();

                var result = new FindReplaceResult();
                result.ContentNode = node;
                result.PropertyAlias = propertyData.Alias;
                result.OriginalPropertyData = originalData;
                result.CurrentDataFormat = originalData != null ? originalData.GetType().ToString() : "NULL";

                if (originalData is string)
                {
                    var origString = originalData.ToString();
                    var newUdi = "";

                    //Check that it needs fixing
                    if (!origString.StartsWith("umb://"))
                    {
                        //Get the INT ID
                        int intOriginalId;
                        var isInteger = Int32.TryParse(origString, out intOriginalId);
                        if (isInteger)
                        {
                            //Get the new UDI
                            var lookupContentNode = _services.ContentService.GetById(intOriginalId);
                            if (lookupContentNode != null)
                            {
                                newUdi = lookupContentNode.GetUdi().ToString();
                            }
                            else
                            {
                                //Try Media
                                var lookupMediaNode = _services.MediaService.GetById(intOriginalId);
                                if (lookupMediaNode != null)
                                {
                                    newUdi = lookupMediaNode.GetUdi().ToString();
                                }
                                else
                                {
                                    //no node found, add a message and continue
                                    var msg = $"Unable to find a matching node for Id {intOriginalId}";
                                    result.Status = result.Status + "; " + msg;
                                    result.ReplaceStrings.Add($"[{msg}]");
                                }
                            }

                            //All done replacing...
                            result.Status = $"OK";
                            result.MatchFound = true;
                            result.NewPropertyData = newUdi;
                        }
                        else
                        {
                            //Not a valid int, add a message and continue
                            var msg = $"{origString} is not a valid Integer Id";
                            result.Status = result.Status + "; " + msg;
                            result.ReplaceStrings.Add($"[{msg}]");
                        }
                    }
                    else
                    {
                        //Already a UDI
                        result.Status = $"Original Data is already a UDI";
                        result.MatchFound = false;
                        result.NewPropertyData = originalData;
                    }
                }
                else
                {
                    //Original Data is not a string value
                    result.Status = $"Original Data is not a string value";
                    result.DataFormatIsNotValidForReplace = true;
                    result.NewPropertyData = originalData;
                }

                results.Add(result);
            }
        }

        resultSet.Results = results;
        return resultSet;
    }

    private FindReplaceResultsSet ProcessFR_FullPropMultiple(FormInputsFindReplace FormInputs)
    {
        var resultSet = new FindReplaceResultsSet();
        resultSet.FormInputs = FormInputs;
        resultSet.Type = Enums.FindReplaceType.IntsToUdis;

        //Setup
        var aliases = CsvToEnumerable(FormInputs.PropertyAliasesCsv);
        resultSet.PropertyAliases = aliases;

        var results = new List<FindReplaceResult>();

        //get content
        var nodes = IdCsvToContents(FormInputs.ContentNodeIdsCsv);

        //loop
        foreach (var node in nodes)
        {
            var propsData = node.Properties.Where(n => aliases.Contains(n.Alias));

            foreach (var propertyData in propsData)
            {
                var originalData = propertyData.GetValue();

                var result = new FindReplaceResult();
                result.ContentNode = node;
                result.PropertyAlias = propertyData.Alias;
                result.OriginalPropertyData = originalData;
                result.CurrentDataFormat = originalData != null ? originalData.GetType().ToString() : "NULL";

                if (originalData is string)
                {
                    //Since we are looping, this value with continually be updated with each match
                    var newDataList = new List<string>();

                    //Get list of all Ids
                    var originalIdsList = SplitDataIntoList(originalData.ToString());

                    //Use Regex to locate the "Find" substring
                    //var constructRegex = formFindString.Replace("~ID~", @"\d+");
                    //Regex regexFindString = new Regex(constructRegex);

                    //var findMatches = regexFindString.Matches(originalData.ToString());

                    //Loop through all matches replacing values
                    foreach (var strId in originalIdsList)
                    {
                        result.FindStrings.Add(strId);

                        //Get the INT ID
                        int intOriginalId;
                        var isInteger = Int32.TryParse(strId, out intOriginalId);
                        if (isInteger)
                        {
                            //Get the new UDI
                            var lookupContentNode = _services.ContentService.GetById(intOriginalId);
                            if (lookupContentNode != null)
                            {
                                //Add to New Data
                                var contentUdi = lookupContentNode.GetUdi().ToString();
                                newDataList.Add(contentUdi);
                                result.ReplaceStrings.Add(contentUdi);
                                result.MatchFound = true;
                            }
                            else
                            {
                                //Try Media
                                var lookupMediaNode = _services.MediaService.GetById(intOriginalId);
                                if (lookupMediaNode != null)
                                {
                                    //Add to New Data
                                    var mediaUdi = lookupMediaNode.GetUdi().ToString();
                                    newDataList.Add(mediaUdi);
                                    result.ReplaceStrings.Add(mediaUdi);
                                    result.MatchFound = true;
                                }
                                else
                                {
                                    //no node found, add a message and continue
                                    var msg = $"Unable to find a matching node for Id {intOriginalId}";
                                    result.Status = result.Status + "; " + msg;
                                    newDataList.Add(strId); //Original ID added again
                                    result.ReplaceStrings.Add($"[{msg}]");
                                }
                            }
                        }
                        else
                        {
                            //Not a valid int, add a message and continue
                            var msg = $"{strId} is not a valid Integer Id";
                            result.Status = result.Status + "; " + msg;
                            newDataList.Add(strId); //Original ID added again
                            result.ReplaceStrings.Add($"[{msg}]");
                        }
                    }

                    //All done replacing for this propertyData...
                    result.NewPropertyData = string.Join(",", newDataList);
                }
                else
                {
                    //Original Data is not a string value
                    result.Status = $"Original Data is not a string value";
                    result.DataFormatIsNotValidForReplace = true;
                    result.NewPropertyData = originalData;
                }

                results.Add(result);
            }
        }

        //All nodes done
        resultSet.Results = results;
        return resultSet;
    }

    #endregion

    #region Process Data - Store Legacy Data

    public LegacyDataResultsSet ProcessStoreLegacyData(FormInputsStoreLegacyData FormInputs)
    {
        //SETUP
        var resultSet = new LegacyDataResultsSet();
        resultSet.FormInputs = FormInputs;

        var results = new List<LegacyDataResult>();

        //STORE CONTENT ID
        if (!string.IsNullOrEmpty(FormInputs.PropertyAliasContentNodeId))
        {
            //Get all doctypes with the property
            var propAlias = FormInputs.PropertyAliasContentNodeId;
            var docTypesWithProps = AllDocTypeProperties.Where(n => n.Property.Alias == propAlias);
            var docTypeAliases = docTypesWithProps.Select(n => n.DocTypeAlias).Distinct().ToList();

            //get content
            var nodes = AllContent.Where(n => docTypeAliases.Contains(n.ContentType.Alias));

            //loop
            foreach (var node in nodes)
            {
                var legacyId = node.Id;

                var result = new LegacyDataResult();
                result.Type = Enums.NodeType.Content;
                result.ContentNode = node;
                result.IdPropertyAlias = propAlias;

                //Check that the property can accept an INT value
                var thisProp = docTypesWithProps.Where(n => n.DocTypeAlias == node.ContentType.Alias).First();
                var legacyPropDataType = _services.DataTypeService.GetDataType(thisProp.Property.DataTypeId);
                var dbType = legacyPropDataType.DatabaseType;
                var currentLegacyData = node.GetValue(propAlias);

                if (dbType == ValueStorageType.Ntext || dbType == ValueStorageType.Nvarchar ||
                    dbType == ValueStorageType.Integer)
                {
                    var currentDataString = currentLegacyData != null ? currentLegacyData.ToString() : null;
                    if (string.IsNullOrEmpty(currentDataString) || FormInputs.OverwriteExistingData)
                    {
                        result.IdData = legacyId;
                        result.ValidToTransfer = true;
                    }
                    else
                    {
                        result.IdData = currentLegacyData;
                    }
                }
                else
                {
                    result.DataFormatIsNotValidForTransfer = true;
                    result.IdData = currentLegacyData;
                }

                results.Add(result);
            }
        }

        //STORE MEDIA ID
        if (!string.IsNullOrEmpty(FormInputs.PropertyAliasMediaNodeId))
        {
            //Get all mediaTypes with the property
            var propAlias = FormInputs.PropertyAliasMediaNodeId;
            var mediaTypesWithProps = AllMediaTypeProperties.Where(n => n.Property.Alias == propAlias);
            var mediaTypeAliases = mediaTypesWithProps.Select(n => n.DocTypeAlias).Distinct().ToList();

            //get media nodes
            var nodes = AllMedia.Where(n => mediaTypeAliases.Contains(n.ContentType.Alias));

            //loop
            foreach (var node in nodes)
            {
                var legacyId = node.Id;

                var result = new LegacyDataResult();
                result.Type = Enums.NodeType.Media;
                result.MediaNode = node;
                result.IdPropertyAlias = propAlias;

                //Check that the property can accept an INT value
                var thisProp = mediaTypesWithProps.Where(n => n.DocTypeAlias == node.ContentType.Alias).First();
                var legacyPropDataType = _services.DataTypeService.GetDataType(thisProp.Property.DataTypeId);
                var dbType = legacyPropDataType.DatabaseType;
                var currentLegacyData = node.GetValue(propAlias);

                if (dbType == ValueStorageType.Ntext || dbType == ValueStorageType.Nvarchar ||
                    dbType == ValueStorageType.Integer)
                {
                    var currentDataString = currentLegacyData != null ? currentLegacyData.ToString() : null;
                    if (string.IsNullOrEmpty(currentDataString) || FormInputs.OverwriteExistingData)
                    {
                        result.IdData = legacyId;
                        result.ValidToTransfer = true;
                    }
                    else
                    {
                        result.IdData = currentLegacyData;
                    }
                }
                else
                {
                    result.DataFormatIsNotValidForTransfer = true;
                    result.IdData = currentLegacyData;
                }

                results.Add(result);
            }
        }

        //FINISH UP
        resultSet.Results = results;
        return resultSet;
    }

    #endregion

    #region Process Data - Lookup Udi

    public UdiLookupResult LookupUdi(FormInputsUdiLookup FormInputs)
    {
        var result = new UdiLookupResult(Enums.LookupStatus.SearchInProgress);
        result.LookupCriteria = FormInputs;

        var types = Enums.UmbracoObjectTypesWithDisplayText();

        if (!string.IsNullOrEmpty(FormInputs.Udi))
        {
            //Lookup type from UDI string
            result.Udi = FormInputs.Udi;

            var udiSplit = FormInputs.Udi.Replace("umb://", "").TrimEnd('/').Split('/');
            var udiType = udiSplit[0];
            var udiGuid = Guid.ParseExact(udiSplit[1], "N");
            result.Guid = udiGuid;
            result.LookupCriteria.Guid = udiGuid.ToString();

            var matchingType = Enums.ConvertStringToUmbracoObjectType(udiType);
            result.ObjectType = matchingType;
            result.ObjectTypeDisplayName = types[matchingType];
            result.LookupCriteria.ObjectType = matchingType;

            if (matchingType == Enums.UmbracoObjectType.Unknown)
            {
                //No type found 
                result.Status = Enums.LookupStatus.Error;
                result.ErrorMsg = $"No Umbraco Object Type found for '{udiType}'";
            }
        }
        else
        {
            //Use provided info
            result.ObjectType = FormInputs.ObjectType;
            result.ObjectTypeDisplayName = types[FormInputs.ObjectType];
            result.Guid = Guid.ParseExact(FormInputs.Guid, "N");
        }

        //Lookup object using Type and Guid
        if (result.Guid != null)
        {
            switch (result.ObjectType)
            {
                case Enums.UmbracoObjectType.Content:
                    var content = _services.ContentService.GetById(result.Guid);
                    if (content != null)
                    {
                        result.Status = Enums.LookupStatus.ObjectFound;
                        result.Id = content.Id;
                        result.Name = content.Name;
                    }
                    else
                    {
                        result.Status = Enums.LookupStatus.ObjectNotFound;
                    }
                    break;

                case Enums.UmbracoObjectType.Media:
                    var media = _services.MediaService.GetById(result.Guid);
                    if (media != null)
                    {
                        result.Status = Enums.LookupStatus.ObjectFound;
                        result.Id = media.Id;
                        result.Name = media.Name;
                    }
                    else
                    {
                        result.Status = Enums.LookupStatus.ObjectNotFound;
                    }
                    break;

                case Enums.UmbracoObjectType.UmbracoForm:
                    //var form = _formService.Get(result.Guid);
                    //if (form != null)
                    //{
                    //    result.Status = Enums.LookupStatus.ObjectFound;
                    //    //result.Id = form.Id;
                    //    result.Name = form.Name;
                    //}
                    //else
                    //{
                    result.Status = Enums.LookupStatus.ObjectTypeNotSupported;
                    result.ErrorMsg =
                        $"Forms are not supported for lookup operations. Use the <a href=\"/umbraco/#/forms/form/edit/{result.Guid}\" target=\"_blank\">back-office section</a> ";
                    // }
                    break;

                case Enums.UmbracoObjectType.ContentType:
                    var contentType = _services.ContentTypeService.Get(result.Guid);
                    if (contentType != null)
                    {
                        result.Status = Enums.LookupStatus.ObjectFound;
                        result.Id = contentType.Id;
                        result.Name = contentType.Name;
                    }
                    else
                    {
                        result.Status = Enums.LookupStatus.ObjectNotFound;
                    }
                    break;

                case Enums.UmbracoObjectType.MediaType:
                    var mediaType = _services.MediaTypeService.Get(result.Guid);
                    if (mediaType != null)
                    {
                        result.Status = Enums.LookupStatus.ObjectFound;
                        result.Id = mediaType.Id;
                        result.Name = mediaType.Name;
                    }
                    else
                    {
                        result.Status = Enums.LookupStatus.ObjectNotFound;
                    }
                    break;

                case Enums.UmbracoObjectType.DataType:
                    var datatype = _services.DataTypeService.GetDataType(result.Guid);
                    if (datatype != null)
                    {
                        result.Status = Enums.LookupStatus.ObjectFound;
                        result.Id = datatype.Id;
                        result.Name = datatype.Name;
                    }
                    else
                    {
                        result.Status = Enums.LookupStatus.ObjectNotFound;
                    }
                    break;

                case Enums.UmbracoObjectType.Unknown:
                    //Ignore lookup
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            //No valid GUID
            result.Status = Enums.LookupStatus.Error;
            result.ErrorMsg = $"No valid GUID provided.";
        }

        return result;
    }

    #endregion

    #region Process Data - Helpers

    private IEnumerable<string> SplitDataIntoList(string OriginalData)
    {
        if (OriginalData.DetectIsJson())
        {
            //TODO: Figure out JSON parsing?
            return OriginalData.AsEnumerableOfOne();
        }
        else
        {
            //Assume CSV
            return OriginalData.Split(',');
        }
    }

    public IEnumerable<string> CsvToEnumerable(string StringsCsv)
    {
        if (StringsCsv != null)
        {
            var strings = StringsCsv.Split(',').ToList();

            return strings;
        }
        else
        {
            return new List<string>();
        }
    }

    public IEnumerable<IContent> IdCsvToContents(string IdsCsv)
    {
        if (IdsCsv != null)
        {
            var idStrings = IdsCsv.Split(',');
            var idInts = idStrings.Select(n => Convert.ToInt32(n));
            var nodes = _services.ContentService.GetByIds(idInts);

            return nodes;
        }
        else
        {
            return new List<IContent>();
        }
    }

    public IEnumerable<IMedia> IdCsvToMedias(string IdsCsv)
    {
        if (IdsCsv != null)
        {
            var idStrings = IdsCsv.Split(',');
            var idInts = idStrings.Select(n => Convert.ToInt32(n));
            var nodes = _services.MediaService.GetByIds(idInts);

            return nodes;
        }
        else
        {
            return new List<IMedia>();
        }
    }

    #endregion


    #region Fill Properties with Collections

    private void FillDocTypesList()
    {
        _allDocTypes = _services.ContentTypeService.GetAll().ToList();
    }
    private void FillMediaTypesList()
    {
        _allMediaTypes = _services.MediaTypeService.GetAll().ToList();
    }
    private void FillDataTypesList()
    {
        _allDataTypes = _services.DataTypeService.GetAll().ToList();
    }
    private void FillPropertyEditorsList()
    {
        var allEditors = AllDataTypes.Select(n => n.Editor);
        _allPropertyEditorTypes = allEditors.DistinctBy(n => n.Alias);
    }
    private void FillPropertiesList()
    {
        var docTypes = this.AllDocTypes;
        var props = new List<DocTypeProperty>();

        foreach (var docType in docTypes)
        {
            //var dtCompsCount = docType.ContentTypeComposition.Count();

            //Props in No Group (tab)
            foreach (var propType in docType.NoGroupPropertyTypes)
            {
                var dtProp = new DocTypeProperty();
                dtProp.DocTypeAlias = docType.Alias;
                dtProp.GroupName = "";
                dtProp.Property = propType;
                dtProp.CompositionDocTypeAlias = "";
                props.Add(dtProp);
            }

            //Props in groups
            foreach (var propGroup in docType.PropertyGroups)
            {
                foreach (var propType in propGroup.PropertyTypes)
                {
                    var dtProp = new DocTypeProperty();
                    dtProp.DocTypeAlias = docType.Alias;
                    dtProp.GroupName = propGroup.Name;
                    dtProp.Property = propType;
                    dtProp.CompositionDocTypeAlias = "";

                    props.Add(dtProp);
                }
            }

            //Props in Compositions
            foreach (var comp in docType.ContentTypeComposition)
            {
                //Comp Props in No Group
                foreach (var propType in comp.NoGroupPropertyTypes)
                {
                    var dtProp = new DocTypeProperty();
                    dtProp.DocTypeAlias = docType.Alias;
                    dtProp.GroupName = "";
                    dtProp.Property = propType;
                    dtProp.CompositionDocTypeAlias = comp.Alias;

                    props.Add(dtProp);
                }

                //Comp Props in groups
                foreach (var propGroup in comp.PropertyGroups)
                {
                    foreach (var propType in propGroup.PropertyTypes)
                    {
                        var dtProp = new DocTypeProperty();
                        dtProp.DocTypeAlias = docType.Alias;
                        dtProp.GroupName = propGroup.Name;
                        dtProp.Property = propType;
                        dtProp.CompositionDocTypeAlias = comp.Alias;

                        props.Add(dtProp);
                    }
                }
            }

            //props.AddRange(docType.NoGroupPropertyTypes);
            //props.AddRange(docType.CompositionPropertyTypes);
        }

        _allDocTypeProperties = props;
    }

    private void FillMediaPropertiesList()
    {
        var mediaTypes = this.AllMediaTypes;
        var props = new List<DocTypeProperty>();

        foreach (var type in mediaTypes)
        {
            //var dtCompsCount = docType.ContentTypeComposition.Count();

            //Props in No Group
            foreach (var propType in type.NoGroupPropertyTypes)
            {
                var dtProp = new DocTypeProperty();
                dtProp.DocTypeAlias = type.Alias;
                dtProp.GroupName = "";
                dtProp.Property = propType;

                props.Add(dtProp);
            }

            //Props in groups
            foreach (var propGroup in type.PropertyGroups)
            {
                foreach (var propType in propGroup.PropertyTypes)
                {
                    var dtProp = new DocTypeProperty();
                    dtProp.DocTypeAlias = type.Alias;
                    dtProp.GroupName = propGroup.Name;
                    dtProp.Property = propType;

                    props.Add(dtProp);
                }
            }

            //Props in Compositions
            foreach (var comp in type.ContentTypeComposition)
            {
                //Comp Props in No Group
                foreach (var propType in comp.NoGroupPropertyTypes)
                {
                    var dtProp = new DocTypeProperty();
                    dtProp.DocTypeAlias = type.Alias;
                    dtProp.GroupName = "";
                    dtProp.Property = propType;
                    dtProp.CompositionDocTypeAlias = comp.Alias;

                    props.Add(dtProp);
                }

                //Comp Props in groups
                foreach (var propGroup in comp.PropertyGroups)
                {
                    foreach (var propType in propGroup.PropertyTypes)
                    {
                        var dtProp = new DocTypeProperty();
                        dtProp.DocTypeAlias = type.Alias;
                        dtProp.GroupName = propGroup.Name;
                        dtProp.Property = propType;
                        dtProp.CompositionDocTypeAlias = comp.Alias;

                        props.Add(dtProp);
                    }
                }
            }

            //props.AddRange(docType.NoGroupPropertyTypes);
            //props.AddRange(docType.CompositionPropertyTypes);
        }

        _allMediaTypeProperties = props;
    }
    public void FillAllContent()
    {
        var rootContent = _services.ContentService.GetRootContent();
        if (rootContent != null && rootContent.Any())
        {
            foreach (var c in rootContent)
            {
                FillRecursiveContent(c);
            }
        }
    }
    private void FillRecursiveContent(IContent Content)
    {
        if (Content != null && !Content.Trashed)
        {
            _allContent.Add(Content);

            if (_services.ContentService.HasChildren(Content.Id))
            {
                var countChildren = _services.ContentService.CountChildren(Content.Id);
                long xTotalRecs;
                var allChildren =
                    _services.ContentService.GetPagedChildren(Content.Id, 0, countChildren, out xTotalRecs);

                foreach (var child in allChildren)
                {
                    FillRecursiveContent(child);
                }
            }
        }
    }

    private void FillAllMedia()
    {
        var rootMedia = _services.MediaService.GetRootMedia();
        if (rootMedia != null && rootMedia.Any())
        {
            foreach (var m in rootMedia)
            {
                FillRecursiveMedia(m);
            }
        }
    }
    private void FillRecursiveMedia(IMedia MediaItem)
    {
        if (MediaItem != null && !MediaItem.Trashed)
        {
            _allMedia.Add(MediaItem);

            if (_services.MediaService.HasChildren(MediaItem.Id))
            {
                var countChildren = _services.MediaService.CountChildren(MediaItem.Id);
                long xTotalRecs;
                var allChildren =
                    _services.MediaService.GetPagedChildren(MediaItem.Id, 0, countChildren, out xTotalRecs);

                foreach (var child in allChildren)
                {
                    FillRecursiveMedia(child);
                }
            }
        }
    }

    #endregion


    #region Looping Example

    //public void LoopAllContent()
    //{
    //    var rootContent = _services.ContentService.GetRootContent();
    //    if (rootContent != null)
    //    {
    //        foreach (var c in rootContent)
    //        {
    //            RecursiveLoopNodes(c);
    //        }
    //    }
    //}

    //private void RecursiveLoopNodes(IContent Content)
    //{
    //    if (Content != null && !Content.Trashed)
    //    {
    //        //Do something

    //        if (_services.ContentService.HasChildren(Content.Id))
    //        {
    //            var countChildren = _services.ContentService.CountChildren(Content.Id);
    //            long xTotalRecs;
    //            var allChildren = _services.ContentService.GetPagedChildren(Content.Id, 0, countChildren, out xTotalRecs);

    //            foreach (var child in allChildren)
    //            {
    //                RecursiveLoopNodes(child);
    //            }
    //        }
    //    }
    //}

    #endregion



    #region Utility Functions

    public static string ConvertToCsvIds(IEnumerable<IContent> AffectedContentNodes)
    {
        var csv = "";

        var ids = AffectedContentNodes.Select(n => n.Id);
        csv = string.Join(",", ids);

        return csv;
    }

    public static string ConvertToCsvIds(IEnumerable<IMedia> AffectedMediaNodes)
    {
        var csv = "";

        var ids = AffectedMediaNodes.Select(n => n.Id);
        csv = string.Join(",", ids);

        return csv;
    }

    #endregion
}
