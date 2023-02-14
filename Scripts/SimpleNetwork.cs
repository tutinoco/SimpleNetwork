using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual), DefaultExecutionOrder(-1024)]
    public class SimpleNetwork : UdonSharpBehaviour
    {
        private SimpleNetworkBehaviour[] behaviours = new SimpleNetworkBehaviour[0];
        private SimpleNetworkProxy[] proxys;
        [UdonSynced(UdonSyncMode.None)] private int[] refer;
        private string command;
        private char gs = (char)0x1D;
        private char rs = (char)0x1E;
        private bool isReady;

        public static SimpleNetwork GetInstance()
        {
            GameObject g = GameObject.Find("SimpleNetwork");
            return g.GetComponent<SimpleNetwork>();
        }

        void Start()
        {
            command = GenerateCmdId();

            int len = gameObject.transform.childCount;
            proxys = new SimpleNetworkProxy[len+1];
            refer = new int[len+1];
            for(int i=1; i<len; i++) {
                Transform t = transform.GetChild(i);
                proxys[i] = t.gameObject.GetComponent<SimpleNetworkProxy>();
                proxys[i].sn = this;
            }
        }

        void Update()
        {
            if( isReady && command.Length!=4 ) {
                int pid = Networking.LocalPlayer.playerId;
                SimpleNetworkProxy p = proxys[refer[pid]];
                p.SendCommand(command);
                command = GenerateCmdId();
            } 
        }

        public void ReceiveCommand( string cmd, VRCPlayerApi player )
        {
            foreach( string record in cmd.Substring(4).Split(gs) ) {
                string[] data = record.Split(rs);
                uint i = UInt32.Parse(data[0], System.Globalization.NumberStyles.HexNumber);
                behaviours[i].ReceiveEvent(data[1], data[2], player);
            }
        }

        private string GenerateCmdId() { return string.Format("{0:X4}", UnityEngine.Random.Range(0, 65536)); }

        public void Register(SimpleNetworkBehaviour behaviour)
        {
            int index = behaviours.Length;
            string x = Networking.GetUniqueName(behaviour.gameObject);
            for (int i = 0; i < behaviours.Length; i++) {
                string y = Networking.GetUniqueName(behaviours[i].gameObject);
                if (y.CompareTo(x) > 0) { index=i; break; }
            }
            var temp = new SimpleNetworkBehaviour[behaviours.Length + 1];
            for (int i = 0; i < index; i++) temp[i] = behaviours[i];
            behaviour._id = (uint)index;
            temp[index] = behaviour;
            for (int i = index+1; i<temp.Length; i++) { behaviours[i-1]._id++; temp[i]=behaviours[i-1]; }
            behaviours = temp;
        }

        public void AddRecord( string record )
        {
            command = command+(command.Length==4?"":gs.ToString())+record;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if( !Networking.IsMaster ) return;

            int pid = player.playerId;
            for(var i=0; i<proxys.Length; i++) {
                SimpleNetworkProxy proxy = proxys[i];
                if( IsProxyUsed(i) ) continue;
                refer[pid] = i;
                if( Networking.IsOwner(proxy.gameObject) ) OnProxyOwnershipTransferred(proxy);
                else Networking.SetOwner(player, proxy.gameObject);
                Debug.Log("新しいユーザに"+i+"番のProxyを割り当てました");
                break;
            }

            RequestSerialization();
        }

        private bool IsProxyUsed( int i )
        {
            foreach(int j in refer) if(j == i) return true;
            return false;
        }

        public void OnProxyOwnershipTransferred( SimpleNetworkProxy proxy )
        {
            if( Networking.LocalPlayer == Networking.GetOwner(proxy.gameObject) ) isReady = true;
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            int pid = player.playerId;
            Debug.Log(refer[pid]+"番のProxyを解除しました");
            refer[pid] = 0;
        }
    }
}
