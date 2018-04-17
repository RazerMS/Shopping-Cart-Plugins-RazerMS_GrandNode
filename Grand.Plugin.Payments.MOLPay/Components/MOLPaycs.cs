using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Grand.Plugin.Payments.MOLPay.Models;

namespace Grand.Plugin.Payments.MOLPay.Components
{
    [ViewComponent(Name = "MOLPay")]
    public class MOLPayComponent : ViewComponent
    {
        public MOLPayComponent() { }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel()
            {
                ChannelTypes = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Maybank2u", Value = "maybank2u" },
                    new SelectListItem { Text = "CIMB Clicks", Value = "cimbclicks" },
                }
            };

            return View("~/Plugins/Payments.MOLPay/Views/MOLPay/PaymentInfo.cshtml");
        }
    }
}