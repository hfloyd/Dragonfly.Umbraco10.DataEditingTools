namespace Demo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Dragonfly.Migration7To8Helpers;
    using Dragonfly.Migration7To8Helpers.Models;
    using Newtonsoft.Json;
    using Umbraco.Core;
    using Umbraco.Core.Migrations.Upgrade.V_8_0_0;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;
    using Umbraco.Web.Composing;
    using Umbraco.Web.Models;

    [Serializable]
    class FixCarouselNestedContent : ICustomFindReplaceDataMigrator
    {
        public bool IsValidForData(int NodeId, string ContentTypeAlias, PropertyType PropType, object OriginalData)
        {
            //Check that it is the Carousel type
            string CarouselNcDataTypeGuid = "2274379b-de2c-4e8c-bd88-3c1fb2caa5e8";
            
            if (PropType.DataTypeKey.ToString() == CarouselNcDataTypeGuid)

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
            try
            {
                ConversionErrorMsg = "";
                var origCarousel = JsonConvert.DeserializeObject<IEnumerable<ConverterCarouselSlide>>(OriginalData.ToString());

                if (origCarousel != null)
                {
                    var newCarousel = new List<ConverterCarouselSlide>();
                    foreach (var os in origCarousel)
                    {
                        //Update Link
                        var origLink = os.SlideLink; //Get the legacy data - probably is a label after v8 DB migration
                        var newLink = ConvertLegacyLinkData(origLink); //This could be in a private function, external helper, etc.
                        var newLinkMulti = newLink.AsEnumerableOfOne(); //Remember - even 'single' Links are stored as an array.
                        os.SlideLinkNew = JsonConvert.SerializeObject(newLinkMulti); //Serialize the new links data and assign it to the temporary model object

                        newCarousel.Add(os); //add the now-updated model to the new data list
                    }

                    return JsonConvert.SerializeObject(newCarousel); //don't forget to serialize the return object!
                }
                else
                {
                    ConversionErrorMsg = "Conversion Error: Unable to Parse Original Data to IEnumerable<ConverterCarouselSlide>";
                    return OriginalData;
                }
            }
            catch (Exception e)
            {
                ConversionErrorMsg = $"Conversion Error: {e.Message}";
                return OriginalData;
            }
        }

        private Link ConvertLegacyLinkData(object OrigLink)
        {
            //Here would be some code to use the legacy link data to generate a valid Umbraco.Web.Models.Link object
            return new Link();
        }
    }

    internal class ConverterCarouselSlide
    {
        /* Based on Example data:
           {
            "key": "6be42d5e-65ca-4100-bdf9-45c5c1c515de",
            "name": "XXXXXX",
            "ncContentTypeAlias": "carouselSlide",
            "headline": "XXXXXX",
            "slideLink": null,
            "slideLinkNew": null,
            "image": "umb://media/4047ceb7c8274ba6b69b6b60fed9bfa9"
            }
         */

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Key { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("ncContentTypeAlias", NullValueHandling = NullValueHandling.Ignore)]
        public string NcContentTypeAlias { get; set; }

        [JsonProperty("headline", NullValueHandling = NullValueHandling.Ignore)]
        public string Headline { get; set; }

        [JsonProperty("slideLink")]
        public object SlideLink { get; set; }

        [JsonProperty("slideLinkNew")]
        public object SlideLinkNew { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }
    }

}
