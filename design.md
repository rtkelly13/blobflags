# blobflags design

Local config for passing into blobflags ui

``` json
{
    "buckets": [
        {
            "environmentName": "Debug",
            "bucketName": "my-product-debug",
            "prefix": "/features",
        }
    ]
}
```

## checkpoints

The idea is that the checkpoint files will store the root file with all the data about every flag.
Or at least a pointer to the file stored in the `/data` folder so when this file changes the client lib can detect that easily

`my-product-debug/features/checkpoint.json`
`my-product-debug/features/checkpoints/2020-01-08T09:00.00.0000000+00:00.json`
`my-product-debug/features/checkpoints/2020-01-08T09:01.00.0000000+00:00.json`
`my-product-debug/features/checkpoints/2020-01-08T09:02.00.0000000+00:00.json`

``` json
{
    "Colour": "#000000",
    "Name": "Debug",
    "RefreshInterval": "1s",
    "Group":
    [
        {
            "Name": "ServerlessFlags",
            "RefreshInterval": "1m",
            "Hash": "SHA1 of flags file"
        }
    ]
}
```

Tags against checkpoint file - used to detect change in checkpoint file

``` json
{
    "VersionHash": "{SHA1 ^}"
}
```

## Data file

A data file contains the actual flag data separate to the configuration this is to reduce download size for each client
as they may require

`my-product-debug/features/data/{SHA1}.json`

``` json
{
    "Features": [
        { "FlagName": "my-product-1234", "Value": true },
        { "FlagName": "Feature-F", "Value": false },
    ]
}
```

# SDK

``` csharp
var client = new blobflagsclient(new S3(), "my-product-debug", prefix: "features", failIfEmpty: true);

var flagValue = await client.GetFlagAsync("ServerlessFlags", "Feature-F");

if(flagValue){

}
```
