
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class JoinEventListTemplate : UdonSharpBehaviour
{
    [SerializeField] private SimpleNetworkConsole console;
    [SerializeField] private Toggle arrow;
    private string name;
    private string target;

    void Start()
    {
    }

    public void SetName( string str )
    {
        name = str;
    }

    public void SetTarget( string str )
    {
        target = str;
    }

    public void OnClickArrow()
    {
        gameObject.transform.GetChild(1).gameObject.SetActive(arrow.isOn);
    }

    public void OnClickDelete()
    {
        console.DeleteJoinEvent(name, target);
    }
}
