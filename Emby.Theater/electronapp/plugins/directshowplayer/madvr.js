define(['loading'], function (loading) {

    return function (view, params) {

        var self = this;

        view.addEventListener('viewbeforeshow', function (e) {

            var isRestored = e.detail.isRestored;

            Emby.Page.setTitle('Madvr');

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

                var selectSmoothMotion = view.querySelector('.selectSmoothMotion');
                config.VideoConfig.UseMadVrSmoothMotion = selectSmoothMotion.value != '';
                config.VideoConfig.MadVrSmoothMotionMode = selectSmoothMotion.value || 'avoidJudder';
                saveConfiguration(config);
            });

        }

        function renderSettings() {

            getConfiguration().then(function (config) {

                var selectSmoothMotion = view.querySelector('.selectSmoothMotion');
                selectSmoothMotion.value = config.VideoConfig.UseMadVrSmoothMotion ? config.VideoConfig.MadVrSmoothMotionMode : '';
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