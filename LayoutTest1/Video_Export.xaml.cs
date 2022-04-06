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
        private Item _cameraItem = new Item();
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
                _cameraItem = cameraItem;
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
            var metadataSources = new List<Item>();


            if (dateTimePickerStart.Value > dateTimePickerEnd.Value)
            {
                MessageBox.Show("Start time need to be lower than end time");
                return;
            }

            if (radioButtonAVI.Checked)
            {
                if (textBoxVideoFilename.Text == "")
                {
                    MessageBox.Show("Please enter a filename for the AVI file.", "Enter Filename");
                    return;
                }
                AVIExporter aviExporter = new AVIExporter
                {
                    Filename = textBoxVideoFilename.Text,
                    Codec = (string)comboBoxCodec.SelectedItem,
                    AudioSampleRate = int.Parse(comboBoxSampleRate.SelectedItem.ToString())
                };

                if (checkBoxIncludeOverlayImage.Checked)
                {
                    if (_overlayImageFileName == null)
                    {
                        MessageBox.Show("Please select an image file for the overlay image.", "Select image file");
                        return;
                    }

                    Bitmap overlayImage = (Bitmap)Image.FromFile(_overlayImageFileName);
                    if (aviExporter.SetOverlayImage(overlayImage,
                            AVIExporter.VerticalOverlayPositionTop,
                            AVIExporter.HorizontalOverlayPositionLeft,
                            0.1,
                            false) == false)
                    {
                        MessageBox.Show("Failed to set overlay image, error: " + aviExporter.LastErrorString, "Overlay image");
                    }
                }
                _exporter = aviExporter;

                destPath = Path.Combine(_path, "Exported Images\\" + MakeStringPathValid(_cameraItems.FirstOrDefault().Name));
            }
            else if (radioButtonMKV.Checked)
            {
                if (textBoxVideoFilename.Text == "")
                {
                    MessageBox.Show("Please enter a filename for the MKV file.", "Enter Filename");
                    return;
                }

                if (_cameraItems.Count > 1)
                    MessageBox.Show("Warning, the MKV Exporter will only export the data from the first camera in the list");

                _exporter = new MKVExporter { Filename = textBoxVideoFilename.Text };
                destPath = Path.Combine(_path, "Exported Images\\" + MakeStringPathValid(_cameraItems.FirstOrDefault().Name));
            }
            else
            {
                if (checkBoxEncrypt.Checked && textBoxEncryptPassword.Text == "")
                {
                    MessageBox.Show("Please enter password to encrypt with.", "Enter Password");
                    return;
                }
                var dbExporter = new DBExporter(true)
                {
                    Encryption = checkBoxEncrypt.Checked,
                    EncryptionStrength = EncryptionStrength.AES128,
                    Password = textBoxEncryptPassword.Text,
                    SignExport = checkBoxSign.Checked,
                    PreventReExport = checkBoxReExport.Checked,
                    IncludeBookmarks = checkBoxIncludeBookmark.Checked
                };
                dbExporter.MetadataList.AddRange(metadataSources);

                _exporter = dbExporter;
            }

            _exporter.Init();
            _exporter.Path = destPath;
            _exporter.CameraList.AddRange(_cameraItems);
            _exporter.AudioList.AddRange(audioSources);

            try
            {
                if (_exporter.StartExport(dateTimePickerStart.Value.ToUniversalTime(), dateTimePickerEnd.Value.ToUniversalTime()))
                {
                    _timer.Tick += ShowProgress;
                    _timer.Start();

                    export_Btn.Enabled = false;
                    buttonCancel.Enabled = true;
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
                MessageBox.Show(ex.Message, "Start Export");
            }
            catch (Exception ex)
            {
                EnvironmentManager.Instance.ExceptionDialog("Start Export", ex);
            }
        }
    }
}
