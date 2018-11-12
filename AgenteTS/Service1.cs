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

        public System.Diagnostics.EventLog eventLog1;
        public Service1()
        {
            InitializeComponent();
           
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
                    
                    socket.Close();
                }
            }catch (Exception e)
            {
                Console.WriteLine(e.Message);
               
            }
        }


    }
}
