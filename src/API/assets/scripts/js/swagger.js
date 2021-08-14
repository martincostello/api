// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

(function () {

  window.onload = function () {

    var url = $('link[rel="swagger"]').attr('href');

    if (url) {

      var hideTopbarPlugin = function () {
        return {
          components: {
            Topbar: function () {
              return null;
            }
          }
        };
      };

      var ui = SwaggerUIBundle({
        url: url,
        dom_id: '#swagger-ui',
        deepLinking: true,
        presets: [
          SwaggerUIBundle.presets.apis,
          SwaggerUIStandalonePreset
        ],
        plugins: [
          SwaggerUIBundle.plugins.DownloadUrl,
          hideTopbarPlugin
        ],
        layout: 'StandaloneLayout',
        booleanValues: ['false', 'true'],
        defaultModelRendering: 'schema',
        displayRequestDuration: true,
        jsonEditor: true,
        showRequestHeaders: true,
        supportedSubmitMethods: ['get', 'post'],
        validatorUrl: null,
        responseInterceptor: function (response) {
          // Delete overly-verbose headers from the UI
          delete response.headers['content-security-policy'];
          delete response.headers['feature-policy'];
        }
      });

      window.ui = ui;
    }
  };
})();
