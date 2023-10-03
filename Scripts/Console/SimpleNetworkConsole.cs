
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco;
using UnityEngine.UI;

public class SimpleNetworkConsole : SimpleNetworkBehaviour
{
    [SerializeField] private GameObject behaviourListContent;
    [SerializeField] private GameObject behaviourListTemplate;
    [SerializeField] private Text logText;

    [SerializeField] private Dropdown inputRequestTo;
    [SerializeField] private Dropdown inputSendTo;
    [SerializeField] private InputField inputEventName;
    [SerializeField] private InputField inputEventValue;
    [SerializeField] private InputField inputTarget;
    [SerializeField] private Slider inputDelay;

    void Start()
    {
        SimpleNetworkInit("__CONSOLE__");
        logText.text = "";
    }

    public void AddLog( string text )
    {
        logText.text += text+"\n";
    }

    public void AddBehaviour( int index, string name )
    {
        GameObject elem = Instantiate(behaviourListTemplate);
        elem.SetActive(true);
        elem.gameObject.GetComponent<BehaviourListTemplate>().SetIndex(index);
        elem.transform.parent = behaviourListContent.transform;
        elem.transform.localPosition = behaviourListTemplate.transform.localPosition;
        elem.transform.localRotation = behaviourListTemplate.transform.localRotation;
        elem.transform.localScale = behaviourListTemplate.transform.localScale;
        elem.transform.GetChild(0).gameObject.GetComponent<Text>().text = index.ToString();
        elem.transform.GetChild(1).gameObject.GetComponent<Text>().text = name;
    }

    public void ChangeBehaviourList()
    {
        gameObject.transform.GetChild(0).GetChild(5).gameObject.SetActive(true);
        gameObject.transform.GetChild(0).GetChild(6).gameObject.SetActive(false);
        gameObject.transform.GetChild(0).GetChild(7).gameObject.SetActive(false);
    }

    public void ChangeJoinSyncEventList()
    {
        gameObject.transform.GetChild(0).GetChild(5).gameObject.SetActive(false);
        gameObject.transform.GetChild(0).GetChild(6).gameObject.SetActive(true);
        gameObject.transform.GetChild(0).GetChild(7).gameObject.SetActive(false);
    }

    public void ChangeSendEvent()
    {
        gameObject.transform.GetChild(0).GetChild(5).gameObject.SetActive(false);
        gameObject.transform.GetChild(0).GetChild(6).gameObject.SetActive(false);
        gameObject.transform.GetChild(0).GetChild(7).gameObject.SetActive(true);
    }

    public void JumpToBehaviour( int index )
    {
        SimpleNetworkBehaviour behaviour = _sn.GetBehaviour(index);
        VRCPlayerApi player = Networking.LocalPlayer;
        player.TeleportTo(behaviour.transform.position, new Quaternion(0,0,0,1));
    }

    public void OnClickSendEvent()
    {
        string requestTo = inputRequestTo.itemText.text;
        string sendTo = inputSendTo.itemText.text;
        string name = inputEventName.text;
        string value = inputEventValue.text;
        string target = inputTarget.text;
        int delay = (int)inputDelay.value;

        if( name == "" ) return;

        object[] evObj = new object[(int)EvObj.Length];

        evObj[(int)EvObj.Source] = this;

        RequestTo r = RequestTo.None;
        if( requestTo == "Owner" ) r = RequestTo.Owner;
        else if( requestTo == "Master" ) r = RequestTo.Master;
        else if( requestTo == "Server" ) r = RequestTo.Server;
        evObj[(int)EvObj.RequestTo] = r;

        SendTo s = SendTo.All;
        if( requestTo == "Owner" ) s = SendTo.Owner;
        else if( requestTo == "Master" ) s = SendTo.Master;
        else if( requestTo == "Self" ) s = SendTo.Self;
        else if( requestTo == "NotOwner" ) s = SendTo.NotOwner;
        else if( requestTo == "NotMaster" ) s = SendTo.NotMaster;
        else if( requestTo == "NotSelf" ) s = SendTo.NotSelf;
        else if( requestTo == "Me" ) s = SendTo.Me;
        evObj[(int)EvObj.SendTo] = s;

        evObj[(int)EvObj.Name] = name;
        evObj[(int)EvObj.Value] = SimpleNetwork.StringToValues(value);
        evObj[(int)EvObj.Target] = target;
        evObj[(int)EvObj.Delay] = delay;

        RequestEvent(evObj);
    }
}
