using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satel
{

    public class Device 
    {
        public byte id {get; private set;}
        public string name {get; private set;}
        public byte type { get; private set; }

        public Device(byte id, string name, byte type)
        {
            this.id=id;
            this.name=name;
            this.type = type;

        }

    }
    
    public class Partition : Device
    {
        public Partition(byte id, string name, byte type)
            : base(id, name,type)
        {
        }
    }

    public class Zone : Device
    {
        public Partition partition { get; private set; }
        public Zone(Partition partition, byte id, string name, byte type): base (id,name,type)
        {
            this.partition = partition;
        }

    }


    public class Output : Device
    {        
        public Output(byte id, string name, byte type) : base(id, name, type)
        {            
        }
    }

  



}
