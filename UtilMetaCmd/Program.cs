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
using System.Reflection;

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
            DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
            Console.WriteLine("***************************************************************************************");
            Console.WriteLine("UtilMetaCMD - " + buildDate.ToString("dd/MM/yyyy HH:mm:SS"));
            Console.WriteLine("1 - copiar os arquivos a serem manipulados numa subpasta junto a este aplicativo.");
            Console.WriteLine("2 - rodar a linha de comando no diretório desde exe para executar a carga de trabalho necessária");
            Console.WriteLine("3 - exemplo de como utilizar no gif na pasta deste mesmo app");
            Console.WriteLine("Comandos: ");
            Console.WriteLine("1 - Formato de Entrada: > UtilMetaCmd manifesto-to-csv c:\\logmanifesto.txt - converte o log do manifesto na planilha formato csv estruturado (saída ConvertedFile.csv na pasta dos xmls)");
            Console.WriteLine("2 - Formato de Entrada: > UtilMetaCmd armazenamentodfe-from-folder c:\\pastaxmls\\ - gera comando de inserção de xmls na armazenamentodfe a partir da pasta com os xmls das notas. (saída arquivo scriptArmazenamentoDfe-GUID.sql na pasta dos xmls)");


            //var txtFileLogManifesto = @"C:\Program Files (x86)\Meta.Net\TK335505\AbelGalinha-LogMetaServerGlobal_GerenciadorNotaFiscalManifestacao25072022.log";
            if (args.Length == 0)
                Console.ReadLine();

            var action = args[0];
            var parametro = args[1];
            //https://i.imgur.com/MELu7LL.png

            //var action = "armazenamentodfe-from-folder";
            //var parametro = @"E:\transient\TK363002\OUTUBRO 2022\";

            switch (action)
            {
                case "manifesto-to-csv":
                    ConversaoLogManifestoCsv(parametro);
                    break;
                case "armazenamentodfe-from-folder":
                    CriarComandosArmazenamentoDFePorPasta(parametro);
                    break;
                default:
                    break;
            }
            
        }

        private static void CriarComandosArmazenamentoDFePorPasta(string parametro)
        {
            var script = @"DECLARE @idDocumento bigint = 0;
                                DECLARE @chaveDocumento varchar(44) = '';";

            var filesXml = Directory.GetFiles(parametro, "*.xml");
            foreach (var xml in filesXml)
            {
                var xmlContent = new StreamReader(xml).ReadToEnd();

                var chave = GetXmlTagValue(xmlContent, "chNFe");
                var chaveMDFe = GetXmlTagValue(xmlContent, "chMDFe");
                var isMDFe = xmlContent.Contains("<mdfeProc");

                if (isMDFe) chave = chaveMDFe;

                var nomeTabelaChave = !isMDFe ? "FAT_NotaFiscal" : "FAT_ManifestoEletronicoDocumentoFiscal";

                var documentScript = @"SET @chaveDocumento='"+chave+@"'
set @idDocumento = (SELECT top 1 Id FROM " + nomeTabelaChave + @" where ChaveAcesso=@chaveDocumento);
SELECT @idDocumento
INSERT INTO [Fiscal.DFe].[ArmazenamentoDFe]
           ([IdEmpresa],[IdUltimoUsuario],[IdDocumentoFiscal],[IdDestinatario],[TipoDocumento],[ModeloDocumento],[StatusDocumento],[ChaveAcesso],[Serie],[Numero],[DataMovimentacao]
           ,[ValorDocumento] ,[XmlProcessado], [Excecao])
		   SELECT DOC.IdEmpresa, null ,DOC.Id ,DESTINATARIO.IdParceiroNegocio ,DOC.TipoDocumento ,DOC.Modelo ,doc.Status ,NOTA.ChaveAcesso ,DOC.Serie ,DOC.Numero
           ,DOC.DataMovimentacao ,DOC.Total 
		   ,'"+xmlContent+@"'
           ,null
		   FROM FAT_DocumentoFiscal DOC
		   INNER JOIN FAT_DocumentoFiscalDadosDestinatario DESTINATARIO ON DESTINATARIO.Id=DOC.Id
		   INNER JOIN FAT_NotaFiscal NOTA ON NOTA.Id = DOC.Id
		   WHERE DOC.Id = @idDocumento;
PRINT 'Inserido documento na fila armazenamentodfe para chave ' + @chaveDocumento
";
                script += documentScript;
            }

            StreamWriter sr = new StreamWriter(Path.Combine(parametro, "scriptArmazenamentoDfe-" + Guid.NewGuid() + ".sql"));
            sr.Write(script);
            sr.Close();

        }

        private static void ConversaoLogManifestoCsv(string txtFileLogManifesto)
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
                RegistrarLog("Inicializando parse da linha " + indexLinha);

                if (linha.Conteudo.Contains("ConsumirServicoEspecifico") && linha.Conteudo.Contains(" - Início"))
                {
                    registroReferencia = new LogManifestoEntradaItemDto();
                    var dataString = linhasArquivo[indexLinha - 1].Conteudo.Substring(11, 19);
                    DateTime parsedDate = DateTime.ParseExact(dataString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    registroReferencia.Horario = parsedDate;
                    registroReferencia = new LogManifestoEntradaItemDto();
                    RegistrarLog("Inicializando registro linha " + indexLinha);
                }

                if (linha.Conteudo.Contains("Data/Hora"))
                {
                    if (registroReferencia == null)
                    {
                        RegistrarLog("Não inicializado registro para a linha  " + indexLinha);
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
                    RegistrarLog("Continuando porque registro não está inicializado " + indexLinha);
                    continue;
                }

                if (linha.Conteudo.Contains("ConsumirServicoEspecifico") && linha.Conteudo.Contains(" - Fim"))
                {
                    linhasConvertidas.Add(registroReferencia);
                    registroReferencia = null;
                    RegistrarLog("Finalizando registro linha " + indexLinha);
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
                    registroReferencia.UltNSUEnvio = GetXmlTagValue(linha.Conteudo, "ultNSU");
                    if (registroReferencia.UltNSUEnvio == null)
                    {
                        registroReferencia.UltNSUEnvio = "Registro por chave " + GetXmlTagValue(linha.Conteudo, "chNFe");
                    }
                    registroReferencia.CNPJ = GetXmlTagValue(linha.Conteudo, "CNPJ");

                    registroReferencia.XmlEnvio = linha.Conteudo;
                    registroReferencia.NumeroLinhaEnvio = indexLinha;
                }

                if (linha.Conteudo.Contains("<retDistDFeInt"))
                {

                    registroReferencia.XmlEnvio = linha.Conteudo;
                    registroReferencia.CStatRetorno = GetXmlTagValue(linha.Conteudo, "cStat");
                    registroReferencia.XMotivoRetorno = GetXmlTagValue(linha.Conteudo, "xMotivo");
                    registroReferencia.MaxNSURetorno = GetXmlTagValue(linha.Conteudo, "maxNSU");
                    registroReferencia.UltNSURetorno = GetXmlTagValue(linha.Conteudo, "ultNSU");
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
            var lineInfo = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " - " + info;
            Console.WriteLine(lineInfo);
            logWriter.WriteLine(lineInfo);
            
        }
    }
}
