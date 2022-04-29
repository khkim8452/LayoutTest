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
using VideoOS.Platform.Client;
using VideoOS.Platform.UI;

namespace LayoutTest1
{
    public class CameraCellMode
    {
        public const int Unused = 0;
        public const int Blank = 1;
        public const int Show = 2;
    }

    /// <summary>
    /// CameraCell.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CameraCell : UserControl
    {
        int _mode = 0;
        bool _isSingleMode = false;
        bool Maintain_R = false;
        bool ptz_control_mode = false;//ptz 제어 보드 처음에는 안나옴
        bool is_ROI_Mode = false;
        double _ratio;

        public int Mode
        {
            get { return _mode; }
            set { SetMode(value);
            }
        }

        public Item _cameraitem=null;
        public Item CameraItem //property 프로퍼티 
        {
            get { return _cameraitem; }
            set { ChangeCamera(value); }
        }

        bool _isConnected = false;
        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public bool IsSingle = false;
        
        public int Row = 0;
        public int Col = 0;
        public int RowSpan = 1;
        public int ColSpan = 1;
        public CameraCell()
        {
            InitializeComponent();

            Init();
            
        }
        public void Init()
        {
            //_v = new ImageViewerWpfControl();
            
            _v.EnableVisibleHeader = false;
            _v.MaintainImageAspectRatio = false;
            _v.ConnectResponseReceived += _v_ConnectResponseReceived;
            _isConnected = false;
            _v.PlaybackControllerFQID = V.CommonPlaybackFQID;
            ContentGrid.Visibility = Visibility.Collapsed;
            
            this.MouseDoubleClick += CameraCell_MouseDoubleClick;
            this.MouseDown += CameraCell_MouseDown;
        }

        private void CameraCell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Layout.Instance.SelectCamera(this);
        }

