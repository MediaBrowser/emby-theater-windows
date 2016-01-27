require(['apphost', 'appSettings', 'connectionManager', 'events'], function (appHost, appSettings, connectionManager, events) {

    var settingsKey = 'dwmcheck';

    // Show once per week
    var messageInterval = 86400000 * 7;

    function showMessage() {

        require(['dialog'], function (dialog) {
            dialog({
                title: 'Enable Windows DWM',
                text: "We've detected that the Microsoft Desktop Window Manager has been disabled. If you need advanced features such as audio bitstreaming or MadVr, then you'll need to enable the Microsoft Desktop Window Manager. If you do not need these features then you can ignore this message.",
                buttons: ['Learn More', 'OK'],
                callback: function (index) {
                    if (index == 0) {
                        openHelp();
                    }
                }
            });
        });
    }

    function openHelp() {
        require(['shell'], function (shell) {
            shell.openUrl('http://www.tomshardware.com/faq/id-2066988/enable-window-transparency-aero-windows.html');
        });
    }

    function showMessageIfNeeded() {

        if (appHost.supports('windowtransparency')) {
            appSettings.set(settingsKey, '');
            return;
        }

        var lastDisplay = parseInt(appSettings.get(settingsKey) || '0');

        var now = new Date().getTime();
        if ((now - lastDisplay) > messageInterval) {
            showMessage();
            appSettings.set(settingsKey, now);
        }
    }

    events.on(connectionManager, 'localusersignedin', showMessageIfNeeded);

});