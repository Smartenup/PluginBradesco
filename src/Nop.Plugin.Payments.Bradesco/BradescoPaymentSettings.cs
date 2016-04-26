using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Bradesco
{
    public class BradescoPaymentSettings : ISettings
    {
        public decimal ValorFrete { get; set; }
        public string NumeroLoja { get; set; }
        public string Banco { get; set; }
        public string NumeroAgencia { get; set; }
        public string NumeroConta { get; set; }
        public string AssinaturaBoleto { get; set;}
        public string NomeServidorBradesco { get; set; }
        public string NomeSacado { get; set; }

        public int NumeroDiasAdicionaisVencimentoBoleto { get; set; }
        public bool UtilizaProdutoUnico { get; set; }
        public string NomeProdutoUnico { get; set; }
        public bool ModoDebug { get; set; }
        public string UnidadePadrao { get; set; }

        public bool UtilizaHTTPS { get; set; }

        public string Carteira { get; set; }
    }
}
