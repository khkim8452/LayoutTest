using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;

namespace LayoutTest1
{
    internal class Event
    {
        JObject loaded_event = new JObject();

        Image image { get; set; }
        string time { get; set; }
        string content { get; set; }



        public Event(JObject j)
        {
            loaded_event = j;
        }

        public void get_new_event(JObject jobj)
        {
            //새 이벤트 받았을 때
            loaded_event = jobj;

        }

        public Image base64_to_Image()
        {
            byte[] data = Convert.FromBase64String(loaded_event["Image_event"].ToString());
            Image image;
            using (MemoryStream ms = new MemoryStream(data))
            {
                image = Image.FromStream(ms);
            }
            return image;
        }
    }
}
