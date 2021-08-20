using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pixelations
{
    public partial class Form1 : Form
    {
        public class ColorKernels
        {
            private List<Color> colors;
            private int nrColors = 0;
            private float r, g, b;
            private int R, G, B;
            public int size
            {
                get => nrColors;
            }
            public Color color
            {
                get
                {
                    int R, G, B;

                    R = (int)r;
                    G = (int)g;
                    B = (int)b;

                    if (R < 0)
                        R = 0;
                    else if (R > 255)
                        R = 255;

                    if (G < 0)
                        G = 0;
                    else if (G > 255)
                        G = 255;

                    if (B < 0)
                        B = 0;
                    else if (B > 255)
                        B = 255;

                    return Color.FromArgb(R, G, B);
                }
            }
            public ColorKernels(Color initial)
            {
                colors = new List<Color>();
                r = initial.R;
                g = initial.G;
                b = initial.B;
                R = G = B = 0;
                nrColors = 0;
            }
            public void addColor(Color col)
            {
                R += col.R;
                G += col.G;
                B += col.B;
                nrColors++;
            }
            public void reset()
            {
                r = 1.0f * R / nrColors;
                g = 1.0f * G / nrColors;
                b = 1.0f * B / nrColors;
                R = G = B = 0;
                nrColors = 0;
            }
        }
        Bitmap selectedImage;
        Bitmap smallVersion;
        Bitmap smallBigVersion;
        Bitmap imageToSave;
        byte[] colors;
        int depth;
        List<ColorKernels> colorNr = new List<ColorKernels>();
        Random random = new Random();
        int colorDistanceIndex = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private Color getColor(byte[] buffer, int x, int y, int width, int height, int depth)
        {
            int offset = (y * width + x) * depth;
            if (offset >= buffer.Length - 2)
                offset = buffer.Length - 3;
            Color col = Color.FromArgb(buffer[offset + 2], buffer[offset + 1], buffer[offset]);

            return col;
        }

        private void setColor(byte[] buffer, int x, int y, int width, int height, int depth, Color newColor)
        {
            int offset = (y * width + x) * depth;
            buffer[offset + 2] = newColor.R;
            if(depth > 1)
            {
                buffer[offset + 1] = newColor.G;
                buffer[offset] = newColor.B;
            }
            if(depth > 3)
                buffer[offset + 3] = newColor.A;
        }

        public void threadGetPixels(int index, int nrThreads)
        {
            int slice = selectedImage.Width / nrThreads;
            int start = index * slice;
            int end = start + slice;
            if (index == nrThreads - 1)
                end = selectedImage.Width;

            for(int i=start; i < end;i++)
                for(int j=0;j<selectedImage.Height;j++)
                {
                    Color col = selectedImage.GetPixel(i, j);
                    //colors[i, j] = col;
                }
        }

        public void threadPutPyxelated(int index, int nrThreads, byte[] buffer, int width, int height, int depth, ColorKernels[] colors, int maxWidth, int maxHeight, bool notPixelated = false)
        {
            int slice = width / nrThreads;
            int start = index * slice;
            int end = start + slice;
            if (index == nrThreads - 1)
                end = width;
            for(int i=start;i<end;i++)
                for(int j=0;j<height;j++)
                {
                    //int x = i * 10;
                    //int y = j * 10;
                    //if (x >= maxWidth)
                    //    x = maxWidth - 1;
                    //if (y >= maxHeight)
                    //    y = maxHeight - 1;
                    ///smal.SetPixel(i, j, );
                    ///Color col = getColor(this.colors, x, y, maxWidth, maxHeight, this.depth);
                    Color col = getColor(buffer, i, j, width, height, depth);


                    int minDist1, minDist2, ind1, ind2;
                    minDist1 = minDist2 = int.MaxValue;
                    ind1 = ind2 = -1;
                    for (int k = 0; k < colors.Length; k++)
                    {
                        Color colO = colors[k].color;

                        int val = distance(col, colO);
                        if (val < minDist1)
                        {
                            minDist2 = minDist1;
                            ind2 = ind1;
                            minDist1 = val;
                            ind1 = k;
                        }
                        else if(val < minDist2)
                        {
                            minDist2 = val;
                            ind2 = k;
                        }
                    }

                    //{
                    //    int colorSelected = (int)(colorNr.Count * 1.0f * j / height);
                    //    setColor(buffer, i, j, width, height, depth, colorNr[colorSelected].color);
                    //    continue;
                    //}

                    if(ind2 < 0 || notPixelated)
                    {
                        setColor(buffer, i, j, width, height, depth, colors[ind1].color);
                    }
                    else
                    {
                        Color col1, col2;
                        col1 = colors[ind1].color;
                        col2 = colors[ind2].color;
                        double dist = Math.Sqrt(1f * minDist1 / minDist2);
                        //if (dist > 1)
                        //    dist = 1;

                        bool second = false;

                        if(dist > 0.25)
                        {
                            if (dist < 0.5)
                            {
                                second = i % 4 == 0 && j % 4 == 0;
                            }
                            else if (dist < 0.75)
                            {
                                second = i % 2 == 0 && j % 2 == 0;
                            }
                            else if (dist < 1)
                            {
                                second = (i + j) % 2 == 0;
                            }
                            else second = true;
                        }

                        if(!second)
                            setColor(buffer, i, j, width, height, depth, col1);
                        else
                            setColor(buffer, i, j, width, height, depth, col2);
                    }
                }
        }
        public void threadMakeCountour(int index, int nrThreads, byte[] buffer, int width, int height, int depth, byte[] output)
        {
            int slice = width / nrThreads;
            int start = index * slice;
            int end = start + slice;
            if (index == nrThreads - 1)
                end = width;

            Console.WriteLine("Start thread " + index);

            for (int i = start; i < end; i++)
                for (int j = 0; j < height; j++)
                {
                    Color col = getColor(buffer, i, j, width, height, depth);

                    float r = 0, g = 0, b = 0;
                    float nrCols = 0;
                    int radius = (width + height) / 300;
                    if (radius <= 0)
                        radius = 1;
                    if (radius > 5)
                        radius = 5;
                    for (int ii = -radius; ii <= radius; ii++)
                        for (int jj = -radius; jj <= radius; jj++)
                        {
                            if (ii == 0 && jj == 0)
                                continue;
                            int x = i + ii;
                            int y = j + jj;
                            if (x < 0)
                                x = 0;
                            if (y < 0)
                                y = 0;
                            if (x >= width)
                                x = width - 1;
                            if (y >= height)
                                y = height - 1;

                            Color blurCol = getColor(buffer, x, y, width, height, depth);
                            r += blurCol.R;
                            g += blurCol.G;
                            b += blurCol.B;
                            nrCols++;
                        }
                    r /= nrCols;
                    g /= nrCols;
                    b /= nrCols;

                    float r_, g_, b_;
                    r_ = (col.R + r * 2) / 3f;
                    g_ = (col.G + g * 2) / 3f;
                    b_ = (col.B + b * 2) / 3f;

                    r -= col.R;
                    g -= col.G;
                    b -= col.B;

                    r /= 255f;
                    g /= 255f;
                    b /= 255f;

                    float dist = (r * r + g * g + b * b) * 27;
                    //float dist = (Math.Abs(r) + Math.Abs(g) + Math.Abs(b)) * 3;
                    r = r_ - dist * 255;
                    g = g_ - dist * 255;
                    b = b_ - dist * 255;
                    if (r < 0)
                        r = 0;
                    if (g < 0)
                        g = 0;
                    if (b < 0)
                        b = 0;

                    setColor(output, i, j, width, height, depth, Color.FromArgb((int)r, (int)g, (int)b));
                    //if (r * r + g * g + b * b > 30 * 30)
                    //{
                    //    setColor(output, i, j, width, height, depth, Color.Black);
                    //}
                    //else
                    //    setColor(output, i, j, width, height, depth, col);
                }
        }

        public void threadPalletExtractor(int index, int nrThreads, byte[] colors, int width, int height, int depth, ColorKernels[] kernels, List<Color>[] groups, MySeaphore semaphore1, MySeaphore semaphore2)
        {
            int slice = width / nrThreads;
            int start = index * slice;
            int end = start + slice;
            if (index == nrThreads - 1)
                end = width;

            while(semaphore1.publicSignal == 0)
            {
                lock(groups)
                {
                    for (int i = start; i < end; i++)
                    {
                        if (semaphore1.publicSignal != 0)
                            break;
                        for (int j = 0; j < height; j++)
                        {
                            Color col = getColor(colors, i, j, width, height, depth);
                            int ind = 0;
                            int dis = distance(col, kernels[ind].color);
                            for (int k = 1; k < kernels.Length; k++)
                            {
                                int newD = distance(col, kernels[k].color);
                                if (newD < dis)
                                {
                                    dis = newD;
                                    ind = k;
                                }
                            }
                            groups[ind].Add(col);
                        }
                    }
                }
                semaphore1.Lock(index);
                semaphore2.Lock(index);
            }
        }

        private void setData()
        {
            //colors = new Color[selectedImage.Width, selectedImage.Height];

            var rect = new Rectangle(0, 0, selectedImage.Width, selectedImage.Height);
            var data = selectedImage.LockBits(rect, ImageLockMode.ReadWrite, selectedImage.PixelFormat);
            depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel
            colors = new byte[data.Width * data.Height * depth];

            //copy pixels to buffer
            Marshal.Copy(data.Scan0, colors, 0, colors.Length);
            selectedImage.UnlockBits(data);

            //Thread[] threads = new Thread[10];
            //for(int i=0;i<threads.Length;i++)
            //{
            //    threads[i] = new Thread(() => threadGetPixels(i, threads.Length));
            //    threads[i].Start();
            //}
            //for (int i = 0; i < threads.Length; i++)
            //    threads[i].Join();
        }

        /// <summary>
        /// transform the selected image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Start");
            if (colorNr.Count == 0)
                trackBar1_Scroll_(null, null);
            Console.WriteLine("Finnished finding main colors");

            //Bitmap smal = new Bitmap(selectedImage.Width / 10, selectedImage.Height / 10);
            string[] size = textBox2.Text.Split(new[] { 'x', ';', ':', ',', '.' });
            int small_width, small_height;
            if(size.Length < 2)
            {
                small_width = selectedImage.Width / 10;
                small_height = selectedImage.Height / 10;
            }
            else
            {
                small_width = int.Parse(size[0]);
                small_height = int.Parse(size[1]);
            }
            Bitmap smal;
            Graphics g_ = Graphics.FromImage(smal = new Bitmap(small_width, small_height));
            if (comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == 3)
                g_.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            else if(comboBox1.SelectedIndex == 1)
                g_.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            else if (comboBox1.SelectedIndex == 2)
                g_.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            g_.DrawImage(selectedImage, 0, 0, smal.Width, smal.Height);
            g_.Dispose();

            if (comboBox1.SelectedIndex == 3)
                smal = Filters.MedianFilter(smal, 3);

            Console.WriteLine(smal.Width + " " + smal.Height);

            if(!checkBox1.Checked || checkBox2.Checked)
            {
                var rect = new Rectangle(0, 0, smal.Width, smal.Height);
                var data = smal.LockBits(rect, ImageLockMode.ReadWrite, smal.PixelFormat);
                var depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; //bytes per pixel
                var buffer = new byte[data.Width * data.Height * depth];

                //copy pixels to buffer
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);


                Thread[] threads = new Thread[10];
                int minWidt, minHeigh, maxWidth, maxHeight;
                minWidt = smal.Width;
                minHeigh = smal.Height;
                maxWidth = selectedImage.Width;
                maxHeight = selectedImage.Height;

                if (!checkBox1.Checked)
                {
                    bool notPixelated = checkBox3.Checked;
                    for (int i = 0; i < threads.Length; i++)
                    {
                        int index = i;
                        threads[i] = new Thread(() => threadPutPyxelated(index, threads.Length, buffer, minWidt, minHeigh, depth, colorNr.ToArray(), maxWidth, maxHeight, notPixelated));
                        threads[i].Start();
                    }
                    for (int i = 0; i < threads.Length; i++)
                        threads[i].Join();
                    Console.WriteLine("Created pixels");
                }

                if(checkBox2.Checked)
                {
                    var outputBuffer = new byte[buffer.Length];
                    buffer.CopyTo(outputBuffer, 0);

                    for (int i = 0; i < threads.Length; i++)
                    {
                        int index = i;
                        threads[i] = new Thread(() => threadMakeCountour(index, threads.Length, buffer, minWidt, minHeigh, depth, outputBuffer));
                        threads[i].Start();
                    }
                    for (int i = 0; i < threads.Length; i++)
                        threads[i].Join();
                    Console.WriteLine("Created contour");
                    buffer = outputBuffer;
                }

                //Copy the buffer back to image
                Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
                smal.UnlockBits(data);
            }

            smallVersion = smal;
            Bitmap bmp2;

            int biggerWidth = smal.Width * 10;
            int biggerHeight = smal.Height * 10;
            if (biggerHeight > 3000 || biggerWidth > 3000)
            {
                biggerWidth = smal.Width;
                biggerHeight = smal.Height;
            }
            g_ = Graphics.FromImage(bmp2 = new Bitmap(biggerWidth, biggerHeight));
            g_.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g_.DrawImage(smal, 0, 0, biggerWidth, biggerHeight);
            g_.Dispose();
            Console.WriteLine("Resized");

            smallBigVersion = bmp2;
            pictureBox2.Image = bmp2;
        }


        /// <summary>
        /// load an image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
            selectedImage = Bitmap.FromFile(openFileDialog1.FileName) as Bitmap;

            if(selectedImage.Width > 1500 || selectedImage.Height > 1500)
            {
                float ratio = 1.0f * selectedImage.Width / selectedImage.Height;
                Bitmap smal;
                Graphics g_ = Graphics.FromImage(smal = new Bitmap((int)(1000 * ratio), (int)(1000 / ratio)));
                g_.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g_.DrawImage(selectedImage, 0, 0, smal.Width, smal.Height);
                g_.Dispose();
                selectedImage = smal;
            }

            textBox2.Text = (selectedImage.Width / 10) + "x" + (selectedImage.Height / 10);
            pictureBox1.Image = selectedImage;
            setData();
            button1_Click(null, null);
        }

        private Vector3 rgbToLab(Color col)
        {
            float r, g, b;

            r = col.R / 255f;
            g = col.G / 255f;
            b = col.B / 255f;

            if (r > 0.04045)
            { 
                r = (r + 0.055f) / 1.055f;
                r *= r;
            }
            else
                r = r / 12.92f;

            if (g > 0.04045)
            {
                g = (g + 0.055f) / 1.055f;
                g *= g;
            }
            else
                g = g / 12.92f;

            if (b > 0.04045)
            {
                b = (b + 0.055f) / 1.055f;
                b *= b;
            }
            else
                b = b / 12.92f;

            r *= 100;
            g *= 100;
            b *= 100;

            float X, Y, Z;
            X = r * 0.4124f + g * 0.3576f + b * 0.1805f;
            Y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
            Z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

            X = X / 95.047f;         // ref_X =  95.047
            Y = Y / 100.0f;          // ref_Y = 100.000
            Z = Z / 108.883f;        // ref_Z = 108.883


            if (X > 0.008856)
                X = (float)Math.Pow(X, 0.3333333333333333f);
            else
                X = (7.787f * X) + (16f / 116f);

            if (Y > 0.008856)
                Y = (float)Math.Pow(Y, 0.3333333333333333f);
            else
                Y = (7.787f * Y) + (16f / 116f);

            if (Z > 0.008856)
                Z = (float)Math.Pow(Z, 0.3333333333333333f);
            else
                Z = (7.787f * Z) + (16f / 116f);

            float L, A, B;

            L = (116 * Y) - 16;
            A = 500 * (X - Y);
            B = 200 * (Y - Z);

            return new Vector3(L * 10, A * 10, B * 10);
        }

        private int distance(Color c1, Color c2)
        {
            float dr, dg, db;
            dr = c1.R - c2.R;
            dg = c1.G - c2.G;
            db = c1.B - c2.B;
            if (colorDistanceIndex == 0)
                return (int)(dr * dr + dg * dg + db * db);
            else if (colorDistanceIndex == 1)
                return (int)(3 * dr * dr + 6 * dg * dg + 1 * db * db);
            else if (colorDistanceIndex == 2)
            {
                float r_ = dr / 2f;
                return (int)((2 + r_ / 256f) * dr * dr + 4 * dg * dg + (2 + (255 - r_) / 256) * db * db);
            }
            else if (colorDistanceIndex == 3)
            {
                var v1 = rgbToLab(c1);
                var v2 = rgbToLab(c2);
                dr = v1.x - v2.x;
                dg = v1.y - v2.y;
                db = v1.z - v2.z;

                return (int)(dr * dr + dg * dg + db * db);
            }
            return 0;
        }

        private void trackBar1_Scroll_(object sender, MouseEventArgs e)
        {
            if (selectedImage == null)
                return;
            if(trackBar1.Value != colorNr.Count)
            {
                colorNr.Clear();

                for (int i = 0; i < trackBar1.Value; i++)
                {
                    int x = random.Next(0, selectedImage.Width - 1);
                    int y = random.Next(0, selectedImage.Height - 1);
                    Color col = getColor(colors, x, y, selectedImage.Width, selectedImage.Height, this.depth);
                    colorNr.Add(new ColorKernels(col));
                }
            }

            int selectWidth = selectedImage.Width;
            int selectHeight = selectedImage.Width;
            Thread[] threads = new Thread[10];
            List<Color>[][] groups = new List<Color>[threads.Length][];
            MySeaphore seaphore1 = new MySeaphore(threads.Length + 1);
            MySeaphore seaphore2 = new MySeaphore(threads.Length + 1);
            for (int i=0;i<threads.Length;i++)
            {
                groups[i] = new List<Color>[colorNr.Count];
                for (int l = 0; l < colorNr.Count; l++)
                    groups[i][l] = new List<Color>();
            }
            int nrEpocs = 5 + trackBar1.Value / 5;
            for (int k = 0; k < nrEpocs; k++)
            {
                Console.WriteLine("Creating palette :  " + (k + 1) + "/" + nrEpocs);
                for (int i = 0; i < threads.Length; i++)
                {
                    int j = i;
                    threads[i] = new Thread(() => { threadPalletExtractor(j, threads.Length, colors, selectWidth, selectHeight, depth, colorNr.ToArray(), groups[j], seaphore1, seaphore2); });
                    threads[i].Start();
                }
                if (k == nrEpocs - 1)
                    seaphore1.publicSignal = 1;
                seaphore1.Lock(threads.Length);
                for (int i = 0; i < threads.Length; i++)
                {
                    //threads[i].Join();
                    for (int l = 0; l < colorNr.Count; l++)
                    {
                        for (int j = 0; j < groups[i][l].Count; j++)
                        { 
                            colorNr[l].addColor(groups[i][l][j]);
                        }
                        groups[i][l] = new List<Color>();
                    }
                }
                for (int i = 0; i < colorNr.Count; i++)
                    colorNr[i].reset();
                seaphore2.Lock(threads.Length);
            }

            for (int i = 0; i < colorNr.Count - 1; i++)
                for (int j = i + 1; j < colorNr.Count; j++)
                {
                    if(colorNr[i].size < colorNr[j].size)
                    {
                        var aux = colorNr[i];
                        colorNr[i] = colorNr[j];
                        colorNr[j] = aux;
                    }
                }
            showPallet();
        }

        private Bitmap showPallet()
        {
            Bitmap showColor = new Bitmap(30, 15 * colorNr.Count);
            for (int i = 0; i < showColor.Width; i++)
                for (int j = 0; j < showColor.Height; j++)
                {
                    int poz = (int)(1.0f * colorNr.Count * i / showColor.Width);
                    showColor.SetPixel(i, j, colorNr[poz].color);
                }
            pictureBox3.Image = showColor;
            return showColor;
        }

        private void addColorToPallet(MouseEventArgs me, PictureBox pB)
        {
            Bitmap spr = new Bitmap(pB.Image);
            Point coordinates = me.Location;
            coordinates.X = (int)(1.0f * spr.Width * coordinates.X / pB.Size.Width);
            coordinates.Y = (int)(1.0f * spr.Height * coordinates.Y / pB.Size.Height);

            colorNr.Add(new ColorKernels(spr.GetPixel(coordinates.X, coordinates.Y)));
            showPallet();
        }

        private void removeColorFromPallete(MouseEventArgs me, PictureBox pB)
        {
            Bitmap spr = new Bitmap(pB.Image);
            Point coordinates = me.Location;
            coordinates.X = (int)(1.0f * spr.Width * coordinates.X / pB.Size.Width);
            coordinates.Y = (int)(1.0f * spr.Height * coordinates.Y / pB.Size.Height);

            var col = spr.GetPixel(coordinates.X, coordinates.Y);
            int index = 0;
            int dist = distance(col, colorNr[index].color);
            for(int i=1;i<colorNr.Count;i++)
            {
                int newD = distance(col, colorNr[i].color);
                if(newD < dist)
                {
                    dist = newD;
                    index = i;
                }
            }

            colorNr.RemoveAt(index);

            showPallet();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (e.Button == MouseButtons.Left)
                addColorToPallet(me, pictureBox1);
            else if (e.Button == MouseButtons.Right)
                removeColorFromPallete(me, pictureBox1);
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (e.Button == MouseButtons.Left)
                addColorToPallet(me, pictureBox2);
            else if (e.Button == MouseButtons.Right)
                removeColorFromPallete(me, pictureBox2);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            colorDistanceIndex = comboBox2.SelectedIndex;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            imageToSave = smallVersion;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "PNG (*.png)|*.png|JPG (*jpg, *jpeg)|*.jpg";
            saveFileDialog1.DefaultExt = ".png";
            saveFileDialog1.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            imageToSave = showPallet();
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "PNG (*.png)|*.png|JPG (*jpg, *jpeg)|*.jpg";
            saveFileDialog1.DefaultExt = ".png";
            saveFileDialog1.ShowDialog();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            imageToSave = smallBigVersion;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.Filter = "PNG (*.png)|*.png|JPG (*jpg, *jpeg)|*.jpg";
            saveFileDialog1.DefaultExt = ".png";
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string fileLocation = saveFileDialog1.FileName;
            if (imageToSave != null)
                imageToSave.Save(fileLocation);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            Color col = colorDialog1.Color;
            colorNr.Add(new ColorKernels(col));
            showPallet();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
                removeColorFromPallete(me, pictureBox3);
        }

    }
}
