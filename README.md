# AWSK(航空戦シミュレーター)
Air War Simulator for Kantai Collection

## 概要

- [艦これ](http://www.dmm.com/netgame/feature/kancolle.html)において、航空戦の結果をシミュレーションするソフトウェアです
- 現在実装が完了している機能は次の部分だけです(それでも役立つとは思いますが)
 - 基地航空隊によって削った後の敵制空値を下側確率で表示
 - それぞれの基地航空隊における制空状況(航空優勢・航空劣勢など)の割合表示
- 高速で計算を行える他、艦娘・装備データをオンラインからダウンロードして更新も可能です
 - ただし、ダウンロード後はソフトウェアを再起動してください
- sampleフォルダに、基地航空隊編成や敵艦隊編成のサンプルデータが置いてあります(読み込み方は後述)

## 画面構成・使い方
![image](https://user-images.githubusercontent.com/3734392/35778074-39d3fac6-09fc-11e8-99ea-6dc00d6551aa.png)

- 基地航空隊において、各航空隊の攻撃回数のコンボボックスを右クリックすると、基地航空隊設定の保存・読み込みができます
- 敵艦隊において、一番上のコンボボックスを右クリックすると、装備の詳細表示、および敵艦隊設定の保存・読み込みができます
- 「計算結果」画面のグラフを右クリックすると、グラフ画像や詳細なテキストデータをクリップボードにコピーできます

![image](https://user-images.githubusercontent.com/3734392/35803422-1dcae388-0ab7-11e8-9be6-d0485eedef2f.png)

## 注意
- ソフトウェアの動作には、 **.NET Framework 4.6.2** 以上が必要です
- **WeaponData.csv** には、装備の戦闘行動半径の情報が書かれています
- **GameData.db** には、艦娘(深海棲艦)・装備の情報が書き込まれています

## 作者
　YSR([Twitter](https://twitter.com/YSRKEN), [GitHub](https://github.com/YSRKEN/))

## 謝辞
- Readmeやヘルプファイルなどの表示に、[tatesuke](https://github.com/tatesuke) さんの「 [かんたんMarkdown](https://github.com/tatesuke/KanTanMarkdown) 」を使用しました
- ソフトウェア開発に協力してくださった、[pokopii](https://twitter.com/galpokopii)さんや[avaris](https://twitter.com/nc254cntct)さんに深く感謝いたします

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
