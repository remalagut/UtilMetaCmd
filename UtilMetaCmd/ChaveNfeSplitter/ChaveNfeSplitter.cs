using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Powered by https://chat.openai.com/, feito para validação do AI e não implementado ainda.
/// </summary>
namespace UtilMetaCmd.ChaveNfeSplitter
{
    public class ChaveNfeSplitter
    {
        public static void SplitChaveNfe(string chave)
        {
            //string chaveNFe = "35210578901234567890123456789012345678901234";
            var componentes = SepararComponentesChaveNFe(chave);

            Console.WriteLine("Ano: " + componentes["ano"]);
            Console.WriteLine("Mês: " + componentes["mes"]);
            Console.WriteLine("CNPJ: " + componentes["cnpj"]);
            Console.WriteLine("Modelo: " + componentes["modelo"]);
            Console.WriteLine("Série: " + componentes["serie"]);
            Console.WriteLine("Número: " + componentes["numero"]);
            Console.WriteLine("Tipo: " + componentes["tipo"]);
            Console.WriteLine("Código: " + componentes["codigo"]);
            Console.WriteLine("Dígito Verificador: " + componentes["digito"]);
        }

        private static Dictionary<string, string> SepararComponentesChaveNFe(string chave)
        {
            var campos = new Dictionary<string, string>
        {
            { "ano", chave.Substring(2, 2) },
            { "mes", chave.Substring(4, 2) },
            { "cnpj", chave.Substring(6, 14) },
            { "modelo", chave.Substring(20, 2) },
            { "serie", chave.Substring(22, 3) },
            { "numero", chave.Substring(25, 9) },
            { "tipo", chave.Substring(34, 1) },
            { "codigo", chave.Substring(35, 8) },
            { "digito", chave.Substring(43) }
        };

            return campos;
        }
    }
}
