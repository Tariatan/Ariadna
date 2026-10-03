# Media duration regression fixture

`media-two-seconds.avi` is a generated 32 by 32 pixel MJPEG AVI with 20 synthetic
grayscale frames at 10 frames per second. Its duration is exactly two seconds.
It contains no personal media and requires no external file or encoder to run tests.

The fixture exercises the existing-file path that a missing-file test bypasses,
checks milliseconds-to-TimeSpan conversion, and verifies that all four detail forms
open without loading the legacy Windows Shell library.
