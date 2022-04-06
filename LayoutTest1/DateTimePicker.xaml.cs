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
        private const string DateTimeFormat = "dd.MM.yyyy HH:mm";

        #region "Properties"

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        #endregion

        #region "DependencyProperties"

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register("SelectedDate",
            typeof(DateTime), typeof(DateTimePicker), new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        public DateTimePicker()
        {
            InitializeComponent();
            CalDisplay.SelectedDatesChanged += CalDisplay_SelectedDatesChanged;
            CalDisplay.SelectedDate = DateTime.Now.AddDays(1);

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
            var hours = (Hours?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "0";
            var minutes = (Min?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "0";
            TimeSpan timeSpan = TimeSpan.Parse(hours + ":" + minutes);
            if (CalDisplay.SelectedDate.Value.Date == DateTime.Today.Date && timeSpan.CompareTo(DateTime.Now.TimeOfDay) < 0)
            {
                timeSpan = TimeSpan.FromHours(DateTime.Now.Hour + 1);
            }
            var date = CalDisplay.SelectedDate.Value.Date + timeSpan;
            DateDisplay.Text = date.ToString(DateTimeFormat);
            SelectedDate = date;
        }

        private void SaveTime_Click(object sender, RoutedEventArgs e)
        {
            CalDisplay_SelectedDatesChanged(SaveTime, EventArgs.Empty);
            PopUpCalendarButton.IsChecked = false;
        }

        private void Time_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalDisplay_SelectedDatesChanged(sender, e);
        }

        private void CalDisplay_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {   // that it's not necessary to click twice after opening the calendar  https://stackoverflow.com/q/6024372
            if (Mouse.Captured is CalendarItem)
            {
                Mouse.Capture(null);
            }
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }
    }
}
