define([], function () {
    'use strict';

    function exists(endpoint, path) {

        // we're doing a windows file system call which will not work with an nfs path
        if (path.indexOf('://') !== -1) {
            return Promise.reject();
        }

        return new Promise(function (resolve, reject) {

            try {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', 'http://localhost:8154/' + endpoint, true);
                xhr.onload = function () {
                    if (this.response == 'true') {
                        resolve();
                    } else {
                        reject();
                    }
                };

                xhr.setRequestHeader('Content-Type', 'text/plain');
                xhr.onerror = reject;
                xhr.send(path);
            } catch (err) {
                reject();
            }
        });
    }

    return {
        fileExists: function (path) {
            return exists('fileexists', path);
        },
        directoryExists: function (path) {
            return exists('directoryexists', path);
        }
    };
});