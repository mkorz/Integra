using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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

    public class Objects : Device
    {
        public Partition partition { get; private set; }
        public Objects(Partition partition, byte id, string name, byte type): base (id,name,type)
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

    
    public class User
    {

        public struct UserType
        {
            public enum Type
            {
                Normal=0,
                Single=1,
                TimeRenewable=2,
                TimeNotRenewable=3,
                Duress=4,
                MonoOutputs=5,
                BiOutputs=6,
                ParitionTemporaryBlocking=7,
                AccessToCashMachine=8,
                Guard=9,
                Schedule=10
            }

            private byte _type;
            public bool changedCode { get { return (_type & 0x80)==0; } }
            public bool reusedCode { get { return (_type & 0x40) == 40; } }
            public Type type { get { return (Type)(_type & 0x0F); } set { _type |= (byte) (0x0F & (byte) value); } }
            public UserType(byte type) {
                _type = type;
            }
        }


        public struct UserPermissions
        {
            private byte[] _permissions;
            public bool Arming { get { return (_permissions[0] & 0x01) == 0x01; } set { if (value) _permissions[0] |= (byte)1; }}
            public bool Disarming { get { return (_permissions[0] & 0x02) == 0x02; } set { if (value) _permissions[0] |= (byte)2; } }
            public bool AlarmClearingOwnPartition { get { return (_permissions[0] & 0x04) == 0x04; } set { if (value) _permissions[0] |= (byte)4; } }
            public bool AlarmClearingOwnObject { get { return (_permissions[0] & 0x08) == 0x08; } set { if (value) _permissions[0] |= (byte)8; } }
            public bool AlarmClearing { get { return (_permissions[0] & 0x10) == 0x10; } set { if (value) _permissions[0] |= (byte)0x10; } }
            public bool ArmDefering { get { return (_permissions[0] & 0x20) == 0x20; } set { if (value) _permissions[0] |= (byte)0x20; } }
            public bool CodeChanging { get { return (_permissions[0] & 0x40) == 0x40; } set { if (value) _permissions[0] |= (byte)0x40; } }
            public bool UsersEditing { get { return (_permissions[0] & 0x80) == 0x80; } set { if (value) _permissions[0] |= (byte)0x80; } }

            public bool ZonesBypassing { get { return (_permissions[1] & 0x01) == 0x01; } set { if (value) _permissions[1] |= (byte)1; } }
            public bool ClockSettings { get { return (_permissions[1] & 0x02) == 0x02; } set { if (value) _permissions[1] |= (byte)2; } }
            public bool TroublesViewing { get { return (_permissions[1] & 0x04) == 0x04; } set { if (value) _permissions[1] |= (byte)4; } }
            public bool EventsViewing { get { return (_permissions[1] & 0x08) == 0x08; } set { if (value) _permissions[1] |= (byte)8; } }
            public bool ZonesResetting { get { return (_permissions[1] & 0x10) == 0x10; } set { if (value) _permissions[1] |= (byte)0x10; } }
            public bool OptionsChanging { get { return (_permissions[1] & 0x20) == 0x20; } set { if (value) _permissions[1] |= (byte)0x20; } }
            public bool Tests { get { return (_permissions[1] & 0x40) == 0x40; } set { if (value) _permissions[1] |= (byte)0x30; } }
            public bool Downloading { get { return (_permissions[1] & 0x80) == 0x80; } set { if (value) _permissions[1] |= (byte)0x80; } }

            public bool CanAlwaysDisarm { get { return (_permissions[2] & 0x01) == 0x01; } set { if (value) _permissions[2] |= (byte)1; } }
            public bool VoiceMessageClearing { get { return (_permissions[2] & 0x02) == 0x02; } set { if (value) _permissions[2] |= (byte)2; } }        
            public bool GuardX { get { return (_permissions[2] & 0x01) == 0x04; } set { if (value) _permissions[2] |= (byte)4;} }
            public bool AccessToTemporaryBlockedPartitions { get { return (_permissions[2] & 0x08) == 0x08; } set { if (value) _permissions[2] |= (byte)8;}}
            public bool Entering1stCode { get { return (_permissions[2] & 0x10) == 0x10; }set { if (value) _permissions[2] |= (byte)0x10; }}
            public bool Entering2ndCode { get { return (_permissions[2] & 0x20) == 0x20; } set { if (value) _permissions[2] |= (byte)0x20;}}
            public bool OutputsControl { get { return (_permissions[2] & 0x40) == 0x40; } set { if (value) _permissions[2] |= (byte)0x40;}}
            public bool ClearingLatchedOutputs { get { return (_permissions[2] & 0x80) == 0x80; } set { if (value) _permissions[2] |= (byte)0x80;}}
            public UserPermissions(byte[] permissions)
            {
                _permissions = permissions;
            }

        }

        public class ProximityCard
        {
            private User user;
            private byte[] _number;

            public byte[] number
            {
                get
                {
                    if (_number != null) return _number;
                    else
                    {
                        var resp = Communication.sendAuthenticatedCommand(0xE8, 0x31, (byte)user.id);
                        _number = resp.Skip(2).Take(5).ToArray();
                        return _number;
                    }
                }

                set {                    
                    var resp=Communication.sendCommand(0xE8, 0x32, (byte)user.id, value[0], value[1], value[2],value[3], value[4]);
                    _number=value;
                }
            }

            public byte[] refresh() {
                _number = null;
                return number;   
            }

            public override string ToString()
            {
                return Encoding.UTF8.GetString(number);
                /*    if (_number != null)
                    return Encoding.UTF8.GetString(_number);
                else
                {
                    return "";
                }*/
            }

            public ProximityCard(User user)
            {
                this.user = user;
            }
        }


            public byte id { get; private set; }
            public UserType type { get; private set; }
            public byte time { get; private set; }
            public byte timetmp { get; private set; }
            public byte[] rights { get; private set; }
            public string name { get; private set; }
            public ProximityCard card { get; private set; }
            public byte[] partitions { get; private set; }


            public User(byte id, byte[] partitions, byte type, byte time, byte timetmp, byte[] rights, string name)
            {
                this.id = id;
                this.type = new User.UserType(type);                
                this.time = time;
                this.timetmp = timetmp;
                this.rights = rights;
                this.name = name;
                this.partitions = partitions;
                this.card = new ProximityCard(this);
            }

            



        }
        



    }
  




