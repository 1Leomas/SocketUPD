using UdpBroadcastChat;

UdpBroadCastChat chat = new UdpBroadCastChat();

chat.Bind();

chat.ReceiveMessages();

chat.SendMessages();