require(['apphost', 'shell', 'events'], function (apphost, shell, events) {

    var isExternalWindowOpen = false;

    function sendWindowStateCommand(e) {

        var state = e.detail.windowState;

        if (state.toLowerCase() == 'normal') {
            document.querySelector('.windowDragRegion').classList.remove('nodrag');
        } else {
            document.querySelector('.windowDragRegion').classList.add('nodrag');
        }

        var xhr = new XMLHttpRequest();
        xhr.open('POST', 'http://localhost:8154/windowstate-' + state + "?externalwindow=" + isExternalWindowOpen, true);

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

    events.on(shell, 'exec', function (e) {
        isExternalWindowOpen = true;
    });

    events.on(shell, 'closed', function (e) {
        isExternalWindowOpen = false;
    });

});