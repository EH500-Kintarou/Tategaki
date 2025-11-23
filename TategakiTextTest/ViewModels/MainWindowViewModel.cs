using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace TategakiTextTest.ViewModels
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		DispatcherTimer timer = new DispatcherTimer();

		public MainWindowViewModel()
		{
			timer.Tick += Timer_Tick;
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Start();
		}

		private void Timer_Tick(object? sender, EventArgs e)
		{
			ChangingText = ChangingText switch {
				"変化する" => "文字列",
				_ => "変化する",
			};
			ChangingTextAlignment = ChangingTextAlignment switch {
				TextAlignment.Left => TextAlignment.Center,
				TextAlignment.Center => TextAlignment.Right,
				TextAlignment.Right => TextAlignment.Justify,
				_ => TextAlignment.Left,
			};

			ChangingHorizontal = ChangingHorizontal switch {
				HorizontalAlignment.Left => HorizontalAlignment.Center,
				HorizontalAlignment.Center => HorizontalAlignment.Right,
				HorizontalAlignment.Right => HorizontalAlignment.Stretch,
				_ => HorizontalAlignment.Left,
			};
			ChangingVertical = ChangingVertical switch {
				VerticalAlignment.Top => VerticalAlignment.Center,
				VerticalAlignment.Center => VerticalAlignment.Bottom,
				VerticalAlignment.Bottom => VerticalAlignment.Stretch,
				_ => VerticalAlignment.Top,
			};

			if(ChangingWeight == FontWeights.Normal && ChangingStyle == FontStyles.Normal) {
				ChangingWeight = FontWeights.Bold;
				ChangingStyle = FontStyles.Normal;
			} else if(ChangingWeight != FontWeights.Normal && ChangingStyle == FontStyles.Normal) {
				ChangingWeight = FontWeights.Bold;
				ChangingStyle = FontStyles.Italic;
			} else if(ChangingWeight != FontWeights.Normal && ChangingStyle != FontStyles.Normal) {
				ChangingWeight = FontWeights.Normal;
				ChangingStyle = FontStyles.Italic;
			} else {
				ChangingWeight = FontWeights.Normal;
				ChangingStyle = FontStyles.Normal;
			}
		}

		public string? ChangingText
		{
			get;
			set
			{
				if(field == value)
					return;
				field = value;
				RaisePropertyChanged();
			}
		}

		public TextAlignment ChangingTextAlignment
		{
			get;
			set
			{
				if(field == value)
					return;
				field = value;
				RaisePropertyChanged();
			}
		}

		public HorizontalAlignment ChangingHorizontal
		{
			get;
			set
			{ 
				if(field == value)
					return;
				field = value;
				RaisePropertyChanged();
			}
		}

		public VerticalAlignment ChangingVertical
		{
			get;
			set
			{
				if(field == value)
					return;
				field = value;
				RaisePropertyChanged();
			}
		}

		public FontWeight ChangingWeight
		{
			get;
			set
			{
				if(field == value)
					return;
				field = value;
				RaisePropertyChanged();
			}
		}

		public FontStyle ChangingStyle
		{
			get;
			set
			{
				if(field == value)
					return;
				field = value;
				RaisePropertyChanged();
			}
		}

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
