using System;
using System.Collections.Generic;
using System.Linq;


namespace Satel
{
    static class Communication
    {
        static public string integraAddress { get; set; }
        static public int integraPort { get; set; }
        static public byte[] userCode { get; set; }


        static private System.Net.Sockets.TcpClient _tcpClient;
        static private System.IO.Stream _stream;


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
            _tcpClient = new System.Net.Sockets.TcpClient(integraAddress, integraPort);
            _stream = _tcpClient.GetStream();
        }

        static public void closeConnection()
        {
            _stream.Close();
            _tcpClient.Close();
        }

       static public byte[] sendCommand(params byte[] command)
        {

            var buffer = new byte[128];
            var attempt = 0;
            int readBytes;
            do
            {
                attempt++;
                var send = new List<byte>();
                if (_tcpClient==null || !_tcpClient.Connected) 
                    openConnection();
               
                _stream.Write(new byte[] { 0xFE, 0xFE }, 0, 2);
                foreach (var b in command)
                {
                    send.Add(b);
                    if (b == 0xFE) send.Add(0xF0);
                }
                _stream.Write(send.ToArray(), 0, send.Count());
           
                send.Clear();
                foreach (var b in checksum(command))
                {
                    send.Add(b);
                    if (b == 0xFE) send.Add(0xF0);
                }
                _stream.Write(send.ToArray(), 0, send.Count());
                _stream.Write(new byte[] { 0xFE, 0x0D }, 0, 2);
                var ts = DateTime.Now.Ticks;                                     
                readBytes=_stream.Read(buffer, 0, 128);

                Console.WriteLine("Response received in {0} ms.", (DateTime.Now.Ticks - ts)/10000);
            } while ((buffer[0] != buffer[1] || buffer[0] != 0xFE) && attempt<3);
           
            var response=new List<byte>();
            for (var i = 3; i < readBytes - 4; i++)
            {
                response.Add(buffer[i]);
                if (buffer[i] == 0xFE && buffer[i + 1] == 0xF0) i++;
            }
            //return buffer.Skip(3).Take(readBytes- 4).ToArray();
            return response.ToArray();
        }
     
        static public byte[] sendAuthenticatedCommand(byte command, params byte[] arguments)
        {
            var parameters = new byte[arguments.Length+1+userCode.Length];
            parameters[0] = command;
            Array.Copy(userCode, 0, parameters, 1, userCode.Length);
            Array.Copy(arguments, 0, parameters, 1 + userCode.Length, arguments.Length);
            return sendCommand(parameters);
        }

    }
}
