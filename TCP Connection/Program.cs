using TCP_Connection;
using System.Text;

Console.WriteLine("Choose server - s, client - c");
string mode = Console.ReadLine();
if (mode == "s")
{
    // Starting server
    Connection.Port = 7777;
    Connection.Start_Connection(Connection.ConnectionRole.Server);
}
else if(mode == "c")
{
    // Connecting to server
    Connection.Port = 7777;
    Connection.IpServ = "127.0.0.1";
    Connection.Start_Connection(Connection.ConnectionRole.Client);
}



while (true)
{
    Thread.Sleep(1000);
    if (Connection.Role == Connection.ConnectionRole.Server)
    {
        // Receive messages
        List<byte[]> messages = Connection.ReceiveMessages(); // Receiving messages
        if (messages != null)
            foreach (byte[] mess in messages)
            {
                Console.WriteLine(Encoding.UTF8.GetString(mess));
            }
    }else if (Connection.Role == Connection.ConnectionRole.Client)
    {
        // Send messages
        string mess = Console.ReadLine();
        Connection.SendMessage(Encoding.UTF8.GetBytes(mess)); // Sending messages
    }
}
