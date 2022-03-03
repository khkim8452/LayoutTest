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

namespace RoutedEventTest
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

        private void Grid6_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine((sender as Grid).Name);
            e.Handled = false;
        }

        private void Grid5_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine((sender as Grid).Name);
        }

        private void Grid4_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine((sender as Grid).Name);
        }

        private void Grid3_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine((sender as Grid).Name);
        }

        private void Grid2_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine((sender as Grid).Name);
        }

        private void Grid1_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine((sender as Grid).Name);
            
        }
    }
}
