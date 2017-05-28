# <img src="icon_small.png" width="48" height="48"/> Beahat


## 概要

iBeaconを簡単に検知し、イベント発火のトリガーとすることを可能にするXamarin用のNuGetパッケージです。<br>
Beahatという名前は、iBeacon handling as a trigger の略です。


## 対応プラットフォーム

| iOS | Android | UWP |
| ------------- | ----------- | ----------- |
| 8.0以上 | 4.4以上 | 制限なし |


## Beahatを使う際のパーミッション関連の設定

Beahatを使う各プラットフォームのプロジェクトで、以下の通りパーミッションの設定を行ってください。

* **iOS**

  Info.plistで、以下のどちらかのプロパティを追加してください。
  * Location When In Use Usage Description
  * Location Always Usage Description Property

* **Android**

  Androidマニフェスト（AndroidManifest.xml）で、以下の2つのアクセス許可を追加してください。
  * BLUETOOTH
  * BLUETOOTH_ADMIN

* **UWP**

  Package.appxmanifestの機能タブで、『Bluetooth』のチェックをオンにしてください。


## サンプルアプリ

同ソリューション内にあるBeahatTutorialがサンプルアプリです。
Xamarin.Formsで作成しています。


## メソッド・プロパティ

#### メソッド

* bool **IsAvailableToUseBluetoothOnThisDevice**

  端末がBluetooth機能（iOSの場合は位置情報サービスも含む）をサポートしているかどうかを取得します。<br>
  サポートしている場合はtrue、そうでない場合はfalseを返します。

* bool **IsEnableToUseBluetoothOnThisDevice**

  端末のBluetooth機能がオンにされているかどうかを取得します。<br>
  端末のBluetooth機能がオンにされている場合はtrue、そうでない場合はfalseを返します。<br>
  端末がBluetoothをサポートしていない場合、常にfalseを返します。

* bool **IsEnableToUseLocationServiceForDetectingBeacons**

  iBeacon検知のために必要な位置情報の使用が許可されているかどうかを取得します。<br>
  位置情報の使用が許可されている場合はtrue、そうでない場合はfalseを返します。<br>
  AndroidとUWPではiBeaconを検知するために位置情報利用の許可が必要ないため、常にtrueを返します。

* void **RequestUserToTurnOnBluetooth**

  端末のBluetooth機能がオフにされている場合に、端末のBluetooth機能をオンにするためのダイアログを呼び出します。<br>
  UWPでは対応する機能がないため、何も処理は行われません。<br>
  端末がBluetoothをサポートしていない場合、例外BluetoothUnsupportedExceptionをthrowします。

* void **RequestUserToAllowUsingLocationServiceForDetectingBeacons**

  位置情報機能がオフにされている、あるいは許可されていない場合に、位置情報の使用許可をユーザーに求めるダイアログを表示します。<br>
  AndroidとUWPではiBeaconを検知するために位置情報利用の許可が必要ないため、何も処理は行われません。

* void **AddObservableBeacon**

  検出対象とするiBeaconを追加します。<br>
  同じiBeaconに対してAddObservableBeaconWithCallbackメソッドを実行することで、検出時に実行する処理を後から追加することが可能です。

* void **AddObservableBeaconWithCallback**

  検出対象とするiBeaconと、検出時に実行したい処理を追加します。<br>
  同じiBeaconに対してこのメソッドを続けて実行することで、検出時に実行する処理を複数設定することが可能です。

* void **ClearAllObservableBeacons**

  AddObservableBeaconやAddObservableBeaconWithCallbackで追加した検出対象ビーコンや実行処理を全て消去します。

* void **InitializeDetectedBeaconList**

  検出されたiBeaconのリスト（DetectedBeaconListFromClosestApproachedInfo、DetectedBeaconListFromLastApproachedInfo）を初期化します。

* void **StartScan**

  iBeaconのスキャンを開始します。
  端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。<br>
  端末のBluetooth機能がオフにされている場合、例外BluetoothTurnedOffExceptionをthrowします。<br>
  iOSのみ、位置情報サービスの使用が許可されていない場合、例外LocationServiceNotAllowedExceptionをthrowします。

* void **StopScan**

  iBeaconのスキャンを停止します。

#### プロパティ

* bool **IsScanning**

  スキャン実行中はtrue、スキャン停止中はfalseを返します。

* List\<iBeacon\> **DetectedBeaconListFromClosestApproachedInfo**

  直近のスキャン、あるいは実行中のスキャンで検出されたiBeaconの情報を持つリストです。<br>
  1種類のiBeaconにつき、1つの要素を持ちます。同じiBeaconを複数回検出した場合、**最も近付いたときの情報**がこのリストに残されます。

* List\<iBeacon\> **DetectedBeaconListFromLastApproachedInfo**

  直近のスキャン、あるいは実行中のスキャンで検出されたiBeaconの情報を持つリストです。<br>
  1種類のiBeaconにつき、1つの要素を持ちます。同じiBeaconを複数回検出した場合、**最後に検出したときの情報**がこのリストに残されます。



## 使い方

1. Beahatのインスタンスを取得する（クラスメソッド"Beahat.Current"による取得と、IPlatformInitializerによるインジェクションの両方に対応しています）。

2. メソッドAddObservableBeaconWithCallbackで、検出対象のiBeaconと、検出時に実行させたい処理を登録する。

3. メソッドStartScanで、iBeaconの検出を開始する。

4. 検出できたiBeaconの近接状況は、DetectedBeaconListFromClosestApproachedInfoを参照することで把握できる。

5. 検出を停止したくなったら、メソッドStopScanを実行する。


## ライセンス・免責

[アンライセンス](https://github.com/microwavePC/Beahat/blob/master/LICENSE)です。<br>
著作権を放棄しているので、利用・改変・再配布等自由にご利用ください。<br>

Beahatを利用することによるトラブル等に関しては自己責任でお願いします。
