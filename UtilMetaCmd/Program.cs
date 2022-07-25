using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using UtilMetaCmd.Model;
using System.Globalization;
using System.Xml;
using System.Text.RegularExpressions;

namespace UtilMetaCmd
{
    class Program
    {
        static StreamWriter logWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "LogConversao.log"));
        public Program()
        {
        }

        static void Main(string[] args)
        {
            //var txtFileLogManifesto = @"C:\Program Files (x86)\Meta.Net\TK335505\AbelGalinha-LogMetaServerGlobal_GerenciadorNotaFiscalManifestacao25072022.log";
            var txtFileLogManifesto = args[0];

            var linhasArquivo = new List<String>();
            var linhasConvertidas = new List<LogManifestoEntradaItemDto>();

            using(StreamReader sr = new StreamReader(txtFileLogManifesto))
            {
                while (!sr.EndOfStream)
                {
                    linhasArquivo.Add(sr.ReadLine());
                }
            }

            LogManifestoEntradaItemDto registroReferencia = new LogManifestoEntradaItemDto();

            foreach(var linha in linhasArquivo)
            {
                var indexLinha = linhasArquivo.IndexOf(linha);
                RegistrarLog("Inicializando parse da linha " + indexLinha);

                if (linha.Contains("ConsumirServicoEspecifico") && linha.Contains(" - Início"))
                {
                    registroReferencia = new LogManifestoEntradaItemDto();
                    var dataString = linhasArquivo[indexLinha - 1].Substring(11, 19);
                    DateTime parsedDate = DateTime.ParseExact(dataString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    registroReferencia.Horario = parsedDate;
                    registroReferencia = new LogManifestoEntradaItemDto();
                    RegistrarLog("Inicializando registro linha " + indexLinha);
                }

                if (linha.Contains("Data/Hora"))
                {
                    if(registroReferencia == null)
                    {
                        RegistrarLog("Não inicializado registro para a linha  " + indexLinha);
                        continue;
                    }
                    var dataString = linha.Substring(11, 19);
                    DateTime parsedDate = DateTime.ParseExact(dataString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    if(registroReferencia.Horario == null)
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

                if(registroReferencia == null)
                {
                    RegistrarLog("Continuando porque registro não está inicializado " + indexLinha);
                    continue;
                }

                if (linha.Contains("ConsumirServicoEspecifico") && linha.Contains(" - Fim"))
                {
                    linhasConvertidas.Add(registroReferencia);
                    registroReferencia = null;
                    RegistrarLog("Finalizando registro linha " + indexLinha);
                    //add o registro na lista de registros e zera o registro
                }

                if (linha.Contains("ConsumirServicoEspecifico") && linha.Contains(" - Url web service"))
                {
                    var lastHttps = linha.LastIndexOf("https");
                    registroReferencia.UrlWebService = linha.Substring(111,74);
                    //add o registro na lista de registros e zera o registro
                    
                }

                if (linha.Contains("<distDFeInt"))
                {
                    registroReferencia.UltNSUEnvio = GetXmlTagValue(linha, "ultNSU");
                    if(registroReferencia.UltNSUEnvio == null)
                    {
                        registroReferencia.UltNSUEnvio = "Registro por chave " + GetXmlTagValue(linha, "chNFe");
                    }
                    registroReferencia.CNPJ = GetXmlTagValue(linha, "CNPJ");

                    registroReferencia.XmlEnvio = linha;
                    registroReferencia.NumeroLinhaEnvio = indexLinha;
                }

                if (linha.Contains("<retDistDFeInt"))
                {
                    
                    registroReferencia.XmlEnvio = linha;
                    registroReferencia.CStatRetorno = GetXmlTagValue(linha,"cStat");
                    registroReferencia.XMotivoRetorno = GetXmlTagValue(linha, "xMotivo");
                    registroReferencia.MaxNSURetorno = GetXmlTagValue(linha, "maxNSU");
                    registroReferencia.UltNSURetorno = GetXmlTagValue(linha, "ultNSU");
                    registroReferencia.NumeroLinhaRetorno = indexLinha;
                }
            }

            using(StreamWriter swConvertedFile = new StreamWriter(Path.Combine(Environment.CurrentDirectory,"ConvertedFile.csv")))
            {
                swConvertedFile.WriteLine("NumeroLinhaEnvio;NumeroLinhaRetorno;CNPJ;Horario;CStatRetorno;XMotivoRetorno;UltNSUEnvio;UltNSURetorno;MaxNSURetorno");
                var objetosConvertidosAgrupadosPorCnpj = linhasConvertidas.GroupBy(x => x.CNPJ);
                foreach (var itemEmpresa in objetosConvertidosAgrupadosPorCnpj)
                {
                    swConvertedFile.WriteLine("CNPJ: " + itemEmpresa.Key);
                    //swConvertedFile.WriteLine(item.NumeroLinhaEnvio + ";" + item.NumeroLinhaRetorno +
                    //    ";" + item.CNPJ + ";" + item.Horario.Value.ToShortDateString() + " - " + item.Horario.Value.ToShortTimeString()
                    //    + ";" + item.CStatRetorno + ";" + item.XMotivoRetorno + ";" + item.UltNSUEnvio + ";" + item.UltNSURetorno + ";" + item.MaxNSURetorno);
                    foreach (var item in itemEmpresa)
                    {
                        swConvertedFile.WriteLine(item.NumeroLinhaEnvio + ";" + item.NumeroLinhaRetorno +
                        ";" + item.CNPJ + ";" + item.Horario.Value.ToShortDateString() + " - " + item.Horario.Value.ToShortTimeString()
                        + ";" + item.CStatRetorno + ";" + item.XMotivoRetorno + ";" + item.UltNSUEnvio + ";" + item.UltNSURetorno + ";" + item.MaxNSURetorno);
                    }
                    
                }
            }
            
         
        }

        public static string RemoveInvalidChars(string strSource)
        {
            return Regex.Replace(strSource, @"[^0-9a-zA-Z=+/]", "");
        }

        public static string GetXmlTagValue(string xml, string tag)
        {
            var indexTag = xml.IndexOf("<" + tag + ">");

            if(indexTag < 0)
            {
                return null;
            }

            var indexTagValue = indexTag + tag.Length+2;
            var indexFechamentoTag = xml.IndexOf("</" + tag + ">");
            var valueLength = indexFechamentoTag - indexTagValue;
            var value = xml.Substring(indexTagValue, valueLength);
            return value;
        }
        public static void RegistrarLog(string info)
        {
            var lineInfo = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " - " + info;
            Console.WriteLine(lineInfo);
            logWriter.WriteLine(lineInfo);
            
        }
    }
}
