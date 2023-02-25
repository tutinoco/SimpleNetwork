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
        [UdonSynced(UdonSyncMode.None)] private int[] types = new int[0];
        [UdonSynced(UdonSyncMode.None)] private int[] indexes = new int[0];
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
            if( types.Length > 0 ) types = new int[0];
            if( indexes.Length > 0 ) indexes = new int[0];
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

            int t = types[idx];
            int i = indexes[idx];
            if( t == 0 ) evObj[(int)EvObj.Value] = bools[i];
            else if( t == 1 ) evObj[(int)EvObj.Value] = chars[i];
            else if( t == 2 ) evObj[(int)EvObj.Value] = bytes[i];
            else if( t == 3 ) evObj[(int)EvObj.Value] = sbytes[i];
            else if( t == 4 ) evObj[(int)EvObj.Value] = shorts[i];
            else if( t == 5 ) evObj[(int)EvObj.Value] = ushorts[i];
            else if( t == 6 ) evObj[(int)EvObj.Value] = ints[i];
            else if( t == 7 ) evObj[(int)EvObj.Value] = uints[i];
            else if( t == 8 ) evObj[(int)EvObj.Value] = longs[i];
            else if( t == 9 ) evObj[(int)EvObj.Value] = ulongs[i];
            else if( t == 10 ) evObj[(int)EvObj.Value] = floats[i];
            else if( t == 11 ) evObj[(int)EvObj.Value] = doubles[i];
            else if( t == 12 ) evObj[(int)EvObj.Value] = Vector2s[i];
            else if( t == 13 ) evObj[(int)EvObj.Value] = Vector3s[i];
            else if( t == 14 ) evObj[(int)EvObj.Value] = Vector4s[i];
            else if( t == 15 ) evObj[(int)EvObj.Value] = Quaternions[i];
            else if( t == 16 ) evObj[(int)EvObj.Value] = strings[i];
            else if( t == 17 ) evObj[(int)EvObj.Value] = VRCUrls[i];
            else if( t == 18 ) evObj[(int)EvObj.Value] = Colors[i];
            else if( t == 19 ) evObj[(int)EvObj.Value] = Color32s[i];

            return evObj;
        }
        
        public void SetEvent( Object[] evObj )
        {
            if( !isWaiting ) { ClearEvents(); isWaiting=true; }

            string name = (string)evObj[(int)EvObj.Name];
            var value = evObj[(int)EvObj.Value];
            int type = -1;
            int index = 0;
            int source = ((SimpleNetworkBehaviour)evObj[(int)EvObj.Source])._id;
            int request = (int)evObj[(int)EvObj.RequestTo];
            int sendto = (int)evObj[(int)EvObj.SendTo];
            int delay = (int)evObj[(int)EvObj.Delay];

            Type t = value.GetType();
            if( t == typeof(bool) ) { type=0; index=AddElement(ref bools, (bool)value); }
            else if( t == typeof(char) ) { type=1; index=AddElement(ref chars, (char)value); }
            else if( t == typeof(byte) ) { type=2; index=AddElement(ref bytes, (byte)value); }
            else if( t == typeof(sbyte) ) { type=3; index=AddElement(ref sbytes, (sbyte)value); }
            else if( t == typeof(short) ) { type=4; index=AddElement(ref shorts, (short)value); }
            else if( t == typeof(ushort) ) { type=5; index=AddElement(ref ushorts, (ushort)value); }
            else if( t == typeof(int) ) { type=6; index=AddElement(ref ints, (int)value); }
            else if( t == typeof(uint) ) { type=7; index=AddElement(ref uints, (uint)value); }
            else if( t == typeof(long) ) { type=8; index=AddElement(ref longs, (long)value); }
            else if( t == typeof(ulong) ) { type=9; index=AddElement(ref ulongs, (ulong)value); }
            else if( t == typeof(float) ) { type=10; index=AddElement(ref floats, (float)value); }
            else if( t == typeof(double) ) { type=11; index=AddElement(ref doubles, (double)value); }
            else if( t == typeof(Vector2) ) { type=12; index=AddElement(ref Vector2s, (Vector2)value); }
            else if( t == typeof(Vector3) ) { type=13; index=AddElement(ref Vector3s, (Vector3)value); }
            else if( t == typeof(Vector4) ) { type=14; index=AddElement(ref Vector4s, (Vector4)value); }
            else if( t == typeof(Quaternion) ) { type=15; index=AddElement(ref Quaternions, (Quaternion)value); }
            else if( t == typeof(string) ) { type=16; index=AddElement(ref strings, (string)value); }
            else if( t == typeof(VRCUrl) ) { type=17; index=AddElement(ref VRCUrls, (VRCUrl)value); }
            else if( t == typeof(Color) ) { type=18; index=AddElement(ref Colors, (Color)value); }
            else if( t == typeof(Color32) ) { type=19; index=AddElement(ref Color32s, (Color32)value); }

            AddElement(ref names, name);
            AddElement(ref types, type);
            AddElement(ref indexes, index);
            AddElement(ref sources, source);
            AddElement(ref requests, request);
            AddElement(ref sendtos, sendto);
            AddElement(ref delays, delay);
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