// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

import moment from 'moment';

document.addEventListener('DOMContentLoaded', () => {
    const trackingId = document.querySelector('meta[name="google-analytics"]').getAttribute('content');
    if (trackingId && 'dataLayer' in window) {
        const dataLayer = (window.dataLayer as any[]) || [];
        const gtag = (...args: any[]) => {
            dataLayer.push(args);
        };
        gtag('js', new Date());
        gtag('config', trackingId);
    }

    const element = document.getElementById('build-date');
    if (element) {
        const timestamp = element.getAttribute('data-timestamp');
        const format = element.getAttribute('data-format');

        const value = moment(timestamp, format);
        if (value.isValid()) {
            const text: string = value.fromNow();
            element.textContent = `(${text})`;
        }
    }

    if ('SwaggerUIBundle' in window && 'SwaggerUIStandalonePreset' in window) {
        const swaggerUIBundle = window['SwaggerUIBundle'] as any;
        const swaggerUIStandalonePreset = window['SwaggerUIStandalonePreset'] as any;
        const url = document.querySelector('link[rel="swagger"]').getAttribute('href');
        const ui: any = swaggerUIBundle({
            url: url,
            /* eslint-disable @typescript-eslint/naming-convention */
            dom_id: '#swagger-ui',
            deepLinking: true,
            presets: [swaggerUIBundle.presets.apis, swaggerUIStandalonePreset],
            plugins: [
                swaggerUIBundle.plugins.DownloadUrl,
                (): any => {
                    return {
                        components: {
                            /* eslint-disable @typescript-eslint/naming-convention */
                            Topbar: (): any => null,
                        },
                    };
                },
            ],
            layout: 'StandaloneLayout',
            booleanValues: ['false', 'true'],
            defaultModelRendering: 'schema',
            displayRequestDuration: true,
            jsonEditor: true,
            showRequestHeaders: true,
            supportedSubmitMethods: ['get', 'post'],
            tryItOutEnabled: true,
            validatorUrl: null,
            responseInterceptor: (response: any): any => {
                delete response.headers['content-security-policy'];
                delete response.headers['content-security-policy-report-only'];
                delete response.headers['cross-origin-embedder-policy'];
                delete response.headers['cross-origin-opener-policy'];
                delete response.headers['cross-origin-resource-policy'];
                delete response.headers['permissions-policy'];
            },
        });

        (window as any).ui = ui;
    }
});
