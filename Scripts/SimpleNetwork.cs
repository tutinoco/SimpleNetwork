using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public enum SendTo
    {
        All,
        Owner,
        Master,
        Self,
        NotOwner,
        NotMaster,
        NotSelf,
    }

    public class SimpleNetwork : UdonSharpBehaviour
    {
        private bool isDebugMode;

        private SimpleNetworkBehaviour[] behaviours;
        private SimpleNetworkProxy[] proxys;
        private int[] refer;
        private char gs = (char)0x1D;
        private char rs = (char)0x1E;
        private bool isInit;
        private bool isReady;
        private int syncCount;
        private int behaviorDigit = 2;

        private int[] rSendTo;
        private SimpleNetworkBehaviour[] rSources;
        private string[] rNames;
        private string[] rValues;
        private int[] rDelays;

        public static SimpleNetwork GetInstance()
        {
            GameObject g = GameObject.Find("SimpleNetwork");
            SimpleNetwork sn = g.GetComponent<SimpleNetwork>();
            sn.Initialize();
            return sn;
        }

        public static void DebugMode( bool flg )
        {
            var sn = GetInstance();
            sn.isDebugMode = flg;
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

            rSendTo = new int[0];
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

            string code = "";
            for(var i=0; i<rDelays.Length; i++) {
                if( rDelays[i] == 0 ) {
                    if( rSendTo[i] == (int)SendTo.Self ) rSources[i].ReceiveEvent(rNames[i], rValues[i]);
                    else code += (code==""?"":gs.ToString()) + JoinRecord(rSources[i], rSendTo[i], rSources[i]/*とりあえず*/, rNames[i], rValues[i]);
                }
                if( rDelays[i] >= 0) rDelays[i]--;
            }

            if( code != "" ) {
                string syncchar = SimpleNetworkConverter.Padding(SimpleNetworkConverter.ToBase94(syncCount++), 1);
                code = syncchar + code;
                if( isDebugMode ) Debug.Log("SimpleNetwork Send Code: "+code.Replace(""+rs,":").Replace(""+gs,","));
                GetProxy(Networking.LocalPlayer).Sync(code);
                code="";
            }
        }

        public string JoinRecord( SimpleNetworkBehaviour source, int player, SimpleNetworkBehaviour target, string name, string value )
        {
            string s = SimpleNetworkConverter.Padding(SimpleNetworkConverter.ToBase94((int)source._id), behaviorDigit);
            string p = SimpleNetworkConverter.Padding(SimpleNetworkConverter.ToBase94(player), 1);
            string t = SimpleNetworkConverter.Padding(SimpleNetworkConverter.ToBase94((int)target._id), behaviorDigit);
            return s + p + t + name + rs + value;
        }

        public string[] SplitRecord( string record )
        {
            string[] ary = new string[5];

            ary[0] = record.Substring(0, behaviorDigit);
            ary[1] = ""+record[behaviorDigit];
            ary[2] = record.Substring(1+behaviorDigit, 2);
            string[] namevalue = record.Substring(1+behaviorDigit*2).Split(rs);
            ary[3] = namevalue[0];
            ary[4] = namevalue[1];

            return ary;
        }

        public SimpleNetworkBehaviour GetBehaviour( int id )
        {
            return behaviours[id];
        }

        public void SetBehaviour(SimpleNetworkBehaviour behaviour)
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

        public void SetEvent( SimpleNetworkBehaviour source, SendTo sendto, string name, string value, int delay ) { _SetEvent(source, proxys.Length+(int)sendto, name, value, delay); }
        public void SetEvent( SimpleNetworkBehaviour source, VRCPlayerApi sendto, string name, string value, int delay ) { _SetEvent(source, sendto.playerId, name, value, delay); }
        private void _SetEvent( SimpleNetworkBehaviour source, int sendto, string name, string value, int delay )
        {
            int idx = rDelays.Length;
            for (int i=0; i<rDelays.Length; i++) if(delay>rDelays[i]){idx=i;break;}
            if( rDelays.Length != idx && rDelays[idx] == -1 ) {
                rSendTo[idx] = sendto;
                rDelays[idx] = delay;
                rSources[idx] = source;
                rNames[idx] = name;
                rValues[idx] = value;
            } else {
                rSendTo = InsertElement(rSendTo, idx, sendto);
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

            if( isDebugMode ) Debug.Log("SimpleNetwork Assigned Proxy #"+proxy.id+" to "+player.displayName);
        }

        private void DetachProxy( VRCPlayerApi player )
        {
            int i = player.playerId - 1;
            if( isDebugMode ) Debug.Log("SimpleNetwork Removed Proxy #"+refer[i]);
            refer[i] = 0;
        }

        private SimpleNetworkProxy GetProxy( VRCPlayerApi player )
        {
            int i = player.playerId - 1;
            return proxys[refer[i]];
        }

        public void OnProxySynced( string code, SimpleNetworkProxy proxy )
        {
            if( isDebugMode ) Debug.Log("SimpleNetwork Receive Code: "+code.Replace(""+rs,":").Replace(""+gs,","));
            // VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            foreach( string record in code.Substring(1).Split(gs) ) {
                string[] data = SplitRecord(record);
                int source = SimpleNetworkConverter.FromBase94(data[0]);
                int sendto = SimpleNetworkConverter.FromBase94(data[1]);
                int target = SimpleNetworkConverter.FromBase94(data[2]);
                SimpleNetworkBehaviour behaviour = behaviours[target];

                bool isReceive = false;
                if( sendto == proxys.Length+(int)SendTo.All ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.Owner && Networking.IsOwner(behaviour.gameObject) ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.Master && Networking.IsMaster ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.Self && Networking.IsOwner(proxy.gameObject) ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.NotOwner && !Networking.IsOwner(behaviour.gameObject) ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.NotMaster && !Networking.IsMaster ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.NotSelf && !Networking.IsOwner(proxy.gameObject) ) isReceive = true;
                else if( Networking.LocalPlayer.playerId == sendto ) isReceive = true;
                if( !isReceive ) continue;

                behaviour.ReceiveEvent(data[3], data[4]);
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

            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            DetachProxy(player);
        }
    }
}
