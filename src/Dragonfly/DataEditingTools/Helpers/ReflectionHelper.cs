﻿
namespace Dragonfly.DataEditingTools;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Extensions;

/// <summary>
/// Helper for dealing with that nasty reflection stuff
/// This is a bit crazy!
/// Copied from Diplo.GodMode.Helpers: https://github.com/DanDiplo/Umbraco.GodMode/blob/v10/Diplo.GodMode/Helpers/ReflectionHelper.cs
/// </summary>
internal static class ReflectionHelper
{
	public static readonly Func<Assembly, bool> IsUmbracoAssemblyPredicate = a => a.ManifestModule.Name.StartsWith("umbraco.", StringComparison.OrdinalIgnoreCase);

	public static readonly Func<Type, Type, bool> IsAssignableClassFromPredicate = (a, b) => a != null && b != null && b.IsClass && !b.IsAbstract && a.IsAssignableFrom(b);

	public static readonly Func<Type, Type, bool> IsAssignableFromPredicate = (a, b) => a.Inherits(b);

	private static readonly string[] PropertiesToIgnore = new[] { "PreviewBadge" };

	internal static object? GetAssemblyTypeInstance(IServiceProvider provider,string FullClassName)
	{
		var allAssemblies = ReflectionHelper.GetAssemblies().ToList();
		if (allAssemblies.Any())
		{
			foreach (var assembly in allAssemblies)
			{
				var allLoadableTypes = assembly.GetLoadableTypes();
				var matches = allLoadableTypes.Where(n => n.FullName == FullClassName).ToList();
				if (matches.Any())
				{
					var matchingtype = matches.First();
					var instance = ActivatorUtilities.CreateInstance(provider, matchingtype);
					//var instance = assembly.CreateInstance(matchingtype.FullName);
					return instance;
				}
			}
		}

		//If we get here...
		return null;
	}


	public static IEnumerable<Type> GetTypesAssignableFrom(Type baseType, Func<Assembly, bool> predicate = null)
	{
		return GetLoadableTypes(predicate).Where(t => IsAssignableClassFromPredicate(baseType, t));
	}

	public static IEnumerable<Type> GetUmbracoTypesAssignableFrom(Type baseType)
	{
		return GetLoadableTypes(IsUmbracoAssemblyPredicate).Where(t => IsAssignableClassFromPredicate(baseType, t));
	}

	public static IEnumerable<Assembly> GetAssemblies(Func<Assembly, bool> predicate = null)
	{
		return predicate != null ? AppDomain.CurrentDomain.GetAssemblies().Where(ass => !ass.IsDynamic).Where(predicate) : AppDomain.CurrentDomain.GetAssemblies().Where(ass => !ass.IsDynamic);
	}

