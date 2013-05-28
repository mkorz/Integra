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
            Satel.Integra i = new Satel.Integra("xxxxxxxx",7094);

            //i.test();
            //Console.WriteLine(i.getVersion());
            i.readPartitions();
            i.readZones();
            i.readOutputs();



        }
    }
}
