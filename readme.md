# SimpleNetwork
![SimpleNetworkMonster](https://user-images.githubusercontent.com/14051445/219826822-95ede90d-f5c7-4679-9218-8e9c4c598dc2.png)
SimpleNetworkは、VRChatのNetworkingがしんどい方のためのNetworkingラッパーなUdonSharpモジュールです。
SendCustomNetworkEventメソッドで、引数を扱えない問題を解消する目的で作成されました。

## 特徴
* `SendCustomNetworkEvent`では不可能な、引数の送信を実現します。
* オブジェクトの所有権にかかわらず低レイテンシーな高速通信ができます。
* `SendCustomNetworkEventDelayedFrames`に相当するネットワークイベントの遅延実行ができます。
* 連続でイベントを叩いても自動的に1回の通信にまとめるから安定して動作します。
* `All` `Owner`の他に`Master`や`playerId`を指定して特定のユーザにイベントを送信できます。
* `Master`にイベント送信を依頼するなど、ユーザを指定してイベントの送信を仲介させることができます。（実装中）
* オブジェクトにタグを付けることで別のオブジェクトにイベントを送信することができます。（実装中）
* 最後に実行したイベントやイベントログをサーバに保存し、Joinした人に同期できます。（実装中）
* シーンに存在しない複製したオブジェクトも同期できます。（実装中）

## 使い方

現在この項目は書き途中です。

### 送信先の指定
```
// 全員にイベントを送信
SendEvent(SimpleNetworkTarget.All, "Jump", 2.0f);

// オブジェクト所有者にイベントを送信
SendEvent(SimpleNetworkTarget.Owner, "Jump", 2.0f);

// インスタンスマスターにイベントを送信
SendEvent(SimpleNetworkTarget.Master, "Jump", 2.0f);

// 自分自身にイベントを送信（ExecEventに同じ）
SendEvent(SimpleNetworkTarget.Self, "Jump", 2.0f);

// オブジェクト所有者以外にイベントを送信
SendEvent(SimpleNetworkTarget.NotOwner, "Jump", 2.0f);

// インスタンスマスター以外にイベントを送信
SendEvent(SimpleNetworkTarget.NotMaster, "Jump", 2.0f);

// 自分以外にイベントを送信
SendEvent(SimpleNetworkTarget.NotSelf, "Jump", 2.0f);

// プレイヤーIDが7のユーザにイベントを送信
SendEvent(7, "Jump", 2.0f);

// プレイヤーIDが7以外のユーザにイベントを送信
SendEvent(~7, "Jump", 2.0f);
```

### コマンド
SimpleNetworkが通信するコマンドは以下の通りです。
![Command](https://user-images.githubusercontent.com/14051445/219865470-36cc6020-74e4-4560-999e-d69e33780cb4.png)
