using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;

namespace RayTracer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CalculatePicture();
        }

        private void BtnShow_Click(object sender, RoutedEventArgs e)
        {
            /*int bmpSize = 300;
            Bitmap bmp = new Bitmap(bmpSize, bmpSize);
            Scene scene = new Scene(bmpSize, bmpSize);
            for (int i = 0; i < bmpSize; i++)
                for (int j = 0; j < bmpSize; j++)
                    if (scene.Intersect(i, j))
                        bmp.SetPixel(i, j, System.Drawing.Color.Red);*/

            /*if (scene.Intersect(150, 175))
                bmp.SetPixel(150, 175, System.Drawing.Color.Red);*/

            /*MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();*/
            CalculatePicture();
        }

        private void CalculatePicture()
        {
            BitmapImage image = new BitmapImage();
            Scene scene = new Scene(image);
            scene.Render();

            this.Width = image.Width + 18;
            this.Height = image.Height + 40;

            SceneImage.Width = image.Width;
            SceneImage.Height = image.Height;
            SceneImage.Source = image;
        }
    }
}
