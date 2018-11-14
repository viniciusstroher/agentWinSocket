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
using System.Net;
using System.Net.Sockets;
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
                
                JObject jsonObject = new JObject(new JProperty("password", Encryptor.MD5Hash("excelsior")));
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
            TcpListener tcpListener = new TcpListener(ipAddress, 5000);
            tcpListener.Start();
            Byte[] bytes = new Byte[256];

            try { 
                while (true)
                {
                    Socket socket = tcpListener.AcceptSocket();
                    int result    = socket.Receive(bytes);

                    ASCIIEncoding asen = new ASCIIEncoding();

                    string str = asen.GetString(bytes);
                    Console.WriteLine(str);
                    eventLog1.WriteEntry("str: "+ str);
                    try
                    {
                        string msg       = "ERROR";
                        string[] params1 = str.Split('|');

                        eventLog1.WriteEntry("compare password: " + Service1.objectPassword["password"] + "<-->" + Encryptor.MD5Hash(params1[0]));
                        eventLog1.WriteEntry("compare password: " + Service1.objectPassword["password"].ToString().Equals(Encryptor.MD5Hash(params1[0])));

                        
                        Array.ForEach(params1, eventLog1.WriteEntry);

                        if (params1.Length == 0)
                        {
                            msg = "Use | para passar a string de parametros";
                        }else if (!Service1.objectPassword["password"].ToString().Equals(Encryptor.MD5Hash(params1[0])))
                        {
                            msg = "Adicione o password na msg";
                        }else if (params1[1].Equals("get-services"))
                        {
                            eventLog1.WriteEntry("FETCH GET-SERVICES!");
                            using (PowerShell PowerShellInstance = PowerShell.Create())
                            {
                                // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                                // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.

                                PowerShellInstance.AddScript("get-service;");
                                Collection<PSObject> results = PowerShellInstance.Invoke();

                                foreach (PSObject obj in results)
                                {
                                    eventLog1.WriteEntry(obj.ToString());
                                }

                                msg = "GET_SERVICE!!";
                                // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                                //PowerShellInstance.AddParameter("param1", "parameter 1 value!");
                            }

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
            }catch (Exception e)
            {
                eventLog1.WriteEntry("In OnStart.");
               
            }
        }


    }
}
