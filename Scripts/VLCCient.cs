using Godot;
using System;
using System.Threading;
using LibVLCSharp.Shared;
using static roverBasestationCameras.App;

namespace roverBasestationCameras
{
    public partial class VLCClient : Node
    {
        private const string baseIP = "192.168.1.", port = "554", user = "admin", password = "";

        private Window newWindow;
        private Media _media;
        private IntPtr windowHandle;
        private MediaPlayer mediaPlayer;
        private LibVLC libVLC;

        public override void _Ready()
        {
            base._Ready();
            StartVideo();
        }

        // This is the method that starts the video playback
        public void StartVideo()
        {
            newWindow = new Window
            {
                InitialPosition = Window.WindowInitialPosition.CenterMainWindowScreen
            };

            newWindow.CloseRequested += Cleanup;

            checked
            {
                windowHandle = (IntPtr)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, StreamC);
            }


            libVLC = new LibVLC("--network-caching=0", "--rtsp-frame-buffer-size=0");
            mediaPlayer = new MediaPlayer(libVLC);


            string ip = baseIP + self.CameraID.Text + ':' + port;
            _media = new Media(libVLC, new Uri($"rtsp://{ip}/user={user}&password={password}&channel=1&stream=0.sdp"));

            mediaPlayer.Hwnd = windowHandle;

            mediaPlayer.Play(_media);

            mediaPlayer.EndReached += (sender, args) => ThreadPool.QueueUserWorkItem(_ => MediaPlayerOnEndReached(sender, args));

            _media.Dispose();
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            _media?.Dispose();
        }

        private void MediaPlayerOnEndReached(object sender, EventArgs e)
        {
            // There will be issues if you try to clean up exactly when the video ends
            // so we'll wait a tiny bit before cleaning up
            Thread.Sleep(100);
            Cleanup();
        }

        private void Cleanup()
        {
            newWindow.QueueFree();
            mediaPlayer.Dispose();
            libVLC.Dispose();

            Clients.Remove(this);
        }
    }
}