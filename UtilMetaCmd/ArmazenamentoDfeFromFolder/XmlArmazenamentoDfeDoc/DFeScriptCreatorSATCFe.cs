using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilMetaCmd.Extensions;

namespace UtilMetaCmd.ArmazenamentoDfeFromFolder.XmlArmazenamentoDfeDoc
{
    public class DFeScriptCreatorSATCFe : DfeScriptCreator
    {
        
        override public string GetDFeInsert(string xml)
        {
            var chave = GetChaveDFe(xml);
            var documentScript = @"SET @chaveDocumento='" + chave + @"'
set @idDocumento = (SELECT top 1 Id FROM PDV_SATFiscal where ChaveAcesso=@chaveDocumento);
SELECT @idDocumento
INSERT INTO [Fiscal.DFe].[ArmazenamentoDFe]
           ([IdEmpresa],[IdUltimoUsuario],[IdDocumentoFiscal],[IdDestinatario],[TipoDocumento],[ModeloDocumento],[StatusDocumento],[ChaveAcesso],[Serie],[Numero],[DataMovimentacao]
           ,[ValorDocumento] ,[XmlProcessado], [Excecao])
		   SELECT DOC.IdEmpresa, null ,DOC.Id ,DESTINATARIO.IdParceiroNegocio ,DOC.TipoDocumento ,DOC.Modelo ,doc.Status ,SAT.ChaveConsulta,DOC.Serie ,DOC.Numero
           ,DOC.DataMovimentacao ,DOC.Total 
		   ,'" + xml + @"'
           ,null
		   FROM FAT_DocumentoFiscal DOC
           INNER JOIN PDV_SATFiscal SAT ON SAT.Id=DOC.Id
		   LEFT JOIN FAT_DocumentoFiscalDadosDestinatario DESTINATARIO ON DESTINATARIO.Id=DOC.Id
		   WHERE DOC.Id = @idDocumento;";
            Log.AppLog.RegistrarLog("Gerado comando para o documento SAT chave " + chave);
            return documentScript;
        }

        public override string GetChaveDFe(string xml)
        {
            //var chave = xml.GetXmlTagValue("chCTe");
            var indexChave = xml.IndexOf("Id=\"CFe");
            indexChave += 4;
            var chave = xml.Substring(indexChave, 47);
            return chave;
        }
    }
}