	public static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, IEnumerable<Type> types)
	{
		return from x in types
			   from z in x.GetInterfaces()
			   let y = x.BaseType
			   where
			   (y != null && y.IsGenericType &&
			   openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
			   (z.IsGenericType &&
			   openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
			   select x;
	}

	public static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType) => GetAllTypesImplementingOpenGenericType(openGenericType, GetLoadableTypes());

	public static IEnumerable<Type> GetLoadableTypes(Func<Assembly, bool> assemblyPredicate = null)
	{
		return GetAssemblies(assemblyPredicate).SelectMany(s => GetTypesThatCanBeLoaded(s));
	}

	public static IEnumerable<Assembly> GetUmbracoAssemblies()
	{
		return GetAssemblies(IsUmbracoAssemblyPredicate);
	}

	public static IEnumerable<Type> GetLoadableUmbracoTypes()
	{
		return GetLoadableTypes(IsUmbracoAssemblyPredicate);
	}

	public static IEnumerable<TypeMap> GetTypeMapFrom(Type myType)
	{
		return GetTypesAssignableFrom(myType).Select(t => new TypeMap(t));
	}

	public static IEnumerable<TypeMap> GetNonGenericInterfaces(Assembly assembly)
	{
		return assembly.GetLoadableTypes().Where(t => t != null && !t.IsGenericType && t.IsPublic && !t.IsGenericTypeDefinition && t.IsInterface).Select(t => new TypeMap(t));
	}

	public static IEnumerable<TypeMap> GetNonGenericTypes(Assembly assembly)
	{
		return assembly.GetLoadableTypes().Where(t => t != null && !t.IsGenericType && t.IsPublic).Select(t => new TypeMap(t));
	}

	public static List<FieldInfo> GetAllPublicConstants(this Type type)
	{
		return type
			.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
	}

	public static IEnumerable<Type> GetTypesInNameSpace(Assembly assembly, string nameSpace)
	{
		return assembly.GetTypes().Where(type => type.Namespace == nameSpace);
	}

	public static IEnumerable<Type> GetInterfaceTypesInNameSpace(Assembly assembly, string nameSpace)
	{
		return assembly.GetTypes().Where(type => type.IsInterface && type.Namespace == nameSpace);
	}

	public static IEnumerable<Type> GetTypesInNameSpace(string nameSpace)
	{
		return GetTypesInNameSpace(Assembly.GetExecutingAssembly(), nameSpace);
	}

	public static IEnumerable<Diagnostic> PopulateDiagnosticsFromConstants(Type type)
	{
		if (type == null)
		{
			return Enumerable.Empty<Diagnostic>();
		}

		return type.GetAllPublicConstants().Select(x => new Diagnostic(x.Name, x.GetRawConstantValue().ToString()));
	}

	public static IEnumerable<Diagnostic> PopulateDiagnosticsFrom(object obj, bool onlyUmbraco = true, bool ignoreNestedType = true)
	{
		if (obj == null)
		{
			return Enumerable.Empty<Diagnostic>();
		}

		var props = obj.GetType().GetAllProperties();

		if (props == null)
		{
			return Enumerable.Empty<Diagnostic>();
		}

		if (onlyUmbraco)
		{
			props = props.Where(x => x.Module.Name.StartsWith("umbraco", StringComparison.OrdinalIgnoreCase)).ToArray();
		}

		var diagnostics = new List<Diagnostic>(props.Length);

		foreach (var prop in props.Where(p => !PropertiesToIgnore.InvariantContains(p.Name)))
		{
			try
			{
				var value = prop.GetValue(obj);

				if (value != null)
				{
					if (value.GetType().IsPublic)
					{
						if (prop.PropertyType.IsArray || (prop.PropertyType != typeof(string) && prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable))))
						{
							var items = ((IEnumerable)value).Cast<string>().ToList();

							if (items != null)
							{
								string sValue = string.Join(", ", items);

								diagnostics.Add(new Diagnostic(GetPropertyDisplayName(prop), sValue));
							}
						}
						else
						{
							string sValue = value.ToString();

							if (ignoreNestedType && sValue.StartsWith("Umbraco."))
							{
							}
							else
							{
								diagnostics.Add(new Diagnostic(GetPropertyDisplayName(prop), sValue));
							}
						}
					}
				}
			}
			catch { };
		}

		return diagnostics;
	}

	private static IEnumerable<Type> GetTypesThatCanBeLoaded(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			return e.Types.Where(t => t != null);
		}
	}

	public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
	{
		return GetTypesThatCanBeLoaded(assembly);
	}

	private static string GetPropertyDisplayName(PropertyInfo prop)
	{
		return prop.Name.Split('.').Last() + (prop.PropertyType == typeof(bool) ? "?" : string.Empty);
	}

	private static string SplitOnCapitals(string text)
	{
		StringBuilder builder = new StringBuilder();

		foreach (char c in text)
		{
			if (Char.IsUpper(c) && builder.Length > 0)
			{
				builder.Append(' ');
			}
			builder.Append(c);
		}

		return builder.ToString();
	}
}

/// <summary>
/// Represents a simple mapping from a .NET type
/// Copied from https://github.com/DanDiplo/Umbraco.GodMode/blob/v10/Diplo.GodMode/Models/TypeMap.cs
/// </summary>
internal class TypeMap
{
	internal TypeMap(Type t)
	{
		this.Module = t.Module.Name;
		this.Assembly = t.Assembly.FullName;
		this.Name = t.Name;
		this.Namespace = t.Namespace;
		this.BaseType = t.BaseType != null ? t.BaseType.Name : String.Empty;
		this.LoadableName = t.GetFullNameWithAssembly();
		this.IsUmbraco = this.Module.StartsWith("Umbraco.");
	}

	internal TypeMap(Assembly a)
	{
		this.Module = a.GetName().Name;
		this.Assembly = a.FullName;
	}

	public string Module { get; set; }

	public string Assembly { get; set; }

	public string Name { get; set; }

	public string Namespace { get; set; }

	public string BaseType { get; set; }

	public string LoadableName { get; set; }

	public bool IsUmbraco { get; set; }
}

/// <summary>
/// Represents an individual diagnostic item
/// Copied from https://github.com/DanDiplo/Umbraco.GodMode/blob/v10/Diplo.GodMode/Models/Diagnostic.cs
/// </summary>
internal class Diagnostic
{
	internal Diagnostic(string key, string value)
	{
		this.Key = key;
		this.Value = value;
	}

	internal Diagnostic(string key, object value)
	{
		this.Key = key;
		this.Value = value.ToString();
	}

	internal Diagnostic(string key, IEnumerable<string> items)
	{
		this.Key = key;
		this.Value = String.Join(", ", items ?? Enumerable.Empty<string>());
	}

	internal Diagnostic(string key, IEnumerable<object> items)
	{
		this.Key = key;
		this.Value = String.Join(", ", items ?? Enumerable.Empty<string>());
	}

	public string Key { get; private set; }

	public string Value { get; private set; }

	public override string ToString()
	{
		return String.Format("{0}: {1}\n", this.Key, this.Value);
	}
}