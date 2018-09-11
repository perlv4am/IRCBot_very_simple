using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace IRCbot
{
    class Program
    {
        static void Main(string[] args)
        {
            var ircBot = new IRCbot(
        server: "chat.freenode.net",
        port: 6667,
        user: "USER AffixBot AffixBot AffixBot :Affix IRC\r\n",
        nick: "AffixPyBot",
        channel: "#bottest"
    );

            ircBot.Start();
        }
    }
    public class IRCbot
    {
        // server to connect to (edit at will)
        private readonly string _server;
        // server port (6667 by default)
        private readonly int _port;
        // user information defined in RFC 2812 (IRC: Client Protocol) is sent to the IRC server 
        private readonly string _user;

        // the bot's nickname
        private readonly string _nick;
        // channel to join
        private readonly string _channel;

        private readonly int _maxRetries;

        
        public IRCbot(string server, int port, string user, string nick, string channel, int maxRetries = 3)
        {
            _server = server;
            _port = port;
            _user = user;
            _nick = nick;
            _channel = channel;
            _maxRetries = maxRetries;

        }

        public string FindName(string Input)
        {
            int i = 1;
            char[] myChar = Input.ToCharArray();
            while (myChar[i] != '!') {
                i++;
            }
            return Input.Substring(1,i-1);
        }


        public void Start()
        {
            var retry = false;
            var retryCount = 0;

            
            do
            {
                try
                {
                    using (var irc = new TcpClient(_server, _port))
                    using (var stream = irc.GetStream())
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))


                    {
                        writer.WriteLine("NICK " + _nick);
                        writer.Flush();
                        writer.WriteLine(_user);
                        writer.Flush();
                        bool shouldRun = true;

                        while (shouldRun)
                        {
                            string inputLine;
                            while ((inputLine = reader.ReadLine()) != null)
                            {
                                Console.WriteLine("<- " + inputLine);

                                // split the lines sent from the server by spaces (seems to be the easiest way to parse them)
                                string[] splitInput = inputLine.Split(new Char[] { ' ' });
                             

                                if (splitInput[0] == "PING")
                                {
                                    string PongReply = splitInput[1];
                                    //Console.WriteLine("->PONG " + PongReply);
                                    writer.WriteLine("PONG " + PongReply);
                                    writer.Flush();
                                    //continue;
                                }

                                switch (splitInput[1])
                                {
                                    case "001":
                                        writer.WriteLine("JOIN " + _channel);
                                        writer.Flush();
                                        break;
                                    case "PRIVMSG":
                                        string name = FindName(inputLine);
                                        Console.WriteLine("<- " + "PRIVMSG " + splitInput[2] + " :Hello");
                                        writer.WriteLine("PRIVMSG " + name + " :Hello");
                                        writer.Flush();
                                        break;
                                    case ":!quit":
                                        writer.WriteLine("JOIN " + _channel);
                                        writer.Flush();
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // shows the exception, sleeps for a little while and then tries to establish a new connection to the IRC server
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                    retry = ++retryCount <= _maxRetries;
                }
            } while (retry);
        }

    }
}
