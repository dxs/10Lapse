using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;

namespace _10Lapse.Logic
{
	public class VideoQuality : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public MediaEncodingProfile profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
		private int selectedItem = 0;
		public int SelectedItem
		{
			get { return selectedItem; }
			set { selectedItem = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem))); }
		}


		public string[] QualityList = new string[] { "Auto", "HD 1080p", "HD 720p", "UHD 2160p", "UHD 4320p" };

		public void ConvertToEncodingQuality()
		{
			switch(SelectedItem)
			{
				case 0:
				case 1:
					profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
					break;
				case 2:
					profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
					break;
				case 3:
					profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Uhd2160p);
					break;
				case 4:
					profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Uhd4320p);
					break;
				default:
					profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
					break;
			}
		}

		public VideoQuality()
		{

		}
	}
}
