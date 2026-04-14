using System.Net.Sockets;

namespace Bugtracker.Utils
{
    public static class WebServerUtils
    {
        /// <summary>
        /// checks if the specified webserver is reachable via https
        /// </summary>
        /// <param name="url">the url to test the connection to</param>
        /// <returns></returns>
        public static bool IsReachable(string url)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect("url", 443);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }


    }
}