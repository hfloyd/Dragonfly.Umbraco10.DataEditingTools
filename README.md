# Dragonfly.Umbraco10.DataEditingTools #
Data cleaning/conversion tools for Umbraco 10+ created by [Heather Floyd](https://www.HeatherFloyd.com). (Based on original code in  "Dragonfly.Migration7To8Helpers")

# Installation #
Via NuGet:
[![Nuget Downloads](https://buildstats.info/nuget/Dragonfly.Umbraco10.DataEditingTools)](https://www.nuget.org/packages/Dragonfly.Umbraco10.DataEditingTools/)

     PM>   Install-Package Dragonfly.Umbraco10.DataEditingTools



# Description #

Includes various quick-to-use "Find-Replace" functionality to edit underlying Content node data directly. Also allows the ability to create custom programmed data-editing functions, with a UI to facilitate selecting, checking, and updating Content values. 

**DISCLAIMER: This tool gives you a lot of power to alter your property data directly. Be responsible. Make database backups in case something goes horribly wrong. Use the "Preview Only" option to check the results before actually updating the data, etc.**

**If you screw up your website, it is your own fault.**

# Usage #

After Installing, log-in to the Umbraco back-office, then visit:

http://YOURSITE.COM/Umbraco/backoffice/Api/TransformationHelperApi/Start

# Features #

## General Visibility ##

You can view lists of Doctypes, DataTypes, and PropertyEditors, and see what other items are referencing them. Drill-down and view raw property data.

Additionally, there is a UDI / GUID Lookup tool, which can be helpful when you are seeing error messages with limited information, or when you need to quickly figure out what a UDI/GUID is referring to.

## Data Conversion Tools ##

### Find/Replace in PropertyData ###
Allows you to bulk alter a node's property's raw data directly.

- **Simple Text-to-Text** - An exact replacement of the entered text inside the property data
- **Replace Integer Ids with UDIs** - To update Node IDs with UDIs inside of data (for example - in the case of Nested Content or other JSON)
- **Custom Transform** - use your own custom Transformation code (implementing interface ICustomFindReplaceTransformer)

### Property-To-Property ###
*Documentation TBD*

## Custom Data Transformers : ICustomFindReplaceTransformer ##
You can create your own custom data transformations to update data inside a property. This is useful when you are dealing with changing data formats inside of third-party property editors or other more complex data.

Just add a class to your project that implements interface ICustomFindReplaceTransformer. The two methods are simple to work with.

### IsValidForData ###

     bool IsValidForData(int NodeId, string ContentTypeAlias,  PropertyType PropType, object OriginalData);

Here you can add whatever tests are needed to ensure that you are only operating on the data you want. The calling function will provide you with the current node Id, ContentType Alias, PropertyType, and the current data in the property. You can use what you need to and ignore the rest. Use Services to get more information about the objects as needed.

**Examples:**

    //Check that it is a specific DataType
    string MyDataTypeGuid = "2274379b-de2c-4e8c-bd88-3c1fb2caa5e8";
    
    if (PropType.DataTypeKey.ToString() == MyDataTypeGuid)
    {
    	return true;
    }
    else
    {
    	return false;
    }

--

    //Check that it is a specific Property Editor
	if (PropertyType.PropertyEditorAlias == "Umbraco.NestedContent")
    {
    	return true;
    }
    else
    {
    	return false;
    }



Your transformer will only get called if the original data in the node/property is not NULL. If your function returns "FALSE", the data will NOT be changed for that node/property. 

### ConvertOriginalData ###
    
     object ConvertOriginalData(object OriginalData,out string ConversionErrorMsg);


This is where you do the conversion. You can do whatever you want here, just return the "new" data. Keep in mind that you are working with the data in the raw DB format, so objects will be in JSON format, for instance, and should be Serialized to a string before getting returned.

Use the `ConversionErrorMessage` to also return any messages about why data was unable to be converted. In the event of an error, you should probably return the original data unchanged. The ConversionErrorMessage will be shown in the table on the "Find Replace Results" Page

**Some Tips:**
- When dealing with NestedContent or other complex object data, you should create some simple class models for deserializing to make it easier to work with your new data. The quickest way to do that is to copy some example data from the property and use https://app.quicktype.io with the "Attributes Only" option selected.

**Examples:**
See the ['~Custom Transformer Samples' folder](https://github.com/hfloyd/Dragonfly.Umbraco10.DataEditingTools/tree/master/src/Dragonfly/~Custom%20Transformer%20Samples) for sample code



# Resources #

GitHub Repository: [https://github.com/hfloyd/Dragonfly.Umbraco10.DataEditingTools](https://github.com/hfloyd/Dragonfly.Umbraco10.DataEditingTools)
