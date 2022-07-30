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
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_setUnityFunctions")]
		private static extern void setUnityFunctions(Action<string>canplayClbks, Action<string>canplaythroughClbks, Action<string>completeClbks, Action<string>durationchangeClbks, Action<string>emptiedClbks, Action<string>endedClbks, Action<string>loadeddataClbks, Action<string>loadedmetadataClbks, Action<string>pauseClbks, Action<string>playClbks, Action<string>playingClbks, Action<string>progressClbks, Action<string>ratechangeClbks, Action<string>seekedClbks, Action<string>seekingClbks, Action<string>stalledClbks, Action<string>suspendClbks, Action<string>timeupdateClbks, Action<string>volumechangeClbks, Action<string>waitingClbks);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_RegisterEvent")]
		private static extern void registerEvent(string id, string evt);
		[DllImport("__Internal", EntryPoint="VideoPlayerWebGL_UnregisterEvent")]
		private static extern void unregisterEvent(string id, string evt);
		
		[Flags]
		public enum evnts {canplay = 1, canplaythrough = 2, complete = 4, durationchange = 8, emptied = 16, ended = 32, loadeddata = 64, loadedmetadata = 128, pause = 256, play = 512, playing = 1024, progress = 2048, ratechange = 4096, seeked = 8192, seeking = 16384, stalled = 32768, suspend = 65536, timeupdate = 131072, volumechange = 262144, waiting = 524288};

		[SerializeField]
		private evnts events = evnts.ended;
		[ShowIf("HasCanplayFlag")]
		public UnityEvent canplay;
		[ShowIf("HasCanplaythroughFlag")]
		public UnityEvent canplaythrough;
		[ShowIf("HasCompleteFlag")]
		public UnityEvent complete;
		[ShowIf("HasDurationchangeFlag")]
		public UnityEvent durationchange;
		[ShowIf("HasEmptiedFlag")]
		public UnityEvent emptied;
		[ShowIf("HasEndedFlag")]
		public UnityEvent ended;
		[ShowIf("HasLoadeddataFlag")]
		public UnityEvent loadeddata;
		[ShowIf("HasLoadedmetadataFlag")]
		public UnityEvent loadedmetadata;
		[ShowIf("HasPauseFlag")]
		public UnityEvent pause;
		[ShowIf("HasPlayFlag")]
		public UnityEvent play;
		[ShowIf("HasPlayingFlag")]
		public UnityEvent playing;
		[ShowIf("HasProgressFlag")]
		public UnityEvent progress;
		[ShowIf("HasRatechangeFlag")]
		public UnityEvent ratechange;
		[ShowIf("HasSeekedFlag")]
		public UnityEvent seeked;
		[ShowIf("HasSeekingFlag")]
		public UnityEvent seeking;
		[ShowIf("HasStalledFlag")]
		public UnityEvent stalled;
		[ShowIf("HasSuspendFlag")]
		public UnityEvent suspend;
		[ShowIf("HasTimeupdateFlag")]
		public UnityEvent timeupdate;
		[ShowIf("HasVolumechangeFlag")]
		public UnityEvent volumechange;
		[ShowIf("HasWaitingFlag")]
		public UnityEvent waiting;
		
		private static Dictionary<string, UnityEvent> canplayDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> canplaythroughDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> completeDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> durationchangeDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> emptiedDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> endedDict = new Dictionary<string, UnityEvent>();
        private static Dictionary<string, UnityEvent> loadeddataDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> loadedmetadataDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> pauseDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> playDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> playingDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> progressDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> ratechangeDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> seekedDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> seekingDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> stalledDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> suspendDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> timeupdateDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> volumechangeDict = new Dictionary<string, UnityEvent>();
		private static Dictionary<string, UnityEvent> waitingDict = new Dictionary<string, UnityEvent>();
		
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void canplayClbks(string id) {canplayDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void canplaythroughClbks(string id) {canplaythroughDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void completeClbks(string id) {completeDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void durationchangeClbks(string id) {durationchangeDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void emptiedClbks(string id) {emptiedDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void endedClbks(string id) {endedDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void loadeddataClbks(string id) {loadeddataDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void loadedmetadataClbks(string id) {loadedmetadataDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void pauseClbks(string id) {pauseDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void playClbks(string id) {playDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void playingClbks(string id) {playingDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void progressClbks(string id) {progressDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void ratechangeClbks(string id) {ratechangeDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void seekedClbks(string id) {seekedDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void seekingClbks(string id) {seekingDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void stalledClbks(string id) {stalledDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void suspendClbks(string id) {suspendDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void timeupdateClbks(string id) {timeupdateDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void volumechangeClbks(string id) {volumechangeDict[id].Invoke();}
		[MonoPInvokeCallback(typeof(Action<string>))]
		private static void waitingClbks(string id) {waitingDict[id].Invoke();}
		
		///registers one event, or multiple events.
		///For example: videoPlayerWebGLInstance.RegisterEvent(VideoPlayerWebGL.evnts.timeupdate | VideoPlayerWebGL.evnts.play) will register 2 events.
		///You can add and remove callbacks from the UnityEvents directly, but you can only subscribe to an event with this method. For example
		///first you add the callbacks that you want
		///myAction += myCallback;
		///videoPlayerWebGLInstance.play.AddListener(myAction);
		///then you call videoPlayerWebGLInstance.RegisterEvent(VideoPlayerWebGL.evnts.play) to make your play UnityEvent be invoked(and all of its callbacks) when the video plays
		///you only need to call this method if you destroyed the video and recreated it without registering the event again, or if you didn't subscribe to the event in the inspector, or if you unregistered the event using the UnregisterEvent method.
		///if you want to register all events, pass (VideoPlayerWebGL.evnts)(-1) as input
		public void RegisterEvent(evnts evt) {
			foreach (evnts x in Enum.GetValues(typeof(evnts))) {
				if (!events.HasFlag(x)) {//if the event is not registered
					if (evt.HasFlag(x))//and the user wants to register it
						((Dictionary<string, UnityEvent>)typeof(VideoPlayerWebGL).GetField(x.ToString() + "Dict",  BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Add(id, (UnityEvent)typeof(VideoPlayerWebGL).GetField(x.ToString(), BindingFlags.Instance | BindingFlags.Public).GetValue(this));
				} else {//if the event is already registered
					evt &= ~x;//if events already has the flag, unset from input(if input doesn't have it, nothing changes. But if it does, unset)
				}
			}
			
			events |= evt;//set flag
			registerEvent(id, evt.ToString());
		}
		
		///unregisters one event, or multiple events.
		///For example: videoPlayerWebGLInstance.UnregisterEvent(VideoPlayerWebGL.evnts.timeupdate | VideoPlayerWebGL.evnts.play) will unregister 2 events.
		///So on this example it means that when the video plays or the timeupdate event is fired from javascript, the Unity Events subscribed to them won't be invoked.
		///if the second argument is true, in addition to unregistering the event, its runtime callbacks will be removed as well.
		///callbacks added from the inspector are not removed if the second argument is true.
		///if you want to unregister all events, pass (VideoPlayerWebGL.evnts)(-1) as input
		public void UnregisterEvent(evnts evt, bool removeAllNonPersistentListeners = false) {
			foreach (evnts x in Enum.GetValues(typeof(evnts))) {
				if (events.HasFlag(x)) {//if the event is registered
					if (evt.HasFlag(x)) {//and the user wants to unregister it
						((Dictionary<string, UnityEvent>)typeof(VideoPlayerWebGL).GetField(x.ToString() + "Dict",  BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Remove(id);
						if (removeAllNonPersistentListeners)
							((UnityEvent)typeof(VideoPlayerWebGL).GetField(x.ToString(), BindingFlags.Instance | BindingFlags.Public).GetValue(this)).RemoveAllListeners();
					}
				} else {//if the event is not registered
					evt &= ~x;//unset from input(if input doesn't have it, nothing changes. But if it does, unset)
				}
			}

			unregisterEvent(id, evt.ToString());//unregister the events on the javascript side
			events &= ~evt;//clear flag
		}
		
		//returns all the currently registered events
		public evnts GetCurrentEvents() {
			return events;
		}

		///all of the methods below are for the inspector only
		private bool HasCanplayFlag() {return events.HasFlag(evnts.canplay);}
		private bool HasCanplaythroughFlag() {return events.HasFlag(evnts.canplaythrough);}
		private bool HasCompleteFlag() {return events.HasFlag(evnts.complete);}
		private bool HasDurationchangeFlag() {return events.HasFlag(evnts.durationchange);}
		private bool HasEmptiedFlag() {return events.HasFlag(evnts.emptied);}
		private bool HasEndedFlag() {return events.HasFlag(evnts.ended);}
		private bool HasLoadeddataFlag() {return events.HasFlag(evnts.loadeddata);}
		private bool HasLoadedmetadataFlag() {return events.HasFlag(evnts.loadedmetadata);}
		private bool HasPauseFlag() {return events.HasFlag(evnts.pause);}
		private bool HasPlayFlag() {return events.HasFlag(evnts.play);}
		private bool HasPlayingFlag() {return events.HasFlag(evnts.playing);}
		private bool HasProgressFlag() {return events.HasFlag(evnts.progress);}
		private bool HasRatechangeFlag() {return events.HasFlag(evnts.ratechange);}
		private bool HasSeekedFlag() {return events.HasFlag(evnts.seeked);}
		private bool HasSeekingFlag() {return events.HasFlag(evnts.seeking);}
		private bool HasStalledFlag() {return events.HasFlag(evnts.stalled);}
		private bool HasSuspendFlag() {return events.HasFlag(evnts.suspend);}
		private bool HasTimeupdateFlag() {return events.HasFlag(evnts.timeupdate);}
		private bool HasVolumechangeFlag() {return events.HasFlag(evnts.volumechange);}
		private bool HasWaitingFlag() {return events.HasFlag(evnts.waiting);}
	}
}
