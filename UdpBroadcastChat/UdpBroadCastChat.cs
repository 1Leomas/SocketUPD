using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpBroadcastChat;

internal class UdpBroadCastChat
{
    private Socket _udpSocket;

    public UdpBroadCastChat()
    {
        _udpSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Dgram,
            ProtocolType.Udp);

    }

    public void Bind()
    {
        var localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        _udpSocket.Bind(localIP);
    }

    public void ReceiveMessages()
    {
        Task.Run(() =>
        {
            while (true)
            {
                byte[] bytesReceived = new byte[1024];

                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                _udpSocket.ReceiveFrom(bytesReceived, ref remoteEP);

                var message = Encoding.UTF8.GetString(bytesReceived);

                Console.WriteLine("[{0}] [{1}]", remoteEP, message);
            }
        });
    }

    public void SendMessages()
    {
        while (true)
        {
            //Console.WriteLine("EnterMessage: ");
            var input = Console.ReadLine() ?? "";

            var bytesToSend = Encoding.UTF8.GetBytes(input);

            EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);

            _udpSocket.SendTo(bytesToSend, remotePoint);
        }
    }
}