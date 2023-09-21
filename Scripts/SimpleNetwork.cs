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
        Me,
        Length,
    }

    public enum RequestTo
    {
        None,
        Owner,
        Master,
        Server,
        Length,
    }

    public enum EvObj
    {
        Source,
        RequestTo,
        SendTo,
        Name,
        Value,
        Target,
        Delay,
        JoinSync,
        Sender,
        Length,
    }

    public enum JoinSync
    {
        None,
        Latest,
        Logging,
    }

    public enum Timing
    {
        Update,
        FixedUpdate,
        LateUpdate,
        PostLateUpdate,
        Joined,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleNetwork : UdonSharpBehaviour
    {
        private bool isDebugMode;

        private object[][] evObjs = new object[0][];
        private SimpleNetworkBehaviour[] behaviours = new SimpleNetworkBehaviour[0];
        [SerializeField] private SimpleNetworkProxy[] proxys;
        [SerializeField] public SimpleNetworkEventSync joinSync;
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
            if( !IsReady() ) return;

            foreach( var evObj in evObjs ) {
                if( evObj.Length == 0 ) continue;

                // Handle request event.
                int request = (int)evObj[(int)EvObj.RequestTo];
                if( request != (int)RequestTo.None ) {
                    var source = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
                    bool isRequestToSelf = request==(int)RequestTo.Master && Networking.IsMaster || request==(int)RequestTo.Owner && Networking.IsOwner(source.gameObject);
                    if( isRequestToSelf ) evObj[(int)EvObj.RequestTo]=(int)RequestTo.None;
                    else { RemoveEvent(evObj); myProxy.SetEvent(evObj); continue; }
                }

                // Execute event when delay count is met.
                double delay = (double)evObj[(int)EvObj.Delay];
                int counter = (int)((delay-(int)delay) * 1000000);
                if( (int)delay == counter ){
                    if(isDebugMode){var v=evObj[(int)EvObj.Value];string s="";if(v!=null){if(v.GetType()==typeof(object[])){foreach(var w in(object[])v){s+=","+w.ToString();}s=s.Substring(1);}else{s=(string)v;}}else{s="null";}Debug.Log(string.Format("SimpleNetwork SendEvent: source={0} sendto={1} target={2} delay={3} name={4} v={5}",((SimpleNetworkBehaviour)evObj[(int)EvObj.Source])._id,evObj[(int)EvObj.SendTo],((string)evObj[(int)EvObj.Target]=="")?"[source]":evObj[(int)EvObj.Target],evObj[(int)EvObj.Delay],evObj[(int)EvObj.Name],s));}
                    RemoveEvent(evObj);
                    if( (int)evObj[(int)EvObj.SendTo]==(int)SendTo.Self ||
                        (int)evObj[(int)EvObj.SendTo]==Networking.LocalPlayer.playerId+(int)SendTo.Length ) ReceiveEvent(evObj, (string)evObj[(int)EvObj.Target]);
                    else myProxy.SetEvent(evObj);
                } else { evObj[(int)EvObj.Delay] = delay+0.000001; }
            }

            myProxy.SyncEvents();
        }

        public bool IsReady() { return myProxy != null; }

        public SimpleNetworkBehaviour[] GetBehaviours() { return behaviours; }
        public SimpleNetworkBehaviour[] GetBehaviours( string group )
        {
            SimpleNetworkBehaviour[] list = new SimpleNetworkBehaviour[0];

            foreach( var behaviour in behaviours ) {
                if( !IsMatch(behaviour._group, group) ) continue;
                SimpleNetworkBehaviour[] tmp = new SimpleNetworkBehaviour[list.Length+1];
                list.CopyTo(tmp, 0);
                tmp[tmp.Length-1] = behaviour;
                list = tmp;
            }

            return list;
        }

        public SimpleNetworkBehaviour GetBehaviour( int id )
        {
            if( id >= behaviours.Length ) Debug.Log("SimpleNetwork Error: Attempted to get SimpleNetworkBehaviour number "+id+" that is not unmanaged.");
            return behaviours[id];
        }           

        public int SetBehaviour(SimpleNetworkBehaviour behaviour)
        {
            behaviour._sn = this;

            int idx = behaviours.Length;
            if( !IsReady() ) {
                string x = Networking.GetUniqueName(behaviour.gameObject);
                for (int i=0; i<behaviours.Length; i++) {
                    string y = Networking.GetUniqueName(behaviours[i].gameObject);
                    if (y.CompareTo(x) > 0) { idx=i; break; }
                }
            }
            var tmp = new SimpleNetworkBehaviour[behaviours.Length+1];
            for (int i=0; i<idx; i++) tmp[i] = behaviours[i];
            behaviour._id = idx;
            tmp[idx] = behaviour;
            for (int i = idx+1; i<tmp.Length; i++) { tmp[i]=behaviours[i-1]; tmp[i]._id++; }
            behaviours = tmp;

            return idx;
        }

        public void AddEvent(object[] evObj)
        {
            int index = -1;
            for (int i=0; i<evObjs.Length; i++) if (evObjs[i].Length==0) { index=i; break; }
            if (index != -1) evObjs[index] = evObj;
            else {
                object[][] tmp = new object[evObjs.Length+1][];
                evObjs.CopyTo(tmp, 0);
                tmp[tmp.Length-1] = evObj;
                evObjs = tmp;
            }
        }

        public void RemoveEvent(object[] evObj)
        {
            for (int i = 0; i < evObjs.Length; i++) if (evObjs[i] == evObj) evObjs[i] = new object[0];
        }

        public int[] GetPlayers()
        {
            int max = 0;
            for(int i=0; i<refer.Length; i++) if( refer[i] > 0 ) max++;
            int[] players = new int[max];
            int cnt = 0;
            for(int i=0; i<refer.Length; i++) {
                if( refer[i] > 0 ) {
                    players[cnt] = i;
                    cnt++;
                }
            }
            return players;
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

            if( isDebugMode ) Debug.Log("SimpleNetwork Assigned Proxy #"+proxy.GetNum()+" to "+player.displayName+" (playerId: "+player.playerId+")");
        }

        private void DetachProxy( VRCPlayerApi player )
        {
            int i = player.playerId - 1;
            if( isDebugMode ) Debug.Log("SimpleNetwork Removed Proxy #"+refer[i]);
            refer[i] = 0;
        }
/*
        public void SetJoinEvent( object[] evObj )
        {
            evObj = (object[])evObj.Clone();
            evObj[(int)EvObj.SendTo] = SendTo.All;

            string name = (string)evObj[(int)EvObj.Name];
            string target = (string)evObj[(int)EvObj.Target];
            if( target != "" ) ClearJoinEvent(name, target);
            else ClearJoinEvent(name, (SimpleNetworkBehaviour)evObj[(int)EvObj.Source]);

            Debug.Log("SetJoinEvent: "+name+" Length:"+joinSync.EventLength());
            joinSync.SetEvent(evObj);
            joinSync.SyncEvents();
            Debug.Log("SetJoinEvent: "+name+" Length:"+joinSync.EventLength());
        }

        public void LoggedJoinEvent( object[] evObj )
        {
            evObj = (object[])evObj.Clone();
            evObj[(int)EvObj.SendTo] = SendTo.All;

            string name = (string)evObj[(int)EvObj.Name];
            joinSync.SetEvent(evObj);
            joinSync.SyncEvents();
            Debug.Log("LoggedJoinEvent: "+name+" Length:"+joinSync.EventLength());
        }

        public void ClearJoinEvent( string name, SimpleNetworkBehaviour behaviour ){ joinSync.RemoveEvents(name, behaviour); }
        public void ClearJoinEvent( string name, string target ){ joinSync.RemoveEvents(name, target); }
        */

        public void OnEventSynced( SimpleNetworkEventSync eventSync )
        {
            Debug.Log("OnEventSynced EventLength: "+eventSync.EventLength());
            HandleAllEvents(eventSync);
        }

        private void HandleAllEvents( SimpleNetworkEventSync events )
        {
            for(int i=0; i<events.EventLength(); i++) {
                object[] evObj = events.GetEvent(i);
                string name = (string)evObj[(int)EvObj.Name];
                var source = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
                int request = (int)evObj[(int)EvObj.RequestTo];
                int sendto = (int)evObj[(int)EvObj.SendTo];
                int sender = (int)evObj[(int)EvObj.Sender];
                string target = (string)evObj[(int)EvObj.Target];

                if( request == (int)RequestTo.Server ) {
                    if( Networking.IsMaster ) ReceiveServerEvent(evObj);
                    continue;
                } else if( request == (int)RequestTo.Master && Networking.IsMaster
                 || request == (int)RequestTo.Owner && Networking.IsOwner(source.gameObject)
                 || request == Networking.LocalPlayer.playerId+(int)RequestTo.Length ) {
                    Debug.Log("REQUEST RECEIVED!!!!!!!!!!!: "+request+" -> "+sendto);
                    if( request > (int)RequestTo.Length )
                    evObj[(int)EvObj.RequestTo] = (int)RequestTo.None;
                    AddEvent(evObj);
                    continue;
                }

                bool isReceive = false;
                if( sendto == (int)SendTo.All ) isReceive = true;
                else if( sendto == (int)SendTo.Owner && Networking.IsOwner(source.gameObject) ) isReceive = true;
                else if( sendto == (int)SendTo.Master && Networking.IsMaster ) isReceive = true;
                else if( sendto == (int)SendTo.Self && Networking.LocalPlayer.playerId==sender ) isReceive = true;
                else if( sendto == (int)SendTo.NotOwner && !Networking.IsOwner(source.gameObject) ) isReceive = true;
                else if( sendto == (int)SendTo.NotMaster && !Networking.IsMaster ) isReceive = true;
                else if( sendto == (int)SendTo.NotSelf && Networking.LocalPlayer.playerId!=sender ) isReceive = true;
                else if( Networking.LocalPlayer.playerId == sendto+(int)SendTo.Length ) isReceive = true;
                if( !isReceive ) continue;

                ReceiveEvent( evObj, (string)evObj[(int)EvObj.Target] );
            }
        }

        public void ReceiveServerEvent( object[] evObj )
        {
            string name = (string)evObj[(int)EvObj.Name];

            if( name == "DUPLICATE" ) {
                var source = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
                object[] v = (object[])evObj[(int)EvObj.Value];
                var b = Instantiate(((SimpleNetworkBehaviour)v[0]).gameObject, (Vector3)v[1], (Quaternion)v[2]).GetComponent<SimpleNetworkBehaviour>();
                SetBehaviour(b);
                source.OnDuplicateComplete(b);
            }

            else if( name == "CLEAR_JOINEVENT" ) {
                Debug.Log("CLEAR_JOINEVENT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
/*                object[] val = (object[])evObj[(int)EvObj.Value];
                string target = (string)val[1];
                if( target != "" ) joinSync.RemoveEvents((string)val[0], target);
                else joinSync.RemoveEvents((string)val[0], (SimpleNetworkBehaviour)evObj[(int)EvObj.Source]);
                joinSync.SyncEvents();
*/            }

            else {
                evObj = (object[])evObj.Clone();
                evObj[(int)EvObj.RequestTo] = RequestTo.None;
                evObj[(int)EvObj.SendTo] = SendTo.All;
                joinSync.SetEvent(evObj);
                Debug.Log("SET JOIN EVENT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! "+joinSync.EventLength());
                joinSync.SyncEvents();
            }
        }

        public void ReceiveEvent( object[] evObj, string target )
        {
            if(isDebugMode){var v=evObj[(int)EvObj.Value];string s="";if(v!=null){if(v.GetType()==typeof(object[])){foreach(var w in(object[])v){s+=","+w.ToString();}s=s.Substring(1);}else{s=(string)v;}}else{s="null";}Debug.Log(string.Format("SimpleNetwork ReceiveEvent: source={0} sendto={1} target={2} delay={3} name={4} v={5}",((SimpleNetworkBehaviour)evObj[(int)EvObj.Source])._id,evObj[(int)EvObj.SendTo],evObj[(int)EvObj.Target]==""?"[source]":evObj[(int)EvObj.Target],evObj[(int)EvObj.Delay],evObj[(int)EvObj.Name],s));}
            if( target == "" ) {
                var b = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
                b._groupIndex = 0;
                b.ReceivesEvent(evObj);
            } else {
                var matchs = GetBehaviours(target);
                for(int i=0; i<matchs.Length; i++) {
                    matchs[i]._groupIndex = i;
                    matchs[i].ReceivesEvent(evObj);
                }
            }
        }

        private bool IsMatch(string input, string pattern)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pattern)) return false;

            foreach (string p in pattern.Split('|')) {
                bool isMatch = true;
                foreach (string subPattern in p.Split('&')) {
                    int i = 0, j = 0;
                    int starIndex = -1;
                    int matchIndex = 0;

                    while (i < input.Length) {
                        if (j < subPattern.Length && (subPattern[j] == '?' || subPattern[j] == input[i])) { i++; j++; }
                        else if (j < subPattern.Length && subPattern[j] == '*') { starIndex=j; matchIndex=i; j++; }
                        else if (starIndex != -1) { j=starIndex+1; matchIndex++; i=matchIndex; }
                        else { isMatch=false; break; }
                    }

                    while (j < subPattern.Length && subPattern[j] == '*') j++;
                    if (j != subPattern.Length) isMatch=false;
                    if (!isMatch) break;
                }

                if (isMatch) return true;
            }

            return false;
        }

        public void OnProxyOwnershipTransferred( SimpleNetworkProxy proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            AttachProxy(player, proxy);

            if( Networking.LocalPlayer != player ) return;            
            myProxy = proxy;

            if( isDebugMode ) Debug.Log("SimpleNetwork BehavioursList ================================");
            for(int i=0; i<behaviours.Length; i++) { behaviours[i].OnSimpleNetworkReady(); if( isDebugMode ) Debug.Log(i+": "+ behaviours[i]._group); }
            if( isDebugMode ) Debug.Log("========================================== BehavioursList End");
        }

        public void Test()
        {
            HandleAllEvents(joinSync);
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
