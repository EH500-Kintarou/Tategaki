using System.Windows;
using System.Windows.Controls;

namespace Tategaki
{
	[Obsolete]
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

		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			itemsctl = (ItemsControl)this.Template.FindName("PART_ItemsControl", this);

			SetText();
		}

		void SetText()
		{
			if(itemsctl != null) {
				var text = Text ?? string.Empty;

				var paragraph = new List<string>();
				var ret = new List<List<string>>() { paragraph };

				int prevStart = -1;
				bool prevlf = false;	// 前が末尾禁則文字

				for(int i = 0; i < text.Length; i++) {
					var c = text[i];

					if(c == '\r') {
						if(prevStart >= 0)
							paragraph.Add(text.Substring(prevStart, i - prevStart));
						prevStart = -1;
						prevlf = false;
					} else if(c == '\n') {
						if(prevStart >= 0)
							paragraph.Add(text.Substring(prevStart, i - prevStart));

						if(paragraph.Count == 0)
							paragraph.Add(string.Empty);
						paragraph = new List<string>();
						ret.Add(paragraph);

						prevStart = -1;
						prevlf = false;
					} else if(prevStart >= 0 && HeadForbiddenChars.Contains(c)) {
						prevlf = false;
					} else if(prevStart >= 0 && LastForbiddenChars.Contains(c)) {
						if(!prevlf) {
							paragraph.Add(text.Substring(prevStart, i - prevStart));
							prevStart = i;
						}
						prevlf = true;
					} else {
						if(!prevlf) {
							if(prevStart >= 0)
								paragraph.Add(text.Substring(prevStart, i - prevStart));
							prevStart = i;
						}						
						prevlf = false;
					}
				}
				if(prevStart >= 0 && prevStart < text.Length)
					paragraph.Add(text.Substring(prevStart, text.Length - prevStart));

				itemsctl.ItemsSource = ret;
			}
		}
	}
}
