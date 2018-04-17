using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Grand.Plugin.Payments.MOLPay.Models
{
    public class ConfigurationModel : BaseGrandModel
    {
        //public ConfigurationModel()
        //{
        //    CapturedModeList = new List<SelectListItem>();
        //    PendingModeList = new List<SelectListItem>();
        //    FailedModeList = new List<SelectListItem>();
        //}

        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.Vkey")]
        public string Vkey { get; set; }
        public bool Vkey_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.Skey")]
        public string Skey { get; set; }
        public bool Skey_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.CapturedMode")]
        public int Captured { get; set; }
        public bool Captured_OverrideForStore { get; set; }
        public SelectList CapturedModes { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.PendingMode")]
        public int Pending { get; set; }
        public bool Pending_OverrideForStore { get; set; }
        public SelectList PendingModes { get; set; }

        [GrandResourceDisplayName("Plugins.Payments.MOLPay.Fields.FailedMode")]
        public int Failed { get; set; }
        public bool Failed_OverrideForStore { get; set; }
        public SelectList FailedModes { get; set; }

    }
}