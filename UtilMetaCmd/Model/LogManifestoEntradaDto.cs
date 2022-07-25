using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilMetaCmd.Model
{
    public class LinhaDocumento
    {
        public int NumeroLinha { get; set; }
        public string Conteudo { get; set; }
    }

    public class LogManifestoEntradaItemDto
    {
        public int NumeroLinhaEnvio { get; set; }
        public int NumeroLinhaRetorno { get; set; }

        //NumeroLinhaEnvio,NumeroLinhaRetorno,CNPJ,InscricaoEstadual,Horario,CStatRetorno,XMotivoRetorno,UltNSUEnvio,UltNSURetorno,MaxNSURetorno,UrlWebService,XmlEnvio,XmlRetorno
        public string CNPJ { get; set; }
        public string InscricaoEstadual { get; set; }
        public DateTime? Horario { get; set; }
        public string CStatRetorno { get; set; }
        public string XMotivoRetorno { get; set; }
        public string UltNSUEnvio { get; set; }
        public string UltNSURetorno { get; set; }
        public string MaxNSURetorno { get; set; }


        public string UrlWebService { get; set; }
        public string XmlEnvio { get; set; }
        public string XmlRetorno { get; set; }
        
        
    }
}
