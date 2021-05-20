using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WSV1._0
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("기본포트번호는 8787 입니다.");
            new Program().Server();
        }
        void Server()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 8787);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipep);
            server.Listen(20);

            while (true)
            {
                Socket client = server.Accept();
                try
                {
                    String file = Recieve(client);
                    FileInfo FI = new FileInfo(file);
                    client.Send(Header(client, FI));
                }
                catch { }
                finally
                {
                    client.Close();
                }
            }
        }
        public String Recieve(Socket client)
        {
            String data_str = "";
            byte[] data = new byte[4096];
            client.Receive(data);
            Console.WriteLine(Encoding.Default.GetString(data).Trim('\0'));
            String[] buf = Encoding.Default.GetString(data).Split("\r\n".ToCharArray());

            if (buf[0].IndexOf("GET") != -1)
            {
                data_str = buf[0].Replace("GET", "").Replace("HTTP/1.1", "").Trim();
            }

            else
            {
                data_str = buf[0].Replace("POST ", "").Replace("HTTP/1.1", "").Trim();
            }

            if (data_str.Trim() == "/")
            {
                data_str += "main.html";
            }

            int pos = data_str.IndexOf("?");
            if (pos > 0)
            {
                data_str = data_str.Remove(pos);
            }
            return "web" + data_str;
        }

        public byte[] Header(Socket client, FileInfo FI)
        {
            byte[] data2 = new byte[FI.Length];
            try
            {
                FileStream FS = new FileStream(FI.FullName, FileMode.Open, FileAccess.Read);

                FS.Read(data2, 0, data2.Length);
                FS.Close();

                String buf = "HTTP/1.0 200 ok\r\n";
                buf += "Data: " + FI.CreationTime.ToString() + "\r\n";
                buf += "server: Myung server\r\n";
                buf += "Content-Length: " + data2.Length.ToString() + "\r\n";
                buf += "content-type:text/html\r\n";
                buf += "\r\n";
                client.Send(Encoding.Default.GetBytes(buf));
            }
            catch
            {
                String buf = "HTTP/1.0 100 BedRequest ok\r\n";
                buf += "server: Myung server\r\n";
                buf += "content-type:text/html\r\n";
                buf += "\r\n";
                client.Send(Encoding.Default.GetBytes(buf));
                data2 = Encoding.Default.GetBytes("Bed Request");

            }
            return data2;
        }
    }
}
