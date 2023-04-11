using UPDBroadcast;


/*
Messages Format:
         MESSAGE:name:ConsoleColor:messageText
 PRIVATE_MESSAGE:name:ConsoleColor:messageText
       CONNECTED:name:ConsoleColor
    DISCONNECTED:name:ConsoleColor

    @name message
*/

ChatUDP chat = new ChatUDP(5000);
try
{
    chat.Bind();
}
catch (Exception e)
{
    Console.WriteLine("Error while Bind receiver socket");
    Console.WriteLine(e);
    return;
}

Client client = new Client();
chat.SetClient(client);

chat.Listen();

chat.SendClientStatus(ChatUDP.MessageType.CONNECTED);

chat.Send();

Console.ReadKey();