using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Payments.MOLPay.Models
{
    public class PaymentInfoModel : BaseGrandModel
    {
        public PaymentInfoModel()
        {
            ChannelTypes = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Payment.SelectChannelType")]
        public string ChannelType { get; set; }

        [GrandResourceDisplayName("Payment.SelectChannelType")]
        public IList<SelectListItem> ChannelTypes { get; set; }

        public bool NewChannelselected { get; set; }
    }
}