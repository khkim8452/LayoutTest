using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;

namespace LayoutTest1
{
    internal class save_ROI
    {
        string save_roi_path;
        //Json 객체 생성
        JObject save_obj = new JObject();

        List<string> save_list_name = new List<string>(); //name
        List<string> save_list_main_color = new List<string>(); //main_color
        List<bool> save_list_isvisible= new List<bool>(); //isvisible
        List<string> save_list_point = new List<string>(); //point

        public bool is_savefile_exist()
        {
            //세이브 파일이 존재하는지 확인 
            if (File.Exists(save_roi_path))
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
            save_roi_path = path;
        }

        
        public void save_ROI_list(ObservableCollection<DrawROI> to_save_list)
        {
            //설정을 저장
            
            if (to_save_list == null)
            {
                Console.WriteLine("저장할 list가 없습니다.");
            }
            else
            {
                string point_xy = "";
                save_list_name.Clear();
                save_list_main_color.Clear();
                save_list_isvisible.Clear();
                save_list_point.Clear();
                for (int i = 0; i < to_save_list.Count; i++)
                {
                    save_list_name.Add(to_save_list[i].name); //item 이름
                    save_list_main_color.Add(to_save_list[i].main_color.ToString());//item 색상
                    save_list_isvisible.Add(to_save_list[i].isvisible);//item 표시
                    for (int j = 0; j < to_save_list[i].ROI_Points.Count; j++)
                    {
                        point_xy += to_save_list[i].ROI_Points[j].X;
                        point_xy += ",";
                        point_xy += to_save_list[i].ROI_Points[j].Y;
                        point_xy += ",";

                    }
                    save_list_point.Add(point_xy); //item 좌표
                    point_xy = "";
                }
            }
            try
            {

                save_obj = new JObject(
                    new JProperty("save_width", to_save_list[0]._width),
                    new JProperty("save_height", to_save_list[0]._height),
                    new JProperty("save_list_name", save_list_name),
                    new JProperty("save_list_main_color", save_list_main_color),
                    new JProperty("save_list_isvisible", save_list_isvisible),
                    new JProperty("save_list_point", save_list_point)
                    );

            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }
            File.WriteAllText(save_roi_path, save_obj.ToString());

        }


        public ObservableCollection<DrawROI> load_ROI_list()
        {
            //설정을 불러옴
            ObservableCollection<DrawROI> loaded_file = new ObservableCollection<DrawROI>();//설정을 반환할 DrawROI 리스트
            try
            {
                string json = File.ReadAllText(save_roi_path); //파일을 가지고 와서 
                JObject jobj = JObject.Parse(json); //json 파일을 읽어 jobj에 파싱해 넣는다.


                int count = 0;
                while(jobj["save_list_main_color"][count] != null)
                {
                    DrawROI new_load = new DrawROI(); //리스트에 하나의 item에 해당하는 DrawROI 객체
                    string point_string = "";
                    int line_count = 0;

                    new_load._width = double.Parse(jobj["save_width"].ToString());
                    new_load._height = double.Parse(jobj["save_height"].ToString()) ;
                    new_load.name = jobj["save_list_name"][count].ToString();
                    new_load.main_color = new BrushConverter().ConvertFromString(jobj["save_list_main_color"][count].ToString()) as SolidColorBrush;
                    new_load.isvisible = string_to_bool(jobj["save_list_isvisible"][count].ToString());
                    //기본 설정

                    point_string = jobj["save_list_point"][count].ToString();//json 파일을 읽어 전체 string 으로 변환
                    
                    if (point_string != "")
                    {
                        string[] point_arr = point_string.Split(',');//json 내 아무것도 없을때를 제외하고, Comma 를 기준으로 파싱후 string 배열로 저장.
                        for (int i = 0 ; i < point_arr.Length - 1; i+=2) // x,y 쌍을 이루는 좌표에 한해서 반복
                        {
                            Point p = new Point(Double.Parse(point_arr[i]),Double.Parse(point_arr[i+1]));
                            new_load.ROI_Points.Add(p);//파싱한 포인트 저장.

                            Ellipse e = new Ellipse();
                            e.Stroke = new_load.main_color;
                            e.Fill = new_load.main_color;
                            e.Width = 14;
                            e.Height = 14;
                            Canvas.SetLeft(e, p.X - 7);
                            Canvas.SetTop(e, p.Y - 7);
                            new_load.ROI_Ellipse.Add(e); //파싱한 포인트 기반으로 Ellipse 저장.

                            if ((new_load.ROI_Ellipse.Count > 1) && (new_load.ROI_Points.Count > 1))
                            {
                                Line l = new Line();
                                l.Stroke = new_load.main_color;
                                l.StrokeThickness = 5;
                                l.X1 = new_load.ROI_Points[line_count].X;
                                l.Y1 = new_load.ROI_Points[line_count].Y;
                                l.X2 = new_load.ROI_Points[line_count - 1].X;
                                l.Y2 = new_load.ROI_Points[line_count - 1].Y;
                                new_load.ROI_Lines.Add(l); //저장된 Ellipse 기반으로 Line 그리기.
                            }
                            line_count++;

                        }
                        Line cl = new Line();
                        cl.Stroke = new_load.main_color;
                        cl.StrokeThickness = 5;
                        cl.X1 = new_load.ROI_Points[line_count - 1].X;
                        cl.Y1 = new_load.ROI_Points[line_count - 1].Y;
                        cl.X2 = new_load.ROI_Points[0].X;
                        cl.Y2 = new_load.ROI_Points[0].Y;
                        new_load.Close_line = cl; //end line 긋고 저장.
                    }

                    loaded_file.Add(new_load); //하나의 색상 ROI 끝. 
                    count++;
                } //요기까지 while 문
                
            }//요기까지 try문 
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return loaded_file; //만들어진 DrawROI 리스트 반환
        }





        private bool string_to_bool(string str)
        {
            if (str == "True")
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
