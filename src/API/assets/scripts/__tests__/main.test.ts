// Copyright (c) Martin Costello, 2016. All rights reserved.
// Licensed under the MIT license. See the LICENSE file in the project root for full license information.

import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('main', () => {
    beforeEach(() => {
        document.body.innerHTML = '';
        vi.clearAllMocks();
    });

    it('should be defined', () => {
        expect(true).toBe(true);
    });
});
