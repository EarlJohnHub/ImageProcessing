using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;

namespace ImageProcessing
{
    public partial class Form1 : Form
    {
        private Device current_device;

        Bitmap originalImage;
        Bitmap processedImage;
        Bitmap imageA;
        Bitmap imageB;
        Bitmap resultImage;

        Timer webcamTimer = new Timer();

        enum ProcessingMode { None, Grayscale, Invert, Sepia }
        ProcessingMode mode = ProcessingMode.None;
        public Form1()
        {
            InitializeComponent();
            webcamTimer.Interval = 30;
            webcamTimer.Tick += WebcamTimer_Tick;
        }

        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            Bitmap currentFrame = GetWebcamFrame();
            if (currentFrame == null) return;

                switch (mode)
                {
                    case ProcessingMode.Grayscale:
                        webcam_grayscale(currentFrame);
                        break;

                    case ProcessingMode.Invert:
                        webcam_invert(currentFrame);
                        break;

                    case ProcessingMode.Sepia:
                        webcam_sepia(currentFrame);
                        break;
                }

                var oldImage = pictureBox2.Image;
                pictureBox2.Image = currentFrame;
                oldImage?.Dispose();
          
        }

        private void webcam_grayscale(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, bytes);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte gray = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3);
                pixels[i] = gray;      // B
                pixels[i + 1] = gray;  // G
                pixels[i + 2] = gray;  // R
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);
        }
        private void webcam_invert(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, bytes);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]);       // B
                pixels[i + 1] = (byte)(255 - pixels[i + 1]); // G
                pixels[i + 2] = (byte)(255 - pixels[i + 2]); // R
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);
        }

        private void webcam_sepia(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, bytes);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte B = pixels[i];
                byte G = pixels[i + 1];
                byte R = pixels[i + 2];

                int tr = (int)(0.393 * R + 0.769 * G + 0.189 * B);
                int tg = (int)(0.349 * R + 0.686 * G + 0.168 * B);
                int tb = (int)(0.272 * R + 0.534 * G + 0.131 * B);

                pixels[i] = (byte)Math.Min(255, tb); // B
                pixels[i + 1] = (byte)Math.Min(255, tg); // G
                pixels[i + 2] = (byte)Math.Min(255, tr); // R
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(ofd.FileName);
            }
        }

        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox2.Image = pictureBox1.Image;
            }
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
                sfd.Title = "Save Image as";
                sfd.FileName = "image";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Choose which pictureBox image to save
                    Image imgToSave = null;

                    if (pictureBox3.Image != null)
                        imgToSave = pictureBox3.Image;
                    else if (pictureBox2.Image != null)
                        imgToSave = pictureBox2.Image;

                    if (imgToSave != null)
                    {
                        // Choose format based on extension
                        var extension = System.IO.Path.GetExtension(sfd.FileName).ToLower();
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                        if (extension == ".jpg")
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (extension == ".bmp")
                            format = System.Drawing.Imaging.ImageFormat.Bmp;

                        imgToSave.Save(sfd.FileName, format);
                        MessageBox.Show("Image saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image to save!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                for (int i = 0; i < bmp.Height - 1; i++)
                {
                    for (int j = 0; j < bmp.Width - 1; j++)
                    {
                        Color pixel = bmp.GetPixel(j, i);
                        int gray = (int)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);
                        Color newColor = Color.FromArgb(gray, gray, gray);
                        bmp.SetPixel(j, i, newColor);
                    }

                }
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = bmp;

            }
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);

                for (int y = 0; y < bmp.Height - 1; y++)
                {
                    for (int x = 0; x < bmp.Width - 1; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        Color inverted = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
                        bmp.SetPixel(x, y, inverted);

                    }
                }
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = bmp;
            }
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                int[] histogram = createGrayScaleHistogram(bmp);


                Bitmap histBmp = DrawHistogram(histogram, 256, 150); // 256 wide, 150 tall
                pictureBox2.Image?.Dispose();
                pictureBox2.Image = histBmp;
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            }
        }

        public static int[] createGrayScaleHistogram(Bitmap image)
        {
            int[] histogram = new int[256];


            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int grayValue = (int)(0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                    histogram[grayValue]++;
                }
            }
            return histogram;

        }

        private Bitmap DrawHistogram(int[] histogram, int width, int height)
        {
            Bitmap histImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);

                int max = histogram.Max(); // find tallest bar
                float scale = (float)height / max;

                for (int i = 0; i < 256; i++)
                {
                    int barHeight = (int)(histogram[i] * scale);
                    g.DrawLine(Pens.Black, i * width / 256, height, i * width / 256, height - barHeight);
                }
            }

            return histImage;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixel = bmp.GetPixel(x, y);

                        int tr = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                        int tg = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                        int tb = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);

                        // Clamp to [0, 255]
                        tr = Math.Min(255, tr);
                        tg = Math.Min(255, tg);
                        tb = Math.Min(255, tb);

                        bmp.SetPixel(x, y, Color.FromArgb(tr, tg, tb));
                    }
                }

                pictureBox2.Image?.Dispose();
                pictureBox2.Image = bmp;
            }
        }

        private void load_image_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageB = new Bitmap(ofd.FileName);
                pictureBox1.Image = imageB;
            }
        }

        private void load_bg_button_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageA = new Bitmap(ofd.FileName);  
                pictureBox2.Image = imageA;
            }
        }

        private void subtract_button_Click(object sender, EventArgs e)
        {
            RemoveGreenScreen();
        }
        private void RemoveGreenScreen()
        {
            if (imageA == null || imageB == null)
            {
                MessageBox.Show("Load a foreground (with green screen) and a background image.");
                return;
            }

            Color myGreen = Color.FromArgb(0, 255, 0);
            int greyGreen = (myGreen.R + myGreen.G + myGreen.B) / 3;
            int threshold = 60;

            int width = Math.Min(imageA.Width, imageB.Width);
            int height = Math.Min(imageA.Height, imageB.Height);

            resultImage = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixel = imageB.GetPixel(x, y);
                    Color backPixel = imageA.GetPixel(x, y);

                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractValue = Math.Abs(grey - greyGreen);

                    bool isGreen = subtractValue < threshold && pixel.G > pixel.R && pixel.G > pixel.B;
                    resultImage.SetPixel(x, y, isGreen ? backPixel : pixel);
                }
            }


            pictureBox3.Image?.Dispose();
            pictureBox3.Image = resultImage;
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Dispose();
                pictureBox2.Image = null;
            }

            if (pictureBox3.Image != null)
            {
                pictureBox3.Image.Dispose();
                pictureBox3.Image = null;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void startWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            load_image_button.Visible = false;
            subtract_button.Visible = false;

            Device[] devices = DeviceManager.GetAllDevices();
            if(devices.Length == 0)
            {
                MessageBox.Show("No webcam devices found.");
                return;
            }

            current_device = devices.FirstOrDefault(d => d.Name.Contains("ManyCam")) ?? devices[0];
            current_device.ShowWindow(pictureBox1);
     
        }

        private void stopWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
           current_device?.Stop();
        }

        private Bitmap GetWebcamFrame()
        {
            try
            {
                current_device.Sendmessage();

                Image clipboardImage = Clipboard.GetImage();
                if (clipboardImage == null)
                    return null;

                Bitmap bmp32 = new Bitmap(clipboardImage.Width, clipboardImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp32))
                {
                    g.DrawImage(clipboardImage, new Rectangle(0, 0, bmp32.Width, bmp32.Height));
                }

                return bmp32;
            }
            catch
            {
                return null;
            }
        }

        private void subtractWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("There is  no Background Found.");
                return;
            }

            Bitmap newBackground = new Bitmap(pictureBox2.Image);
            Bitmap webcamFrame = GetWebcamFrame();

            if (webcamFrame == null)
            {
                MessageBox.Show("No webcam frame detected.");
                return;
            }

            int width = Math.Min(newBackground.Width, webcamFrame.Width);
            int height = Math.Min(newBackground.Height, webcamFrame.Height);

            Bitmap result = new Bitmap(newBackground);

            Color myGreen = Color.FromArgb(0, 255, 0);
            int greyGreen = (myGreen.R + myGreen.G + myGreen.B) / 3;
            int threshold = 60;


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color camPixel = webcamFrame.GetPixel(x, y);
                    Color bgPixel = newBackground.GetPixel(x, y);

                    int grey = (camPixel.R + camPixel.G + camPixel.B) / 3;
                    int subtractValue = Math.Abs(grey - greyGreen);

                    bool isGreen = subtractValue < threshold && camPixel.G > camPixel.R && camPixel.G > camPixel.B;

                    result.SetPixel(x, y, isGreen ? bgPixel : camPixel);
                }
            }

            pictureBox3.Image?.Dispose();
            pictureBox3.Image = result;


        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = ProcessingMode.Grayscale;
            webcamTimer.Start();
        }
        private void sepiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            mode = ProcessingMode.Sepia;
            webcamTimer.Start();
        }
        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mode = ProcessingMode.Invert;
            webcamTimer.Start();
        }
    }
}

