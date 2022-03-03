using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VideoOS.Platform;

namespace LayoutTest1
{
    public class Layout
    {
        static int maxrowcol = 6;
        private static Layout _instance = null;
        public static Layout Instance
        {
            get
            {
                if( _instance == null ) 
                    _instance = new Layout();   
                return _instance;
            }
        }
        public Grid MainGrid = null;

        public int Row = 4;
        public int Col = 4;
        public string LayoutName = null;
        CameraCell[,] Cells=null;

        public bool IsSingle = false;

        private Layout()
        {
            Init();
        }
        public void Init()
        {
            Cells = new CameraCell[maxrowcol,maxrowcol];
            MainGrid = new Grid();
            //MainGrid.ShowGridLines = true;
            for (int i = 0; i < maxrowcol; i++)
                for (int j = 0; j < maxrowcol; j++)
                {

                    Cells[i, j] = new CameraCell();
                    Cells[i, j].Mode = CameraCellMode.Unused;
                    Cells[i, j].Row = i;
                    Cells[i, j].Col = j;

                    Grid.SetRow(Cells[i, j], i);
                    Grid.SetColumn(Cells[i, j], j);
                    Cells[i, j].tbtemp.Text = $"{i},{j}";
                    MainGrid.Children.Add(Cells[i, j]);
                }
            
            SetRowCol( Row, Col );

        }

        public void ActivateCamera(Item item, int r, int c)
        {
            Cells[r, c].CameraItem = item;
        }
        public void ActivateCameras(ListBoxItemsBag bag, int r, int c)
        {
            

            foreach(ListBoxItem lbitem in bag.Bag)
            {
                Item i = lbitem.Tag as Item;
                if(i!=null)
                {
                    Cells[r, c].CameraItem = i;
                }
                c++;
                if(c>=Col)
                {
                    r++;
                    c = 0;
                }
                if(r>=Row)
                {
                    break;
                }
            }
            
        }
        public void SetRowCol(int r, int c)
        {
            Row = r;
            Col = c;
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < Row; i++)
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            for (int j = 0; j < Col; j++)
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

            for (int i = 0; i < maxrowcol; i++)
            for (int j = 0; j < maxrowcol; j++)
                {
                    if(!(i<Row && j<Col))
                    {
                        Cells[i, j].Mode = CameraCellMode.Unused;
                    }else
                    {
                       if(Cells[i, j].Mode == CameraCellMode.Unused)
                        {
                            Cells[i, j].Mode = CameraCellMode.Blank;
                        }
                    }
                }

        }
        public void SetSingle(CameraCell cc)
        {
            MainGrid.BeginInit();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            IsSingle = true;
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                {
                    if (i==cc.Row&&j==cc.Col)
                    {
                        Cells[i, j].IsSingle = true;
                        Grid.SetRow(Cells[i, j], 0);
                        Grid.SetColumn(Cells[i, j], 0);
                    }
                    else
                    {
                        Cells[i, j].IsSingle = false;
                        Cells[i, j].Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            MainGrid.EndInit();
        }
        public void UnsetSingle(CameraCell cc)
        {
            MainGrid.BeginInit();
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < Row; i++)
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            for (int j = 0; j < Col; j++)
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                {
                   
                        Grid.SetRow(Cells[i, j], i);
                        Grid.SetColumn(Cells[i, j], j);
                    Cells[i, j].IsSingle = false;
                    Cells[i, j].Visibility = System.Windows.Visibility.Visible;
                }
            IsSingle = false;
            MainGrid.EndInit();
        }
        public void SelectCamera(CameraCell cc)
        {
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                {
                    //Console.WriteLine();
                    if(i==cc.Row && j == cc.Col)
                    {
                        Cells[i, j].CellBorder.Visibility = Visibility.Visible;
                        //Cells[i, j].CellBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                        //Cells[i,j].HighlightGrid.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Cells[i, j].CellBorder.Visibility = Visibility.Collapsed;
                        //Cells[i, j].CellBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        //                        Cells[i, j].HighlightGrid.Visibility = Visibility.Collapsed;
                    }
                    
                }
        }
        public void CloseAll()
        {
            if (IsSingle)
            {
                MessageBox.Show("싱글 카메라 모드에서는 실행핼 수 없습니다.");
                return;
            }
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                {


                    Cells[i, j].Mode = CameraCellMode.Blank;
                }
        }
        public void UpdateTitleStatus()
        {
            for (int i = 0; i < Row; i++)
                for (int j = 0; j < Col; j++)
                {


                    Cells[i, j].UpdateTitleStatus();
                }
        }

    }
   

   
}
