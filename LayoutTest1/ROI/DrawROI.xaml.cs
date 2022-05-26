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
using System.ComponentModel;


namespace LayoutTest1
{
    /// <summary>
    /// DrawROI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DrawROI : UserControl , INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public double _width;
        public double _height;

        private string Name;
        private Brush Main_Color;
        private bool Isvisible;
        public string name 
        { 
            get
            { 
                return Name; 
            }
            set
            {
                this.Name = value;
                OnPropertyChanged("name");
            }
        
        }
        public Brush main_color
        {
            get { return Main_Color; }
            set
            {
                this.Main_Color = value;
                OnPropertyChanged("main_color");
            }
        }
        public bool isvisible
        {
            get { return Isvisible; }
            set
            {
                this.Isvisible = value;
                OnPropertyChanged("isvisible");
            }
        }

        public List<Ellipse> ROI_Ellipse = new List<Ellipse>();// 점 list  (점 객체 저장)
        public List<Line> ROI_Lines = new List<Line>();// 선 list (선 객체 저장)
        public List<Point> ROI_Points = new List<Point>();// 좌표 list (모든 좌표 저장)
        public Line Close_line = new Line();//시작점과 연결된 닫는 선

        public DrawROI()
        {
            InitializeComponent();
            //  DataContext = this;
        }
        
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            change__();
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        private void change__()
        {
            for(int i = 0; i < ROI_Ellipse.Count; i++)
            {
                ROI_Ellipse[i].Stroke = Main_Color;
                ROI_Ellipse[i].Fill = Main_Color;
            }
            for(int i = 0; i < ROI_Lines.Count; i++)
            {
                ROI_Lines[i].Stroke = Main_Color;
            }
            Close_line.Stroke = Main_Color;
        }

        public void setRatio(double height, double width)
        {
            //Canvas의 가로세로 높이를 받아와 설정.
            Top_Canvas_name.Width = width;
            Top_Canvas_name.Height = height; 
            _height = height;
            _width = width;

        }


        public void draw_point(Point p)
        {
            //점 찍는 것
            Ellipse new_ellipse = new Ellipse();
            new_ellipse.Stroke = main_color;
            new_ellipse.Fill = main_color;
            new_ellipse.Width = 14;
            new_ellipse.Height = 14;
            Canvas.SetLeft(new_ellipse, p.X -7);
            Canvas.SetTop(new_ellipse, p.Y -7); 

            ROI_Ellipse.Add(new_ellipse);
            ROI_Points.Add(p);


            ROI_paper.Children.Add(new_ellipse);//그리기
        }

        public void draw_line()
        {
            //선 그리는 것
            //2개 이상일 때 부터 선을 그림.
            if ((ROI_Ellipse.Count > 1) && (ROI_Points.Count > 1))
            {
                int point_count = ROI_Points.Count(); //점의 갯수

                Line new_line = new Line();
                Point now_point = ROI_Points[point_count - 1];
                Point preview_point = ROI_Points[point_count - 2];


                new_line.Stroke = main_color;
                new_line.X1 = now_point.X;
                new_line.Y1 = now_point.Y;
                new_line.X2 = preview_point.X;
                new_line.Y2 = preview_point.Y;
                new_line.StrokeThickness = 5;

                ROI_Lines.Add(new_line);

                ROI_paper.Children.Add(new_line);//그리기

            }
        }


        public void draw_end_line()
        {
            if ((ROI_Ellipse.Count > 2) && (ROI_Points.Count > 2))
            {
                Point start = ROI_Points[0]; //시작점 
                Point last = ROI_Points[ROI_Points.Count() - 1]; // 현재 끝점
                Line new_last_line = new Line();

                //지웠으니 다시 그린다.
                new_last_line.Stroke = main_color;
                new_last_line.X1 = start.X;
                new_last_line.Y1 = start.Y;
                new_last_line.X2 = last.X;
                new_last_line.Y2 = last.Y;
                new_last_line.StrokeThickness= 5;

                ROI_paper.Children.Remove(Close_line);//있던거 지우고
                ROI_paper.Children.Add(new_last_line);//그리기
                Close_line = new_last_line;//최신화
            }
        }
        
