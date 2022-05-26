using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.IO;
using System.Collections.ObjectModel;
using System;

namespace LayoutTest1
{
    internal class SQLite_Event_DB : Window
    {
        private SQLiteConnection conn = null;
        public string last_query = "";
        public SQLite_Event_DB()
        {
            if(File.Exists(System.IO.Directory.GetCurrentDirectory() + @"event_DB.sqlite"))
            {
                //파일이 존재하면
                Open_DB(); //파일을 연다.
            }
            else
            {
                //파일이 존재하지 않으면
                Create_DB_File(); //새로 파일을 만든다
                Open_DB(); //파일을 연다.
                Create_Table();//테이블을 만들고
            }
        }
        private void Create_DB_File()
        {
            SQLiteConnection.CreateFile(System.IO.Directory.GetCurrentDirectory() + @"event_DB.sqlite");
        }
        private void Open_DB()
        {
            conn = new SQLiteConnection("Data Source=" + System.IO.Directory.GetCurrentDirectory() + @"event_DB.sqlite" + ";Version=3;");
            conn.Open();
        }
        private void Create_Table()
        {
            string sql = "create table events (E_index Integer primary key autoincrement, E_image string, E_time datetime, E_content string, E_kind integer, E_star bool)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery();
        }
        public void Insert_Row(string E_image_, string E_time_, string E_content_, int kind)//데이터 추가
        {
            string sql = "insert into events (E_image, E_time, E_content, E_kind, E_star) values (\"" + E_image_ + "\",\"" + E_time_ + "\",\"" + E_content_ + "\"," + kind + "," + false + ")";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery(); //오류 발생 = / token 잘못된 문자열
        } 
        public ObservableCollection<Event_> Select_Row(string sql, string asc_desc, int Max_Row)//데이터 추출
        {
            //최대 1000개만 보여주기
            ObservableCollection<Event_> le = new ObservableCollection<Event_>();
            last_query = sql;
            SQLiteDataReader rdr;

            if(asc_desc != "")
            {
                sql += " order by E_index " + asc_desc;
            }

            if (Max_Row == -1)
            {
                //전체 출력

                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                rdr = cmd.ExecuteReader();
            }
            else
            {
                SQLiteCommand cmd = new SQLiteCommand(sql + " limit " + Max_Row, conn);
                rdr = cmd.ExecuteReader();
            }

            if (rdr.HasRows && sql != "")
            {
                while (rdr.Read())
                {
                    Event_ eee = new Event_();
                    byte[] data = Convert.FromBase64String(rdr["E_image"].ToString());
                    eee.Image_String = rdr["E_image"].ToString();
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = new System.IO.MemoryStream(data);
                    bmp.EndInit();
                    eee.index = int.Parse(rdr["E_index"].ToString());
                    eee.image = bmp;
                    eee.time = rdr["E_time"].ToString(); // 시간 꺼내기
                    eee.content = rdr["E_content"].ToString();
                    eee.kind = int.Parse(rdr["E_kind"].ToString());
                    eee.star = string_to_bool(rdr["E_Star"].ToString());
                    if(eee.star)
                    {
                        eee.Starbtn_color = Brushes.Yellow;
                    }
                    else
                    {
                    
                        eee.Starbtn_color = Brushes.Transparent;
                    }

                    le.Add(eee);
                }
            }
            else
            {
                //데이터가 없음
            }

            rdr.Close();
            return le;
        }
        public void operate_this_query(string sql)//데이터 수정, 데이터 삭제 
        {
            //그냥 sql을 실행해주는 함수 
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery();
        } 
        private void Close_Connection(object sender, RoutedEventArgs e)
        {
            conn.Close();
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
