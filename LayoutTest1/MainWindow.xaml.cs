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
using System.Data.SQLite;

using VideoOS.Platform;
using VideoOS.Platform.Live;
using VideoOS.Platform.UI;
using System.Diagnostics;
using System.Globalization;


// JSON 파일 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Drawing;

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

            //loginForm.AutoLogin = false;
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
                int a = ss.load_MainWindow_1();
                bool b = ss.load_MainWindow_2();
                default_rowcol = a;
                is_fullscreen = b;

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
        private void event_search_car(object sender, RoutedEventArgs e)//이벤트 검색 - 자동차
        {
            //차량 
            string query = make_Query(3);
            see_some_event(query);
        }   
        private void event_search_person(object sender, RoutedEventArgs e)//이벤트 검색 - 사람
        {
            //사람
            string query = make_Query(4);
            see_some_event(query);
        }
        private void event_search_fire(object sender, RoutedEventArgs e)//이벤트 검색 - 화재
        {
            //화재
            string query = make_Query(5);
            see_some_event(query);
        }
        private void event_search_star(object sender, RoutedEventArgs e)//이벤트 검색 - 좋아요
        {
            //좋아요
            string query = make_Query(6);
            see_some_event(query);
        }
        private void event_search_live(object sender, RoutedEventArgs e)//이벤트 검색 - 실시간
        {
            //실시간으로 보기 버튼
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
                    result = "select * from events where E_kind=0"; break;
                case 4://사람
                    result = "select * from events where E_kind=1"; break;
                case 5://화재
                    result = "select * from events where E_kind=2"; break;
                case 6://좋아요
                    result = "select * from events where E_star=1"; break;
                case 7://실시간 검색
                    result = "select * from events order by E_index desc"; break;
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
        private void event_occur(string json) //이벤트가 발생하면 실행할 함수
        {
            //눌린 상태면
            JObject j = JObject.Parse(json);
            Event_ e = new Event_(j);// 새로운 event를 json 에서 가지고 옴
            try
            {
                database.Insert_Row(e.Image_String, e.time, e.content, e.kind); //해당 이벤트를 db에 저장.
                update_DB();//새로운 이벤트가 발생할때마다 list 업데이트 (다시 검색해서 refresh 함)
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void see_some_event(string query) //db에서 임의의 query에 대해 결과를 받아와 itemsource에 넣어줌.
        {
            //EventList.Clear();
            EventList = database.Select_Row(query, Max_Row_Count); //100개만 보여준다는 뜻

            //event_list.ItemsSource = EventList;
            bottom_system_alert.Text = EventList.Count().ToString() + "개의 이벤트를 표시중입니다.";
        }
        public void update_DB() //db에서 실행된 마지막 query에 대해 결과를 받아와 itemsource에 넣어줌
        {
            if(database.last_query != "")
            {
                //마지막쿼리 보여줌
                EventList = database.Select_Row(database.last_query, Max_Row_Count);
                //event_list.ItemsSource = EventList;
                bottom_system_alert.Text = EventList.Count().ToString() + "개의 이벤트를 표시중입니다.";
            }
            else
            {
                //실시간 표시해줌
                EventList = database.Select_Row(make_Query(7), Max_Row_Count);
                //event_list.ItemsSource = EventList;
                //bottom_system_alert.Text = EventList.Count().ToString() + "개의 이벤트를 표시중입니다.";
            }
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
            try
            {
                System.Windows.Media.ImageSource i = EventList[event_list.SelectedIndex].image;
                Show_Event_Detail sed = new Show_Event_Detail(i);
                sed.Show();
            }
            catch
            {

            }
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
                    event_occur(metadataXml); // 해당 json 파일을 parsing 하여 db에 저장.
                }
            }
        }

        private void max_row_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            Max_Row_Count = int.Parse(combo.SelectedValue.ToString());
            update_DB();
        }
        #endregion

    }
}
