namespace Dragonfly.DataEditingTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.Extensions.Hosting.Internal;

    ///// <summary>
    ///// Configuration settings read from a config file
    ///// </summary>
    public class ConfigData
    {
        //As per https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#bind-hierarchical-configuration-data-using-the-options-pattern-1

        //AppSettings Config Section name
        public const string ConfigSection = "DragonflyDataEditingTools";


        public string AppPluginsPath = "~/App_Plugins/Dragonfly.DataEditingTools/";
        public string DefaultDataPath = "~/App_Data/Dragonfly.DataEditingTools/";

        //[JsonPropertyName("MyProperty")]
        //public bool MyProperty { get; set; } = false;





    }
}
