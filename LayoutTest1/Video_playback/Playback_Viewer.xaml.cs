using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VideoOS.Platform;
using VideoOS.Platform.Client;
using VideoOS.Platform.Data;
using VideoOS.Platform.Messaging;
using VideoOS.Platform.SDK.UI.LoginDialog;
using VideoOS.Platform.UI;

namespace LayoutTest1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Playback_Viewer : Window
    {
        private static readonly Guid IntegrationId = new Guid("15B6ACBB-E1B6-4360-86B3-78445C56684D");
        private const string IntegrationName = "Playback WPF User";
        private const string Version = "1.0";
        private const string ManufacturerName = "Sample Manufacturer";

        save_file_container sfc = new save_file_container();
        save_setting ss = new save_setting();

        private FQID _playbackFQID;
        private AudioPlayerControl _microphonePlayer;
        private AudioPlayerControl _speakerPlayer;
        private MessageCommunication _mc;
        private Item _selectItem;

        public Playback_Viewer()
        {
            InitializeComponent();
            ss.set_path(System.IO.Directory.GetCurrentDirectory() + @"save_setting_file_2");
            load_playback();//저장된 설정 불러오기
        }


        private void load_playback()
        {
            try
            {
                int a = ss.load_MainWindow_1();
                bool b = ss.load_MainWindow_2();

            }
            catch (Exception ex)
            {
                //저장된 파일이 없을때,
                System.Windows.MessageBox.Show("저장된 파일이 없거나, 불러오는데 오류가 발생했습니다.\n");
                System.Windows.MessageBox.Show(ex.Message);
                return;
            }


        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }

        #region Select camera and setup controls
        private void SetupControls()
        {
            _imageViewerControl.Disconnect();

            _imageViewerControl.EnableDigitalZoom = _digitalZoomCheckBox.IsChecked.Value;
            _imageViewerControl.MaintainImageAspectRatio = _maintainAspectRatioCheckBox.IsChecked.Value;
            _imageViewerControl.EnableVisibleHeader = _visibleHeaderCheckBox.IsChecked.Value;
            _imageViewerControl.EnableVisibleCameraName = _visibleCameraNameCheckBox.IsChecked.Value;
            _imageViewerControl.EnableVisibleLiveIndicator = _visibleLiveIndicatorCheckBox.IsChecked.Value;
            _imageViewerControl.EnableVisibleTimestamp = _visibleTimeStampCheckBox.IsChecked.Value;

            if (_playbackFQID == null)
            {
                _playbackFQID = ClientControl.Instance.GeneratePlaybackController();
                _playbackUserControl.Init(_playbackFQID);
                SetPlaybackSkipMode();
            }
        }

        private void _selectCameraButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new ItemPickerForm();
            form.KindFilter = Kind.Camera;
            form.AutoAccept = true;
            form.Init(Configuration.Instance.GetItems());

            if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetupControls();
                _selectItem = form.SelectedItem;
                LoadCamera(_selectItem);
                var relatedItems = _selectItem.GetRelated();
                LoadMicrophone(relatedItems);
                LoadSpeaker(relatedItems);
                _playbackUserControl.SetCameras(new List<FQID>() { _selectItem.FQID });
            }
        }

        private void LoadCamera(Item selectedItem)
        {
            var streamDataSource = new StreamDataSource(selectedItem);
            _imageViewerControl.CameraFQID = selectedItem.FQID;
            _imageViewerControl.PlaybackControllerFQID = _playbackFQID;
            _imageViewerControl.Initialize();
            _imageViewerControl.Connect();

            _imageViewerControl.Selected = true;
            EnvironmentManager.Instance.Mode = Mode.ClientPlayback;

            EnablePlayback();
        }

        private void LoadMicrophone(IEnumerable<Item> relatedItems)
        {
            var item = relatedItems.FirstOrDefault(x => x.FQID.Kind == Kind.Microphone);
            if (item != null)
            {
                _microphonePlayer = GenerateAudioplayer();
                _microphonePlayer.MicrophoneFQID = item.FQID;
                _microphonePlayer.Connect();
            }
        }

        private void LoadSpeaker(IEnumerable<Item> relatedItems)
        {
            var item = relatedItems.FirstOrDefault(x => x.FQID.Kind == Kind.Speaker);
            if (item != null)
            {
                _speakerPlayer = GenerateAudioplayer();
                _speakerPlayer.SpeakerFQID = item.FQID;
                _speakerPlayer.Connect();
            }
        }

        private AudioPlayerControl GenerateAudioplayer()
        {
            AudioPlayerControl audioControl = ClientControl.Instance.GenerateAudioPlayerControl();
            audioControl.Initialize();
            audioControl.PlaybackControllerFQID = _playbackFQID;
            return audioControl;
        }

        private void EnablePlayback()
        {
            _visibleTimeStampCheckBox.IsEnabled = true;
            _playbackUserControl.Visibility = Visibility.Visible;
            _playbackUserControl.SetEnabled(true);
            _imageViewerControl.StartBrowse();
            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                        VideoOS.Platform.Messaging.MessageId.System.ModeChangeCommand,
                                                        Mode.ClientPlayback), _playbackFQID);
        }
        #endregion

        #region Image Viewer properties
        private void _digitalZoomCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _imageViewerControl.EnableDigitalZoom = _digitalZoomCheckBox.IsChecked.Value;
        }

        private void _maintainAspectRatioCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _imageViewerControl.MaintainImageAspectRatio = _maintainAspectRatioCheckBox.IsChecked.Value;
        }

        private void _visibleHeaderCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _imageViewerControl.EnableVisibleHeader = _visibleHeaderCheckBox.IsChecked.Value;
        }








































        private void _visibleCameraNameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _imageViewerControl.EnableVisibleCameraName = _visibleCameraNameCheckBox.IsChecked.Value;
        }

        private void _visibleLiveIndicatorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _imageViewerControl.EnableVisibleLiveIndicator = _visibleLiveIndicatorCheckBox.IsChecked.Value;
        }

        private void _visibleTimeStampCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _imageViewerControl.EnableVisibleTimestamp = _visibleTimeStampCheckBox.IsChecked.Value;
        }

        /// <summary>
        /// If Adaptive Streaming is checked, the stream resolution will be adapted based on the view size.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #endregion

        #region Image Viewer Environment properties
        private void _diagnosticsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentManager.Instance.EnvironmentOptions["PlayerDiagnosticLevel"] = _diagnosticsCheckBox.IsChecked.Value ? "3" : "0";
            EnvironmentManager.Instance.FireEnvironmentOptionsChangedEvent();
        }

        #endregion

        #region Plackback Controller properties
        private void _checkAllRadioButtonsChecked(object sender, RoutedEventArgs e)
        {
            SetPlaybackSkipMode();
        }
        #endregion

        #region Direct Playback Commands(Playback Controller via message communication direct) 

        private void DatePicker_SelectedDateChanged(object sender, EventArgs e)
        {

            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                        MessageId.SmartClient.PlaybackCommand,
                                                        new PlaybackCommandData() { Command = PlaybackData.PlayStop }), _playbackFQID);
            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                        MessageId.SmartClient.PlaybackCommand,
                                                        new PlaybackCommandData() { Command = PlaybackData.Goto, DateTime = _dateTimePicker.returnDT().ToUniversalTime() }), _playbackFQID);

        }
        #endregion

        #region helper method
        private void SetPlaybackSkipMode()
        {
            if (_skipRadioButton.IsChecked.Value)
            {
                EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                                MessageId.SmartClient.PlaybackSkipModeCommand,
                                                                PlaybackSkipModeData.Skip), _playbackFQID);
            }
            else if (_noSkipRadioButton.IsChecked.Value)
            {
                EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                                             MessageId.SmartClient.PlaybackSkipModeCommand,
                                                                             PlaybackSkipModeData.Noskip), _playbackFQID);
            }
            else if (_stopRadioButton.IsChecked.Value)
            {
                EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                                             MessageId.SmartClient.PlaybackSkipModeCommand,
                                                                             PlaybackSkipModeData.StopAtSequenceEnd), _playbackFQID);
            }
        }

        #endregion

        private void _save_video_file_Click(object sender, RoutedEventArgs e)
        {
            //동영상 저장하는 기능 제공.
            //여기서도 문제
            LayoutTest1.Video_Export video_Export = new LayoutTest1.Video_Export();
            video_Export.ShowDialog();

            video_Export.Close();
        }

        private void save_setting_playback_Click(object sender, RoutedEventArgs e)
        {
            ss.save_playback(
                _digitalZoomCheckBox.IsChecked.Value,
                _maintainAspectRatioCheckBox.IsChecked.Value,
                _visibleHeaderCheckBox.IsChecked.Value,
                _visibleCameraNameCheckBox.IsChecked.Value,
                _visibleLiveIndicatorCheckBox.IsChecked.Value,
                _visibleTimeStampCheckBox.IsChecked.Value,
                _skipRadioButton.IsChecked.Value,
                _noSkipRadioButton.IsChecked.Value,
                _stopRadioButton.IsChecked.Value,
                _diagnosticsCheckBox.IsChecked.Value
                );

        }

    }
}