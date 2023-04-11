using System.Net.Sockets;

namespace UPDBroadcast;

internal class Send
{
    private readonly Socket _sender;

    public Send(Socket sender)
    {
        _sender = sender;
    }


}