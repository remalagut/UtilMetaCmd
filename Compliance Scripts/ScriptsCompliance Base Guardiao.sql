--compliance base guardiao
--Identificar Idempresa
select 
	emp.DataImplantacao AS [Data Implantacao],
	emp.Id AS [IdEmpresa],
	coalesce(p.ApelidoFantasia,'') AS [Nome Fantasia],
	coalesce(p.NomeRazao,'') AS [Razao],
	coalesce(p.CpfCnpj,'') AS [CNPJ],
	coalesce(p.RgInscricaoEstadual,'') AS [IE],
	coalesce(c.Nome,'') AS [Cidade],
	coalesce(e.Sigla,'') AS [Estado]
FROM  acs_empresa emp
	INNER JOIN CAD_ParceiroNegocio p on p.Id = emp.Id
	INNER JOIN cad_cidade c on c.id = p.IdCidade
	INNER JOIN CAD_Estado e on e.tipoestado = c.Estado
WHERE 
	emp.Id NOT IN (102,130108)
ORDER BY p.NomeRazao

--Identificar Mercadorias 
DECLARE @IDEMPRESA BIGINT= 0; /*<-- Informar ID empresa*/
SELECT
	DISTINCT
	e.Sigla as Estado,
	mercadoria.AtualizarComplianceFiscal,
	mercadoriaEmpresa.Auditado,
	mercadoriaEmpresa.EanValido,
	CONVERT(VARCHAR(max), mercadoria.DataCadastro, 103) AS DataCadastroMercadoria,
	CONVERT(VARCHAR(max), mercadoriaEmpresa.DataAuditoria, 103) AS DataAuditoria,
	Subgrupo.Nome as Subgrupo,
	mercadoria.codigobarra as CodigoBarra,
	','+''''+mercadoria.codigobarra+'''' as CodigoBarra2,
	mercadoria.Descricao as DescricaoResumidaMercadoria,
	mercadoria.descricaocompleta as DescricaoCompletaMercadoria,
	--NCM
	COALESCE(mercadoria.classificacaoFiscalNCm, '')  AS [NCM],
	--Seguimento CEST
	COALESCE(mercadoria.SeguimentoCest, '')  AS [Seguimento CEST],
	--CEST
	COALESCE(mercadoria.Cest, '')  AS [CEST],
	-- Unidade Venda
	un.Sigla as UnidadeVenda,
	-- Unidade Tributacal
	un2.Sigla as UnidadeTributaval,
	--Cód.ANP e Descricao
	CASE 
        WHEN convert(varchar(max),meranp.codigo) is null THEN '' ELSE CAST(meranp.codigo as varchar(max)) + ' - ' + CAST(meranp.Descricao as varchar(max))
    END as CÓDIGO_DESCRICAO_ANP,
	--CFOP Compra Mercadoria
	CASE 
        WHEN mercadoriaEmpresa.CodigoFiscalEntrada is null THEN '' ELSE (SELECT Codigo FROM FAT_CodigoFiscal WHERE TipoCodigoFiscal = mercadoriaEmpresa.CodigoFiscalEntrada)
    END as CFOPCompra_Mercadoria,
	--CFOP Venda Mercadoria
	CASE 
        WHEN mercadoriaEmpresa.CodigoFiscal is null THEN '' ELSE (SELECT Codigo FROM FAT_CodigoFiscal WHERE TipoCodigoFiscal = mercadoriaEmpresa.CodigoFiscal)
    END as CFOPVenda_Mercadoria,
	
	/***   Regime  Lucro Real e Presumido   ***/
	--CST/CSOSN de ICMS Mercadoria (lucro real e presumido)	    
    mercadoriaEmpresa.IdImpostoICMS
	,CASE 
        WHEN mercadoriaEmpresa.IdImpostoICMS is null THEN '' ELSE (SELECT top 1 cast(impicms.SituacaoTributaria as varchar(max)) + ' - ' + cast(imp.Descricao as varchar(max)) FROM FIS_Imposto imp join FIS_ImpostoICMS impicms on impicms.id = imp.id WHERE imp.id = mercadoriaEmpresa.IdImpostoICMS)
    END as CSOSN_OU_CST_ICMS_Mercadoria_LucroRealPresumido,
	--CST de PIS/COFINS Mercadoria (lucro real e presumido)
	mercadoriaEmpresa.IdImpostoPISCOFINS
	,CASE 
        WHEN mercadoriaEmpresa.IdImpostoPISCOFINS is null THEN '' ELSE (SELECT top 1 cast(impPIS.SituacaoTributariaEntrada as varchar(max)) + ' - Entrada / ' + cast(impPIS.SituacaoTributariaSaida as varchar(max)) + ' - Saída ' + cast(imp.Descricao as varchar) FROM FIS_Imposto imp join FIS_ImpostoPIS impPIS on impPIS.id = imp.id WHERE imp.id = mercadoriaEmpresa.IdImpostoPISCOFINS)
    END as CSTPISCOFINS_Mercadoria_LucroRealPresumido,

	/***   Regime  Simples Nacional   ***/
	--CST/CSOSN de ICMS Mercadoria (Simples Nacional)
    CASE 
        WHEN mercadoriaEmpresa.IdImpostoICMSSimplesNacional is null THEN '' ELSE (SELECT top 1 cast(impicms.SituacaoTributaria as varchar(max)) + ' - ' + cast(imp.Descricao as varchar(max)) FROM FIS_Imposto imp join FIS_ImpostoICMS impicms on impicms.id = imp.id WHERE imp.id = mercadoriaEmpresa.IdImpostoICMSSimplesNacional)
    END as CSOSN_OU_CST_ICMS_Mercadoria_SimplesNacional,
	--CST de PIS/COFINS Mercadoria (Simples Nacional)
	CASE 
        WHEN mercadoriaEmpresa.IdImpostoPISCOFINSSimplesNacional is null THEN '' ELSE (SELECT top 1 cast(impPIS.SituacaoTributariaEntrada as varchar(max)) + ' - Entrada / ' + cast(impPIS.SituacaoTributariaSaida as varchar(max)) + ' - Saída ' + cast(imp.Descricao as varchar) FROM FIS_Imposto imp join FIS_ImpostoPIS impPIS on impPIS.id = imp.id WHERE imp.id = mercadoriaEmpresa.IdImpostoPISCOFINSSimplesNacional)
    END as CSTPISCOFINS_Mercadoria_SimplesNacional,

	--Natureza Receita de PIS/COFINS Mercadoria
	CASE 
        WHEN mercadoriaEmpresa.IdNaturezaReceitaNaoTributadaPisCofins is null THEN '' ELSE (SELECT top 1 cast(nat.codigo as varchar(max)) + ' - ' + cast(nat.descricao as varchar(max)) FROM FIS_EscritaFiscalDigitalContribuicoesNaturezaReceitaNaoTributada nat WHERE nat.id = mercadoriaEmpresa.IdNaturezaReceitaNaoTributadaPisCofins)
    END as NAT_RECEITA_PISCOFINS_Mercadoria