        private void ROI_paper_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //왼쪽 버튼 클릭
            draw_point(e.GetPosition(this)); //점을 그린다.
            draw_line();                     //선을 그린다.
            draw_end_line();                 //끝선을 그린다.

        }
    
        private void ROI_paper_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(ROI_Ellipse.Count != 0)
            {
                erase_point();      //점을 지운다.
                erase_line();       //선을 지운다.
                erase_end_line();   //끝선을 지운다.
            }
        }



        private void erase_point()
        {
            //
            ROI_paper.Children.Remove(ROI_Ellipse[ROI_Ellipse.Count - 1]);
            ROI_Ellipse.RemoveAt(ROI_Ellipse.Count -1);
            ROI_Points.RemoveAt(ROI_Points.Count -1);//?
        }

        private void erase_line()
        {
            //2개 이상일 때만 선을 지움.
            if ((ROI_Ellipse.Count >= 1) && (ROI_Points.Count >= 1))
            {
                ROI_paper.Children.Remove(ROI_Lines[ROI_Lines.Count - 1]); //지우기
                ROI_Lines.RemoveAt(ROI_Lines.Count - 1);
            }
        }

        private void erase_end_line()
        {
            if ((ROI_Ellipse.Count >= 1) && (ROI_Points.Count >= 1))
            {
                Point start = ROI_Points[0]; //시작점 
                Point last = ROI_Points[ROI_Points.Count() - 1]; // 현재 끝점
                Line new_last_line = new Line();

                //지웠으니 다시 그린다.
                new_last_line.Stroke = main_color;
                new_last_line.X1 = start.X;
                new_last_line.Y1 = start.Y;
                new_last_line.X2 = last.X;
                new_last_line.Y2 = last.Y;
                new_last_line.StrokeThickness = 5;

                ROI_paper.Children.Remove(Close_line);//있던거 지우고
                ROI_paper.Children.Add(new_last_line);//그리기
                Close_line = new_last_line;//최신화
            }
        }

        private void ROI_paper_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if (ROI_Ellipse.Count != 0)
                {
                    Console.WriteLine("previewmousedown");
                    erase_point();      //점을 지운다.
                    erase_line();       //선을 지운다.
                    erase_end_line();   //끝선을 지운다.
                }
                //왼쪽 버튼 클릭
                draw_point(e.GetPosition(this)); //점을 그린다.
                draw_line();                     //선을 그린다.
                draw_end_line();                 //끝선을 그린다.
            }
        }

        public void Enable_and_Disable(bool t_f)
        {
            //usercontrol을 켜고 끌수 있게 하는 기능
            if(t_f)
            {
                this.isvisible = true;
                Top_Canvas_name.Visibility = Visibility.Visible;
                Top_Canvas_name.IsEnabled = true;
            }
            else
            {
                this.isvisible = false;
                Top_Canvas_name.Visibility = Visibility.Collapsed;
                Top_Canvas_name.IsEnabled = false;
            }
        }

        public void load_and_draw_(int version)
        {
            //version 
            // 1 - editable 
            // 0 - uneditable
            // 

            if(version == 0) //메인화면
            {
                if (this.isvisible)
                {
                    Top_Canvas_name.Visibility = Visibility.Visible;
                    Top_Canvas_name.IsEnabled = false;
                    ROI_rectangle.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Top_Canvas_name.Visibility = Visibility.Collapsed;
                    Top_Canvas_name.IsEnabled = false;
                    ROI_rectangle.Visibility = Visibility.Collapsed;
                }
            }
            else if(version == 1) // setROI 화면
            {
                if (this.isvisible)
                {
                    Top_Canvas_name.Visibility = Visibility.Visible;
                    Top_Canvas_name.IsEnabled = true;
                    ROI_rectangle.Visibility = Visibility.Collapsed; // 이것만 지우면 됨

                }
                else
                {
                    Top_Canvas_name.Visibility = Visibility.Collapsed;
                    Top_Canvas_name.IsEnabled = false;
                    ROI_rectangle.Visibility = Visibility.Collapsed; //이것만 지우면 됨 rectangle 보이고 싶으면
                }
            }
            for(int i = 0; i < ROI_Ellipse.Count; i++)
            {
                ROI_paper.Children.Add(ROI_Ellipse[i]); // 점 추가 
            }
            for (int j = 0; j < ROI_Lines.Count; j++)
            {
                ROI_paper.Children.Add(ROI_Lines[j]); // 선 추가
            }
            ROI_paper.Children.Add(Close_line); // 끝선 추가
        }
        
        public List<Point> return_points()
        {
            return this.ROI_Points;
        }
    }
}
