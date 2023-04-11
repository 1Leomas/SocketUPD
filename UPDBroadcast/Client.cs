using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UPDBroadcast;

public class Client
{
    public string NickName { get; private set; }
    public ConsoleColor ConsoleColor { get; private set; }
    public IPAddress IPAddress { get; private set; }

    public Client()
    {
        NickName = GenerateNickname();
        ConsoleColor = GenerateColor();
        IPAddress = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList
            .First(x => x.AddressFamily == AddressFamily.InterNetwork);
    }

    public string GenerateNickname()
    {
        var nameGenerator = new NameGenerator();
        var nickname = nameGenerator.Generate(new Random().Next(3, 6));
        return nickname;
    }

    public ConsoleColor GenerateColor()
    {
        var random = new Random().Next(1, 14);
        var color = (ConsoleColor)Enum.ToObject(typeof(ConsoleColor), random);

        return color;
    }
}