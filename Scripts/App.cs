using Godot;
using System.Collections.Generic;

namespace roverBasestationCameras
{
	public partial class App : Node
	{
		public static App self;
		public static int StreamC = 0;
		public static List<VLCClient> Clients = [];
		[Export] public LineEdit CameraID;

		public override void _Ready()
		{
			base._Ready();
			self = this;
		}

		public void _on_button_button_down()
		{
			StreamC++;
			Clients.Add(new VLCClient());
			AddChild(Clients[^1]);
		}
	}
}
