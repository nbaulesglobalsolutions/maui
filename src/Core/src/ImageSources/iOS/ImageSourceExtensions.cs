using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using ImageIO;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using UIKit;

namespace Microsoft.Maui
{
	public static partial class ImageSourceExtensions
	{
		internal static UIImage? GetPlatformImage(this IFontImageSource imageSource, IFontManager fontManager, float scale)
		{
			var font = fontManager.GetFont(imageSource.Font);
			var color = (imageSource.Color ?? Colors.White).ToPlatform();
			var glyph = (NSString)imageSource.Glyph;
#pragma warning disable CS8604
			var attString = new NSAttributedString(glyph, font, color);
			var imagesize = glyph.GetSizeUsingAttributes(attString.GetUIKitAttributes(0, out _)!);
#pragma warning restore CS8604
			UIGraphics.BeginImageContextWithOptions(imagesize, false, scale);
			var ctx = new NSStringDrawingContext();

			var boundingRect = attString.GetBoundingRect(imagesize, 0, ctx);
			attString.DrawString(new CGRect(
				imagesize.Width / 2 - boundingRect.Size.Width / 2,
				imagesize.Height / 2 - boundingRect.Size.Height / 2,
				imagesize.Width,
				imagesize.Height));

			var image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
		}

		internal static UIImage? GetPlatformImage(this IFileImageSource imageSource)
		{
			var filename = imageSource.File;
			return File.Exists(filename)
						? UIImage.FromFile(filename)
						: UIImage.FromBundle(filename);
		}

		internal static CGImageSource? GetPlatformImageSource(this IFileImageSource imageSource)
		{
			ArgumentNullException.ThrowIfNull(imageSource);

			var filename = imageSource.File;
			var url = File.Exists(filename)
				? NSUrl.CreateFileUrl(filename)
				: NSUrl.CreateFileUrl(FileSystemUtils.PlatformGetFullAppPackageFilePath(filename));
			return CGImageSource.FromUrl(url);
		}

		internal static async Task<CGImageSource?> GetPlatformImageSourceAsync(this IStreamImageSource imageSource, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(imageSource);

			var stream = await imageSource.GetStreamAsync(cancellationToken).ConfigureAwait(false);
			if (stream is null)
				throw new ArgumentException("Unable to load image stream.");

			return stream.GetPlatformImageSource();
		}

		internal static CGImageSource? GetPlatformImageSource(this Stream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);

			var data = NSData.FromStream(stream);
			if (data is null)
				throw new ArgumentException("Stream contained no data.", nameof(stream));

			return data.GetPlatformImageSource();
		}

		internal static CGImageSource? GetPlatformImageSource(this NSData data)
		{
			ArgumentNullException.ThrowIfNull(data);

			return CGImageSource.FromData(data);
		}

		internal static UIImage GetPlatformImage(this CGImageSource cgImageSource)
		{
			ArgumentNullException.ThrowIfNull(cgImageSource);

			if (cgImageSource.ImageCount == 0)
				throw new InvalidOperationException("CGImageSource does not contain any images.");

			UIImage image;

			if (cgImageSource.IsAnimated())
			{
				var animated = ImageAnimationHelper.Create(cgImageSource);
				if (animated is null)
					throw new InvalidOperationException("Unable to create animation from CGImageSource.");

				image = animated;
			}
			else
			{
				using var cgimage = cgImageSource.CreateImage(0, new() { ShouldCache = false });
				if (cgimage is null)
					throw new InvalidOperationException("Unable to create CGImage from CGImageSource.");

				image = new UIImage(cgimage);
			}

			return image;
		}
	}
}
