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

        public void SimpleNetworkInit()
        {
            if( sn != null ) return;
            sn = SimpleNetwork.GetInstance();
            sn.Register(this);
        }

        public virtual void OnSimpleNetworkReady() { }

        public virtual void ReceiveEvent(string name, string value) { }

        private void SetEvent(SimpleNetworkTarget target, string name, string value, int delay )
        {
            if( sn == null ) SimpleNetworkInit();
            sn.SetEvent(this, target, name, value, delay);
        }

        private void SetEvent(int target, string name, string value, int delay )
        {
            if( sn == null ) SimpleNetworkInit();
            sn.SetEvent(this, target, name, value, delay);
        }

        // string
        public void ExecEvent(string name, string value="", int delay=0) { SetEvent(SimpleNetworkTarget.Self, name, value, delay); }
        public void SendEvent(string name, string value="", int delay=0) { SetEvent(SimpleNetworkTarget.All, name, value, delay); }
        public void SendEvent(SimpleNetworkTarget target, string name, string value="", int delay=0) { SetEvent(target, name, value, delay); }
        public void SendEvent(int target, string name, string value="", int delay=0) { SetEvent(target, name, value, delay); }

        // bool
        public void ExecEvent(string name, bool value, int delay=0) { SetEvent(SimpleNetworkTarget.Self, name, _Bool(value), delay); }
        public void SendEvent(string name, bool value, int delay=0) { SetEvent(SimpleNetworkTarget.All, name, _Bool(value), delay); }
        public void SendEvent(SimpleNetworkTarget target, string name, bool value, int delay=0) { SetEvent(target, name, _Bool(value), delay); }
        public void SendEvent(int target, string name, bool value, int delay=0) { SetEvent(target, name, _Bool(value), delay); }
        public static bool GetBool( string v ) { return v!="0"; }
        private string _Bool( bool v ) { return v?"1":"0"; }

        // int
        public void ExecEvent(string name, int value, int delay=0) { SetEvent(SimpleNetworkTarget.Self, name, _Int(value), delay); }
        public void SendEvent(string name, int value, int delay=0) { SetEvent(SimpleNetworkTarget.All, name, _Int(value), delay); }
        public void SendEvent(SimpleNetworkTarget target, string name, int value, int delay=0) { SetEvent(target, name, _Int(value), delay); }
        public void SendEvent(int target, string name, int value, int delay=0) { SetEvent(target, name, _Int(value), delay); }
        public static int GetInt( string v ) { return int.Parse(v); }
        private string _Int( int v ) { return ""+v; }

        // float
        public void ExecEvent(string name, float value, int delay=0) { SetEvent(SimpleNetworkTarget.Self, name, _Float(value), delay); }
        public void SendEvent(string name, float value, int delay=0) { SetEvent(SimpleNetworkTarget.All, name, _Float(value), delay); }
        public void SendEvent(SimpleNetworkTarget target, string name, float value, int delay=0) { SetEvent(target, name, _Float(value), delay); }
        public void SendEvent(int target, string name, float value, int delay=0) { SetEvent(target, name, _Float(value), delay); }
        public static float GetFloat( string v ) { return float.Parse(v); }
        private string _Float( float v ) { return ""+v; }

        // Vector3
        public void ExecEvent(string name, Vector3 value, int delay=0) { SetEvent(SimpleNetworkTarget.Self, name, _Vector3(value), delay); }
        public void SendEvent(string name, Vector3 value, int delay=0) { SetEvent(SimpleNetworkTarget.All, name, _Vector3(value), delay); }
        public void SendEvent(SimpleNetworkTarget target, string name, Vector3 value, int delay=0) { SetEvent(target, name, _Vector3(value), delay); }
        public void SendEvent(int target, string name, Vector3 value, int delay=0) { SetEvent(target, name, _Vector3(value), delay); }
        public static Vector3 GetVector3( string v ) { string[] d=v.Split(','); return new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2])); }
        private string _Vector3( Vector3 v ) { return v.x+","+v.y+","+v.z; }
    }
}