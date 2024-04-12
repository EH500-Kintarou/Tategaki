using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
			get { return _ChangingText; }
			set
			{
				if(_ChangingText == value)
					return;
				_ChangingText = value;
				RaisePropertyChanged();
			}
		}
		private string? _ChangingText;


		private HorizontalAlignment _ChangingHorizontal;

		public HorizontalAlignment ChangingHorizontal
		{
			get
			{ return _ChangingHorizontal; }
			set
			{ 
				if(_ChangingHorizontal == value)
					return;
				_ChangingHorizontal = value;
				RaisePropertyChanged();
			}
		}


		private VerticalAlignment _ChangingVertical;

		public VerticalAlignment ChangingVertical
		{
			get
			{ return _ChangingVertical; }
			set
			{
				if(_ChangingVertical == value)
					return;
				_ChangingVertical = value;
				RaisePropertyChanged();
			}
		}

		private FontWeight _ChangingWeight;

		public FontWeight ChangingWeight
		{
			get
			{ return _ChangingWeight; }
			set
			{
				if(_ChangingWeight == value)
					return;
				_ChangingWeight = value;
				RaisePropertyChanged();
			}
		}

		private FontStyle _ChangingStyle;

		public FontStyle ChangingStyle
		{
			get
			{ return _ChangingStyle; }
			set
			{
				if(_ChangingStyle == value)
					return;
				_ChangingStyle = value;
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
