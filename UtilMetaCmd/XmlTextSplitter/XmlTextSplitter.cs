using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilMetaCmd.ArmazenamentoDfeFromFolder.XmlArmazenamentoDfeDoc;

namespace UtilMetaCmd.XmlTextSplitter
{
    public class XmlTextSplitter
    {
        public static void Split(string arquivoContendoUmXmlDFePorLinha)
        {
            var pathEnvironment = Path.Combine(Environment.CurrentDirectory, "XmlSplit-"+DateTime.Now.ToString("yyyyMMddHHmmss"));

            if (!Directory.Exists(pathEnvironment))
            {
                Directory.CreateDirectory(pathEnvironment);
            }

            using (StreamReader sr = new StreamReader(arquivoContendoUmXmlDFePorLinha))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var dfeScriptCreator = DfeScriptCreator.GetInstance(line);

                    var fileOutputXml = Path.Combine(pathEnvironment, dfeScriptCreator.GetChaveDFe(line)) + ".xml";
                    File.WriteAllText(fileOutputXml, line);

                }
            }
        }
    }
}
