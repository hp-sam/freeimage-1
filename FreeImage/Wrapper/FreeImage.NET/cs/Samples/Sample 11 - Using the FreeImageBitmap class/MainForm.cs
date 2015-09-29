using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FreeImageAPI;
using FreeImageAPI.Metadata;
using FreeImageAPI.Plugins;

namespace Sample11
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		[STAThread]
		static void Main()
		{
			// Capture messages generated by FreeImage
			FreeImageEngine.Message += new OutputMessageFunction(FreeImageEngine_Message);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		static void FreeImageEngine_Message(FREE_IMAGE_FORMAT fif, string message)
		{
			// Display the message
			// FreeImage continues code executing when all
			// addes subscribers of 'Message' finished returned.
			MessageBox.Show(message, "FreeImage-Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		// The FreeImageBitmap this sample will work with.
		FreeImageBitmap bitmap = null;

		// Replaces the current bitmap with the given one.
		private void ReplaceBitmap(FreeImageBitmap newBitmap)
		{
			// Checks whether the bitmap is usable
			if (newBitmap == null || newBitmap.IsDisposed)
			{
				MessageBox.Show(
					"Unexpected error.",
					"Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}

			// Check whether the image type of the new bitmap is 'FIT_BITMAP'.
			// If not convert to 'FIT_BITMAP'.
			if (newBitmap.ImageType != FREE_IMAGE_TYPE.FIT_BITMAP)
			{
				if (!newBitmap.ConvertType(FREE_IMAGE_TYPE.FIT_BITMAP, true))
				{
					MessageBox.Show(
						"Error converting bitmap to standard type.",
						"Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error);
					return;
				}
			}

			// Dispose the old bitmap only in case it exists and
			// the old instance is another than the new one.
			if ((bitmap != null) && !object.ReferenceEquals(bitmap, newBitmap))
			{
				bitmap.Dispose();
			}
			// Dispose the picturebox's bitmap in case it exists.
			if (pictureBox.Image != null)
			{
				pictureBox.Image.Dispose();
			}

			// Set the new bitmap.
			pictureBox.Image = (Bitmap)(bitmap = newBitmap);

			// Update gui.
			UpdateBitmapInformations();
			UpdateFrameSelection();
		}

		// Get bitmap properties and display them in the gui.
		private void UpdateBitmapInformations()
		{
			if (Bitmap)
			{
				// Get width
				lWidth.Text = String.Format("Width: {0}", bitmap.Width);
				// Get Height
				lHeight.Text = String.Format("Height: {0}", bitmap.Height);
				// Get color depth
				lBpp.Text = String.Format("Bpp: {0}", bitmap.ColorDepth);
				// Get number of metadata
				ImageMetadata mData = bitmap.Metadata;
				mData.HideEmptyModels = true;
				int mCnt = 0;
				foreach (MetadataModel model in mData.List)
				{
					mCnt += model.Count;
				}
				lMetadataCount.Text = String.Format("Metadata: {0}", mCnt);
				// Get image comment
				lComment.Text = String.Format("Image-comment: {0}", bitmap.Comment != null ? bitmap.Comment : String.Empty);
				// Get the number of real colors in the image
				lColors.Text = String.Format("Colors: {0}", bitmap.UniqueColors);
			}
			else
			{
				// Reset all values
				lWidth.Text = String.Format("Width: {0}", 0);
				lHeight.Text = String.Format("Height: {0}", 0);
				lBpp.Text = String.Format("Bpp: {0}", 0);
				lMetadataCount.Text = String.Format("Metadata: {0}", 0);
				lComment.Text = String.Format("Image-comment: {0}", String.Empty);
				lColors.Text = String.Format("Colors: {0}", 0);
			}
		}

		// Update combobox for frame selection.
		private void UpdateFrameSelection()
		{
			cbSelectFrame.Items.Clear();
			if (Bitmap)
			{
				// Get number of frames in the bitmap
				if (bitmap.FrameCount > 1)
				{
					// Add an entry for each frame to the combobox
					for (int i = 0; i < bitmap.FrameCount; i++)
					{
						cbSelectFrame.Items.Add(String.Format("Frame {0}", i + 1));
					}
				}
			}
		}

		// Returns true in case the variable 'bitmap'
		// is set and not disposed.
		private bool Bitmap
		{
			get { return ((bitmap != null) && (!bitmap.IsDisposed)); }
		}

		private void bLoadImage_Click(object sender, EventArgs e)
		{
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				try
				{
					// Load the file using autodetection
					FreeImageBitmap fib = new FreeImageBitmap(ofd.FileName);
					// Rescale the image so that it fits the picturebox
					// Get the plugin that was used to load the bitmap
					FreeImagePlugin plug = PluginRepository.Plugin(fib.ImageFormat);
					lImageFormat.Text = String.Format("Image-format: {0}", plug.Format);
					// Replace the existing bitmap with the new one
					ReplaceBitmap(fib);
				}
				catch
				{
				}
			}
		}

		private void bSaveImage_Click(object sender, EventArgs e)
		{
			if (pictureBox.Image != null)
			{
				try
				{
					if (sfd.ShowDialog() == DialogResult.OK)
					{
						// Save the bitmap using autodetection
						using (FreeImageBitmap temp = new FreeImageBitmap(pictureBox.Image))
						{
							temp.Save(sfd.FileName);
						}
					}
				}
				catch
				{
				}
			}
		}

		private void bRotate_Click(object sender, EventArgs e)
		{
			if (Bitmap)
			{
				// Create a temporary rescaled bitmap
				using (FreeImageBitmap temp = bitmap.GetScaledInstance(
					pictureBox.DisplayRectangle.Width, pictureBox.DisplayRectangle.Height,
					FREE_IMAGE_FILTER.FILTER_CATMULLROM))
				{
					if (temp != null)
					{
						// Rotate the bitmap
						temp.Rotate((double)vRotate.Value);
						if (pictureBox.Image != null)
						{
							pictureBox.Image.Dispose();
						}
						// Display the result
						pictureBox.Image = (Bitmap)temp;
					}
				}
			}
		}

		private void bGreyscale_Click(object sender, EventArgs e)
		{
			if (Bitmap)
			{
				// Convert the bitmap to 8bpp and greyscale
				ReplaceBitmap(bitmap.GetColorConvertedInstance(
					FREE_IMAGE_COLOR_DEPTH.FICD_08_BPP |
					FREE_IMAGE_COLOR_DEPTH.FICD_FORCE_GREYSCALE));
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cb = sender as ComboBox;
			if ((cb != null) && (cb.Items.Count > 0))
			{
				if (Bitmap)
				{
					try
					{
						// Switch the selected frame
						bitmap.SelectActiveFrame(cb.SelectedIndex);
						ReplaceBitmap(bitmap);
					}
					catch (ArgumentOutOfRangeException)
					{
						MessageBox.Show("Error changing frame.", "Error");
					}
				}
			}
		}

		private void bAdjustGamma_Click(object sender, EventArgs e)
		{
			if (Bitmap)
			{
				// Adjust the gamma value
				bitmap.AdjustGamma((double)vGamma.Value);
				ReplaceBitmap(bitmap);
			}
		}

		private void bRedChannelOnly_Click(object sender, EventArgs e)
		{
			// Mask out green and blue
			SetColorChannels(0xFF, 0x00, 0x00);
		}

		private void bGreenChannel_Click(object sender, EventArgs e)
		{
			// Mask out red and blue
			SetColorChannels(0x00, 0xFF, 0x00);
		}

		private void bBlueChannel_Click(object sender, EventArgs e)
		{
			// Mask out red and green
			SetColorChannels(0x00, 0x00, 0xFF);
		}

		private void bAllChannels_Click(object sender, EventArgs e)
		{
			if (Bitmap)
			{
				// Restore the bitmap using the original
				ReplaceBitmap(bitmap);
			}
		}

		private void SetColorChannels(int redmask, int greenmask, int bluemask)
		{
			if (Bitmap)
			{
				// Create a temporary clone.
				using (FreeImageBitmap bitmap = (FreeImageBitmap)this.bitmap.Clone())
				{
					if (bitmap != null)
					{
						// Check whether the bitmap has a palette
						if (bitmap.HasPalette)
						{
							// Use the Palette class to handle the bitmap's
							// palette. A palette always consist of RGBQUADs.
							Palette palette = bitmap.Palette;
							// Apply the new values for all three color components.
							for (int i = 0; i < palette.Length; i++)
							{
								RGBQUAD rgbq = palette[i];

								rgbq.rgbRed = (byte)(rgbq.rgbRed & redmask);
								rgbq.rgbGreen = (byte)(rgbq.rgbGreen & greenmask);
								rgbq.rgbBlue = (byte)(rgbq.rgbBlue & bluemask);

								palette[i] = rgbq;
							}
						}
						// In case the bitmap has no palette it must have a color depth
						// of 16, 24 or 32. Each color depth needs a different wrapping
						// structure for the bitmaps data. These structures can be accessed
						// by using the foreach clause.
						else if (bitmap.ColorDepth == 16)
						{
							// Iterate over each scanline
							// For 16bpp use either Scanline<FI16RGB555> or Scanline<FI16RGB565>
							if (bitmap.IsRGB555)
							{
								foreach (Scanline<FI16RGB555> scanline in bitmap)
								{
									for (int x = 0; x < scanline.Length; x++)
									{
										FI16RGB555 pixel = scanline[x];
										pixel.Red = (byte)(pixel.Red & redmask);
										pixel.Green = (byte)(pixel.Green & greenmask);
										pixel.Blue = (byte)(pixel.Blue & bluemask);
										scanline[x] = pixel;
									}
								}
							}
							else if (bitmap.IsRGB565)
							{
								foreach (Scanline<FI16RGB565> scanline in bitmap)
								{
									for (int x = 0; x < scanline.Length; x++)
									{
										FI16RGB565 pixel = scanline[x];
										pixel.Red = (byte)(pixel.Red & redmask);
										pixel.Green = (byte)(pixel.Green & greenmask);
										pixel.Blue = (byte)(pixel.Blue & bluemask);
										scanline[x] = pixel;
									}
								}
							}
						}
						else if (bitmap.ColorDepth == 24)
						{
							// Iterate over each scanline
							// For 24bpp Scanline<RGBTRIPLE> must be used
							foreach (Scanline<RGBTRIPLE> scanline in bitmap)
							{
								for (int x = 0; x < scanline.Length; x++)
								{
									RGBTRIPLE pixel = scanline[x];
									pixel.rgbtRed = (byte)(pixel.rgbtRed & redmask);
									pixel.rgbtGreen = (byte)(pixel.rgbtGreen & greenmask);
									pixel.rgbtBlue = (byte)(pixel.rgbtBlue & bluemask);
									scanline[x] = pixel;
								}
							}
						}
						else if (bitmap.ColorDepth == 32)
						{
							// Iterate over each scanline
							// For 32bpp Scanline<RGBQUAD> must be used
							foreach (Scanline<RGBQUAD> scanline in bitmap)
							{
								for (int x = 0; x < scanline.Length; x++)
								{
									RGBQUAD pixel = scanline[x];
									pixel.rgbRed = (byte)(pixel.rgbRed & redmask);
									pixel.rgbGreen = (byte)(pixel.rgbGreen & greenmask);
									pixel.rgbBlue = (byte)(pixel.rgbBlue & bluemask);
									scanline[x] = pixel;
								}
							}
						}
						// Dispose only the picturebox's bitmap
						if (pictureBox.Image != null)
						{
							pictureBox.Image.Dispose();
						}
						pictureBox.Image = (Bitmap)bitmap;
					}
				}
			}
		}

		private void vRotate_Scroll(object sender, EventArgs e)
		{
			TrackBar bar = sender as TrackBar;
			if (bar != null)
			{
				lRotate.Text = bar.Value.ToString();
			}
		}

		private void nShowMetadata_Click(object sender, EventArgs e)
		{
			if (Bitmap)
			{
				MetaDataFrame mFrame = new MetaDataFrame();
				mFrame.Tag = bitmap.Metadata;
				mFrame.ShowDialog(this);
			}
		}
	}
}