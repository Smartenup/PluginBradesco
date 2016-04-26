using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Bradesco.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;


namespace Nop.Plugin.Payments.Bradesco.Controllers
{
    public class PaymentBradescoController : BasePaymentController
    {

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly BradescoPaymentSettings _bradescoPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressAttributeParser _addressAttributeParser;


        public PaymentBradescoController(IWorkContext workContext,
            IStoreService storeService, 
            ISettingService settingService, 
            IPaymentService paymentService, 
            IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            ILogger logger, 
            PaymentSettings paymentSettings, 
            ILocalizationService localizationService,
            BradescoPaymentSettings bradescoPaymentSettings,
            IAddressAttributeParser addressAttributeParser
            )
        {
            this._workContext = workContext;
            this._storeService = storeService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._localizationService = localizationService;
            this._bradescoPaymentSettings = bradescoPaymentSettings;
            this._addressAttributeParser = addressAttributeParser;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var bradescoPaymentSettings = _settingService.LoadSetting<BradescoPaymentSettings>(storeScope);

            var model = new ConfigurationModel();

            model.NomeSacado                            = bradescoPaymentSettings.NomeSacado;
            model.NumeroLoja                            = bradescoPaymentSettings.NumeroLoja;
            model.Banco                                 = bradescoPaymentSettings.Banco;
            model.NumeroAgencia                         = bradescoPaymentSettings.NumeroAgencia;
            model.NumeroConta                           = bradescoPaymentSettings.NumeroConta;
            model.AssinaturaBoleto                      = bradescoPaymentSettings.AssinaturaBoleto;
            model.NomeServidorBradesco                  = bradescoPaymentSettings.NomeServidorBradesco;
            model.NumeroDiasAdicionaisVencimentoBoleto  = bradescoPaymentSettings.NumeroDiasAdicionaisVencimentoBoleto;
            model.UtilizaProdutoUnico                   = bradescoPaymentSettings.UtilizaProdutoUnico; 
            model.NomeProdutoUnico                      = bradescoPaymentSettings.NomeProdutoUnico;
            model.ModoDebug                             = bradescoPaymentSettings.ModoDebug;
            model.UnidadePadrao                         = bradescoPaymentSettings.UnidadePadrao;
            model.UtilizaHTTPS                          = bradescoPaymentSettings.UtilizaHTTPS;
            model.Carteira                              = bradescoPaymentSettings.Carteira;

            return View("~/Plugins/Payments.Bradesco/Views/PaymentBradesco/Configure.cshtml", model);
        }


        [HttpPost]
        [AdminAuthorize]
        [AdminAntiForgery]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var bradescoPaymentSettings = _settingService.LoadSetting<BradescoPaymentSettings>(storeScope);


            //save settings
            bradescoPaymentSettings.NomeSacado                              = model.NomeSacado;
            bradescoPaymentSettings.NumeroLoja                              = model.NumeroLoja;
            bradescoPaymentSettings.Banco                                   = model.Banco;
            bradescoPaymentSettings.NumeroAgencia                           = model.NumeroAgencia;
            bradescoPaymentSettings.NumeroConta                             = model.NumeroConta;
            bradescoPaymentSettings.AssinaturaBoleto                        = model.AssinaturaBoleto;
            bradescoPaymentSettings.NomeServidorBradesco                    = model.NomeServidorBradesco;
            bradescoPaymentSettings.NumeroDiasAdicionaisVencimentoBoleto    = model.NumeroDiasAdicionaisVencimentoBoleto;
            bradescoPaymentSettings.UtilizaProdutoUnico                     = model.UtilizaProdutoUnico;
            bradescoPaymentSettings.NomeProdutoUnico                        = model.NomeProdutoUnico;
            bradescoPaymentSettings.ModoDebug                               = model.ModoDebug;
            bradescoPaymentSettings.UnidadePadrao                           = model.UnidadePadrao; 
            bradescoPaymentSettings.UtilizaHTTPS                            = model.UtilizaHTTPS; 
            bradescoPaymentSettings.Carteira                                = model.Carteira; 


            _settingService.SaveSetting(bradescoPaymentSettings);

            //now clear settings cache
            _settingService.ClearCache();

            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return View("~/Plugins/Payments.Bradesco/Views/PaymentBradesco/Configure.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Payments.Bradesco/Views/PaymentBradesco/PaymentInfo.cshtml");
        }

