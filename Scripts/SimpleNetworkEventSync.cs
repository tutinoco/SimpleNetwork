using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public enum EventSyncType
    {
        Proxy,
        JoynSync,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleNetworkEventSync : UdonSharpBehaviour
    {
        [SerializeField] protected SimpleNetwork sn;
        [SerializeField] protected EventSyncType eventSyncType;
        protected bool isWaiting;
        protected bool isInitialSyncComplete;

        [UdonSynced(UdonSyncMode.None)] protected int count;
        [UdonSynced(UdonSyncMode.None)] protected string[] names = new string[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] values = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] lengths = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] sources = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] requests = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] sendtos = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected string[] targets = new string[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] delays = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] joinsyncs = new int[0];

        [UdonSynced(UdonSyncMode.None)] protected bool[] bools = new bool[0];
        [UdonSynced(UdonSyncMode.None)] protected char[] chars = new char[0];
        [UdonSynced(UdonSyncMode.None)] protected byte[] bytes = new byte[0];
        [UdonSynced(UdonSyncMode.None)] protected sbyte[] sbytes = new sbyte[0];
        [UdonSynced(UdonSyncMode.None)] protected short[] shorts = new short[0];
        [UdonSynced(UdonSyncMode.None)] protected ushort[] ushorts = new ushort[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] ints = new int[0];
        [UdonSynced(UdonSyncMode.None)] protected uint[] uints = new uint[0];
        [UdonSynced(UdonSyncMode.None)] protected long[] longs = new long[0];
        [UdonSynced(UdonSyncMode.None)] protected ulong[] ulongs = new ulong[0];
        [UdonSynced(UdonSyncMode.None)] protected float[] floats = new float[0];
        [UdonSynced(UdonSyncMode.None)] protected double[] doubles = new double[0];
        [UdonSynced(UdonSyncMode.None)] protected Vector2[] Vector2s = new Vector2[0];
        [UdonSynced(UdonSyncMode.None)] protected Vector3[] Vector3s = new Vector3[0];
        [UdonSynced(UdonSyncMode.None)] protected Vector4[] Vector4s = new Vector4[0];
        [UdonSynced(UdonSyncMode.None)] protected Quaternion[] Quaternions = new Quaternion[0];
        [UdonSynced(UdonSyncMode.None)] protected string[] strings = new string[0];
        [UdonSynced(UdonSyncMode.None)] protected VRCUrl[] VRCUrls = new VRCUrl[0];
        [UdonSynced(UdonSyncMode.None)] protected Color[] Colors = new Color[0];
        [UdonSynced(UdonSyncMode.None)] protected Color32[] Color32s = new Color32[0];
        [UdonSynced(UdonSyncMode.None)] protected int[] Behaviors = new int[0];

        private static int AddElement<T>(ref T[] ary, T elm)
        {
            T[] tmp = new T[ary.Length+1];
            ary.CopyTo(tmp, 0);
            int idx = ary.Length;
            tmp[idx] = elm;
            ary = tmp;
            return idx;
        }

        public static void RemoveElement<T>(ref T[] array, int index)
        {
            if (index < 0 || index >= array.Length) return;
            T[] tmp = new T[array.Length-1];
            Array.Copy(array, 0, tmp, 0, index);
            Array.Copy(array, index + 1, tmp, index, array.Length - index - 1);
            array = tmp;
        }
        
        public virtual void Start()
        {
            if( eventSyncType == EventSyncType.JoynSync ) isWaiting = true;
            if( !Networking.IsMaster ) return;
            count++;
            isInitialSyncComplete = true;
            RequestSerialization();
        }

        public int EventLength()
        {
            return names.Length;
        }

        public object[] GetEvent(int idx)
        {
            object[] evObj = new object[(int)EvObj.Length];
            evObj[(int)EvObj.Source] = sn.GetBehaviour(sources[idx]);
            evObj[(int)EvObj.RequestTo] = requests[idx];
            evObj[(int)EvObj.SendTo] = sendtos[idx];
            evObj[(int)EvObj.Name] = names[idx];
            evObj[(int)EvObj.Target] = targets[idx];
            evObj[(int)EvObj.Delay] = delays[idx];
            evObj[(int)EvObj.JoinSync] = joinsyncs[idx];
            evObj[(int)EvObj.Sender] = Networking.GetOwner(gameObject).playerId;

            int index = 0;
            int length = lengths[idx];
            for(int i=0; i<idx; i++) index += lengths[i];

            if( length == 1 ) evObj[(int)EvObj.Value] = GetValues(index, length)[0];
            else if( length > 1 ) evObj[(int)EvObj.Value] = GetValues(index, length);

            return evObj;
        }

        public void SetEvent( object[] evObj )
        {
            if( !isWaiting ) { ClearEvents(); isWaiting=true; }

            var length = 0;
            var value = evObj[(int)EvObj.Value];
            if( value!=null && value.GetType()==typeof(object[]) ) length = SetValues((object[])value);
            else length = SetValues(new object[] {value});

            AddElement(ref names, (string)evObj[(int)EvObj.Name]);
            AddElement(ref lengths, length);
            AddElement(ref sources, ((SimpleNetworkBehaviour)evObj[(int)EvObj.Source])._id);
            AddElement(ref requests, (int)evObj[(int)EvObj.RequestTo]);
            AddElement(ref sendtos, (int)evObj[(int)EvObj.SendTo]);
            AddElement(ref targets, (string)evObj[(int)EvObj.Target]);
            AddElement(ref delays, (int)evObj[(int)EvObj.Delay]);
            AddElement(ref joinsyncs, (int)evObj[(int)EvObj.JoinSync]);
            count++;
        }

        public void SyncEvents()
        {
            if( !isWaiting ) return;
            RequestSerialization();
#if UNITY_EDITOR
            OnPreSerialization();
#endif
        }

        public void RemoveEvents( string name, string target ) { RemoveEvents(name, sn.GetBehaviours(target)); }
        public void RemoveEvents( string name, SimpleNetworkBehaviour behaviour ) { RemoveEvents(name, new SimpleNetworkBehaviour[]{behaviour}); }
        public void RemoveEvents( string name, SimpleNetworkBehaviour[] behaviours )
        {
            foreach(var behaviour in behaviours) {
                int id = behaviour._id;
                for(var i=0; i<sources.Length; i++) {
                    if( name!=null && name!="" && names[i] != name ) continue;
                    if( id == sources[i] ) {
                        RemoveValues(values[i*2], values[i*2+1]);
                        RemoveElement(ref names, i);
                        RemoveElement(ref lengths, i);
                        RemoveElement(ref sources, i);
                        RemoveElement(ref requests, i);
                        RemoveElement(ref sendtos, i);
                        RemoveElement(ref targets, i);
                        RemoveElement(ref delays, i);
                        RemoveElement(ref joinsyncs, i);
                    }
                }
            }
        }

        public void ClearEvents()
        {
            if( names.Length > 0 ) names = new string[0];
            if( values.Length > 0 ) values = new int[0];
            if( lengths.Length > 0 ) lengths = new int[0];
            if( sources.Length > 0 ) sources = new int[0];
            if( requests.Length > 0 ) requests = new int[0];
            if( sendtos.Length > 0 ) sendtos = new int[0];
            if( targets.Length > 0 ) targets = new string[0];
            if( delays.Length > 0 ) delays = new int[0];
            if( joinsyncs.Length > 0 ) joinsyncs = new int[0];
            
            if( bools.Length > 0 ) bools = new bool[0];
            if( chars.Length > 0 ) chars = new char[0];
            if( bytes.Length > 0 ) bytes = new byte[0];
            if( sbytes.Length > 0 ) sbytes = new sbyte[0];
            if( shorts.Length > 0 ) shorts = new short[0];
            if( ushorts.Length > 0 ) ushorts = new ushort[0];
            if( ints.Length > 0 ) ints = new int[0];
            if( uints.Length > 0 ) uints = new uint[0];
            if( longs.Length > 0 ) longs = new long[0];
            if( ulongs.Length > 0 ) ulongs = new ulong[0];
            if( floats.Length > 0 ) floats = new float[0];
            if( doubles.Length > 0 ) doubles = new double[0];
            if( Vector2s.Length > 0 ) Vector2s = new Vector2[0];
            if( Vector3s.Length > 0 ) Vector3s = new Vector3[0];
            if( Vector4s.Length > 0 ) Vector4s = new Vector4[0];
            if( Quaternions.Length > 0 ) Quaternions = new Quaternion[0];
            if( strings.Length > 0 ) strings = new string[0];
            if( VRCUrls.Length > 0 ) VRCUrls = new VRCUrl[0];
            if( Colors.Length > 0 ) Colors = new Color[0];
            if( Color32s.Length > 0 ) Color32s = new Color32[0];
            if( Behaviors.Length > 0 ) Behaviors = new int[0];
        }

        private int SetValues( object[] vals )
        {
            int length = 0;

            foreach( var v in vals ) {

                int type = -1;
                int index = 0;

                Type t = v==null ? null : v.GetType();
                
                if( t == typeof(bool) ) { type=0; index=AddElement(ref bools, (bool)v); }
                else if( t == typeof(char) ) { type=1; index=AddElement(ref chars, (char)v); }
                else if( t == typeof(byte) ) { type=2; index=AddElement(ref bytes, (byte)v); }
                else if( t == typeof(sbyte) ) { type=3; index=AddElement(ref sbytes, (sbyte)v); }
                else if( t == typeof(short) ) { type=4; index=AddElement(ref shorts, (short)v); }
                else if( t == typeof(ushort) ) { type=5; index=AddElement(ref ushorts, (ushort)v); }
                else if( t == typeof(int) ) { type=6; index=AddElement(ref ints, (int)v); }
                else if( t == typeof(uint) ) { type=7; index=AddElement(ref uints, (uint)v); }
                else if( t == typeof(long) ) { type=8; index=AddElement(ref longs, (long)v); }
                else if( t == typeof(ulong) ) { type=9; index=AddElement(ref ulongs, (ulong)v); }
                else if( t == typeof(float) ) { type=10; index=AddElement(ref floats, (float)v); }
                else if( t == typeof(double) ) { type=11; index=AddElement(ref doubles, (double)v); }
                else if( t == typeof(Vector2) ) { type=12; index=AddElement(ref Vector2s, (Vector2)v); }
                else if( t == typeof(Vector3) ) { type=13; index=AddElement(ref Vector3s, (Vector3)v); }
                else if( t == typeof(Vector4) ) { type=14; index=AddElement(ref Vector4s, (Vector4)v); }
                else if( t == typeof(Quaternion) ) { type=15; index=AddElement(ref Quaternions, (Quaternion)v); }
                else if( t == typeof(string) ) { type=16; index=AddElement(ref strings, (string)v); }
                else if( t == typeof(VRCUrl) ) { type=17; index=AddElement(ref VRCUrls, (VRCUrl)v); }
                else if( t == typeof(Color) ) { type=18; index=AddElement(ref Colors, (Color)v); }
                else if( t == typeof(Color32) ) { type=19; index=AddElement(ref Color32s, (Color32)v); }
                else if( t == typeof(UdonSharpBehaviour) ) { type=20; index=AddElement(ref Behaviors, ((SimpleNetworkBehaviour)v)._id); }

                length++;

                AddElement(ref values, type);
                AddElement(ref values, index);
            }

            return length;
        }

        private object[] GetValues( int index, int length )
        {
            object[] obj = new object[length][];

            for(int i=0; i<length; i++) {
                int t = values[(index+i)*2];
                int j = values[(index+i)*2+1];

                if ( t == -1 ) obj[i] = null;
                else if( t == 0 ) obj[i] = bools[j];
                else if( t == 1 ) obj[i] = chars[j];
                else if( t == 2 ) obj[i] = bytes[j];
                else if( t == 3 ) obj[i] = sbytes[j];
                else if( t == 4 ) obj[i] = shorts[j];
                else if( t == 5 ) obj[i] = ushorts[j];
                else if( t == 6 ) obj[i] = ints[j];
                else if( t == 7 ) obj[i] = uints[j];
                else if( t == 8 ) obj[i] = longs[j];
                else if( t == 9 ) obj[i] = ulongs[j];
                else if( t == 10 ) obj[i] = floats[j];
                else if( t == 11 ) obj[i] = doubles[j];
                else if( t == 12 ) obj[i] = Vector2s[j];
                else if( t == 13 ) obj[i] = Vector3s[j];
                else if( t == 14 ) obj[i] = Vector4s[j];
                else if( t == 15 ) obj[i] = Quaternions[j];
                else if( t == 16 ) obj[i] = strings[j];
                else if( t == 17 ) obj[i] = VRCUrls[j];
                else if( t == 18 ) obj[i] = Colors[j];
                else if( t == 19 ) obj[i] = Color32s[j];
                else if( t == 20 ) obj[i] = sn.GetBehaviour(Behaviors[j]);
            }

            return obj;
        }

        private void RemoveValues( int index, int length )
        {
            for(int i=0; i<length; i++) {
                int t = values[(index+i)*2];
                int j = values[(index+i)*2+1];

                if( t == 0 ) RemoveElement(ref bools, j);
                else if( t == 1 ) RemoveElement(ref chars, j);
                else if( t == 2 ) RemoveElement(ref bytes, j);
                else if( t == 3 ) RemoveElement(ref sbytes, j);
                else if( t == 4 ) RemoveElement(ref shorts, j);
                else if( t == 5 ) RemoveElement(ref ushorts, j);
                else if( t == 6 ) RemoveElement(ref ints, j);
                else if( t == 7 ) RemoveElement(ref uints, j);
                else if( t == 8 ) RemoveElement(ref longs, j);
                else if( t == 9 ) RemoveElement(ref ulongs, j);
                else if( t == 10 ) RemoveElement(ref floats, j);
                else if( t == 11 ) RemoveElement(ref doubles, j);
                else if( t == 12 ) RemoveElement(ref Vector2s, j);
                else if( t == 13 ) RemoveElement(ref Vector3s, j);
                else if( t == 14 ) RemoveElement(ref Vector4s, j);
                else if( t == 15 ) RemoveElement(ref Quaternions, j);
                else if( t == 16 ) RemoveElement(ref strings, j);
                else if( t == 17 ) RemoveElement(ref VRCUrls, j);
                else if( t == 18 ) RemoveElement(ref Colors, j);
                else if( t == 19 ) RemoveElement(ref Color32s, j);
                else if( t == 20 ) RemoveElement(ref Behaviors, j);
            }
        }

        public override void OnPreSerialization()
        {
            if( eventSyncType != EventSyncType.Proxy ) return;
            isWaiting = false;
            sn.OnEventSynced(this);
        }

        public override void OnDeserialization()
        {
            bool sync = true;
            if( eventSyncType == EventSyncType.Proxy && !isInitialSyncComplete ) sync = false;
            if( eventSyncType == EventSyncType.JoynSync && isInitialSyncComplete ) sync = false;

            isInitialSyncComplete = true;
            if( sync && sn != null ) { sn.OnEventSynced(this); }
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if( player.isMaster ) return; // Proxy解除時の対策がこれでいいかもっと検討したほうがいい
            if( eventSyncType == EventSyncType.Proxy ) sn.OnProxyOwnershipTransferred(this);
        }
    }
}