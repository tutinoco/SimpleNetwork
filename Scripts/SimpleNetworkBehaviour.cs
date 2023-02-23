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
        public int _id;
        private SimpleNetwork sn;
        private Object[] obj;

        private void I() { SimpleNetworkInit(); }
        public void SimpleNetworkInit()
        {
            if( sn != null ) return;
            sn = SimpleNetwork.GetInstance();
            sn.SetBehaviour(this);
        }

        public void _ReceiveEvent(Object[] evObj) {
            obj = evObj;
            string name = (string)evObj[2];
            var value = evObj[3];
            ReceiveEvent(name);

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

        // meta
        public int GetDelay() { return (int)obj[4]; }
        public SimpleNetworkBehaviour GetSource() { return (SimpleNetworkBehaviour)obj[0]; }

        // bool
        public void ExecEvent(string name, bool value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, bool value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, bool value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, bool value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public bool GetBool() { return (bool)obj[3]; }

        // int
        public void ExecEvent(string name, int value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, int value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, int value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, int value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public int GetInt() { return (int)obj[3]; }

        // float
        public void ExecEvent(string name, float value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, float value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, float value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, float value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public float GetFloat() { return (float)obj[3]; }

        // string
        public void ExecEvent(string name, string value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, string value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, string value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, string value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public string GetString() { return (string)obj[3]; }

        // Vector3
        public void ExecEvent(string name, Vector3 value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.Self), name, value, delay}); }
        public void SendEvent(string name, Vector3 value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(SendTo.All), name, value, delay}); }
        public void SendEvent(SendTo sendto, string name, Vector3 value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public void SendEvent(VRCPlayerApi sendto, string name, Vector3 value, int delay=0) { I(); sn.SetEvent(new Object[]{this, sn.ToInt(sendto), name, value, delay}); }
        public Vector3 GetVector3() { return (Vector3)obj[3]; }
    }
}
