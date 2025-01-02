using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows.Media.Animation;

namespace ImageShrinker
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        }

        #region InitializeText
        private string notice1 = "Trim area to be Icon by Mouse";
        private string notice2 = "Click [Save Icons] button if satisfied";
        private string notice3 = "Created folder and Save Completed";
        private string openFilter = "Image Files (*.png, *.jpg)|*.png;*.jpg|All files (*.*)|*.*";
        private string openFolder = "Icons with same project name are already exist, please change your project name";
        private string notice4 = "Please load .PNG or .JPG";
        private string notice5 = "Fail to make icons, Trim area to be Icon by Mouse";
        private bool IsPasted = false;
        private bool IsJapanese = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CultureInfo.CurrentUICulture.Name == "ja-JP" || App.Language == "Japanese") IsJapanese = true;
            if (App.Language == "English") IsJapanese = false;
            if (IsJapanese)
            {
                Save.Content = "アイコン保存";
                Open.Content = "画像を開く";
                projectLabel.Content = "プロジェクト名";
                CompleteNotice.Content = "クリップボードからコピー＆貼り付けするか、画像をドラッグ＆ドロップするか、[画像を開く]をクリック";
                notice1 = "アイコンにしたい部分をマウスで囲む";
                notice2 = "気に入ったら[アイコン保存]ボタンをクリック";
                notice3 = "フォルダーを作成し、保存しました";
                notice4 = "PNG ファイルか JPG ファイルをロードしてください";
                notice5 = "アイコン作成に失敗、" + notice1;
                openFilter = "画像ファイル (*.png, *.jpg)|*.png;*.jpg|すべてのファイル (*.*)|*.*";
                openFolder = "同じプロジェクト名のアイコンが既にあります、別のプロジェクト名に変更してください";
            }
        }
        #endregion InitializeText

        #region OpenImage

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = openFilter;

            Nullable<bool> result = dlg.ShowDialog();

            if (result.Value)
            {
                ShowImage(dlg.FileName);
            }
            IsPasted = false;
        }

        private void Open_Click_Background(object sender, RoutedEventArgs e)
        {

        }

        private void Open_Click_Foreground(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Handle Copy & Past (ctrl+V)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) && (e.Key == Key.V))
            {
                if (Clipboard.ContainsImage())
                {
                    BitmapSource source = Clipboard.GetImage();
                    ShowImage(source);
                }
            }

        }
        #endregion OpenImage

        #region ShowImage

        // Show Image for Copy & Paste
        private void ShowImage(BitmapSource source)
        {
            projectName.Text = "MyProject";
            fileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\MyProject";
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            imageSource = new BitmapImage();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(memoryStream);
            imageSource.BeginInit();
            imageSource.StreamSource = new MemoryStream(memoryStream.ToArray());
            imageSource.EndInit(); memoryStream.Close();

            grayImage.Source = imageSource;
            if (imageSource.PixelHeight < 600 && imageSource.PixelWidth < 800)
            {
                grayImage.Width = imageSource.PixelWidth;
                grayImage.Height = imageSource.PixelHeight;
            }
            else
            {
                grayImage.Width = 800;
                grayImage.Height = 600;
            }
            //NewImageDisplayed();
        }

        // Show Image for file load and drag
        private void ShowImage(string filename)
        {
            fileName = filename;
            imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.UriSource = new Uri(fileName);
            imageSource.EndInit();

            grayImage.Source = imageSource;

            if (imageSource.PixelHeight < 600 && imageSource.PixelWidth < 800)
            {
                grayImage.Width = imageSource.PixelWidth;
                grayImage.Height = imageSource.PixelHeight;
            }
            else
            {
                grayImage.Width = 800;
                grayImage.Height = 600;
            }
            NewImageDisplayed();
        }

        #endregion ShowImage

        #region SaveIcons
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (Win8Logo180.Fill != null)
            if (true)
            {
                // due to virtualisation, WPF doesn't render each tab until the SelectedIndex changes
                // This is a little hack using .Net 4.5 awaitable, it will change the tab, yield back to the UI renderer, then continue.
                var currentIndex = iconTabControl.SelectedIndex;
                iconTabControl.SelectedIndex = 0;
                await Task.Delay(100);
                iconTabControl.SelectedIndex = 1;
                await Task.Delay(100);
                iconTabControl.SelectedIndex = 2;
                await Task.Delay(100);
                iconTabControl.SelectedIndex = currentIndex;

                string path = System.IO.Path.GetDirectoryName(this.fileName);
                //GenerateWp7Icons(path);
                //GenerateWp8Icons(path);
                //GenerateWin8Icons(path);
                GenerateAndroidIcons(path);

                string folder = "On same folder with your image";
                if (IsPasted) folder = "On your desktop";
                if (IsJapanese)
                {
                    folder = "画像と同じフォルダーに";
                    if (IsPasted) folder = "デスクトップに";
                }

                CompleteNotice.Content = folder + projectName.Text + " Icons" + notice3;
                CompleteNotice.Visibility = Visibility.Visible;

                Storyboard effect = (Storyboard)this.FindResource("Storyboard1");
                BeginStoryboard(effect);
            }
            else
            {
                CompleteNotice.Content = notice5;
            }

        }

        /*
        private void GenerateWp7Icons(string path)
        {
            // Create the folder if needed
            path = path + "\\" + projectName.Text + " WP7 Icons";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Generate the icons
            EncodeAndSave(Wp7Icon300, "StoreIcon.png", path);
            EncodeAndSave(Wp7Icon173, "Background.png", path);
            EncodeAndSave(Wp7Icon62, "ApplicationIcon.png", path);
        }*/

        /*private void GenerateWp8Icons(string path)
        {
            // Create the folder if needed
            path = path + "\\" + projectName.Text + " WP8 Icons";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Generate the icons
            EncodeAndSave(Wp8AppIcon, "ApplicationIcon.png", path);
            EncodeAndSave(Wp8FlipMedium, "FlipCycleTileMedium.png", path);
            EncodeAndSave(Wp8FlipSmall, "FlipCycleTileSmall.png", path);
            EncodeAndSave(Wp8FlipSmall_44, "Square44Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_50, "Square50Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_62, "Square62Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_70, "Square70Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_71, "Square71Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_99, "Square99Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_106, "Square106Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_150, "Square150Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_170, "Square170Logo.png", path);
            EncodeAndSave(Wp8FlipSmall_210, "Square210Logo.png", path);
        }*/

        private void GenerateWin8Icons(string path)
        {
            // Create the folder if needed
            path = path + "\\" + projectName.Text + " Win8 Icons";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            System.Console.WriteLine("Windows 8 Icons are now obsolete..." +
                "years has passed since windows release or sell this devices");
            // Generate the icons
            //EncodeAndSave(Win8Logo180, "Logo.scale-180.png", path);
            //EncodeAndSave(Win8Logo140, "Logo.scale-140.png", path);
            //EncodeAndSave(Win8Logo100, "Logo.scale-100.png", path);
            //EncodeAndSave(Win8Logo80, "Logo.scale-80.png", path);

            //EncodeAndSave(Win8SmallLogo180, "SmallLogo.scale-180.png", path);
            //EncodeAndSave(Win8SmallLogo140, "SmallLogo.scale-140.png", path);
            //EncodeAndSave(Win8SmallLogo100, "SmallLogo.scale-100.png", path);
            //EncodeAndSave(Win8SmallLogo80, "SmallLogo.scale-80.png", path);

            //EncodeAndSave(Win8SmallLogoTarget256, "SmallLogo.target-256.png", path);
            //EncodeAndSave(Win8SmallLogoTarget48, "SmallLogo.target-48.png", path);
            //EncodeAndSave(Win8SmallLogoTarget32, "SmallLogo.target-32.png", path);
            //EncodeAndSave(Win8SmallLogoTarget16, "SmallLogo.target-16.png", path);

            //EncodeAndSave(Win8StoreLogo180, "StoreLogo.scale-180.png", path);
            //EncodeAndSave(Win8StoreLogo140, "StoreLogo.scale-140.png", path);
            //EncodeAndSave(Win8StoreLogo100, "StoreLogo.scale-100.png", path);
        }
        private void GenerateAndroidIcons(string path)
        {
            //path = path + "\\" + projectName.Text + " Android Icons";
            path = path + "\\" + " Android Icons";

            // Generate the Android Icon Res Foldes

            List<string> androidResFolders = new List<string>();
            androidResFolders.Add(path + "\\mipmap-ldpi"); // 120 dpi 
            androidResFolders.Add(path + "\\mipmap-mdpi"); // 160 dpi
            androidResFolders.Add(path + "\\mipmap-hdpi"); // 240 dpi
            androidResFolders.Add(path + "\\mipmap-xhdpi"); // 320 dpi
            androidResFolders.Add(path + "\\mipmap-xxhdpi"); // 480 dpi
            androidResFolders.Add(path + "\\mipmap-xxxhdpi"); // 640 dpi
            //androidResFolders.Add("\\drawable-nodpi"); // 640 dpi ? not sure
            //androidResFolders.Add("\\drawable-tvdpi"); // 213 dpi

            foreach (string androidPath in androidResFolders)
            {
                if (!Directory.Exists(androidPath))
                {
                    Directory.CreateDirectory(androidPath);
                }
            }

            // Generate the icons

            string iconName = "ic_launcher.png";
            //manually sync the array path
            EncodeAndSave(Android_ldpi, iconName, androidResFolders.ElementAt(0));
            EncodeAndSave(Android_mdpi, iconName, androidResFolders.ElementAt(1));
            EncodeAndSave(Android_hdpi, iconName, androidResFolders.ElementAt(2));
            EncodeAndSave(Android_xhdpi, iconName, androidResFolders.ElementAt(3));
            EncodeAndSave(Android_xxhdpi, iconName, androidResFolders.ElementAt(4));
            EncodeAndSave(Android_xxxhdpi, iconName, androidResFolders.ElementAt(5));
            EncodeAndSave(AndroidStore_512, "StoreIcon512.png", path);
            
            //EncodeAndSave(AndroidStore_114, "StoreIcon114.png", path);//This is Amazon AppStore?

        }

        private void EncodeAndSave(FrameworkElement icon, string name, string filePath)
        {
            // Create BitmapFrame for Icon
            var rtb = new RenderTargetBitmap((int)icon.Width, (int)icon.Height, 96.0, 96.0, PixelFormats.Pbgra32);

            // Use DrawingGroup for high quality rendering
            // See: http://www.olsonsoft.com/blogs/stefanolson/post/Workaround-for-low-quality-bitmap-resizing-in-WPF-4.aspx
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(imageSource, new Rect(new Point(), new Size((int)icon.Width, (int)icon.Height))));

            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
                dc.DrawDrawing(group);

            rtb.Render(dv);
            BitmapFrame bmf = BitmapFrame.Create(rtb);
            bmf.Freeze();
            //Icon200.Fill = new ImageBrush(bmf) ;
            //string filePath = System.IO.Path.GetDirectoryName(this.fileName);
            string fileOut = filePath + "\\" + name;
            FileStream stream = new FileStream(fileOut, FileMode.Create);

            // PNGにエンコード
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(bmf);
            encoder.Save(stream);
            stream.Close();
        }

        #endregion SaveIcons

        #region DragAndDrop

        private string fileName;
        BitmapImage imageSource;
        private double scale = 1.0;

        private void myImage_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            string draggedFileName = IsSingleFile(e);

            if (draggedFileName.Contains(".png") || draggedFileName.Contains(".PNG") || draggedFileName.Contains(".jpg") || draggedFileName.Contains(".JPG"))
            {
                //MessageBoxResult mbr = MessageBoxResult.OK;
                //if (fileName != null)
                //    mbr = System.Windows.MessageBox.Show("表示されている画像と入れ替えますか？", "relace ?", MessageBoxButton.OKCancel);

                //if (mbr == MessageBoxResult.OK)
                //{
                ShowImage(draggedFileName);
                //}
                IsPasted = false;
            }
            else
            {
                System.Windows.MessageBox.Show(notice4, "Please drag png/jpg file", MessageBoxButton.OK);
            }

        }

        private void grayImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NewImageDisplayed();
        }
        private void NewImageDisplayed()
        {
            scale = imageSource.Width / grayImage.ActualWidth;
            string name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            projectName.Text = name;
            CompleteNotice.Content = notice1;

            myCanvas.Children.Remove(rectFrame);

            UpdatePreviewIcons(new Point(0, 0), new Point(imageSource.Width, imageSource.Height));
        }

        private string IsSingleFile(DragEventArgs args)
        {
            // データオブジェクト内のファイルをチェック
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] fileNames = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                // 単一ファイル/フォルダをチェック
                if (fileNames.Length == 1)
                {
                    // ファイルをチェック（ディレクトリはfalseを返す）
                    if (File.Exists(fileNames[0]))
                    {
                        // この時点で単一ファイルであることが分かる
                        return fileNames[0];
                    }
                }
            }
            return null;
        }

        private void myImage_PreviewDragOver(object sender, DragEventArgs args)
        {
            // 単一ファイルだけを処理したい
            if (IsSingleFile(args) != null) args.Effects = DragDropEffects.Copy;
            else args.Effects = DragDropEffects.None;

            // イベントをHandledに、するとDragOverハンドラがキャンセルされる
            args.Handled = true;
        }
        #endregion DragAndDrop

        #region MouseMove
        private Point p1 = new Point();
        private Point p2 = new Point();


        private void myCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CompleteNotice.Content = notice2;
        }
        /// <summary>
        /// マウス左ボタン押下用のコールバック
        /// </summary>
        private void UC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            p1 = e.GetPosition(grayImage);
            CompleteNotice.Content = notice1;
        }
        private Rectangle rectFrame = new Rectangle();

        /// <summary>
        /// マウス移動用のコールバック
        /// </summary>
        private void UC_MouseMove(object sender, MouseEventArgs e)
        {
            p2 = e.GetPosition(grayImage);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double w = Math.Abs(p1.X - p2.X);
                double h = Math.Abs(p1.Y - p2.Y);
                if (w > h)
                    p2.Y = (p2.Y > p1.Y) ? p1.Y + w : p1.Y - w;
                else
                    p2.X = (p2.X > p1.X) ? p1.X + h : p1.X - w;

                Point lt = new Point((p1.X > p2.X) ? p2.X : p1.X, (p1.Y > p2.Y) ? p2.Y : p1.Y);
                Point rb = new Point((p1.X > p2.X) ? p1.X : p2.X, (p1.Y > p2.Y) ? p1.Y : p2.Y);

                // rabber band
                myCanvas.Children.Remove(rectFrame);
                rectFrame.Stroke = Brushes.Gray;
                rectFrame.StrokeThickness = 1.0;
                rectFrame.Width = rb.X - lt.X;
                rectFrame.Height = rb.Y - lt.Y;
                rectFrame.RenderTransform = new TranslateTransform(lt.X, lt.Y);
                myCanvas.Children.Add(rectFrame);

                UpdatePreviewIcons(lt, rb);
            }
        }

        private void UpdatePreviewIcons(Point lt, Point rb)
        {
            // Icon Images
            Point sourceLt = new Point(lt.X * scale, lt.Y * scale);
            Point sourceRb = new Point(rb.X * scale, rb.Y * scale);

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = imageSource;
            brush.Viewbox = new Rect(sourceLt, sourceRb);
            brush.ViewboxUnits = BrushMappingMode.Absolute;
            brush.Stretch = Stretch.Fill;
            //Android Icons with google play store
            Android_ldpi.Fill = brush;
            Android_mdpi.Fill = brush;
            Android_hdpi.Fill = brush;
            Android_xhdpi.Fill = brush;
            Android_xxhdpi.Fill = brush;
            Android_xxxhdpi.Fill = brush;
            AndroidStore_512.Fill = brush;
            //AndroidStore_114.Fill = brush;
        }
        #endregion MouseMove
    }
}
