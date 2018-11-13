using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
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

        public Service1()
        {
            InitializeComponent();
            this.AutoLog = false;

            // create an event source, specifying the name of a log that
            // does not currently exist to create a new, custom log
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "AGENT_SOURCE", "AGENT_LOG");
            }
            // configure the event log instance to use this source name
            eventLog1.Source = "AGENT_SOURCE";
            eventLog1.Log    = "AGENT_LOG";

        }

        protected override void OnStart(string[] args)
        {
            //Thread t = new Thread(listernet);
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
                    int result = socket.Receive(bytes);

                    ASCIIEncoding asen = new ASCIIEncoding();

                    string str = asen.GetString(bytes);
                    Console.WriteLine(str);
                    eventLog1.WriteEntry("str: "+ str);
                    if (str.Equals("get-services"))
                    {

                        using (PowerShell PowerShellInstance = PowerShell.Create())
                        {
                            // use "AddScript" to add the contents of a script file to the end of the execution pipeline.
                            // use "AddCommand" to add individual commands/cmdlets to the end of the execution pipeline.
                            PowerShellInstance.AddScript("param($param1) $d = get-date; $s = 'test string value'; " +
                                    "$d; $s; $param1; get-service");

                            // use "AddParameter" to add a single parameter to the last command/script on the pipeline.
                            PowerShellInstance.AddParameter("param1", "parameter 1 value!");
                        }

                    }
                    socket.Close();
                }
            }catch (Exception e)
            {
                eventLog1.WriteEntry("In OnStart.");
                Console.WriteLine(e.Message);
               
            }
        }


    }
}
