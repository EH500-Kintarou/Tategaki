using System;
using System.Collections;
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
using System.Windows.Shapes;
using Tategaki.Logic;
using static System.Net.Mime.MediaTypeNames;

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

		/// <summary>
		/// 行の高さ。FontSizeを下回るとFontSizeが優先される。
		/// </summary>
		public double LineHeight
		{
			get { return (double)GetValue(LineHeightProperty); }
			set { SetValue(LineHeightProperty, value); }
		}
		public static readonly DependencyProperty LineHeightProperty =
			Block.LineHeightProperty.AddOwner(typeof(TategakiText));

		// BorderBrushは未実装
		// BorderThicknessは未実装
		// BreakColumnBeforeは未実装
		// BreakPageBeforeは未実装
		// ClearFloatersは未実装
		// FlowDirectionは未実装
		// IsHyphenationEnabledは未実装
		// LineStackingStrategyは未実装
		// Marginは未実装
		// Paddingは未実装
		// TextAlignmentは未実装

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

				var lineheight = Math.Max(double.IsNaN(LineHeight) ? 0 : LineHeight, glyphcache.GlyphTypeface.Height * fontsize);

				var line = new List<(GlyphRunParam glyph, double width)>();
				var context = new ContextAnalyzer() {
					TextWrapping = TextWrapping,
					AvailableWidth = availableSize.Height,  // 90°回るのでWidthとHeightが入れ替わる
					LastForbiddenChars = LastForbiddenChars,
					HeadForbiddenChars = HeadForbiddenChars,
					LastHangingChars = LastHangingChars,
				};
				context.NewSectionCallback = (int start, int endEx, double width) =>
					line.Add((new GlyphRunParam(glyphcache, textcache, start, endEx, fontsize, spacing, language), width));
				context.NewLineCallback = (double width) => {
					lines.Add((line, new Size(width, lineheight)));
					line = new();
				};

				for(int i = 0; i < textcache.Text.Length; i++)
					context.NextChar(textcache.Text[i], i, textcache.IsVerticals[i], textcache.AdvanceWidths[i] * fontsize);
				context.GetRemaining(text.Length);
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

			ctx.PushClip(new RectangleGeometry(renderRect));    // これ以後の描画はクリッピングされる
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
			bool? currentVertical = null;	// 現在縦書きか横書きか
			double sectionWidth = 0;		// 区間の幅
			double lineWidth = 0;           // 行の幅

			int connectionStartPos = 0;
			double connectionStartWidth = 0;
			bool isPrevLastForbidden = false;

			public void NextChar(char c, int pos, bool? isvertical, double width)
			{
				if(c == '\n') {     // 改行
					if(currentVertical != null && startPos < pos) {
						NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
						lineWidth += sectionWidth;
						sectionWidth = 0;
					}

					NewLineCallback?.Invoke(lineWidth);
					lineWidth = 0;
					currentVertical = null;
				} else if(isvertical == null) {    // 縦も横も無い文字（制御文字など）なら無視
					if(currentVertical != null && startPos < pos) {
						NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
						lineWidth += sectionWidth;
						sectionWidth = 0;
					}
					currentVertical = null;
				} else {
					// ○: 普通の文字 / ●: 文末禁止文字 / ◎: 文頭禁止文字
					// 　　⇩オーバーフロー
					// ○○○○○
					// 　　⇧connectionStartPos←こいつで改行
					// 　　　⇩オーバーフロー
					// ○○●◎○○
					// 　　⇧connectionStartPos←こいつで改行
					// 　　　　⇩オーバーフロー
					// ○○●○◎○○
					// 　　⇧connectionStartPos←こいつで改行
					// 　　　　　⇩オーバーフロー
					// ○○●○○◎○○
					// 　　　　⇧connectionStartPos ←こいつで改行
					// 　　　　　⇩オーバーフロー
					// ○○●○◎◎○○
					// 　　⇧connectionStartPos ←こいつで改行
					// 　　　　　⇩オーバーフロー
					// ○○●○◎○◎○○
					// 　　　　　⇧connectionStartPos ←こいつで改行
					//
					// connectionStartPosをposで上書きする条件は「前回が●でない、かつ、現在が◎でない」
					// 「●と次」または「◎と手前」はつながった（行をまたぐことができない）範囲という意味を込めてconnection～という名前にしている

					if(!isPrevLastForbidden && !HeadForbiddenChars.Contains(c)) {
						connectionStartPos = pos;
						connectionStartWidth = sectionWidth;
					}

					bool needWrap = TextWrapping != TextWrapping.NoWrap && (lineWidth + sectionWidth + width) > AvailableWidth;
					if(needWrap && LastHangingChars.Contains(c))
						needWrap = false;   // ぶらさげ文字だったらやっぱり改行は無しにする

					// 区間の切り替わり目か確認
					if(currentVertical != null && (currentVertical != isvertical || needWrap)) {
						if(startPos < connectionStartPos) { // スタート地点以降につながっている部分がある
							NewSectionCallback?.Invoke(startPos, connectionStartPos, connectionStartWidth);
							lineWidth += connectionStartWidth;
							sectionWidth -= connectionStartWidth;
							startPos = connectionStartPos;
						} else {	// 区間の開始地点以降に改行できるところが無かったので、禁則処理を諦める
							NewSectionCallback?.Invoke(startPos, pos, sectionWidth);
							lineWidth += sectionWidth;
							sectionWidth = 0;
							startPos = pos;
						}

						if(needWrap) {
							NewLineCallback?.Invoke(lineWidth);
							lineWidth = 0;
						}
					}
					if(currentVertical == null && isvertical != null)   // 無効な文字以降初めての有効な文字ならスタート地点とする
						startPos = pos;

					currentVertical = isvertical;
					sectionWidth += width;

					isPrevLastForbidden = LastForbiddenChars.Contains(c);
				}
			}

			public void GetRemaining(int pos)
			{
				if(currentVertical != null && startPos < pos) {
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