        [ValidateInput(false)]
        public ActionResult PaymentData()
        {
            try
            {
                if (_bradescoPaymentSettings.ModoDebug)
                {
                    _logger.Information("Plugin.Payments.Bradesco: Request[transId] : " + Request["transId"]);
                    _logger.Information("Plugin.Payments.Bradesco: Request[numOrder] : " + Request["numOrder"]);
                    _logger.Information("Plugin.Payments.Bradesco: Request[Cod] : " + Request["Cod"]);
                }


                if (Request["transId"] == "getBoleto")
                {
                    
                    string numeroPedido = string.Empty;

                    if (Request["numOrder"].IndexOf(",") != 0)
                        numeroPedido = Request["numOrder"].Substring(0, Request["numOrder"].IndexOf(","));
                    else
                        numeroPedido = Request["numOrder"];                     

                    string resultOrder = string.Empty;
                    string resultboleto = string.Empty;


                    Order order = _orderService.GetOrderById(int.Parse(numeroPedido));

                    resultOrder = "<BEGIN_ORDER_DESCRIPTION><orderid>=(" + numeroPedido + ")";

                    Decimal itensTotalValue = 0;

                    if (_bradescoPaymentSettings.UtilizaProdutoUnico)
                    {
                        resultOrder += "<descritivo>=(" + _bradescoPaymentSettings.NomeProdutoUnico + ")";
                        resultOrder += "<quantidade>=(" + "1" + ")";
                        resultOrder += "<unidade>=(" + _bradescoPaymentSettings.UnidadePadrao + ")";
                        resultOrder += "<valor>=(" + Decimal.Round(order.OrderTotal, 2).ToString().Replace(",", "").Replace(".", "") + ")";

                        itensTotalValue += Decimal.Round(order.OrderTotal, 2);
                    }
                    else
                    {
                        ///Caso utilize o envio de todos os produtos para o Bradesco não existe campo de desconto.
                        ///É necessário verificar se houve descontos e aplica-lo aos itens do carrinho.
                        ///O valor total dos itens do carrinho deve ser igual ao valor total do pedido

                        var cartItems = order.OrderItems;


                        ///Produto sem desconto
                        if (order.OrderSubTotalDiscountInclTax == 0)
                        {
                            foreach (var item in cartItems)
                            {
                                decimal productTotal = (item.UnitPriceInclTax * item.Quantity);

                                string productName = this.GetProcuctName(item);

                                productName = this.AddItemDescrition(productName, item);

                                resultOrder += "<descritivo>=(" + productName + ")";
                                resultOrder += "<quantidade>=(" + item.Quantity.ToString() + ")";
                                resultOrder += "<unidade>=(" + _bradescoPaymentSettings.UnidadePadrao + ")";
                                resultOrder += "<valor>=(" + Decimal.Round(productTotal, 2).ToString().Replace(",", "").Replace(".", "") + ")";
                                itensTotalValue += Decimal.Round(productTotal, 2);
                            }

                        }
                        ///Produto com desconto utilizando percentual
                        else if(order.DiscountUsageHistory.ElementAt(0).Discount.UsePercentage)
                        {

                            foreach (var item in cartItems)
                            {
                                decimal productTotal = (item.UnitPriceInclTax * item.Quantity);

                                decimal discount = productTotal * (order.DiscountUsageHistory.ElementAt(0).Discount.DiscountPercentage/100);

                                decimal productValue = Decimal.Round((productTotal - discount), 2);

                                string productName = this.GetProcuctName(item);

                                productName = this.AddItemDescrition(productName, item);

                                resultOrder += "<descritivo>=(" + productName + ")";
                                resultOrder += "<quantidade>=(" + item.Quantity.ToString() + ")";
                                resultOrder += "<unidade>=(" + _bradescoPaymentSettings.UnidadePadrao + ")";

                                resultOrder += "<valor>=(" + productValue.ToString().Replace(",", "").Replace(".", "") + ")";

                                itensTotalValue += productValue;

                            }
                        }
                        else
                        {
                            ///Caso não usa o valor percentual, estamos considerando que utiliza um valor padrão
                            decimal discountTotal = order.DiscountUsageHistory.ElementAt(0).Discount.DiscountAmount;

                            decimal discountApplied = decimal.Zero;

                            var ordenedCartItens = cartItems.OrderByDescending(x => x.UnitPriceInclTax * x.Quantity).ToList();

                            int count = 0;

                            foreach (var item in ordenedCartItens)
                            {
                                count++;

                                decimal productTotal = (item.UnitPriceInclTax * item.Quantity);

                                ///Proporção do item ao pedido
                                decimal proportionItem = productTotal / order.OrderSubtotalInclTax;

                                ///Valor proporcional de desconto
                                decimal proportionalDescount = Decimal.Round((discountTotal * proportionItem), 2);

                                //Obtem o valor de desconto
                                discountApplied += proportionalDescount;

                                ///Aplicar o desconto
                                decimal productWithDescount = productTotal - proportionalDescount;

                                ///Acerto de erro por arredondamento
                                ///Caso esta no ultimo item, verifica se o desconto aplicado é igual ao desconto total
                                ///Caso contrário corrige a diferença
                                if (count == ordenedCartItens.Count)
                                {
                                    if (discountApplied != discountTotal)
                                    {
                                        productWithDescount += (discountTotal - discountApplied);
                                    }
                                }


                                string productName = this.GetProcuctName(item);

                                productName = this.AddItemDescrition(productName, item);

                                resultOrder += "<descritivo>=(" + productName + ")";
                                resultOrder += "<quantidade>=(" + item.Quantity.ToString() + ")";
                                resultOrder += "<unidade>=(" + _bradescoPaymentSettings.UnidadePadrao + ")";

                                resultOrder += "<valor>=(" + Decimal.Round(productWithDescount, 2).ToString().Replace(",", "").Replace(".", "") + ")";

                                itensTotalValue += Decimal.Round(productWithDescount, 2);

                            }

                        }

                        if (order.OrderShippingInclTax > 0)
                        {
                            resultOrder += "<adicional>=(frete)";
                            resultOrder += "<valorAdicional>=(" + Decimal.Round(order.OrderShippingInclTax, 2).ToString().Replace(",", "").Replace(".", "") + ")";

                            itensTotalValue += Decimal.Round(order.OrderShippingInclTax, 2);
                        }

                    }

                    resultOrder += "<END_ORDER_DESCRIPTION>";

                    resultboleto += "<BEGIN_BOLETO_DESCRIPTION>";
                    resultboleto += "<CEDENTE>=(" + _bradescoPaymentSettings.NomeSacado + ")";
                    resultboleto += "<BANCO>=(" + _bradescoPaymentSettings.Banco + ")";
                    resultboleto += "<NUMEROAGENCIA>=(" + _bradescoPaymentSettings.NumeroAgencia + ")";
                    resultboleto += "<NUMEROCONTA>=(" + _bradescoPaymentSettings.NumeroConta + ")";
                    resultboleto += "<ASSINATURA>=(" + _bradescoPaymentSettings.AssinaturaBoleto + ")";
                    resultboleto += "<DATAEMISSAO>=(" + FormataData(System.DateTime.Now.Date) + ")";
                    resultboleto += "<DATAPROCESSAMENTO>=(" + FormataData(System.DateTime.Now.Date) + ")";

                    var dataVencimento = System.DateTime.Now.AddDays(_bradescoPaymentSettings.NumeroDiasAdicionaisVencimentoBoleto).Date;

                    resultboleto += "<DATAVENCIMENTO>=(" + FormataData(dataVencimento.Date) + ")"; 
                    resultboleto += "<NOMESACADO>=(" + this.GetBillingShippingFullName(order.BillingAddress) + ")";

                    var number = string.Empty;
                    var complement = string.Empty;
                    var cnpjcpf = string.Empty;

                    GetCustomNumberAndComplement(order, out number, out complement, out cnpjcpf);

                    resultboleto += "<ENDERECOSACADO>=(" + order.BillingAddress.Address1 + " - Numero " + number + " - Complemento " + complement + ")";
                    resultboleto += "<CIDADESACADO>=(" + order.BillingAddress.City + ")";
                    resultboleto += "<UFSACADO>=(" + order.BillingAddress.StateProvince.Abbreviation + ")";
                    resultboleto += "<CEPSACADO>=(" + RetouchPostalCode(order.BillingAddress.ZipPostalCode) + ")";
                    resultboleto += "<CPFSACADO>=(" + GetOnlyNumbers(cnpjcpf) + ")";
                    resultboleto += "<NUMEROPEDIDO>=(" + numeroPedido + ")";

                    if (itensTotalValue != order.OrderTotal)
                        resultboleto += "<VALORDOCUMENTOFORMATADO>=(R$" + itensTotalValue.ToString("0,0.00") + ")";
                    else
                        resultboleto += "<VALORDOCUMENTOFORMATADO>=(R$" + order.OrderTotal.ToString("0,0.00") + ")";
                    
                    resultboleto += "<SHOPPINGID>=(1)";
                    resultboleto += "<NUMDOC>=(" + numeroPedido + ")";
                    resultboleto += "<CARTEIRA>=(" + _bradescoPaymentSettings.Carteira + ")";
                    resultboleto += "<ANONOSSONUMERO>=(97)";
                    resultboleto += "<END_BOLETO_DESCRIPTION>";

                    if (_bradescoPaymentSettings.ModoDebug)
                    {
                        _logger.Information(resultOrder + resultboleto);
                    }

                    return Content(resultOrder + resultboleto, "text/plain", System.Text.Encoding.ASCII);
                }
                if (Request["transId"] == "putAuthBoleto")
                {
                    return Content("<PUT_AUTH_OK>", "text/plain");
                }
                if (!string.IsNullOrWhiteSpace(Request["Cod"]))
                {
                    string request = string.Empty;

                    foreach (var item in Request.Form.AllKeys)
	                {
		                request += Request[item];
	                }

                    return Content("<ERRO>" + request, "text/plain");
                }
                else
                {
                    return Content("");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Plugin.Payments.Bradesco: erro ao enviar os dados da compra para o bradesco - Erro" + ex.Message , ex);
                throw;
            }

        }



        [NonAction]
        private void GetCustomNumberAndComplement(Order order, out string number, out string complement, out string cnpjcpf)
        {

            string customAttributes = order.BillingAddress.CustomAttributes;

            number = string.Empty;
            complement = string.Empty;
            cnpjcpf = string.Empty;

            if (!string.IsNullOrWhiteSpace(customAttributes))
            {
                var attributes = _addressAttributeParser.ParseAddressAttributes(customAttributes);

                for (int i = 0; i < attributes.Count; i++)
                {
                    var valuesStr = _addressAttributeParser.ParseValues(customAttributes, attributes[i].Id);

                    var attributeName = attributes[i].GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);

                    if (
                        attributeName.Equals("Número", StringComparison.InvariantCultureIgnoreCase) ||
                        attributeName.Equals("Numero", StringComparison.InvariantCultureIgnoreCase)
                        )
                    {
                        number = _addressAttributeParser.ParseValues(customAttributes, attributes[i].Id)[0];
                    }

                    if (attributeName.Equals("Complemento", StringComparison.InvariantCultureIgnoreCase))
                        complement = _addressAttributeParser.ParseValues(customAttributes, attributes[i].Id)[0];

                    if (attributeName.Equals("CPF/CNPJ", StringComparison.InvariantCultureIgnoreCase))
                        cnpjcpf = _addressAttributeParser.ParseValues(customAttributes, attributes[i].Id)[0];
                }

            }

            if (string.IsNullOrWhiteSpace(cnpjcpf))
                cnpjcpf = order.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Fax);

