using Limilabs.Client.POP3;
using Limilabs.Mail;
using Limilabs.Mail.MIME;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;

namespace DownloadAnexos
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        public Timer timer1;
        protected override void OnStart(string[] args)
        {
            try
            {
                this.AutoLog = true;
                //this.CanStop = true;
                string intervalo = Convert.ToString(ConfigurationManager.AppSettings["Intervalo"]);
                int i = (Convert.ToInt32(intervalo) * 60000);
                timer1 = new Timer(new TimerCallback(timer1_Tick), null, 1000, i);
                StreamWriter vWriter = new StreamWriter(@"C:\log_xml.txt", true);
                vWriter.WriteLine(" ! ! ! ! ! ! ! START SERVICE: " + DateTime.Now.ToString() + " | Intervalo de execução: " + intervalo.ToString() + " minuto(s) | ! ! ! ! ! ! ! |");
                vWriter.Flush();
                vWriter.Close();
            }
            catch
            {
                StreamWriter vWriter = new StreamWriter(@"C:\log_xml.txt", true);
                vWriter.WriteLine("ERR: Erro no serviço de download de XML. Favor verificar suas configurações. | | | ");
                vWriter.Flush();
                vWriter.Close();
            }


        }

        protected override void OnStop()
        {
            StreamWriter vWriter = new StreamWriter(@"C:\log_xml.txt", true);
            vWriter.WriteLine(" ! ! ! ! ! ! ! STOP SERVICE: " + DateTime.Now.ToString() + " |  ! ! ! ! ! ! ! | | ");
            vWriter.Flush();
            vWriter.Close();

        }
        private void timer1_Tick(object sender)
        {

            try
            {
                StreamWriter vWriter = new StreamWriter(@"C:\log_xml.txt", true);
                using (Pop3 pop3 = new Pop3())
                {
                    string conta = Convert.ToString(ConfigurationManager.AppSettings["Login"]);
                    string senha = Convert.ToString(ConfigurationManager.AppSettings["Senha"]);
                    string server = Convert.ToString(ConfigurationManager.AppSettings["Servidor"]);
                    string path = Convert.ToString(ConfigurationManager.AppSettings["PastaDestino"]);
                    string extensao = Convert.ToString(ConfigurationManager.AppSettings["ExtensaoArquivo"]);
                    string extensao2 = Convert.ToString(ConfigurationManager.AppSettings["ExtensaoArquivo"]);
                    extensao = "." + extensao;
                    try
                    {
                        pop3.Connect(server, 110);
                        pop3.UseBestLogin(conta, senha);
                        
                    }
                    catch
                    {
                        vWriter.WriteLine("\nERRO_CRITICO: NÃO FOI POSSÍVEL CONECTAR AO SERVIDOR. VERIFIQUE AS CONFIGURAÇÕES E A CONEXAO COM A INTERNET. | | | \n");
                    }
                    foreach (string uid in pop3.GetAll())
                    {
                        IMail email = new MailBuilder()
                            .CreateFromEml(pop3.GetMessageByUID(uid));

                        foreach (MimeData mime in email.Attachments)
                        {
                            if (mime.SafeFileName.Contains(extensao))
                            {
                                try
                                {
                                    mime.Save(@"" + path + mime.SafeFileName);
                                    vWriter.WriteLine("OK  | " + mime.SafeFileName + " | " + email.From + " " + email.Date + " | LOG: " + DateTime.Now.ToString());               
                                }
                                catch
                                {
                                    vWriter.WriteLine("\nERR_" + extensao2.ToUpper() + ": " + mime.SafeFileName + " | Erro download de arquivo. [Pasta de Destino Inacessivel/Incorreta]\n");
                                }
                            }
                            else
                            {
                                vWriter.WriteLine("DEL | " + mime.SafeFileName + " | " + email.From + " " + email.Date + " | LOG: " + DateTime.Now.ToString());
                            }
                           
                        }
                        try
                        {                            
                            pop3.DeleteMessageByUID(uid);
                        }
                        catch
                        {
                            vWriter.WriteLine("\nERR_DEL | Falha ao deletar a mensagem do servidor. |  | " + "  LOG: " + DateTime.Now.ToString()+ "\n");
                        }
                    }
                    pop3.Close();
                    vWriter.Flush();
                    vWriter.Close();
                }
            }
            catch
            {
                StreamWriter vWriter = new StreamWriter(@"C:\log_xml.txt", true);
                vWriter.WriteLine("ERR: Erro no serviço de download de XML. Favor verificar suas configurações.");
                vWriter.Flush();
                vWriter.Close();
            }
        }
    }
}
