using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using System.Reflection;

namespace MarksAssets.VideoPlayerWebGL {
	public partial class VideoPlayerWebGL : MonoBehaviour
	{
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_CreateVideo")]
		private static extern void createVideo(string URL, string cors, string id, bool autoplay, bool loop, bool muted, double volume, double playbackSpeed, IntPtr texturePtr, string events, ref uint canUpdateTexture);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_PlayVideo")]
		private static extern void playVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_PlayVideoPointerDown")]
		private static extern void playVideoPointerDown(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_PauseVideo")]
		private static extern void pauseVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_StopVideo")]
		private static extern void stopVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_unlockVideoPlayback")]
		private static extern void unlockVideoPlayback(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_GetTimeVideo")]
		private static extern double getTimeVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SetTimeVideo")]
		private static extern void setTimeVideo(string id, double time);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_LengthVideo")]
		private static extern double lengthVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SetLoopVideo")]
		private static extern void loopVideo(string id, bool lp);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SetMuteVideo")]
		private static extern void muteVideo(string id, bool mute);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SetAutoplayVideo")]
		private static extern void autoplayVideo(string id, bool autoplay);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_PlayBackSpeedVideo")]
		private static extern void playBackSpeedVideo(string id, double pbspd);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SourceVideo")]
		private static extern void setSourceVideo(string id, string URL, string cors);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_IsPlayingVideo")]
		private static extern bool isPlayingVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_IsPausedVideo")]
		private static extern bool isPausedVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_WidthVideo")]
		private static extern uint widthVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_HeightVideo")]
		private static extern uint heightVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SetVolumeVideo")]
		private static extern void setVolumeVideo(string id, double volume);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_SetCorsVideo")]
		private static extern void setCorsVideo(string id, string cors);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_IsReadyVideo")]
		private static extern bool isReadyVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_DestroyVideo")]
		private static extern void destroyVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_RestartVideo")]
		private static extern void restartVideo(string id);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_UpdateTexture")]
		private static extern void updateTexture(string id);
		
		public enum cors {anonymous = 1, usecredentials = 2};
		public enum srcs {StreamingAssets, External};
		
		[SerializeField]
		private srcs source = srcs.StreamingAssets;
		[HideIf("isStreamingAssets")]
		[SerializeField]
		private string URL;
		[ShowIf("isStreamingAssets")]
		[SerializeField]
		private string fileName;
		[SerializeField]
		private bool autoplay = true;
		[SerializeField]
		private bool loop;
		[SerializeField]
		private bool muted = true;
		[Range(0, 1)]
		[SerializeField]
		private double volume = 1.0;
		[Range(0, 10)]
		[SerializeField]
		private double playbackSpeed = 1.0;
		public RenderTexture targetTexture;
		private string id = "-1";
		[HideIf("isStreamingAssets")]
		[SerializeField]
		private cors crossOrigin = cors.anonymous;
		
		private static uint videoNumber = 0;
		private static uint canUpdateTexture = 0;//this uint is used by all video players with bit operations
		
		void Start() {
			CreateVideo(source, source == srcs.StreamingAssets ? fileName : URL, crossOrigin, autoplay, loop, muted, volume, playbackSpeed, targetTexture, events);//it is redundant here, but this method can be called anywhere, not just in the Start function. If the user decides to destroy the video, this method can be called later.
		}
		
		///Creates a new video and destroys the previous one, if there was any. Only one video instance exists per VideoPlayerWebGL instance at any time.
		///this function is called automatically on Start. You only need to call it if you destroy the video and want to create another one.
		public void CreateVideo(srcs _source, string _path, cors _crossOrigin, bool _autoplay, bool _loop, bool _muted, double _volume, double _playbackSpeed, RenderTexture _targetTexture, evnts _events) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			if (_targetTexture == null) return;
			Destroy();
			source = _source;
			URL = _path;
			crossOrigin = _crossOrigin;
			autoplay = _autoplay;
			loop = _loop;
			muted = _muted;
			volume = _volume < 0.0 ? 0.0 : _volume > 1.0 ? 1.0 : _volume;
			playbackSpeed = _playbackSpeed < 0.0 ? 0 : _playbackSpeed > 10.0 ? 10.0 : _playbackSpeed;
			targetTexture = _targetTexture;
			events = _events;
			
			URL = source == srcs.StreamingAssets ? System.IO.Path.Combine(Application.streamingAssetsPath, URL) : URL;
			targetTexture.colorBuffer.ToString();//see https://issuetracker.unity3d.com/issues/getnativetextureptr-returns-0-on-rendertexture-until-colorbuffer-property-get-is-called
			id = id == "-1" ? "VideoPlayerWebGL_" + videoNumber++ : id;
			foreach (evnts x in Enum.GetValues(typeof(evnts))) if (events.HasFlag(x)) ((Dictionary<string, UnityEvent>)typeof(VideoPlayerWebGL).GetField(x.ToString() + "Dict",  BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Add(id, (UnityEvent)typeof(VideoPlayerWebGL).GetField(x.ToString(), BindingFlags.Instance | BindingFlags.Public).GetValue(this));
			setUnityFunctions(canplayClbks, canplaythroughClbks, completeClbks, durationchangeClbks, emptiedClbks, endedClbks, loadeddataClbks, loadedmetadataClbks, pauseClbks, playClbks, playingClbks, progressClbks, ratechangeClbks, seekedClbks, seekingClbks, stalledClbks, suspendClbks, timeupdateClbks, volumechangeClbks, waitingClbks);
			createVideo(URL, source == srcs.StreamingAssets ? "" : (crossOrigin == 0 || crossOrigin == cors.anonymous) ? "anonymous" : "use-credentials", id, autoplay, loop, muted, volume, playbackSpeed, targetTexture.GetNativeTexturePtr(), events.ToString(), ref canUpdateTexture);
			#endif
		}

		void Update() {
			#if UNITY_WEBGL && !UNITY_EDITOR
				if ( (canUpdateTexture & (uint)(1 << (int)videoNumber - 1) ) != 0)//if can update this video
					updateTexture(id);
			#endif
		}
		
		//Plays the video from its current time. If the current time is the end of the video, it plays from the start.
		//you can not play multiple videos at the same time on Safari, unless they are muted. On chrome there is no problem.
		//search for "Multiple Simultaneous Audio or Video Streams" here https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/Using_HTML5_Audio_Video/Device-SpecificConsiderations/Device-SpecificConsiderations.html
		public void Play() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			playVideo(id);
			#endif
		}
		
		//same as above, but it only works if you call it on a pointerdown event. 
		//On Safari, you need to either call this method once or UnlockVideoPlayback once. And then you can use Play() normally.
		//Alternatively, you can simply always call PlayPointerDown().
		public void PlayPointerDown() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			playVideoPointerDown(id);
			#endif
		}
		
		///	this function needs to be called on a pointerdown event, just once.
		///	necessary to play if video is unmuted on Safari. Even if it autoplays muted, if it is later unmuted it won't play again unless this function is called.
		/// use this function or PlayPointerDown()
		public void UnlockVideoPlayback() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			unlockVideoPlayback(id);
			#endif
		}
		
