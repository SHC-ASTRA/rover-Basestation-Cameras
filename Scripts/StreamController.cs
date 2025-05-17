using Godot;
using System.Collections.Generic;
using static roverBasestationCameras.VLCClient;
namespace roverBasestationCameras
{
	public partial class StreamController : Node
	{
		[Export] public VLCClient client;
		[Export] public string Location;
		[Export] public int id;
		
		public int StreamID;
		public bool flipped;
		public static Stack<StreamController> openedControllers = new();
		public override void _Ready()
		{
			client = GetChild(0) as VLCClient;

			client.playerWindow.Size = streamSize;

			Button shortc = client.GetChild(0).GetChild(0).GetChild(0) as Button;
			shortc.Text = Location;
			shortc.Shortcut = new Shortcut()
			{
				Events =
				[
					new InputEventKey()
					{
						Keycode = (Key)(48 + StreamID),
						Unicode = 48 + StreamID,
						CtrlPressed = false
					},
					new InputEventKey()
					{
						Keycode = (Key)(48 + StreamID),
						Unicode = 48 + StreamID,
						CtrlPressed = true
					}
				]
			};
		}

		public void _on_panel_toggled(bool on)
		{
			if (on)
			{
				Reposition();

				Open();

				return;
			}
			else if (Input.IsKeyPressed(Key.Ctrl)) client.Close();
			else openedControllers.Push(this);
		}

		public void Reposition()
		{
			Vector2I t = GetWindow().Position;
			t.X += (StreamID-1) % 4 * streamSize.X;
			t.Y += Mathf.FloorToInt((StreamID-1) * 0.25f) * streamSize.Y + 180;

			client.playerWindow.Position = t;
		}

		public void Open()
		{
			if (flipped)
				client.Open(ref VLCClient.libVLCF);
			else client.Open(ref VLCClient.libVLC);
		}

		public void Refresh()
		{
			client.Close();
			if (flipped)
				client.Open(ref VLCClient.libVLCF);
			else client.Open(ref VLCClient.libVLC);
		}

		public void _on_refresh_stream_button_down() => Refresh();

		public void _on_flip_vertically_button_down()
		{
			flipped = !flipped;		
			Refresh();
		}
		public void _on_fullscreen_button_down()
		{
			switch (client.playerWindow.Mode)
			{
				case Window.ModeEnum.Fullscreen:
					client.playerWindow.Mode = Window.ModeEnum.Windowed;
					client.playerWindow.Size = streamSize;
					client.Close();
					_on_panel_toggled(true);
					GetWindow().GrabFocus();
					break;
				default:
					client.playerWindow.Mode = Window.ModeEnum.Fullscreen;
					Refresh();
					break;
			}
		}
		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventKey eventKey)
			{
				if (eventKey.Pressed && eventKey.IsCommandOrControlPressed())
				{
					if (openedControllers.Peek() == this)
					{
						switch (eventKey.Keycode)
						{
							case Key.F:
							case Key.Escape:
								_on_fullscreen_button_down();
								break;
							case Key.R:
							case Key.Refresh:
								client.shouldPop = true;
								Refresh();
								openedControllers.Push(this);
								break;
							case Key.V:
								_on_flip_vertically_button_down();
								break;
						}
					}
				}
			}
		}
	}
}
