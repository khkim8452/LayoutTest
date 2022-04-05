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
using VideoOS.Platform.ConfigurationItems;
using VideoOS.Platform.Messaging;
using VideoOS.Platform.UI;

namespace LayoutTest1
{
    /// <summary>
    /// PTZ_control.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PTZ_control : UserControl
    {
        Item item = null;
        PTZMoveStartCommandData PTZ_Data = new PTZMoveStartCommandData();

        public PTZ_control()
        {
            InitializeComponent();
        }

        public void set_ptz_item(Item i)
        {
            item = i;
        }

        public void set_PTZ_Data(float pan, double speed, float tilt, float zoom)
        {
            PTZ_Data.Pan = pan;
            PTZ_Data.Speed = speed;
            PTZ_Data.Tilt = tilt;
            PTZ_Data.Zoom = zoom;
        }

        private void PTZ_T_Click(object sender, RoutedEventArgs e)
        {
            //위로 버튼
            Console.WriteLine("up button clicked");
            set_PTZ_Data(0, 0.5, -1, 0);
            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStartCommand, PTZ_Data);
            EnvironmentManager.Instance.PostMessage(msg, item.FQID);
        }

        private void PTZ_L_Click(object sender, RoutedEventArgs e)
        {
            //왼쪽 버튼
            Console.WriteLine("left button clicked");
            set_PTZ_Data(-1, 0.5, 0, 0);
            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStartCommand, PTZ_Data);
            EnvironmentManager.Instance.SendMessage(msg, item.FQID);
        }

        private void PTZ_R_Click(object sender, RoutedEventArgs e)
        {
            //오른쪽 버튼 
            Console.WriteLine("right button clicked");
            set_PTZ_Data(1, 0.5, 0, 0);
            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStartCommand, PTZ_Data);
            EnvironmentManager.Instance.SendMessage(msg, item.FQID);
        }


        private void PTZ_B_Click(object sender, RoutedEventArgs e)
        {
            //아래로 버튼
            Console.WriteLine("down button clicked");
            set_PTZ_Data(0, 0.5, 1, 0);
            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStartCommand, PTZ_Data);
            EnvironmentManager.Instance.PostMessage(msg, item.FQID);
        }


        private void PTZ_ZO_Click(object sender, RoutedEventArgs e)
        {
            //줌 아웃
            
            Console.WriteLine("zoom out button clicked");
            set_PTZ_Data(0, 0.5, 0, -1);
            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStartCommand, PTZ_Data);
            EnvironmentManager.Instance.PostMessage(msg, item.FQID);

        }

        private void PTZ_ZI_Click(object sender, RoutedEventArgs e)
        {
            //줌 인
            Console.WriteLine("zoom in button clicked");
            set_PTZ_Data(0, 0.5, 0, 1);
            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStartCommand, PTZ_Data);
            EnvironmentManager.Instance.PostMessage(msg, item.FQID);
        }

        private void PTZ_ST_Click(object sender, RoutedEventArgs e)
        {

            VideoOS.Platform.Messaging.Message msg = new VideoOS.Platform.Messaging.Message(MessageId.Control.PTZMoveStopCommand);
            EnvironmentManager.Instance.PostMessage(msg, item.FQID);
        }
    }
}
