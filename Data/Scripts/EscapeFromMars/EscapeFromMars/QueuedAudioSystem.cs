using System;
using System.Collections.Generic;
using EscapeFromMars;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;

namespace Duckroll
{
	public class QueuedAudioSystem : ModSystemUpdatable
	{
		private MyEntity3DSoundEmitter localPlayerSoundEmitter;
		private long playerEntityId = -1;
		private readonly Queue<IAudioClip> waitingClips = new Queue<IAudioClip>();
		private int timeUntilNextAudioSeconds;
		private readonly Random random = new Random();
		internal IAudioRelay AudioRelay { get; set; }

		public void PlayAudioRandomChance(double probability, IAudioClip audioClip)
		{
			if (probability > 0.99 || probability <= random.NextDouble())
			{
				PlayAudio(audioClip);
			}
		}

		public void PlayAudioRandomChance(double probability, params IAudioClip[] audioClips)
		{
			if (probability > 0.99 || probability <= random.NextDouble())
			{
				PlayAudio(audioClips);
			}
		}

		public void PlayAudio(params IAudioClip[] audioClips)
		{
			//TODO check for null or empty array? We can pass in nothing by accident...
			PlayAudio(audioClips[random.Next(audioClips.Length)]);
		}

		public void PlayAudio(IAudioClip clip)
		{
			if (timeUntilNextAudioSeconds > 0)
			{
				waitingClips.Enqueue(clip);
			}
			else
			{
				PlayClip(clip);
			}

			// Play message to other clients if we are the server
			if (AudioRelay != null)
			{
				AudioRelay.QueueAudioMessageOnAllClients(clip);
			}
		}

		private void PlayClip(IAudioClip clip)
		{
			var player = MyAPIGateway.Session.Player;
			var ent = player?.Controller?.ControlledEntity?.Entity;
			if (ent != null)
			{
				if (playerEntityId != ent.EntityId)
				{
					if (localPlayerSoundEmitter != null) // Player has died and lost their old sound emitter
					{
						localPlayerSoundEmitter.StopSound(true);
					}

					localPlayerSoundEmitter = new MyEntity3DSoundEmitter(ent as VRage.Game.Entity.MyEntity);
					playerEntityId = ent.EntityId;
				}
                string audiofile = clip.Filename;
                if (!string.IsNullOrWhiteSpace(audiofile))
                {
                    var soundPair = new MySoundPair(audiofile);
                    localPlayerSoundEmitter.StopSound(true);
                    localPlayerSoundEmitter.PlaySingleSound(soundPair);
                }
			}

            // Added V22
            //MyAPIGateway.Multiplayer.MultiplayerActive
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                //            MyVisualScriptLogicProvider.SendChatMessage(clip.Subtitle, clip.Speaker, 0, clip.Font);
                string[] aLines = clip.Subtitle.Split('\n');
                foreach (var line in aLines)
                {
                    MyVisualScriptLogicProvider.SendChatMessage(line, clip.Speaker, 0, clip.Font);
                }
            }
            // chat, notifications, billboards... Bad on DS.
            // The following should NOT be done on  DS because nowhere to show it..
            MyAPIGateway.Utilities.ShowNotification(clip.Speaker + ": " + clip.Subtitle, clip.DisappearTimeMs, clip.Font);
            timeUntilNextAudioSeconds = clip.DisappearTimeMs / 1000 + 2; // Add a little 2 second pause between them
		}

		public override void Update60()
		{
			if (timeUntilNextAudioSeconds > 0)
			{
				timeUntilNextAudioSeconds--;
			}

			if (timeUntilNextAudioSeconds == 0 && waitingClips.Count > 0)
			{
				PlayClip(waitingClips.Dequeue());
			}
		}
	}
}