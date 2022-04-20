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
using System.Windows.Shapes;
using VideoOS.Platform;
using VideoOS.Platform.UI;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Threading;

namespace LayoutTest1
{
    /// <summary>
    /// Set_ROI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Set_ROI : Window
    {
        bool video_ratio = false;
        public Set_ROI(Item camera_item)
        {
            InitializeComponent();
            Image_viewer_v.EnableVisibleHeader = false;
            Image_viewer_v.MaintainImageAspectRatio = false;
            view_roi.Stretch = Stretch.Fill;
            Image_viewer_v.ConnectResponseReceived += Image_viewer_v_ConnectResponseReceived;
            Image_viewer_v.CameraFQID = camera_item.FQID;
            Image_viewer_v.Initialize();
            Image_viewer_v.Connect();
            Image_viewer_v.StartLive();
            view_roi.Visibility = Visibility.Visible;

            Thread thread = new Thread(() => after_loaded());
            thread.Start();

        }

        private void after_loaded()
        {
            while(true)
            {
                if((Image_viewer_v.ImageSize.Width != 0) && (Image_viewer_v.ImageSize.Height != 0))
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        _roi.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
                    }));
                    break;
                }
                else
                {
                    continue;
                }

            }
        }

        public void Image_viewer_v_ConnectResponseReceived(object sender, VideoOS.Platform.Client.ConnectResponseEventArgs e)
        {
            Console.WriteLine(e);
        }


        private void Exit_draw_ROI_Btn(object sender, RoutedEventArgs e)
        {
            Image_viewer_v.Disconnect();

            this.Close();
        }

        private void Clear_all(object sender, RoutedEventArgs e)
        {
            //모든 폴리곤 속성 다 지우기
            _roi.Clear_all();
        }

        private void ratio_change(object sender, RoutedEventArgs e)
        {
            if (video_ratio)//고정상태이면 풀어주기
            {
                video_ratio = false;
                //MessageBox.Show("이미지 고정을 해제합니다.");
                Image_viewer_v.MaintainImageAspectRatio = false;
                view_roi.Stretch = Stretch.Fill;
            }
            else//풀린 상태이면 고정하기
            {
                video_ratio = true;
                //MessageBox.Show("이미지를 고정합니다.");
                Image_viewer_v.MaintainImageAspectRatio = true;
                view_roi.Stretch = Stretch.Uniform;
            }
        }

        private void new_ROI(object sender, RoutedEventArgs e)
        {

        }

        private void save_ROI_setting(object sender, RoutedEventArgs e)
        {

        }

        private void erase_ROI(object sender, RoutedEventArgs e)
        {

        }
    }
}
