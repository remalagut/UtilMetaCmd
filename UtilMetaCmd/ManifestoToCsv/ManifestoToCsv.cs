using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilMetaCmd.Extensions;
using UtilMetaCmd.Model;

namespace UtilMetaCmd.ManifestoToCsv
{
    public class ManifestoToCsv
    {
        public static void ConversaoLogManifestoCsv(string txtFileLogManifesto)
        {
            var linhasArquivo = new List<LinhaDocumento>();
            var linhasConvertidas = new List<LogManifestoEntradaItemDto>();

            using (StreamReader sr = new StreamReader(txtFileLogManifesto))
            {
                int counter = 0;
                while (!sr.EndOfStream)
                {
                    counter++;
                    linhasArquivo.Add(new LinhaDocumento()
                    {
                        NumeroLinha = counter,
                        Conteudo = sr.ReadLine()
                    });
                }
            }


            LogManifestoEntradaItemDto registroReferencia = null;
            foreach (var linha in linhasArquivo)
            {
                var indexLinha = linhasArquivo.IndexOf(linha);
                Log.AppLog.RegistrarLog("Inicializando parse da linha " + indexLinha);

                if (linha.Conteudo.Contains("ConsumirServicoEspecifico") && linha.Conteudo.Contains(" - Início"))
                {
                    registroReferencia = new LogManifestoEntradaItemDto();
                    var dataString = linhasArquivo[indexLinha - 1].Conteudo.Substring(11, 19);
                    DateTime parsedDate = DateTime.ParseExact(dataString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    registroReferencia.Horario = parsedDate;
                    registroReferencia = new LogManifestoEntradaItemDto();
                    Log.AppLog.RegistrarLog("Inicializando registro linha " + indexLinha);
                }

                if (linha.Conteudo.Contains("Data/Hora"))
                {
                    if (registroReferencia == null)
                    {
                        Log.AppLog.RegistrarLog("Não inicializado registro para a linha  " + indexLinha);
                        continue;
                    }
                    var dataString = linha.Conteudo.Substring(11, 19);
                    DateTime parsedDate = DateTime.ParseExact(dataString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    if (registroReferencia.Horario == null)
                    {
                        registroReferencia.Horario = parsedDate;
                    }

                    else
                    {
                        var differenceInMinutes = registroReferencia.Horario.Value.Subtract(parsedDate).TotalSeconds;
                        if (differenceInMinutes > 10)
                        {
                            //tratar registro com diferença de horarios, setar pra null e depois registrar em log de conversão
                            registroReferencia = null;
                            continue;
                        }
                    }

                }

                if (registroReferencia == null)
                {
                    Log.AppLog.RegistrarLog("Continuando porque registro não está inicializado " + indexLinha);
                    continue;
                }

                if (linha.Conteudo.Contains("ConsumirServicoEspecifico") && linha.Conteudo.Contains(" - Fim"))
                {
                    linhasConvertidas.Add(registroReferencia);
                    registroReferencia = null;
                    Log.AppLog.RegistrarLog("Finalizando registro linha " + indexLinha);
                    //add o registro na lista de registros e zera o registro
                }

                if (linha.Conteudo.Contains("ConsumirServicoEspecifico") && linha.Conteudo.Contains(" - Url web service"))
                {
                    var lastHttps = linha.Conteudo.LastIndexOf("https");
                    registroReferencia.UrlWebService = linha.Conteudo.Substring(111, 74);
                    //add o registro na lista de registros e zera o registro

                }

                if (linha.Conteudo.Contains("<distDFeInt"))
                {
                    registroReferencia.UltNSUEnvio = linha.Conteudo.GetXmlTagValue("ultNSU");
                    if (registroReferencia.UltNSUEnvio == null)
                    {
                        registroReferencia.UltNSUEnvio = "Registro por chave " + linha.Conteudo.GetXmlTagValue("chNFe");
                    }
                    registroReferencia.CNPJ = linha.Conteudo.GetXmlTagValue("CNPJ");

                    registroReferencia.XmlEnvio = linha.Conteudo;
                    registroReferencia.NumeroLinhaEnvio = indexLinha;
                }

                if (linha.Conteudo.Contains("<retDistDFeInt"))
                {

                    registroReferencia.XmlEnvio = linha.Conteudo;
                    registroReferencia.CStatRetorno = linha.Conteudo.GetXmlTagValue("cStat");
                    registroReferencia.XMotivoRetorno = linha.Conteudo.GetXmlTagValue("xMotivo");
                    registroReferencia.MaxNSURetorno = linha.Conteudo.GetXmlTagValue("maxNSU");
                    registroReferencia.UltNSURetorno = linha.Conteudo.GetXmlTagValue("ultNSU");
                    registroReferencia.NumeroLinhaRetorno = indexLinha;
                }
            }

            using (StreamWriter swConvertedFile = new StreamWriter(Path.Combine(new FileInfo(txtFileLogManifesto).Directory.FullName, "ConvertedFile.csv")))
            {
                swConvertedFile.WriteLine("NumeroLinhaEnvio;NumeroLinhaRetorno;CNPJ;Horario;CStatRetorno;XMotivoRetorno;UltNSUEnvio;UltNSURetorno;MaxNSURetorno");
                var objetosConvertidosAgrupadosPorCnpj = linhasConvertidas.OrderBy(x => x.CNPJ).GroupBy(x => x.CNPJ);
                foreach (var itemEmpresa in objetosConvertidosAgrupadosPorCnpj)
                {
                    swConvertedFile.WriteLine("CNPJ: " + itemEmpresa.Key);
                    //swConvertedFile.WriteLine(itemEmpresa.NumeroLinhaEnvio + ";" + item.NumeroLinhaRetorno +
                    //    ";" + item.CNPJ + ";" + item.Horario.Value.ToShortDateString() + " - " + item.Horario.Value.ToShortTimeString()
                    //    + ";" + item.CStatRetorno + ";" + item.XMotivoRetorno + ";" + item.UltNSUEnvio + ";" + item.UltNSURetorno + ";" + item.MaxNSURetorno);
                    foreach (var item in itemEmpresa)
                    {
                        swConvertedFile.WriteLine(item.NumeroLinhaEnvio + ";" + item.NumeroLinhaRetorno +
                       ";" + item.CNPJ + ";" + item.Horario.Value.ToString("dd/MM/yyyy") + " - " + item.Horario.Value.ToString("HH:mm:ss")
                       + ";" + item.CStatRetorno + ";" + item.XMotivoRetorno + ";" + item.UltNSUEnvio + ";" + item.UltNSURetorno + ";" + item.MaxNSURetorno);
                    }
                }
            }
        }

        //public static string GetXmlTagValue(string xml, string tag)
        //{
        //    var indexTag = xml.IndexOf("<" + tag + ">");

        //    if (indexTag < 0)
        //    {
        //        return null;
        //    }

        //    var indexTagValue = indexTag + tag.Length + 2;
        //    var indexFechamentoTag = xml.IndexOf("</" + tag + ">");
        //    var valueLength = indexFechamentoTag - indexTagValue;
        //    var value = xml.Substring(indexTagValue, valueLength);
        //    return value;
        //}

        //public static string RemoveInvalidChars(string strSource)
        //{
        //    return Regex.Replace(strSource, @"[^0-9a-zA-Z=+/]", "");
        //}

    }
}
