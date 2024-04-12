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
		StringGlyphIndexCache? textcache;
		FontGlyphCache? glyphcache;
		List<(List<(GlyphRunParam glyph, double width)> glyphs, Size size)> lines = new();

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
			lines.Clear();

			//if(string.IsNullOrEmpty(text)) {	// .NET Framework 4.7.2はこれでnullフロー解析が働かない
			if(text == null || text == "") {
				textcache = null;
				lines.Add((new(), new(0, FontSize)));
			} else {
				var fontname = FontFamily?.Source;
				var weight = FontWeight;
				var style = FontStyle;
				var hwvert = EnableHalfWidthCharVertical;
				var spacing = Spacing;
				var fontsize = FontSize;
				var language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);

				if(glyphcache == null || !glyphcache.ParamEquals(fontname, weight, style))
					glyphcache = FontGlyphCache.GetCache(fontname, weight, style);

				if(textcache == null || !textcache.ParamEquals(text, fontname, weight, style, hwvert)) 
					textcache = new StringGlyphIndexCache(text, glyphcache, hwvert);

				var line = new List<(GlyphRunParam glyph, double width)>();
				int start = 0;
				bool? currentvert = null;
				double sectionwidth = 0;
				double totalwidth = 0;
				for(int i = 0; i < textcache.Text.Length; i++) {
					var c = textcache.Text[i];
					var index = textcache.Indices[i];
					var isvert = textcache.IsVerticals[i];
					var advwidth = textcache.AdvanceWidths[i];

					if(c == '\n') {     // 改行
						if(currentvert != null && start < i) {
							line.Add((new GlyphRunParam(glyphcache, textcache, start, i, fontsize, spacing, language), sectionwidth));
							totalwidth += sectionwidth;
							sectionwidth = 0;
						}

						lines.Add((line, new Size(totalwidth, glyphcache.GlyphTypeface.Height * fontsize)));
						line = new();
						totalwidth = 0;
						currentvert = null;
					} else if(isvert == null) {    // 縦も横も無い文字（制御文字など）なら無視
						if(currentvert != null && start < i) {
							line.Add((new GlyphRunParam(glyphcache, textcache, start, i, fontsize, spacing, language), sectionwidth));
							totalwidth += sectionwidth;
							sectionwidth = 0;
						}
						currentvert = null;
					} else {
						if(currentvert != null && currentvert != isvert) {
							line.Add((new GlyphRunParam(glyphcache, textcache, start, i, fontsize, spacing, language), sectionwidth));
							totalwidth += sectionwidth;
							sectionwidth = 0;
						}
						if(currentvert == null && isvert != null)
							start = i;

						currentvert = isvert;
						sectionwidth += advwidth * fontsize;
					}
				}

				if(currentvert != null && start < text.Length) {
					line.Add((new GlyphRunParam(glyphcache, textcache, start, text.Length, fontsize, spacing, language), sectionwidth));
					totalwidth += sectionwidth;
					sectionwidth = 0;
				}

				lines.Add((line, new Size(totalwidth, glyphcache.GlyphTypeface.Height * fontsize)));
			}

			var sizeBeforeRotate = lines.Select(p => p.size).Aggregate(new Size(), (left, right) => new Size(Math.Max(left.Width, right.Width), left.Height + right.Height));

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
			foreach(var line in lines) {
				var height = line.size.Height;
				var x = 0.0;

				foreach(var section in line.glyphs) {
					ctx.DrawGlyphRun(foreground, section.glyph.CreateWithOffsetY0(new Point(x, y)));

					x += section.width;
				}

				y += height;
			}
		}

		#endregion
	}
}
