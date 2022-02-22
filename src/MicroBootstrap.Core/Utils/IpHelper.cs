using System.Net;
using System.Net.Sockets;

namespace MicroBootstrap.Core.Utils;

public static class IpHelper
{
    public static string GetIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        return string.Empty;
    }
}
