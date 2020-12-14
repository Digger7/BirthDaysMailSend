using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Mail;
using System.Data.OracleClient;
using System.Configuration;
using System.Data.Common;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public static OracleConnection GetDBConnection(string host, string port, string sid, string user, string password)
        {
            string connString = "Data Source=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = "
                 + host + ")(PORT = " + port + "))(CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = "
                 + sid + ")));Password=" + password + ";User ID=" + user;
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connString;
            return conn;
        }

        public static string toString = "";
        public static string fromString = "";
        public static string mailserverString = "";
        public static string subjectString = "";

        public static string host = "";
        public static string port = "";
        public static string sid = "";
        public static string user = "";
        public static string password = "";

        public static string table = "";

        public Form1()
        {
            InitializeComponent();

            subjectTextBox.Text = ConfigurationManager.AppSettings["subject"];
            toTextBox.Text = ConfigurationManager.AppSettings["to"];
            fromTextBox.Text = ConfigurationManager.AppSettings["from"];
            mailserverTextBox.Text = ConfigurationManager.AppSettings["mailserver"];
            hostTextBox.Text = ConfigurationManager.AppSettings["host"];
            portTextBox.Text = ConfigurationManager.AppSettings["port"];
            sidTextBox.Text = ConfigurationManager.AppSettings["sid"];
            userTextBox.Text = ConfigurationManager.AppSettings["user"];
            passwordTextBox.Text = ConfigurationManager.AppSettings["password"];
            tableTextBox.Text = ConfigurationManager.AppSettings["table"];
        }

        private void ButtonQuery_Click(object sender, EventArgs e)
        {
            fromString = fromTextBox.Text;
            toString = toTextBox.Text;
            mailserverString = mailserverTextBox.Text;
            subjectString = subjectTextBox.Text;

            host = hostTextBox.Text;
            port = portTextBox.Text;
            sid = sidTextBox.Text;
            user = userTextBox.Text;
            password = passwordTextBox.Text;

            table = tableTextBox.Text;

            string body = QueryDB();
            foreach (string email in toString.Split(';'))
            {
                MailSend(body, subjectString, email, fromString, mailserverString);
            }
        }

        public static bool MailSend(string body, string subject, string to, string from, string server)
        {
            bool result = false;
            try
            {
                var mail = new MailMessage(from, to);
                var client = new SmtpClient
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = server
                };
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                client.Send(mail);
                result = true;
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public static string QueryDB()
        {
            string result = @"<html><body><center>";
            result += "<h3><i><span style='color:red'>Дни рождения на ближайшие 10 дней</span></i></h3>";

            #region ДНИ РОЖДЕНИЯ
            using (var conn = GetDBConnection(host, port, sid, user, password))
            {
                conn.Open();
                try
                {
                    List<string> listUsers = new List<string>();

                    string sql = $@"
                        SELECT 
                            ROUND(sub.BIRTHDAY - sysdate,1) as DIFF, 
                            sub.* 
                        FROM (
                               select
                                 to_date(to_char(sysdate,'yyyy')||to_char(t.DATA_BIRTH,'mmdd'), 'yyyymmdd HH:MI:SS') as BIRTHDAY
                                 ,t.FA || ' ' || t.IM || ' ' || t.OT as FIO
                                 ,to_char(t.DATA_BIRTH,'DD.MM.YYYY') as DATA_BIRTH
                                 ,t.POSITION
                                 ,t.EMAIL
                               from 
                               birthday.{table} t 
                        ) sub
                        WHERE 
                               sub.BIRTHDAY - sysdate <= 10
                               AND sub.BIRTHDAY - sysdate > -1
                               AND sub.DATA_BIRTH IS NOT NULL
                        ORDER BY 
                              sub.birthday ASC
                    ";

                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sql;

                    result += "<table border='0' width='1024'>";
                    result += @"<tr>
                                    <td><b>ФИО</b></td>
                                    <td><b>Дата рождения</b></td>
                                    <td><b>Должность</b></td>
                                    <td><b>E-mail</b></td>
                                </tr>";
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                result += "<tr>";

                                //--FIO--> Разукрашки
                                string fio = reader["FIO"].ToString();
                                double diff = Convert.ToDouble(reader["DIFF"]);
                                if (diff <= 0) fio = $"<span style='color:red'>{fio}</span>";
                                if (diff >= 0 && diff <=5 ) fio = $"<span style='color:blue'>{fio}</span>";
                                if (diff > 5) fio = $"<span style='color:green'>{fio}</span>";
                                //--FIO--<

                                result += "<td>" + fio + "</td>";
                                result += "<td>" + reader["DATA_BIRTH"].ToString() + "</td>";
                                result += "<td>" + reader["POSITION"].ToString() + "</td>";
                                result += "<td>" + reader["EMAIL"].ToString() + "</td>";
                                result += "</tr>";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                
                result += "</table></center>";
                result += @"<p>
                                <i>
                                    Примечание: Чтобы поздравить именинника, щелкните по электронному адресу в столбце “e - mail”
                                </i>
                            </p>";
            }
            #endregion

            result += "</body></html>";
            return result;
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Сохранить параметры?", "Вы уверены?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;

                confCollection["to"].Value = toTextBox.Text;
                confCollection["from"].Value = fromTextBox.Text;
                confCollection["subject"].Value = subjectTextBox.Text;
                confCollection["mailserver"].Value = mailserverTextBox.Text;

                confCollection["host"].Value = hostTextBox.Text;
                confCollection["port"].Value = portTextBox.Text;
                confCollection["sid"].Value = sidTextBox.Text;
                confCollection["user"].Value = userTextBox.Text;
                confCollection["password"].Value = passwordTextBox.Text;

                confCollection["table"].Value = tableTextBox.Text;

                //Шифрование
                config.AppSettings.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
  
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

            }
        }


    }
}
