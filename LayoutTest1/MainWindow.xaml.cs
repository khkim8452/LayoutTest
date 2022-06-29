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
using System.IO;
using VideoOS.Platform.SDK.UI.LoginDialog;
using VideoOS.Platform.SDK.Media;
using System.Windows.Threading;
using System.Threading;
using System.Data.SQLite;
using VideoOS.Platform;
using VideoOS.Platform.Live;
using VideoOS.Platform.UI;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;

// JSON 파일 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Drawing;

// xml 파일
using System.Xml;
using System.Xml.Linq;

namespace LayoutTest1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Guid IntegrationId = new Guid("15B6ACBB-E1B6-4360-86B3-78445C56684D");
        private const string IntegrationName = "Playback WPF User";
        private const string Version = "1.0";
        private const string ManufacturerName = "Sample Manufacturer";
        int default_rowcol = 4; //기본 스케일
        bool is_fullscreen = false; // 전체화면 (초기에는 전체화면 X)
        save_setting ss = new save_setting(); // 메인화면 설정
        Layout l = null;
        List<Item> CameraList = null; //카메라 리스트 
        
        //metadata & event
        ObservableCollection<Event_> EventList = new ObservableCollection<Event_>(); //이벤트 리스트 (전체 담기)
        SQLiteConnection connection = new SQLiteConnection();
        private Item _selectItem1; //메타데이터 채널 설정 아이템
        private MetadataLiveSource _metadataLiveSource; 
        private int _count;
        SQLite_Event_DB database = new SQLite_Event_DB(); //데이터베이스
        int Max_Row_Count = 100; //최대 검색 개수
        string order_Row = ""; // 정렬방법 desc and asc
        int search_Type = -1;
        SerialPort serial_Port = new SerialPort(); //시리얼 포트 
        CameraCell single_cc; // 버튼 누르면 기존 single 모드 해제하기 위해 담아놓는 변수


        public MainWindow()
        {
            InitializeComponent();
            //시계 timer thread 실행
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Clock_Tick;
            timer.Start();
            //원래의 속성을 json 파일로부터 가지고 와서 설정 복구한다.
            ss.set_path(System.IO.Directory.GetCurrentDirectory() + @"save_setting_file");
            load_mainwindow();//저장된 설정 불러오기
            VideoOS.Platform.SDK.Environment.Initialize();			// General initialize.  Always required
            VideoOS.Platform.SDK.UI.Environment.Initialize();		// Initialize UI
            VideoOS.Platform.SDK.Export.Environment.Initialize();   // Initialize recordings access
            VideoOS.Platform.SDK.Media.Environment.Initialize();    // metadata initialize
            _loginButton_Click();
            CameraList = GetCameraList();
            Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
            FillCameraListBox(); 
            event_list.ItemsSource = EventList;
            //데이터베이스
            this.KeyDown += new KeyEventHandler(HandleEsc);

            //ringobell 추가
            string[] ports = SerialPort.GetPortNames();
            port_name.ItemsSource = ports;



            //ringobell


        }
        #region basic_function
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //여기서 왜 오류나냐? 시작하자마자 끄니까 오류나네
            camera_view_grid.Children.Add(l.MainGrid);
            l.SetRowCol(default_rowcol, default_rowcol);
            LoadAllCamera(null, null);
        }

        public List<Item> GetCameraList()
        {
            var q = Configuration.Instance.GetItemsSorted();
            var result = new List<Item>();
            while (q.Count > 0)
            {
                Item i = q[0];
                q.RemoveAt(0);
                if (i.FQID.Kind == Kind.Camera && i.FQID.FolderType == FolderType.No)
                {
                    Console.WriteLine(i.Name);
                    result.Add(i);
                }
                q.AddRange(i.GetChildrenSorted());
            }
            return result;

        }
        public void FillCameraListBox()
        {
            camera_list_box.Items.Clear();
            foreach (Item item in CameraList)
            {
                ListBoxItem i = new ListBoxItem();
                i.Content = item.Name;
                i.Tag = item;
                camera_list_box.Items.Add(i);
            }
        }
        private void _loginButton_Click()
        {
            var loginForm = new DialogLoginForm(SetLoginResult, IntegrationId, IntegrationName, Version, ManufacturerName); //로그인 폼

            loginForm.AutoLogin = false;
            loginForm.ShowDialog();
            if (Connected)
            {
                loginForm.Close();
                l = Layout.Instance;
            }
        }

        private static bool Connected = false;
        private static void SetLoginResult(bool connected)
        {
            Connected = connected;
        }
        private void Scale_Change_Btn_Click(object sender, RoutedEventArgs e)
        {
            int r = int.Parse((sender as Button).Tag as string);
            l.SetRowCol(r, r);

        }


        private void LoadAllCamera(object sender, RoutedEventArgs e)
        {
            ///싱글테이크상태일때 전체 카메라 부르기 X
            if (Layout.Instance.IsSingle)
            {
                return;
            }
            for (int i = 0; i < CameraList.Count; i++)
            {
                int r = Layout.Instance.Row;
                int c = Layout.Instance.Col;
                if (i / c == r) break;
                Layout.Instance.ActivateCamera(CameraList[i], i / c, i % c);
            }
        }

        private void CloseAll(object sender, RoutedEventArgs e)
        {
            Layout.Instance.CloseAll();
        }

        bool DND_MouseDown = false;
        System.Windows.Point DND_StartPoint;
        private void CameraListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DND_MouseDown = true;
            DND_StartPoint = e.GetPosition(null);
            e.Handled = false;
        }
        private void CameraListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DND_MouseDown = false;
            e.Handled = false;
        }
        private void CameraListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Vector diff = DND_StartPoint - e.GetPosition(null);
            //e.Handled = true;
            //if (MouseDown) StartDrag();

            if (e.LeftButton == MouseButtonState.Pressed && (
                Math.Abs(diff.X) > 5 &&
                Math.Abs(diff.Y) > 5))
            {
                StartDrag();
            }

        }
        public void StartDrag()
        {
            ListBoxItemsBag b = new ListBoxItemsBag();
            foreach (ListBoxItem i in camera_list_box.SelectedItems)
            {
                b.Bag.Add(i);
            }

            //List<ListBoxItem> data = new List<ListBoxItem>();


            if (b != null && b.Bag.Count > 0)
            {
                DragDrop.DoDragDrop(camera_list_box, b, DragDropEffects.Move);
            }
        }

        private void rbShowTitle_Click(object sender, RoutedEventArgs e)
        {
            V.ShowTitleAlways = true;
            l.UpdateTitleStatus();
        }

        private void rbHideTitle_Click(object sender, RoutedEventArgs e)
        {
            V.ShowTitleAlways = false;
            l.UpdateTitleStatus();
        }

        private void Full_Screen_Btn_Click(object sender, RoutedEventArgs e)
        {
            //전체화면
            is_fullscreen = true;
            camera_list_grid.Visibility = Visibility.Collapsed;
            event_grid.Visibility = Visibility.Collapsed;
            top_grid.Visibility = Visibility.Collapsed;
            bottom_system_bar.Visibility = Visibility.Collapsed;
            this.WindowStyle = WindowStyle.None;

        }

        private void Save_Settings(object sender, RoutedEventArgs e)
        {
            //설정 저장을 위해 json 파일에 현재 설정값을 저장하는 버튼
            ss.save_MainWindow(l.Row, is_fullscreen);
        }

        private void PlayBack_Viewer(object sender, RoutedEventArgs e)
        {
            LayoutTest1.Playback_Viewer pb = new LayoutTest1.Playback_Viewer();
            pb.ShowDialog();
        }

        private void load_mainwindow()
        {
            if (ss.is_savefile_exist())
            {
                if(ss.load_MainWindow_1() != -1)
                {
                    int a = ss.load_MainWindow_1(); //스케일 가져오기
                    bool b = ss.load_MainWindow_2();//전체화면 여부 가져오기
                    default_rowcol = a;
                    is_fullscreen = b;
                }

            }
            else
            {
                //저장된 파일이 없으면 
                Console.WriteLine("저장된 세이브 파일이 없습니다. ");
                return;
            }

        }
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (is_fullscreen)
                {
                    camera_list_grid.Visibility = Visibility.Visible;
                    event_grid.Visibility = Visibility.Visible;
                    top_grid.Visibility = Visibility.Visible;
                    bottom_system_bar.Visibility = Visibility.Visible;

                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                }
            }
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            bottom_clock.Text = DateTime.Now.ToString();
        }
        #endregion

        #region event_function
        private void event_search_time_content(object sender, RoutedEventArgs e) //이벤트 검색(예비)   
        {
            //이벤트 검색 누르면~
            Max_Row_Count = -1; //전체 표시
            max_row_combo.SelectedIndex = 4;
            DateTime dts = event_search_start_time.returnDT();
            DateTime dte = event_search_end_time.returnDT();

            if(Search_content.Text != "")
            {
                //내용이 있을 때
                if(dts < dte)
                {
                    string query = make_Query(2);
                    see_some_event(query);
                }
                else if((dts > dte)||(dts==dte))
                {
                    MessageBox.Show("시간 형식이 맞지 않거나 잘못된 입력값 입니다. \n 내용만으로 검색한 결과를 표시합니다.");
                    string query = make_Query(1);
                    see_some_event(query);
                }
            }
            else
            {
                //내용이 없을 때 (시간만으로 검색)
                if (dts < dte)
                {
                    string query = make_Query(0);
                    see_some_event(query);
                }
                else if ((dts > dte) || (dts == dte))
                {
                    MessageBox.Show("시간 형식이 맞지 않거나 잘못된 입력값 입니다.");
                }
            }

        }
        private void Btn_Clear() // 모든 버튼 배경 투명으로 표시
        {
            Btn_c.Background = System.Windows.Media.Brushes.Transparent;
            Btn_p.Background = System.Windows.Media.Brushes.Transparent;
            Btn_f.Background = System.Windows.Media.Brushes.Transparent;
            Btn_s.Background = System.Windows.Media.Brushes.Transparent;
            Btn_l.Background = System.Windows.Media.Brushes.Transparent;
        }
        private void event_search_car(object sender, RoutedEventArgs e)//이벤트 검색 - 자동차
        {
            //차량
            Btn_Clear();
            if(search_Type == 0)
            {
                Btn_c.Background = System.Windows.Media.Brushes.Transparent;
                search_Type = -1;
            }
            else
            {
                search_Type = 0;
                Btn_c.Background = System.Windows.Media.Brushes.DeepSkyBlue;
            }
            string query = make_Query(3);
            see_some_event(query);
        }   
        private void event_search_person(object sender, RoutedEventArgs e)//이벤트 검색 - 사람
        {
            //사람
            Btn_Clear();
            if (search_Type == 1)
            {
                Btn_p.Background = System.Windows.Media.Brushes.Transparent;
                search_Type = -1;
            }
            else
            {
                search_Type = 1;
                Btn_p.Background = System.Windows.Media.Brushes.PeachPuff;
            }
            string query = make_Query(4);
            see_some_event(query);
        }
        private void event_search_fire(object sender, RoutedEventArgs e)//이벤트 검색 - 화재
        {
            //화재
            Btn_Clear();
            if (search_Type == 2)
            {
                Btn_f.Background = System.Windows.Media.Brushes.Transparent;
                search_Type = -1;
            }
            else
            {
                search_Type = 2;
                Btn_f.Background = System.Windows.Media.Brushes.Red;
            }
            string query = make_Query(5);
            see_some_event(query);
        }
        private void event_search_star(object sender, RoutedEventArgs e)//이벤트 검색 - 좋아요
        {
            //좋아요
            Btn_Clear();
            if (search_Type == 3)
            {
                Btn_s.Background = System.Windows.Media.Brushes.Transparent;
                search_Type = -1;
            }
            else
            {
                search_Type = 3;
                Btn_s.Background = System.Windows.Media.Brushes.Yellow;
            }
            string query = make_Query(6);
            see_some_event(query);
        }
        private void event_search_live(object sender, RoutedEventArgs e)//이벤트 검색 - 실시간
        {
            //실시간으로 보기 버튼
            Btn_Clear();
            if (search_Type == 4)
            {
                Btn_l.Background = System.Windows.Media.Brushes.Transparent;
                search_Type = -1;
            }
            else
            {
                search_Type = 4;
                Btn_l.Background = System.Windows.Media.Brushes.LimeGreen;
            }
            string query = make_Query(7);
            see_some_event(query);
        }
        public string make_Query(int event_type) //검색 옵션을 파악하고 원하는 쿼리를 만들어 반환함.
        {
            string result = "";
            string _dts = event_search_start_time.returnDT().ToString("yyyy-MM-dd HH:mm:ss");
            string _dte = event_search_end_time.returnDT().ToString("yyyy-MM-dd HH:mm:ss");
            
            switch (event_type)
            {
                case 0://시간
                    result = "select * from events where strftime('%s', E_time) between strftime('%s', '" + _dts + "') and strftime('%s', '" + _dte + "')"; break;
                case 1://내용
                    result = "select * from events where E_Content like '%" + Search_content.Text + "%'"; break;
                case 2://시간 + 내용
                    result = "select * from events where strftime('%s', E_time) between strftime('%s', '" + _dts + "') and strftime('%s', '" + _dte + "') and E_Content like '%" + Search_content.Text + "%'"; break;
                case 3://차량
                    result = "select * from events where E_kind=0 "; break;
                case 4://사람
                    result = "select * from events where E_kind=1"; break;
                case 5://화재
                    result = "select * from events where E_kind=2"; break;
                case 6://좋아요
                    result = "select * from events where E_star=1"; break;
                case 7://실시간 검색
                    result = "select * from events "; break;
            }

            switch (search_Type)
            {
                case -1: break;
                case 0:
                    result += " and E_kind=0 "; break;
                case 1:
                    result += " and E_kind=1 "; break;
                case 2:
                    result += " and E_kind=2 "; break;
                case 3:
                    result += " and E_star=1 "; break;
                case 4: break;
            }

            return result;
        }
        private void StarBtn(object sender, RoutedEventArgs e) //좋아요 버튼
        {
            //해당 event의 DB 에 E_Star = 1을 넣고 refresh 다시 누르면 E_Star = 0을 넣고 refresh

            ListViewItem lvi = FindParent<ListViewItem>((sender as Button));
            ListView lv = FindParent<ListView>((sender as Button));
            lv.SelectedItem = lvi.DataContext;

            string query = "update events set E_star=((E_star | 1) - (E_star & 1)) where E_index = " + EventList[event_list.SelectedIndex].index;
            database.operate_this_query(query);
            if (EventList[event_list.SelectedIndex].star)
            {
                //true면 
                EventList[event_list.SelectedIndex].Starbtn_color = System.Windows.Media.Brushes.Transparent;
            }
            else
            {
                EventList[event_list.SelectedIndex].Starbtn_color = System.Windows.Media.Brushes.Yellow;
            }
            update_DB();
            //(sender as Button).Background = System.Windows.Media.Brushes.Yellow;
        }

        private void draw_rect_event_received(int cell_x, int cell_y, string[] xywh) //새로 받은 영역 rectangle 칠하기.
        {
            l.Cells[cell_x, cell_y].cell_object_border.Children.Clear();//원래 그림 지우고,
            string[] xywh_ = xywh;
            for (int i = 0; i < xywh_.Length / 4; i++)
            {
                int k = i * 4;
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.StrokeThickness = 5;
                rect.Stroke = new SolidColorBrush(Colors.Yellow);
                rect.Fill = new SolidColorBrush(Colors.Transparent);
                Canvas.SetLeft(rect, double.Parse(xywh_[k]));   //x 좌표
                Canvas.SetTop(rect, double.Parse(xywh_[k + 1]));//y 좌표
                rect.Width = double.Parse(xywh_[k + 2]);        //w 너비
                rect.Height = double.Parse(xywh_[k + 3]);       //h 높이
                //l.Cells[cell_x, cell_y]._cameraitem.GetRelated();//무엇을 가지고 있는지 보기 위해
                l.Cells[cell_x, cell_y].cell_object_border.Width = l.Cells[cell_x, cell_y].cell_canvas_roi.Width;
                l.Cells[cell_x, cell_y].cell_object_border.Height = l.Cells[cell_x, cell_y].cell_canvas_roi.Height;
                if (l.Cells[cell_x, cell_y].isROIon(double.Parse(xywh_[k]), double.Parse(xywh_[k + 1]), double.Parse(xywh_[k + 2]), double.Parse(xywh_[k + 3])))// 차량 좌표가 ROI 좌표 위에 있는지 확인하는 함수
                {
                    rect.Stroke = new SolidColorBrush(Colors.Green);//ROI 안에서 그려지면 초록색으로 표시
                }
                else
                {
                    rect.Stroke = new SolidColorBrush(Colors.Red);//ROI 밖에서 그려지면 빨간색으로 표시
                }
                l.Cells[cell_x, cell_y].cell_object_border.Children.Add(rect);
            }
        }
        private void on_the_ROI_count()
        {

            //얼마나 많이 감지된 차량인지 알려주는 함수
            
        }


        private void event_occur_json(string json) //이벤트가 발생하면 실행할 함수
        {

            //눌린 상태면
            JObject j = JObject.Parse(json);
            if (j["Kind_event"].ToString() == "100")
            {
                CameraCell cc = find_cam_from_FQID_object_id(j["FQID_event"].ToString());
                string[] xywh = j["xywh_event"].ToString().Split(' ');
                draw_rect_event_received(cc.Row, cc.Col, xywh); //rectangle 칠하기
                on_the_ROI_count();//칠한거 count
            }
            else if (j["Kind_event"].ToString() == "500")
            {
                //주정차
                CameraCell cc = find_cam_from_FQID_object_id(j["FQID_event"].ToString());
                string[] xywh = j["xywh_event"].ToString().Split(' ');
                draw_rect_event_received(cc.Row, cc.Col, xywh); //rectangle 칠하기
                on_the_ROI_count();//칠한거 count
            }
            else //j["Kind_event"].ToString() == "0" 
            {
                Event_ e = new Event_(j);// 새로운 event를 json 에서 가지고 옴
                try
                {
                    database.Insert_Row(e.Image_String, e.time, e.content, e.kind, e.fqid); //해당 이벤트를 db에 저장.
                    update_DB();
                    //ROI 영역 판별하고 화면에 차량 boundary 표시하는 부분
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void event_occur_xml(string xml) //이벤트가 발생하면 실행할 함수
        {
            //xml parsing -> truen 카메라임
            double left = 0;
            double top = 0;
            double right = 0;
            double bottom = 0;
            xml = xml.Replace("tt:", string.Empty);//문자열 제거

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList xmlNodeList = doc.GetElementsByTagName("BoundingBox");
            string temp = "";


            //일단 scale 은 640 * 352 기준
            XmlNodeList translate = doc.GetElementsByTagName("Translate");
            XmlNodeList scale = doc.GetElementsByTagName("Scale");


            if (xmlNodeList != null && translate != null && scale != null)
            {
                //double scale_x = double.Parse(scale[0].Attributes["x"].Value);
                //double scale_y = double.Parse(scale[0].Attributes["y"].Value);
                //double meta_image_size_x = Math.Abs(2 / scale_x); //640
                //double meta_image_size_y = Math.Abs(2 / scale_y); //352

                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    left = double.Parse(xmlNodeList[i].Attributes["left"].Value) / 640 * 1920;
                    top = double.Parse(xmlNodeList[i].Attributes["top"].Value) / 352 * 1080;
                    right = double.Parse(xmlNodeList[i].Attributes["right"].Value) / 640 * 1920;
                    bottom = double.Parse(xmlNodeList[i].Attributes["bottom"].Value) / 352 * 1080;

                    temp = temp + left + " " + top + " " + (right - left) + " " + (bottom - top) + " ";
                }
            }
            string[] xywh = temp.Split(' ');
            draw_rect_event_received(2, 2, xywh); //rectangle 칠하기

        }
        private void see_some_event(string query) //db에서 임의의 query에 대해 결과를 받아와 itemsource에 넣어줌.
        {
            EventList.Clear();
            EventList = database.Select_Row(query, order_Row, Max_Row_Count); //100개만 보여준다는 뜻
            event_list.ItemsSource = EventList;
            bottom_system_alert.Text = EventList.Count().ToString() + "개의 이벤트를 표시중입니다.";
        }
        public async void update_DB() //db에서 실행된 마지막 query에 대해 결과를 받아와 itemsource에 넣어줌
        {
            await Task.Run(new Action(() => {

                if (database.last_query != "")
                {
                    
                    //마지막쿼리 보여줌
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                    {
                        EventList = database.Select_Row(database.last_query, order_Row, Max_Row_Count);
                        event_list.BeginInit();
                        event_list.ItemsSource = EventList;
                        event_list.EndInit();
                        bottom_system_alert.Text = EventList.Count().ToString() + "개의 이벤트를 표시중입니다.";

                    }));


                }
                else
                {
                    
                    //실시간 표시해줌
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                    {
                        if (event_list == null)
                            return;
                        EventList = database.Select_Row(make_Query(7), order_Row, Max_Row_Count);
                        event_list.BeginInit();
                        event_list.ItemsSource = EventList;
                        event_list.EndInit();
                        bottom_system_alert.Text = EventList.Count().ToString() + "개의 이벤트를 표시중입니다.";

                    }));
                }
            }));
            
        }
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) //이벤트 listview 선택줄 변경시
        {
            //이벤트 listview 선택줄 변경시
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_metadataLiveSource != null)
            {
                Debug.WriteLine("활성화");
                _metadataLiveSource.LiveModeStart = true;
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_metadataLiveSource != null)
            {
                Debug.WriteLine("비활성화");
                _metadataLiveSource.LiveModeStart = false;
            }
        }        
        private T FindParent<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);
            if (parent == null) return null;
            var parentT = parent as T;
            return parentT ?? FindParent<T>(parent);
        }
        private void delete_event(object sender, RoutedEventArgs e)
        {
            //우클릭시 이벤트 삭제
            string query = "delete from events where E_index = " + EventList[event_list.SelectedIndex].index;
            database.operate_this_query(query);
            update_DB();
        }
        private void see_Detail_event(object sender, RoutedEventArgs e)
        {
            //우클릭시 이벤트 자세히 보기
            if(event_list.SelectedIndex != -1)
            {
                System.Windows.Media.ImageSource i = EventList[event_list.SelectedIndex].image;
                Show_Event_Detail sed = new Show_Event_Detail(i);
                sed.Show();
            }
            else
            {
                MessageBox.Show("데이터를 표시하는 도중 실패했습니다.");
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (combo.SelectedIndex == 0)
            {
                order_Row = "desc";
            }
            else if (combo.SelectedIndex == 1)
            {
                order_Row = "asc";
            }
            update_DB();
        }

        #endregion

        #region Live Click handling
        private void select_meta_channel(object sender, EventArgs e)
        {
            if (_metadataLiveSource != null)
            {
                // Close any current displayed Metadata Live Source
                _metadataLiveSource.LiveContentEvent -= OnLiveContentEvent;
                _metadataLiveSource.Close();
                _metadataLiveSource = null;
            }

            ItemPickerForm form = new ItemPickerForm();
            form.KindFilter = Kind.Metadata;
            form.AutoAccept = true;
            form.Init(Configuration.Instance.GetItems());

            // Ask user to select a metadata device
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _selectItem1 = form.SelectedItem;

                _metadataLiveSource = new MetadataLiveSource(_selectItem1);
                try
                {
                    _metadataLiveSource.LiveModeStart = false;
                    _metadataLiveSource.Init(); //오류
                    _metadataLiveSource.LiveContentEvent += OnLiveContentEvent;
                    _metadataLiveSource.ErrorEvent += OnErrorEvent;

                    _count = 0;
                    event_checkbox.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"Could not Init:" + ex.Message);
                    _metadataLiveSource = null;
                }
            }
            else
            {
                _selectItem1 = null;
                bottom_system_alert.Text = @"Select Metadata device ...";
                event_checkbox.IsEnabled = false;
            }
        }

        /// <summary>
        /// This event is called when some exception has occurred
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exception"></param>
        private void OnErrorEvent(MetadataLiveSource sender, Exception exception)
        {
            if (!CheckAccess())
            {
                // Make sure we execute on the UI thread before updating UI Controls
                Dispatcher.BeginInvoke(new Action(() => OnErrorEvent(sender, exception)));
            }
            else
            {
                // Display the error

                if (exception is CommunicationMIPException)
                {
                    bottom_system_alert.Text = @"Connection lost to server ...";
                }
                else
                {
                    bottom_system_alert.Text = exception.ToString();
                }
            }
        }

        /// <summary>
        /// This event is called when metadata is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnLiveContentEvent(MetadataLiveSource sender, MetadataLiveContent e)
        {
            if (!CheckAccess())
            {
                // Make sure we execute on the UI thread before updating UI Controls
                Dispatcher.BeginInvoke(new Action(() => OnLiveContentEvent(sender, e)));
            }
            else
            {
                if (e.Content != null)
                {
                    // Display the received metadata
                    var metadataXml = e.Content.GetMetadataString(); //날아온 json 파일
                    //xml인지 json인지 확인후 구분해주기
                    metadataXml.Trim();
                    if (metadataXml[0] == '<')
                    {
                        //xml 파일이면
                        event_occur_xml(metadataXml);
                    }
                    else if (metadataXml[0] == '{')
                    {
                        //json 파일임
                        event_occur_json(metadataXml); // 해당 json 파일을 parsing 하여 db에 저장.
                    }
                    else
                    {
                        Console.WriteLine("수신한 메타데이터의 형식을 알 수 없습니다.");
                    }
                    
                }
            }
        }

        private void max_row_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if(combo.SelectedValue.ToString() == "전체")
            {
                //최대 표시 개수가 크면 로딩에 시간이 걸릴 수 있습니다. -> popup 또는 message 창
                Max_Row_Count = -1;
            }
            else
            {
                Max_Row_Count = int.Parse(combo.SelectedValue.ToString());

            }
            update_DB();
        }


        #endregion

        private void parking_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void parking_Item_detail(object sender, RoutedEventArgs e)
        {
            //단속 이벤트 자세히 보기

        }
        #region ringo_bell
        private void scb_Click(object sender, RoutedEventArgs e)
        {
            // 시리얼 포트 연결하는 버튼 
            if (!serial_Port.IsOpen)
            {
                try
                {
                    if (port_name.Text == null)
                    {
                        System.Windows.MessageBox.Show("시리얼 포트를 선택해주세요");
                        return;
                    }
                    serial_Port.PortName = port_name.Text.ToString();
                    System.Console.WriteLine(serial_Port.PortName);
                    serial_Port.BaudRate = 115200;
                    serial_Port.DataBits = 8;
                    serial_Port.StopBits = StopBits.One;
                    serial_Port.Parity = Parity.None;
                    //시리얼 통신 기본 설정.
                    serial_Port.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                    //포트 연결
                    serial_Port.Open();

                    bottom_system_alert.Text = "시리얼 포트에 성공적으로 연결되었습니다.";
                    port_name.IsEnabled = false;
                    serial_connect_Btn.Content = "연결됨";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    System.Windows.MessageBox.Show("시리얼 포트를 선택해주세요");
                }
            }
        }


        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) //데이터 수신시 정리, 답장해줌, bellid 반환
        {
            //데이터가 들어오면
            Bell_Data bd = new Bell_Data((SerialPort)sender);

            if(bd.Option == 0xA501 && bd.check_checksum())//데이터 종류 확인과 checksum 동시 확인
            {
                //pc연결 확인 
                Byte[] b = { 0x03, 0x01, 0x00, 0x08, 0xC5, 0x01, 0x00, 0x2E };
                serial_Port.Write(b, 0, b.Length); //확인 메시지 반송
            }
            else if(bd.Option == 0xA505 && bd.check_checksum())//데이터 종류 확인과 checksum 동시 확인
            {
                //bell data 받음
                Byte[] b = { 0x03, 0x01, 0x00, 0x08, 0xC5, 0x05, 0x00, 0x2A };
                serial_Port.Write(b, 0, b.Length);//확인 메시지 반송

                string bell_id = bd.Bell_ID.ToString("x8");

                if(ss.is_savefile_exist()) // 저장된 파일이 있고,
                {
                    if (ss.have_the_bell_data(bell_id)) // 벨 id가 저장되어있으면,
                    {
                        //띄워짐
                        string loaded_FQID = ss.load_bell_data(bell_id);//저장된 json 파일로 부터 bell_id에 맞는 FQID를 가지고 오는 것
                        CameraCell i = find_cam_from_FQID_object_id(loaded_FQID);
                        if(i != null)
                        {
                            if (single_cc != null)
                            {
                                //띄워져있는게 있거나, 
                                if (single_cc != i)//서로 다를경우
                                {
                                    Dispatcher.BeginInvoke(new Action(() => l.UnsetSingle(single_cc)));
                                    Dispatcher.BeginInvoke(new Action(() => l.SetSingle(i)));
                                    single_cc = i;
                                    //띄워진거 내리고 새로운거 띄움
                                }
                                else if (single_cc == i)//서로 같을경우
                                {
                                    Dispatcher.BeginInvoke(new Action(() => l.UnsetSingle(single_cc)));
                                    single_cc = null;
                                    //띄워진거 내리기만 함
                                }
                            }
                            else //안띄워짐
                            {
                                Dispatcher.BeginInvoke(new Action(() => l.SetSingle(i)));
                                single_cc = i;
                                //띄우고 띄워진것 체크
                            }
                        }
                    }
                    else
                    {
                        //데이터 없음
                        MessageBoxResult result = MessageBox.Show("현재 bell 에 등록된 카메라가 없습니다. 등록하시겠습니까?", "등록", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            //등록
                            //카메라 FQID 값을 받아 와야 함.
                            int x = -1;
                            int y = -1;
                            var camera_picker = new ItemPickerForm();
                            camera_picker.KindFilter = Kind.Camera;
                            camera_picker.AutoAccept = true;
                            camera_picker.Init(Configuration.Instance.GetItems());

                            if (camera_picker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                Item _selectItem = camera_picker.SelectedItem;
                                string FQID_object = _selectItem.FQID.ObjectId.ToString();
                                ss.save_bell_data(bell_id, FQID_object);
                            }

                            MessageBox.Show("등록이 완료되었습니다.");

                        }
                        else
                        {
                            //등록 안함.

                        }
                    }
                }
                else
                {
                    //savefile 없음
                    MessageBox.Show("save file이 없습니다. 설정 저장후 다시 시도해주세요.");
                    //또는 여기서 save파일을 만들자

                }
            }
        }
        private CameraCell find_cam_from_FQID_object_id(string FQID_object_id)
        {
            //벨 데이터랑 카메라 FQID랑 연결하는 부분.
            for (int i = 0; i < l.Row; i++)
            {
                for (int j = 0; j < l.Col; j++)
                {
                    if (l.Cells[i, j]._cameraitem.FQID.ObjectId.ToString() == FQID_object_id)
                    {
                        //같으면 return
                        return l.Cells[i, j];
                    }
                }
            }
            MessageBox.Show("선택한 카메라는 현재 표시되고 있지 않습니다.");
            MessageBox.Show("카메라 object_id : " + FQID_object_id);
            return null;
        }
        #endregion

        private void event_playback_view(object sender, RoutedEventArgs e)
        {
            //이벤트 우클릭을 하면 해당 시점의 playback 영상을 보여준다.

            if (event_list.SelectedIndex != -1)
            {
                Item camera_item = new Item();
                string event_time = EventList[event_list.SelectedIndex].time;
                string fqid = EventList[event_list.SelectedIndex].fqid;
                if(fqid == "0")
                {
                    fqid = l.Cells[0, 0]._cameraitem.FQID.ObjectId.ToString();
                    camera_item = find_cam_from_FQID_object_id(fqid)._cameraitem;

                    if(camera_item != null)
                    {
                        LayoutTest1.Playback_Viewer pb = new LayoutTest1.Playback_Viewer();
                        pb.self_on(camera_item, event_time);
                        pb.ShowDialog();
                    }
                }
                

            }
            else
            {
                MessageBox.Show("데이터를 표시하는 도중 실패했습니다. 선택된 데이터 항목이 없습니다.");
            }
        }

    }
}
