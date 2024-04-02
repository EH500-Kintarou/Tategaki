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
	public class TategakiMultiline : Control
	{
		ItemsControl? itemsctl;


		static TategakiMultiline()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TategakiMultiline), new FrameworkPropertyMetadata(typeof(TategakiMultiline)));
		}

		#region Properties

		/// <summary>
		/// 表示テキスト
		/// </summary>
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text), typeof(string), typeof(TategakiMultiline), new PropertyMetadata((d, e) => {
				TategakiMultiline me = (TategakiMultiline)d;
				me.SetText();
			})
		);
		/*
		/// <summary>
		/// 文字間隔
		/// </summary>
		public double Spacing
		{
			get { return (double)GetValue(SpacingProperty); }
			set { SetValue(SpacingProperty, value); }
		}
		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
			"Spacing", typeof(double), typeof(LightResizingTategakiMultiline), new PropertyMetadata((double)100, (d, e) => {
				LightResizingTategakiMultiline me = (LightResizingTategakiMultiline)d;
				me.SetText();
			})
		);
		*/
		/// <summary>
		/// 行間隔
		/// </summary>
		public double LineMargin
		{
			get { return (double)GetValue(LineMarginProperty); }
			set { SetValue(LineMarginProperty, value); }
		}
		public static readonly DependencyProperty LineMarginProperty = DependencyProperty.Register(
			nameof(LineMargin), typeof(double), typeof(TategakiMultiline), new PropertyMetadata((d, e) => {
				TategakiMultiline me = (TategakiMultiline)d;
				me.SetText();
			})
		);

		/// <summary>
		/// 文末に禁止された文字
		/// </summary>
		public string LastForbiddenChars
		{
			get { return (string)GetValue(LastForbiddenCharsProperty); }
			set { SetValue(LastForbiddenCharsProperty, value); }
		}
		public static readonly DependencyProperty LastForbiddenCharsProperty = DependencyProperty.Register(
			nameof(LastForbiddenChars), typeof(string), typeof(TategakiMultiline), new PropertyMetadata("（［｛「『([{｢", (d, e) => {
				TategakiMultiline me = (TategakiMultiline)d;
				me.SetText();
			})
		);

		/// <summary>
		/// 文頭に禁止された文字
		/// </summary>
		public string HeadForbiddenChars
		{
			get { return (string)GetValue(HeadForbiddenCharsProperty); }
			set { SetValue(HeadForbiddenCharsProperty, value); }
		}
		public static readonly DependencyProperty HeadForbiddenCharsProperty = DependencyProperty.Register(
			nameof(HeadForbiddenChars), typeof(string), typeof(TategakiMultiline), new PropertyMetadata("、。，．・？！゛゜ヽヾゝゞ々ー）］｝」』!),.:;?]}｡｣､･ｰﾞﾟァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇷ゚ㇺㇻㇼㇽㇾㇿ々〻", (d, e) => {
				TategakiMultiline me = (TategakiMultiline)d;
				me.SetText();
			})
		);
		/*
		/// <summary>
		/// 文末にぶら下げる文字
		/// </summary>
		public string LastHangingChars
		{
			get { return (string)GetValue(LastHangingCharsProperty); }
			set { SetValue(LastHangingCharsProperty, value); }
		}
		public static readonly DependencyProperty LastHangingCharsProperty = DependencyProperty.Register(
			"LastHangingChars", typeof(string), typeof(LightResizingTategakiMultiline), new PropertyMetadata("、。，．,.｡､", (d, e) => {
				LightResizingTategakiMultiline me = (LightResizingTategakiMultiline)d;
				me.SetText();
			})
		);
		*/
		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			itemsctl = (ItemsControl)this.Template.FindName("PART_ItemsControl", this);

			SetText();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			switch(e.Property.Name) {
				case nameof(this.FontFamily):
					Util.MakeCache(Text, FontFamily.Source);
					break;
			}
			base.OnPropertyChanged(e);
		}

		void SetText()
		{
			if(itemsctl != null) {
				Util.MakeCache(Text, FontFamily.Source);

				IEnumerable<string> splited = (Text ?? string.Empty).Split('\n').Select(p => string.IsNullOrEmpty(p) ? " " : p);

				itemsctl.ItemsSource = splited.Select(p => {
					if(p.Length == 0)
						return new string[] { };
					else if(p.Length == 1)
						return new string[] { p.First().ToString() };
					else {
						List<string> ret = new List<string>(p.ToCharArray().Select(p1 => p1.ToString()));

						for(int i = 0; i < ret.Count; i++) {
							if(i > 0 && HeadForbiddenChars.Contains(ret[i].First())) {
								ret[i - 1] = ret[i - 1] + ret[i];
								ret.RemoveAt(i);
								i -= 2;
								continue;
							}
							if(i < ret.Count - 1 && LastForbiddenChars.Contains(ret[i].Last())) {
								ret[i] = ret[i] + ret[i + 1];
								ret.RemoveAt(i + 1);
								i -= 1;
								continue;
							}
						}

						return ret.AsReadOnly().AsEnumerable();
					}
				}).ToArray();
			}
		}
	}
}
