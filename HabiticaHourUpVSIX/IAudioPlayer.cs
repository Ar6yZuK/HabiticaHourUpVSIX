namespace HabiticaHourUpVSIX;
#nullable enable

internal interface IAudioPlayer
{
	string? AudioPath { get; set; }
	void Play();
}
internal class SoundPlayer : IAudioPlayer
{
	private readonly System.Media.SoundPlayer _soundPlayer = new();

	public string? AudioPath { get => _soundPlayer.SoundLocation; set => _soundPlayer.SoundLocation = value; }

	public void Play()
		=> _soundPlayer.PlaySync();
}
internal class AudioPlayer : IAudioPlayer
{
	private readonly WMPLib.WindowsMediaPlayer _player = new();

	public string? AudioPath { get => _player.URL; set => _player.URL = value; }

	public void Play()
		=> _player.controls.play();
}