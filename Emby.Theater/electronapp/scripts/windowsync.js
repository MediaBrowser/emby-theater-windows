require(['apphost'], function (apphost) {

    function sendWindowStateCommand(e) {

        var state = e.detail.windowState;

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
});