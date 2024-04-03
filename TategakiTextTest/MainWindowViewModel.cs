using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TategakiTextTest
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

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
