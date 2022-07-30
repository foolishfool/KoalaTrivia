Module['VideoPlayerWebGL'] = Module['VideoPlayerWebGL'] || {};

Module['VideoPlayerWebGL'].canplay = function(id) {
	this.canplayInternal = this.canplayInternal || Module.cwrap("VideoPlayerWebGL_canplayClbks", null, ["string"]);
	this.canplayInternal(id);
};

Module['VideoPlayerWebGL'].canplaythrough = function(id) {
	this.canplaythroughInternal = this.canplaythroughInternal || Module.cwrap("VideoPlayerWebGL_canplaythroughClbks", null, ["string"]);
	this.canplaythroughInternal(id);
};

Module['VideoPlayerWebGL'].complete = function(id) {
	this.completeInternal = this.completeInternal || Module.cwrap("VideoPlayerWebGL_completeClbks", null, ["string"]);
	this.completeInternal(id);
};

Module['VideoPlayerWebGL'].durationchange = function(id) {
	this.durationchangeInternal = this.durationchangeInternal || Module.cwrap("VideoPlayerWebGL_durationchangeClbks", null, ["string"]);
	this.durationchangeInternal(id);
};

Module['VideoPlayerWebGL'].emptied = function(id) {
	this.emptiedInternal = this.emptiedInternal || Module.cwrap("VideoPlayerWebGL_emptiedClbks", null, ["string"]);
	this.emptiedInternal(id);
};

Module['VideoPlayerWebGL'].ended = function(id) {
	this.endedInternal = this.endedInternal || Module.cwrap("VideoPlayerWebGL_endedClbks", null, ["string"]);
	this.endedInternal(id);
};

Module['VideoPlayerWebGL'].loadeddata = function(id) {
	this.loadeddataInternal = this.loadeddataInternal || Module.cwrap("VideoPlayerWebGL_loadeddataClbks", null, ["string"]);
	this.loadeddataInternal(id);
};

Module['VideoPlayerWebGL'].loadedmetadata = function(id) {
	this.loadedmetadataInternal = this.loadedmetadataInternal || Module.cwrap("VideoPlayerWebGL_loadedmetadataClbks", null, ["string"]);
	this.loadedmetadataInternal(id);
};

Module['VideoPlayerWebGL'].pause = function(id) {
	this.pauseInternal = this.pauseInternal || Module.cwrap("VideoPlayerWebGL_pauseClbks", null, ["string"]);
	this.pauseInternal(id);
};

Module['VideoPlayerWebGL'].play = function(id) {
	this.playInternal = this.playInternal || Module.cwrap("VideoPlayerWebGL_playClbks", null, ["string"]);
	this.playInternal(id);
};

Module['VideoPlayerWebGL'].playing = function(id) {
	this.playingInternal = this.playingInternal || Module.cwrap("VideoPlayerWebGL_playingClbks", null, ["string"]);
	this.playingInternal(id);
};

Module['VideoPlayerWebGL'].progress = function(id) {
	this.progressInternal = this.progressInternal || Module.cwrap("VideoPlayerWebGL_progressClbks", null, ["string"]);
	this.progressInternal(id);
};

Module['VideoPlayerWebGL'].ratechange = function(id) {
	this.ratechangeInternal = this.ratechangeInternal || Module.cwrap("VideoPlayerWebGL_ratechangeClbks", null, ["string"]);
	this.ratechangeInternal(id);
};

Module['VideoPlayerWebGL'].seeked = function(id) {
	this.seekedInternal = this.seekedInternal || Module.cwrap("VideoPlayerWebGL_seekedClbks", null, ["string"]);
	this.seekedInternal(id);
};

Module['VideoPlayerWebGL'].seeking = function(id) {
	this.seekingInternal = this.seekingInternal || Module.cwrap("VideoPlayerWebGL_seekingClbks", null, ["string"]);
	this.seekingInternal(id);
};

Module['VideoPlayerWebGL'].stalled = function(id) {
	this.stalledInternal = this.stalledInternal || Module.cwrap("VideoPlayerWebGL_stalledClbks", null, ["string"]);
	this.stalledInternal(id);
};

Module['VideoPlayerWebGL'].suspend = function(id) {
	this.suspendInternal = this.suspendInternal || Module.cwrap("VideoPlayerWebGL_suspendClbks", null, ["string"]);
	this.suspendInternal(id);
};

Module['VideoPlayerWebGL'].timeupdate = function(id) {
	this.timeupdateInternal = this.timeupdateInternal || Module.cwrap("VideoPlayerWebGL_timeupdateClbks", null, ["string"]);
	this.timeupdateInternal(id);
};

Module['VideoPlayerWebGL'].volumechange = function(id) {
	this.volumechangeInternal = this.volumechangeInternal || Module.cwrap("VideoPlayerWebGL_volumechangeClbks", null, ["string"]);
	this.volumechangeInternal(id);
};

Module['VideoPlayerWebGL'].waiting = function(id) {
	this.waitingInternal = this.waitingInternal || Module.cwrap("VideoPlayerWebGL_waitingClbks", null, ["string"]);
	this.waitingInternal(id);
};