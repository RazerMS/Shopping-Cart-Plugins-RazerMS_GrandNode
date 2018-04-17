using Grand.Core;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Payments;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Mvc.Filters;
using Grand.Plugin.Payments.MOLPay.Models;
using Grand.Services.Common;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Grand.Plugin.Payments.MOLPay.Controllers
{
    
    public class MOLPayController : BasePaymentController
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly MOLPayPaymentSettings _molPayPaymentSettings;
        private readonly IGenericAttributeService _genericAttributeService;

        public MOLPayController(IWorkContext workContext,
            IStoreService storeService,
            ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            MOLPayPaymentSettings molPayPaymentSettings,
            IGenericAttributeService genericAttributeService)
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._molPayPaymentSettings = molPayPaymentSettings;
            this._genericAttributeService = genericAttributeService;
        }

        [AuthorizeAdmin]
        [Area("Admin")]
        public IActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var molPayPaymentSettings = _settingService.LoadSetting<MOLPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = molPayPaymentSettings.UseSandbox,
                MerchantId = molPayPaymentSettings.MerchantId,
                Vkey = molPayPaymentSettings.Vkey,
                Skey = molPayPaymentSettings.Skey,

                Captured = (int)molPayPaymentSettings.CapturedMode,
                Pending = (int)molPayPaymentSettings.PendingMode,
                Failed = (int)molPayPaymentSettings.FailedMode,

                CapturedModes = _molPayPaymentSettings.CapturedMode.ToSelectList(),
                PendingModes = _molPayPaymentSettings.PendingMode.ToSelectList(),
                FailedModes = _molPayPaymentSettings.FailedMode.ToSelectList()
            };
            //model.ActiveStoreScopeConfiguration = storeScope;
            //if (!String.IsNullOrEmpty(storeScope))
            //{
            //    model.UseSandbox_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.UseSandbox, storeScope);
            //    model.MerchantId_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.MerchantId, storeScope);
            //    model.Vkey_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.Vkey, storeScope);
            //    model.Skey_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.Skey, storeScope);
            //    model.Captured_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.Captured, storeScope);
            //    model.Pending_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.Pending, storeScope);
            //    model.Failed_OverrideForStore = _settingService.SettingExists(molPayPaymentSettings, x => x.Failed, storeScope);
            //}

            return View("~/Plugins/Payments.MOLPay/Views/MOLPay/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area("Admin")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var molPayPaymentSettings = _settingService.LoadSetting<MOLPayPaymentSettings>(storeScope);

            //save settings
            molPayPaymentSettings.UseSandbox = model.UseSandbox;
            molPayPaymentSettings.MerchantId = model.MerchantId;
            molPayPaymentSettings.Vkey = model.Vkey;
            molPayPaymentSettings.Skey = model.Skey;

            molPayPaymentSettings.CapturedMode = (PaymentStatus)model.Captured;
            molPayPaymentSettings.PendingMode = (PaymentStatus)model.Pending;
            molPayPaymentSettings.FailedMode = (PaymentStatus)model.Failed;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            if (model.UseSandbox_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.UseSandbox, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.UseSandbox, storeScope);

            if (model.MerchantId_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.MerchantId, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.MerchantId, storeScope);

            if (model.Vkey_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.Vkey, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.Vkey, storeScope);

            if (model.Skey_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.Skey, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.Skey, storeScope);

            if (model.Captured_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.CapturedMode, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.CapturedMode, storeScope);

            if (model.Pending_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.PendingMode, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.PendingMode, storeScope);

            if (model.Failed_OverrideForStore || String.IsNullOrEmpty(storeScope))
                _settingService.SaveSetting(molPayPaymentSettings, x => x.FailedMode, storeScope, false);
            else if (!String.IsNullOrEmpty(storeScope))
                _settingService.DeleteSetting(molPayPaymentSettings, x => x.FailedMode, storeScope);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult PDTHandler(IFormCollection form)
        {
            var vkey = _molPayPaymentSettings.Vkey;
            var tranID = Request.Form["tranID"];
            string orderid = Request.Form["orderid"];
            var status = Request.Form["status"];
            var domain = Request.Form["domain"];
            var amount = Request.Form["amount"];
            var currency = Request.Form["currency"];
            var appcode = Request.Form["appcode"];
            var paydate = Request.Form["paydate"];
            var skey = Request.Form["skey"];
            var channel = Request.Form["channel"];

            var mc_gross = decimal.Zero;
            try
            {
                mc_gross = decimal.Parse(form["mc_gross"], new CultureInfo("en-US"));
            }
            catch (Exception exc)
            {
                _logger.Error("MOLPay PDT. Error getting mc_gross", exc);
            }

            if (tranID != "")
            {
                var order = _orderService.GetOrderByNumber(Int32.Parse(orderid));

                var sb = new StringBuilder();
                sb.AppendLine("MOLPay PDT:");
                sb.AppendLine("tranID: " + tranID);
                sb.AppendLine("orderid: " + orderid);
                sb.AppendLine("Payment status: " + status);
                sb.AppendLine("domain: " + domain);
                sb.AppendLine("amount: " + amount);
                sb.AppendLine("currency: " + currency);
                sb.AppendLine("appcode: " + appcode);
                sb.AppendLine("paydate: " + paydate);
                sb.AppendLine("skey: " + skey);

                var captured = _molPayPaymentSettings.CapturedMode;
                var failed = _molPayPaymentSettings.FailedMode;
                var pending = _molPayPaymentSettings.PendingMode;

                var result = pending;
                
                switch (status)
                {
                    case "00":
                        result = captured;
                        break;
                    case "11":
                        result = failed;
                        break;
                    case "22":
                        result = pending;
                        break;
                    default:
                        break;
                }

                var newPaymentStatus = result;
                sb.AppendLine("New payment status: " + newPaymentStatus);

                //order note
                _orderService.InsertOrderNote(new OrderNote
                {
                    Note = sb.ToString(),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    //OrderId = order.Id,
                });
                _orderService.UpdateOrder(order);

                //validate order total
                var orderTotalSentToMOLPay = order.GetAttribute<decimal?>(MOLPayHelper.OrderTotalSentToMOLPay);
                //if (orderTotalSentToMOLPay.HasValue && mc_gross != orderTotalSentToMOLPay.Value)
                //{
                //    var errorStr =
                //        $"MOLPay PDT. Returned order total {mc_gross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
                //    //log
                //    _logger.Error(errorStr);
                //    //order note
                //    _orderService.InsertOrderNote(new OrderNote
                //    {
                //        Note = errorStr,
                //        DisplayToCustomer = false,
                //        CreatedOnUtc = DateTime.UtcNow
                //    });
                //    _orderService.UpdateOrder(order);

                //    return RedirectToAction("Index", "Home", new { area = "" });
                //}

                //clear attribute
                if (orderTotalSentToMOLPay.HasValue)
                    _genericAttributeService.SaveAttribute<decimal?>(order, MOLPayHelper.OrderTotalSentToMOLPay, null);

                //mark order as paid
                if (newPaymentStatus == captured)
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = tranID;
                        _orderService.UpdateOrder(order);

                        _orderProcessingService.MarkOrderAsPaid(order);
                    }
                }

                order.PaymentStatus = result;

                var key0 = md5encode(tranID + orderid + status + domain + amount + currency);
                var key1 = md5encode(paydate + domain + key0 + appcode + vkey);

                if (skey != key1)
                {
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = "Invalid Transaction.",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                else
                {
                    switch (status)
                    {
                        case "00":
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = "Captured.",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                            return RedirectToRoute("CheckoutCompleted", new { orderId = orderid });
                        case "11":
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = "Failed.",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                            return RedirectToAction("Index", "Home", new { area = "" });
                        case "22":
                            _orderService.InsertOrderNote(new OrderNote
                            {
                                Note = "Pending.",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            _orderService.UpdateOrder(order);
                            return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        private string md5encode(string input)
        {
            using (MD5 hasher = MD5.Create())
            {
                byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder str = new StringBuilder();
                for (int n = 0; n <= hash.Length - 1; n++)
                {
                    str.Append(hash[n].ToString("X2"));
                }

                return str.ToString().ToLower();
            }
        }

        public IActionResult IPNHandler()
        {
            var vkey = _molPayPaymentSettings.Vkey;
            var tranID = Request.Form["tranID"];
            var orderid = Request.Form["orderid"];
            var status = Request.Form["status"];
            var domain = Request.Form["domain"];
            var amount = Request.Form["amount"];
            var currency = Request.Form["currency"];
            var appcode = Request.Form["appcode"];
            var paydate = Request.Form["paydate"];
            var skey = Request.Form["skey"];
            var nbcb = Request.Form["nbcb"];

            var key0 = md5encode(tranID + orderid + status + domain + amount + currency);
            var key1 = md5encode(paydate + domain + key0 + appcode + vkey);

            if (skey == key1)
            {
                if (nbcb == "1")
                {
                    var order = _orderService.GetOrderByNumber(Int32.Parse(orderid));

                    var sb = new StringBuilder();
                    sb.AppendLine("MOLPay PDT:");
                    sb.AppendLine("tranID: " + tranID);
                    sb.AppendLine("orderid: " + orderid);
                    sb.AppendLine("Payment status: " + status);
                    sb.AppendLine("domain: " + domain);
                    sb.AppendLine("amount: " + amount);
                    sb.AppendLine("currency: " + currency);
                    sb.AppendLine("appcode: " + appcode);
                    sb.AppendLine("paydate: " + paydate);
                    sb.AppendLine("skey: " + skey);

                    var captured = _molPayPaymentSettings.CapturedMode;
                    var failed = _molPayPaymentSettings.FailedMode;
                    var pending = _molPayPaymentSettings.PendingMode;

                    var result = pending;

                    switch (status)
                    {
                        case "00":
                            result = captured;
                            break;
                        case "11":
                            result = failed;
                            break;
                        case "22":
                            result = pending;
                            break;
                        default:
                            break;
                    }

                    var newPaymentStatus = result;
                    sb.AppendLine("New payment status: " + newPaymentStatus);

                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        //OrderId = order.Id,
                    });
                    _orderService.UpdateOrder(order);

                    if (order != null)
                    {
                        switch (status)
                        {
                            case "00":
                                {
                                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                    {
                                        order.AuthorizationTransactionId = tranID;
                                        _orderService.UpdateOrder(order);

                                        _orderProcessingService.MarkOrderAsPaid(order);
                                    }
                                }
                                break;
                            case "11":
                                {
                                    if (_orderProcessingService.CanVoidOffline(order))
                                    {
                                        _orderProcessingService.VoidOffline(order);
                                    }
                                }
                                break;
                        }
                    }
                    //Response.WriteAsync("CBTOKEN:MPSTATOK");
                    return Content(Response.WriteAsync("CBTOKEN:MPSTATOK").ToString());
                }

                if (nbcb == "2")
                {
                    var order = _orderService.GetOrderByNumber(Int32.Parse(orderid));

                    var sb = new StringBuilder();
                    sb.AppendLine("MOLPay PDT:");
                    sb.AppendLine("tranID: " + tranID);
                    sb.AppendLine("orderid: " + orderid);
                    sb.AppendLine("Payment status: " + status);
                    sb.AppendLine("domain: " + domain);
                    sb.AppendLine("amount: " + amount);
                    sb.AppendLine("currency: " + currency);
                    sb.AppendLine("appcode: " + appcode);
                    sb.AppendLine("paydate: " + paydate);
                    sb.AppendLine("skey: " + skey);

                    var captured = _molPayPaymentSettings.CapturedMode;
                    var failed = _molPayPaymentSettings.FailedMode;
                    var pending = _molPayPaymentSettings.PendingMode;

                    var result = pending;

                    switch (status)
                    {
                        case "00":
                            result = captured;
                            break;
                        case "11":
                            result = failed;
                            break;
                        case "22":
                            result = pending;
                            break;
                        default:
                            break;
                    }

                    var newPaymentStatus = result;
                    sb.AppendLine("New payment status: " + newPaymentStatus);

                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = sb.ToString(),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        //OrderId = order.Id,
                    });
                    _orderService.UpdateOrder(order);

                    if (order != null)
                    {
                        switch (status)
                        {
                            case "00":
                                {
                                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                                    {
                                        order.AuthorizationTransactionId = tranID;
                                        _orderService.UpdateOrder(order);

                                        _orderProcessingService.MarkOrderAsPaid(order);
                                    }
                                }
                                break;
                            case "11":
                                {
                                    if (_orderProcessingService.CanVoidOffline(order))
                                    {
                                        _orderProcessingService.VoidOffline(order);
                                    }
                                }
                                break;
                        }
                    }
                } //End nbcb == 2
            }


            //nothing should be rendered to visitor
            return Content("");
        }


        public IActionResult CancelOrder(IFormCollection form)
        {
            var order = _orderService.SearchOrders(storeId: _storeContext.CurrentStore.Id,
                customerId: _workContext.CurrentCustomer.Id, pageSize: 1).FirstOrDefault();
            if (order != null)
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            return RedirectToRoute("HomePage");
        }
    }
}