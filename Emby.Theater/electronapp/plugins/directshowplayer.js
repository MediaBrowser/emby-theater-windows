define(['apphost', 'pluginManager', 'events'], function (appHost, pluginManager, events) {

    return function () {

        var self = this;

        self.name = 'Windows Player';
        self.type = 'mediaplayer';
        self.id = 'directshowplayer';
        self.requiresVideoTransparency = true;

        var currentSrc;
        var playerState = {};
        var ignoreEnded;

        self.getRoutes = function () {

            var routes = [];

            routes.push({
                path: 'directshowplayer/audio.html',
                transition: 'slide',
                dependencies: [
                    'emby-select'
                ],
                controller: pluginManager.mapPath(self, 'directshowplayer/audio.js'),
                type: 'settings',
                title: 'Audio',
                category: 'Playback',
                thumbImage: ''
            });

            if (appHost.supports('windowtransparency')) {
                routes.push({
                    path: 'directshowplayer/video.html',
                    transition: 'slide',
                    dependencies: [
                        'emby-select',
                        'emby-button',
                        'emby-checkbox'
                    ],
                    controller: pluginManager.mapPath(self, 'directshowplayer/video.js'),
                    type: 'settings',
                    title: 'Video',
                    category: 'Playback',
                    thumbImage: ''
                });

                routes.push({
                    path: 'directshowplayer/madvr.html',
                    transition: 'slide',
                    dependencies: [
                    'emby-select'
                    ],
                    controller: pluginManager.mapPath(self, 'directshowplayer/madvr.js'),
                    thumbImage: ''
                });
            }

            return routes;
        };

        self.getTranslations = function () {

            var files = [];

            files.push({
                lang: 'en-us',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/en-US.json')
            });

            files.push({
                lang: 'fr',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/fr.json')
            });

            files.push({
                lang: 'hr',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/hr.json')
            });

            files.push({
                lang: 'pl',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/pl.json')
            });

            files.push({
                lang: 'pt-PT',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/pt-PT.json')
            });

            files.push({
                lang: 'ru',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/ru.json')
            });

            files.push({
                lang: 'sv',
                path: pluginManager.mapPath(self, 'directshowplayer/strings/sv.json')
            });

            return files;
        };

        self.canPlayMediaType = function (mediaType) {

            if ((mediaType || '').toLowerCase() == 'video') {

                return appHost.supports('windowtransparency');
            }
            return (mediaType || '').toLowerCase() == 'audio';
        };

        self.canPlayItem = function (item) {

            return item.Type != 'TvChannel' || (item.ServiceName || '').toLowerCase().indexOf('next') == -1;
        };

        self.getDeviceProfile = function () {

            var profile = {};

            profile.MaxStreamingBitrate = 100000000;
            profile.MaxStaticBitrate = 100000000;
            profile.MusicStreamingTranscodingBitrate = 192000;

            profile.DirectPlayProfiles = [];

            profile.DirectPlayProfiles.push({
                Container: 'm4v,3gp,ts,mpegts,mov,xvid,vob,mkv,wmv,asf,ogm,ogv,m2v,avi,mpg,mpeg,mp4,webm,wtv,dvr-ms,iso',
                Type: 'Video'
            });

            profile.DirectPlayProfiles.push({
                Container: 'aac,mp3,mpa,wav,wma,mp2,ogg,oga,webma,ape,opus,flac',
                Type: 'Audio'
            });

            profile.TranscodingProfiles = [];

            profile.TranscodingProfiles.push({
                Container: 'mkv',
                Type: 'Video',
                AudioCodec: 'mp3,ac3,aac',
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
                Format: 'dvbsub',
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

            return Promise.resolve(profile);
        };

        self.currentSrc = function () {
            return currentSrc;
        };

        self.play = function (options) {

            var mediaSource = JSON.parse(JSON.stringify(options.mediaSource));

            var url = options.url;

            ignoreEnded = false;
            currentSrc = url;

            //var isVideo = options.mimeType.toLowerCase('video').indexOf() == 0;
            var isVideo = options.item.MediaType == 'Video';

            var enableFullscreen = options.fullscreen !== false;

            // Update the text url in the media source with the full url from the options object
            mediaSource.MediaStreams.forEach(function (ms) {
                var textTrack = options.tracks.filter(function (t) {
                    return t.index == ms.Index;

                })[0];

                if (textTrack) {
                    ms.DeliveryUrl = textTrack.url;
                }
            });

            var requestBody = {
                url: url,
                isVideo: isVideo,
                item: options.item,
                mediaSource: mediaSource,
                startPositionTicks: options.playerStartPositionTicks,
                fullscreen: enableFullscreen
            };

            return sendCommand('play', requestBody).then(function () {

                if (isVideo) {
                    if (enableFullscreen) {

                        Emby.Page.showVideoOsd();

                    } else {
                        Emby.Page.setTransparency(Emby.TransparencyLevel.Backdrop);
                    }
                }

                startTimeUpdateInterval();

                return Promise.resolve();

            }, function (err) {
                stopTimeUpdateInterval();
                throw err;
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

            return playerState.durationTicks / 10000;
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
                events.trigger(self, 'stopped');
            }
        }

        function onTimeUpdate() {

            updatePlayerState();
            events.trigger(self, 'timeupdate');
        }

        function onVolumeChange() {
            events.trigger(self, 'volumechange');
        }

        function onPlaying() {

            events.trigger(self, 'playing');
        }

        function onPause() {
            events.trigger(self, 'pause');
        }

        function onError() {

            stopTimeUpdateInterval();
            events.trigger(self, 'error');
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

                var previousPlayerState = playerState;

                if (state.playstate == 'idle' && previousPlayerState.playstate != 'idle' && previousPlayerState.playstate) {
                    if (!ignoreEnded) {
                        ignoreEnded = true;
                        onEnded(true);
                    }
                    return playerState;
                }

                playerState = state;

                if (previousPlayerState.isMuted != state.isMuted ||
                    previousPlayerState.volume != state.volume) {
                    onVolumeChange();
                }

                return state;
            });
        }

        function updatePlayerState() {

            return sendCommand('refresh');
        }
    }
});
