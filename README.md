[English Readme is here](https://github.com/EH500-Kintarou/Tategaki/blob/master/README-en.md)

# WPF用縦書きテキストブロック Tategaki

WPFアプリケーションにおいて縦書きを使用するためのコントロールライブラリです。  
WPFはネイティブで縦書きをサポートしておらず簡単に日本語の縦書きのようなものを作ることができませんが、このコントロールを利用すれば、WPF標準のTextBlockコントロールを使うような感覚で縦書きを実現することができます。

![](https://img.shields.io/badge/Nuget-3.2.1-blue?logo=nuget&style=plastic)
![](https://img.shields.io/badge/.NET_Framework-4.7.2-orange?logo=.net&style=plastic)
![](https://img.shields.io/badge/.NET-6-orange?logo=.net&style=plastic)

![Screenshot of Tategaki](https://raw.githubusercontent.com/EH500-Kintarou/Tategaki/master/Images/SampleScreenshot.png)

## 動作環境

- Windows OS
- .NET 6 / .NET Framework 4.7.2

## 使用方法
### 1. Nugetからインストール
![](https://img.shields.io/badge/Nuget-3.2.1-blue?logo=nuget&style=plastic) https://www.nuget.org/packages/Tategaki/

### 2. XAML名前空間を設定
XAMLで名前空間 "http://schemas.eh500-kintarou.com/Tategaki" を登録し、"TategakiText"要素を追加することで縦書きのテキストを表示させることができるようになります。
```xaml
<Window x:Class="TategakiTextTest.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:tg="http://schemas.eh500-kintarou.com/Tategaki"
        Title="MainWindow" />
```
```xaml
<tg:TategakiText Text="「こんにちは」"/>
```

### 3. サンプルコードをチェック
このリポジトリにはサンプルコードが含まれています。 [サンプルコード](https://github.com/EH500-Kintarou/Tategaki/tree/master/TategakiSample) を見ることでより理解が深まります。

## プロジェクトURL
![](https://img.shields.io/badge/Github-3.2.1-green?logo=github&style=plastic) https://github.com/EH500-Kintarou/Tategaki  
![](https://img.shields.io/badge/Nuget-3.2.1-blue?logo=nuget&style=plastic) https://www.nuget.org/packages/Tategaki/  
![](https://img.shields.io/badge/Blogger-3.2.1-orange?logo=blogger&style=plastic) https://days-of-programming.blogspot.com/search/label/WPF%E7%94%A8%E7%B8%A6%E6%9B%B8%E3%81%8D%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%E3%83%96%E3%83%AD%E3%83%83%E3%82%AF%20Tategaki

## 利用しているライブラリ
- System.Memory: https://www.nuget.org/packages/System.Memory/
- Extended WPF Toolkit (サンプルコード内のみ): https://github.com/xceedsoftware/wpftoolkit

## バージョン履歴
### ver.3.2.1 (2024/05/02)
- 縦書き対応フォントを検索する際（初回TategakiText.AvailableFontsを呼び出す際）のパフォーマンスを大幅に改善
- 下線/中線/上線の配置を改善（フォントファイルのBASEテーブルのデータに基づくようにした）
  - それに伴い、テキストの左右の余白のバランスがおかしかったのも改善
- フォントファイルの作成/修正日時が大きすぎたときに例外を吐いて落ちる不具合を修正

### ver.3.2.0 (2024/04/21)
- フォントファイルの読み込みをTypeLoaderから自前のコードに変更
  - 対応しているフォントを選択した場合、プロポーショナルフォントが使えるようになった。
  - 使用できるフォントの種類が増えた。
- 代替描画機能を追加（AlternateRendering; 処理は重いが、ＭＳ Ｐ明朝などのフォントを美しく描画できる）
- 折り返し設定なし、かつTextAlignment.Justify設定で表示範囲よりテキストのサイズのほうが大きかったとき、文字が重なる不具合を修正
- 半角文字を横書きにするオプションが有効の場合、他の文末禁止文字との組み合わせによっては正しく横書きにならなかったり、正しく折り返されなかったりする不具合を修正

### ver.3.1.0 (2024/04/13)
- TategakiTextに以下の機能を実装
  - TextWrappingプロパティを実装（複数行対応）
  - 禁則文字（文末禁止文字、文頭禁止文字）、文末ぶらさげ文字を実装
  - TextDecorationsプロパティを実装（下線、取消線などを付けられるようになった）
  - LineHeightプロパティを実装（行の高さを設定できるようになった）
  - TextAlignmentプロパティを実装
  - Paddingプロパティを実装
  - EnableHalfWidthCharVerticalプロパティを追加（半角文字を縦書きにするか横書きにするかのオプション）
- 使えるフォントの種類が増えた（今までは全ユーザー用にインストールされたフォントしか使えなかった）
- TategakiMultilineにObsolete属性を追加（TategakiTextで複数行表示ができるようになり、使う必要がなくなったため）

### ver.3.0.1 (2024/04/06)
- ターゲットに.NET Framework 4.7.2も追加。

### ver.3.0.0 (2024/04/05)
- TategakiTextコントロールを刷新した。
  - カスタムコントロールではなく、FrameworkElementを継承したクラスとして実現。
- TategakiMultilineの禁則文字処理を高速化した。
- ターゲットを.NET Frameworkから.NET 6に変更した。

### ver.2.1.1 (2015/12/18)
- コードを一部C#6化した
- コントロールの名前空間をURLにした（アセンブリ参照でも使える）
- Nugetデビュー

### ver.2.1.0 (2015/01/24)
- フォント抽出とグリフインデックス変換をTypeLoaderで行うようにした。それによって扱えるフォント数が増えた（と思う）。

### ver.2.0.0 (2015/01/21)
- コントロールをカスタムコントロールで作りなおした。それによってデフォルトで親のフォントを継承できるようになった。
- TategakiMultilineをWrapPanelを使って実現した。
- 実装上の都合でTategakiMultilineから文末ぶら下げ処理と文字間スペーシング機能が削除された。

### ver.1.1.2 (2014/12/04)
- Meiryo UIの縦書きフォントが取得でいない環境でコントロールが使えない問題を修正
- 禁則処理やぶら下げ組みに対応
- 行のサイズを計算するプログラムを高速化した

### ver.1.1.1 (2014/12/03)
- 一部、プロパティを変更しても画面に反映されないバグを修正
- 利用できるフォントを取得するGetAvailableFontsメソッドをAvailableFontsプロパティに変更した
- 行のサイズを計算するプログラムを2倍くらい高速化した

### ver.1.1.0 (2014/08/01)
- 複数行（自動折り返し）の縦書きコントロール"TategakiMultiline"を実装
- 太字、斜体に対応
- 比較的短い長さのテキストを表示させようとするとバグる不具合を修正
- フォントファミリ名やサイズなどをXAMLで指定しないと表示されないバグを修正

### ver.1.0.0 (2014/07/22)
- 初公開
