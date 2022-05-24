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

namespace LayoutTest1
{
    /// <summary>
    /// Show_Event_Detail.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Show_Event_Detail : Window
    {
        ImageSource _imageSource { get; set; }
        public Show_Event_Detail(ImageSource i)
        {
            InitializeComponent();
            event_image_detail.Source = i;
        }
    }
}
