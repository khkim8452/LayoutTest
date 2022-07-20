using System;
using System.IO;
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
using System.Collections.ObjectModel;
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
        ObservableCollection<DrawROI> ROIs_list = new ObservableCollection<DrawROI>();
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
            _v.ImageOrPaintInfoChanged += _v_ImageOrPaintInfoChanged;
            _isConnected = false;
            _v.PlaybackControllerFQID = V.CommonPlaybackFQID;
            ContentGrid.Visibility = Visibility.Collapsed;

            this.MouseDoubleClick += CameraCell_MouseDoubleClick;
            this.MouseDown += CameraCell_MouseDown;
        }

        private void _v_ImageOrPaintInfoChanged(object sender, ImageOrPaintInfoChangedEventArgs e)
        {
            cell_object_border.Width = _v.ImageSize.Width;
            cell_object_border.Height = _v.ImageSize.Height;
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
                update_ROI();
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
            update_ROI();
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
                cell_view_roi.Stretch = Stretch.Fill;
                cell_view_object.Stretch = Stretch.Fill;
            }
            else//풀린 상태이면 고정하기
            {
                Maintain_R = true;
                //MessageBox.Show("이미지를 고정합니다.");
                _v.MaintainImageAspectRatio = true;
                cell_view_roi.Stretch = Stretch.Uniform; 
                cell_view_object.Stretch= Stretch.Uniform;
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
            if (_cameraitem.Name.Contains("")) // 카메라 이름에 PTZ 글자가 들어가야 실행
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
            LayoutTest1.Set_ROI roi = new LayoutTest1.Set_ROI(this.CameraItem);
            roi.ShowDialog(); //오류
                              // 창만 닫은 것이고, roi 객체 자체는 아직 사라진게 아님.
                              // roi에 접근해서 가지고오기.

            update_ROI();
        }

        public void update_ROI()
        {
            //현재 카메라 셀에 ROI를 최신 버전으로 업데이트 하여 표시하고자 함.
            //저장된걸 가지고 오기
            save_ROI save_roi = new save_ROI();
            save_roi.set_path(System.IO.Directory.GetCurrentDirectory() + _v.CameraFQID.ObjectId);
            ROIs_list = new ObservableCollection<DrawROI>();
            cell_canvas_roi.Children.Clear();
            if (save_roi.is_savefile_exist()) //세이브파일이 있으면
            {
                ROIs_list = save_roi.load_ROI_list(); //세이브 파일에 저장된 리스트 원래 형식대로 가지고 오기
                load_canvas(ROIs_list); // 리스트에 있는 객체들 canvas에 children 속성으로 그리기.
                cell_canvas_roi.Width = ROIs_list[0]._width;
                cell_canvas_roi.Height = ROIs_list[0]._height;
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
                    cell_canvas_roi.Children.Add(ROIs_list[i]);
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
            Point p00 = new Point(x, y);            Point p01 = new Point(x + w / 2, y);            Point p02 = new Point(x + w, y);
            Point p10 = new Point(x, y + h / 2);    Point p11 = new Point(x + w / 2, y + h / 2);    Point p12 = new Point(x + w, y + h / 2);
            Point p20 = new Point(x, y + h);        Point p21 = new Point(x + w / 2, y + h);        Point p22 = new Point(x + w, y + h);

            int count = 0;

            for(int i = 0; i< this.ROIs_list.Count; i++) //ROI 개수
            {
                Point[] points = new Point[ROIs_list[i].ROI_Points.Count];
                for (int j = 0; j < ROIs_list[i].ROI_Points.Count;j++)//point 수 
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
                if(count >= 4)
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

        private void Get_Related(object sender, RoutedEventArgs e)
        {
             this._cameraitem.GetRelated();
        }
    }
}
