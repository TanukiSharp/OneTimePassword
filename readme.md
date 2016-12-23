# Overview

This is an implementation of the Time-based One-time password algorithm, also known as TOTP or Two Factor Authentication.

You can find more information about this algorithm on [Wikipedia][3] and by reading the [RFC 6238][4].

To test this implementation, you can download the Google Authenticator application for [Android][1] or for [iOS][2], and compare the result you get on your screen.

Add a new code, and type the secret manually, as on the following screenshot:

*Make sure to select 'Time based' and not 'Counter based'.*

![Google Authenticator on Android](cellphone_screenshot.png "Google Authenticator on Android")

Then you should see codes matching on your phone and on the console application.

# Base32 implementation

The `Base32` implementation is a naive implementation and allocates a lot of memory.
A better implementation could allocate much less, but would require a bit more work, and several unit tests.

[1]: https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2
[2]: https://itunes.apple.com/us/app/google-authenticator/id388497605
[3]: https://en.wikipedia.org/wiki/One-time_password
[4]: https://tools.ietf.org/html/rfc6238
