require(['apphost', 'events'], function (apphost, events) {
    'use strict';

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
});