using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using Object = System.Object;

namespace tutinoco
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleNetworkProxy : UdonSharpBehaviour
    {
        [SerializeField] private SimpleNetwork sn;
        private int num;
        private bool isWaiting;

        [UdonSynced(UdonSyncMode.None)] private int count;
        [UdonSynced(UdonSyncMode.None)] private string[] names = new string[0];
        [UdonSynced(UdonSyncMode.None)] private int[] values = new int[0];
        [UdonSynced(UdonSyncMode.None)] private int[] lengths = new int[0];
        [UdonSynced(UdonSyncMode.None)] private int[] sources = new int[0];
        [UdonSynced(UdonSyncMode.None)] private int[] requests = new int[0];
        [UdonSynced(UdonSyncMode.None)] private int[] sendtos = new int[0];
        [UdonSynced(UdonSyncMode.None)] private int[] delays = new int[0];

        [UdonSynced(UdonSyncMode.None)] private bool[] bools = new bool[0];
        [UdonSynced(UdonSyncMode.None)] private char[] chars = new char[0];
        [UdonSynced(UdonSyncMode.None)] private byte[] bytes = new byte[0];
        [UdonSynced(UdonSyncMode.None)] private sbyte[] sbytes = new sbyte[0];
        [UdonSynced(UdonSyncMode.None)] private short[] shorts = new short[0];
        [UdonSynced(UdonSyncMode.None)] private ushort[] ushorts = new ushort[0];
        [UdonSynced(UdonSyncMode.None)] private int[] ints = new int[0];
        [UdonSynced(UdonSyncMode.None)] private uint[] uints = new uint[0];
        [UdonSynced(UdonSyncMode.None)] private long[] longs = new long[0];
        [UdonSynced(UdonSyncMode.None)] private ulong[] ulongs = new ulong[0];
        [UdonSynced(UdonSyncMode.None)] private float[] floats = new float[0];
        [UdonSynced(UdonSyncMode.None)] private double[] doubles = new double[0];
        [UdonSynced(UdonSyncMode.None)] private Vector2[] Vector2s = new Vector2[0];
        [UdonSynced(UdonSyncMode.None)] private Vector3[] Vector3s = new Vector3[0];
        [UdonSynced(UdonSyncMode.None)] private Vector4[] Vector4s = new Vector4[0];
        [UdonSynced(UdonSyncMode.None)] private Quaternion[] Quaternions = new Quaternion[0];
        [UdonSynced(UdonSyncMode.None)] private string[] strings = new string[0];
        [UdonSynced(UdonSyncMode.None)] private VRCUrl[] VRCUrls = new VRCUrl[0];
        [UdonSynced(UdonSyncMode.None)] private Color[] Colors = new Color[0];
        [UdonSynced(UdonSyncMode.None)] private Color32[] Color32s = new Color32[0];

        private static int AddElement<T>(ref T[] ary, T elm)
        {
            T[] tmp = new T[ary.Length + 1];
            ary.CopyTo(tmp, 0);
            int idx = ary.Length;
            tmp[idx] = elm;
            ary = tmp;
            return idx;
        }

        void Start()
        {
            num = gameObject.transform.GetSiblingIndex();
        }

        public int GetNum()
        {
            return num;
        }

        public int EventLength()
        {
            return names.Length;
        }

        public void ClearEvents()
        {
            if( names.Length > 0 ) names = new string[0];
            if( values.Length > 0 ) values = new int[0];
            if( lengths.Length > 0 ) lengths = new int[0];
            if( sources.Length > 0 ) sources = new int[0];
            if( requests.Length > 0 ) requests = new int[0];
            if( sendtos.Length > 0 ) sendtos = new int[0];
            if( delays.Length > 0 ) delays = new int[0];
            
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
        }

        public Object[] GetEvent(int idx)
        {
            Object[] evObj = new Object[] { sn.GetBehaviour(sources[idx]), requests[idx], sendtos[idx], names[idx], null, delays[idx] };

            int index = 0;
            int length = lengths[idx];
            for(int i=0; i<idx; i++) index += lengths[i];

            if( length == 1 ) evObj[(int)EvObj.Value] = GetValues(index, length)[0];
            else if( length > 1 ) evObj[(int)EvObj.Value] = GetValues(index, length);

            return evObj;
        }

        private int SetValues( Object[] vals )
        {
            int length = 0;

            foreach( var v in vals ) {

                int type = -1;
                int index = 0;

                Type t = v.GetType();
                if( t == typeof(bool) ) { length++; type=0; index=AddElement(ref bools, (bool)v); }
                else if( t == typeof(char) ) { length++; type=1; index=AddElement(ref chars, (char)v); }
                else if( t == typeof(byte) ) { length++; type=2; index=AddElement(ref bytes, (byte)v); }
                else if( t == typeof(sbyte) ) { length++; type=3; index=AddElement(ref sbytes, (sbyte)v); }
                else if( t == typeof(short) ) { length++; type=4; index=AddElement(ref shorts, (short)v); }
                else if( t == typeof(ushort) ) { length++; type=5; index=AddElement(ref ushorts, (ushort)v); }
                else if( t == typeof(int) ) { length++; type=6; index=AddElement(ref ints, (int)v); }
                else if( t == typeof(uint) ) { length++; type=7; index=AddElement(ref uints, (uint)v); }
                else if( t == typeof(long) ) { length++; type=8; index=AddElement(ref longs, (long)v); }
                else if( t == typeof(ulong) ) { length++; type=9; index=AddElement(ref ulongs, (ulong)v); }
                else if( t == typeof(float) ) { length++; type=10; index=AddElement(ref floats, (float)v); }
                else if( t == typeof(double) ) { length++; type=11; index=AddElement(ref doubles, (double)v); }
                else if( t == typeof(Vector2) ) { length++; type=12; index=AddElement(ref Vector2s, (Vector2)v); }
                else if( t == typeof(Vector3) ) { length++; type=13; index=AddElement(ref Vector3s, (Vector3)v); }
                else if( t == typeof(Vector4) ) { length++; type=14; index=AddElement(ref Vector4s, (Vector4)v); }
                else if( t == typeof(Quaternion) ) { length++; type=15; index=AddElement(ref Quaternions, (Quaternion)v); }
                else if( t == typeof(string) ) { length++; type=16; index=AddElement(ref strings, (string)v); }
                else if( t == typeof(VRCUrl) ) { length++; type=17; index=AddElement(ref VRCUrls, (VRCUrl)v); }
                else if( t == typeof(Color) ) { length++; type=18; index=AddElement(ref Colors, (Color)v); }
                else if( t == typeof(Color32) ) { length++; type=19; index=AddElement(ref Color32s, (Color32)v); }

                if( type == -1 ) continue;

                AddElement(ref values, type);
                AddElement(ref values, index);
            }

            return length;
        }

        private Object[] GetValues( int index, int length )
        {
            Object[] obj = new Object[length][];

            for(int i=0; i<length; i++) {
                int t = values[(index+i)*2];
                int j = values[(index+i)*2+1];

                if( t == 0 ) obj[i] = bools[j];
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
            }

            return obj;
        }

        public void SetEvent( Object[] evObj )
        {
            if( !isWaiting ) { ClearEvents(); isWaiting=true; }

            var length = 0;
            var value = evObj[(int)EvObj.Value];
            if( value.GetType() == typeof(Object[]) ) length = SetValues((Object[])value);
            else length = SetValues(new Object[] {value});

            AddElement(ref names, (string)evObj[(int)EvObj.Name]);
            AddElement(ref lengths, length);
            AddElement(ref sources, ((SimpleNetworkBehaviour)evObj[(int)EvObj.Source])._id);
            AddElement(ref requests, (int)evObj[(int)EvObj.RequestTo]);
            AddElement(ref sendtos, (int)evObj[(int)EvObj.SendTo]);
            AddElement(ref delays, (int)evObj[(int)EvObj.Delay]);
        }

        public void SyncEvents()
        {
            if( !isWaiting ) return;
            count++;
            isWaiting = false;
            RequestSerialization();
            if( Networking.IsOwner(gameObject) ) OnDeserialization();
        }

        public override void OnDeserialization()
        {
            sn.OnProxySynced(this);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if( player.isMaster ) return; // Proxy解除時の対策がこれでいいかもっと検討したほうがいい
            sn.OnProxyOwnershipTransferred(this);
        }
    }
}