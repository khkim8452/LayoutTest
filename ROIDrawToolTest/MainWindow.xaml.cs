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

namespace ROIDrawToolTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddPolygon_ButtonClick(object sender, RoutedEventArgs e)
        {
            Polygon p = new Polygon();  
            p.Stroke = System.Windows.Media.Brushes.Black;
            p.Fill = System.Windows.Media.Brushes.LightSeaGreen;
            p.StrokeThickness = 2;
            p.HorizontalAlignment = HorizontalAlignment.Left;
            p.VerticalAlignment = VerticalAlignment.Center;
            System.Windows.Point Point1 = new System.Windows.Point(300, 300);
            System.Windows.Point Point2 = new System.Windows.Point(500, 300);
            System.Windows.Point Point3 = new System.Windows.Point(500, 500);
            System.Windows.Point Point4 = new System.Windows.Point(300, 500);
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(Point1);
            myPointCollection.Add(Point2);
            myPointCollection.Add(Point3);
            myPointCollection.Add(Point4);
            p.Points = myPointCollection;
            ROICanvas.Children.Add(p);
        }
    }
}
