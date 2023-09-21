using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public class SimpleNetworkBehaviour : UdonSharpBehaviour
    {
        [System.NonSerialized] public int _id;
        [System.NonSerialized] public SimpleNetwork _sn;
        [System.NonSerialized] public string _group;
        [System.NonSerialized] public int _groupIndex;
        private object[] obj;
        private string err = "SimpleNetworkError: Could not find {0} in received value.";
        public object[] none { get; } = (object[])null;
        protected VRCPlayerApi me { get { return Networking.LocalPlayer; } }

        public void SimpleNetworkInit( string group="" )
        {
            if( _sn == null ) {
                _sn = SimpleNetwork.GetInstance();
                _sn.SetBehaviour(this);
            }
            if( group != "" ) SetGroupName(group);
        }

        public object[] Pack(params object[] args) { return (object[])args.Clone(); }

        public bool IsMaster() { return Networking.IsMaster; }
        public bool IsOwner( SimpleNetworkBehaviour b=null ) { b=b??this; return Networking.IsOwner(b.gameObject); }
        public bool IsMe( VRCPlayerApi p ) { return Networking.LocalPlayer == p; }

        public SimpleNetworkBehaviour[] GetBehaviours() { return _sn.GetBehaviours(); }
        public SimpleNetworkBehaviour[] GetBehaviours( string group ) { return _sn.GetBehaviours(group); }

        public void Duplicate( SimpleNetworkBehaviour behaviour, Vector3 position ) { Duplicate(behaviour, position, Quaternion.identity); }
        public void Duplicate( SimpleNetworkBehaviour behaviour, Vector3 position, Quaternion rotation )
        {
            RequestEvent(RequestTo.Server, "DUPLICATE", Pack(behaviour, position, (Quaternion)rotation));
        }

        public string GetGroupName() { return _group; }
        public void SetGroupName( string group )
        {
            string charsToRemove = "?*|&";
            foreach (char c in charsToRemove) group = group.Replace(c.ToString(), "");
            _group = group;
        }

        private object[] E(params object[] args)
        {
            SimpleNetworkInit();
            object[] evObj = (object[])args.Clone();

            var request = evObj[(int)EvObj.RequestTo];
            evObj[(int)EvObj.RequestTo] = request.GetType()==typeof(VRCPlayerApi) ? ((VRCPlayerApi)request).playerId+(int)RequestTo.Length : (int)request;

            var sendto = evObj[(int)EvObj.SendTo];
            if( sendto.GetType()==typeof(VRCPlayerApi) ) evObj[(int)EvObj.SendTo] = ((VRCPlayerApi)sendto).playerId+(int)SendTo.Length;
            else {
                if( (int)sendto == (int)SendTo.Me ) evObj[(int)EvObj.SendTo] = Networking.LocalPlayer.playerId+(int)SendTo.Length;
                else evObj[(int)EvObj.SendTo] = (int)sendto;
            }

            return evObj;
        }

        private object[] R(object[] evObj)
        {
            _sn.AddEvent(evObj);
            return evObj;
        }

        private int I( Type type, int start, object[] values ) { return IndexOf(type, start, values); }
        public int IndexOf( Type type, int start=0, object[] values=null )
        {
            if( values == null ) values = GetValues();
            for(int i=start; i<values.Length; i++) if( values[i].GetType() == type ) return i;
            return -1;
        }

        public virtual void OnSimpleNetworkReady() { }
        public virtual void OnChangeGroupName( bool global ) { }
        public virtual void OnDuplicateComplete( SimpleNetworkBehaviour behaviour ) { }

        public void ReceivesEvent(object[] evObj) {
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
            else if( t == typeof(object[]) ) ReceiveEvent(name, (object[])value);
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
        public virtual void ReceiveEvent(string name, object[] value) { }

        public void SetJoinEvent( object[] evObj )
        {
            string name = (string)evObj[(int)EvObj.Name];
            string target = (string)evObj[(int)EvObj.Target];
            ClearJoinEvent(name, target);

            evObj = (object[])evObj.Clone();
            evObj[(int)EvObj.RequestTo] = RequestTo.Server;
            RequestEvent(evObj);
        }

        public void LoggedJoinEvent( object[] evObj )
        {
            evObj = (object[])evObj.Clone();
            evObj[(int)EvObj.RequestTo] = RequestTo.Server;
            RequestEvent(evObj);
        }

        public void ClearJoinEvent( string name="", string target="" )
        {
            RequestEvent(RequestTo.Server, "CLEAR_JOINEVENT", Pack(name, target));
        }

        // Meta
        public int GetDelay() { return (int)obj[(int)EvObj.Delay]; }
        public SimpleNetworkBehaviour GetSource() { return (SimpleNetworkBehaviour)obj[(int)EvObj.Source]; }
        public int GetIndex() { return _groupIndex; }
        public int GetSender() { return (int)obj[(int)EvObj.Sender]; }
        public string GetTarget() { return (string)obj[(int)EvObj.Target]; }
        public int[] GetRecipients()
        {
            int sendto = (int)obj[(int)EvObj.SendTo];
            if( sendto < (int)SendTo.Length ) {
                if( sendto == (int)SendTo.All ) return _sn.GetPlayers();
                else if( sendto == (int)SendTo.Owner ) return new int[] {Networking.GetOwner(((SimpleNetworkBehaviour)obj[(int)EvObj.Source]).gameObject).playerId};
                else if( sendto == (int)SendTo.Master ) return new int[] {Networking.GetOwner(_sn.gameObject).playerId};
                else if( sendto == (int)SendTo.Self ) return new int[] {Networking.LocalPlayer.playerId};
                else if( sendto == (int)SendTo.NotOwner ) return RemovePlayer(_sn.GetPlayers(), Networking.GetOwner(((SimpleNetworkBehaviour)obj[(int)EvObj.Source]).gameObject).playerId);
                else if( sendto == (int)SendTo.NotMaster ) return RemovePlayer(_sn.GetPlayers(), Networking.GetOwner(_sn.gameObject).playerId);
                else if( sendto == (int)SendTo.NotSelf ) return RemovePlayer(_sn.GetPlayers(), Networking.LocalPlayer.playerId);
            }
            return new int[] {sendto-(int)SendTo.Length};
        }

        private int[] RemovePlayer(int[] arr, int player)
        {
            int index = Array.IndexOf(arr, player);
            if (index >= 0) {
                int[] newArr = new int[arr.Length-1];
                Array.Copy(arr, 0, newArr, 0, index);
                Array.Copy(arr, index + 1, newArr, index, arr.Length - index - 1);
                return newArr;
            }
            return arr;
        }

        // EvObj
        public object[] ExecEvent(object[] evObj) { if(evObj==null) return null; evObj=(object[])evObj.Clone(); evObj[(int)EvObj.RequestTo]=RequestTo.None; evObj[(int)EvObj.SendTo]=SendTo.Self; return R(evObj); }
        public object[] SendEvent(object[] evObj) { if(evObj==null) return null; evObj=(object[])evObj.Clone(); evObj[(int)EvObj.RequestTo]=RequestTo.None; return R(evObj); }
        public object[] RequestEvent(object[] evObj) { if(evObj==null) return null; evObj=(object[])evObj.Clone(); return R(evObj); }
        public object[] RequestEvent(RequestTo request, object[] evObj) { if(evObj==null) return null; evObj=(object[])evObj.Clone(); evObj[(int)EvObj.RequestTo]=request; return R(evObj); }

        // void
        public object[] ExecEvent(string name) { return R(E(this, RequestTo.None, SendTo.Self, name, null, "", 0, JoinSync.None)); }
        public object[] SendEvent(string name) { return R(E(this, RequestTo.None, SendTo.All, name, null, "", 0, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name) { return R(E(this, RequestTo.None, sendto, name, null, "", 0, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name) { return R(E(this, RequestTo.None, sendto, name, null, "", 0, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name) { return R(E(this, request, SendTo.All, name, null, "", 0, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name) { return R(E(this, request, SendTo.All, name, null, "", 0, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name) { return R(E(this, request, sendto, name, null, "", 0, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name) { return R(E(this, request, sendto, name, null, "", 0, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name) { return R(E(this, request, sendto, name, null, "", 0, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name) { return R(E(this, request, sendto, name, null, "", 0, JoinSync.None)); }
        public object[] CreateEvent(RequestTo request, string name) { return E(this, request, SendTo.All, name, null, "", 0, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name) { return E(this, request, SendTo.All, name, null, "", 0, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name) { return E(this, request, sendto, name, null, "", 0, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name) { return E(this, request, sendto, name, null, "", 0, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name) { return E(this, request, sendto, name, null, "", 0, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name) { return E(this, request, sendto, name, null, "", 0, JoinSync.None); }

        // bool
        public object[] ExecEvent(string name, bool value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, bool value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, bool value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, bool value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, bool value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, bool value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, bool value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, bool value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, bool value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, bool value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, bool value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, bool value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, bool value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, bool value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, bool value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, bool value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, bool value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, bool value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, bool value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, bool value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, bool value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, bool value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, bool value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, bool value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, bool value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, bool value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, bool value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, bool value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, bool value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, bool value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, bool value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, bool value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, bool value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, bool value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, bool value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, bool value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, bool value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, bool value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, bool value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, bool value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, bool value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, bool value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, bool value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, bool value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, bool value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, bool value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, bool value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public bool GetBool(int i=0) { object[] v=GetValues(); i=I(typeof(bool),i,v); if(i==-1)Debug.LogError(string.Format(err,"bool")); return (bool)v[i]; }

        // int
        public object[] ExecEvent(string name, int value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, int value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, int value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, int value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, int value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, int value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, int value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, int value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, int value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, int value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, int value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, int value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, int value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, int value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, int value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, int value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, int value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, int value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, int value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, int value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, int value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, int value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, int value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, int value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, int value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, int value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, int value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, int value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, int value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, int value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, int value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, int value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, int value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, int value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, int value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, int value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, int value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, int value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, int value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, int value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, int value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, int value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, int value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, int value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, int value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, int value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, int value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, int value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, int value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, int value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, int value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, int value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, int value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, int value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, int value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, int value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, int value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, int value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, int value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public int GetInt(int i=0) { object[] v=GetValues(); i=I(typeof(int),i,v); if(i==-1)Debug.LogError(string.Format(err,"int")); return (int)v[i]; }

        // float
        public object[] ExecEvent(string name, float value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, float value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, float value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, float value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, float value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, float value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, float value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, float value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, float value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, float value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, float value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, float value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, float value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, float value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, float value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, float value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, float value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, float value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, float value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, float value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, float value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, float value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, float value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, float value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, float value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, float value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, float value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, float value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, float value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, float value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, float value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, float value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, float value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, float value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, float value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, float value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, float value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, float value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, float value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, float value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, float value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, float value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, float value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, float value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, float value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, float value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, float value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, float value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, float value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, float value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, float value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, float value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, float value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, float value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, float value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, float value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, float value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, float value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, float value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public float GetFloat(int i=0) { object[] v=GetValues(); i=I(typeof(float),i,v); if(i==-1)Debug.LogError(string.Format(err,"float")); return (float)v[i]; }

        // string
        public object[] ExecEvent(string name, string value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, string value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, string value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, string value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, string value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, string value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, string value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, string value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, string value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, string value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, string value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, string value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, string value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, string value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, string value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, string value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, string value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, string value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, string value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, string value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, string value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, string value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, string value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, string value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, string value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, string value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, string value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, string value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, string value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, string value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, string value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, string value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, string value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, string value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, string value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, string value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, string value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, string value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, string value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, string value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, string value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, string value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, string value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, string value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, string value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, string value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, string value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, string value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, string value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, string value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, string value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, string value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, string value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, string value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, string value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, string value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, string value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, string value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, string value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public string GetString(int i=0) { object[] v=GetValues(); i=I(typeof(string),i,v); if(i==-1)Debug.LogError(string.Format(err,"string")); return (string)v[i]; }

        // Vector3
        public object[] ExecEvent(string name, Vector3 value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, Vector3 value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, Vector3 value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, Vector3 value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, Vector3 value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Vector3 value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, Vector3 value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Vector3 value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Vector3 value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Vector3 value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Vector3 value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Vector3 value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, Vector3 value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Vector3 value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Vector3 value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Vector3 value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Vector3 value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, Vector3 value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, Vector3 value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, Vector3 value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, Vector3 value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, Vector3 value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Vector3 value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Vector3 value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Vector3 value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Vector3 value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Vector3 value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Vector3 value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Vector3 value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Vector3 value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Vector3 value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Vector3 value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Vector3 value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Vector3 value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public Vector3 GetVector3(int i=0) { object[] v=GetValues(); i=I(typeof(Vector3),i,v); if(i==-1)Debug.LogError(string.Format(err,"Vector3")); return (Vector3)v[i]; }

        // Quaternion
        public object[] ExecEvent(string name, Quaternion value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, Quaternion value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, Quaternion value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, Quaternion value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, Quaternion value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Quaternion value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, Quaternion value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Quaternion value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Quaternion value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Quaternion value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Quaternion value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Quaternion value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, Quaternion value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Quaternion value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Quaternion value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Quaternion value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Quaternion value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, Quaternion value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, Quaternion value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, Quaternion value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, Quaternion value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, Quaternion value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Quaternion value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Quaternion value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Quaternion value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, Quaternion value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, Quaternion value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Quaternion value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Quaternion value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Quaternion value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Quaternion value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, Quaternion value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, Quaternion value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, Quaternion value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public Quaternion GetQuaternion(int i=0) { object[] v=GetValues(); i=I(typeof(Quaternion),i,v); if(i==-1)Debug.LogError(string.Format(err,"Quaternion")); return (Quaternion)v[i]; }

        // Behaviour
        public object[] ExecEvent(string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, SimpleNetworkBehaviour value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, SimpleNetworkBehaviour value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public SimpleNetworkBehaviour GetBehaviour(int i=0) { object[] v=GetValues(); i=I(typeof(UdonSharpBehaviour),i,v); if(i==-1)Debug.LogError(string.Format(err,"SimpleNetworkBehaviour")); return (SimpleNetworkBehaviour)v[i]; }

        // object[]
        public object[] ExecEvent(string name, object[] value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.Self, name, value, target, delay, JoinSync.None)); }
        public object[] ExecEvent(string name, object[] value, int delay) { return R(E(this, RequestTo.None, SendTo.Self, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, object[] value, string target="", int delay=0) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(string name, object[] value, int delay) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] SendEvent(string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] SendEvent(string name, object[] value, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] SendEvent(string name, object[] value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, object[] value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, object[] value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(SendTo sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, object[] value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(SendTo sendto, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, object[] value, string target="", int delay=0) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, object[] value, int delay) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", delay, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, object[] value, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, "", 0, joinsync)); }
        public object[] SendEvent(VRCPlayerApi sendto, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, RequestTo.None, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, object[] value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, object[] value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, object[] value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, object[] value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, object[] value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, object[] value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, SendTo sendto, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, object[] value, string target="", int delay=0) { return R(E(this, request, SendTo.All, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, object[] value, int delay) { return R(E(this, request, SendTo.All, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, object[] value, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, request, SendTo.All, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, string target="", int delay=0) { return R(E(this, request, sendto, name, value, target, delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, int delay) { return R(E(this, request, sendto, name, value, "", delay, JoinSync.None)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, int delay, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", delay, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, JoinSync joinsync) { return R(E(this, request, sendto, name, value, "", 0, joinsync)); }
        public object[] RequestEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, string target, JoinSync joinsync) { return R(E(this, request, sendto, name, value, target, 0, joinsync)); }
        public object[] CreateEvent(RequestTo request, string name, object[] value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, object[] value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, string name, object[] value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, object[] value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, object[] value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, string name, object[] value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, object[] value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, object[] value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, object[] value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, object[] value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, SendTo sendto, string name, object[] value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(RequestTo request, VRCPlayerApi sendto, string name, object[] value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, object[] value, string target="", int delay=0) { return E(this, request, SendTo.All, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, object[] value, int delay) { return E(this, request, SendTo.All, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, string name, object[] value, string target, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, object[] value, int delay, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, object[] value, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, string name, object[] value, string target, JoinSync joinsync) { return E(this, request, SendTo.All, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, SendTo sendto, string name, object[] value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, string target="", int delay=0) { return E(this, request, sendto, name, value, target, delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, int delay) { return E(this, request, sendto, name, value, "", delay, JoinSync.None); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, string target, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, target, delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, int delay, JoinSync joinsync) { return E(this, request, sendto, name, value, "", delay, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, JoinSync joinsync) { return E(this, request, sendto, name, value, "", 0, joinsync); }
        public object[] CreateEvent(VRCPlayerApi request, VRCPlayerApi sendto, string name, object[] value, string target, JoinSync joinsync) { return E(this, request, sendto, name, value, target, 0, joinsync); }
        public object[] GetValues() { var v=obj[(int)EvObj.Value]; return v==null?new object[0]:(v.GetType()==typeof(object[])?(object[])v:new object[]{v}); }
    }
}
