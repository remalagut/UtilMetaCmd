# UtilMetaCmd

 > UtilMetaCmd manifesto-to-csv c:\\logmanifesto.txt - converte o log do manifesto na planilha formato csv estruturado (saída ConvertedFile.csv na pasta dos xmls)");
 > UtilMetaCmd armazenamentodfe-from-folder c:\\pastaxmls\\ - gera comando de inserção de xmls na armazenamentodfe a partir da pasta com os xmls das notas. (saída arquivo scriptArmazenamentoDfe-GUID.sql na pasta dos xmls)");
 > UtilMetaCmd chave 35210578901234567890123456789012345678901234 - decompoe a chave da nfe e printa cada componente da chave separadamente em tela");
 > UtilMetaCmd xml-split c:\\arquivo_xml_contendo_1xml_por_linha.txt - separa um arquivo de texto que tem um xml por linha descarregando um arquivo por documento nomeado pela chave (forma facil de pegar os xmls é dando select no SSMS, selecionando a coluna de xml e copiando, ou exportando do mongodb pelo export data), OBS: Xmls devem estar exportados sem aspas duplicadas"

![como-utilizar-utilmetacmd-logmanifesto](https://user-images.githubusercontent.com/91275523/181038885-5605b789-f973-460a-992c-82d6f193e236.gif)
