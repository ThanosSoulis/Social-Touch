using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace NetworkedBehaviour
{
    public class LocalIPAddress : MonoBehaviour
    {
        public string LocalIP;

        private void Start()
        {
            LocalIP = GetLocalIPAddress();
        }
        
        private string GetLocalIPAddress()
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }
    }
}