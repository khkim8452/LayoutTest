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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            _roi.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            _roi.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
        }

        private void Window_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _roi.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);

        }
    }
}
