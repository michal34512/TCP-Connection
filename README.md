# TCP Connection
Simple connetion between two hosts based on TCP protocol.

# Establishing connection
Server:
```diff
# Starting server
  Connection.Port = 7777;
  Connection.Start_Connection(Connection.ConnectionRole.Server);
```
Client:
```diff
# Connecting to server
  Connection.Port = 7777;
  Connection.IpServ = "127.0.0.1";
  Connection.Start_Connection(Connection.ConnectionRole.Client);
```

# Sending & receiving messages
Sending:
```diff
# Sending messages
  string mess = Console.ReadLine();
  Connection.SendMessage(Encoding.UTF8.GetBytes(mess));
```
Receiving:
```diff
# Receive messages
  List<byte[]> messages = Connection.ReceiveMessages();
  if (messages != null)
      foreach (byte[] mess in messages)
      {
          Console.WriteLine(Encoding.UTF8.GetString(mess));
      }
```
