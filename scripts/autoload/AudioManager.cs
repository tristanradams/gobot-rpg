using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RpgCSharp.scripts.autoload;

public partial class AudioManager : Node
{
    private const string MasterBus = "Master";
    private const string MusicBus = "Music";
    private const string SfxBus = "SFX";

    private AudioStreamPlayer _musicPlayer;
    private List<AudioStreamPlayer> _sfxPlayers = new();
    private int _sfxPoolSize = 8;

    private AudioStream _currentMusic;

    public override void _Ready()
    {
        SetupMusicPlayer();
        SetupSfxPool();
    }

    private void SetupMusicPlayer()
    {
        _musicPlayer = new AudioStreamPlayer();
        _musicPlayer.Bus = MusicBus;
        AddChild(_musicPlayer);
    }

    private void SetupSfxPool()
    {
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            var player = new AudioStreamPlayer();
            player.Bus = SfxBus;
            AddChild(player);
            _sfxPlayers.Add(player);
        }
    }

    public void PlayMusic(AudioStream stream, float fadeIn = 0.5f)
    {
        if (stream == _currentMusic) return;

        _currentMusic = stream;
        _musicPlayer.Stream = stream;
        _musicPlayer.VolumeDb = fadeIn > 0 ? -80.0f : 0.0f;
        _musicPlayer.Play();

        if (fadeIn > 0)
        {
            var tween = CreateTween();
            tween.TweenProperty(_musicPlayer, "volume_db", 0.0f, fadeIn);
        }
    }

    public void StopMusic(float fadeOut = 0.5f)
    {
        if (fadeOut > 0)
        {
            var tween = CreateTween();
            tween.TweenProperty(_musicPlayer, "volume_db", -80.0f, fadeOut);
            tween.TweenCallback(Callable.From(() => _musicPlayer.Stop()));
        }
        else
        {
            _musicPlayer.Stop();
        }
        _currentMusic = null;
    }

    public void PlaySfx(AudioStream stream, float volumeDb = 0.0f)
    {
        foreach (var player in _sfxPlayers.Where(player => !player.Playing))
        {
            player.Stream = stream;
            player.VolumeDb = volumeDb;
            player.Play();
            return;
        }

        // All players busy, use the first one
        _sfxPlayers[0].Stream = stream;
        _sfxPlayers[0].VolumeDb = volumeDb;
        _sfxPlayers[0].Play();
    }

    public void SetMasterVolume(float volume)
    {
        AudioServer.SetBusVolumeDb(
            AudioServer.GetBusIndex(MasterBus),
            Mathf.LinearToDb(volume)
        );
    }

    public void SetMusicVolume(float volume)
    {
        AudioServer.SetBusVolumeDb(
            AudioServer.GetBusIndex(MusicBus),
            Mathf.LinearToDb(volume)
        );
    }

    public void SetSfxVolume(float volume)
    {
        AudioServer.SetBusVolumeDb(
            AudioServer.GetBusIndex(SfxBus),
            Mathf.LinearToDb(volume)
        );
    }
}