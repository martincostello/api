﻿// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

(function () {
  $(document).ready(function () {
    var element = $('#build-date');
    if (element) {
      var timestamp = element.attr('data-timestamp');
      var format = element.attr('data-format');
      var value = moment(timestamp, format);
      if (value.isValid()) {
        var text = value.fromNow();
        element.text('(' + text + ')');
      }
    }
  });
})();
