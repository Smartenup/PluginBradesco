using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Payments.Bradesco
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Bradesco.Configure",
                "Plugins/PaymentBradesco/Configure",
                new { controller = "PaymentBradesco", action = "Configure" },
                new[] { "Nop.Plugin.Payments.Bradesco.Controllers" }
           );

            routes.MapRoute("Plugin.Payments.Bradesco.PaymentData",
                 "Plugins/PaymentBradesco/PaymentData",
                 new { controller = "PaymentBradesco", action = "PaymentData" },
                 new[] { "Nop.Plugin.Payments.Bradesco.Controllers" }
            );
        }

        public int Priority
        {
            get { return 0; }
        }
    }
}
