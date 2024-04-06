﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tategaki.Logic;
using static System.Net.Mime.MediaTypeNames;

namespace Tategaki
{
	public class TategakiText : FrameworkElement
	{
		GlyphRunParam? param = null;

		public TategakiText()
		{
		}

		#region Properties

		/// <summary>
		/// このコントロールで使えるフォント名
		/// </summary>
		public static string[] AvailableFonts
		{
			get
			{
				if(_AvailableFonts == null)
					_AvailableFonts = FontUriTable.CultureVerticalFonts[CultureInfo.CurrentUICulture].Select(p => p.Key).ToArray();
				return _AvailableFonts;
			}
		}
		static string[]? _AvailableFonts = null;

		/// <summary>
		/// 表示テキスト
		/// </summary>
		public string? Text
		{
			get { return (string?)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text), typeof(string), typeof(TategakiText),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// フォントファミリー
		/// </summary>
		public FontFamily? FontFamily
		{
			get { return (FontFamily?)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}
		public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// フォントサイズ
		/// </summary>
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}
		public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// フォントの太さ
		/// </summary>
		public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}
		public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// フォントスタイル
		/// </summary>
		public FontStyle FontStyle
		{
			get { return (FontStyle)GetValue(FontStyleProperty); }
			set { SetValue(FontStyleProperty, value); }
		}
		public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// 文字間隔
		/// </summary>
		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
			nameof(Spacing), typeof(double), typeof(TategakiText), new FrameworkPropertyMetadata(
				100.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// 文字の色
		/// </summary>
		public Brush? Foreground
		{
			get { return (Brush?)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}
		public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// 背景色
		/// </summary>
		public Brush? Background
		{
			get { return (Brush?)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}
		public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
			nameof(Background), typeof(Brush), typeof(TategakiText), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		#endregion

		#region Overrides

		/// <summary>
		/// 要素のサイズを決定したいときに呼ばれるメソッド
		/// </summary>
		/// <param name="availableSize">この要素が使用可能なサイズ。無限大の場合はどのような大きさでも良いという意味となる。</param>
		/// <returns>この要素が必要とするサイズ。</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			var text = Text;
			if(string.IsNullOrEmpty(text)) {
				param = null;
				return new Size(FontSize * 4 / 3, 0);	// 文字列の幅分を確保する（1pt=は1/72in, 1px=1/96inより、4/3倍すればよい）
			} else {
				param = new GlyphRunParam(text!, FontFamily?.Source, FontSize, FontWeight, FontStyle, Spacing, XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name));
				return new Size(param.GlyphBox.Height, param.GlyphBox.Width);
			}
		}

		/// <summary>
		/// この要素のサイズを決定したときに呼ばれるメソッド
		/// </summary>
		/// <param name="finalSize">最終決定したサイズ</param>
		/// <returns>実際に使用されたサイズ</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			return base.ArrangeOverride(finalSize);
		}

		/// <summary>
		/// レイアウトシステムによる描画に介入するメソッド
		/// この描画指示はこのメソッドが呼ばれたときに直ちに実行はされず、保管され、後の描画処理時に使用されます。
		/// </summary>
		/// <param name="ctx">この要素のための描画コンテキスト</param>
		protected override void OnRender(DrawingContext ctx)
		{
			var renderRect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);

			if(Background != null)
				ctx.DrawRectangle(Background, null, renderRect);			

			if(param != null) {
				ctx.PushClip(new RectangleGeometry(renderRect));						// これ以後の描画はクリッピングされる
				ctx.PushTransform(new RotateTransform(90, param.GlyphBox.Height, 0));	// これ以後の描画は回転される

				var glyphrun = param.Create(new Point(param.GlyphBox.Height, -param.GlyphBox.Y));
				ctx.DrawGlyphRun(Foreground ?? Brushes.Black, glyphrun);
			}
		}

		#endregion

		private class GlyphRunParam
		{
			public GlyphRunParam(string text, string? fontname, double size, FontWeight weight, FontStyle style, double spacing, XmlLanguage language)
			{
				if(string.IsNullOrEmpty(text))
					throw new ArgumentException("Length of text must be more zan zero.", nameof(text));

				FontUri = FontUriTable.FromName(fontname);
				if(fontname == null)
					FontName = FontUriTable.AllVerticalFonts.Where(p => p.Value == FontUri).First().Key;
				else
					FontName = fontname;
				GlyphTypeface = new GlyphTypeface(FontUri, ((weight == FontWeights.Normal) ? StyleSimulations.None : StyleSimulations.BoldSimulation) | ((style == FontStyles.Normal) ? StyleSimulations.None : StyleSimulations.ItalicSimulation));

				Text = text;
				GlyphIndices = VerticalIndicesCache.GetCache(FontUri).GetIndices(Text);
				
				RenderingEmSize = size;
				AdvanceWidths = Enumerable.Repeat<double>(size, Text.Length).ToArray();
				GlyphOffsets = Enumerable.Range(0, Text.Length).Select(p => new Point(p * (spacing - 100) / 100 * size, 0)).ToArray();

				Language = language;

				GlyphBox = Create(new Point(0, 0)).ComputeAlignmentBox();
			}

			public string FontName { get;  }

			public Uri FontUri { get;  }

			public GlyphTypeface GlyphTypeface { get;  }

			public double RenderingEmSize { get;  }

			public IList<double> AdvanceWidths { get;  }

			public IList<Point> GlyphOffsets { get;  }

			public string Text { get;  }

			public IList<ushort> GlyphIndices { get;  }

			public XmlLanguage Language { get; }

			public Rect GlyphBox { get; }

			public GlyphRun Create(Point origin)
			{
				return new GlyphRun(GlyphTypeface, 0, true, RenderingEmSize, 1, GlyphIndices, origin, AdvanceWidths, GlyphOffsets, Text.ToArray(), FontName, null, null, Language);
			}
		}
	}
}
