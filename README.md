# About Sitecore Bucket Counter module
This is a simple but handy module for developers and editors. This module allow content editor users to have a visual counter of how many items a bucket has inside of it.

On this counting the module only considers content items, inner folders that belong to a bucket structure are not considered at all.

By default only buckets will show the counter next to the item display name. There is a configuration file to enhance this functionality to all items.

This module only applies to content inside the /sitecore/content/ tree, items in other trees are not considered.

## Sitecore Package
[Download Package Sitecore 8.2](https://github.com/caraujo84/Sitecore.SharedSource.BucketCounter/raw/master/package/Bucket%20Counter%20Module%20Sitecore%208.2.zip)

This package contains the following files
 - /sitecore/shell/Applications/Content Manager/Default.aspx
 - /sitecore/shell/Applications/Content Manager/Execute.aspx
 - /App_Config/Include/Sitecore.SharedSource.BucketCounter.config
 - /bin/Sitecore.SharedSource.BucketCounter.dll
 
 To use this module you just need to install it through installation wizard and you are good to go. The installer will install above files on your sitecore instance.

## Usage

The counter works just after installation for all buckets inside the /sitecore/content/ tree. If you want to have the same behavior for all items inside the same tree, please update the configuration file and change the value of the setting from 0 (disabled) to 1 (enable).

```xml
<!-- To add the counter for all tree nodes: 0 for disable and 1 for enable -->
<settings>
      <setting name="SharedSource.BucketCounter.AllItems" value="0" />
</settings>
```
 
## Screenshots
New Experience with Buckets
![New Experience with Buckets](screenshots/bucketCounter.png?raw=true "Bucket Counter")

New Experience with Bucket Counter For all Items
![New Experience with Bucket Counter For all Items](screenshots/bucketCounter.png?raw=true "Bucket Counter For All")