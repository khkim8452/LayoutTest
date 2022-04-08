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
using System.Drawing;
using System.Windows.Controls.Primitives;

namespace LayoutTest1
{
    /// <summary>
    /// DateTimePicker.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class DateTimePicker : UserControl
    {
        DateTime output_Date;
        public delegate void DatetimeChangeEventDelegate(object sender, EventArgs e);
        public event DatetimeChangeEventDelegate DatetimeChangeEvent;
        public DateTimePicker()
        {
            InitializeComponent();
            CalDisplay.SelectedDatesChanged += CalDisplay_SelectedDatesChanged;

            BitmapSource ConvertGDI_To_WPF(Bitmap bm)
            {
                BitmapSource bms = null;
                IntPtr h_bm = IntPtr.Zero;
                h_bm = bm.GetHbitmap();
                bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(h_bm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bms.Freeze();
                h_bm = IntPtr.Zero;
                return bms;
            }
            Bitmap bitmap1 = Properties.Resources.DateTimePicker;
            bitmap1.MakeTransparent(System.Drawing.Color.Black);
            CalIco.Source = ConvertGDI_To_WPF(bitmap1);
        }

        #region "EventHandlers"

        private void CalDisplay_SelectedDatesChanged(object sender, EventArgs e)
        {
            PopUpCalendarButton.IsChecked = true;
            if (Hours.SelectedIndex == -1 || Min.SelectedIndex == -1 || AMPM.SelectedIndex == -1)
            {
                var date = CalDisplay.SelectedDate.Value.Date + DateTime.Now.TimeOfDay;
                DateDisplay.Text = date.ToString("yyyy-MM-dd hh:mm tt");
                return;
            }
            if (AMPM.Text == "PM")
            {
                int hours = Convert.ToInt32(Hours.Text) + 12;
                TimeSpan timeSpan = TimeSpan.Parse(hours.ToString() + ":" + Min.Text);
                if (CalDisplay.SelectedDate.Value.Date == DateTime.Today.Date && timeSpan.CompareTo(DateTime.Now.TimeOfDay) < 0)
                {
                    var date = CalDisplay.SelectedDate.Value.Date + DateTime.Now.TimeOfDay;
                    DateDisplay.Text = date.ToString("yyyy-MM-dd hh:mm tt");

                    output_Date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                }
                else
                {
                    var date = CalDisplay.SelectedDate.Value.Date + timeSpan;
                    DateDisplay.Text = date.ToString("yyyy-MM-dd hh:mm tt");

                    output_Date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                }
            }
            else //AM
            {
                TimeSpan timeSpan = TimeSpan.Parse(Hours.Text + ":" + Min.Text);
                if (CalDisplay.SelectedDate.Value.Date == DateTime.Today.Date && timeSpan.CompareTo(DateTime.Now.TimeOfDay) < 0)
                {
                    var date = CalDisplay.SelectedDate.Value.Date + DateTime.Now.TimeOfDay;
                    DateDisplay.Text = date.ToString("yyyy-MM-dd hh:mm tt");

                    output_Date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                }
                else
                {
                    var date = CalDisplay.SelectedDate.Value.Date + timeSpan;
                    DateDisplay.Text = date.ToString("yyyy-MM-dd hh:mm tt");

                    output_Date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                }
            }
            if(DatetimeChangeEvent != null)
            {
                DatetimeChangeEvent(this, new EventArgs());
            }
        }

        private void SaveTime_Click(object sender, RoutedEventArgs e)
        {
            if (CalDisplay.SelectedDate.Value.Date == null)
            {
                CalDisplay.SelectedDate = DateTime.Today.Date;
                CalDisplay.DisplayDate = DateTime.Today.Date;
            }
            if (Hours.SelectedIndex == -1 || Min.SelectedIndex == -1 || AMPM.SelectedIndex == -1) { }
            else
            {
                CalDisplay_SelectedDatesChanged(SaveTime, EventArgs.Empty);
            }
            PopUpCalendarButton.IsChecked = false;
        }

        #endregion

        public DateTime returnDT()
        {
            return this.output_Date;
        }
        

    }

}
