namespace Dragonfly.DataEditingTools
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Reflection;

	using Dragonfly.DataEditingTools.Models;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Logging;
	using Umbraco.Cms.Core;
	using Umbraco.Cms.Core.PropertyEditors;
	using Umbraco.Cms.Core.Services;


	public class CustomTransformerService
	{

		#region CTOR & DI

		private readonly ILogger<CustomTransformerService> _logger;
		//private readonly IConfiguration _AppSettingsConfig;
		private readonly IWebHostEnvironment _hostingEnvironment;

		//private readonly AppCaches _appCaches;
		//private readonly UmbracoContext _umbracoContext;
		//private readonly UmbracoHelper _umbracoHelper;
		private readonly ServiceContext _services;
		//   private readonly IPublishedContentQuery _publishedContentQuery;

		public CustomTransformerService(
			ILogger<CustomTransformerService> logger,
			DependencyLoader Dependencies
		)
		{
			_logger = logger;
			_services = Dependencies.Services;
			_hostingEnvironment = Dependencies.HostingEnvironment;

		}

		#endregion

		#region Public

		public IEnumerable<string> GetNestedContentPropertyDocTypes(int DataTypeId)
		{
			var list = new List<string>();

			var dType = _services.DataTypeService.GetDataType(DataTypeId);
			if (dType.EditorAlias != "Umbraco.NestedContent")
			{
				throw new ArgumentException(
					$"Provided DataType must be Umbraco.NestedContent. (Argument DataType: {dType.Id} = {dType.Name} ({dType.EditorAlias}))");
			}

			var config = dType.Configuration as NestedContentConfiguration;
			//var config = JsonConvert.DeserializeObject<NestedContentConfiguration>(configJson.ToString());

			foreach (var contentType in config.ContentTypes)
			{
				var docTypeAlias = contentType.Alias;
				var groupName = contentType.TabAlias;

				list.Add(docTypeAlias);
			}

			return list;
		}

		#endregion


		#region Internal 
		/// <summary>
		/// Gets All Assemblies of a Specified Custom Transformer Type
		/// </summary>
		/// <param name="TransformerType"></param>
		/// <returns></returns>
		internal IEnumerable<string> GetAssemblyNames(Enums.CustomTransformerTypes Type)
		{
			var systemType = Enums.GetCustomTransformerType(Type);

			List<string> assemblyNames = new List<string>();
			var allAssemblies = ReflectionHelper.GetAssemblies().ToList();

			foreach (var assembly in allAssemblies)
			{
				if (assembly.GetLoadableTypes().Any(x => systemType.IsAssignableFrom(x)))
				{
					assemblyNames.Add(assembly.FullName);
				}
			}

			return assemblyNames;
		}

		///// <summary>
		///// Gets Classed from a specified Assembly of a Specified Custom Transformer Type
		///// </summary>
		///// <param name="TransformerType"></param>
		///// <returns></returns>
		//internal IEnumerable<string> GetClassNames(string AssemblyName, Enums.CustomTransformerTypes Type)
		//{
		//	var systemType = Enums.GetCustomTransformerType(Type);
		//	Assembly assembly = AssemblyHelpers.GetAssembly(AssemblyName, _hostingEnvironment);

		//	if (assembly != null)
		//	{
		//		return assembly
		//			.GetLoadableTypes()
		//			.Where(x => systemType.IsAssignableFrom(x))
		//			.Select(x => x.FullName);
		//	}

		//	return null;
		//}

		/// <summary>
		/// Gets All Classes of a Specified Custom Transformer Type
		/// </summary>
		/// <param name="TransformerType"></param>
		/// <returns></returns>
		internal IEnumerable<string> GetAllTransformerClassNames(Enums.CustomTransformerTypes Type)
		{
			var finalList = new List<string>();
			var systemType = Enums.GetCustomTransformerType(Type);
			//var allAssemblies = GetAssemblyNames(Type);
			var allAssemblies = ReflectionHelper.GetAssemblies();


			foreach (var assembly in allAssemblies)
			{
				//Assembly assembly = AssemblyHelpers.GetAssembly(assemblyName,_hostingEnvironment);

				var names = assembly
						 .GetLoadableTypes()
						 .Where(x => systemType.IsAssignableFrom(x) && !x.FullName.Contains(".ICustomPropToPropDataTransformer"))
						 .Select(x => x.FullName);

				finalList.AddRange(names);

			}

			return finalList;
		}

		#endregion

	}
}
