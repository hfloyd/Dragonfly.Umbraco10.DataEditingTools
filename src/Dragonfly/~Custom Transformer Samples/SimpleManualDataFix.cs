namespace Demo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dragonfly.Migration7To8Helpers.Models;
    using Umbraco.Core.Models;

    /*This is an example of a very basic value-setting function. Generally you wouldn't be using this as-is...
     It might be useful in a case where you need to revert a change you made which broke a property
     */


    [Serializable]
    class SimpleManualDataFix : ICustomFindReplaceDataMigrator
    {
        #region Implementation of ICustomFindReplaceDataMigrator

        public bool IsValidForData(int NodeId, string ContentTypeAlias, PropertyType PropType, object OriginalData)
        {
            //Some sort of code to determine that you are updating the specific data desired.
            if (NodeId == 99999999)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertOriginalData(object OriginalData, out string ConversionErrorMsg)
        {
            ConversionErrorMsg = null;

            return @"[ { 
            ""key"": ""8f2237fd-68ce-4ae4-bbb5-1ca87f7f769e"", 
            ""name"": ""XXXXX"", 
            ""ncContentTypeAlias"": ""MyAlias"", 
            ""Headline"": ""XXXXX"",
            ""Image"": ""umb://media/8b094dd166784faf8642884183f0a3ac""
            } ]";
        }

        #endregion
    }
}
