// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

const puppeteer = require('puppeteer');
process.env.CHROME_BIN = puppeteer.executablePath();

module.exports = function (config) {
  config.set({

    autoWatch: false,
    concurrency: Infinity,

    browsers: ['ChromeHeadlessNoSandbox'],

    customLaunchers: {
      ChromeHeadlessNoSandbox: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox']
      }
    },

    frameworks: ['jasmine'],

    files: [
      'Assets/Scripts/js/site.js',
      'Assets/Scripts/**/*.spec.js'
    ],

    htmlDetailed: {
      splitResults: false
    },

    plugins: [
      'karma-chrome-launcher',
      'karma-html-detailed-reporter',
      'karma-jasmine'
    ],

    reporters: [
      'progress'
    ]
  });
};
