using System;
using System.IO;
using Deploy;
using Pulumi;
using Pulumi.Aws.S3;
using Pulumi.Aws.S3.Inputs;

await Deployment.RunAsync<AwsInfra>();

class AwsInfra : Stack
{
    public AwsInfra()
    {
        // Create an AWS resource (S3 Bucket)
        var bucket = new Bucket("my-bucket", new BucketArgs
        {
            Website = new BucketWebsiteArgs
            {
                IndexDocument = "index.html"
            }
        });

        var publicDir = Directory.EnumerateFiles("www");

        foreach(var filePath in publicDir)
        {
            var name = Path.GetFileName(filePath);
            var contents = File.ReadAllBytes(filePath);
            var bucketArgs = new BucketObjectArgs
            {
                Acl = "public-read",
                Bucket = bucket.BucketName,
                Key = name,
                ContentBase64 = Convert.ToBase64String(contents),
                ContentType = MimeTypes.GetMimeType(name)
            };

            var fileWithinBucket = new BucketObject(name, bucketArgs, new CustomResourceOptions
            {
                Parent = bucket
            });
        }

        // Export the name of the bucket
        this.BucketName = bucket.Id;
        // Export the URL of the bucket
        this.BucketUrl = Output.Format($"http://{bucket.WebsiteEndpoint}");
    }

    [Output]
    public Output<string> BucketName { get; set; }
    [Output]
    public Output<string> BucketUrl { get; set; }
}