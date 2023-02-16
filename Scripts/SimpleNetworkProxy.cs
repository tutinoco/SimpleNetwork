using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleNetworkProxy : UdonSharpBehaviour
    {
        public SimpleNetwork sn;
        public int id;

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(cmd))] private string _cmd;
        public string cmd {
            get { return _cmd; }
            set { sn.OnProxySynced(_cmd=value, this); }
        }

        public void Sync(string command)
        {
            cmd = command;
            RequestSerialization();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if( player.isMaster ) return; // Proxy解除時の対策がこれでいいかもっと検討したほうがいい
            sn.OnProxyOwnershipTransferred(this);
        }
    }
}