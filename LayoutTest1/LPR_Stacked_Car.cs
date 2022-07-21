using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Media;
using System.ComponentModel;

namespace LayoutTest1
{
    internal class LPR_Stacked_Car : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static int license_Threshold = 2; //번호판 차이 임계값
        static int xy_Threshold = 25; //좌표 차이 임계값
        static int wh_Threshold = 30; //가로세로 차이 임계값

        string[] stack_license = { };//쌓인 번호판 //가변배열
        public string main_Lincense;  //대표 번호판
        Point current_xy;//이전 좌표
        Point current_wh;//이전 가로세로

        //차량 움직임 상태와 그 시간을 세는 단위
        public bool stop_state; //true = 주정차 false = 움직임
        public int stop_count;  //정차한 frame 수를 센다.

        public DateTime last_receive_time; //마지막으로 이벤트 발생한 시각
        public DateTime first_stop_time;   //처음으로 멈춤 이벤트 발생한 시각
        public bool stopped = false;       //first_stop_time 가져올 조건
        public string elapse_time;         //정차후 경과 시간

        public bool is_cracked_down = false;

        private bool flag = false;
        public bool flag2 = false;


        public ImageSource Current_image; //실시간 - 대표이미지 
        public ImageSource current_image
        {
            get
            {
                return Current_image;
            }
            set
            {
                Current_image = value;
                OnPropertyChanged("current_image");
            }
        }

        public string Main_Lincense
        {
            get
            {
                return main_Lincense;
            }
            set
            {
                main_Lincense = value;
                OnPropertyChanged("Main_Lincense");
            }
        }
        public string Elapse_time
        {
            get
            {
                return elapse_time;
            }
            set
            {
                elapse_time = value;
                OnPropertyChanged("Elapse_time");
            }
        }

        //? 정차 상태


        public LPR_Stacked_Car(string new_license, int x_, int y_, int w_, int h_, ImageSource image_)//생성자
        {
            this.main_Lincense = new_license;
            stack_license = stack_license.Append(new_license).ToArray();
            current_xy = new Point(x_, y_);
            current_wh = new Point(w_, h_);
            stop_state = false;//move
            current_image = image_;
            last_receive_time = DateTime.Now;
            //first_stop_time = DateTime.Now;
            Thread t = new Thread(new ThreadStart(Run));
            t.Start();
            
        }

        public void update_main_license(string new_license)
        {
            if(stack_license.Length <=500)//버벅일것을 우려 500이하일때 까지만 계산
            {
                stack_license = stack_license.Append(new_license).ToArray();
                Array.Sort(stack_license, StringComparer.InvariantCulture);//정렬

                //가장 많은것을 main에 대입
                var group = stack_license
                    .GroupBy(x => x)
                    .OrderByDescending(grp => grp.Count())
                    .FirstOrDefault();
                main_Lincense = group.Key;
            }
            last_receive_time = DateTime.Now;
    }
        public bool is_same_license(string compare_license)
        {
            int diff_result = Edit_distance(this.main_Lincense, compare_license);
            if (diff_result <= license_Threshold)//이미 있는게 있으면~
            {
                //같은번호판으로 판단함.
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int Edit_distance(string first, string second)
        {
            if (first.Length == 0)
            {
                return second.Length;
            }

            if (second.Length == 0)
            {
                return first.Length;
            }

            var d = new int[first.Length + 1, second.Length + 1];
            for (var i = 0; i <= first.Length; i++)
            {
                d[i, 0] = i;
            }

            for (var j = 0; j <= second.Length; j++)
            {
                d[0, j] = j;
            }

            for (var i = 1; i <= first.Length; i++)
            {
                for (var j = 1; j <= second.Length; j++)
                {
                    var cost = (second[j - 1] == first[i - 1]) ? 0 : 1;
                    d[i, j] = Min(d[i - 1, j] + 1, d[i, j - 1] + 1, d[i - 1, j - 1] + cost);
                }
            }
            return d[first.Length, second.Length];
        }
        private static int Min(int e1, int e2, int e3) => Math.Min(Math.Min(e1, e2), e3);
        public bool is_same_position(int x, int y, int w, int h)
        {
            // 입력된 x랑 y의 값을 가지고, 최대 최소값과 비교하여 멈춤 상태인지 움직임 상태인지 판단하는 것.
            double x_diff = Math.Abs(current_xy.X - x);
            double y_diff = Math.Abs(current_xy.Y - y);
            double w_diff = Math.Abs(current_wh.X - w);
            double h_diff = Math.Abs(current_wh.Y - h);

            double xy = (x_diff + y_diff);
            double wh = (w_diff + h_diff);

            Console.WriteLine($"{x} {y} {w} {h} ");

            current_xy = new Point(x, y);
            current_wh = new Point(w, h);

            if ((xy <= xy_Threshold) && (wh <= wh_Threshold))//이미 있는게 있으면~
            {
                //주정차
                stop_count += 1;
                if(stop_count == 10) //10프레임 이상 정차면, count 시작
                {
                    stop_state = true;
                    first_stop_time = DateTime.Now;
                }
                return true;
            }
            else
            {
                //움직임
                stop_state = false;
                stop_count = 0;
                return false;
            }

        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public void Abort()
        {
            this.flag = true;
        }

        private void Run()
        {
            while(true)
            { 
                if (flag)
                {
                    break;
                }
                TimeSpan elapse_t2 = DateTime.Now - last_receive_time;

                if (elapse_t2 > TimeSpan.FromSeconds(2))
                {
                    //2초 이상 데이터가 안들어오면
                    stop_state = false;
                    this.elapse_time = "감지 안됨";
                    continue;
                    if (elapse_t2 > TimeSpan.FromSeconds(20))
                    {
                        //20초 이상 데이터가 안들어오면
                        this.flag2 = true;
                    }
                }

                if (stop_state)
                {
                    //차가 멈췄다면, 카운트
                    TimeSpan elapse_t1 = DateTime.Now - first_stop_time;
                    DateTime dt = new DateTime() + elapse_t1;
                    this.elapse_time = "정차 " + Convert.ToDateTime(dt).ToString("m분s초");
                }
                else
                {
                    this.elapse_time = "상태 확인중";
                }
                Thread.Sleep(100);
            }
        }
    }
}
