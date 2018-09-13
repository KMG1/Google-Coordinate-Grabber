/* 
 * ==========================================
 * CoodinateGrabber
 * 
 * Copyright (C) 2018 Kyle Grygo
 * ==========================================
 * 
 * The purpose of this program is to accept a specially formatted input file and parse it for address data. This data is then sent out to the Google Geolocation API in an attempt to determine
 * the geographic coordinates associated with the address. Results are output to a file and the total number addresses that failed to return results is displayed on screen. Note that the results
 * returned from the API might not be entirely accurate; the API struggles with addresses such as "123 Street Ave Upper Apt". It may not return a result, or it may return a bogus result that isn't anywhere 
 * near where it should be. To avoid these issues you should scrub the input file to address inconsistencies such as this (AKA: garbage in, garbage out).
 * 
 * 
 * Expected file format (tab-delimited):
 * 
 * Address  City    State
 * 
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace CoordinateGrabber
{
    // Google API Objects (variables lower-case intentionally for deserialization)
    public class GeoCodeResponse
    {
        public List<Results> results;
        public string status;
    }

    public class Results
    {
        public string formatted_address;
        public Geometry geometry;
    }

    public class Geometry
    {
        public Location location;
    }


    // Used for storing coordinate data
    public class Location
    {
        public string Address;
        public string Latitude;
        public string Longitude;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter full path of data file: ");
            var filePath = Console.ReadLine();

            Console.WriteLine("Enter full path to save output file to: ");
            var outputFilePath = Console.ReadLine();

            var totalRecords = 0;
            var failures = 0;

            var googleUri = ConfigurationManager.AppSettings["MapsUri"];
            var streamreader = new StreamReader(filePath);

            char[] delimiter = { '\t' };

            // grab dispatch calls from file
            var addressLines = new List<string[]>();
            while (streamreader.Peek() > 0)
            {
                var line = streamreader.ReadLine().Split(delimiter);

                addressLines.Add(line);
                totalRecords++;
            }


            // iterate over all collected addresses and attempt to fetch coordinates
            var geoLocations = new List<Location>();
            foreach (var address in addressLines)
            {
                var link = string.Format(googleUri, address[0].Replace(' ', '+') + string.Format("+{0}+{1}", address[1], address[2]));
                var uri = new Uri(link);

                var request = WebRequest.Create(uri);
                request.Method = "GET";
                request.ContentType = "application/json";

                try
                {
                    var response = request.GetResponse();
                    string responseData;
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        geoLocations.Add(new Location
                        {
                            Address = address[0],
                            Latitude = string.Empty,
                            Longitude = string.Empty
                        });
                        failures++;

                        // sleep to avoid Google throttling requests
                        System.Threading.Thread.Sleep(200);
                        continue;
                    }


                    using (var reader = new StreamReader(responseStream))
                        responseData = reader.ReadToEnd();

                    // deserialize response stream into a more readable format
                    var result = JsonConvert.DeserializeObject<GeoCodeResponse>(responseData);


                    // Ensure we look at an address in the specified city (API will return matches from other cities as well)
                    var matchingLocation = result.results.FirstOrDefault(r => r.formatted_address.Contains(string.Format("{0}, {1}", address[1], address[2])));
                    if (matchingLocation == null)
                    {
                        geoLocations.Add(new Location
                        {
                            Address = address[0],
                            Latitude = string.Empty,
                            Longitude = string.Empty
                        });
                        failures++;

                        // sleep to avoid Google throttling requests
                        System.Threading.Thread.Sleep(200);
                        continue;
                    }

                    if (result.status != "OK")
                    {
                        Console.WriteLine(result.status);
                        geoLocations.Add(new Location
                        {
                            Address = address[0],
                            Latitude = string.Empty,
                            Longitude = string.Empty
                        });
                        failures++;

                        // sleep to avoid Google throttling requests
                        System.Threading.Thread.Sleep(200);
                        continue;
                    }

                    geoLocations.Add(matchingLocation.geometry.location);
                }
                catch (WebException ex)
                {
                    geoLocations.Add(new Location
                    {
                        Address = address[0],
                        Latitude = string.Empty,
                        Longitude = string.Empty
                    });
                    failures++;

                    // sleep to avoid Google throttling requests
                    System.Threading.Thread.Sleep(200);
                    continue;
                }
                catch (Exception ex)
                {
                    geoLocations.Add(new Location
                    {
                        Address = address[0],
                        Latitude = string.Empty,
                        Longitude = string.Empty
                    });
                    failures++;

                    // sleep to avoid Google throttling requests
                    System.Threading.Thread.Sleep(200);
                    continue;
                }

                // sleep to avoid Google throttling requests
                System.Threading.Thread.Sleep(200);
            }

            // Write results out to file
            using (StreamWriter file = new StreamWriter(outputFilePath))
            {
                foreach (var coord in geoLocations)
                {
                    file.WriteLine(string.Format("{0}{1}{2}{1}{3}", coord.Address, delimiter, coord.Latitude, coord.Longitude));
                }
            }

            Console.WriteLine("Total Records: {0}", totalRecords);
            Console.WriteLine("Total Failures: {0}", failures);
            Console.WriteLine(string.Empty);

            Console.ReadLine();
        }

    }
}
