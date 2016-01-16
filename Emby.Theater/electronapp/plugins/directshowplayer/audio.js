define(['loading'], function (loading) {

    return function (view, params) {

        var self = this;

        view.addEventListener('viewbeforeshow', function (e) {

            var isRestored = e.detail.isRestored;

            Emby.Page.setTitle('Windows Player');

            loading.hide();

            if (!isRestored) {
                renderSettings();
            }
        });

        view.addEventListener('viewbeforehide', function (e) {

            saveSettings();
        });

        function saveSettings() {

            getConfiguration().then(function (config) {

                var selectAudioBitstreamingMode = view.querySelector('.selectAudioBitstreamingMode');
                config.AudioConfig.AudioBitstreaming = selectAudioBitstreamingMode.getValue();

                var selectAudioRenderer = view.querySelector('.selectAudioRenderer');
                config.AudioConfig.Renderer = selectAudioRenderer.getValue();

                var selectSpeakerLayout = view.querySelector('.selectSpeakerLayout');
                config.AudioConfig.SpeakerLayout = selectSpeakerLayout.getValue();

                var selectDrc = view.querySelector('.selectDrc');
                config.AudioConfig.EnableDRC = selectDrc.getValue() != '';
                config.AudioConfig.DRCLevel = selectDrc.getValue() || '100';

                saveConfiguration(config);
            });

        }

        function renderSettings() {

            getConfiguration().then(function (config) {

                var selectAudioBitstreamingMode = view.querySelector('.selectAudioBitstreamingMode');
                selectAudioBitstreamingMode.setValue(config.AudioConfig.AudioBitstreaming);

                var selectAudioRenderer = view.querySelector('.selectAudioRenderer');
                selectAudioRenderer.setValue(config.AudioConfig.Renderer);

                view.querySelector('.selectSpeakerLayout').setValue(config.AudioConfig.SpeakerLayout);

                var selectDrc = view.querySelector('.selectDrc');
                selectDrc.setValue(config.AudioConfig.EnableDRC ? config.AudioConfig.DRCLevel : '');
            });
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