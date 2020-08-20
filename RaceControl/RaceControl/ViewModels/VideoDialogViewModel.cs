using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using RaceControl.Common.Settings;
using RaceControl.Core.Mvvm;
using RaceControl.Enums;
using RaceControl.Events;
using RaceControl.Services.Interfaces.F1TV;
using RaceControl.Streamlink;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using Timer = System.Timers.Timer;

namespace RaceControl.ViewModels
{
    public class VideoDialogViewModel : DialogViewModelBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IApiService _apiService;
        private readonly IStreamlinkLauncher _streamlinkLauncher;
        private readonly IVideoSettings _videoSettings;
        private readonly LibVLC _libVLC;
        private readonly Guid _uniqueIdentifier = Guid.NewGuid();

        private ICommand _mouseDownVideoCommand;
        private ICommand _mouseMoveVideoCommand;
        private ICommand _mouseEnterVideoCommand;
        private ICommand _mouseLeaveVideoCommand;
        private ICommand _mouseMoveControlBarCommand;
        private ICommand _mouseEnterControlBarCommand;
        private ICommand _mouseLeaveControlBarCommand;
        private ICommand _togglePauseCommand;
        private ICommand _toggleMuteCommand;
        private ICommand _fastForwardCommand;
        private ICommand _syncSessionCommand;
        private ICommand _toggleFullScreenCommand;
        private ICommand _moveToCornerCommand;
        private ICommand _audioTrackSelectionChangedCommand;
        private ICommand _scanChromecastCommand;
        private ICommand _startCastVideoCommand;
        private ICommand _stopCastVideoCommand;

        private string _token;
        private ContentType _contentType;
        private string _contentUrl;
        private string _syncUID;
        private bool _isLive;
        private bool _isCasting;
        private Process _streamlinkProcess;
        private Process _streamlingRecordingProcess;
        private MediaPlayer _mediaPlayer;
        private Media _media;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private long _duration;
        private long _sliderTime;
        private TimeSpan _displayTime;
        private RendererDiscoverer _rendererDiscoverer;
        private ObservableCollection<RendererItem> _rendererItems;
        private RendererItem _selectedRendererItem;
        private Timer _showControlsTimer;
        private bool _showControls;
        private double _top;
        private double _left;
        private double _width = 1200;
        private double _height = 705;
        private ResizeMode _resizeMode = ResizeMode.CanResize;
        private WindowState _windowState = WindowState.Normal;

        public VideoDialogViewModel(
            IEventAggregator eventAggregator,
            IApiService apiService,
            IStreamlinkLauncher streamlinkLauncher,
            IVideoSettings videoSettings,
            LibVLC libVLC)
        {
            _eventAggregator = eventAggregator;
            _apiService = apiService;
            _streamlinkLauncher = streamlinkLauncher;
            _videoSettings = videoSettings;
            _libVLC = libVLC;
        }

