using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleNetworkProxy : SimpleNetworkEventSync
    {
        private int num;

        public override void Start()
        {
            num = gameObject.transform.GetSiblingIndex();
            base.Start();
        }

        public int GetNum()
        {
            return num;
        }

        public override void OnPreSerialization()
        {
            isWaiting = false;
            sn.OnEventSynced(this);
        }

        public override void OnDeserialization()
        {
            if( !isInitialSyncComplete ) { isInitialSyncComplete=true; return; }            
            if( sn != null ) sn.OnEventSynced(this);
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if( player.isMaster ) return; // Proxy解除時の対策がこれでいいかもっと検討したほうがいい
            sn.OnProxyOwnershipTransferred(this);
        }
    }
}