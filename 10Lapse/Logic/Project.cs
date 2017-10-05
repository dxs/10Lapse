using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace _10Lapse.Logic
{
	public class Project : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int nbFrame = 0;
		public int NbFrame
		{
			get { return nbFrame; }
			set { nbFrame = value; Duration = (double)1 / Framerate * NbFrame; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NbFrame))); }
		}

		private double framerate = 30;
		public double Framerate
		{
			get { return framerate; }
			set
			{
				framerate = value;
				if (framerate == 0)
					framerate = 1e-5;
				Duration = (double)1 / framerate * NbFrame;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Framerate)));
				ReloadMediaClip();
			}
		}

		private double duration = 1e-5;
		public double Duration
		{
			get { return duration; }
			set
			{
				duration = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
			}
		}

		private double progress = 0;
		public double Progress
		{
			get { return progress; }
			set { progress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress))); }
		}

		private bool isLoading = false;
		public bool IsLoading
		{
			get { return isLoading; }
			set { isLoading = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading))); }
		}

		private bool isRendering = false;
		public bool IsRendering
		{
			get { return isRendering; }
			set { isRendering = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRendering))); }
		}

		private VideoQuality quality = new VideoQuality();
		public VideoQuality Quality
		{
			get { return quality; }
			set { quality = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality))); }
		}

		private MediaComposition Composition;
		
		IAsyncOperationWithProgress<TranscodeFailureReason, double> saveOperation;
		public MediaStreamSource mediaStreamSource;
		public MediaPlayerElement mediaPlayerElement;

		private IReadOnlyList<StorageFile> files;
		public IReadOnlyList<StorageFile> Files
		{
			get { return files; }
			set { files = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Files))); }
		}

		private ObservableCollection<BitmapImage> images;
		public ObservableCollection<BitmapImage> Images
		{
			get { return images; }
			set { images = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Images))); }
		}

		MainPage homeLink;

		public Project(MainPage _home)
		{
			homeLink = _home;
			Composition = new MediaComposition();
		}

		private async void ReloadMediaClip()
		{
			if (Files == null)
				return;
			if (Files.Count <= 0)
			{
				NbFrame = 0;
				return;
			}
			nbFrame = Files.Count;
			Composition.Clips.Clear();
			foreach (StorageFile file in Files)
			{
				var clip = await MediaClip.CreateFromImageFileAsync(file, new TimeSpan(0, 0, 0, 0, (int)((double)1 / Framerate * 1000)));
				Composition.Clips.Add(clip);
			}
			UpdateMediaElementSource();
			IsLoading = false;
		}

		public async Task PickFileAndAddClip()
		{
			IsLoading = true;
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".JPG");
			picker.FileTypeFilter.Add(".png");
			picker.FileTypeFilter.Add(".PNG");
			Files = await picker.PickMultipleFilesAsync();
			await LoadClip();
		}

		public async Task LoadClip()
		{
			if (Files == null)
			{
				IsLoading = false;
				return;
			}
			if (Files.Count <= 0)
			{
				NbFrame = 0;
				IsLoading = false;
				return;
			}
			nbFrame = Files.Count;
			Images = new ObservableCollection<BitmapImage>();
			foreach (StorageFile file in Files)
			{
				var clip = await MediaClip.CreateFromImageFileAsync(file, new TimeSpan(0, 0, 0, 0, (int)((double)1/Framerate*1000)));
				Composition.Clips.Add(clip);
				using (StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.PicturesView, 100, ThumbnailOptions.UseCurrentScale))
				{
					BitmapImage i = new BitmapImage();
					i.SetSource(thumbnail);
					Images.Add(i);
				}
			}
			UpdateMediaElementSource();
			IsLoading = false;
		}

		public async Task RenderCompositionToFile()
		{
			var picker = new Windows.Storage.Pickers.FileSavePicker();
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
			picker.FileTypeChoices.Add("MP4 files", new List<string>() { ".mp4" });
			picker.SuggestedFileName = "Timelapse.mp4";

			StorageFile file = await picker.PickSaveFileAsync();
			if (file != null)
			{
				Quality.ConvertToEncodingQuality();
				IsRendering = true;
				saveOperation = Composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise, Quality.profile);
				saveOperation.Progress = new AsyncOperationProgressHandler<TranscodeFailureReason, double>(async (info, cprogress) =>
				{
					await homeLink.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
					{
						Progress = cprogress;
					}));
				});
				saveOperation.Completed = new AsyncOperationWithProgressCompletedHandler<TranscodeFailureReason, double>(async (info, status) =>
				{
					//COMPLETE
					await homeLink.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
					{
						IsRendering = false;
					}));

				});
			}
			else
			{
				Debug.WriteLine("User cancelled the file selection");
				IsRendering = false;
				Progress = 0;
			}
		}

		public void UpdateMediaElementSource()
		{

			mediaStreamSource = Composition.GeneratePreviewMediaStreamSource(
				(int)mediaPlayerElement.ActualWidth,
				(int)mediaPlayerElement.ActualHeight);

			mediaPlayerElement.Source = MediaSource.CreateFromMediaStreamSource(mediaStreamSource);

		}

		public void CancelRender()
		{
			saveOperation.Cancel();
			IsRendering = false;
			progress = 0;
		}

		public void Clear()
		{
			Images.Clear();	
		}
	}
}
