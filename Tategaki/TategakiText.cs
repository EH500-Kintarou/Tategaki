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
using Tategaki.Logic;

namespace Tategaki
{
	public class TategakiText : FrameworkElement
	{
		List<List<GlyphRunParam>> glyphs = new();

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
		/// 文字間隔
		/// </summary>
		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
			nameof(Spacing), typeof(double), typeof(TategakiText),
			new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// 半角文字を縦書きにする
		/// </summary>
		public bool EnableHalfWidthCharVertical
		{
			get { return (bool)GetValue(EnableHalfWidthCharVerticalProperty); }
			set { SetValue(EnableHalfWidthCharVerticalProperty, value); }
		}
		public static readonly DependencyProperty EnableHalfWidthCharVerticalProperty = DependencyProperty.Register(
			nameof(EnableHalfWidthCharVertical), typeof(bool), typeof(TategakiText),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		#region TextElement

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
		public static readonly DependencyProperty BackgroundProperty = TextElement.BackgroundProperty.AddOwner(typeof(TategakiText));

		// FontStretchは未実装

		#endregion

		#region Inline

		// BaselineAlignmentは未実装
		// FlowDirectionは未実装
		// TextDecorationsは未実装

		#endregion

		#region Block

		// BorderBrushは未実装
		// BorderThicknessは未実装
		// BreakColumnBeforeは未実装
		// BreakPageBeforeは未実装
		// ClearFloatersは未実装
		// FlowDirectionは未実装
		// IsHyphenationEnabledは未実装
		// LineHeightは未実装
		// LineStackingStrategyは未実装
		// Marginは未実装
		// Paddingは未実装
		// TextAlignmentは未実装

		#endregion

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
			glyphs.Clear();

			if(string.IsNullOrEmpty(text))
				glyphs.Add(new());
			else {
				var line = new List<GlyphRunParam>();

				var hwvert = EnableHalfWidthCharVertical;
				int start = 0;
				bool? halfwidth = null;
				for(int i = 0; i < text!.Length; i++) {
					var c = text[i];

					if(c == '\n') {     // 改行
						if(halfwidth != null && start < i)
							line.Add(new GlyphRunParam(text.Substring(start, i - start), !halfwidth.Value, FontFamily?.Source, FontWeight, FontStyle, FontSize, Spacing, XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)));
						
						glyphs.Add(line);
						line = new List<GlyphRunParam>();
						halfwidth = null;
					} else if(c < ' ') {    // それ以外の制御文字は無視（スペースより文字コードが小さいのは制御文字）
						if(halfwidth != null && start < i)
							line.Add(new GlyphRunParam(text.Substring(start, i - start), !halfwidth.Value, FontFamily?.Source, FontWeight, FontStyle, FontSize, Spacing, XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)));
						halfwidth = null;
					} else if(c < 0x80 && !hwvert) {	// 半角文字
						if(halfwidth == false)
							line.Add(new GlyphRunParam(text.Substring(start, i - start), true, FontFamily?.Source, FontWeight, FontStyle, FontSize, Spacing, XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)));
						if(halfwidth != true)
							start = i;
						
						halfwidth = true;
					} else {
						if(halfwidth == true)
							line.Add(new GlyphRunParam(text.Substring(start, i - start), false, FontFamily?.Source, FontWeight, FontStyle, FontSize, Spacing, XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)));
						if(halfwidth != false)
							start = i;

						halfwidth = false;
					}
				}

				if(halfwidth != null && start < text.Length)
					line.Add(new GlyphRunParam(text.Substring(start, text.Length - start), !halfwidth.Value, FontFamily?.Source, FontWeight, FontStyle, FontSize, Spacing, XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)));

				glyphs.Add(line);
			}

			var sizeBeforeRotate = glyphs
				.Select(p => p.Count == 0 ? new Size(FontSize * 4 / 3, 0) : p.Aggregate(new Size(), (left, right) => new Size(Math.Max(left.Width, right.GlyphBox.Width), Math.Max(left.Height, right.GlyphBox.Height))))
				.Aggregate(new Size(), (left, right) => new Size(Math.Max(left.Width, right.Width), left.Height + right.Height));

			return new Size(sizeBeforeRotate.Height, sizeBeforeRotate.Width);
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

			ctx.PushClip(new RectangleGeometry(renderRect));	// これ以後の描画はクリッピングされる
			ctx.PushTransform(new RotateTransform(90, RenderSize.Width / 2, RenderSize.Width / 2));     // これ以後の描画は回転される

			var foreground = Foreground ?? Brushes.Black;
			var y = 0.0;
			foreach(var line in glyphs) {
				var height = line.Count > 0 ? line.Max(p => p.GlyphBox.Height) : 0.0;
				var x = 0.0;

				foreach(var section in line) {
					var box = section.GlyphBox;
					ctx.DrawGlyphRun(foreground, section.Create(new Point(x, y + height - box.Height - box.Top)));
					x += box.Width;
				}

				y += height;
			}
		}

		#endregion
	}
}
