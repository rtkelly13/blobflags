# blobflags design

Local config for passing into blobflags ui

``` json
{
    "buckets": [
        {
            "environmentName": "Debug",
            "bucketName": "my-product-debug",
            "prefix": "/features",
            "gzip": true //default: false
        }
    ]
}
```

## Checkpoints

The idea is that the checkpoint files will store the root file with all the data about the flag groups.
This should change infrequently so doesn't necessarily need a checkpoint file but will be good to look through the history.

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
            "RefreshInterval": "1m"
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
as they may require. Using the checkpoint system and having the hash of both the Group and Root Checkpoints.
This can detect when the root file changes.

`my-product-debug/features/{GroupName}/checkpoint.json`

``` json
{
    "Features": [
        { "FlagName": "my-product-1234", "Value": true },
        { "FlagName": "Feature-F", "Value": false },
    ]
}
```

## SDK

``` csharp
var client = new blobflagsclient(new S3(), "my-product-debug", prefix: "features", failIfEmpty: true);

var flagValue = await client.GetFlagAsync("ServerlessFlags", "Feature-F");

if(flagValue){

}
```

## Scaling Idea

Instead of allowing direct updates to the S3 files pipe all the changes through a FIFO SQS Queue,
With a lambda function on the end of it to process the batched changes.
This approach should guarantee high throughput with handling syncronisation and batching.
By using the message queue and the local config to keep the bucket values updated.
[AWS Link](https://aws.amazon.com/blogs/compute/new-for-aws-lambda-sqs-fifo-as-an-event-source/)
I will provide an example pulumi script to scaffold the infrastructure.
Need to figure out how to source a lambda function to people who want to use it.