from CAD_Mercadoria mercadoria WITH (NOLOCK)
INNER JOIN CAD_MercadoriaDadosEmpresa mercadoriaEmpresa  WITH (NOLOCK) on mercadoria.ID= mercadoriaEmpresa.IdMercadoria and mercadoriaEmpresa.IdEmpresa = @IDEMPRESA
left join FIS_ImpostoCOFINS ICOFINS WITH (NOLOCK) ON mercadoriaEmpresa.ImpostoPISCOFINSResolvidoNoGrupoSubGrupoParaTributarMercadoria = ICOFINS.ID
INNER JOIN acs_empresa emp WITH (NOLOCK) on emp.id = mercadoriaEmpresa.IdEmpresa
INNER JOIN CAD_ParceiroNegocio p WITH (NOLOCK) on p.Id = emp.Id
INNER JOIN cad_cidade c WITH (NOLOCK) on c.id = p.IdCidade
INNER JOIN CAD_Estado e WITH (NOLOCK) on e.tipoestado = c.Estado
LEFT JOIN CAD_UnidadeVenda un WITH (NOLOCK) on un.id = mercadoria.IdUnidadeVenda
LEFT JOIN CAD_UnidadeVenda un2 WITH (NOLOCK) on un2.id = mercadoriaEmpresa.UnidadeTributavel
LEFT JOIN CAD_MercadoriaEstocavel merest  WITH (NOLOCK) on mercadoria.Id = merest.Id
LEFT JOIN PST_TabelaAnp meranp  WITH (NOLOCK) on merest.IdTabelaAnp = meranp.Id
LEFT JOIN CAD_SubGrupoMercadoria Subgrupo  WITH (NOLOCK) on Subgrupo.Id = mercadoria.IdSubGrupoMercadoria
LEFT JOIN CAD_SubGrupoMercadoriaDadosEmpresa SubgrupoEmpresa WITH (NOLOCK) on Subgrupo.Id = SubgrupoEmpresa.IdSubGrupoMercadoria and SubgrupoEmpresa.IdEmpresa = @IDEMPRESA
LEFT JOIN CAD_GrupoMercadoria Grupo  WITH (NOLOCK) on Grupo.Id = Subgrupo.IdGrupoMercadoria
LEFT JOIN CAD_GrupoMercadoriaDadosEmpresa GrupoEmpresa WITH (NOLOCK) on  GrupoEmpresa.IdGrupoMercadoria  = Grupo.Id and GrupoEmpresa.IdEmpresa = @IDEMPRESA
WHERE mercadoriaEmpresa.EanValido = 0
--AND mercadoriaEmpresa.Auditado = 0  
--AND sub.Id <> 1525 /* ID do Subgrupo CODIGO DE BARRAS INVALIDOS */
-- DESCOMENTAR AS CONDIÇÕES ABAIXO CASO QUEIRA VALIDAR MERCADORIAS SEM INFORMAÇÃO DE NAT.RECEITA DE PIS/COFINS
--AND ICOFINS.SituacaoTributariaSaida <> 1 -- TODAS QUE NÃO FOREM ALIQUOTA BÁSICA
--AND MDE.NaturezaReceitaNaoTributadaPisCofinsResolvidoNoGrupoSubGrupo IS NULL
and mercadoria.codigobarra in
(
'0'
)ORDER BY mercadoria.codigobarra