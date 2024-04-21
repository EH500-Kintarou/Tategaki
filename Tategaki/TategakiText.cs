using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Tategaki.Logic;

namespace Tategaki
{
	public class TategakiText : FrameworkElement
	{
		StringGlyphIndexCache? textcache;
		FontGlyphCache? glyphcache;
		List<(List<GlyphRunParam> glyphs, Size size)> lines = new();
		TextAlignment? lastTextAlignment = null;

		public TategakiText()
		{
			TextDecorations = new TextDecorationCollection();
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
					_AvailableFonts = VerticalFontTable.FamilyNames.ToArray();
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

		/// <summary>
		/// プロポーショナルフォントを有効にする
		/// </summary>
		public bool EnableProportionalAlternate
		{
			get { return (bool)GetValue(EnableProportionalAlternateProperty); }
			set { SetValue(EnableProportionalAlternateProperty, value); }
		}
		public static readonly DependencyProperty EnableProportionalAlternateProperty = DependencyProperty.Register(
			nameof(EnableProportionalAlternate), typeof(bool), typeof(TategakiText),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// 代替レンダリングを有効にする
		/// falseの場合はDrawGlyphRunを使用して描画します。多くの場合はこちらで問題ないです。
		/// trueの場合、DrawGeometryを使用して描画します。描画処理は重くなりますが、ＭＳ Ｐ明朝などで文字間隔がおかしくなる場合はこちらを使用すると改善することがあります。
		/// </summary>
		public bool EnableAlternateRendering
		{
			get { return (bool)GetValue(EnableAlternateRenderingProperty); }
			set { SetValue(EnableAlternateRenderingProperty, value); }
		}
		public static readonly DependencyProperty EnableAlternateRenderingProperty = DependencyProperty.Register(
			nameof(EnableAlternateRendering), typeof(bool), typeof(TategakiText),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));


		#region TextWrapping

		/// <summary>
		/// 文字を折り返すオプション
		/// </summary>
		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)GetValue(TextWrappingProperty); }
			set { SetValue(TextWrappingProperty, value); }
		}
		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
			nameof(TextWrapping), typeof(TextWrapping), typeof(TategakiText),
			new FrameworkPropertyMetadata(TextWrapping.NoWrap, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender),
			IsValidTextWrap);

		/// <summary>
		/// 文末に禁止された文字
		/// </summary>
		public string LastForbiddenChars
		{
			get { return (string)GetValue(LastForbiddenCharsProperty); }
			set { SetValue(LastForbiddenCharsProperty, value); }
		}
		public static readonly DependencyProperty LastForbiddenCharsProperty = DependencyProperty.Register(
			nameof(LastForbiddenChars), typeof(string), typeof(TategakiText),
			new FrameworkPropertyMetadata("（［｛「『([{｢", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

		/// <summary>
		/// 文頭に禁止された文字
		/// </summary>
		public string HeadForbiddenChars
		{
			get { return (string)GetValue(HeadForbiddenCharsProperty); }
			set { SetValue(HeadForbiddenCharsProperty, value); }
		}
		public static readonly DependencyProperty HeadForbiddenCharsProperty = DependencyProperty.Register(
			nameof(HeadForbiddenChars), typeof(string), typeof(TategakiText),
			new FrameworkPropertyMetadata("、。，．・？！゛゜ヽヾゝゞ々ー）］｝」』!),.:;?]}｡｣､･ｰﾞﾟァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇷ゚ㇺㇻㇼㇽㇾㇿ々〻",
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

		/// <summary>
		/// 文末にぶら下げる文字
		/// </summary>
		public string LastHangingChars
		{
			get { return (string)GetValue(LastHangingCharsProperty); }
			set { SetValue(LastHangingCharsProperty, value); }
		}
		public static readonly DependencyProperty LastHangingCharsProperty = DependencyProperty.Register(
			nameof(LastHangingChars), typeof(string), typeof(TategakiText),
			new FrameworkPropertyMetadata("、。，．,.｡､ 　", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

		#endregion

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
		/// フォントの引き伸ばし
		/// </summary>
		public FontStretch FontStretch
		{
			get { return (FontStretch)GetValue(FontStretchProperty); }
			set { SetValue(FontStretchProperty, value); }
		}
		public static readonly DependencyProperty FontStretchProperty =
			TextElement.FontStretchProperty.AddOwner(typeof(TategakiText));

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

		#endregion

		#region Inline

		/// <summary>
		/// 取り消し線などの装飾
		/// </summary>
		public TextDecorationCollection TextDecorations
		{
			get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
			set { SetValue(TextDecorationsProperty, value); }
		}
		public static readonly DependencyProperty TextDecorationsProperty =
			Inline.TextDecorationsProperty.AddOwner(typeof(TategakiText), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		// BaselineAlignmentは未実装
		// FlowDirectionは未実装

		#endregion

		#region Block

		/// <summary>
		/// 行の高さ。FontSizeを下回るとFontSizeが優先される。
		/// </summary>
		public double LineHeight
		{
			get { return (double)GetValue(LineHeightProperty); }
			set { SetValue(LineHeightProperty, value); }
		}
		public static readonly DependencyProperty LineHeightProperty = Block.LineHeightProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// 文字の配置
		/// </summary>
		public TextAlignment TextAlignment
		{
			get { return (TextAlignment)GetValue(TextAlignmentProperty); }
			set { SetValue(TextAlignmentProperty, value); }
		}
		public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(typeof(TategakiText));

		/// <summary>
		/// パディング
		/// </summary>
		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}
		public static readonly DependencyProperty PaddingProperty = Block.PaddingProperty.AddOwner(
			typeof(TategakiText), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure));

		// BorderBrushは未実装
		// BorderThicknessは未実装
		// BreakColumnBeforeは未実装
		// BreakPageBeforeは未実装
		// ClearFloatersは未実装
		// FlowDirectionは未実装
		// IsHyphenationEnabledは未実装
		// LineStackingStrategyは未実装

		#endregion

		#region Validation

		private static bool IsValidTextWrap(object o)
		{
			TextWrapping value = (TextWrapping)o;
			return value == TextWrapping.Wrap || value == TextWrapping.NoWrap || value == TextWrapping.WrapWithOverflow;
		}

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
			lastTextAlignment = null;

			//if(string.IsNullOrEmpty(text)) {	// .NET Framework 4.7.2はこれでnullフロー解析が働かない
			if(text == null || text == "") {
				textcache = null;
				lines.Add((new(), new(0, FontSize)));
			} else {
				var fontname = VerticalFontTable.FromName(FontFamily?.Source);
				var weight = FontWeight;
				var style = FontStyle;
				var stretch = FontStretch;
				var hwvert = EnableHalfWidthCharVertical;
				var enablevpal = EnableProportionalAlternate;
				var spacing = Spacing;
				var fontsize = FontSize;
				var language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);

				if(glyphcache == null || !glyphcache.ParamEquals(fontname, weight, style, stretch))
					glyphcache = FontGlyphCache.GetCache(fontname, weight, style, stretch);

				if(textcache == null || !textcache.ParamEquals(text, glyphcache, hwvert, enablevpal))
					textcache = new StringGlyphIndexCache(text, glyphcache, hwvert, enablevpal);

				var lineheight = Math.Max(double.IsNaN(LineHeight) ? 0 : LineHeight, glyphcache.GlyphTypeface.Height * fontsize);

				var line = new List<GlyphRunParam>();
				var context = new ContextAnalyzer() {
					TextWrapping = TextWrapping,
					AvailableWidth = availableSize.Height - Padding.Top - Padding.Bottom,  // 90°回るのでWidthとHeightが入れ替わる
					LastForbiddenChars = LastForbiddenChars,
					HeadForbiddenChars = HeadForbiddenChars,
					LastHangingChars = LastHangingChars,
				};
				context.NewSectionCallback = (int start, int endEx, double width) =>
					line.Add(new GlyphRunParam(glyphcache, textcache, start, endEx, fontsize, spacing, language));
				context.NewLineCallback = (double width) => {
					double widthtotal = 0;
					for(int i = 0; i < line.Count; i++)
						widthtotal += (i == line.Count - 1) ? line[i].TotalBoxWidth : line[i].TotalBoxWidthWithSpacing;

					lines.Add((line, new Size(widthtotal, lineheight)));
					line = new();
				};

				for(int i = 0; i < textcache.Text.Length; i++)
					context.NextChar(textcache.Text[i], i, textcache.IsVerticals[i], textcache.AdvanceWidths[i] * fontsize, (spacing - 100) / 100.0 * fontsize);
				context.GetRemaining(text.Length);
			}

			var sizeBeforeRotate = lines.Select(p => p.size).Aggregate(new Size(), (left, right) => new Size(Math.Max(left.Width, right.Width), left.Height + right.Height));
			var underlinemargin = ((glyphcache?.GlyphTypeface?.Baseline ?? 1.0) - (glyphcache?.GlyphTypeface?.UnderlinePosition ?? 0.0) - 1) * FontSize + 1;
			var width = sizeBeforeRotate.Height + Padding.Left + Padding.Right + underlinemargin;
			var height = (TextAlignment == TextAlignment.Justify) ? availableSize.Height : sizeBeforeRotate.Width + Padding.Top + Padding.Bottom;

			return new Size(width, height);
		}


		/// <summary>
		/// この要素のサイズを決定したときに呼ばれるメソッド
		/// </summary>
		/// <param name="finalSize">最終決定したサイズ</param>
		/// <returns>実際に使用されたサイズ</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			var fontsize = FontSize;
			var nowAlign = TextAlignment;

			// TextAlignmentの調整を行う
			if(lastTextAlignment != TextAlignment.Justify && nowAlign == TextAlignment.Justify) {	// Justify以外→Justifyに変化
				var finalwidth = finalSize.Height - Padding.Top - Padding.Bottom;

				foreach(var line in lines) {
					var textwidth = line.glyphs.Sum(p => p.TotalAdvanceWidth);
					var charcount = line.glyphs.Sum(p => p.AdvanceWidths.Count);
					var spacing = (charcount >= 2 && textwidth <= finalwidth) ? (finalwidth - textwidth) / (charcount - 1) : 0.0;

					foreach(var glyph in line.glyphs)
						glyph.ApplyJustifyAlignmentOffset(spacing);					
				}
			} else if(lastTextAlignment == TextAlignment.Justify && nowAlign != TextAlignment.Justify) {    // Justify→Justify以外に変化
				foreach(var line in lines) {
					foreach(var glyph in line.glyphs)
						glyph.ApplyNormalAlignmentOffset();
				}
			}

			lastTextAlignment = nowAlign;

			return finalSize;
		}

		/// <summary>
		/// レイアウトシステムによる描画に介入するメソッド
		/// この描画指示はこのメソッドが呼ばれたときに直ちに実行はされず、保管され、後の描画処理時に使用されます。
		/// </summary>
		/// <param name="ctx">この要素のための描画コンテキスト</param>
		protected override void OnRender(DrawingContext ctx)
		{
			var renderRect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
			var altrender = EnableAlternateRendering;

			// 背景を描画
			if(Background != null)
				ctx.DrawRectangle(Background, null, renderRect);

			// クリッピングと回転を設定
			ctx.PushClip(new RectangleGeometry(renderRect));    // これ以後の描画はクリッピングされる
			ctx.PushTransform(new RotateTransform(90, RenderSize.Width / 2, RenderSize.Width / 2));     // これ以後の描画は回転される

			// 装飾（下線など）を取得
			var decorations = GetDecorations();

			// 文字を描画
			var foreground = Foreground ?? Brushes.Black;
			var y = Padding.Right;
			foreach(var (glyphs, size) in lines) {
				var height = size.Height;

				var xstart = TextAlignment switch {
					TextAlignment.Right => RenderSize.Height - size.Width - Padding.Bottom,
					TextAlignment.Center => (RenderSize.Height - size.Width + Padding.Top - Padding.Bottom) / 2,
					_ => Padding.Top,
				};
				var x = xstart;
				var xNoLastSpacing = x;

				foreach(var glyph in glyphs) {
					if(altrender) {
						var xnext = x + glyph.TotalBoxWidthWithSpacing;
						xNoLastSpacing = x + glyph.TotalBoxWidth;
						for(int i = 0; i < glyph.Text.Length; i++) {
							var geometry = glyph.GlyphTypeface.GetGlyphOutline(glyph.GlyphIndices[i], FontSize, FontSize);
							var aw = glyph.AdvanceWidths[i];

							var tf = new TransformGroup();
							if(glyph.IsSideways) {
								tf.Children.Add(new RotateTransform(-90, 0, 0));
								tf.Children.Add(new TranslateTransform(
									x + glyph.GlyphOffsets[i].X + glyph.AlternateRenderingOffsets[i].X + glyph.GlyphTypeface.Baseline * glyph.RenderingEmSize,
									y + glyph.GlyphOffsets[i].Y + glyph.AlternateRenderingOffsets[i].Y + glyph.GlyphTypeface.Height * glyph.RenderingEmSize));
							} else {
								tf.Children.Add(new TranslateTransform(
									x + glyph.GlyphOffsets[i].X + glyph.AlternateRenderingOffsets[i].X,
									y + glyph.GlyphOffsets[i].Y + glyph.AlternateRenderingOffsets[i].Y + glyph.GlyphTypeface.Baseline * glyph.RenderingEmSize));
							}
							geometry.Transform = tf;
							ctx.DrawGeometry(foreground, null, geometry);
							x += aw;
						}
						x += xnext;
					} else {
						ctx.DrawGlyphRun(foreground, glyph.CreateWithOffsetY0(new Point(x, y)));
						xNoLastSpacing = x + glyph.TotalBoxWidth;
						x += glyph.TotalBoxWidthWithSpacing;
					}
				}

				// 1行ごとに装飾も実施
				foreach(var deco in decorations)
					ctx.DrawLine(deco.pen, new Point(xstart, y + deco.y), new Point(xNoLastSpacing, y + deco.y));

				y += height;
			}
		}

		private (double y, Pen pen)[] GetDecorations()
		{
			var fontsize = FontSize;
			var fontheight = (glyphcache?.GlyphTypeface?.Height ?? 1.0) * fontsize;
			var baseline = glyphcache?.GlyphTypeface?.Baseline ?? 1.0;
			var strikethrough = glyphcache?.GlyphTypeface?.StrikethroughPosition ?? 0.5;
			var underline = glyphcache?.GlyphTypeface?.UnderlinePosition ?? 0.0;
			var stthickness = glyphcache?.GlyphTypeface.StrikethroughPosition ?? 1.0;
			var ulthickness = glyphcache?.GlyphTypeface.UnderlineThickness ?? 1.0;

			return (TextDecorations ?? Enumerable.Empty<TextDecoration>())
				.Select(p => {
					double y = p.Location switch {
						TextDecorationLocation.Baseline => baseline * fontheight,
						TextDecorationLocation.OverLine => 0,
						TextDecorationLocation.Strikethrough => (baseline - strikethrough) * fontheight,
						_ => (baseline - underline) * fontheight,	// default: Underline
					};
					double thickness = p.Location switch {
						TextDecorationLocation.Baseline => ulthickness * fontsize,
						TextDecorationLocation.OverLine => ulthickness * fontsize,
						TextDecorationLocation.Strikethrough => stthickness * fontsize,
						_ => ulthickness * fontsize,				// default: Underline
					};

					var pen = p.Pen ?? new Pen(Foreground, thickness);
					if(p.PenThicknessUnit == TextDecorationUnit.FontRenderingEmSize) {
						pen = pen.Clone();
						pen.Thickness *= fontsize;
					}

					y += p.PenOffset * ((p.PenOffsetUnit == TextDecorationUnit.FontRenderingEmSize) ? fontsize : 1);

					return (y, pen);
				}).ToArray();
		}

		#endregion

		/// <summary>
		/// 文字を1文字ずつ受けて改行位置などを判断していくメソッド
		/// </summary>
		private struct ContextAnalyzer
		{
			public ContextAnalyzer()
			{
				LastForbiddenChars = "";
				HeadForbiddenChars = "";
				LastHangingChars = "";
			}

			#region Properties

			/// <summary>
			/// 区間（=1つのGlyphRunで表現できる範囲）が切り替わったときに呼ばれるメソッド
			/// </summary>
			public NewSectionCallback? NewSectionCallback { get; set; } = default;

			/// <summary>
			/// 改行が発生したときに呼ばれるメソッド
			/// </summary>
			public NewLineCallback? NewLineCallback { get; set; } = default;

			/// <summary>
			/// 改行オプション
			/// </summary>
			public TextWrapping TextWrapping { get; set; } = default;

			/// <summary>
			/// 文字を表示可能な幅
			/// </summary>
			public double AvailableWidth { get; set; } = default;

			/// <summary>
			/// 文末に禁止された文字
			/// </summary>
			public string LastForbiddenChars { get; set; }

			/// <summary>
			/// 文頭に禁止された文字
			/// </summary>
			public string HeadForbiddenChars { get; set; }

			/// <summary>
			/// 文末にぶら下げる文字
			/// </summary>
			public string LastHangingChars { get; set; }

			#endregion

			int startPos = 0;				// 現在の区間のスタート位置
			bool? prevvertical = null;		// 前の文字が縦書きか横書きか
			double sectionWidth = 0;		// 区間の幅
			double lineWidth = 0;           // 行の幅

			int blockStartPos = 0;
			double blockStartWidth = 0;
			bool isPrevLastForbidden = false;

			public void NextChar(char c, int pos, bool? isvertical, double width, double spacing)
			{
				if(c == '\n') {     // 改行
					if(prevvertical != null && startPos < pos) {
						NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
						lineWidth += sectionWidth;
						sectionWidth = 0;
					}

					NewLineCallback?.Invoke(lineWidth);
					lineWidth = 0;
					prevvertical = null;
				} else if(isvertical == null) {    // 縦も横も無い文字（制御文字など）なら無視
					if(prevvertical != null && startPos < pos) {
						NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
						lineWidth += sectionWidth;
						sectionWidth = 0;
					}
					prevvertical = null;
				} else if(prevvertical != null && prevvertical != isvertical) { // 縦書き⇔横書きの変化なら強制的にブロック分け
					if(startPos < pos) {
						NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
						lineWidth += sectionWidth;
					}
					startPos = pos;
					sectionWidth = width + spacing;
					isPrevLastForbidden = false;
					blockStartPos = 0;
					blockStartWidth = 0;
					prevvertical = isvertical;

					if(TextWrapping != TextWrapping.NoWrap && (lineWidth + width) > AvailableWidth) {	// 必要に応じて改行も実施
						NewLineCallback?.Invoke(lineWidth);
						lineWidth = 0;
					}
				} else {
					// ○: 普通の文字 / ●: 文末禁止文字 / ◎: 文頭禁止文字
					// 　　⇩オーバーフロー
					// ○○○○○
					// 　　⇧blockStartPos←こいつで改行
					// 　　　⇩オーバーフロー
					// ○○●◎○○
					// 　　⇧blockStartPos←こいつで改行
					// 　　　　⇩オーバーフロー
					// ○○●○◎○○
					// 　　⇧blockStartPos←こいつで改行
					// 　　　　　⇩オーバーフロー
					// ○○●○○◎○○
					// 　　　　⇧blockStartPos ←こいつで改行
					// 　　　　　⇩オーバーフロー
					// ○○●○◎◎○○
					// 　　⇧blockStartPos ←こいつで改行
					// 　　　　　⇩オーバーフロー
					// ○○●○◎○◎○○
					// 　　　　　⇧blockStartPos ←こいつで改行
					//
					// blockStartPosをposで上書きする条件は「前回が●でない、かつ、現在が◎でない」
					// 「●と次」または「◎と手前」は塊（行をまたぐことができない）という意味を込めてblock～という名前にしている

					if(!isPrevLastForbidden && !HeadForbiddenChars.Contains(c)) {
						blockStartPos = pos;
						blockStartWidth = sectionWidth;
					}

					bool needWrap = TextWrapping != TextWrapping.NoWrap && (lineWidth + sectionWidth + width) > AvailableWidth; // はみ出し判定にはspacingは使わない
					if(needWrap && LastHangingChars.Contains(c))
						needWrap = false;   // ぶらさげ文字だったらやっぱり改行は無しにする

					// 区間の切り替わり目か確認
					if(prevvertical != null && needWrap) {
						if(startPos < blockStartPos) { // スタート地点以降につながっている部分がある
							NewSectionCallback?.Invoke(startPos, blockStartPos, blockStartWidth);
							lineWidth += blockStartWidth;
							sectionWidth -= blockStartWidth;
							startPos = blockStartPos;
						} else if(TextWrapping == TextWrapping.Wrap) {  // 区間の開始地点以降に改行できるところが無かったので、禁則処理を諦める
							NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
							lineWidth += sectionWidth;
							sectionWidth = 0;
							startPos = pos;
						} else  // 区間の開始地点以降に改行できるところが無かったので、改行そのものを諦める
							needWrap = false;

						if(needWrap) {
							NewLineCallback?.Invoke(lineWidth);
							lineWidth = 0;
						}
					}
					if(prevvertical == null && isvertical != null)   // 無効な文字以降初めての有効な文字ならスタート地点とする
						startPos = pos;

					prevvertical = isvertical;
					sectionWidth += width + spacing;

					isPrevLastForbidden = LastForbiddenChars.Contains(c) || CheckWordChars(c);
				}
			}

			public void GetRemaining(int pos)
			{
				if(prevvertical != null && startPos < pos) {
					NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
					lineWidth += sectionWidth;
					sectionWidth = 0;
					startPos = pos;
				}

				if(lineWidth > 0)
					NewLineCallback?.Invoke(lineWidth);
			}

			private bool CheckWordChars(char c)
			{
				return ('0' <= c && c <= '9') ||
					   ('a' <= c && c <= 'z') ||
					   ('A' <= c && c <= 'Z') ||
					   c == '_';
			}
		}

		delegate void NewSectionCallback(int start, int endEx, double width);

		delegate void NewLineCallback(double width);
	}
}
