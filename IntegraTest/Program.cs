using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IntegraTest
{
    class Program
    {
        static void Main(string[] args)
        {                 
            Satel.Integra i = new Satel.Integra(Properties.Settings.Default.IntegraIP,
                                                Properties.Settings.Default.IntegraPort,
                                                Properties.Settings.Default.UserCode);
            

           
            //Console.WriteLine(i.getVersion());
            //i.readPartitions();
            //i.readZones();
            //i.readOutputs();
            i.readUsers();
            // read user #1 card number
            //Console.WriteLine(i.users[1].card.ToString());
        }
    }
}
