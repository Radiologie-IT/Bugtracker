using System;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using Bugtracker.Configuration;

namespace Bugtracker.Utils
{
    public static class ServerUtils
    {
        private static readonly RunningConfiguration runningConfiguration = RunningConfiguration.GetInstance();

        /// <summary>
        /// Load XML File from given URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static XmlDocument GetXMLFromURI(string uri)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(uri);

            return xmlDoc;
        }
    }

    public class Server
    {
        public static event EventHandler CheckedServerStatus;
        
        /// <summary>
        /// Returns the current server status
        /// </summary>
        public ServerStatus ServerStatus { get; protected set; }
        
        /// <summary>
        /// Returns the current server path as string
        /// </summary>
        public string ServerPath { get; set; }
        
        /// <summary>
        /// Check interval in seconds
        /// </summary>
        public int CheckInterval { get; set; }
        private static System.Timers.Timer _aTimer;
        //private readonly Ping pinger;

        /// <summary>
        /// The constructor of the server class
        /// </summary>
        /// <param name="serverPath"></param>
        public Server(string serverPath)
        {
            //pinger = new Ping();
            this.ServerPath = serverPath;

            //SetTimer();

        }

        /// <summary>
        /// Set the timer for the server status check
        /// </summary>
        private void SetTimer()
        {
            _aTimer = new System.Timers.Timer(5000);
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }

        /// <summary>
        /// Check the server status. Currently just returns true -> Available
        /// My mehtod returns a catastophic exception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            /*try
            {
                PingReply reply = pinger.Send(ServerPath);
                if(reply.Status == IPStatus.Success)
                    ServerStatus = ServerStatus.Available;
            }
            catch
            {
                ServerStatus = ServerStatus.NotAvailable;
            }

            if(CheckedServerStatus != null)
                CheckedServerStatus?.Invoke(null,null);*/
            ServerStatus = ServerStatus.Available;

        }
    }
}
