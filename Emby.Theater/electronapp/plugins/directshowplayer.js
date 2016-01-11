define([], function () {

    return function () {

        var self = this;

        self.name = 'Windows Player';
        self.type = 'mediaplayer';
        self.packageName = 'directshowplayer';

        var currentSrc;
        var playerState = {};
        var ignoreEnded;

        self.getRoutes = function () {

            var routes = [];

            routes.push({
                path: Emby.PluginManager.mapPath(self, 'directshowplayer/settings.html'),
                id: 'directshowplayer-settings',
                transition: 'slide',
                dependencies: [
                    Emby.PluginManager.mapPath(self, 'directshowplayer/settings.js'),
                    'emby-dropdown-menu'
                ],
                type: 'settings',
                title: 'Windows Player',
                category: 'Playback',
                thumbImage: ''
            });

            return routes;
        };

        self.getTranslations = function () {

            var files = [];

            files.push({
                lang: 'en-us',
                path: Emby.PluginManager.mapPath(self, 'directshowplayer/strings/en-us.json')
            });

            return files;
        };

        self.canPlayMediaType = function (mediaType) {

            return (mediaType || '').toLowerCase() == 'audio' || (mediaType || '').toLowerCase() == 'video';
        };

        self.getDeviceProfile = function () {

            return new Promise(function (resolve, reject) {

                var profile = {};

                profile.MaxStreamingBitrate = 100000000;
                profile.MaxStaticBitrate = 100000000;
                profile.MusicStreamingTranscodingBitrate = 192000;

                profile.DirectPlayProfiles = [];

                profile.DirectPlayProfiles.push({
                    Container: 'm4v,3gp,ts,mpegts,mov,xvid,vob,mkv,wmv,asf,ogm,ogv,m2v,avi,mpg,mpeg,mp4,webm',
                    Type: 'Video'
                });

                profile.DirectPlayProfiles.push({
                    Container: 'aac,mp3,mpa,wav,wma,mp2,ogg,oga,webma,ape,opus',
                    Type: 'Audio'
                });

                profile.TranscodingProfiles = [];

                profile.TranscodingProfiles.push({
                    Container: 'mkv',
                    Type: 'Video',
                    AudioCodec: 'aac,mp3,ac3',
                    VideoCodec: 'h264',
                    Context: 'Streaming'
                });

                profile.TranscodingProfiles.push({
                    Container: 'mp3',
                    Type: 'Audio',
                    AudioCodec: 'mp3',
                    Context: 'Streaming',
                    Protocol: 'http'
                });

                profile.ContainerProfiles = [];

                profile.CodecProfiles = [];

                // Subtitle profiles
                // External vtt or burn in
                profile.SubtitleProfiles = [];
                profile.SubtitleProfiles.push({
                    Format: 'srt',
                    Method: 'External'
                });
                profile.SubtitleProfiles.push({
                    Format: 'srt',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'subrip',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'ass',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'ssa',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'pgs',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'pgssub',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'dvdsub',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'vtt',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'sub',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'idx',
                    Method: 'Embed'
                });
                profile.SubtitleProfiles.push({
                    Format: 'smi',
                    Method: 'Embed'
                });

                profile.ResponseProfiles = [];

                resolve(profile);
            });
        };

        self.currentSrc = function () {
            return currentSrc;
        };

        self.play = function (options) {

            return new Promise(function (resolve, reject) {

                ignoreEnded = false;
                currentSrc = options.url;

                //var isVideo = options.mimeType.toLowerCase('video').indexOf() == 0;
                var isVideo = options.item.MediaType == 'Video';

                var requestBody = {
                    url: options.url,
                    isVideo: isVideo,
                    item: options.item,
                    mediaSource: options.mediaSource,
                    startPositionTicks: options.playerStartPositionTicks
                };

                sendCommand('play', requestBody).then(function () {

                    Events.trigger(self, 'started');

                    if (isVideo) {
                        if (options.fullscreen !== false) {

                            Emby.Page.showVideoOsd();

                        } else {
                            Emby.Page.setTransparency(Emby.TransparencyLevel.Backdrop);
                        }
                    }

                    startTimeUpdateInterval();

                    resolve();

                }, function () {
                    stopTimeUpdateInterval();
                    reject();
                });
            });
        };

        // Save this for when playback stops, because querying the time at that point might return 0
        self.currentTime = function (val) {

            if (val != null) {
                sendCommand('positionticks?val=' + (val * 10000)).then(onTimeUpdate);
                return;
            }

            return (playerState.positionTicks || 0) / 10000;
        };

        self.duration = function (val) {

            return playerState.durationTicks;
        };

        self.stop = function (destroyPlayer, reportEnded) {

            ignoreEnded = true;

            var cmd = destroyPlayer ? 'stopfade' : 'stop';
            return sendCommand(cmd).then(function () {

                onEnded(reportEnded);

                if (destroyPlayer) {
                    self.destroy();
                }
            });
        };

        self.destroy = function () {
            Emby.Page.setTransparency(Emby.TransparencyLevel.None);
        };

        self.pause = function () {
            sendCommand('pause').then(onPause);
        };

        self.unpause = function () {
            sendCommand('unpause').then(onPlaying);
        };

        self.paused = function () {

            return playerState.isPaused || false;
        };

        self.volume = function (val) {
            if (val != null) {
                sendCommand('volume?val=' + val).then(onVolumeChange);
                return;
            }

            return playerState.volume || 0;
        };

        self.setSubtitleStreamIndex = function (index) {
            sendCommand('setSubtitleStreamIndex?index=' + index);
        };

        self.setAudioStreamIndex = function (index) {
            sendCommand('setAudioStreamIndex?index=' + index);
        };

        self.canSetAudioStreamIndex = function () {
            return true;
        };

        self.setMute = function (mute) {

            var cmd = mute ? 'mute' : 'unmute';

            sendCommand(cmd).then(onVolumeChange);
        };

        self.isMuted = function () {
            return playerState.isMuted || false;
        };

        var timeUpdateInterval;
        function startTimeUpdateInterval() {
            stopTimeUpdateInterval();
            timeUpdateInterval = setInterval(onTimeUpdate, 500);
        }

        function stopTimeUpdateInterval() {
            if (timeUpdateInterval) {
                clearInterval(timeUpdateInterval);
                timeUpdateInterval = null;
            }
        }

        function onEnded(reportEnded) {
            stopTimeUpdateInterval();

            if (reportEnded) {
                Events.trigger(self, 'stopped');
            }
        }

        function onTimeUpdate() {

            updatePlayerState();
            Events.trigger(self, 'timeupdate');
        }

        function onVolumeChange() {
            Events.trigger(self, 'volumechange');
        }

        function onPlaying() {

            Events.trigger(self, 'playing');
        }

        function onPause() {
            Events.trigger(self, 'pause');
        }

        function onError() {

            stopTimeUpdateInterval();
            Events.trigger(self, 'error');
        }

        function getFetchPromise(request) {

            var headers = request.headers || {};

            if (request.dataType == 'json') {
                headers.accept = 'application/json';
            }

            var fetchRequest = {
                headers: headers,
                method: request.type
            };

            var contentType = request.contentType;

            if (request.data) {

                if (typeof request.data === 'string') {
                    fetchRequest.body = request.data;
                } else {
                    fetchRequest.body = paramsToString(request.data);

                    contentType = contentType || 'application/x-www-form-urlencoded; charset=UTF-8';
                }
            }

            if (contentType) {

                headers['Content-Type'] = contentType;
            }

            return fetch(request.url, fetchRequest);
        }

        function paramsToString(params) {

            var values = [];

            for (var key in params) {

                var value = params[key];

                if (value !== null && value !== undefined && value !== '') {
                    values.push(encodeURIComponent(key) + "=" + encodeURIComponent(value));
                }
            }
            return values.join('&');
        }

        function sendCommand(name, body) {

            var request = {
                type: 'POST',
                url: 'http://localhost:8154/directshowplayer/' + name,
                dataType: 'json'
            };

            if (body) {
                request.contentType = 'application/json';
                request.data = JSON.stringify(body);
            }

            return getFetchPromise(request).then(function (response) {

                return response.json();

            }).then(function (state) {

                var previousPlayState = playerState.playstate;

                playerState = state;

                if (state.playstate == 'idle' && previousPlayState != 'idle' && previousPlayState) {
                    if (!ignoreEnded) {
                        ignoreEnded = true;
                        onEnded(true);
                    }
                }

                return state;
            });
        }

        function updatePlayerState() {

            return sendCommand('refresh');
        }
    }
});