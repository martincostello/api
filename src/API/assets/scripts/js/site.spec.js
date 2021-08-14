// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

describe('Given the namespaces are defined', function () {

  it('then martinCostello is not null', function () {
    expect(martinCostello).not.toBeNull();
  });

  it('then martinCostello.api is not null', function () {
    expect(martinCostello.api).not.toBeNull();
  });
});
