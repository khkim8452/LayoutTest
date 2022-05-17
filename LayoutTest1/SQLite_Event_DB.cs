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

namespace LayoutTest1
{
    internal class SQLite_Event_DB : Window
    {
        private SQLiteConnection conn = null;
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
            string sql = "create table events (E_image string, E_time string, E_content string, E_Kind int, E_Star bool)";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery();
            sql = "create index E_index on events(E_time)";
            command = new SQLiteCommand(sql, conn);
            result = command.ExecuteNonQuery();
        }

        public void Insert_Row(string E_image_, string E_time_, string E_content_, int kind)
        {
            string sql = "insert into events (E_image, E_time, E_content, E_Kind, E_Star) values (\"" + E_image_ + "\",\"" + E_time_ + "\",\"" + E_content_ + "\"," + kind + "," + false + ")";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            int result = command.ExecuteNonQuery(); //오류 발생 = / token 잘못된 문자열
        }

        private void Query_Data() //조회
        {
            string sql = "select * from events";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                MessageBox.Show(rdr["name"] + " " + rdr["age"]);
            }
            rdr.Close();
        }
        public ObservableCollection<Event_> see_all_Query_Data()
        {
            //최대 1000개만 보여주기
            ObservableCollection<Event_> le = new ObservableCollection<Event_>();
            Event_ eee = new Event_();
            string sql = "select * from events limit 10 ";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                MessageBox.Show(rdr["name"] + " " + rdr["age"]);
                eee.Image_String = rdr["E_image"].ToString(); //이미지 꺼내기
                eee.time = rdr["E_time"].ToString(); // 시간 꺼내기
                eee.content = rdr["E_Content"].ToString();
                eee.kind = int.Parse(rdr["E_Kind"].ToString());
                //eee.star = bool.Parse(rdr["E_Star"].ToString());
                
                le.Add(eee);
            }
            rdr.Close();
            return le;
        }

        private void Close_Connection(object sender, RoutedEventArgs e)
        {
            conn.Close();
        }
    }
}
