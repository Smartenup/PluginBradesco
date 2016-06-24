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

        /// <summary>
        /// Usuário utilizado para logar no site administrativo do bradesco
        /// https://mup.comercioeletronico.com.br/sepsManager/login.asp?merchantid=100004933&url=%2FsepsManager%2Fcompras%2Easp%3FMerchantid%3D100004933
        /// </summary>
        public string Manager { get; set; }

        /// <summary>
        /// Senha utilizada para logar no site administrativo do bradesco
        /// https://mup.comercioeletronico.com.br/sepsManager/login.asp?merchantid=100004933&url=%2FsepsManager%2Fcompras%2Easp%3FMerchantid%3D100004933
        /// </summary>
        public string SenhaManager { get; set; }
    }
}
