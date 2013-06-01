using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Satel
{    
    public class Integra
    {
        public Dictionary<byte,Partition> partition { get; private set; }
        public Dictionary<byte, Objects> zone{ get; private set; }
        public Dictionary<byte, Output> output { get; private set; }
        public Dictionary<byte, User> users { get; private set; }

        string hardwareModel(int code)
        {
            switch (code)
            {
                case 0:
                    return "24";                    
                case 1:
                    return "32";
                case 2:
                    return "64";
                case 3:
                    return "128";
                case 4:
                    return "128-WRL SIM300";
                case 132:
                    return "128-WRL LEON";
                case 66:
                    return "64 PLUS";
                case 67:
                    return "128 PLUS";
                default:
                    return "UNKNOWN";                    
            }

        }

        //converts UserCode specified as string (1234) to format used by Integra (0x12 0x34)
        byte[] convertUserCode(string usercode,string prefix="")
        {
            List<byte> result=new List<byte>();
            var code = prefix + usercode;

            for (var i=0;i<code.Length; i+=2)
            {
                var b = (byte)(Convert.ToByte(code.Substring(i,1)) << 4);
                if (i <= (code.Length - 2))
                    b |= Convert.ToByte(code.Substring(i + 1, 1));
                else b |= 0x0F;
                result.Add(b);                       
            }

            // Integra expects either 4 bytes or 8 if prefix used.
            if (prefix == "" && result.Count < 4
                || prefix != "" && result.Count < 8)
                            for (var i = result.Count(); i < (prefix == "" ? 4 : 8); i++) result.Add(0xFF);
            return result.ToArray();
        }

       
        public void Setup(string host, int port, byte[] userCode)
        {
            Communication.integraAddress = host;
            Communication.integraPort = port;            
            Communication.userCode = userCode;

            partition=new Dictionary<byte,Partition>();
            zone = new Dictionary<byte, Objects>();
            output = new Dictionary<byte, Output>();
            users=new Dictionary<byte,User>();
        }

        //constructors
        //public Integra(string host, int port, byte[] userCode)
        //{
            //Setup(host, port, userCode);
        //}

        public Integra(string host, int port, string userCode)
        {        
            Setup(host, port, convertUserCode(userCode));           
        }



        public string getVersion()
        {
            try
            {
                var resp = Communication.sendCommand(0x7E);

                var result = "INTEGRA " + hardwareModel(resp[0]);
                result += " " + (char)resp[1] + "." + (char)resp[2] + (char)resp[3] + " " + (char)resp[4] + (char)resp[5] + (char)resp[6] + (char)resp[7];
                result += "-" + (char)resp[8] + (char)resp[9] + "-" + (char)resp[10] + (char)resp[11];
                result += " LANG: " + (resp[12] == 1 ? "English" : "Other");
                result += " SETTINGS: " + (resp[13] == 0xFF ? "stored" : "NOT STORED") + " in flash";
                return result;
            }
            catch
            {
                return "Communication failiure.";
            }
        }

        public void readPartitions()
        {            
            for (byte i = 1; i < 33; i++)
            {
                var resp = Communication.sendCommand(0xEE, 0x0, i);
                if (resp[3]!=0xfe) 
                    partition.Add(i, new Partition(i, Encoding.UTF8.GetString(resp, 3, 16),resp[2]));
            }
            Communication.closeConnection();
        }


        public void readZones()
        {
            for (byte i = 1; i < 129; i++)
            {
                var resp = Communication.sendCommand(0xEE, 0x5, i);
                if (resp[3] != 0xfe)                    
                    zone.Add(i, new Objects(partition[resp[19]],i, Encoding.UTF8.GetString(resp, 3, 16),resp[2]));
            }
            Communication.closeConnection();
        }

        public void readOutputs()
        {
            for (byte i = 1; i < 129; i++)
            {
                var resp = Communication.sendCommand(0xEE, 0x4, i);
                if (resp[2] != 0)
                    output.Add(i, new Output(i, Encoding.UTF8.GetString(resp, 3, 16),resp[2]));
            }
            Communication.closeConnection();

        }
        
        public void readUsers()
        {
            //for (byte i = 1; i <= 248; i++)
            for (byte i = 1; i<=10; i++)
            {
                var resp = Communication.sendAuthenticatedCommand(0xE1, i);
                if (resp.Length != 1 && resp[0] != 0x03)
                {
                    users.Add(i, new User(i, resp.Skip(1).Take(4).ToArray(), resp[5], resp[6], resp[7], resp.Skip(8).Take(3).ToArray(), Encoding.UTF8.GetString(resp, 11, 16)));
                }

            }
                

        }

    }
}
