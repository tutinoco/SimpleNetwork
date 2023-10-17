using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using System.Text.RegularExpressions;

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
        [SerializeField] private SimpleNetworkConsole console;
        private SimpleNetworkBehaviour[] behaviours = new SimpleNetworkBehaviour[0];
        [SerializeField] private SimpleNetworkEventSync[] proxys;
        [SerializeField] public SimpleNetworkEventSync joinSync;
        private SimpleNetworkEventSync myProxy;
        private int[] refer = new int[0];

        [Header("コンソールにアクセスできる人を登録（空なら誰でも操作できます）")]
        [SerializeField] public string[] administrators;
        
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

        public static object[] StringToValues(string strvalue)
        {
            if( strvalue == "" ) return null;

            string[] values = strvalue.Split(',');
            object[] objs = new object[values.Length];

            for(int i = 0; i < values.Length; i++) {
                string value = values[i];

                float _f;
                long _l;
                double _d;
                int _i;

                if( value[value.Length-1]=='f' && float.TryParse(value.Substring(0,value.Length-1), out _f) ) objs[i]=_f;
                else if( value[value.Length-1]=='l' && long.TryParse(value, out _l) ) objs[i]=_l;
                else if( value.IndexOf('.')!=-1 && double.TryParse(value, out _d) ) objs[i]=_d;
                else if( int.TryParse(value, out _i) ) objs[i]=_i;
                else objs[i]=value;
            }

            return objs;
        }

        void Start()
        {
            if( administrators.Length > 0 ) {
                console.gameObject.SetActive(false);
                string name = Networking.LocalPlayer.displayName;
                foreach (var m in administrators) if(m == name) { console.gameObject.SetActive(true); break; }
            }
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
                    if(isDebugMode) DebugEventLog("<color=\"#ff6188\">SendEvent:</color>", evObj);
                    RemoveEvent(evObj);
                    if( (int)evObj[(int)EvObj.SendTo]==(int)SendTo.Self ||
                        (int)evObj[(int)EvObj.SendTo]==Networking.LocalPlayer.playerId+(int)SendTo.Length ) ReceiveEvent(evObj, (string)evObj[(int)EvObj.Target]);
                    else myProxy.SetEvent(evObj);
                } else { evObj[(int)EvObj.Delay] = delay+0.000001; }
            }

            myProxy.SyncEvents();
        }

        public static string ValuesToString(object v)
        {
            string s="";
            if( v != null ) {
                if(v.GetType()==typeof(object[])) {
                    foreach(var w in(object[])v) s+=", "+w.ToString();
                    s=s.Substring(1);
                } else {
                    s = v.ToString();
                }
            } else {
                s = "null";
            }

            return s;
        }

        public void DebugEventLog( string label, object[] evObj ) {
            string s = ValuesToString(evObj[(int)EvObj.Value]);
            string t = string.Format("source={0} sendto={1} target={2} delay={3} name={4} value={5}",((SimpleNetworkBehaviour)evObj[(int)EvObj.Source])._id,evObj[(int)EvObj.SendTo],((string)evObj[(int)EvObj.Target]=="")?"[source]":evObj[(int)EvObj.Target],evObj[(int)EvObj.Delay],evObj[(int)EvObj.Name],s);

            DateTime dt = DateTime.Now;            
            console.AddLog(label+" <color=grey>["+dt.ToString("HH:mm:ss")+"] </color>"+t);
        }

        public bool IsReady() { return myProxy != null; }

        public SimpleNetworkBehaviour[] GetBehaviours() { return behaviours; }
        public SimpleNetworkBehaviour[] GetBehaviours( string find )
        {
            SimpleNetworkBehaviour[] list = new SimpleNetworkBehaviour[0];

            foreach( var behaviour in behaviours ) {
                if( !IsMatch(behaviour.gameObject.name, find) ) continue;
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

        private void AttachProxy( VRCPlayerApi player, SimpleNetworkEventSync proxy )
        {
            int pid = player.playerId;
            int[] temp = new int[pid];
            refer.CopyTo(temp, 0);
            refer = temp;
            refer[pid-1] = proxy.gameObject.transform.GetSiblingIndex();

            if( isDebugMode ) Debug.Log("SimpleNetwork Assigned Proxy #"+refer[pid-1]+" to "+player.displayName+" (playerId: "+player.playerId+")");
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
            if( joinSync == eventSync ) console.OnJoinEventSynced();

            if( joinSync == eventSync && eventSync.isFirstSyncDone() ) return;
            if( joinSync != eventSync && !eventSync.isFirstSyncDone() ) return;

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

            else if( name == "REMOVE_JOINEVENT" ) {
                object[] val = (object[])evObj[(int)EvObj.Value];
                string n = (string)val[0];
                string t = (string)val[1];
                if( t != "" ) joinSync.RemoveEvents(n, t);
//                else joinSync.RemoveEvents(name, (SimpleNetworkBehaviour)evObj[(int)EvObj.Source]);
                joinSync.SyncEvents();
            }

            else {
                evObj = (object[])evObj.Clone();
                evObj[(int)EvObj.RequestTo] = RequestTo.None;
                evObj[(int)EvObj.SendTo] = SendTo.All;
                joinSync.SetEvent(evObj);
                joinSync.SyncEvents();
            }
        }

        public void ReceiveEvent( object[] evObj, string target )
        {
            if(isDebugMode) DebugEventLog("<color=#3bbfbf>ReceiveEvent:</color>", evObj);
            
            int index = 0;
            if( int.TryParse(target, out index) ) {
                var b = behaviours[index];
                b._findIndex = 0;
                b.ReceivesEvent(evObj);
            } else if( target == "" ) {
                var b = (SimpleNetworkBehaviour)evObj[(int)EvObj.Source];
                b._findIndex = 0;
                b.ReceivesEvent(evObj);
            } else {
                var matchs = GetBehaviours(target);
                for(int i=0; i<matchs.Length; i++) {
                    matchs[i]._findIndex = i;
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

        public void OnProxyOwnershipTransferred( SimpleNetworkEventSync proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            AttachProxy(player, proxy);

            if( Networking.LocalPlayer != player ) return;            
            myProxy = proxy;

            if( isDebugMode ) Debug.Log("SimpleNetwork BehavioursList ================================");
            for(int i=0; i<behaviours.Length; i++) { 
                behaviours[i].OnSimpleNetworkReady();
                if( isDebugMode ) Debug.Log(i+": "+ behaviours[i].gameObject.name);
                console.AddBehaviour(i, behaviours[i].gameObject.name);
            }
            if( isDebugMode ) Debug.Log("========================================== BehavioursList End");
        }

/*
        // イベントを送る方法メモ
        public void AddConsoleEvent( string name, string text )
        {
            object[] evObj = new object[(int)EvObj.Length];
            evObj[(int)EvObj.Source] = this;
            evObj[(int)EvObj.RequestTo] = RequestTo.None;
            evObj[(int)EvObj.SendTo] = SendTo.All;
            evObj[(int)EvObj.Name] = name;
            evObj[(int)EvObj.Value] = text;
            evObj[(int)EvObj.Target] = "Console";
            evObj[(int)EvObj.Delay] = 0;
            evObj[(int)EvObj.JoinSync] = JoinSync.None;
            evObj[(int)EvObj.Sender] = Networking.LocalPlayer.playerId;
            AddEvent(evObj);
        }
*/
        public void Test()
        {
            HandleAllEvents(joinSync);
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if( !Networking.IsMaster ) return;

            for(var i=0; i<proxys.Length; i++) {
                SimpleNetworkEventSync proxy = proxys[i];
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
