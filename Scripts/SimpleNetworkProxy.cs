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

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(cmd))] private string _cmd;
        public string cmd {
            get { return _cmd; }
            set { sn.ReceiveCommand(_cmd=value, Networking.GetOwner(gameObject)); }
        }

        public void SendCommand(string command)
        {
            cmd = command;
            RequestSerialization();
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            sn.OnProxyOwnershipTransferred(this);
        }
    }
}