using _10Lapse.Logic;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace _10Lapse
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public Project Project;

		public MainPage()
		{
			this.InitializeComponent();

			Project = new Project(this);
			Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
			Object o = localSettings.Values["version"];
			if (o == null || Int32.Parse(o.ToString()) < INFO.Version)
			{
				//First boot
				localSettings.Values["version"] = INFO.Version;
				popupAd.Visibility = Visibility.Visible;
			}
			
		}

		private async void ButtonChooseFile_Click(object sender, RoutedEventArgs e)
		{
			Project.mediaPlayerElement = mediaPlayerElement;
			await Project.PickFileAndAddClip();
		}

		private async void ConvertButton_Click(object sender, RoutedEventArgs e)
		{
			await Project.RenderCompositionToFile();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Project.CancelRender();
		}

		private void ClosePopupClick(object sender, RoutedEventArgs e)
		{
			popupAd.Visibility = Visibility.Collapsed;
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Project.Clear();
		}
	}
}
