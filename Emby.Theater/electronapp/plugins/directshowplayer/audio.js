define(['loading'], function (loading) {

    return function (view, params) {

        var self = this;

        view.addEventListener('viewbeforeshow', function (e) {

            var isRestored = e.detail.isRestored;

            Emby.Page.setTitle('Windows Player');

            loading.hide();

            if (!isRestored) {
                renderSettings();
                view.querySelector('.btnReset').addEventListener('click', onResetClick);
            }
        });

        view.addEventListener('viewbeforehide', function (e) {

            saveSettings();
        });

        function saveSettings() {

            getConfiguration().then(function (config) {

                var selectAudioBitstreamingMode = view.querySelector('.selectAudioBitstreamingMode');
                config.AudioConfig.AudioBitstreaming = selectAudioBitstreamingMode.value;

                var selectAudioRenderer = view.querySelector('.selectAudioRenderer');
                config.AudioConfig.Renderer = selectAudioRenderer.value;

                var selectSpeakerLayout = view.querySelector('.selectSpeakerLayout');
                config.AudioConfig.SpeakerLayout = selectSpeakerLayout.value;

                var selectDrc = view.querySelector('.selectDrc');
                config.AudioConfig.EnableDRC = selectDrc.value != '';
                config.AudioConfig.DRCLevel = selectDrc.value || '100';

                config.AudioConfig.ExpandMono = view.querySelector('.selectExpandMono').value == 'true';
                config.AudioConfig.Expand61 = view.querySelector('.selectExpandSixToSeven').value == 'true';

                var selectAudioEndPoint = view.querySelector('.selectAudioEndPoint');
                config.AudioConfig.AudioDevice = selectAudioEndPoint.value;

                saveConfiguration(config);
            });

        }

        function renderSettings() {
            getAudioDevices().then(function (audioDevices) {
                getConfiguration().then(function (config) {

                    var selectAudioBitstreamingMode = view.querySelector('.selectAudioBitstreamingMode');
                    selectAudioBitstreamingMode.value = config.AudioConfig.AudioBitstreaming;

                    var selectAudioRenderer = view.querySelector('.selectAudioRenderer');
                    selectAudioRenderer.value = config.AudioConfig.Renderer;

                    view.querySelector('.selectSpeakerLayout').value = config.AudioConfig.SpeakerLayout;

                    view.querySelector('.selectDrc').value = config.AudioConfig.EnableDRC ? config.AudioConfig.DRCLevel : '';

                    view.querySelector('.selectExpandMono').value = config.AudioConfig.ExpandMono;
                    view.querySelector('.selectExpandSixToSeven').value = config.AudioConfig.Expand61;

                    var selectAudioEndPoint = view.querySelector('.selectAudioEndPoint');
                    selectAudioEndPoint.innerHTML = audioDevices.map(function (i) {
                        return '<option value="' + i.ID + '">' + i.Name + '</option>';
                    }).join('');
                    selectAudioEndPoint.value = config.AudioConfig.AudioDevice;
                });
            });
        }

        function getAudioDevices() {

            return sendCommand('getaudiodevices');
        }

        function onResetClick() {
            return sendCommand('configresetdefaults-audio');
        }

        function getConfiguration() {

            return sendCommand('config');
        }

        function saveConfiguration(config) {

            return sendCommand('configsave', config);
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

            });
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
    }

});