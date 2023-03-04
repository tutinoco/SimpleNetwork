using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using Object = System.Object;

namespace tutinoco
{
    public class SimpleNetworkBehaviour : UdonSharpBehaviour
    {
        [System.NonSerialized] public int _id;
        [System.NonSerialized] public SimpleNetwork _sn;
        private Object[] obj;
        private string err = "SimpleNetworkError: Could not find {0} in received value.";

        private void I() { SimpleNetworkInit(); }
        public void SimpleNetworkInit()
        {
            if( _sn != null ) return;
            _sn = SimpleNetwork.GetInstance();
            _sn.SetBehaviour(this);
        }

        public void ReceivesEvent(Object[] evObj) {
            obj = evObj;
            string name = (string)evObj[(int)EvObj.Name];
            var value = evObj[(int)EvObj.Value];
            ReceiveEvent(name);

            if( value == null ) return;

            Type t = value.GetType();
            if( t == typeof(bool) ) ReceiveEvent(name, (bool)value);
            else if( t == typeof(char) ) ReceiveEvent(name, (char)value);
            else if( t == typeof(byte) ) ReceiveEvent(name, (byte)value);
            else if( t == typeof(sbyte) ) ReceiveEvent(name, (sbyte)value);
            else if( t == typeof(short) ) ReceiveEvent(name, (short)value);
            else if( t == typeof(ushort) ) ReceiveEvent(name, (ushort)value);
            else if( t == typeof(int) ) ReceiveEvent(name, (int)value);
            else if( t == typeof(uint) ) ReceiveEvent(name, (uint)value);
            else if( t == typeof(long) ) ReceiveEvent(name, (long)value);
            else if( t == typeof(ulong) ) ReceiveEvent(name, (ulong)value);
            else if( t == typeof(float) )  ReceiveEvent(name, (float)value);
            else if( t == typeof(double) )  ReceiveEvent(name, (double)value);
            else if( t == typeof(Vector2) ) ReceiveEvent(name, (Vector2)value);
            else if( t == typeof(Vector3) ) ReceiveEvent(name, (Vector3)value);
            else if( t == typeof(Vector4) ) ReceiveEvent(name, (Vector4)value);
            else if( t == typeof(Quaternion) ) ReceiveEvent(name, (Quaternion)value);
            else if( t == typeof(string) ) ReceiveEvent(name, (string)value);
            else if( t == typeof(VRCUrl) ) ReceiveEvent(name, (VRCUrl)value);
            else if( t == typeof(Color) ) ReceiveEvent(name, (Color)value);
            else if( t == typeof(Color32) ) ReceiveEvent(name, (Color32)value);
            else if( t == typeof(Object[]) ) ReceiveEvent(name, (Object[])value);
        }

        public virtual void ReceiveEvent(string name) { }
        public virtual void ReceiveEvent(string name, bool value) { }
        public virtual void ReceiveEvent(string name, char value) { }
        public virtual void ReceiveEvent(string name, byte value) { }
        public virtual void ReceiveEvent(string name, sbyte value) { }
        public virtual void ReceiveEvent(string name, short value) { }
        public virtual void ReceiveEvent(string name, ushort value) { }
        public virtual void ReceiveEvent(string name, int value) { }
        public virtual void ReceiveEvent(string name, uint value) { }
        public virtual void ReceiveEvent(string name, long value) { }
        public virtual void ReceiveEvent(string name, ulong value) { }
        public virtual void ReceiveEvent(string name, float value) { }
        public virtual void ReceiveEvent(string name, double value) { }
        public virtual void ReceiveEvent(string name, Vector2 value) { }
        public virtual void ReceiveEvent(string name, Vector3 value) { }
        public virtual void ReceiveEvent(string name, Vector4 value) { }
        public virtual void ReceiveEvent(string name, Quaternion value) { }
        public virtual void ReceiveEvent(string name, string value) { }
        public virtual void ReceiveEvent(string name, VRCUrl value) { }
        public virtual void ReceiveEvent(string name, Color value) { }
        public virtual void ReceiveEvent(string name, Color32 value) { }
        public virtual void ReceiveEvent(string name, Object[] value) { }

        public Object[] Pack(params Object[] args) { return (Object[])args.Clone(); }

        public bool IsMaster() { return Networking.IsMaster; }
        public bool IsOwner( SimpleNetworkBehaviour b=null ) { b=b??this; return Networking.IsOwner(b.gameObject); }
        public bool IsMe( VRCPlayerApi p ) { return Networking.LocalPlayer == p; }

        private int FindValue( Object[] values, Type type, int idx )
        {
            for(int i=idx; i<values.Length; i++) if( values[i].GetType() == type ) return i;
            return -1;
        }

        public virtual void OnDuplicateComplete( SimpleNetworkBehaviour behaviour ) { }
        public void Duplicate( SimpleNetworkBehaviour behaviour, Vector3 position, Quaternion rotation )
        {
            RequestEvent(RequestTo.Master, "__DUPLICATE__", Pack(behaviour, position, rotation));
        }

        // meta
        public int GetDelay() { return (int)obj[(int)EvObj.Delay]; }
        public object[] GetValues() { var v=obj[(int)EvObj.Value]; return v==null?new Object[0]:(v.GetType()==typeof(Object[])?(Object[])v:new Object[]{v}); }
        public SimpleNetworkBehaviour GetSource() { return (SimpleNetworkBehaviour)obj[(int)EvObj.Source]; }

        // void
        public void ExecEvent(string name) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, null, 0}); }
        public void SendEvent(string name) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, null, 0}); }

        // bool
        public void ExecEvent(string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public bool GetBool(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(bool),i); if(i==-1)Debug.LogError(string.Format(err,"bool")); return (bool)v[i]; }

        // int
        public void ExecEvent(string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public int GetInt(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(int),i); if(i==-1)Debug.LogError(string.Format(err,"int")); return (int)v[i]; }

        // float
        public void ExecEvent(string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public float GetFloat(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(float),i); if(i==-1)Debug.LogError(string.Format(err,"float")); return (float)v[i]; }

        // string
        public void ExecEvent(string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public string GetString(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(string),i); if(i==-1)Debug.LogError(string.Format(err,"string")); return (string)v[i]; }

        // Vector3
        public void ExecEvent(string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public Vector3 GetVector3(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(Vector3),i); if(i==-1)Debug.LogError(string.Format(err,"Vector3")); return (Vector3)v[i]; }

        // Quaternion
        public void ExecEvent(string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public Quaternion GetQuaternion(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(Quaternion),i); if(i==-1)Debug.LogError(string.Format(err,"Quaternion")); return (Quaternion)v[i]; }

        // Behaviour
        public void ExecEvent(string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public SimpleNetworkBehaviour GetBehaviour(int i=0) {  Object[] v=GetValues(); i=FindValue(v,typeof(UdonSharpBehaviour),i); if(i==-1)Debug.LogError(string.Format(err,"SimpleNetworkBehaviour")); return (SimpleNetworkBehaviour)v[i]; }

        // Object[]
        public void ExecEvent(string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(RequestTo.None), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(RequestTo request, SendTo sendto, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(SendTo.All), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
        public void RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Object[] value, int delay=0) { I(); _sn.AddEvent(new Object[]{this, _sn.ToInt(request), _sn.ToInt(sendto), name, value, delay}); }
    }
}
