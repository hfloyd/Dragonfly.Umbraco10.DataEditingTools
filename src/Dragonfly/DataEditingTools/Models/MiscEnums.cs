namespace Dragonfly.DataEditingTools.Models;

using System;
using System.Collections.Generic;


public partial class Enums
{
	#region NodeType

	public enum NodeType
	{
		Content,
		Media,
		Unknown
	}

	public static NodeType GetNodeType(string TypeString)
	{
		switch (TypeString)
		{
			case "Content":
				return NodeType.Content;

			case "Media":
				return NodeType.Media;

			default:
				return NodeType.Unknown;
		}
	}

	#endregion

	#region Custom Transformers
	public enum CustomTransformerTypes
	{
		ICustomFindReplaceDataTransformer,
		ICustomPropToPropDataTransformer
	}

	public static Dictionary<CustomTransformerTypes, Type> CustomTransformerTypesWithTypes()
	{
		var dict = new Dictionary<CustomTransformerTypes, Type>();

		dict.Add(CustomTransformerTypes.ICustomFindReplaceDataTransformer, typeof(ICustomFindReplaceTransformer));
		dict.Add(CustomTransformerTypes.ICustomPropToPropDataTransformer, typeof(ICustomPropToPropDataTransformer));

		return dict;
	}

	public static Type GetCustomTransformerType(CustomTransformerTypes Type)
	{
		var dict = CustomTransformerTypesWithTypes();
		return dict[Type];
	}

	//public static IEnumerable<SelectListItem> FindReplaceTypesSelectList()
	//{
	//    var dict = FindReplaceTypesWithDisplayText();
	//    var options = dict.Select(d => new SelectListItem
	//    {
	//        Value = d.Key.ToString(),
	//        Text = d.Value.ToString()
	//    });

	//    //if default option needed... (string DefaultValue, string DefaultText)
	//    //if (!string.IsNullOrEmpty(DefaultValue))
	//    //{
	//    //    var defaultSelect = Enumerable.Repeat(new SelectListItem
	//    //    {
	//    //        Value = DefaultId,
	//    //        Text = DefaultText
	//    //    }, count: 1);

	//    //    return defaultSelect.Concat(options);
	//    //}

	//    return options;
	//}

	#endregion

}


