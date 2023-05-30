using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilMetaCmd.Model;
using UtilMetaCmd.Extensions;
using UtilMetaCmd.ArmazenamentoDfeFromFolder.XmlArmazenamentoDfeDoc;
using System.Diagnostics;

namespace UtilMetaCmd.ArmazenamentoDfeFromFolder
{
    public class ArmazenamentoDFeFromFolder
    {
        public static void CriarComandosArmazenamentoDFePorPasta(string urlPastaContendoXmls)
        {
            var script = @"DECLARE @idDocumento bigint = 0;
                                DECLARE @chaveDocumento varchar(44) = '';";

            var filesXml = Directory.GetFiles(urlPastaContendoXmls, "*.xml");
            foreach (var xml in filesXml)
            {
                var xmlContent = new StreamReader(xml).ReadToEnd();
                var modelo = xmlContent.GetXmlTagValue("mod");

                DfeScriptCreator dfeScriptCreator = DfeScriptCreator.GetInstance(xmlContent);

                if(dfeScriptCreator == null)
                {
                    Console.WriteLine("dfeScriptCreator não inicializado (modelo não encontrado), seguindo para próximo documento");
                    continue;
                }

                string documentoScript = dfeScriptCreator.GetDFeInsert(xmlContent);
                script += documentoScript;
            }
            var dateNow = DateTime.Now.ToString("yyyyMMddHHmmss");
            var pathArquivoInsert = Path.Combine(urlPastaContendoXmls, "scriptArmazenamentoDfe-" + dateNow + ".sql");
            StreamWriter sr = new StreamWriter(pathArquivoInsert);
            sr.Write(script);
            sr.Close();

            Console.WriteLine("Arquivo de script armazenamento DFe gerado com sucesso em " + pathArquivoInsert);
            Console.WriteLine("Deseja abrir o arquivo gerado? Digite S/N e tecle ENTER");
            var key = Console.ReadKey();
            if(key.Key == ConsoleKey.S)
            {
                Process.Start(pathArquivoInsert);
            }

        }
    }
}
