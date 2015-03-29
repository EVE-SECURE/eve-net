This library was designed as a resource to read data provided by the EVE Online API, which is documented here: http://wiki.eveonline.com/en/wiki/Category:API
The EVE.Net libary is released under the MIT License, details of which are included in the file 'license.txt'

The classes provided are intended to be a one-to-one mapping to the various API calls available in the EVE API.  They do not provide any additional details or
do any mappings against the EVE database dumps.

This library is compiled against .NET 2.0, and contains no additional references or dependencies.

Using this library is pretty straightforward.  Each class provides one or more constructors which accept the arguments that are necessary to make a successful
call against the API.  In all cases where API keys are required, it is expected that you will use the new API key format.  Use of the old keys has not been tested.
You can obtain your API keys from the key management page here: https://support.eveonline.com/api