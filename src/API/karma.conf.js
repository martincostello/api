// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

module.exports = function (config) {
    config.set({

        autoWatch: false,
        concurrency: Infinity,

        browsers: ["PhantomJS"],
        frameworks: ["jasmine"],

        files: [
            "wwwroot/lib/**/dist/*.js",
            "Assets/Scripts/js/site.js",
            "Assets/Scripts/**/*.spec.js"
        ],

        htmlDetailed: {
            splitResults: false
        },

        plugins: [
            "karma-appveyor-reporter",
            "karma-chrome-launcher",
            "karma-html-detailed-reporter",
            "karma-jasmine",
            "karma-phantomjs-launcher"
        ],

        reporters: [
            "progress",
            "appveyor"
        ]
    })
}