        public ICommand MouseDownVideoCommand => _mouseDownVideoCommand ??= new DelegateCommand<MouseButtonEventArgs>(MouseDownVideoExecute);
        public ICommand MouseMoveVideoCommand => _mouseMoveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseEnterVideoCommand => _mouseEnterVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseLeaveVideoCommand => _mouseLeaveVideoCommand ??= new DelegateCommand(MouseEnterOrLeaveOrMoveVideoExecute);
        public ICommand MouseMoveControlBarCommand => _mouseMoveControlBarCommand ??= new DelegateCommand(MouseMoveControlBarExecute);
        public ICommand MouseEnterControlBarCommand => _mouseEnterControlBarCommand ??= new DelegateCommand(MouseEnterControlBarExecute);
        public ICommand MouseLeaveControlBarCommand => _mouseLeaveControlBarCommand ??= new DelegateCommand(MouseLeaveControlBarExecute);
        public ICommand TogglePauseCommand => _togglePauseCommand ??= new DelegateCommand(TogglePauseExecute);
        public ICommand ToggleMuteCommand => _toggleMuteCommand ??= new DelegateCommand(ToggleMuteExecute);
        public ICommand FastForwardCommand => _fastForwardCommand ??= new DelegateCommand<string>(FastForwardExecute, CanFastForwardExecute).ObservesProperty(() => IsLive);
        public ICommand SyncSessionCommand => _syncSessionCommand ??= new DelegateCommand(SyncSessionExecute, CanSyncSessionExecute).ObservesProperty(() => IsLive);
        public ICommand ToggleFullScreenCommand => _toggleFullScreenCommand ??= new DelegateCommand(ToggleFullScreenExecute);
        public ICommand MoveToCornerCommand => _moveToCornerCommand ??= new DelegateCommand<WindowLocation?>(MoveToCornerExecute, CanMoveToCornerExecute).ObservesProperty(() => WindowState);
        public ICommand AudioTrackSelectionChangedCommand => _audioTrackSelectionChangedCommand ??= new DelegateCommand<SelectionChangedEventArgs>(AudioTrackSelectionChangedExecute);
        public ICommand ScanChromecastCommand => _scanChromecastCommand ??= new DelegateCommand(ScanChromecastExecute, CanScanChromecastExecute).ObservesProperty(() => RendererDiscoverer);
        public ICommand StartCastVideoCommand => _startCastVideoCommand ??= new DelegateCommand(StartCastVideoExecute, CanStartCastVideoExecute).ObservesProperty(() => SelectedRendererItem);
        public ICommand StopCastVideoCommand => _stopCastVideoCommand ??= new DelegateCommand(StopCastVideoExecute, CanStopCastVideoExecute).ObservesProperty(() => IsCasting);

        public bool IsLive
        {
            get => _isLive;
            set => SetProperty(ref _isLive, value);
        }

