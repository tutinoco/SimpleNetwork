using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public class SimpleNetworkBehaviour : UdonSharpBehaviour
    {
        public uint _id;
        private SimpleNetwork sn;

        private void _i() { SimpleNetworkInit(); }
        public void SimpleNetworkInit()
        {
            if( sn != null ) return;
            sn = SimpleNetwork.GetInstance();
            sn.SetBehaviour(this);
        }

        public virtual void ReceiveEvent(string name, string value) { }

        // string
        public void ExecEvent(string name, string value="", int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, value, delay); }
        public void SendEvent(string name, string value="", int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, value, delay); }
        public void SendEvent(SendTo sendto, string name, string value="", int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, value, delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, string value="", int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, value, delay); }

        // bool
        public void ExecEvent(string name, bool value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, _Bool(value), delay); }
        public void SendEvent(string name, bool value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, _Bool(value), delay); }
        public void SendEvent(SendTo sendto, string name, bool value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Bool(value), delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, bool value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Bool(value), delay); }
        public bool GetBool( string v ) { return v!="0"; }
        private string _Bool( bool v ) { return v?"1":"0"; }

        // int
        public void ExecEvent(string name, int value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, _Int(value), delay); }
        public void SendEvent(string name, int value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, _Int(value), delay); }
        public void SendEvent(SendTo sendto, string name, int value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Int(value), delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, int value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Int(value), delay); }
        public int GetInt( string v ) { return SimpleNetworkConverter.Base94ToInt(v); }
        private string _Int( int v ) { return SimpleNetworkConverter.ToBase94(v); }

        // long
        public void ExecEvent(string name, long value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, _Long(value), delay); }
        public void SendEvent(string name, long value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, _Long(value), delay); }
        public void SendEvent(SendTo sendto, string name, long value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Long(value), delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, long value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Long(value), delay); }
        public long GetLong( string v ) { return SimpleNetworkConverter.Base94ToLong(v); }
        private string _Long( long v ) { return SimpleNetworkConverter.ToBase94(v); }

        // float
        public void ExecEvent(string name, float value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, _Float(value), delay); }
        public void SendEvent(string name, float value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, _Float(value), delay); }
        public void SendEvent(SendTo sendto, string name, float value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Float(value), delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, float value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Float(value), delay); }
        public float GetFloat( string v ) { return float.Parse(v); }
        private string _Float( float v ) { return ""+v; }

        // Vector3
        public void ExecEvent(string name, Vector3 value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, _Vector3(value), delay); }
        public void SendEvent(string name, Vector3 value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, _Vector3(value), delay); }
        public void SendEvent(SendTo sendto, string name, Vector3 value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Vector3(value), delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, Vector3 value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Vector3(value), delay); }
        public Vector3 GetVector3( string v ) { string[] d=v.Split(','); return new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2])); }
        private string _Vector3( Vector3 v ) { return v.x+","+v.y+","+v.z; }

        // Object
        public void ExecEvent(string name, SimpleNetworkBehaviour value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.Self), name, _Object(value), delay); }
        public void SendEvent(string name, SimpleNetworkBehaviour value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(SendTo.All), name, _Object(value), delay); }
        public void SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Object(value), delay); }
        public void SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay=0) { _i(); sn.SetEvent(this, sn.ToInt(sendto), name, _Object(value), delay); }
        public SimpleNetworkBehaviour GetObject( string v ) { return sn.GetBehaviour(SimpleNetworkConverter.Base94ToInt(v)); }
        private string _Object( SimpleNetworkBehaviour v ) { return SimpleNetworkConverter.ToBase94(v._id); }
    }
}