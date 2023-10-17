using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco;

public class SimpleNetworkObject : SimpleNetworkBehaviour
{
    private Rigidbody rigidbody;

    void Start()
    {
        SimpleNetworkInit();
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public override void ReceiveEvent(string name)
    {
        if( name == "TeleportTo" ) {
            transform.position = GetVector3(0);
        }

        if( name == "Jump" ) {
            if( rigidbody == null ) return;
            rigidbody.AddForce(new Vector3(0,1,0)*GetFloat(), ForceMode.Impulse);
        }
    }
}
