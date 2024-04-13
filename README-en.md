[Japanese Readme is here](https://github.com/EH500-Kintarou/Tategaki/blob/master/README.md)

# WPF Vertical TextBlock "Tategaki"

This control library provides WPF vertical TextBlock.  
WPF doesn't support vertical writing so that it is difficult to show a text vertically like a Japnese text. However, this control realize a vertical text like a WPF standard TextBlock control easily.

"Tategaki" means "Vertical writing" in Japanese.

![](https://img.shields.io/badge/Nuget-3.1.0-blue?logo=nuget&style=plastic)
![](https://img.shields.io/badge/.NET_Framework-4.7.2-orange?logo=.net&style=plastic)
![](https://img.shields.io/badge/.NET-6-orange?logo=.net&style=plastic)

![Screenshot of Tategaki](https://raw.githubusercontent.com/EH500-Kintarou/Tategaki/master/Images/SampleScreenshot.png)

## Required Environment

- Windows OS
- .NET 6 / .NET Framework 4.7.2

## How to use
### 1. Get via Nuget
![](https://img.shields.io/badge/Nuget-3.1.0-blue?logo=nuget&style=plastic) https://www.nuget.org/packages/Tategaki/

### 2. Add XAML namespace
Add namespace of "http://schemas.eh500-kintarou.com/Tategaki" and add the elements of "TategakiText" or "TategakiMultiline" in your XAML.
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

### 3. Check sample code out
This repository contains [a sample project](https://github.com/EH500-Kintarou/Tategaki/tree/master/TategakiSample). It will make you more clear how to use it.

## Project URL
![](https://img.shields.io/badge/Github-3.1.0-green?logo=github&style=plastic) https://github.com/EH500-Kintarou/Tategaki  
![](https://img.shields.io/badge/Nuget-3.1.0-blue?logo=nuget&style=plastic) https://www.nuget.org/packages/Tategaki/  
![](https://img.shields.io/badge/Blogger-3.1.0-orange?logo=blogger&style=plastic) https://days-of-programming.blogspot.com/search/label/WPF%E7%94%A8%E7%B8%A6%E6%9B%B8%E3%81%8D%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88%E3%83%96%E3%83%AD%E3%83%83%E3%82%AF%20Tategaki

## Libraries which is reffered to
- TypeLoader: https://typeloader.codeplex.com/  
TypeLoeader URL has been already expired so that it is contained in this repository.
- Extended WPF Toolkit (Only in sample app): https://github.com/xceedsoftware/wpftoolkit

## Version History
### ver.3.1.0 (13-April-2024)
- Following functions are added to TategakiText.
  - TextWrapping property is implemented.
  - Forbidden charactor rules (Last-forbidden, Head-forbidden) and last hanging charactor rule are implemented.
  - TextDecorations property is implemented.
  - LineHeight property is implemented.
  - TextAlignment property is implemented.
  - Padding is implemented.
  - EnableHalfWidthCharVertical property is implemented, which allows to choose whether half-width characters are written in vertical or horizontal.
- Font variations which can be used are increased.
- Obsolete attribute is added to TategakiMultiline Control because TategakiText can describe multiline and wrapping text nowadays.

### ver.3.0.1 (06-April-2024)
- .NET Framework 4.7.2 is added to Target Frameworks.

### ver.3.0.0 (05-April-2024)
- Overhauled TategakiText Control
  - It is implemented by inheriting FrameworkElement class instead of a custom control.
- Overhauled and fastened the process of forbidden characters in TategakiMultiline.
- Target framework was changed to .NET6 from .NET Framework.

For more older history, see Japanese Readme.
