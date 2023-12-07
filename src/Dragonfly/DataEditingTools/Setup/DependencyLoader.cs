namespace Dragonfly.DataEditingTools
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Umbraco.Cms.Core.IO;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Cms.Web.Common;


    public class DependencyLoader
    {
        public IConfiguration AppSettingsConfig { get; }
        public IWebHostEnvironment HostingEnvironment { get; }
        public Umbraco.Cms.Core.Hosting.IHostingEnvironment UmbracoHostingEnvironment { get; }
        public IHttpContextAccessor ContextAccessor { get; }
        public IUmbracoContextAccessor UmbracoContextAccessor { get; }

        public UmbracoHelper UmbHelper;
        public HttpContext Context;
        public ServiceContext Services;
        public NetHelperServices.FileHelperService DragonflyFileHelperService { get; }

        public MediaFileManager MediaFileManager;
        public DependencyLoader(
            IConfiguration appSettingsConfig,
            IWebHostEnvironment hostingEnvironment,
            Umbraco.Cms.Core.Hosting.IHostingEnvironment umbHostingEnvironment,
            IHttpContextAccessor contextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor,
            NetHelperServices.FileHelperService fileHelperService,
            ServiceContext serviceContext,
            MediaFileManager mediaFileManager
           )
        {
            AppSettingsConfig = appSettingsConfig;
            HostingEnvironment = hostingEnvironment;
            UmbracoHostingEnvironment = umbHostingEnvironment;
            ContextAccessor = contextAccessor;
            UmbracoContextAccessor = umbracoContextAccessor;
            UmbHelper = contextAccessor.HttpContext.RequestServices.GetRequiredService<UmbracoHelper>();
            DragonflyFileHelperService = fileHelperService;
            Context = contextAccessor.HttpContext;
            Services = serviceContext;
            MediaFileManager = mediaFileManager;
        }
    }
}
