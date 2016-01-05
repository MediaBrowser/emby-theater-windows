(function (globalScope) {

    globalScope.ServerDiscovery = {

        findServers: function (timeoutMs) {

            var deferred = DeferredBuilder.Deferred();

            HttpClient.request({

                url: 'http://localhost/serverdiscovery',
                type: 'GET',
                dataType: 'json'

            }).then(function (response) {
                deferred.resolveWith(null, [response]);

            }, function () {

                deferred.resolveWith(null, [[]]);
            });

            // Expected server properties
            // Name, Id, Address, EndpointAddress (optional)

            return deferred.promise();
        }
    };

})(this);