        public bool IsCasting
        {
            get => _isCasting;
            set => SetProperty(ref _isCasting, value);
        }

        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            set => SetProperty(ref _mediaPlayer, value);
        }

        public Media Media
        {
            get => _media;
            set => SetProperty(ref _media, value);
        }

        public ObservableCollection<TrackDescription> AudioTrackDescriptions
        {
            get => _audioTrackDescriptions ??= new ObservableCollection<TrackDescription>();
            set => SetProperty(ref _audioTrackDescriptions, value);
        }

        public long Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public long SliderTime
        {
            get => _sliderTime;
            set
            {
                if (SetProperty(ref _sliderTime, value))
                {
                    SetMediaPlayerTime(_sliderTime);
                }
            }
        }

        public TimeSpan DisplayTime
        {
            get => _displayTime;
            set => SetProperty(ref _displayTime, value);
        }

        public RendererDiscoverer RendererDiscoverer
        {
            get => _rendererDiscoverer;
            set => SetProperty(ref _rendererDiscoverer, value);
        }

        public ObservableCollection<RendererItem> RendererItems
        {
            get => _rendererItems ??= new ObservableCollection<RendererItem>();
            set => SetProperty(ref _rendererItems, value);
        }

        public RendererItem SelectedRendererItem
        {
            get => _selectedRendererItem;
            set => SetProperty(ref _selectedRendererItem, value);
        }

        public bool ShowControls
        {
            get => _showControls;
            set => SetProperty(ref _showControls, value);
        }

        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set => SetProperty(ref _resizeMode, value);
        }

        public WindowState WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public override async void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            _token = parameters.GetValue<string>(ParameterNames.TOKEN);
            _contentType = parameters.GetValue<ContentType>(ParameterNames.CONTENT_TYPE);
            _contentUrl = parameters.GetValue<string>(ParameterNames.CONTENT_URL);
            _syncUID = parameters.GetValue<string>(ParameterNames.SYNC_UID);
            Title = parameters.GetValue<string>(ParameterNames.TITLE);
            IsLive = parameters.GetValue<bool>(ParameterNames.IS_LIVE);

            var streamUrl = await GenerateStreamUrlAsync();

            if (IsLive)
            {
                _streamlinkProcess = _streamlinkLauncher.StartStreamlinkExternal(streamUrl, out streamUrl);

                if (_videoSettings.EnableRecording)
                {
                    var recordingStreamUrl = await GenerateStreamUrlAsync();
                    _streamlingRecordingProcess = _streamlinkLauncher.StartStreamlinkRecording(recordingStreamUrl, Title);
                }
            }

            CreateMedia(streamUrl);
            CreateMediaPlayer();
            StartPlayback();

            _showControlsTimer = new Timer(2000) { AutoReset = false };
            _showControlsTimer.Elapsed += ShowControlsTimer_Elapsed;

            _eventAggregator.GetEvent<SyncStreamsEvent>().Subscribe(OnSyncSession);
        }

        public override void OnDialogClosed()
        {
            base.OnDialogClosed();

            _eventAggregator.GetEvent<SyncStreamsEvent>().Unsubscribe(OnSyncSession);

            _showControlsTimer.Stop();
            _showControlsTimer.Elapsed -= ShowControlsTimer_Elapsed;
            _showControlsTimer.Dispose();
            _showControlsTimer = null;

            StopPlayback();
            RemoveMediaPlayer();
            RemoveMedia();

            if (RendererDiscoverer != null)
            {
                RendererDiscoverer.Stop();
                RendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
                RendererDiscoverer.Dispose();
            }

            _streamlinkProcess?.Kill(true);
            _streamlingRecordingProcess?.Kill(true);
        }

        private void Media_DurationChanged(object sender, MediaDurationChangedEventArgs e)
        {
            Duration = e.Duration;
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            SetProperty(ref _sliderTime, e.Time, nameof(SliderTime));
            DisplayTime = TimeSpan.FromMilliseconds(e.Time);
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var audioTrackDescription = MediaPlayer.AudioTrackDescription.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTrackDescriptions.Add(audioTrackDescription);
                        });
                        break;
                }
            }
        }

        private void MediaPlayer_ESDeleted(object sender, MediaPlayerESDeletedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var audioTrackDescription = AudioTrackDescriptions.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTrackDescriptions.Remove(audioTrackDescription);
                        });
                        break;
                }
            }
        }

        private void ShowControlsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowControls = false;
                Mouse.OverrideCursor = Cursors.None;
            });
        }

        private void RendererDiscoverer_ItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
        {
            if (e.RendererItem.CanRenderVideo)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RendererItems.Add(e.RendererItem);
                });
            }
        }

        private void MouseDownVideoExecute(MouseButtonEventArgs args)
        {
            if (args.ChangedButton != MouseButton.Left)
            {
                return;
            }

            switch (args.ClickCount)
            {
                case 1:
                    if (args.Source is DependencyObject dependencyObject)
                    {
                        Window.GetWindow(dependencyObject)?.Owner?.DragMove();
                    }

                    break;

                case 2:
                    if (ToggleFullScreenCommand.CanExecute(null))
                    {
                        ToggleFullScreenCommand.Execute(null);
                    }

                    break;
            }
        }

        private void MouseEnterOrLeaveOrMoveVideoExecute()
        {
            _showControlsTimer?.Stop();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowControls = true;
                Mouse.OverrideCursor = null;
            });

            _showControlsTimer?.Start();
        }

        private static void MouseMoveControlBarExecute()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
        }

        private void MouseEnterControlBarExecute()
        {
            _showControlsTimer?.Stop();
        }

        private void MouseLeaveControlBarExecute()
        {
            _showControlsTimer?.Start();
        }

        private void TogglePauseExecute()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }

        private void ToggleMuteExecute()
        {
            MediaPlayer.ToggleMute();
        }

        private bool CanFastForwardExecute(string arg)
        {
            return !IsLive;
        }

        private void FastForwardExecute(string value)
        {
            if (int.TryParse(value, out var seconds))
            {
                var time = MediaPlayer.Time + seconds * 1000;
                SetMediaPlayerTime(time);
            }
        }

        private bool CanSyncSessionExecute()
        {
            return !IsLive;
        }

        private void SyncSessionExecute()
        {
            if (MediaPlayer.IsPlaying)
            {
                var payload = new SyncStreamsEventPayload(_uniqueIdentifier, _syncUID, MediaPlayer.Time);
                _eventAggregator.GetEvent<SyncStreamsEvent>().Publish(payload);
            }
        }

        private void OnSyncSession(SyncStreamsEventPayload payload)
        {
            if (_uniqueIdentifier != payload.RequesterIdentifier && _syncUID == payload.SyncUID)
            {
                SetMediaPlayerTime(payload.Time, true);
            }
        }

        private void ToggleFullScreenExecute()
        {
            if (WindowState != WindowState.Maximized)
            {
                SetFullScreen();
            }
            else
            {
                SetWindowed();
            }
        }

        private bool CanMoveToCornerExecute(WindowLocation? location)
        {
            return WindowState != WindowState.Maximized && location != null;
        }

        private void MoveToCornerExecute(WindowLocation? location)
        {
            ResizeMode = ResizeMode.NoResize;

            var screen = Screen.FromRectangle(new Rectangle((int)Left, (int)Top, (int)Width, (int)Height));
            var scale = Math.Max(Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.PrimaryScreenWidth, Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.PrimaryScreenHeight);
            var top = screen.WorkingArea.Top / scale;
            var left = screen.WorkingArea.Left / scale;
            var width = screen.WorkingArea.Width / 2D / scale;
            var height = screen.WorkingArea.Height / 2D / scale;

            switch (location)
            {
                case WindowLocation.TopLeft:
                    Top = top;
                    Left = left;
                    break;

                case WindowLocation.TopRight:
                    Top = top;
                    Left = left + width;
                    break;

                case WindowLocation.BottomLeft:
                    Top = top + height;
                    Left = left;
                    break;

                case WindowLocation.BottomRight:
                    Top = top + height;
                    Left = left + width;
                    break;
            }

            Width = width;
            Height = height;
        }

        private void AudioTrackSelectionChangedExecute(SelectionChangedEventArgs args)
        {
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is TrackDescription trackDescription)
            {
                MediaPlayer.SetAudioTrack(trackDescription.Id);
            }
        }

        private bool CanScanChromecastExecute()
        {
            return RendererDiscoverer == null;
        }

        private void ScanChromecastExecute()
        {
            RendererDiscoverer = new RendererDiscoverer(_libVLC);
            RendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            RendererDiscoverer.Start();
        }

        private bool CanStartCastVideoExecute()
        {
            return SelectedRendererItem != null;
        }

        private async void StartCastVideoExecute()
        {
            await ChangeRendererAsync(SelectedRendererItem);
            IsCasting = true;
        }

        private bool CanStopCastVideoExecute()
        {
            return IsCasting;
        }

        private async void StopCastVideoExecute()
        {
            await ChangeRendererAsync(null);
            IsCasting = false;
        }

        private async Task<string> GenerateStreamUrlAsync()
        {
            return await _apiService.GetTokenisedUrlAsync(_token, _contentType, _contentUrl);
        }

        private async Task ChangeRendererAsync(RendererItem renderer)
        {
            var streamTime = MediaPlayer.Time;
            var streamUrl = IsLive ? Media.Mrl : await GenerateStreamUrlAsync();

            StopPlayback();
            RemoveMedia();
            CreateMedia(streamUrl);
            StartPlayback(renderer);

            if (!IsLive)
            {
                SetMediaPlayerTime(streamTime);
            }
        }

        private void SetFullScreen()
        {
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void SetWindowed()
        {
            WindowState = WindowState.Normal;
        }

        private void SetMediaPlayerTime(long time, bool mustBePlaying = false)
        {
            if (!mustBePlaying || MediaPlayer.IsPlaying)
            {
                MediaPlayer.Time = time;
            }
        }

        private void CreateMedia(string url)
        {
            Media = new Media(_libVLC, url, FromType.FromLocation);
            Media.DurationChanged += Media_DurationChanged;
        }

        private void CreateMediaPlayer()
        {
            MediaPlayer = new MediaPlayer(_libVLC)
            {
                EnableHardwareDecoding = true,
                EnableMouseInput = false,
                EnableKeyInput = false,
                FileCaching = 2000,
                NetworkCaching = 4000
            };

            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
        }

        private void StartPlayback(RendererItem renderer = null)
        {
            AudioTrackDescriptions.Clear();
            MediaPlayer.SetRenderer(renderer);
            MediaPlayer.Play(Media);
        }

        private void RemoveMedia()
        {
            Media.DurationChanged -= Media_DurationChanged;
            Media.Dispose();
        }

        private void RemoveMediaPlayer()
        {
            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
            MediaPlayer.Dispose();
        }

        private void StopPlayback()
        {
            MediaPlayer.Stop();
            AudioTrackDescriptions.Clear();
        }
    }
}