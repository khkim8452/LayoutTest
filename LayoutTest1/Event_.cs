using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LayoutTest1
{
    internal class Event_ : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        JObject loaded_event = new JObject();

        public string Image_String;
        private ImageSource Image;
        private string Time;
        private string Content;
        private int Kind;
        // kind - 0 : car
        // kind - 1 : fire
        // kind - 2 : person

        public int kind
        {
            get 
            { 
                return Kind; 
            }
            set 
            {
                Kind = value;
                OnPropertyChanged("kind");
            }
        }
        public ImageSource image
        {
            get
            {
                return Image;
            }
            set
            {
                this.Image = value;
                OnPropertyChanged("image");
            }

        }
        public string time
        {
            get { return Time; }
            set
            {
                this.Time = value;
                OnPropertyChanged("time");
            }
        }
        public string content
        {
            get { return Content; }
            set
            {
                this.Content = value;
                OnPropertyChanged("content");
            }
        }

        public Event_(JObject j)
        {
            loaded_event = j;
            base64_to_Image();//이미지 넣기
            this.time = j["Time_event"].ToString();//시간 넣기
            this.content = j["Car_number_event"].ToString(); //일단
            this.kind = int.Parse(j["Kind_event"].ToString());// 이벤트 종류 넣기
        }


        public void base64_to_Image()
        {
            byte[] data = Convert.FromBase64String(loaded_event["data"].ToString());
            Image_String = loaded_event["data"].ToString();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new System.IO.MemoryStream(data);
            bmp.EndInit();
            image = bmp;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
