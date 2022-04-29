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
        ObservableCollection<DrawROI> ROIs_list = new ObservableCollection<DrawROI>();//ROI 

        public Set_ROI(Item camera_item)
        {
            InitializeComponent();
            // polygon_item.DataContext = this;
            Image_viewer_v.EnableVisibleHeader = false;
            Image_viewer_v.MaintainImageAspectRatio = false;
            Image_viewer_v.CameraFQID = camera_item.FQID;
            Image_viewer_v.Initialize();
            Image_viewer_v.Connect();
            Image_viewer_v.StartLive();
            polygon_item.ItemsSource = ROIs_list;
        }

        private void Exit_draw_ROI_Btn(object sender, RoutedEventArgs e)
        {
            //나가기 버튼
            Image_viewer_v.Disconnect();
            this.Close();
        }

        private void ratio_change(object sender, RoutedEventArgs e)
        {
            //비율 고정
            
            MessageBox.Show($" video :{Image_viewer_v.ImageSize} , grid:{top_grid.ActualWidth},{top_grid.ActualHeight} , viewbox :{view_roi.Width},{view_roi.Height} , canvas:{canvas_roi.ActualWidth},{canvas_roi.ActualHeight}");

            if (video_ratio)//고정상태이면 풀어주기
            {
                video_ratio = false;
                MessageBox.Show("이미지 고정을 해제합니다.");
                Image_viewer_v.MaintainImageAspectRatio = false;
                view_roi.Stretch = Stretch.Fill;
            }
            else//풀린 상태이면 고정하기
            {
                video_ratio = true;
                MessageBox.Show("이미지를 고정합니다.");
                Image_viewer_v.MaintainImageAspectRatio = true;
                view_roi.Stretch = Stretch.Uniform;
            }
        }

        private void Clear_all(object sender, RoutedEventArgs e)
        {
            //모든 폴리곤 속성 다 지우기
            canvas_roi.Children.Clear();
            ROIs_list.Clear();
        }


        private void save_ROI_setting(object sender, RoutedEventArgs e)
        {
            //ROI 설정 저장

        }

        private void Add_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 추가
            canvas_roi.Width = Image_viewer_v.ImageSize.Width;
            canvas_roi.Height = Image_viewer_v.ImageSize.Height;
            DrawROI new_roi = new DrawROI();
            new_roi.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
            new_roi.name = "";
            new_roi.isvisible = false;
            Random r = new Random();
            switch(r.Next(5))
            {
                case 0: new_roi.main_color = Brushes.Red; break;
                case 1: new_roi.main_color = Brushes.Yellow; break;
                case 2: new_roi.main_color = Brushes.Blue; break;
                case 3: new_roi.main_color = Brushes.Green; break;
                case 4: new_roi.main_color = Brushes.Purple; break;
            }
            ROIs_list.Add(new_roi); //리스트에 추가하고,
            canvas_roi.Children.Add(new_roi);//캔버스에 자식 할당.
        }

        private void Delete_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 삭제
            if(polygon_item.SelectedIndex == -1)
            {
                MessageBox.Show("선택된 ROI가 없습니다.\n삭제하고자 하는 ROI를 선택하고 다시 실행해주세요.");
            }
            else
            {
                canvas_roi.Children.Remove(ROIs_list[polygon_item.SelectedIndex]);
                ROIs_list.Remove(ROIs_list[polygon_item.SelectedIndex]);
            }
        }


        private void color_button_Click(object sender, RoutedEventArgs e)
        {
            //list_text_block
            ListViewItem lvi = FindParent<ListViewItem>((sender as Button));
            ListView lv = FindParent<ListView>((sender as Button));
            lv.SelectedItem = lvi.DataContext;
        }

        private T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }

        private void polygon_item_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            color_picker.Color = (ROIs_list[polygon_item.SelectedIndex].main_color as SolidColorBrush).Color;
        }

        private void list_radio_button_Click(object sender, RoutedEventArgs e)
        {
            //ROI 보이기
            ListViewItem lvi = FindParent<ListViewItem>((sender as RadioButton));
            ListView lv = FindParent<ListView>((sender as RadioButton));
            lv.SelectedItem = lvi.DataContext;

            
        }

        private void ColorPicker_MouseMove(object sender, MouseEventArgs e)
        {
            //마우스 왼쪽이 눌렸을때만 move가 되고,
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //리스트에서 선택된 개체가 있을때만 실행.
                if (polygon_item.SelectedIndex != -1)
                {
                    Brush b = new SolidColorBrush(color_picker.Color);
                    ROIs_list[polygon_item.SelectedIndex].main_color = b;
                    
                }
            }
        }
    }
}
