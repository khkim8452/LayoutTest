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

        private FQID _playbackFQID;
        private AudioPlayerControl _microphonePlayer;
        private AudioPlayerControl _speakerPlayer;
        private MessageCommunication _mc;
        private Item _selectItem;

        public Playback_Viewer()
        {
            InitializeComponent();
            //하단 제어바 시작하자마자 모든 설정 보여주는 것으로 고정.
            _playbackUserControl.ShowTallUserControl = true;
            _playbackUserControl.ShowSpeedControl = true;
            _playbackUserControl.ShowTimeSpanControl = true;
        }

        private object IPAddressResponseHandler(VideoOS.Platform.Messaging.Message message, FQID destination, FQID sender)
        {
            string ip = (string)message.Data;
            System.Windows.MessageBox.Show(ip, _selectItem.Name);
            return null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupControls();
        }

        private void _closeButton_Click(object sender, RoutedEventArgs e)
        {
            VideoOS.Platform.SDK.Environment.RemoveAllServers();
            Close();
        }

        private void liftPrivacyMask_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Instance.ServerFQID.ServerId.UserContext.SetPrivacyMaskLifted(!Configuration.Instance.ServerFQID.ServerId.UserContext.PrivacyMaskLifted);
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
        private void _stopButton_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                            MessageId.SmartClient.PlaybackCommand,
                                            new PlaybackCommandData() { Command = PlaybackData.PlayStop }), _playbackFQID);
        }

        private void _forwardButton_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                            MessageId.SmartClient.PlaybackCommand,
                                            new PlaybackCommandData() { Command = PlaybackData.PlayForward, Speed = 1.0 }), _playbackFQID);

        }

        #endregion

        #region Commands to Playback Controller
        private void _maxForwardSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playbackFQID != null)
            {
                PlaybackCommandData data = new PlaybackCommandData() { Command = PlaybackData.PlayForward, Speed = 32 };
                EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                    MessageId.SmartClient.PlaybackCommand,
                                                    data), _playbackFQID);
            }
        }

        private void _maxTimespanButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playbackUserControl != null && _playbackUserControl.TimeSpan.Days != 28)
            {
                _playbackUserControl.TimeSpan = new TimeSpan(28, 0, 0, 0);
            }
        }
        #endregion

        #region Get the IP


        private void _IpButton_Click(object sender, RoutedEventArgs e)
        {
            EnsureMessageCommunicationInitialized();
            _mc.TransmitMessage(new VideoOS.Platform.Messaging.Message(MessageId.Server.GetIPAddressRequest, _selectItem.FQID), null, null, null);
        }

        private void EnsureMessageCommunicationInitialized()
        {
            if (_mc == null)
            {
                MessageCommunicationManager.Start(EnvironmentManager.Instance.MasterSite.ServerId);
                _mc = MessageCommunicationManager.Get(EnvironmentManager.Instance.MasterSite.ServerId);
                _mc.RegisterCommunicationFilter(IPAddressResponseHandler, new CommunicationIdFilter(MessageId.Server.GetIPAddressResponse));
            }
        }
        #endregion

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

        private void groupBox0_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //새로운 캘린더로 검색할거니까 여기다가 기존의 것 구현하기
            Calendar calendar = sender as Calendar;
            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                        MessageId.SmartClient.PlaybackCommand,
                                                        new PlaybackCommandData() { Command = PlaybackData.PlayStop }), _playbackFQID);
            EnvironmentManager.Instance.SendMessage(new VideoOS.Platform.Messaging.Message(
                                                        MessageId.SmartClient.PlaybackCommand,
                                                        new PlaybackCommandData() { Command = PlaybackData.Goto, DateTime = calendar.SelectedDate.Value.ToUniversalTime() }), _playbackFQID);
        }
    }
}