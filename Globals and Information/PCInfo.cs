using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Bugtracker.Attributes;
using Bugtracker.Configuration;
using Bugtracker.Logging;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bugtracker.Globals_and_Information
{
    public class PCInfo
    {
        private protected RunningConfiguration _configuration;
        
        /// <summary>
        /// constructor
        /// </summary>
        public PCInfo()
        {
            Logger.Log("New PCinfo object was created.", (LoggingSeverity)2);

            Logger.Log("GetHostname() executed", (LoggingSeverity)2);
            Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME");

            Logger.Log("GetClientname() executed", (LoggingSeverity)2);
            if (IsRemoteSession)
            {
                Clientname = System.Environment.GetEnvironmentVariable("CLIENTNAME");
            }
            else
            {
                Clientname = Hostname;
            }

            Logger.Log("GetDomainName() executed", (LoggingSeverity)2);
            DomainName = System.Environment.UserDomainName;

            Logger.Log("GetIPAddress() executed", (LoggingSeverity)2);

            // Get IPv4 address, excluding loopback and link-local addresses
            try
            {
                var hostEntry = Dns.GetHostEntry(Hostname);
                var validAddresses = hostEntry.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                    .Where(ip => !System.Net.IPAddress.IsLoopback(ip)) // Exclude 127.x.x.x
                    .Where(ip => !ip.ToString().StartsWith("169.254.")) // Exclude link-local 169.254.x.x
                    .ToList();

                if (validAddresses.Any())
                {
                    IPAddress = validAddresses.First().ToString();
                }
                else
                {
                    IPAddress = "unknown";
                    Logger.Log("No valid IPv4 address found, using 'unknown'", LoggingSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                IPAddress = "unknown";
                Logger.Log($"Failed to detect IP address: {ex.Message}", LoggingSeverity.Warning);
            }

            Logger.Log("GetMACAddress() executed", (LoggingSeverity)2);
            MACAddress = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                     where nic.OperationalStatus == OperationalStatus.Up
                     select nic.GetPhysicalAddress().ToString()
                    ).FirstOrDefault().ToString();

            Logger.Log("GetUsername() executed", (LoggingSeverity)2);
            CurrentUserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            Logger.Log("GetBoottime() executed", (LoggingSeverity)2);
            LastBootTime = PCInfo.UpTime.ToString();
        }


        /// <summary>
        /// Returns hostname of device
        /// </summary>
        /// <returns></returns>
        [Key("hostname")]
        public static string Hostname { get; private set; }

        /// <summary>
        /// Returns clientname of device
        /// </summary>
        [Key("clientname")]
        public static string Clientname { get; private set; }

        /// <summary>
        /// Returns if device is in remote session
        /// </summary>
        [Key("remoteSession")]
        public static bool IsRemoteSession
        {
            get => System.Windows.Forms.SystemInformation.TerminalServerSession;
        }

        /// <summary>
        /// Returns the domain name
        /// </summary>
        /// <returns></returns>
        [Key("domainName")]
        public static string DomainName { get; private set; }
 

        /// <summary>
        /// Returns IP address of device
        /// </summary>
        /// <returns></returns>
        [Key("ipAddress")]
        public static string IPAddress { get; private set; }

        /// <summary>
        /// Returns MAC address of device
        /// </summary>
        /// <returns></returns>
        [Key("macAddress")]
        public static string MACAddress { get; private set; }

        /// <summary>
        /// Returns name of current user
        /// </summary>
        /// <returns></returns>
        [Key("userName")]
        public static string CurrentUserName { get; private set; }
       

       
        //Credits: - https://stackoverflow.com/a/16673001
        //Author: Martin
        public static TimeSpan UpTime
        {
            get
            {
                [DllImport("kernel32")]
                extern static UInt64 GetTickCount64();
                return TimeSpan.FromMilliseconds(GetTickCount64());
            }
        }

        /// <summary>
        /// Get time and date of last boot up 
        /// from WMI
        /// </summary>
        /// <returns></returns>
        [Key("bootTime")]
        public static string LastBootTime { get; private set; }


        /// <summary>
        /// Summary on pc contains, hostname
        /// ip, mac, domain name and currently logged
        /// in user
        /// </summary>
        /// <returns></returns>
        public static string Summary()
        {
            StringBuilder info = new();
            
            try
            {
                info.AppendLine("Hostname:\t"       + Hostname);
                info.AppendLine("Domain:\t\t"       + DomainName);
                info.AppendLine("User: \t\t"        + CurrentUserName);
                info.AppendLine("IP-Addr.:\t\t"     + IPAddress);
                info.AppendLine("MAC-Addr.:\t"      + MACAddress);
                info.AppendLine("Startup time: \t"  + LastBootTime);
                info.AppendLine("Remote Session\t" +  IsRemoteSession);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, 0);
            }

            return info.ToString();
        }

    }
}
