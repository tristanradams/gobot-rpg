using Godot;
using RpgCSharp.scripts.autoload;

namespace RpgCSharp.scripts.levels;

public partial class Main : Node2D
{
	[Export] public AudioStream BackgroundMusic { get; set; }

	public override void _Ready()
	{
		var gameManager = GetNode<GameManager>("/root/GameManager");
		gameManager.StartGame();

		if (BackgroundMusic != null)
		{
			var audioManager = GetNode<AudioManager>("/root/AudioManager");
			audioManager.PlayMusic(BackgroundMusic);
		}
	}
}
