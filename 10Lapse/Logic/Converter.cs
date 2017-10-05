using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace _10Lapse.Logic
{
	public class ImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string FileName = value as string;
			var file = Windows.Storage.KnownFolders.PicturesLibrary.GetFileAsync(FileName).AsTask().Result;
			var stream = file.OpenReadAsync().AsTask().Result;
			var bitmapImage = new BitmapImage();
			bitmapImage.SetSource(stream);
			return bitmapImage;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class DurationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			double d = (double)value;
			if (d < 1e-3)
				d = 0;

			string tmp = (int)(d / 3600) + "h " + (int)(d % 3600 / 60) + "m " + (int)(d % 3600 % 60 / 60) + " s";
			return tmp;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return double.Parse((string)value);
		}
	}

	public class FPSConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			double d = (double)value;
			if (d < 1e-3)
				d = 0;
			return d.ToString() + " frames per seconds";
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value;
		}
	}

	public class ProgressConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (int)(double)value + "%";
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}

	public class FilesToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			IReadOnlyList<StorageFile> a = value as IReadOnlyList<StorageFile>;
			if (a == null)
				return false;
			if (a.Count > 0)
				return true;
			else
				return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
