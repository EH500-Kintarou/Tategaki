using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TategakiTextTest.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DocCtrl docctrl = new DocCtrl();

			var doc = new FixedDocument();
			var page = new FixedPage() { Height = 1122.52, Width = 793.7 };
			var pageContent = new PageContent();
			page.Children.Add(docctrl);
			pageContent.Child = page;
			doc.Pages.Add(pageContent);

			documentViewer.Document = doc;
		}
	}
}