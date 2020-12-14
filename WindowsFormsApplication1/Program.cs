using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using WindowsFormsApplication1;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "/a")
            {
                Form1.toString = ConfigurationManager.AppSettings["to"];
                Form1.fromString = ConfigurationManager.AppSettings["from"];
                Form1.mailserverString = ConfigurationManager.AppSettings["mailserver"];
                Form1.subjectString = ConfigurationManager.AppSettings["subject"];

                Form1.host = ConfigurationManager.AppSettings["host"];
                Form1.port = ConfigurationManager.AppSettings["port"];
                Form1.sid = ConfigurationManager.AppSettings["sid"];
                Form1.user = ConfigurationManager.AppSettings["user"];
                Form1.password = ConfigurationManager.AppSettings["password"];

                Form1.table = ConfigurationManager.AppSettings["table"];

                string body = Form1.QueryDB();
                foreach (string email in Form1.toString.Split(';'))
                {
                    Form1.MailSend(body, Form1.subjectString, email, Form1.fromString, Form1.mailserverString);
                }
            }
            else {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
