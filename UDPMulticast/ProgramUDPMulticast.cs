using UDPMulticast;

string multcastIP = "239.5.6.7";
int port = 5002;

UDPChat chat = new UDPChat(multcastIP, port);

Console.WriteLine("Input format: <IP>:<Text>"); //192.168.1.24: salut
Console.WriteLine("Ip = 0 -> Multicast");

chat.StartReceiveLoop();

while (true)
{
    try
    {
        var input = Console.ReadLine() ?? "";
        var splitted = input.Split(":");
        var toIP = splitted[0];
        var text = splitted[1];

        if (toIP == "0")
        {
            chat.SendToGeneral(text);
        }
        else
        {
            chat.SendTo(toIP, text);
        }
    }
    catch (Exception e)
    {
        Console.WriteLine("Exception: {0}", e.Message);
    }
}