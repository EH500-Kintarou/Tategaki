using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tategaki.Logic;

namespace Tategaki
{
	public class TategakiText : Control
	{
		Glyphs? glyph;

		static TategakiText()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TategakiText), new FrameworkPropertyMetadata(typeof(TategakiText)));
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
				me.RedrawText();
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
				me.RedrawText();
			})
		);

		#endregion

		#region Fonts

		#endregion

		#region Overrides

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			glyph = (Glyphs)this.Template.FindName("PART_Glyphs", this);

			RedrawText();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			switch(e.Property.Name) {
				case nameof(this.FontFamily):
					RedrawText();
					break;
				case nameof(this.FontWeight):
				case nameof(this.FontStyle):
					SetStyleSimulations();
					break;
				case nameof(this.Visibility):
					ChangeVisibility();
					break;
			}
			base.OnPropertyChanged(e);
		}

		#endregion

		#region Methods

		void ChangeVisibility()
		{
			if(glyph != null) {
				if(string.IsNullOrEmpty(Text))
					glyph.Visibility = Visibility.Collapsed;
				else
					glyph.Visibility = this.Visibility;
			}
		}

		void SetStyleSimulations()
		{
			if(glyph != null) {
				glyph.StyleSimulations =
					((FontWeight != FontWeights.Normal) ? StyleSimulations.BoldSimulation : StyleSimulations.None) |
					((FontStyle != FontStyles.Normal) ? StyleSimulations.ItalicSimulation : StyleSimulations.None);
			}
		}

		void RedrawText()
		{
			if(glyph != null) {
				if(!string.IsNullOrEmpty(Text)) {
					string[] fonts = new[] {	//フォントを優先度高い順に
						FontFamily.Source,
						SystemFonts.MessageFontFamily.Source,
						"ＭＳ ゴシック",
						"ＭＳ 明朝",
						AvailableFonts.First(),
					};

					string fontname = fonts.Where(p => AvailableFonts.Contains(p)).First();
					glyph.FontUri = Util.GetFontUri(fontname);
					glyph.Indices = Util.GetIndices(Text, fontname, Spacing);
					SetStyleSimulations();
					glyph.UnicodeString = Text;
				}
				ChangeVisibility(); //文字列がemptyだとGlyphsが例外を吐くのでVisibilityでごまかす
			}
		}
		#endregion

	}
}
