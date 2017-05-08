using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Bradesco.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.Manager")]
        public string Manager { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.SenhaManager")]
        public string SenhaManager { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NomeSacado")]
        public string NomeSacado { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NumeroLoja")]
        public string NumeroLoja { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.Banco")]
        public string Banco { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NumeroAgencia")]
        public string NumeroAgencia { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NumeroConta")]
        public string NumeroConta { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.AssinaturaBoleto")]
        public string AssinaturaBoleto { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NomeServidorBradesco")]
        public string NomeServidorBradesco { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NumeroDiasAdicionaisVencimentoBoleto")]
        public int NumeroDiasAdicionaisVencimentoBoleto { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.UtilizaProdutoUnico")]
        public bool UtilizaProdutoUnico { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.NomeProdutoUnico")]
        public string NomeProdutoUnico { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.ModoDebug")]
        public bool ModoDebug { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.UnidadePadrao")]
        public string UnidadePadrao { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.UtilizaHTTPS")]
        public bool UtilizaHTTPS { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.Carteira")]
        public string Carteira { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Bradesco.Fields.AdicionarNotaPrazoFabricaoEnvio")]
        public bool AdicionarNotaPrazoFabricaoEnvio { get; set; }

    }
}
