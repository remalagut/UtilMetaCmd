﻿using System;
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
using UtilMetaCmd.Extensions;

namespace UtilMetaCmd
{
    class Program
    {
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
            Console.WriteLine("3 - Formato de entrada: > UtilMetaCmd chave 35210578901234567890123456789012345678901234 - decompoe a chave da nfe e printa cada componente da chave separadamente em tela");
            Console.WriteLine("4 - Formato de Entrada: > UtilMetaCmd xml-split c:\\arquivo_xml_contendo_1xml_por_linha.txt - separa um arquivo de texto que tem um xml por linha descarregando um arquivo por documento nomeado pela chave (forma facil de pegar os xmls é dando select no SSMS, selecionando a coluna de xml e copiando, ou exportando do mongodb pelo export data)");
            Console.WriteLine("4 - Formato de Entrada: > UtilMetaCmd.exe sped-to-sql \".\\spedtest\\AUTO POSTO IRMAOS ROTTAVA LTDA-1-SPED-FISCAL202308.TXT\" - gera os scripts create table e insert a partir de uma EFD");

            //var txtFileLogManifesto = @"C:\Program Files (x86)\Meta.Net\TK335505\AbelGalinha-LogMetaServerGlobal_GerenciadorNotaFiscalManifestacao25072022.log";
            if (args.Length < 2)
                Console.ReadLine();

            var action = args[0];
            var parametro = args.Length > 1 ? args[1] : null;
            //https://i.imgur.com/MELu7LL.png

            //var action = "armazenamentodfe-from-folder";
            //var parametro = @"E:\transient\TK363002\OUTUBRO 2022\";

            parametro = @"D:\transient\11092023\AUTO POSTO IRMAOS ROTTAVA LTDA-1-SPED-FISCAL202308.TXT";
            switch (action)
            {
                case "manifesto-to-csv":
                    ManifestoToCsv.ManifestoToCsv.ConversaoLogManifestoCsv(parametro);
                    break;
                case "armazenamentodfe-from-folder":
                    ArmazenamentoDfeFromFolder.ArmazenamentoDFeFromFolder.CriarComandosArmazenamentoDFePorPasta(parametro);
                    break;
                case "chave":
                    UtilMetaCmd.ChaveNfeSplitter.ChaveNfeSplitter.SplitChaveNfe(parametro);
                    break;
                case "xml-split":
                    UtilMetaCmd.XmlTextSplitter.XmlTextSplitter.Split(parametro);
                    break;
                case "sped-to-sql":
                    UtilMetaCmd.SPEDToSql.SpedToSql.ConverterSpedParaSql(parametro);
                    break;
                default:
                    break;
            }
        }
    }
}
