using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilMetaCmd.Extensions;

namespace UtilMetaCmd.ArmazenamentoDfeFromFolder.XmlArmazenamentoDfeDoc
{
    public abstract class DfeScriptCreator
    {
        private string xmlContent;

        public abstract string GetDFeInsert(string xml);
        public abstract string GetChaveDFe(string xml);

        public static DfeScriptCreator GetInstance(string xml)
        {
            DfeScriptCreator dfeScriptCreator = null;

            var modelo = xml.GetXmlTagValue("mod");
            switch (modelo)
            {
                case "57":
                    dfeScriptCreator = new DFeScriptCreatorCTe();
                    break;
                case "55":
                case "65":
                    dfeScriptCreator = new DFeScriptCreatorNFe();
                    break;
                case "58":
                    dfeScriptCreator = new DFeScriptCreatorMDFe();
                    break;
                case "59":
                    dfeScriptCreator = new DFeScriptCreatorSATCFe();
                    break;
                default:
                    Console.WriteLine("Documento de modelo não implementado será ignorado: " + xml);
                    break;
            }

            dfeScriptCreator.xmlContent = xml;

            return dfeScriptCreator;
        }
    }
}
