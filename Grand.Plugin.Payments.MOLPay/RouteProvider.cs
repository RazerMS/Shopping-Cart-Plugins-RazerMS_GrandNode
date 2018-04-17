using Grand.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Grand.Plugin.Payments.MOLPay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Return
            routeBuilder.MapRoute("Plugin.Payments.MOLPay.PDTHandler",
                 "Plugins/MOLPay/PDTHandler",
                 new { controller = "MOLPay", action = "PDTHandler" }
            );
            //IPN
            routeBuilder.MapRoute("Plugin.Payments.MOLPay.IPNHandler",
                 "Plugins/MOLPay/IPNHandler",
                 new { controller = "MOLPay", action = "IPNHandler" }
            );
            //Cancel
            routeBuilder.MapRoute("Plugin.Payments.MOLPay.CancelOrder",
                 "Plugins/MOLPay/CancelOrder",
                 new { controller = "MOLPay", action = "CancelOrder" }
            );
            //Notification
            routeBuilder.MapRoute("Plugin.Payments.MOLPay.NotificationHandler",
                 "Plugins/MOLPay/NotificationHandler",
                 new { controller = "MOLPay", action = "NotificationHandler" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
