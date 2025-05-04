using System;
using System.Threading.Tasks;
using Godot;
using LibVLCSharp.Shared;

namespace libVLCsolution.Scripts;

public partial class App : Node
{
	private Media _media;
	private IntPtr windowHandle;
	private MediaPlayer mediaPlayer;
	private LibVLC libVLC;
	private Window newWindow;
	[Export] private LineEdit CameraID;

	public override void _Ready()
	{
		base._Ready();
	}

	public void _on_button_button_down()
	{
		StartVideo();
	}

	// This is the method that starts the video playback
	public void StartVideo()
	{
		// If so, we'll create a new window
		newWindow = new Window();
		// Add the new window to the scene tree
		AddChild(newWindow);
		// Set the name of the window
		newWindow.Name = "Video Player";
		// Subscribe to the CloseRequested event so we can clean up
		newWindow.CloseRequested += NewWindowOnCloseRequested;
		// Set the size and position of the window
		newWindow.PopupCentered(new Vector2I(700, 500));
		// Get the window handle of the new window
		checked
		{
			windowHandle = (IntPtr)DisplayServer.WindowGetNativeHandle(DisplayServer.HandleType.WindowHandle, 1);
		}

		// Initialize the libVLC library
		libVLC = new LibVLC("--low-delay", "--network-caching=0");
		// Create a new MediaPlayer instance
		mediaPlayer = new MediaPlayer(libVLC);

		string baseIP = "192.168.1.";
		string port = "554";

		string user = "admin";
		string password = "";

		string ip = baseIP + CameraID.Text + ':' + port;
		// Create a new Media instance with the path to the video file
		_media = new Media(libVLC, new Uri($"rtsp://{ip}/user={user}&password={password}&channel=1&stream=0.sdp"));
		// Set the window handle of the video player to the Godot window handle
		mediaPlayer.Hwnd = windowHandle;
		// Play the video
		mediaPlayer.Play(_media);
		// Subscribe to the EndReached event so we can clean up
		mediaPlayer.EndReached += MediaPlayerOnEndReached;
		// We can clean up the Media instance now
		_media.Dispose();
	}

	private async void MediaPlayerOnEndReached(object sender, EventArgs e)
	{
		// There will be issues if you try to clean up exactly when the video ends
		// so we'll wait a tiny bit before cleaning up
		await Task.Delay(100);
		// Clean up
		mediaPlayer.Dispose();
		libVLC.Dispose();
		newWindow.QueueFree();
	}
	
	private void NewWindowOnCloseRequested()
	{
		// Clean up
		newWindow.QueueFree();
		mediaPlayer.Dispose();
		libVLC.Dispose();
	}
	
	// Clean up if there's an early close
	public override void _ExitTree()
	{
		base._ExitTree();
		_media?.Dispose();
	}
}
