define(['loading', 'pluginManager', 'scrollHelper', 'appSettings', 'emby-select', 'emby-checkbox'], function (loading, pluginManager, scrollHelper, appSettings) {

    function getMediaFilterInfo() {
        return new Promise(function (resolve, reject) {

            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'http://localhost:8154/mediafilterinfo', true);
            xhr.onload = function () {
                resolve(this.response);
            };

            xhr.onerror = reject;
            xhr.send();
        });
    }

    function openMadVrFolder() {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'http://localhost:8154/openmadvr', true);

        xhr.send();
    }

    return function (view, params) {

        view.addEventListener('viewbeforeshow', function (e) {

            var isRestored = e.detail.isRestored;

            Emby.Page.setTitle('Video Settings');

            loading.hide();

            if (!isRestored) {
                scrollHelper.centerFocus.on(view, false);

                renderSettings();
            }
        });

        view.addEventListener('viewbeforehide', saveSettings);

        function saveSettings() {

            var elems = view.querySelectorAll('.emby-select');
            var i, length;
            for (i = 0, length = elems.length; i < length; i++) {

                appSettings.set(elems[i].getAttribute('data-setting'), elems[i].value);
            }

            elems = view.querySelectorAll('[is=emby-checkbox]');
            var i, length;
            for (i = 0, length = elems.length; i < length; i++) {

                appSettings.set(elems[i].getAttribute('data-setting'), elems[i].checked.toString().toLowerCase());
            }
        }

        function renderSettings() {

            var elems = view.querySelectorAll('.emby-select');
            var i, length;
            for (i = 0, length = elems.length; i < length; i++) {

                elems[i].value = appSettings.get(elems[i].getAttribute('data-setting')) || '';
            }

            elems = view.querySelectorAll('[is=emby-checkbox]');
            var i, length;
            for (i = 0, length = elems.length; i < length; i++) {

                elems[i].checked = appSettings.get(elems[i].getAttribute('data-setting')) === 'true';
            }

            getMediaFilterInfo().then(function (path) {

                var elem = view.querySelector('.filterPath');
                elem.innerHTML = '<a is="emby-linkbutton" class="button-flat">' + path + '\\madVR' + '</a>';
                elem.querySelector('a').addEventListener('click', openMadVrFolder);
            });
        }
    }

});