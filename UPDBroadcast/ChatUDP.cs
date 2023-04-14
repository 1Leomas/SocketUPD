using System.Collections.Concurrent;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UPDBroadcast;

internal class ChatUDP
{
    private readonly Socket _sender;
    private readonly Socket _receiver;
    private int _port { get; }

    private Client _client;
    private StringBuilder _input = new();

    private Send _send;

    private ConcurrentDictionary<string, EndPoint> _clients = new();

    private readonly object _identity = new();

    public ChatUDP(int port)
    {
        _sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _sender.EnableBroadcast = true;

        _send = new Send(_sender);

        _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        _port = port;
    }

    private IPAddress GetIPAdress()
    {
        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
        return ipEntry.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
    }

    public void Bind()
    {
        var localIP = new IPEndPoint(IPAddress.Any, _port);

        _receiver.Bind(localIP);

        Console.WriteLine("Listening on {0}", localIP);
    }

    public void SendClientStatus(MessageType type)
    {
        var message = $"{type}:{_client.NickName}:{_client.ConsoleColor}:";
        var byteToSend = Encoding.UTF8.GetBytes(message);
        EndPoint remotePoint = new IPEndPoint(IPAddress.Broadcast, _port);
        _sender.SendTo(byteToSend, remotePoint);
    }

    public void Send()
    {
        Console.WriteLine("Input message and press ENTER");
        Console.WriteLine("For private messages use this format: @userName yourMessage");
        while (true)
        {
            lock (_identity)
            {
                PrintClientMessage(_client.NickName, _client.ConsoleColor);
            }

            _input.Append(Console.ReadLine() ?? "");

            if (_input.Length == 0) continue;

            if (_input.ToString().StartsWith('@'))
            {
                SendToSpecificClient();
            }
            else
            {
                var byteToSend = Encoding.UTF8.GetBytes
                    ($"{MessageType.MESSAGE}:{_client.NickName}:{_client.ConsoleColor}:{_input}");

                EndPoint remotePoint = new IPEndPoint(IPAddress.Broadcast, _port);
                _sender.SendTo(byteToSend, remotePoint);
            }

            _input.Clear();
        }
    }

    private void SendToSpecificClient()
    {
        var name = _input.ToString().Split().First().Remove(0, 1);
        var message = _input.ToString().Replace($"@{name} ", "");

        if (!_clients.TryGetValue(name, out var remotePoint))
        {
            Console.WriteLine($"Client {name} not found");
            return;
        }

        var byteToSend = Encoding.UTF8.GetBytes
            ($"{MessageType.PRIVATE_MESSAGE}:{_client.NickName}:{_client.ConsoleColor}:{message}");

        var ipAdress = IPAddress.Parse(remotePoint.ToString().Split(':').First()).ToString();
        EndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ipAdress), _port);
        _sender.SendTo(byteToSend, remoteEP);
    }

    public void Listen()
    {
        Task.Run(() =>
        {
            while (true)
            {
                byte[] byteReceive = new byte[1024];
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                _receiver.ReceiveFrom(byteReceive, ref remoteIp);

                if (ItsMe(remoteIp)) continue;

                lock (_identity)
                {
                    ProcessMessagesByType(byteReceive, remoteIp);
                }
            }
        });
    }

    private bool ItsMe(EndPoint remoteIp)
    {
        var ip = remoteIp.ToString().Split(':').First();
        return ip == _client.IPAddress.ToString();
    }

    private void ProcessMessagesByType(byte[] byteReceive, EndPoint remoteIp)
    {
        var receiveList = Encoding.UTF8.GetString(byteReceive).Split(':');

        MessageType messageType;
        Enum.TryParse(receiveList[0], out messageType);

        switch (messageType)
        {
            case MessageType.TRASH:
                break;
            case MessageType.MESSAGE: // M:n:c:m
            {
                AddNewClientToList(receiveList[1], remoteIp);

                var cp = Console.GetCursorPosition();
                Console.SetCursorPosition(0, cp.Top);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, cp.Top);

                ConsoleColor color = Enum.Parse<ConsoleColor>(receiveList[2]);
                PrintClientMessage(receiveList[1], color, receiveList[3]);

                break;
            }
            case MessageType.PRIVATE_MESSAGE:
            {
                AddNewClientToList(receiveList[1], remoteIp);

                var cp = Console.GetCursorPosition();
                Console.SetCursorPosition(0, cp.Top);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, cp.Top);

                ConsoleColor color = Enum.Parse<ConsoleColor>(receiveList[2]);
                PrintPrivateMessage(receiveList[1], color, receiveList[3]);
                break;
            }
            case MessageType.CONNECTED: // C:n:c
            {
                AddNewClientToList(receiveList[1], remoteIp);

                var cp = Console.GetCursorPosition();
                Console.SetCursorPosition(0, cp.Top);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, cp.Top);

                ConsoleColor color = Enum.Parse<ConsoleColor>(receiveList[2]);

                PrintClient(receiveList[1], color);
                Console.WriteLine(" connected to server.");

                break;
            }
            case MessageType.DISCONECTED: // В:n:c
            {
                var result = RemoveClientFromList(receiveList[1], remoteIp);

                var cp = Console.GetCursorPosition();
                Console.SetCursorPosition(0, cp.Top);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, cp.Top);

                ConsoleColor color = Enum.Parse<ConsoleColor>(receiveList[2]);

                PrintClient(receiveList[1], color);
                Console.WriteLine(" disconnected from the server.");

                break;
            }
        }

        if (_input.Length > 0) PrintClientMessage(_client.NickName, _client.ConsoleColor, _input.ToString());
        else PrintClientMessage(_client.NickName, _client.ConsoleColor);



    }

    private bool RemoveClientFromList(string receive, EndPoint remoteIp)
    {
        return _clients.TryRemove(receive, out remoteIp);
    }

    private void AddNewClientToList(string clientName, EndPoint remoteIp)
    {
        Task.Run(() =>
        {
            if (!ItsMe(remoteIp))
            {
                _clients.TryAdd(clientName, remoteIp);
            }
        });
    }

    private void PrintClientMessage(string name, ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.Write($"{name}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($": {message}");
    }
    private void PrintClientMessage(string name, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"{name}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(": ");
    }

    private void PrintClient(string name, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"{name}");
        Console.ForegroundColor = ConsoleColor.White;
    }
        
    private void PrintPrivateMessage(string name, ConsoleColor color, string message)
    {
        //Console.BackgroundColor = ConsoleColor.Gray;
        Console.ForegroundColor = color;
        Console.Write($"{name}");
        Console.WriteLine($": {message}");
        Console.ForegroundColor = ConsoleColor.White;
        //Console.BackgroundColor = ConsoleColor.Black;

    }

    public void SetClient(Client client)
    {
        _client = client;
    }

    public enum MessageType
    {
        TRASH = 0,
        MESSAGE = 1,
        CONNECTED = 2,
        DISCONECTED = 3,
        PRIVATE_MESSAGE = 4
    }
}