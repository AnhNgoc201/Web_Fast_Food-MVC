using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;

namespace Web_Do_An_Nhanh
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Remove(config.Formatters.XmlFormatter);

            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ReferenceLoopHandling
                = ReferenceLoopHandling.Ignore;
        }
    }
}
