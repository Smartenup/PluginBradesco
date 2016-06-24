using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Bradesco.Controllers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Payments;
using Nop.Services.Tasks;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Nop.Plugin.Payments.Bradesco
{
    public class BradescoPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields
        private readonly BradescoPaymentSettings _bradescoPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IWorkContext _workContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        #endregion

        public BradescoPaymentProcessor(
            BradescoPaymentSettings bradescoPaymentSettings,
            ISettingService settingService,
            ITaxService taxService, 
            IPriceCalculationService priceCalculationService,
            ICurrencyService currencyService, 
            ICustomerService customerService,
            CurrencySettings currencySettings, 
            IWebHelper webHelper,
            StoreInformationSettings storeInformationSettings,
            IAddressAttributeParser addressAttributeParser,
            IWorkContext workContext,
            IScheduleTaskService scheduleTaskService
            )
        {
            _bradescoPaymentSettings  = bradescoPaymentSettings;
            _settingService           = settingService;
            _taxService               = taxService;
            _priceCalculationService  = priceCalculationService;
            _currencyService          = currencyService;
            _customerService          = customerService;
            _currencySettings         = currencySettings;
            _webHelper                = webHelper;
            _storeInformationSettings = storeInformationSettings;
            _addressAttributeParser   = addressAttributeParser;
            _workContext              = workContext;
            _scheduleTaskService      = scheduleTaskService;
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {

            string protocolo = _bradescoPaymentSettings.UtilizaHTTPS ? @"https://" : @"http://";

            string link_Boleto = protocolo + _bradescoPaymentSettings.NomeServidorBradesco + "/sepsBoletoRet/" + _bradescoPaymentSettings.NumeroLoja
                + "/prepara_pagto.asp?merchantid=" + _bradescoPaymentSettings.NumeroLoja
                + "&orderid=" + postProcessPaymentRequest.Order.Id
                + "&numOrder=" + postProcessPaymentRequest.Order.Id;

            HttpContext.Current.Response.Redirect(link_Boleto);

        }

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _bradescoPaymentSettings.ValorFrete;
        }

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("bool CanRePostProcessPayment");

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentBradesco";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Bradesco.Controllers" }, { "area", null } };

        }

        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentBradesco";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Bradesco.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentBradescoController);
        }

        public bool SupportCapture
        {
            get { return false; }
        }

        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        public bool SupportRefund
        {
            get { return false; }
        }

        public bool SupportVoid
        {
            get { return false; }
        }

        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; ; }
        }

        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        public bool SkipPaymentInfo
        {
            get { return false; }
        }


        public override void Install()
        {
            base.Install();

            ScheduleTask taskByType = _scheduleTaskService.GetTaskByType("Nop.Plugin.Payments.Bradesco.BradescoPaymentUpdateTask, Nop.Plugin.Payments.Bradesco");

            if (taskByType == null)
            {
                taskByType = new ScheduleTask() {
                    Enabled = false,
                    Name = "BradescoPaymentUpdateTask",
                    Seconds = 600,
                    StopOnError = false,
                    Type = "Nop.Plugin.Payments.Bradesco.BradescoPaymentUpdateTask, Nop.Plugin.Payments.Bradesco"
                };

                _scheduleTaskService.InsertTask(taskByType);
            }
        }

        public override void Uninstall()
        {
            base.Uninstall();

            ScheduleTask taskByType = _scheduleTaskService.GetTaskByType("Nop.Plugin.Payments.Bradesco.BradescoPaymentUpdateTask, Nop.Plugin.Payments.Bradesco");

            if (taskByType != null)
            {
                _scheduleTaskService.DeleteTask(taskByType);
            }
        }
    }
}
