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
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.IO;

namespace LayoutTest1
{
    /// <summary>
    /// Set_ROI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Set_ROI : Window
    {
        bool video_ratio = false;
        //최대 ROI 수
        int Max_ROI_Count = 100;
        static ObservableCollection<DrawROI> drawROIs = new ObservableCollection<DrawROI>();//ROI 


        public Set_ROI(Item camera_item)
        {
            InitializeComponent();
            Image_viewer_v.EnableVisibleHeader = false;
            Image_viewer_v.MaintainImageAspectRatio = false;
            Image_viewer_v.ConnectResponseReceived += Image_viewer_v_ConnectResponseReceived;
            Image_viewer_v.CameraFQID = camera_item.FQID;
            Image_viewer_v.Initialize();
            Image_viewer_v.Connect();
            Image_viewer_v.StartLive();

            //Thread thread = new Thread(() => after_loaded());
            //thread.Start();

            polygon_item.ItemsSource = drawROIs;

        }

        /*
         * ROI 한개만 필요했을 때 시작하자마자 비율 설정하기위한 thread
        private void after_loaded()
        {
            while (true)
            {
                if ((Image_viewer_v.ImageSize.Width != 0) && (Image_viewer_v.ImageSize.Height != 0))
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
        */

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
            if(view_roi.Visibility == Visibility.Visible)
            {
                _roi.Clear_all();
            }
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


        /*
         * 보류
        private void change_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 변경
            if (view_roi_1.Visibility == Visibility.Visible)
            {
                _roi_2.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
                view_roi_1.Visibility = Visibility.Collapsed;
                view_roi_2.Visibility = Visibility.Visible;

            }
            else if (view_roi_2.Visibility == Visibility.Visible)
            {
                _roi_3.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
                view_roi_2.Visibility = Visibility.Collapsed;
                view_roi_3.Visibility = Visibility.Visible;
            }
            else if (view_roi_3.Visibility == Visibility.Visible)
            {
                _roi_1.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
                view_roi_3.Visibility = Visibility.Collapsed;
                view_roi_1.Visibility = Visibility.Visible;
            }
            else
            {
                _roi_1.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
                view_roi_1.Visibility = Visibility.Visible;
            }
        }

        */


        private void save_ROI_setting(object sender, RoutedEventArgs e)
        {
            //ROI 설정 저장

        }

        private void Add_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 추가
            DrawROI new_roi = new DrawROI();
            new_roi.index = drawROIs.Count();
            new_roi.isvisible = false;
            new_roi.main_color = Brushes.Red;
            drawROIs.Add(new_roi);


        }
        private void Modify_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 수정

        }


        private void Delete_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 삭제
        }

        private void ColorPicker_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            //main_color 색을 바꿔야함.
            //ListViewItem lvi = FindParent<ListViewItem>((sender as ));


            //drawROIs[].main_color = e.NewValue;
        }

        private void color_button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }

    }
}
