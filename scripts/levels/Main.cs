using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.levels;

public partial class Main : Node2D
{
	[Export] public AudioStream BackgroundMusic { get; set; }

	public override void _Ready()
	{
		if (BackgroundMusic == null) return;
		var audioManager = GetNode<AudioManager>("/root/AudioManager");
		audioManager.PlayMusic(BackgroundMusic);
	}
}
