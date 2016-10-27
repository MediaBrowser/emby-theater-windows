require(['apphost', 'events'], function (apphost, events) {
    'use strict';

    var windowDragRegion;

    function sendWindowStateCommand(e) {

        var state = e.detail.windowState;

        if (!windowDragRegion) {
            windowDragRegion = document.querySelector('.windowDragRegion');
        }

        if (state.toLowerCase() == 'normal') {
            windowDragRegion.classList.remove('nodrag');
        } else {
            windowDragRegion.classList.add('nodrag');
        }

        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'http://localhost:8154/windowstate-' + state, true);

        xhr.send();
    }

    function sendWindowSizeCommand() {

        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'http://localhost:8154/windowsize', true);
        xhr.send();
    }

    document.addEventListener('windowstatechanged', sendWindowStateCommand);
    window.addEventListener('resize', sendWindowSizeCommand);
    window.addEventListener('move', sendWindowSizeCommand);

    sendWindowSizeCommand({ detail: { windowState: apphost.getWindowState() } });

    function sendCommand(name) {

        var xhr = new XMLHttpRequest();
        xhr.open('GET', 'electronapphost://' + name, true);

        xhr.send();
    }

    sendCommand('loaded');

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

    require(['css!electronfile://windowstyle']);
});