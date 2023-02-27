# SimpleNetwork
![SimpleNetworkMonster](https://user-images.githubusercontent.com/14051445/219826822-95ede90d-f5c7-4679-9218-8e9c4c598dc2.png)
SimpleNetworkは、VRChatのNetworkingがしんどい方のためのNetworkingラッパーなUdonSharpモジュールです。
SendCustomNetworkEventメソッドで、引数を扱えない問題を解消する目的で作成されました。

SimpleNetworkは、[SimpleNetworkUdonBehavior](https://github.com/tutinoco/SimpleNetworkUdonBehaviour)の進化版です。

## 特徴
* `SendCustomNetworkEvent`で不可能な引数の送信ができ、メッセージングプログラミングを可能にします。
* 煩わしい所有権から解放され、権限の無いオブジェクトからも低レイテンシーで高速な通信が行えます。
* オブジェクトにグループを設定して、複数のオブジェクトにイベントをイテレーション送信できます。◆
* 大量にイベントを発行しても、自動的に全てのイベントを1回の送信にまとめるため、安定して動作します。
* `All`や`Owner`のほかに、`Master`や`player`を設定して、イベントの受信者を限定することができます。
* `Master`にイベント送信を依頼するなど、他のプレイヤーにイベントの送信を依頼することができます。
* 最後に実行したイベントやイベント履歴をサーバに保存し、後から参加したプレイヤーに送信することができます。◆
* `SendCustomNetworkEventDelayedFrames`に相当するネットワークイベントの遅延送信が可能です。
* シーンに存在しない動的に生成したオブジェクトに対しても通信が可能です。◆

◆の機能はまだ実装中です。

## 使い方
シーンに`SimpleNetwork.prefab`を配置し、適当なクラスを作って`SimpleNetworkBehaviour`を継承して`SendEvent()`メソッドを実行することで、インスタンス内にいるプレイヤー（自分を含む）のオブジェクトに値を届けることができます。
イベントの受信は、サブクラスで`ReceiveEvent`メソッドをオーバーライドすることで可能となり、第一引数にイベント名が、第二引数に値が届きます。

```C#
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco; // 追加して

public class Test : SimpleNetworkBehaviour // 継承して
{
    void Start()
    {
        SimpleNetworkInit(); // 初期化して
        SendEvent("Talk", "こんにちは！"); // イベントを送ると
    }

    public override void ReceiveEvent(string name, string value)
    {
        if( name == "Talk" ) {
            Debug.Log(value); // こんにちは！が全ユーザに届きます。
        }
    }
}
```
上記のコードでは、オブジェクトが自らのイベントを送信していますが、嬉しいのは、値の送信が可能になったことで、命令されたら動くアクションだけをまとめたクラスを作成できるようになることです。

以下は、命令待ちをするモンスタークラスです。
```C#
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco;

public class Monster : SimpleNetworkBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private Text fukidashi;

    void Start()
    {
        SimpleNetworkInit();
    }

    public override void ReceiveEvent(string name) // valueを付けなくても
    {
        // 【上にジャンプ】
        if( name == "jump" ) {
            float power = GetFloat(); // floatで受け取れます。
            rigidbody.AddForce(transform.up*power, ForceMode.Impulse);
        }

        // 【座標にワープ】
        if( name == "warp" ) {
            Vector3 pos = GetVector3(); // Vector3で受け取れます。
            gameObject.transform.position = pos;
        }

        // 【ふきだしに文字表示】
        if( name == "talk" ) {
            fukidashi.text = GetString(); // stringで受け取れます。
        }
    }
}
```
このモンスタークラスを使って作られたモンスターを制御するには、適当な場所に以下のコードを記述します。
```C#
// ジャンプ力5.0で飛ぶ！
monster.SendEvent("jump", 5.0f);

// x:1 y:2 z:3 の座標にワープ！
monster.SendEvent("warp", new Vector3(1.0f, 2.0f, 3.0f));

// ふきだしに文字を表示！
monster.SendEvent("talk", "僕を捕まえられるかな？");
```

### 対応している型
現在`bool` `int` `float` `string` `Vector3`の型の送受信に対応しています。

```C#
public override void ReceiveEvent(string name)
{
    // SendEvent("hoge", true);
    bool boolValue = GetBool();

    // SendEvent("hoge", 5);
    int intValue = GetInt();

    // SendEvent("hoge", 3.14f);
    float floatValue = GetFloat();

    // SendEvent("hoge", "こんにちは");
    string stringValue = GetString();

    // SendEvent("hoge", new Vector3(1.0f, 2.0f, 3.0f));
    Vector3 vector3 = GetVector3();
}
```

値の無いイベントの送信も可能です。
```C#
monster.SendEvent("Init");
```

### 複数の値を送受信する
複数の値を送るには、`Pack()`メソッドを利用します。
対応していない型の値を`Pack()`に含めるとはできません。
```C#
Vector3 position = Vector3(1.0f, 2.0f, 3.0f);
Quaternion rotation = Quaternion.identity;
monster.SendEvent("warp2", Pack(position, rotation));
```

呼び出し元が`SimpleNetworkBehaviour`のサブクラスでない等、`Pack`メソッドを利用できない場合は以下のようにします。
```C#
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Object = System.Object; // 追加して

public class Test : UdonSharpBehaviour
{
    [SerializeField] private Monster monster;

    void Start()
    {
        Vector3 position = Vector3(1.0f, 2.0f, 3.0f);
        Quaternion rotation = Quaternion.identity;
        Objects[] values = new Objects[] {position, rotation};
        monster.SendEvent("warp2", values);
    }
}
```

値の受信方法は以下のようにします。
```C#
public override void ReceiveEvent(string name)
{
    Vector3 position = GetVector3(0);       // 0番目の値をVector3で取得
    Quaternion rotation = GetQuaternion(1); // 1番目の値をQuaternionで取得

    // indexを省略することもできます。
    Vector3 position2 = GetVector3();       // 最初に見つかったVector3を取得
    Quaternion rotation2 = GetQuaternion(); // 最初に見つかったQuaternionを取得
}
```

また、ダイレクトレシーブを使って受信することも可能です。
```C#
public override void ReceiveEvent(string name, Object[] values) // valuesに全ての値が届く
{
    Vector3 position = (Vector3)values[0];       // 0番目の値をVector3で取得
    Quaternion rotation = (Quaternion)values[1]; // 1番目の値をQuaternionで取得
}
```


### メタデータの受信
```C#
public override void ReceiveEvent(string name)
{
    // 送信元オブジェクトを取得します
    SimpleNetworkBehaviour behavior = GetSource();

    // このイベントがどのくらい遅延して送られたかを取得します
    int delay = GetDelay();

    // 送信元プレイヤーを取得します◆
    VRCPlayerApi player = GetSourcePlayer();

    // このイベントが送られたプレイヤー一覧を取得します◆
    VRCPlayerApi[] players = GetSendToPlayers();

    // このイベントを受信したオブジェクト一覧を取得します◆
    SimpleNetworkBehaviour[] behaviors = GetTargetObjects();

    // イベントに設定されたグループ名を取得します◆
    string group = GetGroup();

    // グループでイテレートした際のインデックスを取得します◆
    int index = GetGroupIndex();
}
```

## イベントの送信方法
### ローカルイベント実行
```C#
// 自分のみイベントを実行
ExecEvent("Jump", 5.0f);
```

### イベントの遅延送信
```C#
// 約2秒の120フレーム後にイベント送信
ExecEvent("Jump", 5.0f, 120); // 自分のみ
SendEvent("Jump", 5.0f, 120); // 全員
```

### 送信先の指定
```C#
// 全員にイベントを送信（デフォルト）
SendEvent(SendTo.All, "Jump", 5.0f);

// オブジェクト所有者にイベントを送信
SendEvent(SendTo.Owner, "Jump", 5.0f);

// インスタンスマスターにイベントを送信
SendEvent(SendTo.Master, "Jump", 5.0f);

// 自分自身にイベントを送信
SendEvent(SendTo.Self, "Jump", 5.0f); // ExecEventに同じ
SendEvent(SendTo.Me, "Jump", 5.0f); // RequestEvent()ではSelfとMeは挙動が異なる

// オブジェクト所有者以外にイベントを送信
SendEvent(SendTo.NotOwner, "Jump", 5.0f);

// インスタンスマスター以外にイベントを送信
SendEvent(SendTo.NotMaster, "Jump", 5.0f);

// 自分以外にイベントを送信
SendEvent(SendTo.NotSelf, "Jump", 5.0f);

// 指定のプレイヤーにイベントを送信
SendEvent(player, "Jump", 5.0f);
```

もちろん以下のように送信先を指定した上でイベントの遅延送信を行うこともできます。
```C#
// インスタンスマスターに2秒遅延してイベントを送信
SendEvent(SendTo.Master, "Jump", 5.0f, 120);
```

### イベント送信の依頼
`RequestEvent()`メソッドを使うことで、イベントの送信を他のプレイヤーに依頼することができます。
```C#
// 【イベントの送信を依頼】
// インスタンスマスターから全員にイベントを送信するよう依頼
RequestEvent(RequestTo.Master, "Jump", 5.0f);

// 【SendToと一緒に利用】
// インスタンスマスターからオブジェクトオーナーにイベントを送信するよう依頼
RequestEvent(RequestTo.Master, SendTo.Owner, "Jump", 5.0f);

// 【プレイヤーを直接指定】
// プレイヤーAからプレイヤーBにイベントを送信するよう依頼
RequestEvent(playerA, playerB, "Jump", 5.0f);

// 【遅延送信と一緒に利用】
// オブジェクトオーナー側で60フレーム待機してから自分にイベントを送信するよう依頼
RequestEvent(RequestTo.Owner, SendTo.Me, "Jump", 5.0f, 60);
```

## その他の機能
### デバッグ
```C#
// Debug.Logに送信されたコマンド等が送られる
SimpleNetwork.DebugMode(true);
```

## 仕組み
<p align="center"><img alt="structure" src="https://user-images.githubusercontent.com/14051445/220154122-edd5e748-78af-4085-b28f-79192161deab.png" width="60%"></p>

80人までの同時接続が可能な`SimpleNetworkProxy`（以下、プロキシ）が`SimpleNetwork.prefab`に用意されており、プロキシには各型の`UdonSynced`同期変数の配列が用意されています。

プレイヤーがワールドに参加すると、利用可能なプロキシが自動的に割り当てられるため、全てのプレイヤーは専用プロキシを所有します。

`SimpleNetwork`は、シーンに存在する全ての`SimpleNetworkBehavior`から発行されたイベントを収集しており、`SimpleNetworkBehavior`を継承したオブジェクトで`SendEvent()`メソッドが実行されると、イベント情報が`SimpleNetwork`に送信され、収集した複数のイベントの名前や値などの情報は1フレーム毎にひとつ同期にまとめられます。

同期する値は、専用プロキシの各型の同期変数に投げられ、これにより全てのプレイヤーに値が同期され、同期された値は`SimpleNetwork`がリアルタイムに受信します。

プロキシは、`OnDeserialization()`によってコードの受信を検知しており、`SimpleNetwork`は、受け取った値をイベントオブジェクトにまとめて配列を作成し、各イベントを`ReceiveEvent()`に送信します。このとき、イベントのターゲットに含まれないプレイヤーは、受信したイベントを無視するよう設定されています。

実行が許されたイベントは、適切なオブジェクトの`ReceiveEvent()`メソッドが呼ばれ、`SimpleNetworkBehavior`に値を保存、`GetInt()`などのメソッドにより値にアクセスすことで、引数付きのネットワークイベントの発行が実現されています。

また、プロキシの`UdonBehaviourSyncMode`は`Manual`に設定されており、同期変数は高速に同期されるので、`SimpleNetwork`の`SendEvent()`は、`SendCustomNetworkEvent()`よりも高速に送受信できます。