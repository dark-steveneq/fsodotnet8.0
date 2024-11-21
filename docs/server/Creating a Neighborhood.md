# Creating a Neighborhood
Once you have your server running, you'll need to create at least one neighborhood. Without one you your server will literally crash when you'd try to purchase a lot since each lot needs to be associated with a neighborhood. Here are the steps of making one!

1. Create a JSON file  
The file can have whatever name you'd like but for the sake of the tutorial, I'll name it `neighborhoods.json`. The file contains an array of a special neighborhood structure. Here's the list of all its fields
```json5
{
    "GUID": "00000000-0000-0000-0000-000000000000", // Neighborhood GUID, should get randomized on import
    "Name": "Name",                                 // Neighborhood name
    "Description": "Description",                   // Neighborhood description
    "Location": {"x": 0, "y": 0},                   // Neighborhood location
    "Color": {"r": 0, "g": 0, "b": 0, "a": 0},      // Neighborhood color used in the neighborhood filter
    "ID": 1,                                        // ID of the neighborhood you want to create/replace
    "DistanceMul": 1.0                              // Appears to be unreferenced, supposed to do something with rendering
}
```
Now that you know the structure, you copy the structure into an array in that JSON file so that its contents looks like this
```json5
[
    {...}
]
```

2. Import the file  
In order to import the neighborhood JSON file, launch the server with `--import-nhood` argument and specify a Shard ID (City ID) and path to the JSON file you've created.

3. Start the server  
Everything should be good to go!