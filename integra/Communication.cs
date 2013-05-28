using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satel
{
    static class Communication
    {
        static public string integraAddress { get; set; }
        static public int integraPort { get; set; }

        static private System.Net.Sockets.TcpClient tcpClient;
        static private System.IO.Stream stream;


        static byte[] checksum(byte[] command)
        {
            var crc = 0x147a;
            foreach (var b in command)
            {
                var bit = (crc & 0x8000) >> 15;
                crc = ((crc << 1) & 0xFFFF) | bit;
                crc = crc ^ 0xFFFF;
                crc = crc + (crc >> 8) + b;
            }
            return new byte[] { Convert.ToByte((crc >> 8) & 0xFF), Convert.ToByte(crc & 0xFF) };
}

        static public void openConnection()
        {
            tcpClient = new System.Net.Sockets.TcpClient(integraAddress, integraPort);
            stream = tcpClient.GetStream();
        }

        static public void closeConnection()
        {
            stream.Close();
            tcpClient.Close();
        }

       static public byte[] sendCommand(byte[] command)
        {
            byte[] buffer = new byte[128];
            int attempt = 0;
            do
            {
                attempt++;
                List<byte> send = new List<byte>();
                if (tcpClient==null || !tcpClient.Connected) 
                    openConnection();
               
                stream.Write(new byte[] { 0xFE, 0xFE }, 0, 2);
                foreach (var b in command)
                {
                    send.Add(b);
                    if (b == 0xFE) send.Add(0xF0);
                }
                stream.Write(send.ToArray(), 0, send.Count());
           
                send.Clear();
                foreach (var b in checksum(command))
                {
                    send.Add(b);
                    if (b == 0xFE) send.Add(0xF0);
                }
                stream.Write(send.ToArray(), 0, send.Count());
                stream.Write(new byte[] { 0xFE, 0x0D }, 0, 2);
                var ts = DateTime.Now.Ticks;                                     
                stream.Read(buffer, 0, 128);
                Console.WriteLine("Response received in {0} ms.", (DateTime.Now.Ticks - ts)/10000);
            } while ((buffer[0] != buffer[1] || buffer[0] != 0xFE) && attempt<3);
            return buffer.Skip(3).Take(buffer.Length - 4).ToArray();
        }

       static public byte[] sendCommand(byte b1)
        {
            return sendCommand(new byte[] { b1 });
        }

       static public byte[] sendCommand(byte b1, byte b2)
        {
            return sendCommand(new byte[] { b1, b2 });
        }

       static public byte[] sendCommand(byte b1, byte b2, byte b3)
       {
           return sendCommand(new byte[] { b1, b2,b3 });
       }


    }
}
