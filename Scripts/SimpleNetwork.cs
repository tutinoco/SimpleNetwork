using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using Object = System.Object;

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
        Me,
    }

    public enum RequestTo
    {
        None,
        Owner,
        Master,
    }

    public enum EvObj
    {
        Source,
        RequestTo,
        SendTo,
        Name,
        Value,
        Delay,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleNetwork : UdonSharpBehaviour
    {
        private bool isDebugMode;

        private Object[][] evObjs = new Object[0][];
        private SimpleNetworkBehaviour[] behaviours = new SimpleNetworkBehaviour[0];
        [SerializeField] private SimpleNetworkProxy[] proxys;
        private SimpleNetworkProxy myProxy;
        private int[] refer = new int[0];

        public static SimpleNetwork GetInstance()
        {
            GameObject g = GameObject.Find("SimpleNetwork");
            SimpleNetwork sn = g.GetComponent<SimpleNetwork>();
            return sn;
        }

        public static void DebugMode( bool flg )
        {
            var sn = GetInstance();
            sn.isDebugMode = flg;
        }

        void Update()
        {
            if( myProxy==null ) return;

            foreach( var evObj in evObjs ) {
                if( evObj.Length == 0 ) continue;
                double delay = (double)evObj[(int)EvObj.Delay];
                int counter = (int)((delay-(int)delay) * 1000000);
                if( (int)evObj[(int)EvObj.RequestTo]!=ToInt(RequestTo.None) ) { RemoveEvent(evObj); myProxy.SetEvent(evObj); }
                else if( (int)delay == counter ){
                    RemoveEvent(evObj);
                    if( (int)evObj[(int)EvObj.SendTo]==ToInt(SendTo.Self) ||
                        (int)evObj[(int)EvObj.SendTo]==ToInt(Networking.LocalPlayer) ) ReceiveEvent(evObj);
                    else myProxy.SetEvent(evObj);
                }
            }

            myProxy.SyncEvents();
        }

        public SimpleNetworkBehaviour GetBehaviour( int id )
        {
            return behaviours[id];
        }

        public void SetBehaviour(SimpleNetworkBehaviour behaviour)
        {
            int idx = behaviours.Length;
            string x = Networking.GetUniqueName(behaviour.gameObject);
            for (int i=0; i<behaviours.Length; i++) {
                string y = Networking.GetUniqueName(behaviours[i].gameObject);
                if (y.CompareTo(x) > 0) { idx=i; break; }
            }
            var temp = new SimpleNetworkBehaviour[behaviours.Length+1];
            for (int i=0; i<idx; i++) temp[i] = behaviours[i];
            behaviour._id = idx;
            temp[idx] = behaviour;
            for (int i = idx+1; i<temp.Length; i++) { temp[i]=behaviours[i-1]; temp[i]._id++; }
            behaviours = temp;
        }

        public void AddEvent(Object[] evObj)
        {
            int index = -1;
            for (int i = 0; i < evObjs.Length; i++) if (evObjs[i].Length == 0) { index = i; break; }
            if (index != -1) evObjs[index] = evObj;
            else {
                Object[][] tmp = new Object[evObjs.Length + 1][];
                evObjs.CopyTo(tmp, 0);
                tmp[evObjs.Length] = evObj;
                evObjs = tmp;
            }
        }

        public void RemoveEvent(Object[] evObj)
        {
            for (int i = 0; i < evObjs.Length; i++) if (evObjs[i] == evObj) evObjs[i] = new Object[0];
        }

        private bool IsProxyUsed( int i )
        {
            foreach(int j in refer) if(j == i) return true;
            return false;
        }

        private void AttachProxy( VRCPlayerApi player, SimpleNetworkProxy proxy )
        {
            int pid = player.playerId;
            int[] temp = new int[pid];
            refer.CopyTo(temp, 0);
            refer = temp;
            refer[pid-1] = proxy.GetNum();

            if( isDebugMode ) Debug.Log("SimpleNetwork Assigned Proxy #"+proxy.GetNum()+" to "+player.displayName);
        }

        private void DetachProxy( VRCPlayerApi player )
        {
            int i = player.playerId - 1;
            if( isDebugMode ) Debug.Log("SimpleNetwork Removed Proxy #"+refer[i]);
            refer[i] = 0;
        }

        public int ToInt( SendTo sendto )
        {
            if( sendto == SendTo.Me ) return Networking.LocalPlayer.playerId;
            return proxys.Length + (int)sendto;
        }

        public int ToInt( RequestTo request )
        {
            return proxys.Length + (int)request;
        }

        public int ToInt( VRCPlayerApi player )
        {
            return player.playerId;
        }

        public void OnProxySynced( SimpleNetworkProxy proxy )
        {
            for(int i=0; i<proxy.EventLength(); i++) {
                Object[] evObj = proxy.GetEvent(i);
                var source = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
                int request = (int)evObj[(int)EvObj.RequestTo];
                int sendto = (int)evObj[(int)EvObj.SendTo];
                int delay = (int)evObj[(int)EvObj.Delay];

                if( request == ToInt(RequestTo.Master) && Networking.IsMaster
                 || request == ToInt(RequestTo.Owner) && Networking.IsOwner(source.gameObject)
                 || request == Networking.LocalPlayer.playerId ) {
                    evObj[(int)EvObj.RequestTo]=ToInt(RequestTo.None);
                    AddEvent(evObj);
                }

                if( request != ToInt(RequestTo.None) ) continue;

                bool isReceive = false;
                var behaviour = source; // ひとまず
                if( sendto == proxys.Length+(int)SendTo.All ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.Owner && Networking.IsOwner(behaviour.gameObject) ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.Master && Networking.IsMaster ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.Self && Networking.IsOwner(proxy.gameObject) ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.NotOwner && !Networking.IsOwner(behaviour.gameObject) ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.NotMaster && !Networking.IsMaster ) isReceive = true;
                else if( sendto == proxys.Length+(int)SendTo.NotSelf && !Networking.IsOwner(proxy.gameObject) ) isReceive = true;
                else if( Networking.LocalPlayer.playerId == sendto ) isReceive = true;
                if( !isReceive ) continue;

                ReceiveEvent( evObj );
            }
        }

        public void ReceiveEvent( Object[] evObj )
        {
            var behaviour = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
            if( isDebugMode ) Debug.Log("ReceiveEvent: src="+behaviour._id+" sendto="+(int)evObj[(int)EvObj.SendTo]+" name="+(string)evObj[(int)EvObj.Name]+" value="+evObj[(int)EvObj.Value]+" delay="+(int)evObj[(int)EvObj.Delay]);
            behaviour._ReceiveEvent(evObj);
        }

        public void OnProxyOwnershipTransferred( SimpleNetworkProxy proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            AttachProxy(player, proxy);
            if( Networking.LocalPlayer == player ) myProxy = proxy;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if( !Networking.IsMaster ) return;

            for(var i=0; i<proxys.Length; i++) {
                SimpleNetworkProxy proxy = proxys[i];
                if( IsProxyUsed(i) ) continue;
                if( Networking.IsOwner(player, proxy.gameObject) ) OnProxyOwnershipTransferred(proxy);
                else Networking.SetOwner(player, proxy.gameObject);
                break;
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            DetachProxy(player);
        }
    }
}
