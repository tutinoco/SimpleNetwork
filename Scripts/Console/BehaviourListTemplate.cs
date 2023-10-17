
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BehaviourListTemplate : UdonSharpBehaviour
{
    [SerializeField] private SimpleNetworkConsole console;
    private int index;

    public void SetIndex( int i )
    {
        index = i;
    }

    public void OnClickJump()
    {
        console.JumpToBehaviour(index);
    }

    public void OnClickSend()
    {
        console.ChangeSendEvent(index.ToString());
    }
}
