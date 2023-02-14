
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace tutinoco
{
    public class SimpleNetworkBehaviour : UdonSharpBehaviour
    {
        public uint _id;
        private SimpleNetwork _sn;
        private char _rs = (char)0x1E;

        protected void Initialize()
        {
            _sn = SimpleNetwork.GetInstance();
            _sn.Register(this);
        }

        public void SendEvent(string name, string value)
        {
            string record = _id.ToString("X")+_rs+name+_rs+value;
            _sn.AddRecord(record);
        }

        public virtual void ReceiveEvent(string name, string value, VRCPlayerApi player) { }
    }
}

