# SimpleNetwork
![SimpleNetworkMonster](https://user-images.githubusercontent.com/14051445/222969037-ce88f00c-3b61-4c8f-b9ff-e03ca2b34042.png)
SimpleNetworkは、VRChatのNetworkingがしんどい方のためのNetworkingラッパーなUdonSharpモジュールです。
SendCustomNetworkEventメソッドで、引数を扱えない問題を解消する目的で作成されました。

SimpleNetworkは、[SimpleNetworkUdonBehavior](https://github.com/tutinoco/SimpleNetworkUdonBehaviour)の進化版です。

## 特徴
* `SendCustomNetworkEvent`で不可能な引数の送信ができ、メッセージングプログラミングを可能にします。
* 煩わしい所有権から解放され、権限の無いオブジェクトからも低レイテンシーで高速な通信が行えます。
* オブジェクトにグループ名を設定して、複数のオブジェクトにイベントをイテレーション送信できます。
* 大量にイベントを発行しても、自動的に全てのイベントを1回の送信にまとめるため、安定して動作します。
* `All`や`Owner`のほかに、`Master`やプレイヤーを直接設定して、イベント受信者を限定することができます。
* `Master`にイベント送信を依頼するなど、他のプレイヤーにイベントの送信を依頼することができます。
* 最後に実行したイベントやイベント履歴をサーバに保存し、後から参加したプレイヤーに送信することができます。
* `SendCustomNetworkEventDelayedFrames`に相当する、ネットワークイベントの遅延送信が可能です。
* シーンに存在しない動的に生成したオブジェクトに対しても通信が可能です。

## 使い方
`SimpleNetwork.prefab`をシーンに配置し、適当なクラスを作成して`SimpleNetworkBehaviour`を継承すると、`SendEvent()`メソッドで、全てのプレイヤー（自分自身を含む）のオブジェクトに値を送信することができます。
イベントを受信するには、サブクラスで`ReceiveEvent`メソッドをオーバーライドします。第一引数にはイベント名が、第二引数には値が届きます。
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

上記のコードでは、オブジェクトが自らのイベントを送信していますが、特に嬉しいのは、値を送信できるようになったことで、命令されたら動くアクションだけをまとめたクラスを簡単に作成できるようになることです。
以下は、命令を待って動くモンスタークラスの例です。
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

    public override void ReceiveEvent(string name) // 第二引数valueを省略しても
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
このモンスタークラスを使って作られたモンスターを制御するには、適当な箇所に以下のコードを記述します。
```C#
// ジャンプ力5.0で飛ぶ！
monster.SendEvent("jump", 5.0f);

// x:1 y:2 z:3 の座標にワープ！
monster.SendEvent("warp", new Vector3(1.0f, 2.0f, 3.0f));

// ふきだしに文字を表示！
monster.SendEvent("talk", "僕を捕まえられるかな？");
```

### 色々な型の送受信
`bool` `char` `byte` `sbyte` `short` `ushort` `int` `uint` `long` `ulong` `float` `double` `Vector2` `Vector3` `Vector4` `Quaternion` `string` `VRCUrl` `Color` `Color32`に加え
`Object[]` `SimpleNetworkBehaviour`の送受信に対応しています。

現在、`Object[]`以外の配列の送受信は非対応です。
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
    Vector3 vector3Value = GetVector3();
}
```

また、値の無いイベントの送信も可能です。
```C#
monster.SendEvent("Init");
monster.SendEvent("Init", none);

// monster.SendEvent("Init", null); // 型指定の無いnullは非対応です ＞＜
```
## イベントの送信
### イベントのローカル実行
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
// インスタンスマスターから全員にイベントを送信するよう依頼
RequestEvent(RequestTo.Master, "Jump", 5.0f);

// インスタンスマスターからオブジェクトオーナーにイベントを送信するよう依頼
RequestEvent(RequestTo.Master, SendTo.Owner, "Jump", 5.0f);

// プレイヤーAからプレイヤーBにイベントを送信するよう依頼
RequestEvent(playerA, playerB, "Jump", 5.0f);

// オブジェクトオーナー側で60フレーム待機してから自分にイベントを送信するよう依頼
RequestEvent(RequestTo.Owner, SendTo.Me, "Jump", 5.0f, 60);
```

### 複数の値を送信する
複数の値を送るには、`Pack()`メソッドを利用します。
```C#
Vector3 position = Vector3(1.0f, 2.0f, 3.0f);
Quaternion rotation = Quaternion.identity;

monster.SendEvent("warp2", Pack(position, rotation));
```

`Main`クラスの役割を果たしているクラスでも`SimpleNetworkBehaviour`を継承することをお勧めしますが、
万が一呼び出し元が`SimpleNetworkBehaviour`のサブクラスでない等、`Pack`メソッドを利用できない場合には、以下のように`Object[]`を利用します。
```C#
Objects[] values = new Objects[] {position, rotation}; // Objects[]に格納して
monster.SendEvent("warp2", values); // 送信する。
```
上記コードは`using Object = System.Object;`を追加する必要があるかもしれません。

複数の値を受信するには、以下のように値の引数番号を指定します。
```C#
public override void ReceiveEvent(string name)
{
    Vector3 position = GetVector3(0);       // 0番目の値をVector3で取得
    Quaternion rotation = GetQuaternion(1); // 1番目の値をQuaternionで取得

    // 引数番号を省略することもできます。
    Vector3 position2 = GetVector3();       // 最初に見つかったVector3を取得
    Quaternion rotation2 = GetQuaternion(); // 最初に見つかったQuaternionを取得
}
```
なお、対応していない型の値を`Pack()`に含めるとはできません。

## 様々な使い方

### パラメータの順序
```C#
ExecEvent(名前, 値, グループ指定, 遅延);
SendEvent(送信先, 名前, 値, グループ指定, 遅延, 参加同期);
RequestEvent(依頼先, 送信先, 名前, 値, グループ指定, 遅延, 参加同期);
```
各イベント送信メソッドのパラメータの設定方法は上記の通りで、`名前`以外は省略することができます。
設定可能な型は以下の通りです。
* 依頼先：`RequestTo`型、または依頼先プレイヤーを`VRCPlayerApi`型で指定します。
* 送信先：`SendTo`型、または送信先プレイヤーを`VRCPlayerApi`型で指定します。
* 名前：`string`型で実行するイベント名を指定します。
* 値：送信する値を設定します。対応している型は、[色々な型の送受信](#色々な型の送受信)を参照してください。
* グループ指定：`string`型でグループ名を指定してイベントを受信するオブジェクトを選択します。
* 遅延：`int`型でフレーム数を設定し、イベントの送信を遅らせます。
* 参加同期：`JoinSync`型で、ワールドに参加したユーザに同期するイベントを設定します。

### ２つの受信方法
SimpleNetworkではイベントを受信する方法に「ダイレクトレシーブ」と「インディレクトレシーブ」が存在します。

ダイレクトレシーブを使うと`GetFloat()`などの値取得用のメソッドを利用せず、直接値を受け取ることができます。
以下のように様々な`ReceiveEvent()`メソッドをオーバーライドすることで、型に応じて受信用メソッドを分けることができます。
```C#
// float型のデータを受信
public override void ReceiveEvent(string name, float value) { ... }

// Vector3型のデータを受信
public override void ReceiveEvent(string name, Vector3 value) { ... }

// Packでまとめられた複数の値を受信
public override void ReceiveEvent(string name, Object[] values) { ... }
```

おすすめの受信方法は、すべてのイベントと値を柔軟に受信できる、インディレクトレシーブです。
```C#
// 全てのイベントを受信
public override void ReceiveEvent(string name) { ... } // 第二引数を省略する
```
値の受信方法は`GetFloat()`などの受信メソッドを利用することですが、該当する型のデータが届かなかった場合にエラーが発生するため注意が必要です。
また、`GetValues()`メソッドを利用することで受信したすべての値を受け取ることもできます。

### メタデータの受信
送信された値以外にも、付随するデータを取得することができます。
```C#
public override void ReceiveEvent(string name)
{
    // 送信元オブジェクトを取得します
    SimpleNetworkBehaviour behavior = GetSource();

    // 送信元プレイヤーIDを取得します
    int player = GetSender();

    // このイベントを受信したプレイヤーIDの一覧を取得します
    int[] players = GetRecipients();

    // このイベントのグループターゲット文字列を取得します
    SimpleNetworkBehaviour[] behaviors = GetTarget();

    // グループでイテレートした際のインデックスを取得します
    int index = GetIndex();

    // このイベントがどのくらい遅延して送られたかを取得します
    int delay = GetDelay();

    // 受信したすべての値を受け取ります。
    Object[] values = GetValues();
}
```

### 制御可能なオブジェクトの複製
通常、`Instantiate`を使って複製したオブジェクトはUdonでは制御できませんが
SimpleNetworkでは、制御可能なオブジェクトを複製する機能が備わっています。

オブジェクトを複製するには`Duplicate()`メソッドを実行します。
```C#
// monsterをx:0 y:0 z:0に複製します
Duplicate(monster, new Vector3(0,0,0) /*,Quaternion.identity*/); // Quaternionは省略可能
```

複製されたオブジェクトを取得するには、`OnDuplicateComplete()`メソッドをオーバーライドします。
```C#
public override void OnDuplicateComplete( SimpleNetworkBehaviour behaviour )
{
    Monster newMonster = (Monster)behaviour;
    newMonster.SendEvent("Jump", 5.0f); // 複製したオブジェクトにイベントを送信
}
```

### グループ名を設定する
`SimpleNetworkInit()`メソッドの第一引数にグループ名を設定すると、イベントのグループ指定送信を行った際にマッチしたグループにイベントを送信することができます。

例えば、下記のように`Monster`クラスの`Start()`メソッドで`SimpleNetworkBehaviour`を初期化する際に`"Monster"`というグループ名を設定します。

#### Monster.cs
```C#
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco;

public class Monster : SimpleNetworkBehaviour
{
    private VRCPlayerApi target;

    void Start()
    {
        SimpleNetworkInit("Monster"); // グループ名を設定して初期化
    }

    public override void ReceiveEvent(string name)
    {
        // 【モンスターの初期化】
        if( name == "Init" ) {
            float distance = GetFloat();
            float x = Random.Range(-0.5*distance, 0.5*distance);
            float z = Random.Range(-0.5*distance, -0.5*distance);
            Vector3 pos =  new Vector3(x, 0f, z);
            gameObject.transform.position = pos;
        }

        // 【プレイヤーを見る】
        if( name == "See" ) {
            int playerId = GetInt();
            VRCPlayerApi target = VRCPlayerApi.GetPlayerById(playerId);
            gameObject.transform.LookAt(target.transform);
        }
    }

}
```

全ての`Monster`に同じイベントを送信...つまり、全ての`Monster`に一斉に見つめられるようにするには`Main`クラスを以下のようにします。
#### Main.cs
```C#
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using tutinoco;

public class Main : SimpleNetworkBehaviour // Mainクラスでも継承する
{
    // [SerializeField] private Monsters[] monsters; // ←これはもういらない
    private VRCPlayerApi target;

    void Start()
    {
        SimpleNetworkInit();

        // Monsterを10m四方にランダム配置して初期化する
        if( IsMaster() ) SendEvent("Init", 10.0f "Monster"); // ← for文を利用せず全てのMonsterを初期化
    }

    void Update()
    {
        // 全てのモンスターがtargetを常に見る
        if( IsMaster() && target != null ) SendEvent("See", target, "Monster");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        target = player.playerId;
    }

}
```
冒頭ではモンスターにイベントを送信する際、`monster.SendEvent("Jump", 5.0f);`といった形でイベントを送信していました。
しかし、グループ名を利用することで、メインクラスは、モンスターを所持したり、複数のモンスターをコレクションする必要は無くなり、
グループ名を指定するだけで、全ての`Monster`にイベントを送信することができるようになりました。

また、`GetIndex()`メソッドを利用することで、イテレーションのインデックスを取得することができるため、
処理された順番によって異なる処理を施すこともできます。
```C#
public override void ReceiveEvent(string name)
{
    // 【モンスターの初期化】
    if( name == "Init" ) {

        // 複数のMonsterを一列に並べて配置
        float x = (float)GetIndex();
        Vector3 pos = new Vector3(x, 0f, 0f); // インデックスをXに代入
        gameObject.transform.position = pos;
    }

    ...
}
```
また、`GetBehaviours()`メソッドを利用することで、イテレーションによく使う対象オブジェクトの数を確認することもできます。
```C#
int monsterLength = GetBehaviours("Monster").Length; // モンスター数を確認

// ワイルドカードを使用することもできます。
SimpleNetworkBehaviour[] enemys = GetBehaviours("Mon*"); // Monから始まるグループ名のオブジェクトを全て取得
```

### ワイルドカード
ワイルドカードは、イベントのグループ指定送信を行うときにも利用することもできます。
以下のように、グループ名にマッチしたオブジェクトにイベントを送信することができます。
```C#
// MonsterだけでなくMoisterなどにもイベントを送信する
SendEvent("Initialize", none, "Mo?ster");

// MonsterだけでなくRocksterやPopsterにもイベントを送信する
SendEvent("Initialize", none, "*ster");

// MonsterだけでなくMondeyやMonochromeなどにもイベントを送信する
SendEvent("Initialize", none, "Mon*");
```

また、OR`|`やAND`&`を使った選択も可能です。
```C#
// MonsterとAlienとInvaderにイベントを送信する
SendEvent("Initialize", none, "Monster|Alien|Invader");

// Mから始まりabcを含むグループ名のオブジェクトにイベントを送信する
SendEvent("Initialize", none, "M*&*abc*");

// Mから始まりabcを含むグループ名または
// Aから始まりdefを含むグループ名のオブジェクトにイベントを送信する
SendEvent("Initialize", none, "M*&*abc*|A*&*def*");
```

### グループ名の変更
初期化時に設定したグループ名は、後から変更することもできます。

```C#
SetGroupName("Monster-TypeA-001");
```

グループ名の変更を受け取るには以下のようにします。
```C#
public override void OnChangeGroupName( bool global ) // グローバルな変更が行われたらtrue
{
    string groupName = GetGroupName(); // "Monster-TypeA-001"
}
```

また、他のプレイヤーのPC上で挙動が変わるためお勧めできませんが、第二引数に`false`を指定することで、グループ名をローカルでのみ変更することもできます。
```C#
SetGroupName("Monster-TypeA-001", false); // グループ名の変更を同期せず自分のみ変える
```

### 参加同期
SimpleNetworkでは、最後に実行したイベントやイベント履歴をサーバに保存して、後から参加したプレイヤーに送信することができます。
サーバに値を保存するには次のようにします。
```C#
// 最後に実行したSetPositionイベントを後から参加したプレイヤーに送信する形でイベントを実行
SendEvent("SetPosition", pos, JoinSync.Latest);

// 今まで実行したDrawLineイベントを後から参加したプレイヤーに連続的に送信する形でイベントを実行
SendEvent("DrawLine", pos, JoinSync.Logging);
```

グループと併用することも可能です
```C#
// 全てのドミノをtype1でリセットしたことを後から参加したプレイヤーに送信する形でリセットする
int type = 1;
SendEvent("Reset", type, "Domino", JoinSync.Latest);
```

サーバに保存されている値を削除するには`ClearJoinSync()`メソッドを実行します
```C#
ClearJoinSync(); // このオブジェクトに保存された全てのイベント履歴を削除
ClearJoinSync("Monster|Alien"); // MonsterとAlienに保存された全てのイベント履歴を削除
ClearJoinSync(monster); // 指定したオブジェクトに保存された全てのイベント履歴を削除
```

### デバッグ
SimpleNetworkが送受信したデータの内容を確認するにはデバッグモードを有効にします。
```C#
// Debug.Logに送信されたコマンド等が送られる
SimpleNetwork.DebugMode(true);
```

## 仕組み
<p align="center"><img alt="structure" src="https://user-images.githubusercontent.com/14051445/222938801-1d53d07c-3349-4e8e-9c6b-273e9fa5ff6c.png" width="60%"></p>

80人までの同時接続が可能な`SimpleNetworkProxy`（以下、プロキシ）が`SimpleNetwork.prefab`に用意されており、プロキシには各型の`UdonSynced`同期変数の配列が用意されています。

プレイヤーがワールドに参加すると、利用可能なプロキシが自動的に割り当てられるため、全てのプレイヤーは専用プロキシを所有します。

`SimpleNetwork`は、シーンに存在する全ての`SimpleNetworkBehavior`から発行されたイベントを収集しており、`SimpleNetworkBehavior`を継承したオブジェクトで`SendEvent()`メソッドが実行されると、イベント情報が`SimpleNetwork`に送信され、収集した複数のイベントの名前や値などの情報は1フレーム毎にひとつ同期にまとめられます。

同期する値は、専用プロキシの各型の同期変数に投げられ、これにより全てのプレイヤーに値が同期され、同期された値は`SimpleNetwork`がリアルタイムに受信します。

プロキシは、`OnDeserialization()`によってコードの受信を検知しており、`SimpleNetwork`は、受け取った値をイベントオブジェクトにまとめて配列を作成し、各イベントを`ReceiveEvent()`に送信します。このとき、イベントのターゲットに含まれないプレイヤーは、受信したイベントを無視するよう設定されています。

実行が許されたイベントは、適切なオブジェクトの`ReceiveEvent()`メソッドが呼ばれ、`SimpleNetworkBehavior`に値を保存、`GetInt()`などのメソッドにより値にアクセスすことで、引数付きのネットワークイベントの発行が実現されています。

また、プロキシの`UdonBehaviourSyncMode`は`Manual`に設定されており、同期変数は高速に同期されるので、`SimpleNetwork`の`SendEvent()`は、`SendCustomNetworkEvent()`よりも高速に送受信できます。

## 注意事項
* イベント名に`__DUPLICATE__`と`__SETGROUPNAME__`を利用することはできません。
* グループ名に`?` `*` `|` `&`を利用することはできません。利用した場合は自動的に取り除かれます。
* `SimpleNetworkInit`を利用したグループ名を初期化時設定できる機能はローカルで設定されるため、動的なグループ名の設定は予期せぬ動作につながる可能性があります。