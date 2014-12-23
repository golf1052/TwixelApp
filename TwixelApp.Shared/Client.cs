using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace TwixelApp
{
    public delegate void MessageHandler(object source, MessageHandlerEventArgs e);

    public class MessageHandlerEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class MessageHandlerListener
    {
        public void Subscribe(Client c)
        {
            c.Message += c_Message;
        }

        void c_Message(object source, MessageHandlerEventArgs e)
        {
            //Debug.WriteLine(e.Message);
        }
    }

    public class Client
    {
        StreamSocket client;
        HostName serverHost;
        string messageTerminator = "\r\n";
        DataWriter writer;
        DataReader reader;
        StreamReader streamReader;
        Task readTask;
        string channel;
        List<string> messages;

        public event MessageHandler Message;

        public enum Status
        {
            Disconnected,
            Connecting,
            Connected
        }

        public bool isConnectedToChannel = false;

        public Status status = Status.Disconnected;

        public Client(string channel)
        {
            client = new StreamSocket();
            this.channel = channel;
            messages = new List<string>();
        }

        public async Task Connect()
        {
            status = Status.Connecting;
            client = new StreamSocket();

            serverHost = new HostName("irc.twitch.tv");
            try
            {
                await client.ConnectAsync(serverHost, "6667");
                status = Status.Connected;
                writer = new DataWriter(client.OutputStream);
                reader = new DataReader(client.InputStream);
                streamReader = new StreamReader(client.InputStream.AsStreamForRead(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
                {
                }
            }
            
            readTask = Task.Factory.StartNew(() => ReadFromStream());
        }

        public async Task SendMessage(string message)
        {
            writer.WriteString(message + messageTerminator);
            //Debug.WriteLine("SENT: " + message);
            await writer.StoreAsync();
        }

        public async Task SendJoin()
        {
            await SendMessage("JOIN #" + channel);
            isConnectedToChannel = true;
        }

        public async Task SendPart()
        {
            await SendMessage("PART #" + channel);
        }

        public async Task SendIRCMessage(string message)
        {
            await SendMessage("PRIVMSG #" + channel + " :" + message);
        }

        public async Task SendPong(string message)
        {
            await SendMessage("PONG " + message);
        }

        public async Task SendWho()
        {
            await SendMessage("WHO #" + channel);
        }

        public async Task Login(string name, string accessToken)
        {
            await SendMessage("PASS oauth:" + accessToken);
            await SendMessage("NICK " + name);
        }

        public async Task SetNick(string name)
        {
            await SendMessage("NICK " + name);
        }

        async void ReadFromStream()
        {
            string result = "";
            while (status == Status.Connected)
            {
                reader.InputStreamOptions = InputStreamOptions.Partial;
                //string output = await streamReader.ReadLineAsync();
                //Debug.WriteLine(output);
                //try
                //{
                //    var count = await reader.LoadAsync(512);
                //}
                //catch (Exception ex)
                //{
                //    // shit can be broken here, something about closed streams...
                //    Debug.WriteLine(ex.Message);
                //}
                //byte[] content = new byte[reader.UnconsumedBufferLength];
                //reader.ReadBytes(content);
                //result = Encoding.UTF8.GetString(content, 0, content.Length);
                result = await streamReader.ReadLineAsync();
                //List<string> tmpMessages = await ProcessServerResponses(result);
                //foreach (string m in tmpMessages)
                //{
                //    messages.Add(m);
                //}
                messages.Add(result);

                for (int i = 0; i < messages.Count; i++)
                {
                    MessageHandlerEventArgs message = new MessageHandlerEventArgs();
                    message.Message = messages[i];
                    Message(this, message);
                    Debug.WriteLine(messages[i]);
                    await HandleResponse(messages[i]);
                    messages.RemoveAt(i);
                    i--;
                }
            }
        }

        async Task<List<string>> ProcessServerResponses(string message)
        {
            List<string> responses = new List<string>();
            int index = -1;
            do
            {
                index = message.IndexOf(messageTerminator);
                if (index != -1)
                {
                    string prefix = message.Substring(0, index);
                    string postfix = message.Substring(index + messageTerminator.Length);

                    bool handledResponse = await HandleResponse(prefix);

                    if (!handledResponse)
                    {
                        responses.Add(prefix);
                    }

                    message = postfix;
                }
            }
            while (index != -1);
            return responses;
        }

        async Task<bool> HandleResponse(string message)
        {
            if (message.StartsWith("PING"))
            {
                string server = message.Remove(0, 4);
                server.TrimStart(new char[] { ' ' });
                await SendPong(server);
                return true;
            }

            return false;
        }
    }
}
