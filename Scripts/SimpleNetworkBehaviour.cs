
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
        private char rs = (char)0x1E;

        private string[] delayDatas = new string[0];
        private int[] delayCounts = new int[0];

        public void SimpleNetworkInit()
        {
            sn = SimpleNetwork.GetInstance();
            sn.Register(this);
        }

        private void SetRecordDelayed(int delay, bool isSync, string name, string value, string target)
        {
            for (int i = 0; i < delayCounts.Length; i++) {
                if (delayCounts[i] >= 0) continue;
                delayCounts[i] = delay;
                delayDatas[i] = (isSync?"1":"0")+rs+name+rs+value+rs+target;
                return;
            }

            int[] temp = new int[delayCounts.Length+1];
            delayCounts.CopyTo(temp, 0);
            temp[temp.Length-1] = delay;
            delayCounts = temp;

            string[] temp2 = new string[delayDatas.Length+1];
            delayDatas.CopyTo(temp2, 0);
            temp2[temp2.Length-1] = (isSync?"1":"0")+rs+name+rs+value+rs+target;
            delayDatas = temp2;
        }

        private void SetRecord(bool isSync, string name, string value, string target)
        {
            if( sn==null || !sn.IsReady() ) { SetRecordDelayed(0, isSync, name, value, target); return; }
            if( target == "" ) target = _id.ToString("X");
            string record = sn.CreateRecord(name, value, target);
            if( isSync ) sn.SendRecord(record);
            else sn.ExecRecord(record);
        }

        public void _Update()
        {
            for (int i = 0; i < delayCounts.Length; i++) {
                if( delayCounts[i] == 0 ) {
                    string[] d = delayDatas[i].Split(rs);
                    SetRecord(d[0]!="0", d[1], d[2], d[3]);
                }
                if( delayCounts[i] >= 0 ) delayCounts[i]--;
            }
        }

        public virtual void OnSimpleNetworkReady() { }
        public virtual void ReceiveEvent(string name, string value) { }

        // string
        public void ExecEvent(string name, string value="", string target="") { SetRecord(false, name, value, target); }
        public void SendEvent(string name, string value="", string target="") { SetRecord(true, name, value, target); }

        // bool
        public void ExecEvent(string name, bool value, string target="") { SetRecord(false, name, _Bool(value), target); }
        public void SendEvent(string name, bool value, string target="") { SetRecord(true, name, _Bool(value), target); }
        public static bool GetBool( string v ) { return v!="0"; }
        private string _Bool( bool v ) { return v?"1":"0"; }

        // int        
        public void ExecEvent(string name, int value, string target="") { SetRecord(false, name, _Int(value), target); }
        public void SendEvent(string name, int value, string target="") { SetRecord(true, name, _Int(value), target); }
        public static int GetInt( string v ) { return int.Parse(v); }
        private string _Int( int v ) { return ""+v; }

        // float
        public void ExecEvent(string name, float value, string target="") { SetRecord(false, name, _Float(value), target); }
        public void SendEvent(string name, float value, string target="") { SetRecord(true, name, _Float(value), target); }
        public static float GetFloat( string v ) { return float.Parse(v); }
        private string _Float( float v ) { return ""+v; }

        // Vector3
        public void ExecEvent(string name, Vector3 value, string target="") { SetRecord(false, name, _Vector3(value), target); }
        public void SendEvent(string name, Vector3 value, string target="") { SetRecord(true, name, _Vector3(value), target); }
        public static Vector3 GetVector3( string v ) { string[] d=v.Split(','); return new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2])); }
        private string _Vector3( Vector3 v ) { return v.x+","+v.y+","+v.z; }
    }
}

