# Google Coordinate Grabber

This small console app is designed to read in a tab delimited text file that contains address information. It then takes this information and runs it through Google's GeoLocation API Service in an attempt to determine the geographic coordinates. 

This was originally written as a quick utility to augment work I did for a friend who needed to display thousands of addresses onto Google Maps. The original file they worked with contained written addresses, so I wrote this program to quickly fetch the geographic coordinates to plot them on a webpage using Google Maps.

## Input File

As stated before, when running the program merely point it to a source file that has three columns of data, in this order: Street, City, State. This program currently only works for US addresses, but you can modify it to fit your needs.


## Usage Instructions

This app is very straightforward to run. Here is a handy summary of how easy it is:

1. Obtain a Google Maps API Key
2. Stick that API Key where indicated in [App.config](CoordinateGrabber/App.config)
3. Build & run the program
4. Point the program to your input & output file location
5. Sit back and wait for processing to complete. Once complete, the process will show you some summary information.


## Additional Notes

Google limits the number of requests you can make to their Geolocation service, currently it is limited to 2500 calls per day. If you need to make more calls than that within a 24-hour window, you will need to setup billing information for your Google account. More info can be found on the [Usage Limits page](https://developers.google.com/maps/documentation/geocoding/usage-and-billing).
