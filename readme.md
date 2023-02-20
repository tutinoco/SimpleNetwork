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
* `Master`にイベント送信を依頼するなど、他のプレイヤーにイベントの送信を仲介してもらうことができます。◆
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
using tutinoco;

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
            Debug.Log(value); // こんにちは！が全ユーザに届きます
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

    public override void ReceiveEvent(string name, string value)
    {
        // 上にジャンプする
        if( name == "jump" ) {
            float power = GetFloat(value);
            rigidbody.AddForce(transform.up*power, ForceMode.Impulse);
        }

        // 指定の座標にワープする
        if( name == "warp" ) {
            Vector3 pos = GetVector3(value);
            gameObject.transform.position = pos;
        }

        // ふきだしに文字を表示
        if( name == "talk" ) {
            fukidashi.text = value;
        }
    }
}
```
このモンスタークラスを使って作られたモンスターを制御するには、適当な場所に以下のコードを記述します。
```C#
// 5のパワーでジャンプ
monster.SendEvent("jump", 5.0f);

// (X:1, Y:2, Z:3)の座標にワープ
monster.SendEvent("warp", new Vector3(1.0f, 2.0f, 3.0f));

// ふきだしに文字を表示
monster.SendEvent("talk", "僕を捕まえられるかな？");
```

### 対応している型
```C#
// SendEvent("hoge", true);
bool boolValue = GetBool(value);

// SendEvent("hoge", 5);
int intValue = GetInt(value);

// SendEvent("hoge", 3.14f);
float floatValue = GetFloat(value);

// SendEvent("hoge", "こんにちは");
string stringValue = GetString(value);

// SendEvent("hoge", new Vector3(1.0f, 2.0f, 3.0f));
Vector3 vector3 = GetVector3(value);

// SendEvent("hoge", this);
Monster monster = (Monster)GetObject(value); // 初期化されたSimpleNetworkBehaviourのみ受信可能
```

### メタデータの受信
```C#
// 送信元プレイヤーを取得します
VRCPlayerApi player = GetSourcePlayer(value);

// このイベントが送られたプレイヤー一覧を取得します
VRCPlayerApi[] players = GetSendToPlayers(value);

// 送信元オブジェクトを取得します
SimpleNetworkBehaviour behavior = GetSourceObject(value);

// このイベントを受信したオブジェクト一覧を取得します
SimpleNetworkBehaviour[] behaviors = GetTargetObjects(value);

// イベントに設定されたグループ名を取得します
string group = GetGroup(value);

// グループでイテレートした際のインデックスを取得します。
int index = GetGroupIndex(value);
```

## 機能
### ローカルイベント実行
```C#
// 自分のみイベントを実行
ExecEvent("Jump", 5.0f);
```

### イベントの遅延送信
```C#
// 120フレーム（約2秒）後にイベントを送信
SendEvent("Jump", 5.0f, 120);
```

### 送信先の指定
```C#
// 全員にイベントを送信（デフォルト）
SendEvent(SendTo.All, "Jump", 5.0f);

// オブジェクト所有者にイベントを送信
SendEvent(SendTo.Owner, "Jump", 5.0f);

// インスタンスマスターにイベントを送信
SendEvent(SendTo.Master, "Jump", 5.0f);

// 自分自身にイベントを送信（ExecEventに同じ）
SendEvent(SendTo.Self, "Jump", 5.0f);

// オブジェクト所有者以外にイベントを送信
SendEvent(SendTo.NotOwner, "Jump", 5.0f);

// インスタンスマスター以外にイベントを送信
SendEvent(SendTo.NotMaster, "Jump", 5.0f);

// 自分以外にイベントを送信
SendEvent(SendTo.NotSelf, "Jump", 5.0f);

// 指定のプレイヤーにイベントを送信
SendEvent(player, "Jump", 5.0f);
```

### イベント送信の依頼
```C#
// インスタンスマスターから全員にイベントを送信
RequestEvent(SendTo.Master, "Jump", 5.0f);

