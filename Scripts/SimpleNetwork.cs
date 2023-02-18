using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public class SimpleNetwork : UdonSharpBehaviour
    {
        private SimpleNetworkBehaviour[] behaviours;
        private SimpleNetworkProxy[] proxys;
        private int[] refer;
        private char gs = (char)0x1D;
        private char rs = (char)0x1E;
        private bool isInit;
        private bool isReady;

        private bool[] rIsSyncs;
        private int[] rDelays;
        private SimpleNetworkBehaviour[] rSources;
        private string[] rNames;
        private string[] rValues;

        private static string GenerateCmdId() { return string.Format("{0:X4}", UnityEngine.Random.Range(0, 65536)); }

        public static SimpleNetwork GetInstance()
        {
            GameObject g = GameObject.Find("SimpleNetwork");
            SimpleNetwork sn = g.GetComponent<SimpleNetwork>();
            sn.Initialize();
            return sn;
        }

        public static T[] InsertElement<T>(T[] ary, int idx, T elm)
        {
            T[] tmp = new T[ary.Length + 1];
            int j = 0;
            for (int i = 0; i < tmp.Length; i++) {
                if (i == idx) tmp[i] = elm;
                else { tmp[i]=ary[j]; j++; }
            }
            return tmp;
        }

        private void Initialize()
        {
            if( isInit ) return;
            isInit = true;

            behaviours = new SimpleNetworkBehaviour[0];
            refer = new int[0];

            rIsSyncs = new bool[0];
            rDelays = new int[0];
            rSources = new SimpleNetworkBehaviour[0];
            rNames = new string[0];
            rValues = new string[0];

            int len = gameObject.transform.childCount;
            proxys = new SimpleNetworkProxy[len+1];
            for(int i=1; i<len; i++) {
                Transform t = transform.GetChild(i);
                proxys[i] = t.gameObject.GetComponent<SimpleNetworkProxy>();
                proxys[i].id = i;
                proxys[i].sn = this;
            }
        }

        private void Update()
        {
            if( !isReady ) return;

            string command = "";
            for(var i=0; i<rDelays.Length; i++) {
                if( rDelays[i] == 0 ) {
                    if( !rIsSyncs[i] ) rSources[i].ReceiveEvent(rNames[i], rValues[i]);
                    else {
                        string srcId = rSources[i]._id.ToString("X");
                        string record = srcId + rs + rNames[i] + rs + rValues[i];
                        command += (command==""?GenerateCmdId():gs.ToString())+record;
                    }
                }
                if( rDelays[i] >= 0) rDelays[i]--;
            }

            if( command != "" ) {
                Debug.Log(command);
                GetProxy(Networking.LocalPlayer).Sync(command);
                command="";
            }
        }

        public void Register(SimpleNetworkBehaviour behaviour)
        {
            int index = behaviours.Length;
            string x = Networking.GetUniqueName(behaviour.gameObject);
            for (int i=0; i<behaviours.Length; i++) {
                string y = Networking.GetUniqueName(behaviours[i].gameObject);
                if (y.CompareTo(x) > 0) { index=i; break; }
            }
            var temp = new SimpleNetworkBehaviour[behaviours.Length+1];
            for (int i=0; i<index; i++) temp[i] = behaviours[i];
            behaviour._id = (uint)index;
            temp[index] = behaviour;
            for (int i = index+1; i<temp.Length; i++) { temp[i]=behaviours[i-1]; temp[i]._id++; }
            behaviours = temp;
        }

        public void SetEvent( bool isSync, int delay, SimpleNetworkBehaviour source, string name, string value )
        {
            int idx = rDelays.Length;
            for (int i=0; i<rDelays.Length; i++) if(delay>rDelays[i]){idx=i;break;}
            if( rDelays.Length != idx && rDelays[idx] == -1 ) {
                rIsSyncs[idx] = isSync;
                rDelays[idx] = delay;
                rSources[idx] = source;
                rNames[idx] = name;
                rValues[idx] = value;
            } else {
                rIsSyncs = InsertElement(rIsSyncs, idx, isSync);
                rDelays = InsertElement(rDelays, idx, delay);
                rSources = InsertElement(rSources, idx, source);
                rNames = InsertElement(rNames, idx, name);
                rValues = InsertElement(rValues, idx, value);
            }
        }

        private bool IsProxyUsed( int i )
        {
            foreach(int j in refer) if(j == i) return true;
            return false;
        }

        private void AttachProxy( VRCPlayerApi player, SimpleNetworkProxy proxy )
        {
            int[] temp = new int[player.playerId];
            refer.CopyTo(temp, 0);
            refer = temp;
            int i = player.playerId - 1;
            refer[i] = proxy.id;

            Debug.Log(player.displayName+"さんに"+proxy.id+"番のProxyを割り当てました");
        }

        private void DetachProxy( VRCPlayerApi player )
        {
            int i = player.playerId - 1;
            Debug.Log(refer[i]+"番のProxyを解除しました");
            refer[i] = 0;
        }

        private SimpleNetworkProxy GetProxy( VRCPlayerApi player )
        {
            int i = player.playerId - 1;
            return proxys[refer[i]];
        }

        public void OnProxySynced( string cmd, SimpleNetworkProxy proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            foreach( string record in cmd.Substring(4).Split(gs) ) {
                string[] data = record.Split(rs);
                uint i = UInt32.Parse(data[0], System.Globalization.NumberStyles.HexNumber);
                behaviours[i].ReceiveEvent(data[1], data[2]);
            }
        }

        public void OnProxyOwnershipTransferred( SimpleNetworkProxy proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            AttachProxy(player, proxy);
            if( Networking.LocalPlayer == player ) isReady = true;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if( !Networking.IsMaster ) return;

            int pid = player.playerId;
            for(var i=1; i<proxys.Length+1; i++) {
                SimpleNetworkProxy proxy = proxys[i];
                if( IsProxyUsed(i) ) continue;
                if( Networking.IsOwner(player, proxy.gameObject) ) OnProxyOwnershipTransferred(proxy);
                else Networking.SetOwner(player, proxy.gameObject);
                break;
            }

            // デバッグ用
            string refstr =""; foreach(int r in refer) refstr += r+","; Debug.Log("refer:"+refstr);

            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            DetachProxy(player);
        }
    }
}
