# AWSK(航空戦シミュレーター)
Air War Simulator for Kantai Collection

## 概要

- [艦これ](http://www.dmm.com/netgame/feature/kancolle.html)において、航空戦の結果をシミュレーションするソフトウェアです
- プログラムの動作には、[.NET Framework 4.7.2](https://www.microsoft.com/net/download/thank-you/net472)以上が必要です。無い人はインストールを
- 現在実装が完了している機能は次の部分だけです(それでも役立つとは思いますが)
 - 基地航空隊によって削った後の敵制空値を下側確率と確率分布で表示
 - それぞれの基地航空隊における制空状況(航空優勢・航空劣勢など)の割合表示
- 高速で計算を行える他、艦娘・装備データをオンラインからダウンロードして更新も可能です
 - ただし、ダウンロード後はソフトウェアを再起動してください
- sampleフォルダに、基地航空隊編成や敵艦隊編成のサンプルデータが置いてあります(読み込み方は後述)

## 画面構成・使い方

　**まずAWSK.exeをクリックして起動してみてください。**  
　**Ver.1.4.5から、基地航空隊の装備コンボボックスは「装備名：対空値：戦闘行動半径」と表示されます。**

![image](https://user-images.githubusercontent.com/3734392/45603590-4a41ee00-ba68-11e8-936f-8a9717b6f184.png)

- Ver.1.5.0から、艦載機熟練度に「>>>」(内部熟練度120)を指定できるようになりました
 - それまでの「>>」は、内部熟練度100を表していました
- **基地航空隊のグループボックス内のコントロール**を右クリックすると、基地航空隊設定の保存・読み込みができます
- **敵艦隊のグループボックス内のコントロール**を右クリックすると、装備の詳細表示、および敵艦隊設定の保存・読み込み、そして**Wikiから検索(後述)**ができます
- 「計算結果」画面のグラフを右クリックすると、グラフ画像や詳細なテキストデータをクリップボードにコピーできます

![ver 2](https://user-images.githubusercontent.com/3734392/46573065-9f6b8280-c9ca-11e8-865e-7e07b5cfb959.png)

- 基地航空隊の装備コンボボックスにマウスを合わせると、その装備についての情報がツールチップ表示されます
  - 表示内容は、名称や戦闘行動半径、装備の対空値(素対空値と基地対空値)やそのスロットの制空値などです

![image](https://user-images.githubusercontent.com/3734392/37255447-e3691a7c-258f-11e8-8f38-b350cd2498c3.png)

- 敵艦を選択する際は、正式名称だけでなく「俗称」も選択できます
  - コンボボックスの選択項目の中で、頭に「＊」が付いているものがそれです
  - こういった俗称項目は増やせます**(後述)**

![image](https://user-images.githubusercontent.com/3734392/37255470-21e8da08-2590-11e8-9543-f3c9761874cb.png)

- 敵艦隊部分を右クリック→「編成を検索」をクリックすると、次のようなウィンドウが表示されます
- 見ての通り、マップ・難易度・マス名を選択すると敵編成情報が表示され、「決定」ボタンでメイン画面に反映される**素敵仕様**です
- マップ画像が小さいと感じる場合は、ウィンドウを縦や横に広げてみましょう
- もちろん、この画面からセットした編成も、メイン画面右クリックから保存できます

![image](https://user-images.githubusercontent.com/3734392/46573082-e194c400-c9ca-11e8-99c7-01b6e3f1af9b.png)

- ResourceフォルダのBasedAirUnitRange.csvは、戦闘行動半径に関する情報のキャッシュファイルです
- BasedAirUnitRange.csvはCSV(BOMなしUTF-8)形式で、見ての通りの入力フォーマットとなっています
- BasedAirUnitRange.csvに入力された内容は何よりも優先されます。と言うより、戦闘行動半径の情報をネットから読みに行くのに通信コストが馬鹿にならないので付けたファイルとも言えます(BasedAirUnitRange.csvが無くても更新はできるが、更新に掛かる時間がずっと長くなる)

![image](https://user-images.githubusercontent.com/3734392/37043235-7db5cec2-21a3-11e8-82ec-63661f510a8f.png)

- ResourceフォルダのEnemyFamiliarName.csvは、深海棲艦の俗称を登録するためのファイルです
- EnemyFamiliarName.csvはCSV(BOMなしUTF-8)形式で、見ての通りの入力フォーマットとなっています
- ID欄で指定するのは、深海棲艦に割り振られた艦船IDです。分からない場合は[英wiki](http://kancolle.wikia.com/wiki/Kancolle_Wiki)を読むか、GameData.dbの中身をSQLite系のツールで読んで下さい

![image](https://user-images.githubusercontent.com/3734392/37255496-a2f2f552-2590-11e8-82af-d1910e7f02af.png)

- ResourceフォルダのKammusuPatch.csvは、『「データベースを更新」ボタンでも引っ張れないような艦』を追加するためのファイルです
 - 例えばイベントが始まったばかりだと、英Wikiなどのサイトにはまだ新艦のデータが上がってなかったりします
 - そこで、テキストファイルを編集することにより、まさに「パッチ」としてデータを補う機能がこれです
- KammusuPatch.csvはCSV(BOMなしUTF-8)形式で、見ての通りの入力フォーマットとなっています
 - 左から順に、ID・艦名・艦種・素の対空値・スロット数・スロットの搭載数・初期装備・艦娘フラグを入力します
 - 艦名と艦種は文字列で入力しますが、引用符で囲まないでください
 - 「スロットの搭載数」と「初期装備」はそれぞれ5スロット分あります
 - 艦娘フラグは、艦娘なら「1」、深海棲艦なら「0」を入力してください
 - IDの番号をどう付けたらいいかですが、 **お察しください**
 - 艦種の文字列は今のところ「駆逐艦」「海防艦」「魚雷挺」「軽巡洋艦」「重雷装巡洋艦」「練習巡洋艦」「重巡洋艦」「航空巡洋艦」「正規空母」「装甲空母」「軽空母」「水上機母艦」「戦艦」「巡洋戦艦」「航空戦艦」「潜水艦」「潜水空母」「輸送艦」「補給艦」「揚陸艦」「工作艦」「潜水母艦」「陸上型」としています
- KammusuPatch.csvに入力された内容は何よりも優先されます。「データベースを更新」ボタンを押すとまずオンラインの情報を拾い集めてデータベースを構築し、それからKammusuPatch.csvの内容が追加・上書きされます
 - この上書きはIDの番号をキーとしますので、例えばKammusuPatch.csvでIDが410、type(艦種)が"戦艦"であるデータを重ねますと、既にID410に"清霜"がいることから、ソフトの上でだけ **清霜は戦艦になれます**

![image](https://user-images.githubusercontent.com/3734392/36781797-5e4e3a08-1cba-11e8-9277-23b623cb3de5.png)

## 注意
- ソフトウェアの動作には、 **.NET Framework 4.7.2** 以上が必要です
- **GameData2.db** には、艦娘(深海棲艦)・装備の情報がSQLite3形式で書き込まれています(無いと動作しません)
- **BasedAirUnitRange.csv** には、装備の戦闘行動半径の情報が書かれています(無くても動作します)
- **EnemyFamiliarName.csv**には、深海棲艦の俗称がCSV形式で書き込まれています(無くても動作します)
- *KammusuPatch.csv**には、艦娘/深海棲艦の情報がCSV形式で書き込まれています(無くても動作します)

## 作者
　YSR([Twitter](https://twitter.com/YSRKEN), [GitHub](https://github.com/YSRKEN/))

## 謝辞
- Readmeやヘルプファイルなどの表示に、[tatesuke](https://github.com/tatesuke) さんの「 [かんたんMarkdown](https://github.com/tatesuke/KanTanMarkdown) 」を使用しました
- ソフトウェア開発に協力してくださった、[pokopii](https://twitter.com/galpokopii)さんや[avaris](https://twitter.com/nc254cntct)さんや[mizucchi41](https://twitter.com/mizucchi41)さんや[tkscot](https://twitter.com/99_999999999)さんに深く感謝いたします
- Ver.1.5.0における更新内容には、[ADMIRAL DATOR](https://twitter.com/aindator)さんの寄与がとても大きいです

## License
　MIT License

## 使用ライブラリ
- [Prism.Unity](https://www.nuget.org/packages/Prism.Unity/)
 - MVVMによるソフトウェア構築に使用
- [ReactiveProperty](https://www.nuget.org/packages/ReactiveProperty/4.0.0-pre4)
 - 同上
- [DynamicJson](https://archive.codeplex.com/?p=dynamicjson)
 - JSONファイルの読み込みに使用
- [System.Data.SQLite.Core](https://www.nuget.org/packages/System.Data.SQLite.Core/)
 - データベースの読み書きに使用
- [OxyPlot.Wpf](https://www.nuget.org/packages/OxyPlot.Wpf/)
 - グラフ表示に使用
- [MersenneTwister](https://github.com/akiotakahashi/MersenneTwister)
 - 乱数生成に使用

## 更新履歴

### Ver.1.7.1
- 5-3や5-5など、一部の海域で情報が取得できていなかったケースが有ったので個別に修正
- Ver.1.6.0で、陸戦・局戦の迎撃が対空と同じ値になってしまっていたのを修正

### Ver.1.7.0
- バックグラウンドのソースを修正し、Ver.1.6.0の時に作った新しいEntityクラスを全面的に利用できるようにした
- 敵編成検索画面を実装。これにより、敵編成の入力が超絶簡単になった

### Ver.1.6.0
- データベース部分の処理を全面的に書き直した
- ↑により、データベースをダウンロード更新する時間が大きく減少し、より最新のデータを提供できるようになった

### Ver.1.5.4.2
- 制空判定にミスがあったので修正

### Ver.1.5.4.1
- 一部の深海棲艦を選択するとエラーが出ていた問題を修正

### Ver.1.5.4
- 基地航空隊の装備について、カテゴリーによる絞り込みが効くようにした

### Ver.1.5.3
- 18夏イベの新艦・新装備に対応
- 18夏イベ用のプリセットデータを追加

### Ver.1.5.2
- データベースを更新するボタンを押すと例外が飛んでいたので修正

### Ver.1.5.1
- 基地航空隊の攻撃回数の表示を、「0・1・2」から「なし・分散・集中」に変更
- 右クリックメニューが表示される範囲を格段に広げた
- 装備選択欄のツールチップに、その装備についての情報を表示するようにした
- 深海棲艦の俗称を登録できるようにした。これにより、敵艦を入力しやすくなるはず

### Ver.1.5.0
- 内部熟練度の扱いを変更しました。これにより、基地航空隊の制空値が僅かに上昇します
- 内部熟練度120に対応しました。使いたい方はどうぞ
- UIのコントロールの大きさを調整しました
- 艦偵・水偵・飛行艇を基地航空隊の制空計算に組み入れました、これにより、基地航空隊の制空値が僅かに上昇します

### Ver.1.4.6
- ダウンロード時のエラーをなるべく出さないようにした(訂正機能を強化)
- 一部の深海棲艦のデータがダウンロードできなくなっていた問題を修正

### Ver.1.4.5
- WeaponDataPatch.csvに追加の装備の戦闘行動半径を記述するようにした
- これにより、オンラインのデータベースに存在してない装備でも戦闘行動半径を入力できるようになります
- 装備の対空値と戦闘行動半径が直感的でなかったので、コンボボックスに表示するようにした
- 2種類のパッチで上書きされた艦船・装備について、その情報を表示するようにした

### Ver.1.4.4
- 計算速度が2倍速くなった
- KammusuPatch.csvに追加の艦娘/深海棲艦を記述するようにした
 - これにより、オンラインのデータベースに存在してない艦でも好きなだけ追加できるようになります

### Ver.1.4.3
- イベントのボスマス敵艦隊の編成をE2～E7までサンプルとして用意した
 - これに合わせて敵艦のデータも拡充した
- 敵制空値の分布をモンテカルロ法で割り出す際の手法を変更した
 - Ver.1.3.0から、残存機数の分布を制空値の分布に変換して畳み込んでいた
 - しかしそれだと **実際には起こりえない組み合わせのスロット配置** も勘案していたことが判明
 - ゆえに、St1撃墜した後の敵制空値をそのままカウントする方式にコードを差し戻した

### Ver.1.4.2
- イベントのボスマス敵艦隊の編成をE2～E4までサンプルとして用意した

### Ver.1.4.1
- データベースをダウンロードする際、より幅広い艦娘のデータを使えるようになった
- データベースの更新中は、更新ボタンとシミュレーションボタンを押せないようにした
- サンプルデータを増やした

### Ver.1.4.0
- 敵艦のデータを大幅に追加した。また、区別しやすいよう、敵艦にはIDを表示するようにした
- UIオブジェクトの幅を調節し、また小さすぎた時用にウィンドウをリサイズ可能とした
- 敵艦を、艦種から選択できるようにした(詳しくは「使い方」欄にて)
- 敵艦隊の詳細を表示する際、搭載数情報を表示するようにした

### Ver.1.3.0
- UI周りのソースコードを大幅にリファクタリング
- 小数を含む計算を、等価な整数演算に置き換えて高速化
- 乱数生成エンジンを、NET標準の雑な奴からメルセンヌ・ツイスタ(SFMT)に変更
- 敵制空値の分布について、下側確率だけでなく確率分布も表示するように
- 敵制空値の分布をモンテカルロ法で割り出す際の手法を変更した
 - 従来は、St1撃墜した後の敵制空値をそのままカウントしていた
 - 今後は、残存機数の分布をスロット毎にカウントしてから、それぞれを畳み込むように
 - これにより、シミュレーション結果の精度が大幅に向上することになった
- その他細かな計算の高速化

### Ver.1.2.0
- 基地航空隊における航空戦では、敵艦隊の水上偵察機も制空権争いに参加することを忘れていたので修正
- 敵艦の詳細(装備など)を表示できるようにした
- 基地航空隊および敵艦隊の情報を保存・読み込みする機能を追加

### Ver.1.1.0
- 局地戦闘機などの制空値が反映されていない不具合を修正
- 艦載機の改修効果を間違えていたので修正
- ウィンドウを最小化できるようにした
- ちょっとした表示ミスを修正

### Ver.1.0.0
- 初公開
