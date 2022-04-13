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

namespace LayoutTest1
{
    /// <summary>
    /// DrawROI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DrawROI : UserControl
    {
        Polygon ROI_Polygon = new Polygon();
        private List<Point> ROI_Points = new List<Point>();//폴리곤 버튼 list에 저장.


        public DrawROI()
        {
            InitializeComponent();
            set_ROI_Polygon();//polygon 세팅
        }
        private void set_ROI_Polygon()
        {
            ROI_Polygon.Stroke = Brushes.Orange;
            ROI_Polygon.Fill = Brushes.Transparent.
        }

        public void setRatio(double height, double width)
        {
            Top_Canvas_name.Width = width;
            Top_Canvas_name.Height = height;
        }
        
        public void setStrokeThickness(int thickness)
        {

        }

        public void make_point()
        {
            ROI_Polygon.PointFromScreen(new Point(0, 0));
        }

        private void Left_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Left");
            ROI_Points.Add(new Point(e.XButton1, e.Timestamp));
        }

        private void Right_Mouse_Down(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Right");
        }
    }
}
