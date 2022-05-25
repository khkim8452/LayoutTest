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
using System.ComponentModel;

namespace LayoutTest1
{
    /// <summary>
    /// Set_ROI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Set_ROI : Window
    {
        bool video_ratio = false;
        int count = 0;
        int z_index_count = 1; //0으로 하면 맨 처음 클릭은 위에 표시되지 않는 오류가 있음.
        ObservableCollection<DrawROI> ROIs_list = new ObservableCollection<DrawROI>();//ROI 
        save_ROI sroi = new save_ROI();

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
            sroi.set_path(System.IO.Directory.GetCurrentDirectory() + Image_viewer_v.CameraFQID.ObjectId);
            if (sroi.is_savefile_exist()) //세이브파일이 있으면
            {
                ROIs_list = sroi.load_ROI_list(); //세이브 파일에 저장된 리스트 원래 형식대로 가지고 오기
                load_canvas(ROIs_list); // 리스트에 있는 객체들 canvas에 children 속성으로 그리기.
                canvas_roi.Width = ROIs_list[0]._width;
                canvas_roi.Height = ROIs_list[0]._height;
            }
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

            if (video_ratio)//고정상태이면 풀어주기
            {
                video_ratio = false;
                Image_viewer_v.MaintainImageAspectRatio = false;
                view_roi.Stretch = Stretch.Fill;
            }
            else//풀린 상태이면 고정하기
            {
                video_ratio = true;
                Image_viewer_v.MaintainImageAspectRatio = true;
                view_roi.Stretch = Stretch.Uniform;
            }
        }

        private void Clear_all(object sender, RoutedEventArgs e)
        {
            //모든 폴리곤 속성 다 지우기
            MessageBoxResult result = MessageBox.Show("지우면 되돌릴 수 없습니다. 지우시겠습니까?","경고",MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                canvas_roi.Children.Clear();
                ROIs_list.Clear(); //오류
                sroi.delete_file();
            }
            else if(result == MessageBoxResult.No)
            {

            }
        }


        private void save_ROI_setting(object sender, RoutedEventArgs e)
        {
            //ROI 설정 저장
            if (ROIs_list.Count == 0)
            {
                //저장할게 없으면
                return;
            }
            else
            {
                sroi.save_ROI_list(ROIs_list);
            }
        }

        private void Add_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 추가
            canvas_roi.Width = Image_viewer_v.ImageSize.Width;
            canvas_roi.Height = Image_viewer_v.ImageSize.Height;

            DrawROI new_roi = new DrawROI();
            new_roi.setRatio(Image_viewer_v.ImageSize.Height, Image_viewer_v.ImageSize.Width);
            new_roi.name = "이름";
            new_roi.isvisible = false;

            //Random r = new Random(); -> 랜덤
            //switch(r.Next(5)) -> 랜덤
            switch (count)
            {
                case 0: new_roi.main_color = Brushes.Red; count++; break;
                case 1: new_roi.main_color = Brushes.DarkOrange; count++; break;
                case 2: new_roi.main_color = Brushes.Yellow; count++; break;
                case 3: new_roi.main_color = Brushes.Lime; count++; break;
                case 4: new_roi.main_color = Brushes.DarkGreen; count++; break;
                case 5: new_roi.main_color = Brushes.Cyan; count++; break;
                case 6: new_roi.main_color = Brushes.MediumBlue; count++; break;
                case 7: new_roi.main_color = Brushes.MediumPurple; count++; break;
                case 8: new_roi.main_color = Brushes.Violet; count++; break;
                case 9: new_roi.main_color = Brushes.Snow; count++; break;
                case 10: new_roi.main_color = Brushes.Black; count = 0; break;
            }

            ROIs_list.Add(new_roi); //리스트에 추가하고,
            canvas_roi.Children.Add(new_roi);//캔버스에 자식 할당.
        }

        private void Delete_ROI(object sender, RoutedEventArgs e)
        {
            //ROI 삭제
            if (polygon_item.SelectedIndex == -1)
            {
                MessageBox.Show("선택된 ROI가 없습니다.\n삭제하고자 하는 ROI를 선택하고 다시 실행해주세요.");
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("삭제하면 되돌릴 수 없습니다. 삭제하시겠습니까?", "경고", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {

                    canvas_roi.Children.Remove(ROIs_list[polygon_item.SelectedIndex]);
                    try
                    {

                        ROIs_list.RemoveAt(polygon_item.SelectedIndex);
                        if(ROIs_list.Count == 0)
                        {
                            sroi.delete_file();
                        }
                        else
                        {
                            sroi.save_ROI_list(ROIs_list);

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else if(result == MessageBoxResult.No)
                {
                    //삭제 안함
                }
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
            if (polygon_item.SelectedIndex < 0) return;

            color_picker.Color = (ROIs_list[polygon_item.SelectedIndex].main_color as SolidColorBrush).Color;
            Canvas.SetZIndex(canvas_roi.Children[polygon_item.SelectedIndex], z_index_count++);
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

        private void list_radio_button_Checked(object sender, RoutedEventArgs e)
        {
            //ROI 보이기
            ListViewItem lvi = FindParent<ListViewItem>((sender as CheckBox));
            ListView lv = FindParent<ListView>((sender as CheckBox));
            lv.SelectedItem = lvi.DataContext;

            ROIs_list[polygon_item.SelectedIndex].Enable_and_Disable(true);

        }

        private void list_radio_button_Unchecked(object sender, RoutedEventArgs e)
        {
            //ROI 보이기
            ListViewItem lvi = FindParent<ListViewItem>((sender as CheckBox));
            ListView lv = FindParent<ListView>((sender as CheckBox));
            lv.SelectedItem = lvi.DataContext;

            ROIs_list[polygon_item.SelectedIndex].Enable_and_Disable(false);
        }


        private void change_ROI_name(object sender, RoutedEventArgs e)
        {
            change_ROI_name cn = new change_ROI_name();

            cn.set_Name_out(ROIs_list[polygon_item.SelectedIndex].name);//현재 이름을 dialog에 toss해줌.
            cn.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            cn.ShowDialog();

            ROIs_list[polygon_item.SelectedIndex].name = cn.result; //결과 이름을 현재 이름에 반영해줌.
        }

        private void load_canvas(ObservableCollection<DrawROI> ROIs_list)
        {
            try
            {
                for (int i = 0; i < ROIs_list.Count; i++)
                {
                    ROIs_list[i].setRatio(ROIs_list[0]._height, ROIs_list[0]._width);
                    ROIs_list[i].load_and_draw_(1); //DrawROI 객체 안에서 ROI_paper canvas 안에 추가하는 작업
                    canvas_roi.Children.Add(ROIs_list[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            if(ROIs_list.Count == 0)
            {
                //저장할게 없으면
                return;
            }
            else
            {
                sroi.save_ROI_list(ROIs_list);
            }


        }
        public ObservableCollection<DrawROI> return_ROI_outside()
        {
            return ROIs_list;
        }

    }
}
