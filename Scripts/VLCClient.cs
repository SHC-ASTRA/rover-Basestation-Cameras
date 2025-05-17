using Godot;
using System;
using LibVLCSharp.Shared;
using static roverBasestationCameras.StreamController;
namespace roverBasestationCameras
{
	public partial class VLCClient : Node
	{
		public static LibVLC libVLC;
		public static LibVLC libVLCF;
		public static readonly Vector2I streamSize;
		private const string baseIP = "192.168.1.", port = "554", user = "admin", password = "";

		private StreamController Controller;

		public Window playerWindow;
		private long windowHandle;
		public bool shouldbeFullscreen;

		private Media _media;
		private MediaPlayer mediaPlayer;

		static VLCClient()
		{
			Vector2I t = DisplayServer.ScreenGetSize();
			streamSize.X = Mathf.RoundToInt(t.X * 0.25f);
			streamSize.Y = (int)((t.Y - 180) * 0.5f);
		}

		public override void _Ready()
		{
			libVLC ??= new LibVLC();
			libVLCF ??= new LibVLC("--video-filter=transform{type=vflip}");

			Controller = GetParent() as StreamController;

			GD.Print(Controller.StreamID);

			if (playerWindow == null)
			{
				playerWindow = new Window()
				{
					InitialPosition = Window.WindowInitialPosition.Absolute,
					CurrentScreen = GetWindow().CurrentScreen,
					AlwaysOnTop = true,
					Unfocusable = true,
					Size = streamSize,
					Title = Controller.Location + ':' + (Controller.StreamID = int.Parse(Controller.Name.ToString().Split("Controller")[1])),
				};

				GetParent().GetParent().CallDeferred(Node.MethodName.AddChild, playerWindow);

				playerWindow.Visible = false;

				playerWindow.CloseRequested += CloseReq;
			}
		}

		public void Open(ref LibVLC owner)
		{
			playerWindow.Visible = true;
			openedControllers.Push(Controller);
			windowHandle = DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, playerWindow.GetWindowId());
			StartVideo(ref owner);
		}
		public void Close()
		{
			playerWindow.Visible = false;
			if (openedControllers.Peek() == Controller)
				openedControllers.Pop();
			mediaPlayer?.Dispose();
			_media?.Dispose();
		}
		public void Close(object s, EventArgs e)
		{
			playerWindow.SetDeferred(Window.PropertyName.Visible, false);
			mediaPlayer?.Dispose();
			_media?.Dispose();
		}

		public bool shouldPop;
		public void StartVideo(ref LibVLC owner)
		{
			mediaPlayer = new MediaPlayer(owner);


			string ip = baseIP + Controller.id + ':' + port;
			_media = new Media(owner, new Uri($"rtsp://{ip}/user={user}&password={password}&channel=1&stream=0.sdp?"));

#if GODOT_WINDOWS
			checked
			{
				mediaPlayer.Hwnd = (IntPtr)windowHandle;
			}
#else
			checked
			{
				mediaPlayer.XWindow = (uint)windowHandle;
			}
#endif

			mediaPlayer.EndReached += Close;

			if (shouldPop)
				mediaPlayer.Playing += PopInteract;

			mediaPlayer.Play(_media);
		}

		public override void _ExitTree()
		{
			base._ExitTree();
			_media?.Dispose();
		}
		public void CloseReq()
		{
			Close();
		}

		public void PopInteract(object o, EventArgs e)
		{
			shouldPop = false;
			openedControllers.Pop();
		}
	}
}
