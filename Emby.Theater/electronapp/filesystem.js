define([], function () {
    'use strict';

    function exits(endpoint, path) {
        return new Promise(function (resolve, reject) {

            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'http://localhost:8154/' + endpoint, true);
            xhr.onload = function () {
                if (this.response == 'true') {
                    resolve();
                } else {
                    reject();
                }
            };

            try {
                //xhr.setRequestHeader('Content-Length', path.length);
            } catch (err) {
                //console.log(err);
            }

            xhr.setRequestHeader('Content-Type', 'text/plain');
            xhr.onerror = reject;
            xhr.send(path);
        });
    }

    require(['css!electronfile://windowstyle']);

    return {
        fileExists: function (path) {
            return exits('fileexists', path);
        },
        directoryExists: function (path) {
            return exits('directoryexists', path);
        }
    };
});