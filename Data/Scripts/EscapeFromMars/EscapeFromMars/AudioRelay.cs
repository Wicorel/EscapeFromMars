namespace Duckroll
{
	internal interface IAudioRelay
	{
		void QueueAudioMessageOnAllClients(IAudioClip audioClip);
	}
}