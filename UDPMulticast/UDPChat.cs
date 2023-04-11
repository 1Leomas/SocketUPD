using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPMulticast;

internal class UDPChat
{
    private Socket _multicastSocket;
    private Socket _senderSocket;
    private string _multicastIP;
    private int _multicastPort;
    
    public UDPChat(string multicastIP, int multicastPort)
    {
        //IpAddress.Any - asculta de pe orice interfata
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, multicastPort); 
        
        _multicastPort = multicastPort;
        _multicastIP = multicastIP;

        _multicastSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram, 
            ProtocolType.Udp);

        _multicastSocket.Bind(ipEndPoint);

        _multicastSocket.SetSocketOption(
            SocketOptionLevel.IP, 
            SocketOptionName.AddMembership, 
            new MulticastOption(IPAddress.Parse(multicastIP)));

        _senderSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram, 
            ProtocolType.Udp);
        _senderSocket.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.AddMembership,
            new MulticastOption(IPAddress.Parse(multicastIP)));

        Console.WriteLine("User address: {0}", _multicastSocket.LocalEndPoint);
    }

    public void SendToGeneral(string text)
    {
        IPAddress multicastIP = IPAddress.Parse(_multicastIP);
        EndPoint multicastEndPoint = new IPEndPoint(multicastIP, _multicastPort);
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        _senderSocket.SendTo(bytes, multicastEndPoint);
    }

    public void SendTo(string ip, string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        EndPoint receiverEndpoint = new IPEndPoint(IPAddress.Parse(ip), _multicastPort);

        _senderSocket.SendTo(buffer, receiverEndpoint);
    }

    public void StartReceiveLoop()
    {
        Task.Run(() => receive());
    }

    private void receive()
    {
        while (true)
        {
            byte[] buffer = new byte[1024];
            EndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);

            _multicastSocket.ReceiveFrom(buffer, ref remoteSender);

            string text = Encoding.UTF8.GetString(buffer);

            Console.WriteLine("From: {0}. Message: {1}", 
                remoteSender, text);
        }
    }
}