            if (!string.IsNullOrWhiteSpace(cnpjcpf))
                cnpjcpf = GetOnlyNumbers(cnpjcpf);            
        }

        [NonAction]
        private string GetBillingShippingFullName(Nop.Core.Domain.Common.Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            string firstName = address.FirstName;
            string lastName = address.LastName;
            string stringWithTwoOrMoreSpace = "";

            if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
            {
                stringWithTwoOrMoreSpace = string.Format("{0} {1}", firstName, lastName);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    stringWithTwoOrMoreSpace = firstName;
                }
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    stringWithTwoOrMoreSpace = lastName;
                }
            }


            string billingShippingFullName = this.RemoveIncorrectSpaces(stringWithTwoOrMoreSpace);

            return billingShippingFullName;
        }

        [NonAction]
        private string RemoveIncorrectSpaces(string stringWithTwoOrMoreSpace)
        {
            string str = string.Empty;
            for (int i = 0; i < stringWithTwoOrMoreSpace.Length; i++)
            {
                if (stringWithTwoOrMoreSpace[i] == ' ')
                {
                    if (((i + 1) < stringWithTwoOrMoreSpace.Length) && (stringWithTwoOrMoreSpace[i + 1] != ' '))
                    {
                        str = str + stringWithTwoOrMoreSpace[i];
                    }
                }
                else
                {
                    str = str + stringWithTwoOrMoreSpace[i];
                }
            }
            return str.Trim();
        }

        [NonAction]
        private string RetouchPostalCode(string postalCode)
        {
            string stringOnlyNumbers = this.GetOnlyNumbers(postalCode);

            if (stringOnlyNumbers.Length > 8)
            {
                stringOnlyNumbers = stringOnlyNumbers.Substring(0, 8);
            }

            return stringOnlyNumbers;
        }
        
        [NonAction]
        private string GetOnlyNumbers(string stringValue)
        {
            Regex r = new Regex(@"\d+");

            string result = "";
            foreach (Match m in r.Matches(stringValue))
                result += m.Value;

            return result;
        }

        [NonAction]
        private string FormataData(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy");
        }

        [NonAction]
        private string GetProcuctName(OrderItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.Product.Name))
            {
                return item.Product.Name;
            }
            return "Nome não especificado";
        }

        [NonAction]
        private string AddItemDescrition(string productName, OrderItem item)
        {
            string attributeDescription = item.AttributeDescription;
            if (!string.IsNullOrWhiteSpace(attributeDescription))
            {
                attributeDescription = Regex.Replace(attributeDescription, @"<(.|\n)*?>", " - ");
            }
            productName = string.IsNullOrWhiteSpace(attributeDescription) ? productName : (productName + " - " + attributeDescription);
            return productName;
        }
    }
}