		//restarts the video. Be sure to have called PlayPointerDown or UnlockVideoPlayback on Safari at least once.
		public void Restart() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			restartVideo(id);
			#endif
		}
		
		//pauses video
		public void Pause() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			pauseVideo(id);
			#endif
		}
		
		//stops the video. That is, set video's time to 0 and pause.
		public void Stop() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			stopVideo(id);
			#endif
		}
		
		//returns the current time of the video, in seconds.
		public double Time() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return getTimeVideo(id);
			#else
			return 0.0f;
			#endif
		}
		
		///seeks the video. If you pass a value that is less than 0.0 it will become 0.0. If the value is bigger than the video's length, it becomes the video's length.
		///if you are trying to set the current time of the video but it always goes back to 0, see https://stackoverflow.com/questions/36783521/why-does-setting-currenttime-of-html5-video-element-reset-time-in-chrome
		public void Time(double time) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			double videoLength = lengthVideo(id);
			time = time < 0.0 ? 0.0 : time > videoLength ? videoLength : time;
			setTimeVideo(id, time);
			#endif
		}
		
		//returns length of the video, in seconds.
		public double Length() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return lengthVideo(id);
			#else
			return 0.0f;
			#endif
		}
		
		//returns true if video is set to loop, false if not.
		public bool IsSetToLoop() {
			return loop;
		}
		
		//set true if you want the video to loop, false if not.
		public void Loop(bool lp) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			loop = lp;
			loopVideo(id, loop);
			#endif
		}
		
		//returns true if the video is muted, false if not.
		public bool IsMuted() {
			return muted;
		}
		
		public void Muted(bool mute) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			muted = mute;
			muteVideo(id, muted);
			#endif
		}
		
		//returns true if the autoplay attribute is set, false if not.
		public bool IsSetToAutoPlay() {
			return autoplay;
		}
		
		///changes the autoplay attribute of the video. It's only useful to use this if you change the source of the video at runtime, and
		///want to autoplay a video that previously was set to not autoplay, and vice versa.
		///if set to true, it will autoplay
		///if set to false, it won't.
		///on chrome, autoplay will work normally if the video is muted. If the video is not muted, it will only autoplay if the user touches anywhere on the screen before the video loads
		///on safari, autoplay only works if the video is muted. You have 2 options: always call PlayPointerDown method on a pointerdown event, 
		///or call UnlockVideoPlayback on a pointerdown event once, and then call the Play method whenever you want(onClick for example). 
		public void Autoplay(bool _autoplay) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			autoplay = _autoplay;
			autoplayVideo(id, autoplay);
			#endif
		}
		
		//returns the current playback speed of the video. Default is 1.0
		public double PlaybackSpeed() {
			return playbackSpeed;
		}
		
		///sets the playback speed of the video. It can be anything from 0.0(inclusive) to 10.0(inclusive)
		///smaller values than 0.0 will be set to 0.0, and larger values than 10.0 will be set to 10.0
		///you can not play a video in reverse with a negative playback speed.
		public void PlaybackSpeed(double pbspd) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			playbackSpeed = pbspd < 0.0 ? 0 : pbspd > 10.0 ? 10.0 : pbspd;
			playBackSpeedVideo(id, playbackSpeed);
			#endif
		}
		
		///returns the current source of the video.
		public string Source() {
			return URL;
		}

		/// example external source: 'Source(srcs.External, "https://d8d913s460fub.cloudfront.net/videoserver/cat-test-video-320x240.mp4")'
		///	example StreamingAssets(local) source: 'Source(srcs.StreamingAssets, "cat-test-video-320x240.mp4")'		
		public void Source(srcs src, string path, cors crossorigin = cors.anonymous) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			source = src;
			URL = source == srcs.StreamingAssets ? System.IO.Path.Combine(Application.streamingAssetsPath, path) : path;
			setSourceVideo(id, URL, source == srcs.StreamingAssets ? "" : (crossOrigin == 0 || crossOrigin == cors.anonymous) ? "anonymous" : "use-credentials");
			#endif
		}
		
		//returns true if the video is currently playing, false if not.
		public bool IsPlaying() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return isPlayingVideo(id);
			#else
			return false;
			#endif
		}
		
		//returns if the video is currently paused(true) or not(false)
		public bool IsPaused() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return isPausedVideo(id);
			#else
			return false;
			#endif
		}
	
		//returns the current width of the video, in pixels
		public uint Width() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return widthVideo(id);
			#else
			return 0;
			#endif
		}
		
		//returns the current height of the video, in pixels
		public uint Height() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return heightVideo(id);
			#else
			return 0;
			#endif
		}
		
		//returns the current volume of the video
		public double Volume() {
			return volume;
		}
		
		//returns the current crossorigin configuration. It can either be cors.anonymous or cors.usecredentials
		public cors CORS() {
			return crossOrigin;
		}
		
		//returns the current source type. It can either be srcs.StreamingAssets or srcs.External
		public srcs SourceType() {
			return source;
		}
		
		///sets the cross origin attribute on the video. Irrelevant if the video is local(StreamingAssets folder), but required if the source is external.
		///You may get an error like "Access to video at 'urlVideo' from origin 'urlOrigin' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource."
		///Please check here https://stackoverflow.com/questions/41822932/html5-video-doesnt-play-with-crossorigin-anonymous .
		///The CORS configuration needs to be set on the provider of the video as well, not just here.
		public void CORS(cors crossorigin) {//warning: if the source of your video is external, you must set the CORS to "anonymous" or "use-credentials" depending on how the server providing the video is configured. It won't work without the CORS attribute.
			#if UNITY_WEBGL && !UNITY_EDITOR
			crossOrigin = crossorigin;
			setCorsVideo(id, crossorigin == 0 ? "" : crossorigin == cors.anonymous ? "anonymous" : "use-credentials");
			#endif
		}
		
		///Change the volume of the video. Varies from 0.0(inclusive) to 1.0(inclusive). A value that is less than 0.0 becomes 0.0, and if it's higher than 1.0 it becomes 1.0
		///you can not change the volume on mobile Safari
		///search for "Volume Control in JavaScript" here: https://developer.apple.com/library/archive/documentation/AudioVideo/Conceptual/Using_HTML5_Audio_Video/Device-SpecificConsiderations/Device-SpecificConsiderations.html
		public void Volume(double vol) {
			#if UNITY_WEBGL && !UNITY_EDITOR
			volume = vol < 0.0 ? 0.0 : vol > 1.0 ? 1.0 : vol;
			setVolumeVideo(id, volume);
			#endif
		}
		
		//returns if video is ready to be played(true) or not(false)
		public bool IsReady() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			return isReadyVideo(id);
			#else
			return false;
			#endif
		}
		
		///Destroys the video from the html side, releases the texture, unregisters all Unity Events and removes all non persistent(runtime) callbacks from them.
		///the callbacks you added in the inspector won't be removed
		///all the other variables are left unchanged, so you can call create with the getters to recreate the video on the same state it was before being destroyed.
		public void Destroy() {
			#if UNITY_WEBGL && !UNITY_EDITOR
			destroyVideo(id);
			foreach (evnts x in Enum.GetValues(typeof(evnts))) {
				((Dictionary<string, UnityEvent>)typeof(VideoPlayerWebGL).GetField(x.ToString() + "Dict",  BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Remove(id);//if the entry doesn't exist, nothing happens.
				((UnityEvent)typeof(VideoPlayerWebGL).GetField(x.ToString(), BindingFlags.Instance | BindingFlags.Public).GetValue(this)).RemoveAllListeners();//remove all non persistent listeners
			}
			if (targetTexture != null) targetTexture.Release();
			#endif
		}
		
		//called when unloading scene. Destroys video
		void OnDestroy() {
			Destroy();
		}
		
		//used for the inspector
		private bool isStreamingAssets() {return source == srcs.StreamingAssets;}
	}
}
