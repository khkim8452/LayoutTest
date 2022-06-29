using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;

namespace LayoutTest1
{
    internal class save_setting
    {
        //세이브 파일 저장할 경로
        string save_path;
        //Json 객체 생성
        JObject save_obj = new JObject();
        save_file_container sfc = new save_file_container();



        public bool is_savefile_exist()
        {
            //세이브 파일이 존재하는지 확인 
            if (File.Exists(save_path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void set_path(string path)
        {
            save_path = path;
        }
        public void save_MainWindow(int arr_cam, bool full_display)
        {
            save_obj.Add("array_of_cam", arr_cam);
            save_obj.Add("full_display", full_display);

            File.WriteAllText(save_path, save_obj.ToString());

        }

        public void save_playback(bool pb_1, bool pb_2, bool pb_3, bool pb_4, bool pb_5, bool pb_6, bool pb_7, bool pb_8, bool pb_9, bool pb_10)
        {
            //10가지 설정 모두 저장
            save_obj = new JObject(
                new JProperty("pb_save_1", pb_1),//디지털줌
                new JProperty("pb_save_2", pb_2),//화면 비율 유지
                new JProperty("pb_save_3", pb_3),//상태바 활성화
                new JProperty("pb_save_4", pb_4),//카메라 이름 보이기
                new JProperty("pb_save_5", pb_5),//실시간 정보 표시
                new JProperty("pb_save_6", pb_6),//시간 정보 표시
                new JProperty("pb_save_7", pb_7),//다음 이벤트로 건너뛰기
                new JProperty("pb_save_8", pb_8),//다음 이벤트로 건너뛰지 않기
                new JProperty("pb_save_9", pb_9),//이벤트 마지막에서 정지
                new JProperty("pb_save_10", pb_10)//상세 정보 표시

                );

            File.WriteAllText(save_path, save_obj.ToString());
        }

        public int load_MainWindow_1() //스케일
        {
            int arr_cam = 4;

            try
            {
                string json = File.ReadAllText(save_path); //파일을 가지고 와서 
                JObject jobj = JObject.Parse(json); //파싱
                if (jobj["array_of_cam"] != null)
                {
                    arr_cam = int.Parse(jobj["array_of_cam"].ToString());
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return arr_cam;

        }


        public bool load_MainWindow_2() //전체화면
        {
            bool is_full_screen = false;

            try
            {
                string json = File.ReadAllText(save_path); //파일을 가지고 와서 
                JObject jobj = JObject.Parse(json); //파싱
                is_full_screen = string_to_bool(jobj["full_display"].ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return is_full_screen;
        }

        public save_file_container load_playback()
        {
            string json = File.ReadAllText(save_path);
            JObject jobj = JObject.Parse(json);

            sfc.a = string_to_bool(jobj["pb_save_1"].ToString());
            sfc.b = string_to_bool(jobj["pb_save_2"].ToString());
            sfc.c = string_to_bool(jobj["pb_save_3"].ToString());
            sfc.d = string_to_bool(jobj["pb_save_4"].ToString());
            sfc.e = string_to_bool(jobj["pb_save_5"].ToString());
            sfc.f = string_to_bool(jobj["pb_save_6"].ToString());
            sfc.g = string_to_bool(jobj["pb_save_7"].ToString());
            sfc.h = string_to_bool(jobj["pb_save_8"].ToString());
            sfc.i = string_to_bool(jobj["pb_save_9"].ToString());
            sfc.j = string_to_bool(jobj["pb_save_10"].ToString());

            return sfc;

        }
        public bool have_the_main_data()
        {
            string json = File.ReadAllText(save_path);
            JObject jobj = JObject.Parse(json);

            if (jobj["array_of_cam"] == null )
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool have_the_bell_data(string bell_id)
        {
            string json = File.ReadAllText(save_path);
            JObject jobj = JObject.Parse(json);
            if (jobj[bell_id] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public string load_bell_data(string bell_id)
        {
            string json = File.ReadAllText(save_path);
            JObject jobj = JObject.Parse(json);
            string bell_fqid = jobj[bell_id].ToString();//bell_id에 저장된 fQID를 가지고 온다.

            return bell_fqid;

        }
        public void save_bell_data(string bell_id, string FQID__)
        {
            string fqid = FQID__;
            save_obj.Add(bell_id, FQID__);
            File.WriteAllText(save_path, save_obj.ToString());
        }


        private bool string_to_bool(string str)
        {
            if(str == "True")
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}
