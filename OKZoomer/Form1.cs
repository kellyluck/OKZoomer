using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OKZoomer
{
    public partial class Form1 : Form
    {
        string imagesSourcePath = @"F:\AIArt\output\img2img-images\2024-04-09\test";
        string destinationPath = @"F:\AIArt\output\img2img-images\2024-04-09\test\out";
        List<String> sourceImages;
        int pictureCount = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = imagesSourcePath;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                imagesSourcePath = folderBrowserDialog1.SelectedPath;
                LoadImages(imagesSourcePath);
            }
        }

        private void LoadImages(string imagePath)
        {
            sourceImages = new List<string>();
            lvImages.Items.Clear();
            string[] fileEntries = Directory.GetFiles(imagePath);
            foreach (string fileName in fileEntries)
            {
                sourceImages.Add(fileName);
                lvImages.Items.Add(fileName);
            }

        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            string img1, img2;

            pictureCount = 0;
            for (int i = 0; i < lvImages.Items.Count - 1; i++)
            {
                img1 = lvImages.Items[i].Text;
                img2 = lvImages.Items[i + 1].Text;
                lblProgress.Text = "Merging " + img1 + "...";
                lblProgress.Refresh();
                Debug.WriteLine($"Tweening {img1} and {img2}...");
                //MergeImages(img1, img2);
                TweenImages(img1, img2);
                pic.Refresh();
            }
            lblProgress.Text = "Done!";
        }

        private void TweenImages(string image1, string image2)
        {
            FileStream fs1 = new System.IO.FileStream(image1, FileMode.Open, FileAccess.Read);
            Image img1 = Image.FromStream(fs1);
            Image img0 = (Image)img1.Clone();
            pic.Image = img0;

            FileStream fs2 = new System.IO.FileStream(image2, FileMode.Open, FileAccess.Read);
            Image img2 = Image.FromStream(fs2);

            int frames = (int)numTweenFrames.Value;
            decimal multiplier = (decimal)(1.0 / frames);
            int origWidth = img1.Width;
            int origHeight = img1.Height;
            int outerScaleX, outerScaleY, innerScaleX, innerScaleY;
            int outerPosX, outerPosY, innerPosX, innerPosY;
            double innerOpacity;
            ImageAttributes opaque = new ImageAttributes();
            ImageAttributes translucent = new ImageAttributes();
            ColorMatrix opMatrix = new ColorMatrix();
            ColorMatrix trMatrix = new ColorMatrix();

            opMatrix.Matrix33 = 1; // transparency
            opaque.SetColorMatrix(opMatrix);

            for (int i = 0; i < frames; i++)
            {
                outerScaleX = (int)((origWidth * 2) - (origWidth * multiplier * i));
                outerScaleY = (int)((origHeight * 2) - (origHeight * multiplier * i));
                innerScaleX = (int)(origWidth - ((origWidth / 2) * multiplier * i));
                innerScaleY = (int)(origHeight - ((origHeight / 2) * multiplier * i));
                innerOpacity = (double)(1 - (multiplier * i));
                outerPosX = (origWidth - outerScaleX) / 2;
                outerPosY = (origHeight - outerScaleY) / 2; 
                innerPosX = (origWidth - innerScaleX) / 2;
                innerPosY = (origHeight - innerScaleY) / 2;

                trMatrix.Matrix33 = (float)innerOpacity; // transparency
                translucent.SetColorMatrix(trMatrix);

                Debug.WriteLine($"Frame {pictureCount}: Outer image is {outerScaleX} x {outerScaleY} at ({outerPosX},{outerPosY})");
                Debug.WriteLine($"...Inner image is {innerScaleX} x {innerScaleY} at ({innerPosX},{innerPosY})");

                Graphics grMerge = Graphics.FromImage(pic.Image);
                grMerge.DrawImage(img2, new Rectangle(outerPosX,outerPosY,outerScaleX,outerScaleY), 0, 0, origWidth, origHeight, GraphicsUnit.Pixel,opaque);
                // actually, never mind blending the inner image. I think the outer works just fine.
                //grMerge.DrawImage(img1, new Rectangle(innerPosX, innerPosY, innerScaleX, innerScaleY), 0, 0, origWidth, origHeight, GraphicsUnit.Pixel,translucent);

                pic.Invalidate();
                pic.Image.Save(destinationPath + "/" + txtPrefix.Text + "-" + frameNum(pictureCount) + ".jpg",ImageFormat.Jpeg);
                pictureCount++;
            }
            fs1.Close();
            fs2.Close();

        }

        private string frameNum(int frame)
        {
            string zeros = "000000";
            return frame.ToString(zeros);
        }

        //private void MergeImages(string image1, string image2)
        //{
        //    FileStream fs = new System.IO.FileStream(image1, FileMode.Open, FileAccess.Read);
        //    pic.Image = Image.FromStream(fs);
        //    fs.Close();

        //    fs = new System.IO.FileStream(image2, FileMode.Open, FileAccess.Read);
        //    Image img2 = Image.FromStream(fs);
        //    fs.Close();

        //    ImageAttributes attributes = new ImageAttributes();
        //    ColorMatrix matrix = new ColorMatrix();
        //    matrix.Matrix33 = 0.5f; // transparency
        //    attributes.SetColorMatrix(matrix);
        //    pic.Invalidate();

        //    Graphics grMerge = Graphics.FromImage(pic.Image);
        //    grMerge.DrawImage(img2,
        //        new Rectangle(0, 0, img2.Width, img2.Height),
        //        0, 0, img2.Width, img2.Height,
        //        GraphicsUnit.Pixel, attributes);

        //    pic.Image.Save(image1.Replace(".jpg", "a.jpg"));
        //}
    }
}
