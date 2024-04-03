using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tategaki.Logic;
using static System.Net.Mime.MediaTypeNames;

namespace Tategaki
{
	public class TategakiText : FrameworkElement
	{
		readonly VisualCollection children;
		DrawingVisual? drawingText = null;
		DrawingVisual? drawingBackground = null;
		Size textSize;
		Size availableSize;
		GlyphRun? lastGlyph = null;
		Rect? lastBgRect = null;

		public TategakiText()
		{
			children = new VisualCollection(this);
		}

		#region Properties

		/// <summary>
		/// このコントロールで使えるフォント名
		/// </summary>
		public static string[] AvailableFonts
		{
			get { return Util.AvailableFonts; }
		}

		/// <summary>
		/// 表示テキスト
		/// </summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text), typeof(string), typeof(TategakiText), new PropertyMetadata((d, e) => {
				TategakiText me = (TategakiText)d;
				me.Redraw(true);
			})
		);

		/// <summary>
		/// フォントファミリー
		/// </summary>
		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}
		public static readonly DependencyProperty FontFamilyProperty =　DependencyProperty.Register(
			nameof(FontFamily), typeof(FontFamily), typeof(TategakiText), new PropertyMetadata(new FontFamily("游ゴシック")/*SystemFonts.MessageFontFamily*/, (d, e) => {
				TategakiText me = (TategakiText)d;

				var font = e.NewValue as FontFamily;
				if(font == null)	// nullだったらデフォルトのフォント
					me.FontFamily = SystemFonts.MessageFontFamily;
				else if(!Util.AvailableFontUris.Contains(font.BaseUri ?? Util.GetFontUri(font.FamilyNames.First().Value)))	// 存在しないフォントだったら存在する最初のフォント
					me.FontFamily = new FontFamily(Util.AvailableFonts.First());
				else
					me.Redraw(true);
			})
		);

		/// <summary>
		/// フォントサイズ
		/// </summary>
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}
		public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
			nameof(FontSize), typeof(double), typeof(TategakiText), new PropertyMetadata(SystemFonts.MessageFontSize, (d, e) => {
				TategakiText me = (TategakiText)d;

				if(e.NewValue is double size && size <= 0)    // 0以下だったらデフォルトのフォントサイズ
					me.FontSize =  SystemFonts.MessageFontSize;
				else
					me.Redraw(true);
			})
		);

		/// <summary>
		/// フォントの太さ
		/// </summary>
		public FontWeight FontWeight
		{
			get { return (FontWeight)GetValue(FontWeightProperty); }
			set { SetValue(FontWeightProperty, value); }
		}
		public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
			nameof(FontWeight), typeof(FontWeight), typeof(TategakiText), new PropertyMetadata(SystemFonts.MessageFontWeight, (d, e) => {
				TategakiText me = (TategakiText)d;
				me.Redraw(true);
			})
		);

		/// <summary>
		/// フォントスタイル
		/// </summary>
		public FontStyle FontStyle
		{
			get { return (FontStyle)GetValue(FontStyleProperty); }
			set { SetValue(FontStyleProperty, value); }
		}
		public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
			nameof(FontStyle), typeof(FontStyle), typeof(TategakiText), new PropertyMetadata(SystemFonts.MessageFontStyle, (d, e) => {
				TategakiText me = (TategakiText)d;
				me.Redraw(true);
			})
		);

		/// <summary>
		/// 文字間隔
		/// </summary>
		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
			nameof(Spacing), typeof(double), typeof(TategakiText), new PropertyMetadata((double)100, (d, e) => {
				TategakiText me = (TategakiText)d;
				me.Redraw(true);
			})
		);

		/// <summary>
		/// 文字の色
		/// </summary>
		public Brush Foreground
		{
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}
		public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
			nameof(Foreground), typeof(Brush), typeof(TategakiText), new PropertyMetadata(SystemColors.ControlTextBrush, (d, e) => {
				TategakiText me = (TategakiText)d;
				me.Redraw(false);
			})
		);

		/// <summary>
		/// 背景色
		/// </summary>
		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}
		public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
			nameof(Background), typeof(Brush), typeof(TategakiText), new PropertyMetadata(Brushes.Transparent, (d, e) => {
				TategakiText me = (TategakiText)d;
				me.Redraw(false);
			})
		);

		#endregion

		#region Overrides

		protected override Size MeasureOverride(Size availableSize)
		{
			this.availableSize = availableSize;

			if(!lastBgRect.HasValue || lastBgRect.Value.Width != availableSize.Width || lastBgRect.Value.Height != availableSize.Height) {
				RedrawBackground();
				UpdateLayout();
			}

			return new Size(
				double.IsPositiveInfinity(availableSize.Width) ? textSize.Width : availableSize.Width,
				double.IsPositiveInfinity(availableSize.Height) ? textSize.Height : availableSize.Height);
		}

		protected override int VisualChildrenCount => children.Count;

		protected override Visual GetVisualChild(int index)
		{
			if(index < 0 || index >= children.Count)
				throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is out of range");

			return children[index];
		}

		#endregion

		#region Methods

		void Redraw(bool glyphChanged)
		{
			RedrawBackground();
			RedrawText(glyphChanged);

			UpdateLayout();
			if(glyphChanged)
				InvalidateMeasure();			
		}

		void RedrawBackground()
		{
			if(drawingBackground != null) {
				children.Remove(drawingBackground);
				drawingBackground = null;
			}

			var drawing = new DrawingVisual();
			DrawingContext context = drawing.RenderOpen();

			var rectangle = new Rect(0, 0, double.IsPositiveInfinity(availableSize.Width) ? textSize.Width : availableSize.Width, double.IsPositiveInfinity(availableSize.Height) ? textSize.Height : availableSize.Height);
			context.DrawRectangle(Background, null, rectangle);

			context.Close();

			lastBgRect = rectangle;

			drawingBackground = drawing;
			children.Insert(0, drawingBackground);
		}


		void RedrawText(bool glyphChanged)
		{
			if(drawingText != null) {
				children.Remove(drawingText);
				drawingText = null;
			}

			if(!string.IsNullOrEmpty(Text)) {
				var drawing = new DrawingVisual();
				DrawingContext context = drawing.RenderOpen();

				if(glyphChanged) {
					var language = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);
					var fontname = FontFamily.FamilyNames[XmlLanguage.GetLanguage("en-us")];
					var face = new GlyphTypeface(Util.GetFontUri(fontname), ((FontWeight == FontWeights.Normal) ? StyleSimulations.None : StyleSimulations.BoldSimulation) | ((FontStyle == FontStyles.Normal) ? StyleSimulations.None : StyleSimulations.ItalicSimulation));
					var renderingEmSize = FontSize;
					var spacing = Spacing;
					var origin = new Point(0, 0);
					var advanceWidth = Enumerable.Repeat<double>(renderingEmSize, Text.Length).ToArray();
					var glyphOffset = Enumerable.Range(0, Text.Length).Select(p => new Point((double)p * (spacing - 100) / 100 * renderingEmSize, 0)).ToArray();
					var text = Text;
					var glyphIndices = Util.GetVerticalGlyphIndex(Text, fontname);

					// 文字列の描画サイズを確認する
					var glyphrun = new GlyphRun(face, 0, true, renderingEmSize, 1, glyphIndices, origin, advanceWidth, glyphOffset, text.ToArray(), fontname, null, null, language);
					var glyphbox = glyphrun.ComputeAlignmentBox();

					// 文字の高さ分右に動かして、かつ左上を原点とする
					origin.Offset(glyphbox.Height, -glyphbox.Y);
					glyphrun = new GlyphRun(face, 0, true, renderingEmSize, 1, glyphIndices, origin, advanceWidth, glyphOffset, text.ToArray(), fontname, null, null, language);

					lastGlyph = glyphrun;
					textSize = new Size(glyphbox.Height, glyphbox.Width + text.Length * (double)(spacing - 100) / 100 * renderingEmSize);   // 回転後のサイズ
				}
				context.DrawGlyphRun(Foreground, lastGlyph);
				context.Close();

				drawing.Clip = new RectangleGeometry(new Rect(textSize.Width, 0, textSize.Height, textSize.Width)); // 回転前の寸法でクリッピングする
				drawing.Transform = new RotateTransform(90, textSize.Width, 0); // 起点は文字幅分右に行ったところ

				children.Add(drawing);

				drawingText = drawing;
			} else {
				lastGlyph = null;
				textSize = new Size();
			}
		}

		#endregion

	}
}
