define(['apphost', 'pluginManager', 'events', 'embyRouter', 'appSettings', 'require', 'loading', 'dom'], function (appHost, pluginManager, events, embyRouter, appSettings, require, loading, dom) {
    'use strict';

    return function () {

        var self = this;

        self.name = 'Windows Player';
        self.type = 'mediaplayer';
        self.id = 'directshowplayer';
        self.priority = -2;

        var currentSrc;
        var playerState = {};
        var ignoreEnded;
        var videoDialog;

        self.getRoutes = function () {

            var routes = [];

            if (appHost.supports('windowtransparency')) {

                routes.push({
                    path: 'directshowplayer/video.html',
                    transition: 'slide',
                    dependencies: [
                        'emby-select'
                    ],
                    controller: pluginManager.mapPath(self, 'directshowplayer/video.js'),
                    thumbImage: '',
                    type: 'settings',
                    title: 'MadVR Video Player',
                    category: 'Playback',
                    order: 1000
                });
            }

            return routes;
        };

        self.getTranslations = function () {

            var files = [];

            return files;
        };

        self.canPlayMediaType = function (mediaType) {

            if ((mediaType || '').toLowerCase() == 'video') {

                return appHost.supports('windowtransparency');
            }
            return false;
        };

        self.canPlayItem = function (item, playOptions) {

            if (!item.RunTimeTicks) {
                return false;
            }
            if (!playOptions.fullscreen) {
                return false;
            }
            if (item.VideoType !== 'VideoFile') {
                return false;
            }
            if (item.LocationType !== 'FileSystem') {
                return false;
            }

            return appSettings.get('dsplayer-enabled') === 'true';
        };

        self.getDeviceProfile = function () {

            var profile = {};

            profile.MaxStreamingBitrate = 100000000;
            profile.MaxStaticBitrate = 100000000;
            profile.MusicStreamingTranscodingBitrate = 192000;

            profile.DirectPlayProfiles = [];

            profile.DirectPlayProfiles.push({
                Container: 'm4v,3gp,ts,mpegts,mov,xvid,vob,mkv,wmv,asf,ogm,ogv,m2v,avi,mpg,mpeg,mp4,webm,wtv,iso,m2ts',
                Type: 'Video'
            });

            profile.DirectPlayProfiles.push({
                Container: 'aac,mp3,mpa,wav,wma,mp2,ogg,oga,webma,ape,opus,flac,m4a',
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

                Container: 'ts',
                Type: 'Audio',
                AudioCodec: 'aac',
                Context: 'Streaming',
                Protocol: 'hls',
                SegmentLength: '3'
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
                Format: 'ass',
                Method: 'External'
            });
            profile.SubtitleProfiles.push({
                Format: 'ssa',
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

        function getPlayerWindowHandle() {
            return window.PlayerWindowId;
        }

        self.play = function (options) {

            return createMediaElement(options).then(function () {
                return playInternal(options);
            });
        };

        function playInternal(options) {

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
                fullscreen: enableFullscreen,
                windowHandle: getPlayerWindowHandle(),
                playerOptions: {
                    dynamicRangeCompression: appSettings.get('mpv-drc'),
                    audioChannels: appSettings.get('mpv-speakerlayout'),
                    audioSpdif: appSettings.get('mpv-audiospdif'),
                    videoOutputLevels: appSettings.get('mpv-outputlevels'),
                    deinterlace: appSettings.get('mpv-deinterlace'),
                    hwdec: appSettings.get('mpv-hwdec')
                }
            };

            return sendCommand('play', requestBody).then(function () {

                if (isVideo) {
                    if (enableFullscreen) {

                        embyRouter.showVideoOsd().then(onNavigatedToOsd);

                    } else {
                        embyRouter.setTransparency('backdrop');

                        if (videoDialog) {
                            videoDialog.classList.remove('mpv-videoPlayerContainer-withBackdrop');
                            videoDialog.classList.remove('mpv-videoPlayerContainer-onTop');
                        }
                    }
                }

                startTimeUpdateInterval();

                return Promise.resolve();

            }, function (err) {
                stopTimeUpdateInterval();
                throw err;
            });
        }

        function onNavigatedToOsd() {

            if (videoDialog) {
                videoDialog.classList.remove('mpv-videoPlayerContainer-withBackdrop');
                videoDialog.classList.remove('mpv-videoPlayerContainer-onTop');
            }
        }

        function createMediaElement(options) {

            if (options.mediaType !== 'Video') {
                return Promise.resolve();
            }

            return new Promise(function (resolve, reject) {

                var dlg = document.querySelector('.mpv-videoPlayerContainer');

                if (!dlg) {

                    require(['css!./mpvplayer'], function () {

                        loading.show();

                        var dlg = document.createElement('div');

                        dlg.classList.add('mpv-videoPlayerContainer');

                        if (options.backdropUrl) {

                            dlg.classList.add('mpv-videoPlayerContainer-withBackdrop');
                            dlg.style.backgroundImage = "url('" + options.backdropUrl + "')";
                        }

                        if (options.fullscreen) {
                            dlg.classList.add('mpv-videoPlayerContainer-onTop');
                        }

                        document.body.insertBefore(dlg, document.body.firstChild);
                        videoDialog = dlg;

                        if (options.fullscreen) {
                            zoomIn(dlg).then(resolve);
                        } else {
                            resolve();
                        }

                    });

                } else {

                    if (options.backdropUrl) {

                        dlg.classList.add('mpv-videoPlayerContainer-withBackdrop');
                        dlg.style.backgroundImage = "url('" + options.backdropUrl + "')";
                    }

                    resolve();
                }
            });
        }

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

        self.stop = function (destroyPlayer) {

            ignoreEnded = true;

            var cmd = destroyPlayer ? 'stopfade' : 'stop';
            return sendCommand(cmd).then(function () {

                onEnded(true);

                if (destroyPlayer) {
                    self.destroy();
                }
            });
        };

        self.destroy = function () {
            embyRouter.setTransparency('none');

            var dlg = videoDialog;
            if (dlg) {

                videoDialog = null;

                dlg.parentNode.removeChild(dlg);
            }
        };

        self.playPause = function () {
            sendCommand('playpause').then(function (state) {

                if (state.isPaused) {
                    onPause();
                } else {
                    onUnpause();
                }
            });
        };

        self.pause = function () {
            sendCommand('pause').then(onPause);
        };

        self.unpause = function () {
            sendCommand('unpause').then(onUnpause);
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

        function onUnpause() {

            events.trigger(self, 'unpause');
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

        function zoomIn(elem) {

            return new Promise(function (resolve, reject) {

                var duration = 240;
                elem.style.animation = 'mpvvideoplayer-zoomin ' + duration + 'ms ease-in normal';
                dom.addEventListener(elem, dom.whichAnimationEvent(), resolve, {
                    once: true
                });
            });
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
