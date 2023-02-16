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
        private string command;
        private char gs = (char)0x1D;
        private char rs = (char)0x1E;
        private bool isInit;
        private bool isReady;

        private static string GenerateCmdId() { return string.Format("{0:X4}", UnityEngine.Random.Range(0, 65536)); }

        public static SimpleNetwork GetInstance()
        {
            GameObject g = GameObject.Find("SimpleNetwork");
            SimpleNetwork sn = g.GetComponent<SimpleNetwork>();
            sn.Initialize();
            return sn;
        }

        private void Initialize()
        {
            if( isInit ) return;
            isInit = true;

            behaviours = new SimpleNetworkBehaviour[0];
            refer = new int[0];
            command = GenerateCmdId();

            int len = gameObject.transform.childCount;
            proxys = new SimpleNetworkProxy[len+1];
            for(int i=1; i<len; i++) {
                Transform t = transform.GetChild(i);
                proxys[i] = t.gameObject.GetComponent<SimpleNetworkProxy>();
                proxys[i].id = i;
                proxys[i].sn = this;
            }
        }

        void Update()
        {
            if( !isReady ) return;
            
            if( command.Length!=4 ) {
                GetProxy(Networking.LocalPlayer).Sync(command);
                command = GenerateCmdId();
            }

            foreach( SimpleNetworkBehaviour behaviour in behaviours ) behaviour._Update();
        }

        public bool IsReady() { return isReady; }

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

        public void ExecRecord( string record, VRCPlayerApi player=null )
        {
            if( player == null ) player = Networking.LocalPlayer;
            string[] data = record.Split(rs);
            uint i = UInt32.Parse(data[0], System.Globalization.NumberStyles.HexNumber);
            behaviours[i].ReceiveEvent(data[1], data[2]);
        }

        public void SendRecord( string record )
        {
            command = command+(command.Length==4?"":gs.ToString())+record;
        }

        public string CreateRecord( string name, string value, string target )
        {
            return target+rs+name+rs+value;
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

        private bool IsProxyUsed( int i )
        {
            foreach(int j in refer) if(j == i) return true;
            return false;
        }

        public void OnProxySynced( string cmd, SimpleNetworkProxy proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            foreach( string record in cmd.Substring(4).Split(gs) ) ExecRecord( record, player );
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if( Networking.LocalPlayer == player ) Ready();
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

        public void OnProxyOwnershipTransferred( SimpleNetworkProxy proxy )
        {
            VRCPlayerApi player = Networking.GetOwner(proxy.gameObject);
            AttachProxy(player, proxy);
            if( Networking.LocalPlayer == player ) Ready();
        }

        private void Ready()
        {
            if( isReady ) return;
            isReady = true;
            foreach( SimpleNetworkBehaviour behaviour in behaviours ) behaviour.OnSimpleNetworkReady();
        }
    }
}
