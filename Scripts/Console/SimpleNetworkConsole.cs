
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco;
using UnityEngine.UI;

public class SimpleNetworkConsole : SimpleNetworkBehaviour
{
    [SerializeField] private SimpleNetworkEventSync joinSync;
    [SerializeField] private Text logText;

    [SerializeField] private GameObject behaviourListContent;
    [SerializeField] private GameObject behaviourListTemplate;

    [SerializeField] private GameObject joinEventListContent;
    [SerializeField] private GameObject joinEventListTemplate;
    [SerializeField] private GameObject joinEventTreeView;
    [SerializeField] private GameObject joinEventListView;
    [SerializeField] private Toggle inputToggleJoinEventView;

    [SerializeField] private Dropdown inputRequestTo;
    [SerializeField] private Dropdown inputSendTo;
    [SerializeField] private InputField inputEventName;
    [SerializeField] private InputField inputEventValue;
    [SerializeField] private InputField inputTarget;
    [SerializeField] private Slider inputDelay;
    [SerializeField] private Text inputDelayValue;

    private int joinSyncCount = 0;

    void Start()
    {
        joinEventListTemplate.transform.parent = joinEventListTemplate.transform.parent.parent;
        gameObject.name = "__CONSOLE__";
        SimpleNetworkInit();
        logText.text = "";
    }

    void Update()
    {
        inputDelayValue.text = inputDelay.value+"<size=8> frame</size>";
    }

    public void AddLog( string text )
    {
        logText.text += text+"\n";
    }

    public void AddJoinEvent( int order, string target, string name, string value )
    {
        if( !inputToggleJoinEventView.isOn ) {
            Text text2 = joinEventListView.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<Text>();
            text2.text += "<color=grey>"+order+"</color>\t"+target+"-"+name+"\t"+value+"\n";
            return;
        }

        JoinEventListTemplate b = null;
        for( int i=0; i<joinEventListContent.transform.childCount; i++ ) {
            Transform t = joinEventListContent.transform.GetChild(i);
            if( t.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text == target+"-"+name ) { b=t.gameObject.GetComponent<JoinEventListTemplate>(); break; }
        }

        if( b == null ) {
            b = Instantiate(joinEventListTemplate).GetComponent<JoinEventListTemplate>();
            b.gameObject.SetActive(true);
            b.SetName(name);
            b.SetTarget(target);
            b.transform.parent = joinEventListContent.transform;
            b.transform.localPosition = joinEventListTemplate.transform.localPosition;
            b.transform.localRotation = joinEventListTemplate.transform.localRotation;
            b.transform.localScale = joinEventListTemplate.transform.localScale;
            b.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Text>().text = target+"-"+name;
        }

        Text text = b.transform.GetChild(1).gameObject.GetComponent<Text>();
        text.text += (text.text.Length==0?"":"\n")+"\t\t\t<color=grey>"+order+"</color>\t";
        text.text += value;
    }

    public void OnJoinEventSynced()
    {
        if( !inputToggleJoinEventView.isOn ) {
            joinEventListView.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<Text>().text = "";
        } else {
            int len = joinEventListContent.transform.childCount;
            for( int i=len-1; i>=0; i-- ) {
                Transform t = joinEventListContent.transform.GetChild(i);
                t.parent = t.parent.parent;
                Destroy(t.gameObject);
            }
        }

        for(int i=0; i<joinSync.EventLength(); i++ ) {
            object[] evObj = joinSync.GetEvent(i);
            string target = (string)evObj[(int)EvObj.Target];
            string name = (string)evObj[(int)EvObj.Name];
            string value = SimpleNetwork.ValuesToString(evObj[(int)EvObj.Value]);
            AddJoinEvent(i, target, name, value);
        }
    }

    public void ToggleJoinEventView()
    {
        joinEventTreeView.SetActive(inputToggleJoinEventView.isOn);
        joinEventListView.SetActive(!inputToggleJoinEventView.isOn);
        OnJoinEventSynced();
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

    public void ChangeSendEvent( string target )
    {
        if( target != null ) inputTarget.text = target;
        ChangeSendEvent();
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

    public void DeleteJoinEvent( string name, string target )
    {
        RemoveJoinEvent(name, target);
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
