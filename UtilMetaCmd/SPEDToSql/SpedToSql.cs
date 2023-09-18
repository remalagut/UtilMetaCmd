using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilMetaCmd.SPEDToSql
{
    public class SpedToSql
    {
        public static void ConverterSpedParaSql(string pathArquivoEfd)
        {
            //inicializa lista chave-valor, a lista conterá a chave igual ao numero do registro e o valor igual ao comando create
            //alternativa utilizada para evitar que seja adicionado mais de um create para o mesmo bloco encontrado em sped
            var createTableCommands = new Dictionary<string,string>();
            //lista de insert commands chave-valor, onde chave é o numero da linha e valor é o comando insert gerado para o item
            var insertCommands = new Dictionary<int, string>();

            using (var sr = new StreamReader(pathArquivoEfd, Encoding.Default, true))
            {
                var listaRegistros = new List<string>();
                int linha = 0;

                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();
                    linha++;
                    var valoresLinha = currentLine.Split('|');
                    var tipoRegistro = valoresLinha[1];
                    //lista de campos a partir do objeto da linha, nomeado inicialmente como "Campo2,Campo3", conforme definição do campo em layout sped
                    var listaCamposParaInsercao = new Dictionary<string, string>();

                    for (int i = 2; i < valoresLinha.Length - 1; i++)
                    {
                        var valorAtual = valoresLinha[i];
                        listaCamposParaInsercao.Add("Campo" + (i).ToString(), valorAtual);
                        
                    }

                    var comandoCreateTable = GerarComandoCreate(tipoRegistro,listaCamposParaInsercao);
                    if(!createTableCommands.Any(x => x.Key==tipoRegistro))
                        createTableCommands.Add(tipoRegistro, comandoCreateTable);

                    var comandoInsercao = GerarComandoInsercao(tipoRegistro,listaCamposParaInsercao);
                    insertCommands.Add(linha,comandoInsercao);
                }
            }


            var pastaArquivoEFD = Path.GetDirectoryName(pathArquivoEfd);
            var nomeArquivoEFD = Path.GetFileName(pathArquivoEfd);
            var nomeArquivoCreate = nomeArquivoEFD.ToUpper().Replace(".TXT", "")+"-CreateCommands-" + DateTime.Now.ToString("yyyyMMddHHmmss")  + ".sql";
            var nomeArquivoInsert = nomeArquivoEFD.ToUpper().Replace(".TXT", "") + "-InsertCommands-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".sql";

            using (var sw = new StreamWriter(Path.Combine(pastaArquivoEFD,nomeArquivoCreate)))
            {
                foreach (var itemLinhaCreate in createTableCommands)
                    sw.WriteLine(itemLinhaCreate.Value);
            }
            using (var sw = new StreamWriter(Path.Combine(pastaArquivoEFD, nomeArquivoInsert)))
            {
                foreach (var itemLinhaCreate in insertCommands)
                    sw.WriteLine(itemLinhaCreate.Value);
            }

        }

        private static string GerarComandoInsercao(string tipoRegistro,Dictionary<string, string> listaCamposParaInsercao)
        {
            var sb = new StringBuilder();
            sb.Append("INSERT INTO Registro" + tipoRegistro + " (");
            //string comandoInsercao = $"INSERT INTO {tipoRegistro} ";

            foreach (var itemCampoValor in listaCamposParaInsercao)
            {
                sb.Append(itemCampoValor.Key + ",");
            }
            //remove a virgula do ultimo campo
            sb.Remove(sb.Length - 1, 1);

            sb.Append(") VALUES (");

            foreach(var itemCampoValor in listaCamposParaInsercao)
            {
                sb.Append("'" + itemCampoValor.Value + "',");
            }
            //remove a virgula do ultimo campo
            sb.Remove(sb.Length - 1, 1);

            sb.Append(");");

            var comandoInsercao = sb.ToString();
            return comandoInsercao;

        }

        private static string GerarComandoCreate(string registro, Dictionary<string, string> listaCamposParaInsercao)
        {
            var sb = new StringBuilder();
            sb.Append("CREATE TABLE Registro" + registro + "(");

            foreach(var itemCampoValor in listaCamposParaInsercao)
            {
                sb.Append(itemCampoValor.Key + " VARCHAR(MAX),");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(");");


            var createCommand = sb.ToString();
            return createCommand;
        }
    }
}
