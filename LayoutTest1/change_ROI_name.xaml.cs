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
    /// change_name.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class change_ROI_name : Window
    {
        public string result
        {
            get { return name_text_box.Text; }
        }
        public change_ROI_name()
        {
            InitializeComponent();
        }

        public void set_Name_out(string Name_out)
        {
            name_text_box.Text = Name_out;// 외부에서 가지고 온 이름.
        }
        private void change_accept(object sender, RoutedEventArgs e)
        {
            //이름 변경
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }


        private void change_cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
