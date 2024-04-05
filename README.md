[English Readme is here](README-en.md)

# WPF用縦書きテキストブロック Tategaki

WPFアプリケーションにおいて縦書きを使用するためのコントロールライブラリです。  
WPFはネイティブで縦書きをサポートしておらず簡単に日本語の縦書きのようなものを作ることができませんが、このコントロールを利用すれば、WPF標準のTextBlockコントロールを使うような感覚で縦書きを実現することができます。

![Screenshot of Tategaki](https://raw.githubusercontent.com/EH500-Kintarou/Tategaki/master/Images/SampleScreenshot.png)

## 動作環境

- Windows OS
- .NET6

## 使用方法
### 1. Nugetからインストール
![](https://img.shields.io/badge/Nuget-3.0.0-004880.svg?logo=nuget&style=plastic) https://www.nuget.org/packages/Tategaki/

### 2. XAML名前空間を設定
XAMLで名前空間 "http://schemas.eh500-kintarou.com/Tategaki" を登録し、"TategakiText"要素および"TategakiMultiline"要素を追加することで縦書きのテキストを表示させることができるようになります。
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

## 利用しているライブラリ
- TypeLoader: https://typeloader.codeplex.com/  
TypeLoeaderはすでにリンク切れを起こしているため、当リポジトリ内に取り込んだうえで.NET6でビルドして利用しております。
- Extended WPF Toolkit (サンプルコード内のみ): https://github.com/xceedsoftware/wpftoolkit

## バージョン履歴
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
