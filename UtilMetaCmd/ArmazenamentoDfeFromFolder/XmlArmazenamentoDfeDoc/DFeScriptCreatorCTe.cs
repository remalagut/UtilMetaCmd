using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilMetaCmd.Extensions;

namespace UtilMetaCmd.ArmazenamentoDfeFromFolder.XmlArmazenamentoDfeDoc
{
    public class DFeScriptCreatorCTe : DfeScriptCreator
    {
        
        override public string GetDFeInsert(string xml)
        {
            var chave = xml.GetXmlTagValue("chCTe");
            var documentScript = @"SET @chaveDocumento='" + chave + @"'
set @idDocumento = (SELECT top 1 Id FROM FAT_ConhecimentoTransporteEletronico where ChaveAcesso=@chaveDocumento);
SELECT @idDocumento
INSERT INTO [Fiscal.DFe].[ArmazenamentoDFe]
           ([IdEmpresa],[IdUltimoUsuario],[IdDocumentoFiscal],[IdDestinatario],[TipoDocumento],[ModeloDocumento],[StatusDocumento],[ChaveAcesso],[Serie],[Numero],[DataMovimentacao]
           ,[ValorDocumento] ,[XmlProcessado], [Excecao])
		   SELECT DOC.IdEmpresa, null ,DOC.Id ,DESTINATARIO.IdParceiroNegocio ,DOC.TipoDocumento ,DOC.Modelo ,doc.Status ,CTE.ChaveAcesso ,DOC.Serie ,DOC.Numero
           ,DOC.DataMovimentacao ,DOC.Total 
		   ,'" + xml + @"'
           ,null
		   FROM FAT_DocumentoFiscal DOC
		   INNER JOIN FAT_DocumentoFiscalDadosDestinatario DESTINATARIO ON DESTINATARIO.Id=DOC.Id
		   --LEFT JOIN FAT_NotaFiscal NOTA ON NOTA.Id = DOC.Id
           LEFT JOIN FAT_ConhecimentoTransporteEletronico CTE ON CTE.Id=DOC.Id
		   WHERE DOC.Id = @idDocumento;";
            Log.AppLog.RegistrarLog("Gerado comando para o documento CTe chave " + chave);
            //PRINT 'Inserido documento na fila armazenamentodfe para chave ' + @chaveDocumento
            return documentScript;
        }
    }
}
