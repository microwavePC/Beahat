# Beahat

### 概要

iBeaconを簡単に検知し、イベント発火のトリガーとすることを可能にするXamarin用のNuGetパッケージです。
Beahatという名前は、iBeacon handling as a trigger の略です。

### Beahatを使う際のパーミッション関連の設定

Beahatを使う各プラットフォームのプロジェクトで、以下の通りパーミッションの設定を行ってください。

* **iOS**

  Info.plistで、プロパティ『Location When In Use Usage Description』を追加してください。

* **Android**

  Androidマニフェスト（AndroidManifest.xml）で、アクセス許可『BLUETOOTH』『BLUETOOTH_ADMIN』を追加してください。

* **UWP**

  Package.appxmanifestの機能タブで、『Bluetooth』のチェックをオンにしてください。

### メソッド・プロパティ

##### メソッド

* **bool BluetoothIsAvailableOnThisDevice**

  端末がBluetooth機能（iOSの場合は位置情報サービスも含む）をサポートしているかどうかを取得します。
  サポートしている場合、trueを返します。

* **bool BluetoothIsEnableOnThisDevice**

  端末のBluetooth機能（iOSの場合は位置情報サービスも含む）がオンにされているかどうかを取得します。
  端末がBluetoothに対応している場合、trueを返します。
  端末がBluetoothをサポートしていない場合、常時falseを返します。

* **void AddEvent**

  検出対象のiBeaconと検出時に自動実行したい処理を登録します。
  iBeaconのみを登録することもできます。その場合、検出時には検出ビーコン一覧（DetectedBeaconList）への登録だけが行われます。

* **RequestUserToTurnOnBluetooth**

  端末のBluetooth機能をオンにするためのダイアログを表示します。
  UWPには対応する機能がないため、何もしません。
  iOSの場合、位置情報サービスをオフにしている場合もダイアログを表示します。
  端末がBluetoothをサポートしていない場合、例外BluetoothUnsupportedExceptionをthrowします。

* **void ClearEvent**

  AddEventで登録した情報を全て消去します。

* **StartScan**

  iBeaconのスキャンを開始します。
  端末がBluetoothに対応していない場合、例外BluetoothUnsupportedExceptionをthrowします。
  端末のBluetooth機能がオフにされている場合、例外BluetoothTurnedOffExceptionをthrowします。
  スキャンの開始時、DetectedBeaconListをリセットします。

* **StopScan**

  iBeaconのスキャンを停止します。

##### プロパティ

* **IsScanning**

  スキャン中はtrue、スキャン停止中はfalseを返します。

* **DetectedBeaconList**

  直近のスキャン、あるいは実行中のスキャンで検出されたiBeaconの情報を持つリストです。1種類のiBeaconにつき、1つの要素を持ちます。同じiBeaconを複数回検出した場合、最も近付いたときの情報のみがこのリストに残されます。


### 使い方

1. Beahatのインスタンスを取得する（クラスメソッド"Beahat.Current"による取得と、IPlatformInitializerによるインジェクションの両方に対応しています）。

2. メソッドAddEventで、検出対象のiBeacon（と、検出時に実行させたい処理がある場合はその処理）を登録する。

3. メソッドStartScanで、iBeaconの検出を開始する。

4. 検出できたiBeaconの近接状況は、DetectedBeaconListを参照することで把握できる。

5. 検出を停止したくなったら、メソッドStopScanを実行する。
