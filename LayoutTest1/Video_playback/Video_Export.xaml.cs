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
using System.Windows.Shapes;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using VideoOS.Platform;
using VideoOS.Platform.Data;
using VideoOS.Platform.UI;

namespace LayoutTest1
{
    /// <summary>
    /// Video_Export.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Video_Export : Window
    {
        private readonly List<Item> _cameraItem = new List<Item>();
        private string _path;
        private IExporter _exporter;
        private readonly Timer _timer = new Timer { Interval = 100 };

        public Video_Export()
        {
            InitializeComponent();
        }

        private void select_cam_button_Click(object sender, RoutedEventArgs e)
        {
            //추출할 카메라 선택하는 부분
            ItemPickerForm form = new ItemPickerForm
            {
                KindFilter = Kind.Camera,
                AutoAccept = true
            };
            form.Init(Configuration.Instance.GetItems());

            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var cameraItem = form.SelectedItem;
                _cameraItem.Add(cameraItem);
                selected_cam_to_save.Text = cameraItem.Name;

                EnableExport();
            }
        }

        private void EnableExport()
        {
            //카메라 선택과 폴더 선택이 모두 완료 되면, Export 버튼을 활성화.
            if ((_cameraItem != null) && (_path != null))
            {
                export_Btn.IsEnabled = true;
            }
            else
            {
                export_Btn.IsEnabled = false;
            }
        }

        private void select_folder_button_Click(object sender, RoutedEventArgs e)
        {
            //추출한 동영상을 저장할 폴더 선택하는 부분
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _path = dialog.SelectedPath;
                selected_folder_to_save.Text = _path;

                EnableExport();
            }
        }

        private void export_Btn_Click(object sender, RoutedEventArgs e)
        {            
                //받아온 정보를 기준으로 추출을 시작

            string destPath = _path;

            // Get the related audio devices
            var audioSources = new List<Item>();

            if (dateTimePickerStart.returnDT().ToUniversalTime() > dateTimePickerEnd.returnDT().ToUniversalTime())
            {
                System.Windows.MessageBox.Show("시작 시간은 끝나는 시간보다 이른 시간이어야 합니다.");
                return;
            }
            //AVI 만 출력함.
            if (textBoxVideoFilename.Text == "")
            {
                System.Windows.MessageBox.Show("파일 이름을 입력해주세요.");
                return;
            }

            _exporter = new MKVExporter { Filename = textBoxVideoFilename.Text };
            destPath = System.IO.Path.Combine(_path, "Exported Images\\" + MakeStringPathValid(_cameraItem.FirstOrDefault().Name));


            _exporter.Init();
            _exporter.Path = destPath;
            _exporter.CameraList.AddRange(_cameraItem);//??
            _exporter.AudioList.AddRange(audioSources);

            try
            {
                if (_exporter.StartExport(dateTimePickerStart.returnDT().ToUniversalTime(), dateTimePickerEnd.returnDT().ToUniversalTime()))
                {
                    _timer.Tick += ShowProgress;
                    _timer.Start();

                    export_Btn.IsEnabled = false;
                    Cancel_Btn.IsEnabled = true;
                }
                else
                {
                    int lastError = _exporter.LastError;
                    string lastErrorString = _exporter.LastErrorString;
                    labelError.Text = lastErrorString + "  ( " + lastError + " )";
                    _exporter.EndExport();
                }
            }
            catch (NoVideoInTimeSpanMIPException ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Start Export");
            }
            catch (Exception ex)
            {
                EnvironmentManager.Instance.ExceptionDialog("Start Export", ex);
            }
        }

        private void ShowProgress(object sender, EventArgs e)
        {
            if (_exporter != null)
            {
                int progress = _exporter.Progress;
                int lastError = _exporter.LastError;
                string lastErrorString = _exporter.LastErrorString;
                if (progress >= 0)
                {
                    progressBar.Value = progress;
                    if (progress == 100)
                    {
                        _timer.Stop();
                        labelError.Text = "Done";
                        _exporter.EndExport();
                        _exporter = null;
                        Cancel_Btn.IsEnabled = false;
                    }
                }
                if (lastError > 0)
                {
                    progressBar.Value = 0;
                    labelError.Text = lastErrorString + "  ( " + lastError + " )";
                    if (_exporter != null)
                    {
                        _exporter.EndExport();
                        _exporter = null;
                        Cancel_Btn.IsEnabled = false;
                    }
                }
            }
        }


        private static string MakeStringPathValid(string unsafeString)
        {
            char[] invalidCharacters = System.IO.Path.GetInvalidFileNameChars();
            string result = unsafeString;
            foreach (var invalidCharacter in invalidCharacters)
            {
                result = result.Replace(invalidCharacter, '_');
            }
            return result;
        }

        private void Close_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (_exporter != null)
            {
                _exporter.Cancel();
                _exporter.EndExport();
                _exporter.Close();
            }
            //VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }

        private void Cancel_Btn_Click(object sender, RoutedEventArgs e)
        {
            _exporter?.Cancel();
        }

    }
}
