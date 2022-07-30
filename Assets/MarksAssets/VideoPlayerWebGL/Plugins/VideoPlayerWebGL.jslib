mergeInto(LibraryManager.library, {
	VideoPlayerWebGL_CreateVideo: function (url, cors, id, autoplay, loop, muted, volume, playbackSpeed, texturePtr, events, canUpdateTexture) {
		id = UTF8ToString(id);
		
		Module['VideoPlayerWebGL'].canUpdateTextureRef = Module['VideoPlayerWebGL'].canUpdateTextureRef || canUpdateTexture;
		Module['VideoPlayerWebGL'].canUpdateTextureArr = Module['VideoPlayerWebGL'].canUpdateTextureArr || new Uint32Array(buffer, canUpdateTexture, 1);

		//videos have the following custom property: VideoPlayerWebGL : {requestId, texturePtr, playingFlag, timeupdateFlag, createdVideoFlag, events: {}, createVideo:function, timeupdate:function, playing:function, pause:function}
        const video = document.getElementById(id) || document.querySelector("body").appendChild(document.createElement("video"));
        if (video.hasAttribute("src")) return;//if it has the src attribute set, the video was already created and set up. Don't do anything else, just return.
        url = UTF8ToString(url) + "#t=0.0000001";//this last part is a trick to get Safari to show the first frame of the video, even if it doesn't play automatically.
        cors = UTF8ToString(cors);
        events = UTF8ToString(events).replace(/\s/g, "").split(",");
        events = events[0] === "-1" ? ["canplay", "canplaythrough", "complete", "durationchange", "emptied", "ended", "loadeddata", "loadedmetadata", "pause", "play", "playing", "progress", "ratechange", "seeked", "seeking", "stalled", "suspend", "timeupdate", "volumechange", "waiting"] : events[0] === "0" ? undefined : events;
        video.VideoPlayerWebGL = {};
		video.VideoPlayerWebGL.playingFlag = false;
		video.VideoPlayerWebGL.timeupdateFlag = false;
		video.VideoPlayerWebGL.createdVideoFlag = false;
        video.VideoPlayerWebGL.events = {};
        events && events.forEach((function(event) {
            video.VideoPlayerWebGL.events[event] = (function() {
                Module["VideoPlayerWebGL"][event](id)
            });
            video.addEventListener(event, video.VideoPlayerWebGL.events[event])
        }));
        video.setAttribute("id", id);
        video.setAttribute("name", /[^/]*$/.exec(url)[0].replace("#t=0.0000001", ""));
        video.setAttribute("src", url);
        video.setAttribute("playsinline", "");
        video.defaultPlaybackRate = playbackSpeed;
        video.playbackRate = playbackSpeed;
        video.volume = volume;
        video.VideoPlayerWebGL.texturePtr = texturePtr;
        video.preload === "metadata" && (video.preload = "auto");//it needs to be done like this because of Safari. On mobile Safari, the video.preload value is already set to auto. Setting the value again to auto makes the video not play for some reason. So I only touch the preload attribute if its value is not already auto, which happens on Android Chrome. 
        muted && (video.muted = true);//the muted attribute doesn't do anything. The property that needs to be changed. They are different things.
        autoplay && video.setAttribute("autoplay", "");//it will autoplay on chrome even if not muted, as long as the user touches the screen anywhere before the video loads. The user can do this even before the Unity scenes loads. On Safari, however, this won't work. The user will need to click on a button using a pointerdown event on the Unity side, so that the play method is called on the js side on a pointerup.
        loop && video.setAttribute("loop", "");
        cors && video.setAttribute("crossorigin", cors);
        video.style.pointerEvents = "none";
		video.style.zIndex = "-1000";
		video.style.position = "fixed";
		video.style.top = "0";
        video.width = video.height = 1;//using style.display = "none" makes the autoplay fail on all browsers, even if it's muted. Changing the opacity to 0 or visibility to hidden still shows the video on the HTML as a white plane. Setting the width and height to 0 makes the autoplay fail on Safari. But setting it to 1 makes the autoplay work if it's muted on both browsers and solves the problem of showing the video on the html.
		
		video.VideoPlayerWebGL.createVideo = function() {
			if (!video.VideoPlayerWebGL.createdVideoFlag && video.VideoPlayerWebGL.playingFlag && video.VideoPlayerWebGL.timeupdateFlag) {
				video.addEventListener("timeupdate", function() {//after it's playing and time update ran once, still need to check once more to guarantee there is data. All of this set up is because of firefox.
					video.VideoPlayerWebGL.createdVideoFlag = video.VideoPlayerWebGL.canUpdateTexture = true;
					(function(number, bitPosition) {
						if (Module['VideoPlayerWebGL'].canUpdateTextureArr.byteLength === 0)//buffer resized, need to assign array again
							Module['VideoPlayerWebGL'].canUpdateTextureArr = new Uint32Array(buffer, Module['VideoPlayerWebGL'].canUpdateTextureRef, 1);
					
						Module['VideoPlayerWebGL'].canUpdateTextureArr[0] = number | (1 << bitPosition);
					})(Module['VideoPlayerWebGL'].canUpdateTextureArr[0], parseInt(id.split('_')[1]));
				}, {once: true});
			}
		}
	
		video.VideoPlayerWebGL.pause = (function() {
            video.VideoPlayerWebGL.canUpdateTexture = false;
			(function(number, bitPosition) {
				if (Module['VideoPlayerWebGL'].canUpdateTextureArr.byteLength === 0)//buffer resized, need to assign array again
					Module['VideoPlayerWebGL'].canUpdateTextureArr = new Uint32Array(buffer, Module['VideoPlayerWebGL'].canUpdateTextureRef, 1);
				
				const mask = ~(1 << bitPosition);
				Module['VideoPlayerWebGL'].canUpdateTextureArr[0] = number & mask;
			})(Module['VideoPlayerWebGL'].canUpdateTextureArr[0], parseInt(id.split('_')[1]));
        });
		
		video.VideoPlayerWebGL.playing = function() {
			if (video.VideoPlayerWebGL.createdVideoFlag) {
				video.VideoPlayerWebGL.canUpdateTexture = true;
				(function(number, bitPosition) {
					if (Module['VideoPlayerWebGL'].canUpdateTextureArr.byteLength === 0)//buffer resized, need to assign array again
						Module['VideoPlayerWebGL'].canUpdateTextureArr = new Uint32Array(buffer, Module['VideoPlayerWebGL'].canUpdateTextureRef, 1);
				
					Module['VideoPlayerWebGL'].canUpdateTextureArr[0] = number | (1 << bitPosition);
				})(Module['VideoPlayerWebGL'].canUpdateTextureArr[0], parseInt(id.split('_')[1]));
			} else {
				video.VideoPlayerWebGL.playingFlag = true;
				video.VideoPlayerWebGL.createVideo();
			}
		};
		
		video.VideoPlayerWebGL.timeupdate = function() {
			if (!video.VideoPlayerWebGL.createdVideoFlag) {
				video.VideoPlayerWebGL.timeupdateFlag = true;
				video.VideoPlayerWebGL.createVideo();
			}
		};
		
        video.addEventListener("playing", video.VideoPlayerWebGL.playing);
		video.addEventListener("timeupdate", video.VideoPlayerWebGL.timeupdate);
        video.addEventListener("pause", video.VideoPlayerWebGL.pause);
    },
	/*
		this method was made for Safari. It needs to be called on a pointerdown event on the Unity side. This is because to play a video that is not muted a user gesture is required. 
		But only the first time, so I made this method. The idea is to call this on a button to start the experience so the video is unlocked,
		and then later you can play the video calling the play method on the onClick method or any other method you want and not need to worry if it will work.
		I suggest calling this method with other methods about requesting permission, like using the gyroscope using my asset: https://assetstore.unity.com/packages/tools/camera/gyrocamwebgl-176394
		This method just plays the video and immediately pauses it, to just unlock it so you can call VideoPlayerWebGL_PlayVideo later whenever you want on any Unity method.
		If you want to press a button and play the video immediately, without having a button just to unlock the video playback, you can have a normal button in Unity
		calling VideoPlayerWebGL_PlayVideo on an onClick event, and on the same button add an onpointerdown event with VideoPlayerWebGL_unlockVideoPlayback.
		Alternatively, you can just always call VideoPlayerWebGL_PlayVideoPointerDown on a pointerdown event.
	*/
	VideoPlayerWebGL_unlockVideoPlayback: function(id) { 
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		
		document.documentElement.addEventListener('pointerup', function() {
			const curTime = video.currentTime;
            video.play();
			video.pause();
			video.currentTime = curTime;//just in case this function is called more than once by accident, it won't do anything.
		}, { once: true });
		
	},
	VideoPlayerWebGL_UpdateTexture: function(id) {
		id = UTF8ToString(id);
		const video = document.getElementById(id);
		if (!video) return false;
		if (!video.VideoPlayerWebGL.canUpdateTexture) return false;
        if (!(video.videoWidth > 0 && video.videoHeight > 0)) return false;
        if (video.lastUpdateTextureTime === video.currentTime) return false;
        video.lastUpdateTextureTime = video.currentTime;
	
		if (video.previousUploadedWidth != video.videoWidth || video.previousUploadedHeight != video.videoHeight) {
            GLctx.deleteTexture(GL.textures[video.VideoPlayerWebGL.texturePtr]);
			GL.textures[video.VideoPlayerWebGL.texturePtr] = GLctx.createTexture();
			GL.textures[video.VideoPlayerWebGL.texturePtr].name = video.VideoPlayerWebGL.texturePtr;
			var prevTex = GLctx.getParameter(GLctx.TEXTURE_BINDING_2D);
			GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[video.VideoPlayerWebGL.texturePtr]);
			GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
			GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
			GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
			GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
			GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, video);
			GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
			GLctx.bindTexture(GLctx.TEXTURE_2D, prevTex);
            video.previousUploadedWidth = video.videoWidth;
            video.previousUploadedHeight = video.videoHeight
        } else {
            GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
			var prevTex = GLctx.getParameter(GLctx.TEXTURE_BINDING_2D);
			GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[video.VideoPlayerWebGL.texturePtr]);
			GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, video);
			GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
			GLctx.bindTexture(GLctx.TEXTURE_2D, prevTex);
        }
	},
	VideoPlayerWebGL_PlayVideoPointerDown: function(id) { 
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		
		document.documentElement.addEventListener('pointerup', function() {
            video.play();
		}, { once: true });
		
	},
	VideoPlayerWebGL_PlayVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		
		video.play();
		
	},
	VideoPlayerWebGL_PauseVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		video.pause();
	},
	VideoPlayerWebGL_StopVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		video.pause();
		video.currentTime = 0;
	},
	VideoPlayerWebGL_RestartVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		video.currentTime = 0;
		video.play();
	},
	VideoPlayerWebGL_GetTimeVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		return !video ? -1.0 : video.currentTime;
	},
	VideoPlayerWebGL_SetTimeVideo: function(id, time) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		
		video.currentTime = time;
	},
	VideoPlayerWebGL_LengthVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));	
		return !video ? -1.0 : video.duration;
	},
	VideoPlayerWebGL_SetLoopVideo: function(id, loop) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		if (loop && !video.hasAttribute("loop"))
			video.setAttribute("loop", "");
		else if (!loop && video.hasAttribute("loop"))
			video.removeAttribute("loop");
	},
	VideoPlayerWebGL_SetAutoplayVideo: function(id, autoplay) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		if (autoplay && !video.hasAttribute("autoplay"))
			video.setAttribute("autoplay", "");
		else if (!autoplay && video.hasAttribute("autoplay"))
			video.removeAttribute("autoplay");
	},
	VideoPlayerWebGL_SetMuteVideo: function(id, mute) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		video.muted = mute;
	},
	VideoPlayerWebGL_PlayBackSpeedVideo: function(id, speed) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		video.defaultPlaybackRate = speed;
		video.playbackRate = speed;
	},
	/*
		You may get the following error if you're trying to load a video from an external URL and not the local StreamingAssets folder:
		Access to video at 'urlVideo' from origin 'urlOrigin' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.
	*/
	VideoPlayerWebGL_SourceVideo: function(id, url, cors) {
		const video = document.getElementById(UTF8ToString(id));
        if (!video) return;
        url = UTF8ToString(url);
        cors = UTF8ToString(cors);
        video.setAttribute("src", url);
        video.setAttribute("name", /[^/]*$/.exec(url)[0].replace("#t=0.0000001", ""));
		video.VideoPlayerWebGL.createdVideoFlag = video.VideoPlayerWebGL.playingFlag = video.VideoPlayerWebGL.timeupdateFlag = false;
        cors ? video.setAttribute("crossorigin", cors) : video.removeAttribute("crossorigin");
		video.VideoPlayerWebGL.playedFirstTimeAfterLoading = false;
        video.load();
	},
	VideoPlayerWebGL_IsPlayingVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		return !video ? false : !!(video.currentTime > 0 && !video.paused && !video.ended && video.readyState > 2);
	},
	VideoPlayerWebGL_IsPausedVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		return !video ? false : video.paused;
	},
	VideoPlayerWebGL_WidthVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		return !video ? 0 : video.videoWidth;
	},
	VideoPlayerWebGL_HeightVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		return !video ? 0 : video.videoHeight;
	},
	VideoPlayerWebGL_SetVolumeVideo: function(id, volume) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		video.volume = volume;
	},
	VideoPlayerWebGL_SetCorsVideo: function(id, cors) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		cors = UTF8ToString(cors);
		cors ? video.setAttribute("crossorigin", cors) : video.removeAttribute("crossorigin");
	},
	VideoPlayerWebGL_IsReadyVideo: function(id) {
		const video = document.getElementById(UTF8ToString(id));
		if (!video)
			return;
		return video.readyState >= 4;
	},
	VideoPlayerWebGL_DestroyVideo: function(id) {
		id = UTF8ToString(id);
		var video = document.getElementById(id);
        if (!video) return;
        video.VideoPlayerWebGL.canUpdateTexture = false;
		(function(number, bitPosition) {
			if (Module['VideoPlayerWebGL'].canUpdateTextureArr.byteLength === 0)//buffer resized, need to assign array again
				Module['VideoPlayerWebGL'].canUpdateTextureArr = new Uint32Array(buffer, Module['VideoPlayerWebGL'].canUpdateTextureRef, 1);
			
			const mask = ~(1 << bitPosition);
			Module['VideoPlayerWebGL'].canUpdateTextureArr[0] = number & mask;
		})(Module['VideoPlayerWebGL'].canUpdateTextureArr[0], parseInt(id.split('_')[1]));
        GLctx.bindTexture(GLctx.TEXTURE_2D, null);
        video.pause();
        video.removeAttribute("src");
        video.load();
		//just to be safe...remove event listeners manually. Although it is said that modern browsers take care of this.
        Object.keys(video.VideoPlayerWebGL.events).forEach((function(event) {video.removeEventListener(event, video.VideoPlayerWebGL.events[event])}));
        video.removeEventListener("playing", video.VideoPlayerWebGL.playing);
		video.removeEventListener("timeupdate", video.VideoPlayerWebGL.timeupdate);
        video.removeEventListener("pause", video.VideoPlayerWebGL.pause);
        video.remove();
        video = null
	},
	VideoPlayerWebGL_RegisterEvent: function(id, events) {
		id = UTF8ToString(id);
		const video = document.getElementById(id);
        if (!video) return;
		events = UTF8ToString(events).replace(/\s/g, "").split(",");//can be multiple events
		events = events[0] === "-1" ? ["canplay", "canplaythrough", "complete", "durationchange", "emptied", "ended", "loadeddata", "loadedmetadata", "pause", "play", "playing", "progress", "ratechange", "seeked", "seeking", "stalled", "suspend", "timeupdate", "volumechange", "waiting"] : events[0] === "0" ? undefined : events;
		events && events.forEach(function(event) {
			video.removeEventListener(event, video.VideoPlayerWebGL.events[event]);//if a previous event was already being listened to, remove it.
			video.VideoPlayerWebGL.events[event] = function() {Module['VideoPlayerWebGL'][event](id);}//store new event
			video.addEventListener(event, video.VideoPlayerWebGL.events[event]);//and listen to it.
		});
	},
	VideoPlayerWebGL_UnregisterEvent: function(id, events) {
		const video = document.getElementById(UTF8ToString(id));
        if (!video) return;
		events = UTF8ToString(events).replace(/\s/g, "").split(",");//can be multiple events
		events = events[0] === "-1" ? ["canplay", "canplaythrough", "complete", "durationchange", "emptied", "ended", "loadeddata", "loadedmetadata", "pause", "play", "playing", "progress", "ratechange", "seeked", "seeking", "stalled", "suspend", "timeupdate", "volumechange", "waiting"] : events[0] === "0" ? undefined : events;
		events && events.forEach(function(event) {
			video.removeEventListener(event, video.VideoPlayerWebGL.events[event]);//if a previous event was already being listened to, remove it.
			delete video.VideoPlayerWebGL.events[event];
		});
	},
});