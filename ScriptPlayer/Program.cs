using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using System.Xml;
using System.Data;

namespace ScriptPlayer
{
    class Program
    {

        //TODO
        //Пофиксить с xml - файлом. Сделать List<string> Files

        //логи выводятся в папку с проектом рядом с app.config
        private static string _path;
        private static int _errors = 0;
        private static List<string> notFound = new List<string>();
        static void Main(string[] args)
        {
            _path = AppDomain.CurrentDomain.BaseDirectory + @"..\" + @"..\";//путь до App.config
            string connectionString = Settings.Default.connectionString;
            string filePath = Settings.Default.path;

            var filesList = Settings.Default.files.Files;
            List<string> filesConfig = new List<string>();
            foreach (var _node in filesList)
                filesConfig.Add(_node.file.Trim());

            Console.WriteLine("{0} file(s) to execute", filesList.Count);
            foreach (var fileName in filesConfig)
                executeScript(fileName, filePath + @"\", connectionString);
            Console.WriteLine("Completed");
            Console.WriteLine("{0} error(s)", _errors);
            //Console.WriteLine("Not executed {0} file(s)", notFound.Count);
            Console.ReadLine();
        }

        private static void executeScript(string fileName, string Path, string connectionString)
        {
            string filePath = Path + fileName;
            if (File.Exists(filePath))
            {
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(connectionString))
                    {
                        string sqlExpression = System.IO.File.ReadAllText(filePath, Encoding.Default);
                        var sqlCmd = new SqlCommand(sqlExpression, sqlConn);
                        sqlConn.Open();
                        logger("Script execution", fileName);
                        sqlConn.InfoMessage += (s, e) => logger(fileName, "Message:" + e.Message);
                        sqlCmd.StatementCompleted += (s, e) => logger(fileName, e.RecordCount + " row(s) affected \n");
                        try
                        {
                            var tmp = sqlCmd.ExecuteReader();
                            if (tmp != null)
                                logger(tmp, "===================");
                        }
                        finally
                        {
                            sqlConn.Close();
                        }
                    }
                }
                catch (Exception e2)
                {
                    logger("Exception", e2.ToString());
                    _errors++;
                }
                finally
                {
                    logger();
                }
            }
            else
            {
                logger("File doesnt exist", filePath);
                notFound.Add(filePath);
            }
        }

        private static void logger()
        {
            using (StreamWriter writer = new StreamWriter(_path + "log.txt", true))
            {
                writer.WriteLine();
                writer.Flush();
            }
        }

        private static void logger(string level, string text)
        {
            using (StreamWriter writer = new StreamWriter(_path + "log.txt", true))
            {
                writer.WriteLine(string.Format("{0}: {1:dd/MM/yyyy hh:mm:ss}  {2} ", level, DateTime.Now, text));
                if (level == "File doesnt exist")
                    writer.WriteLine();
                writer.Flush();
            }
        }
        private static void logger(SqlDataReader reader, string separator)
        {
            using (StreamWriter writer = new StreamWriter(_path + "log.txt", true))
            {
                StringBuilder table = new StringBuilder();
                table.AppendLine(separator);
                if (reader.HasRows)
                {
                    int fieldCount = reader.FieldCount;
                    table.Append("|   ");
                    for (int i = 0; i < fieldCount; i++)
                    {
                        table.Append(reader.GetName(i) + "   ");
                    }
                    table.AppendLine("|");
                    table.AppendLine("-----------------");
                    while (reader.Read())
                    {
                        table.Append("|   ");
                        for (int i = 0; i < fieldCount; i++)
                            if (reader.GetValue(i) != null)
                                table.Append(reader.GetValue(i) + "   ");
                            else
                                table.Append("null   ");
                        table.AppendLine("|");
                    }
                }
                else
                    table.AppendLine("Empty table");
                table.AppendLine(separator);
                writer.WriteLine(table);
                writer.Flush();
            }
        }
    }
}