        private void CameraCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSingle();

        }

        private void _v_ConnectResponseReceived(object sender, VideoOS.Platform.Client.ConnectResponseEventArgs e)
        {
            Console.WriteLine(e);
        }

        void SetMode(int  mode)
        {
            _mode= mode; ;
            switch(_mode)
            {
                case CameraCellMode.Unused:
                    SetUnused();
                    break;
                case CameraCellMode.Blank:
                    SetBlank();
                    break;
                case CameraCellMode.Show:
                    SetShow();
                    break;


            }
        }

        public void SetUnused()
        {
            CameraDisconnect();
            this.Visibility= Visibility.Collapsed;
            _mode = CameraCellMode.Unused;
        }
        public void SetBlank()
        {
            CameraDisconnect();
            this.ContentGrid.Visibility= Visibility.Collapsed; 
            this.Visibility = Visibility.Visible;
            _mode = CameraCellMode.Blank;
        }
        public void SetShow()
        {
            this.ContentGrid.Visibility=Visibility.Visible;
            this.Visibility = Visibility.Visible;
            _mode = CameraCellMode.Show;
        }
        public void SetSingle()
        {

            if (!IsSingle)
            {
                Layout.Instance.SetSingle(this);
            }else
            {
                Layout.Instance.UnsetSingle(this);
            }
        }
        public void DownloadFromUrl(string url)
        {

        }
        public void CameraConnect()
        {
            try
            {
                if (_cameraitem == null) return;
                _v.Connect();
                _v.StartLive();
                _isConnected = true;

                if (Mode != CameraCellMode.Show)
                {
                    this.SetShow();
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void CameraDisconnect()
        {
            string s = "";
            if (_cameraitem != null) s = _cameraitem.Name;
            _v.Disconnect();
            _isConnected = false;
            //Console.WriteLine($"Disconnect : {s}");
        }
        public void ChangeCamera(Item item)
        {
            _cameraitem = item;
            
            if (item == null)
            {
                this.Mode = CameraCellMode.Blank;
                TitleText.Text = "";

            }
            else
            {
                CameraDisconnect();
                _v.CameraFQID = _cameraitem.FQID;
                TitleText.Text = _cameraitem.Name;
                _v.Initialize();
                CameraConnect();
                Console.WriteLine($"Connected={_cameraitem.FQID}");
                if (V.ShowTitleAlways) ShowTitle();
            }
            

        }
        public void ShowTitle()
        {
            TitleGrid.Visibility = Visibility.Visible;
            TitleText.Visibility = Visibility.Visible;
        }
        public void HideTitle()
        {
            if (!V.ShowTitleAlways)
            {
                TitleGrid.Visibility = Visibility.Collapsed;
                TitleText.Visibility = Visibility.Collapsed;
            }
        }
        private void MouseHoverGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("MouseHover Entered");
            ShowTitle();
            e.Handled = true;
        }

        private void MouseHoverGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("MouseHover Leave");
            HideTitle();
            ToolbarGrid.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

   
        private void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            var f = new ItemPickerForm();
            f.KindFilter = Kind.Camera;
            f.AutoAccept = true;
            f.Init(Configuration.Instance.GetItems());
            
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.CameraItem = f.SelectedItem;
            }
        }

        private void VideoClose_Click(object sender, RoutedEventArgs e)
        {
            this.Mode = CameraCellMode.Blank;
        }

        private void MouseHoverGrid_Drop(object sender, DragEventArgs e)
        {
         
          
                e.Effects = DragDropEffects.None;
            ListBoxItemsBag data = e.Data.GetData(typeof(ListBoxItemsBag)) as ListBoxItemsBag;
            if (data.Count > 0) Layout.Instance.ActivateCameras(data, Row, Col);
                e.Effects = DragDropEffects.Move;
            HighlightGrid.Visibility = Visibility.Collapsed;
        }

        private void MouseHoverGrid_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            HighlightGrid.Visibility = Visibility.Visible;
            

        }

        private void MouseHoverGrid_DragOver(object sender, DragEventArgs e)
        {

        }

        private void MouseHoverGrid_DragLeave(object sender, DragEventArgs e)
        {
            HighlightGrid.Visibility = Visibility.Collapsed;
        }

        private void Maintain_Ratio_Click(object sender, RoutedEventArgs e)
        {
            //bool Maintain_R = false; 전역변수 추가

            if(Maintain_R)//고정상태이면 풀어주기
            {
                Maintain_R = false;
                //MessageBox.Show("이미지 고정을 해제합니다.");
                _v.MaintainImageAspectRatio = false;
            }
            else//풀린 상태이면 고정하기
            {
                Maintain_R = true;
                //MessageBox.Show("이미지를 고정합니다.");
                _v.MaintainImageAspectRatio = true;
            }
        }

        private void ToolbarHoverGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine("ToolbarHoverGrid_MouseEnter Entered");
            ToolbarGrid.Visibility = Visibility.Visible;
        }

        private void ToolbarHoverGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            ToolbarGrid.Visibility = Visibility.Collapsed;
        }

        private void MouseHoverGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(this);

            //마우스가 하단 80% 넘어가면 툴바 영역 표시
            if ( p.Y > this.ActualHeight * 0.8 && Mode==CameraCellMode.Show)
            {
                ToolbarGrid.Visibility = Visibility.Visible;
            }
            else
            {
                ToolbarGrid.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdateTitleStatus()
        {
            if (V.ShowTitleAlways) ShowTitle();
            else HideTitle();
        }


        private void activate_PTZ(object sender, RoutedEventArgs e)
        {
            //PTZ 컨트롤을 활성화 하고 비활성화 합니다.
            if (_cameraitem.Name.Contains("PTZ")) // 카메라 이름에 PTZ 글자가 들어가야 실행
            {
                ptzptz.set_ptz_item(this.CameraItem);

                if (ptz_control_mode)
                {
                    ptz_control_mode = false;//ptz 비활성화
                    PTZGrid.Visibility = Visibility.Collapsed;

                }
                else
                {
                    ptz_control_mode = true;//ptz 활성화
                    PTZGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                MessageBox.Show("PTZ 카메라가 아닙니다.");
            }
        }

        private void activate_ROI(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(_v.ImageSize.Height.ToString());
            //MessageBox.Show(_v.ActualHeight.ToString());
            //MessageBox.Show(_v.ImageSize.ToString());

            LayoutTest1.Set_ROI roi = new LayoutTest1.Set_ROI(this.CameraItem);
            roi.ShowDialog();
        }
    }
}
