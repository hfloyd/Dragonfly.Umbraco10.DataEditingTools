namespace Dragonfly.DataEditingTools.Helpers
{
	using System;
	using System.CodeDom;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Security.Policy;
    using System.Web;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Hosting.Internal;
	using Umbraco.Cms.Core.Extensions;

	//Adapted from nuPickers Helper.cs

    internal static class AssemblyHelpers
    {
        internal static IEnumerable<string> GetAssemblyNames(IWebHostEnvironment HostingEnvironment)
        {
            List<string> assemblyNames = new List<string>();

            // try to add App_Code directory
            string appCodePath = HostingEnvironment.MapPathContentRoot("~/App_Code");
            if (appCodePath != null)
            {
                DirectoryInfo appCode = new DirectoryInfo(appCodePath);

                if (appCode.Exists && GetAssembly(appCode.Name, HostingEnvironment) != null)
                {
                    assemblyNames.Add(appCode.Name);
                }
            }

            // add any .dll assemblies from the /bin directory
            string binPath = HostingEnvironment.MapPathContentRoot("~/bin");
            if (binPath != null)
            {
                assemblyNames.AddRange(Directory.GetFiles(binPath, "*.dll").Select(x => x.Substring(x.LastIndexOf('\\') + 1)));
            }

            return assemblyNames;
        }

        /// <summary>
        /// attempts to get an assembly by it's name
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns>an Assembly or null</returns>
        internal static Assembly GetAssembly(string assemblyName,IWebHostEnvironment HostingEnvironment)
        {
            if (string.Equals(assemblyName, "App_Code", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    return Assembly.Load(assemblyName);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }

            string assemblyFilePath = HostingEnvironment.MapPathContentRoot(string.Concat("~/bin/", assemblyName));
            if (!string.IsNullOrEmpty(assemblyFilePath))
            {
                try
                {
                    // HACK: http://stackoverflow.com/questions/1031431/system-reflection-assembly-loadfile-locks-file
                    return Assembly.Load(File.ReadAllBytes(assemblyFilePath));
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// extension method on Assembly to handle reflection loading exceptions
        /// </summary>
        /// <param name="assembly">the assembly to get types from</param>
        /// <returns>a collection of types found</returns>
        internal static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(x => x != null);
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        ///// <summary>
        ///// uses supplied url to check the file system (if prefixed with ~/) else makes an http query
        ///// </summary>
        ///// <param name="url">Url to download the resource from</param>
        ///// <returns>An empty string, or the string result of either a file or an http response</returns>
        //internal static string GetDataFromUrl(string url)
        //{
        //    string data = string.Empty;

        //    if (!string.IsNullOrEmpty(url))
        //    {
        //        if (VirtualPathUtility.IsAppRelative(url)) // starts with ~/
        //        {
        //            bool fileExists = false;

        //            if (!url.Contains("?"))
        //            {
        //                string filePath = HttpContext.Current.Server.MapPath(url);

        //                if (File.Exists(filePath))
        //                {
        //                    url = filePath;
        //                    fileExists = true;
        //                }
        //            }

        //            if (!fileExists)
        //            {
        //                url = url.Replace("~/", HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/");
        //            }
        //        }

        //        using (WebClient client = new WebClient())
        //        {
        //            data = client.DownloadString(url);
        //        }
        //    }

        //    return data;
        //}

        internal static object GetAssemblyTypeInstance(string FullClassName,IWebHostEnvironment HostingEnvironment)
        {
            var infos = GetAllAssembliesAndClasses(HostingEnvironment).ToList();
            var matches = infos.Where(n => n.ClassName == FullClassName).ToList();
            if (matches.Any())
            {
                var item = matches.First();

                //AppDomainSetup info = new AppDomainSetup();
                //string binPath = HostingEnvironment.MapPath("~/bin");
                //info.ApplicationBase = binPath;
                //Evidence baseEvidence = AppDomain.CurrentDomain.Evidence;
                //Evidence evidence = new Evidence(baseEvidence);
                //var _Dom = AppDomain.CreateDomain("SubDomain", evidence, info);
                //var instance = _Dom.CreateInstanceAndUnwrap(item.AssemblyName, item.ClassName);

                var instance = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(item.Type.Assembly.FullName, item.Type.FullName);
                //var instance= AppDomain.CurrentDomain.CreateInstanceAndUnwrap(item.AssemblyName, item.ClassName);
                //  var instance =AppDomain.CurrentDomain.CreateInstanceAndUnwrap(GetAssembly(item.AssemblyName).FullName,item.ClassName);
                return instance;
                //return Convert.ChangeType(instance, item.Type);
            }
            else
            {
                return null;
            }
        }

        internal static IEnumerable<AssemblyClassInfo> GetAllAssembliesAndClasses(IWebHostEnvironment HostingEnvironment)
        {
            var list = new List<AssemblyClassInfo>();

            var allAssemblies = GetAssemblyNames(HostingEnvironment);

            foreach (var assemblyName in allAssemblies)
            {
                Assembly assembly = GetAssembly(assemblyName, HostingEnvironment);

                if (assembly != null)
                {
                    var classes = assembly.GetLoadableTypes();

                    foreach (var c in classes)
                    {
                        var info = new AssemblyClassInfo();
                        info.AssemblyName = assemblyName;
                        info.ClassName = c.FullName;
                        info.Type = c;

                        list.Add(info);
                    }
                }
            }

            return list;
        }

        public static bool TestSerializable(Type TypeToTest)
        {
            return TypeToTest.IsSerializable;
        }
    }

    internal class AssemblyClassInfo
    {
        public Type Type { get; set; }
        public string AssemblyName { get; set; }

        public string ClassName { get; set; }
    }
}