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
using VideoOS.Platform;
using VideoOS.Platform.SDK.UI.LoginDialog;
using System.IO;
using System.Windows.Threading;

// JSON 파일 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        save_setting ss = new save_setting();
        Layout l = null;
        List<Item> CameraList = null;
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
            _loginButton_Click();
            CameraList=GetCameraList();
            FillCameraListBox();
            this.KeyDown += new KeyEventHandler(HandleEsc);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //여기서 왜 오류나냐? 시작하자마자 끄니까 오류나네
            camera_view_grid.Children.Add(l.MainGrid);
            l.SetRowCol(default_rowcol, default_rowcol);
            LoadAllCamera(null, null);
        }

        public List<Item> GetCameraList()
        {
            var q=Configuration.Instance.GetItemsSorted();
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
            foreach(Item item in CameraList)
            {
                ListBoxItem i = new ListBoxItem();
                i.Content = item.Name;
                i.Tag = item;
                camera_list_box.Items.Add(i);
            }
        }
        private void _loginButton_Click()
        {
            var loginForm = new DialogLoginForm(SetLoginResult, IntegrationId, IntegrationName, Version, ManufacturerName);
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
            if(Layout.Instance.IsSingle)
            {
                return;
            }
            for(int i = 0; i < CameraList.Count; i++)
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

        private void CameraListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
        }
        bool DND_MouseDown = false;
       Point DND_StartPoint;
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

            if(e.LeftButton==MouseButtonState.Pressed &&(
                Math.Abs(diff.X) > 5 &&
                Math.Abs(diff.Y) >5))
            {
                StartDrag();
            }

        }
        public void StartDrag()
        {
            ListBoxItemsBag b = new ListBoxItemsBag();
            foreach(ListBoxItem i in camera_list_box.SelectedItems)
            {
                b.Bag.Add(i);
            }

            //List<ListBoxItem> data = new List<ListBoxItem>();
            

            if (b != null && b.Bag.Count>0)
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
            bottom_system_bar.Visibility=Visibility.Collapsed;
            this.WindowStyle = WindowStyle.None;
            
        }

        private void Save_Settings(object sender, RoutedEventArgs e)
        {
            //설정 저장을 위해 json 파일에 현재 설정값을 저장하는 버튼
            ss.save_MainWindow(l.Row, is_fullscreen);
        }

        private void CreateJson(string path)
        {
            if(!File.Exists(path))
            {
                using (File.Create(path))
                {
                    MessageBox.Show("파일 생성 성공.");
                }
            }
            else
            {
                MessageBox.Show("이미 파일이 존재합니다.");
            }
        }
        private void WriteJson()
        {
            string path = @"C:\Users\9junb\source\repos\LayoutTest\saved_settings.json";

            if(File.Exists(path))
            {
                InputJson(path);
            }
            else
            {
                CreateJson(path);
            }
        }
        private void InputJson(string path)
        {
            //사용자 정보 배열로 선언
            var users = new[] { "USER1", "USER2", "USER3", "USER4" };

            JObject dbSpec = new JObject(
                new JProperty("a", "1"),
                new JProperty("b", "0"),
                new JProperty("c", "1"),
                new JProperty("d", "1"),
                new JProperty("e", "0")
                );

            //Jarray 로 추가
            dbSpec.Add("USERS", JArray.FromObject(users));

            File.WriteAllText(path, dbSpec.ToString());

        }

        private void PlayBack_Viewer(object sender, RoutedEventArgs e)
        {
            LayoutTest1.Playback_Viewer pb = new LayoutTest1.Playback_Viewer();
            pb.ShowDialog();
        }

        private void load_mainwindow()
        {
            if(ss.is_savefile_exist())
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
                if(is_fullscreen)
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
    }
}
