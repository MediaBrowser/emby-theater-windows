define(['loading', 'pluginManager'], function (loading, pluginManager) {

    return function (view, params) {

        var self = this;

        view.addEventListener('viewbeforeshow', function (e) {

            var isRestored = e.detail.isRestored;

            Emby.Page.setTitle('Windows Player');

            loading.hide();

            if (!isRestored) {
                renderSettings();

                view.querySelector('.btnMadvr').addEventListener('click', onMadvrClick);
            }
        });

        view.addEventListener('viewbeforehide', function (e) {

            saveSettings();
        });

        function onMadvrClick() {
            Emby.Page.show(pluginManager.mapPath('directshowplayer', 'directshowplayer/madvr.html'));
        }

        function saveSettings() {

            getConfiguration().then(function (config) {

                var selectHwaMode = view.querySelector('.selectHwaMode');
                config.VideoConfig.HwaMode = selectHwaMode.getValue();

                var selectVideoRenderer = view.querySelector('.selectVideoRenderer');
                config.VideoConfig.EnableMadvr = selectVideoRenderer.getValue() == 'madVR';
                config.VideoConfig.UseCustomPresenter = selectVideoRenderer.getValue() == 'EVRCP';

                var selectRefreshRateMode = view.querySelector('.selectRefreshRateMode');
                config.VideoConfig.AutoChangeRefreshRate = selectRefreshRateMode.getValue();

                saveConfiguration(config);
            });

        }

        function renderSettings() {

            getConfiguration().then(function (config) {

                var selectHwaMode = view.querySelector('.selectHwaMode');
                selectHwaMode.setValue(config.VideoConfig.HwaMode);

                var selectVideoRenderer = view.querySelector('.selectVideoRenderer');
                var videoRenderer = config.VideoConfig.EnableMadvr ? 'madVR' : config.VideoConfig.UseCustomPresenter ? 'EVRCP' : 'EVR';
                selectVideoRenderer.setValue(videoRenderer);

                var selectRefreshRateMode = view.querySelector('.selectRefreshRateMode');
                selectRefreshRateMode.setValue(config.VideoConfig.AutoChangeRefreshRate);
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