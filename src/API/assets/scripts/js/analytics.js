// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

(function () {
  var trackingId = $('meta[name="google-analytics"]').attr('content');
  function gtag(){dataLayer.push(arguments);}
  if (trackingId !== '') {
    window.dataLayer = window.dataLayer || [];
    gtag('js', new Date());
    gtag('config', trackingId);
  }
})();
