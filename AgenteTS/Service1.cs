using CryptoLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgenteTS
{
    public partial class Service1 : ServiceBase
    {
        //CD C:\Windows\Microsoft.NET\Framework\v2.0.50727
        //InstallUtil C:\PathToEXE\Service.exe -i
        public static System.Diagnostics.EventLog eventLog1 = new System.Diagnostics.EventLog();
        public static JObject objectPassword;

        public Service1()
        {
            InitializeComponent();
            this.AutoLog = false;

            try { 
                // create an event source, specifying the name of a log that
                // does not currently exist to create a new, custom log
                if (!System.Diagnostics.EventLog.SourceExists("AGENT_SOURCE"))
                {
                    System.Diagnostics.EventLog.CreateEventSource(
                        "AGENT_SOURCE", "AGENT_LOG");
                }
                // configure the event log instance to use this source name
                eventLog1.Source = "AGENT_SOURCE";
                eventLog1.Log    = "AGENT_LOG";
            }catch(Exception ex)
            {
                
            }
        }

        protected override void OnStart(string[] args)
        {
            try {
                initService();
            }catch (Exception e){
                eventLog1.WriteEntry(e.Message);
            }
            
        }

        public void initService()
        {
            string curFile = @"c:\agentWin32Service.json";

            if (!File.Exists(curFile))
            {

                File.Create(curFile).Close();
                
                JObject jsonObject = new JObject(new JProperty("password", Encryptor.MD5Hash("excelsior"));
                jsonObject.Add(new JProperty("port", 5000));
                File.WriteAllText(curFile, jsonObject.ToString());

                eventLog1.WriteEntry(curFile+"-->"+jsonObject.ToString());
            }


            Service1.objectPassword = JObject.Parse(File.ReadAllText(curFile));
            
            Thread t = new Thread(new ThreadStart(listernet));
            t.Start();
        }

        protected override void OnStop()
        {
        }

        public static void listernet() {
            IPAddress ipAddress     = IPAddress.Parse("0.0.0.0");
            TcpListener tcpListener = new TcpListener(ipAddress, (int)Service1.objectPassword["port"]);
            tcpListener.Start();
            Byte[] bytes = new Byte[256];

            while (true)
            {
                Socket socket = tcpListener.AcceptSocket();
                int result    = socket.Receive(bytes);

                ASCIIEncoding asen = new ASCIIEncoding();

                string str = asen.GetString(bytes);
                eventLog1.WriteEntry("str: "+ str);
                try
                {
                    string msg           = "ERROR";
                    var parametros       = str.Split('|');
                    eventLog1.WriteEntry(parametros[1].ToString());

                    //SEMPRE PASSAR MAIS 1 PIPE PARA NA ODAR ERROR
                    //POS EM SOCKET A ULTIMA STRING EH \n\0\0\0\0\0\0\0
                    // echo "<password>|<get-session kill-session>|param2|" | nc <ip> <port>
                    if (parametros.Length == 0)
                    {
                        msg = "Use | para passar a string de parametros";
                    }

                    if (!Service1.objectPassword["password"].ToString().Equals(Encryptor.MD5Hash(parametros[0])))
                    {
                        msg = "Adicione o password na msg";
                    }

                    if (parametros[1].Equals("get-session"))
                    {
                        eventLog1.WriteEntry("get-session!");
                        string param2 = null;
                        if (!parametros[2].Equals(""))
                        {
                            param2 = parametros[2];
                        }

                        string output = runCmd("qwinsta", param2);
                        msg           = output;
                    }

                    if (parametros[1].Equals("kill-session"))
                    {
                        eventLog1.WriteEntry("kill-session!");
                        string param2 = null;
                        if (!parametros[2].Equals(""))
                        {
                            param2 = parametros[2];
                        }
                        string output = runCmd("rwinsta", param2);
                        msg           = output;
                    }

                    socket.Send(Encoding.ASCII.GetBytes(msg));
                }
                catch(Exception e)
                {
                    eventLog1.WriteEntry(e.Message);
                }
                finally
                {
                    socket.Close();
                }
                    
            }
            
        }


        public static string runCmd(string command, string params1)
        {
            string filePath = Path.Combine(Environment.SystemDirectory, "qwinsta.exe");

            Process compiler = new Process();
            compiler.StartInfo.FileName = @"C:\Windows\SysNative\"+ command + ".exe";
            compiler.StartInfo.Arguments = null;
            if(params1 != null)
            {
                compiler.StartInfo.Arguments = params1;
            }


            compiler.StartInfo.UseShellExecute = false;
            compiler.StartInfo.RedirectStandardOutput = true;
            compiler.StartInfo.Verb = "runas";
            compiler.Start();

            string ouput = compiler.StandardOutput.ReadToEnd();
            eventLog1.WriteEntry(compiler.StandardOutput.ReadToEnd());

            compiler.WaitForExit();
            return ouput;
        }
    }


}

