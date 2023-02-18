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

        private void SetEvent(bool isSync, string name, string value, int delay )
        {
            if( sn == null ) SimpleNetworkInit();
            sn.SetEvent(isSync, delay, this, name, value);
        }

        // string
        public void ExecEvent(string name, string value="", int delay=0) { SetEvent(false, name, value, delay); }
        public void SendEvent(string name, string value="", int delay=0) { SetEvent(true, name, value, delay); }

        // bool
        public void ExecEvent(string name, bool value, int delay=0) { SetEvent(false, name, _Bool(value), delay); }
        public void SendEvent(string name, bool value, int delay=0) { SetEvent(true, name, _Bool(value), delay); }
        public static bool GetBool( string v ) { return v!="0"; }
        private string _Bool( bool v ) { return v?"1":"0"; }

        // int        
        public void ExecEvent(string name, int value, int delay=0) { SetEvent(false, name, _Int(value), delay); }
        public void SendEvent(string name, int value, int delay=0) { SetEvent(true, name, _Int(value), delay); }
        public static int GetInt( string v ) { return int.Parse(v); }
        private string _Int( int v ) { return ""+v; }

        // float
        public void ExecEvent(string name, float value, int delay=0) { SetEvent(false, name, _Float(value), delay); }
        public void SendEvent(string name, float value, int delay=0) { SetEvent(true, name, _Float(value), delay); }
        public static float GetFloat( string v ) { return float.Parse(v); }
        private string _Float( float v ) { return ""+v; }

        // Vector3
        public void ExecEvent(string name, Vector3 value, int delay=0) { SetEvent(false, name, _Vector3(value), delay); }
        public void SendEvent(string name, Vector3 value, int delay=0) { SetEvent(true, name, _Vector3(value), delay); }
        public static Vector3 GetVector3( string v ) { string[] d=v.Split(','); return new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2])); }
        private string _Vector3( Vector3 v ) { return v.x+","+v.y+","+v.z; }
    }
}