// インスタンスマスターから自分にイベントを送信
RequestEvent(SendTo.Master, Networking.LocalPlayer, "Jump", 5.0f);
```

### デバッグ
```C#
// Debug.Logに送信されたコマンド等が送られる
SimpleNetwork.DebugMode(true);
```

## 仕組み
<p align="center"><img alt="structure" src="https://user-images.githubusercontent.com/14051445/220154122-edd5e748-78af-4085-b28f-79192161deab.png" width="60%"></p>

80人までの同時接続が可能な`SimpleNetworkProxy`（以下、プロキシ）が`SimpleNetwork.Prefab`に用意されており、プロキシには`string`型の`UdonSynced`同期変数が用意されています。

プレイヤーがワールドに参加すると、利用可能なプロキシが自動的に割り当てられるため、全てのプレイヤーは専用プロキシを所有します。

`SimpleNetwork`は、シーンに存在する全ての`SimpleNetworkBehavior`から発行されたイベントを収集しており、`SimpleNetworkBehavior`を継承したオブジェクトで`SendEvent()`メソッドが実行されると、イベント情報が`SimpleNetwork`に送信され、収集した複数のイベントの名前や値などの情報は1フレーム毎にひとつのコードにシリアライズされます。

作成されたコードは、専用プロキシの同期変数に投げられ、これにより全てのプレイヤーにコードが同期され、同期されたコードは`SimpleNetwork`がリアルタイムに受信します。

プロキシは、`OnValueChanged`によってコードの受信を検知しており、`SimpleNetwork`は、受け取ったコードをデシリアライズして、イベントに分割します。このとき、イベントのターゲットに含まれないプレイヤーは、受信したイベントを無視するよう設定されています。

実行が許されたイベントは、適切なオブジェクトの`ReceiveEvent()`メソッドを呼び出し、これにより、引数付きのネットワークイベントの発行が実現されています。

また、プロキシの`UdonBehaviourSyncMode`は`Manual`に設定されており、同期変数は高速に同期されるので、`SimpleNetwork`の`SendEvent()`は、`SendCustomNetworkEvent()`よりも高速に送受信できます。

### コード
以下の仕様は、プロキシが通信するコードについて説明しています。
`SimpleNetwork`でどのようなデータが通信されるか知りたい場合は、以下をご覧ください。

![Code](https://user-images.githubusercontent.com/14051445/220154173-055f55a5-02b3-4b27-a6d9-060c5b81404c.png)
RSは「レコード区切り文字」と呼ばれる特殊な文字で、可変長のイベント名と値を分割するために使用されます。一方、GSは「グループ区切り文字」と呼ばれる特殊な文字で、複数のイベントが同時に送信される場合に、それぞれのイベントを区切るために使用されています。

以下が各項目の詳細です。
* `SyncChar`：同期用文字。コマンドが同じ場合に`UdonSynced`の同期が省略されてしまうことを防ぐために使用されます。ASCIIコードの`!`から`~`までの94文字が、コードの先頭だけに順番に挿入されます。
* `Source`：送信元オブジェクトID。送信元オブジェクトの管理インデックスを2桁の94進数に変換したものです。設定によって桁数を変更でき、デフォルトでは最大8742個のオブジェクトを管理できます。
* `SendTo`：送信先プレイヤーID。1桁の94進数で、`!`が宛先無し、`"`から`q`の80文字が`playerId`による宛先指定となります。`r`からは、`All` `Owner` `Master` `Self` `NotOwner` `NotMaster` `NotSelf`による送信先の条件指定が可能です。
* `Target`：送信先オブジェクトID。先頭が`~`の場合、2桁目の文字がグループIDとして使用されます。デフォルトの最大オブジェクト管理数が8836個ではなく8742個なのは、これによるものです。
* `Event Name`：イベント名です。
* `Value`：イベントに送信された値です。`GetFloat()`などのメソッドを使用して値を受け取ることができます。

### コードの圧縮
SimpleNetworkは、通信するコード量を減らすため、94進数とsignedな93進数を利用してコードを圧縮しています。
コードに含まれるunsignedな整数は、`!`から`~`までのASCIIコード文字を利用して94進数に変換されます。
```
! " # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \ ] ^ _ ` a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~
```
また、signedな整数に関しては`~`をマイナス文字として利用した93進数の文字列に変換します。
