require(['apphost', 'events'], function (apphost, events) {
    'use strict';

    function sendCommand(name) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'http://localhost:8154/' + name, true);
        xhr.send();
    }

    function sendRunAtStartupConfigValue(appSettings) {
        var value = appSettings.get('runatstartup');
        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'http://localhost:8154/runatstartup-' + value, true);
        xhr.send();
    }

    require(['appSettings'], function (appSettings) {

        events.on(appSettings, 'change', function (e, name) {
            if (name == 'runatstartup') {
                sendRunAtStartupConfigValue(appSettings);
            }
        });
    });

    function onWindowStateChanged(e) {

        sendCommand('windowstate-' + e.detail.windowState.toLowerCase());
    }

    if (document.windowState && document.windowState !== 'Fullscreen') {
        require(['css!electronfile://windowstyle']);
    } else {
        require(['css!electronfile://windowstyle']);
    }

    document.addEventListener('windowstatechanged', onWindowStateChanged);
});