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
    /// ToggleSwitch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private bool on_off_status; // 꺼짐 - 켜짐 상태를 담는 bool 변수
        private bool is_on;
        private bool is_off;

        //선언부

        public ToggleSwitch()
        {
            InitializeComponent();
            //이 사용자 정의 컨트롤이 실행되면 실행된다.
            Loaded += ToggleSwitch_Loaded;
        }
        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            //시작과 동시에 상태를 끔.
            Off = true;
        }

        protected void OnPropertyChanged(string name)
        {
            //프로퍼티가 변경되면 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool On
        {
            get
            {
                return is_on;
            }
            set
            {
                is_on = value;
                if (is_on)
                {
                    Off = false;
                    Back.Fill = new SolidColorBrush(Color.FromRgb(249, 199, 106));
                    EllOn.Visibility = Visibility.Visible;
                }
                else
                {
                    EllOn.Visibility = Visibility.Collapsed;
                }
                OnPropertyChanged("On");
            }
        }

        public bool Off
        {
            get
            {
                return is_off;
            }
            set
            {
                is_off = value;
                if (is_off)
                {
                    On = false;
                    Back.Fill = new SolidColorBrush(Color.FromRgb(155, 230, 170));
                    EllOff.Visibility = Visibility.Visible;
                }
                else
                {
                    EllOff.Visibility = Visibility.Collapsed;
                }
                OnPropertyChanged("Off");
            }
        }


        private void changeMode()
        {
            if(on_off_status)
            {
                On = true;
            }
            else
            {
                Off = true;
            }
        }
        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            changeMode();
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            changeMode();
        }

    }
}
