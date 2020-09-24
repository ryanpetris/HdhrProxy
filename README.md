# SiliconDust HDHomeRun Proxy

Proxy for legacy SiliconDust HDHomeRun devices. This proxy sits in front of one or many HDHomeRun devices to simulate channel scanning and lineups; this negates the need to use SiliconDust's online service and Windows application for channel scanning and lineup support.

Current limitations:

* Each proxied devices requires an IP address assigned just for that devices proxy. This is due to most applications identifying HDHomeRun devices by only the device's IP address.
* If the application you use automatically scans the network and updates devices based on the device ID and scan results, you'll need to assign a Faux Device ID to each proxy. These faux device IDs should be unique and not match any existing device ids on your network.
* UDP streams for television data come directly from the device rather than being proxied. This does not seem to cause a problem, and therefore no effort has yet been made to proxy these streams.
