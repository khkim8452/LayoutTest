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
using VideoOS.Platform.Live;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Speech.Synthesis;

namespace LayoutTest1
{
    /// <summary>
    /// LPR_window.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LPR_window : Window
    {
        ObservableCollection<Event_> EventList = new ObservableCollection<Event_>();
        private MetadataLiveSource _metadataLiveSource; //메타 채널
        Item cameraItem;//카메라 Item
        bool ratio_ = false;//비율 고정

        //ROI
        ObservableCollection<DrawROI> ROIs_list = new ObservableCollection<DrawROI>();
        save_ROI save_roi;

        //주정차 알고리즘
        LPR_Stacked_Car[] Stacked_Car_List = { };
        double crack_down_time = 120.0;

        //Database
        LPR_SQLite new_sql = new LPR_SQLite(); //SQL 데이터
        int Max_Row_Count = 200; //최대 검색 개수 -1이면 전체 보여줌.

        //TTS
        SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        public LPR_window(Item cam, Item meta)
        {
            InitializeComponent();
            //시간 표시용 timer 초기화
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Clock_Tick;
            timer.Start();
            //ROI 객체 생성
            cameraItem = cam;
            save_roi = new save_ROI();
            save_roi.set_path(System.IO.Directory.GetCurrentDirectory() + cam.FQID.ObjectId);
            update_ROI();//ROI 창 닫으면 LPR_window에 업데이트 반영 save_ROI에서 저장된 data 가지고 옴.
            //카메라 세팅
            LPR_camera.EnableVisibleHeader = false;
            LPR_camera.MaintainImageAspectRatio = false;
            LPR_camera.CameraFQID = cam.FQID;
            LPR_camera.ImageOrPaintInfoChanged += LPR_camera_ImageOrPaintInfoChanged;
            LPR_camera.Initialize();
            LPR_camera.Connect();
            LPR_camera.StartLive();
            //SQL 초반 불러오기
            EventList = new_sql.Select_Row("select * from events", Max_Row_Count);
            //이벤트 리스트 세팅
            LPR_event_list.BeginInit();
            LPR_event_list.ItemsSource = EventList;
            LPR_event_list.EndInit();
            LPR_current_car.BeginInit();
            LPR_current_car.ItemsSource = Stacked_Car_List;
            LPR_current_car.EndInit();
            //데이터 감지후 삭제용 Thread
            Thread t = new Thread(new ThreadStart(auto_delete_LPR));
            t.Start();
            
            System_.Text = System_.Text + "기존 데이터를 불러오는데 성공하였습니다.";
            //TTS 세팅
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            speechSynthesizer.SelectVoice("Microsoft Heami Desktop");

            _metadataLiveSource = new MetadataLiveSource(meta);
            try
            {
                _metadataLiveSource.LiveModeStart = true;
                _metadataLiveSource.Init(); //오류
                _metadataLiveSource.LiveContentEvent += OnLiveContentEvent; //메타 데이터 받는 이벤트
                _metadataLiveSource.ErrorEvent += OnErrorEvent;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Could not Init:" + ex.Message);
                _metadataLiveSource = null;
            }
            
        }
        #region System
        private void Clock_Tick(object sender, EventArgs e)
        {
            Datetime_.Text = DateTime.Now.ToString();
        }
        private void LPR_camera_ImageOrPaintInfoChanged(object sender, VideoOS.Platform.Client.ImageOrPaintInfoChangedEventArgs e)
        {
            BOX_canvas.Width = LPR_camera.ImageSize.Width;
            BOX_canvas.Height = LPR_camera.ImageSize.Height;
            ROI_canvas.Width = LPR_camera.ImageSize.Width;
            ROI_canvas.Height = LPR_camera.ImageSize.Height;
        }
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
                    
                    event_occur_json(metadataXml); // 해당 json 파일을 parsing 하여 db에 저장.

                }
            }
        }
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
                    System_.Text = @"Connection lost to server ...";
                }
                else
                {
                    System_.Text = exception.ToString();
                }
            }
        }
        private void LPR_RATIO_Click(object sender, RoutedEventArgs e)
        {
            if (ratio_)//고정상태이면 풀어주기
            {
                ratio_ = false;
                LPR_camera.MaintainImageAspectRatio = false;
                roi_event.Stretch = Stretch.Fill;
                box_event.Stretch = Stretch.Fill;
            }
            else//풀린 상태이면 고정하기
            {
                ratio_ = true;
                LPR_camera.MaintainImageAspectRatio = true;
                roi_event.Stretch = Stretch.Uniform;
                box_event.Stretch = Stretch.Uniform;
            }
        }
        private void Change_crack_down_timer(object sender, RoutedEventArgs e)
        {
            try
            {
                this.crack_down_time = double.Parse(set_crack_down.Text);
                System_.Text = System_.Text + "\n" + $"주차 단속 기준이 {crack_down_time}초로 수정되었습니다.";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }


        private void event_occur_json(string json) //이벤트가 발생하면 실행할 함수
        {
            JObject jobj = JObject.Parse(json); //데이터 도착
            if(json == "{}")
            {
                return;
            }
            if (jobj["Kind_event"].ToString() == "100") // box data
            {
                //데이터가 들어오면
                string[] License = jobj["Car_number_event"].ToString().Split(' ');
                string[] xywh = jobj["xywh_event"].ToString().Split(' ');
                string[] plate_xy = jobj["plate_event"].ToString().Split(' ');
                string[] img = jobj["img_event"].ToString().Trim().Split(' ');

                //rectangle 표시
                draw_rect_event_received(xywh,License,plate_xy); //rectangle 칠하기
                
                //들어온 데이터와 쌓인 데이터를 비교 -> 기존에 있으면 표시, 없으면 새로 추가
                for(int i = 0; i < License.Length; i++)
                {
                    int count_xywh = i * 4;
                    int x = int.Parse(xywh[count_xywh + 0]);
                    int y = int.Parse(xywh[count_xywh + 1]);
                    int w = int.Parse(xywh[count_xywh + 2]);
                    int h = int.Parse(xywh[count_xywh + 3]);

                    bool list_exist = false;
                    for (int j = 0; j < Stacked_Car_List.Length; j++) //들어온 차번 하나하나 마다(i) 저장된번호판 하나하나(j)비교
                    {

                        if (Stacked_Car_List[j].is_same_license(License[i])) 
                        {
                            //.is_same_license 함수는 같은 번호판이면, 새로들어온 정보를 다시 쌓아 저장한다.
                            //같은 번호면 
                            Stacked_Car_List[j].update_main_license(License[i]);//대표 번호판 업데이트     같은 번호판이 있음.
                            list_exist = true;

                            if (Stacked_Car_List[j].is_same_position(x, y, w, h)) //.is_same_position 함수는 움직임,정차 상태를 판단하여 bool형식으로 가지고 온다.
                            {
                                // 확실한 주정차 (10프레임 이상)
                                if (Stacked_Car_List[j].stop_state)
                                {
                                    TimeSpan ts = DateTime.Now - Stacked_Car_List[j].first_stop_time;  // 정차한 차량을 찾음.
                                    if (ts.TotalSeconds > crack_down_time) //5분
                                    {
                                        //확실한 단속 
                                        //SQL 저장
                                        if (!Stacked_Car_List[j].is_cracked_down && isROIon(x, y, w, h)) // 5분이상 정차했을 때.
                                        {
                                            Stacked_Car_List[j].is_cracked_down = true; //이전에 걸린적 없고, ROI안에 있어.
                                            try
                                            {
                                                //화면 효과 추가 요망

                                                //TTS
                                                //speechSynthesizer.Speak("자동차가 단속되었습니다."); //말하는 도중 프로그램 멈춤
                                                //System Alert
                                                System_.Text = System_.Text + "\n" + $"{Stacked_Car_List[j].Main_Lincense} 자동차가 단속되었습니다.";

                                                new_sql.Insert_Row(img[i], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Stacked_Car_List[j].main_Lincense, 0, LPR_camera.CameraFQID.ObjectId.ToString()); //해당 이벤트를 db에 저장할거야.
                                                EventList = new_sql.Select_Row("select * from events ", Max_Row_Count);
                                                LPR_event_list.ItemsSource = EventList;

                                                //ROI 영역 판별하고 화면에 차량 boundary 표시하는 부분
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine(ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //움직임
                            }
                            break; //찾았으니 다음걸로 넘어가
                        }

                    }
                    if (!list_exist)//안들어있으면           
                    {
                        //새로운 차 => 추가
                        //차량 이미지 형변환
                        byte[] data = Convert.FromBase64String(img[i]);
                        BitmapImage bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = new System.IO.MemoryStream(data);
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();

                        LPR_Stacked_Car new_stack = new LPR_Stacked_Car(License[i], x, y, w, h, bmp); //같은 번호판이 없음... 저장 안되어있어서 추가
                        Stacked_Car_List = Stacked_Car_List.Append(new_stack).ToArray();
                    }
                }

                ////쌓인 데이터 중 최근 (20초) 데이터가 없는 데이터 삭제 
                //for(int i = 0; i < Stacked_Car_List.Length; i++)
                //{
                //    DateTime early = Stacked_Car_List[i].last_receive_time;
                //    DateTime lately = DateTime.Now;
                //    TimeSpan ts = lately - early;
                //    if (ts.TotalSeconds > 20)
                //    {
                //        //삭제하기
                //        Stacked_Car_List[i].Abort(); // thread 삭제
                //        Stacked_Car_List[i] = null;
                //    }
                //}

                ////삭제후 비어있는 배열 index 삭제 빈 값 삭제
                //Stacked_Car_List = Stacked_Car_List.Where(x => x != null).ToArray();

            }
            //==========================================================================================================
            else if (jobj["Kind_event"].ToString() == "500") //주정차 data
            {
                //Event_ e = new Event_(jobj);// 새로운 event를 json 에서 가지고 옴
                //try
                //{
                //    license_.Text = e.content;
                //    new_sql.Insert_Row(e.Image_String, e.time, e.content, e.kind, e.fqid); //해당 이벤트를 db에 저장.
                //    update_DB();
                //    //ROI 영역 판별하고 화면에 차량 boundary 표시하는 부분
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
            }

        }

        private void draw_rect_event_received(string[] xywh, string[] license, string[] plate_xy) //새로 받은 영역 rectangle 칠하기.
        {
            int count_xywh = 0;
            BOX_canvas.Children.Clear();//원래 그림 지우고

            for (int i = 0; i < xywh.Length / 4; i++)
            {
                count_xywh = i * 4;
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle(); //새로운 rect box
                rect.StrokeThickness = 5;
                if (isROIon(double.Parse(xywh[count_xywh]), double.Parse(xywh[count_xywh + 1]), double.Parse(xywh[count_xywh + 2]), double.Parse(xywh[count_xywh + 3])))// 차량 좌표가 ROI 좌표 위에 있는지 확인하는 함수
                {
                    rect.Stroke = new SolidColorBrush(Colors.Green);//ROI 안에서 그려지면 초록색으로 표시
                }
                else
                {
                    rect.Stroke = new SolidColorBrush(Colors.Red);//ROI 밖에서 그려지면 빨간색으로 표시
                }
                rect.Fill = new SolidColorBrush(Colors.Transparent);
                Canvas.SetLeft(rect, double.Parse(xywh[count_xywh]));   //x 좌표
                Canvas.SetTop(rect, double.Parse(xywh[count_xywh + 1]));//y 좌표
                rect.Width = double.Parse(xywh[count_xywh + 2]);        //w 너비
                rect.Height = double.Parse(xywh[count_xywh + 3]);       //h 높이

                //===============================================================================================================
                Grid panel = new Grid();

                System.Windows.Shapes.Rectangle rect_license = new System.Windows.Shapes.Rectangle(); //새로운 번호판
                rect_license.Width = 200;
                rect_license.Height = 50;
                rect_license.Stroke = Brushes.Black;
                rect_license.StrokeThickness = 3;
                rect_license.Fill = Brushes.White;
                
                TextBlock text = new TextBlock();
                text.Text = license[i];
                text.FontSize = 30;
                text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                text.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                
                panel.Children.Add(rect_license);
                panel.Children.Add(text);
                
                Canvas.SetLeft(panel, double.Parse(plate_xy[count_xywh]) + (double.Parse(plate_xy[count_xywh + 2]) / 2) - 100);
                Canvas.SetTop(panel, double.Parse(plate_xy[count_xywh + 1]) + double.Parse(plate_xy[count_xywh + 3]));
                
                BOX_canvas.Children.Add(rect);
                BOX_canvas.Children.Add(panel);
            }

        }
        private void auto_delete_LPR()
        {

            //쌓인 데이터 중 최근 (20초) 데이터가 없는 데이터 삭제 
            while(true)
            {
                //lock(Stacked_Car_List)
                //{
                //    for (int i = 0; i < Stacked_Car_List.Length; i++)
                //    {
                //        if (Stacked_Car_List[i].flag2)
                //        {
                //            //삭제하기
                //            Stacked_Car_List[i].Abort(); // thread 삭제
                //            Stacked_Car_List[i] = null;
                //            //삭제후 비어있는 배열 index 삭제 빈 값 삭제
                //            Stacked_Car_List = Stacked_Car_List.Where(x => x != null).ToArray();
                //        }
                //    }

                //    //최종 정리된 데이터 listview 에 표시
                //    Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                //    {
                //        LPR_current_car.ItemsSource = Stacked_Car_List;//끊김.
                //    }));
                //}
                Thread.Sleep(100);
            }

        }
        #endregion
        #region Search_DB

        private void event_search_time_content(object sender, RoutedEventArgs e) //이벤트 검색(예비)   
        {
            //이벤트 검색 누르면~
            Max_Row_Count = -1; //전체 표시
            DateTime dts = event_search_start_time.returnDT();
            DateTime dte = event_search_end_time.returnDT();

            if (Search_content.Text != "")
            {
                //내용이 있을 때
                if (dts < dte)
                {
                    string query = "select * from events where strftime('%s', E_time) between strftime('%s', '" + dts + "') and strftime('%s', '" + dte + "') and E_Content like '%" + Search_content.Text + "%'";
                    see_some_event(query);
                }
                else if ((dts > dte) || (dts == dte))
                {
                    string query = "select * from events where E_Content like '%" + Search_content.Text + "%'";
                    see_some_event(query);
                }
            }
            else
            {
                //내용이 없을 때 (시간만으로 검색)
                if (dts < dte)
                {
                    string query = "select * from events where strftime('%s', E_time) between strftime('%s', '" + dts + "') and strftime('%s', '" + dte + "')";
                    see_some_event(query);
                }
                else if ((dts > dte) || (dts == dte))
                {
                    MessageBox.Show("시간 형식이 맞지 않거나 잘못된 입력값 입니다.");
                }
            }

        }
        private void see_some_event(string query) //db에서 임의의 query에 대해 결과를 받아와 itemsource에 넣어줌.
        {
            EventList.Clear();
            EventList = new_sql.Select_Row(query, Max_Row_Count); //100개만 보여준다는 뜻
            LPR_event_list.ItemsSource = EventList;
        }

        private void see_all_events(object sender, RoutedEventArgs e)
        {
            EventList.Clear();
            string query = "select * from events";
            EventList = new_sql.Select_Row(query, Max_Row_Count); //100개만 보여준다는 뜻
            LPR_event_list.ItemsSource = EventList;
        }

        #endregion
        #region LPR_Algorithm

        #endregion
        #region mouse_right_click
        private void LPR_event_playback_view(object sender, RoutedEventArgs e)
        {
            //이벤트 우클릭을 하면 해당 시점의 playback 영상을 보여준다.

            if (LPR_event_list.SelectedIndex != -1)
            {
                string event_time = EventList[LPR_event_list.SelectedIndex].time;
                LayoutTest1.Playback_Viewer pb = new LayoutTest1.Playback_Viewer();
                pb.self_on(this.cameraItem, event_time);
                pb.ShowDialog();
            }
            else
            {
                MessageBox.Show("데이터를 표시하는 도중 실패했습니다. 선택된 데이터 항목이 없습니다.");
            }
        }

        private void LPR_event_deatil_view(object sender, RoutedEventArgs e)
        {
            //우클릭시 이벤트 자세히 보기
            if (LPR_event_list.SelectedIndex != -1)
            {
                System.Windows.Media.ImageSource i = EventList[LPR_event_list.SelectedIndex].image;
                Show_Event_Detail sed = new Show_Event_Detail(i);
                sed.Show();
            }
            else
            {
                MessageBox.Show("데이터를 표시하는 도중 실패했습니다.");
            }
        }

        
        #endregion
        #region ROI
        private void LPR_ROI_Click(object sender, RoutedEventArgs e)
        {
            Set_ROI roi = new Set_ROI(this.cameraItem);
            roi.ShowDialog();
            update_ROI();//ROI 창 닫으면 LPR_window에 업데이트 반영 save_ROI에서 저장된 data 가지고 옴.
        }
        public void update_ROI()
        {
            ROIs_list.Clear();
            ROI_canvas.Children.Clear();
            if (save_roi.is_savefile_exist()) //세이브파일이 있으면
            {
                ROIs_list = save_roi.load_ROI_list(); //세이브 파일에 저장된 리스트 원래 형식대로 가지고 오기
                load_canvas(ROIs_list); // 리스트에 있는 객체들 canvas에 children 속성으로 그리기.
            }

        }
        private void load_canvas(ObservableCollection<DrawROI> ROIs_list)
        {
            try
            {
                for (int i = 0; i < ROIs_list.Count; i++)
                {
                    ROIs_list[i].setRatio(ROIs_list[0]._height, ROIs_list[0]._width);
                    ROIs_list[i].load_and_draw_(0); //DrawROI 객체 안에서 ROI_paper canvas 안에 추가하는 작업
                    ROI_canvas.Children.Add(ROIs_list[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public bool isROIon(double x, double y, double w, double h) //poly 안에 box가 50 % 이상 차있으면, true를 반환
        {
            //하나의 box를 가지고 와서 모든 roi와 비교 해야한다. 하나라도 5개 이상 겹치면 true 아니면 false를 반환한다.
            Point p00 = new Point(x, y); Point p01 = new Point(x + w / 2, y); Point p02 = new Point(x + w, y);
            Point p10 = new Point(x, y + h / 2); Point p11 = new Point(x + w / 2, y + h / 2); Point p12 = new Point(x + w, y + h / 2);
            Point p20 = new Point(x, y + h); Point p21 = new Point(x + w / 2, y + h); Point p22 = new Point(x + w, y + h);

            int count = 0;

            for (int i = 0; i < this.ROIs_list.Count; i++) //ROI 개수
            {
                Point[] points = new Point[ROIs_list[i].ROI_Points.Count];
                for (int j = 0; j < ROIs_list[i].ROI_Points.Count; j++)//point 수 
                {
                    if (ROIs_list[i].isvisible == true)
                    {
                        //보이는거면 하기
                        points[j] = ROIs_list[i].ROI_Points[j];//형변환
                    }
                }
                if (IsInPolygon(points, p00)) { count++; }
                if (IsInPolygon(points, p01)) { count++; }
                if (IsInPolygon(points, p02)) { count++; }
                if (IsInPolygon(points, p10)) { count++; }
                if (IsInPolygon(points, p11)) { count++; }
                if (IsInPolygon(points, p12)) { count++; }
                if (IsInPolygon(points, p20)) { count++; }
                if (IsInPolygon(points, p21)) { count++; }
                if (IsInPolygon(points, p22)) { count++; }
                if (count >= 4)
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }
            return false;
        }
        public static bool IsInPolygon(Point[] poly, Point p) //p 가 poly안에 있으면 true, 없으면 false
        {
            Point p1, p2;
            bool inside = false;

            if (poly.Length < 3)
            {
                return inside;
            }

            var oldPoint = new Point(
                poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                var newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }
        #endregion

    }
}
