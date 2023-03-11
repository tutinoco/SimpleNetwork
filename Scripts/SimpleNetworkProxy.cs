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

        void Start()
        {
            num = gameObject.transform.GetSiblingIndex();
        }

        public int GetNum()
        {
            return num;
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