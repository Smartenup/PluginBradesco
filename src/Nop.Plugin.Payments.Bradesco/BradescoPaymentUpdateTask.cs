using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Nop.Plugin.Payments.Bradesco
{
    public class BradescoPaymentUpdateTask : ITask
    {

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly BradescoPaymentSettings _bradescoPaymentSettings;
        private readonly ILogger _logger;


        public BradescoPaymentUpdateTask(IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            BradescoPaymentSettings bradescoPaymentSettings
            )
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _bradescoPaymentSettings = bradescoPaymentSettings;
            _logger = logger;
       }

        public void Execute()
        {
            
            try
            {
                ///Obtem as parametrizações para acesso ao boleto bradesco

                string protocolo = @"https://";

                string link_Boleto = protocolo + _bradescoPaymentSettings.NomeServidorBradesco + "/sepsmanager/ArqRetBradescoBoleto_XML2_dados.asp?"
                    + "merchantid=" + _bradescoPaymentSettings.NumeroLoja
                    + "&data=" + DateTime.Now.ToString("dd/MM/yyyy")
                    + "&Manager=" + _bradescoPaymentSettings.Manager
                    + "&passwd=" + _bradescoPaymentSettings.SenhaManager
                    + "&NumOrder=";

                if (_bradescoPaymentSettings.ModoDebug)
                    _logger.Information(link_Boleto);

                var orders = _orderService.SearchOrders(paymentMethodSystemName: "Payments.Bradesco", os: Core.Domain.Orders.OrderStatus.Pending,
                    ps: Core.Domain.Payments.PaymentStatus.Pending
                );


                foreach (var order in orders)
                {
                    Stream dataStream = null;
                    WebResponse response = null;

                    if (order.PaymentStatusId != (int)Nop.Core.Domain.Payments.PaymentStatus.Pending)
                        continue;
                    try
                    {

                        //WebRequest request = WebRequest.Create("https://mup.comercioeletronico.com.br/sepsmanager/ArqRetBradescoBoleto_XML2_dados.asp?merchantid=100004933&data=20/06/2016&Manager=adm_imp4933&passwd=uiskas7680&NumOrder=9447");

                        string link_boleto_ordem = string.Concat(link_Boleto, order.Id.ToString());

                        if (_bradescoPaymentSettings.ModoDebug)
                            _logger.Information(link_boleto_ordem);

                        var request = WebRequest.Create(link_boleto_ordem);
                        request.Credentials = CredentialCache.DefaultCredentials;

                        response = request.GetResponse();
                        dataStream = response.GetResponseStream();

                        var document = XDocument.Load(dataStream);

                        IEnumerable<XAttribute> attributeList = document.Element("DadosFechamento").Element("Bradesco").Element("Pedido").Attributes();

                        foreach (XAttribute att in attributeList)
                        {
                            if (_bradescoPaymentSettings.ModoDebug)
                                _logger.Information(att.Name + " = " + att.Value );

                            if (att.Name == "LinhaDigitavel")
                            {
                                var notaLinhaDigitavel = order.OrderNotes.Where(note => note.Note.Contains("Linha Digitável Boleto: "));
                                
                                ///Caso não tenha anotação da linha do boleto é adicionado 
                                if (notaLinhaDigitavel.Count() == 0)
                                {
                                    AddOrderNote("Linha Digitável Boleto: " + att.Value , true, order);
                                }
                            }

                            if (att.Name == "Status")
                            {
                                ///15.........................Boleto Pago  
                                ///21.........................Boleto Pago Igual (Boleto Bancário com retorno para a loja) 
                                if (att.Value.Equals("15") || att.Value.Equals("21"))
                                {
                                    _orderProcessingService.MarkOrderAsPaid(order);
                                    AddOrderNote("Pagamento aprovado.", true, order);
                                    AddOrderNote("Aguardando Impressão - Excluir esse comentário ao imprimir ", false, order);
                                }
                                ///22.........................Boleto Pago Menor (Boleto Bancário com retorno para a loja) 
                                if (att.Value.Equals("22"))
                                {
                                    AddOrderNote("Boleto Pago Menor - Por favor entrar em contato para verificar a diferença", true, order);
                                }
                                ///23.........................Boleto Pago Maior (Boleto Bancário com retorno para a loja)
                                if (att.Value.Equals("23"))
                                {
                                    _orderProcessingService.MarkOrderAsPaid(order);
                                    AddOrderNote("Boleto Pago Maior - Por favor entrar em contato para verificar a diferença", true, order);
                                    AddOrderNote("Aguardando Impressão - Excluir esse comentário ao imprimir ", false, order);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Erro na atualização de status de boleto bradesco, orderID " + order.Id.ToString(), ex);
                    }
                    finally
                    {
                        dataStream.Flush();
                        response.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error("Erro na execução de atualização de status de boleto bradesco", ex, null);
            }
        }

        [NonAction]
        //Adiciona anotaçoes ao pedido
        private void AddOrderNote(string note, bool showNoteToCustomer, Nop.Core.Domain.Orders.Order order)
        {
            var orderNote = new Nop.Core.Domain.Orders.OrderNote();
            orderNote.CreatedOnUtc = DateTime.UtcNow;
            orderNote.DisplayToCustomer = showNoteToCustomer;
            orderNote.Note = note;
            order.OrderNotes.Add(orderNote);

            this._orderService.UpdateOrder(order);
        }

    }
}
