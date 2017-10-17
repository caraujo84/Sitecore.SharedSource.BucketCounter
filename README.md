# About Sitecore Bucket Counter module Sitecore.SharedSource.BucketCounter
This is a simple but handy module for developers and editors. This module allow content editor users to have a visual counter of how many items a bucket has inside of it.

On this counting the module only considers content items, inner folders that belong to a bucket structure are not considered at all.

By default only buckets will show the counter next to the item display name. There is a configuration file to enhance this functionality to all items.

This module only applies to content inside the /sitecore/content/ tree, items in other trees are not considered.

# Sitecore Package

This package contains the following files
 - /sitecore/shell/Applications/Content Manager/Default.aspx
 - /sitecore/shell/Applications/Content Manager/Execute.aspx
 - /App_Config/Include/Sitecore.SharedSource.BucketCounter.config
 - /bin/Sitecore.SharedSource.BucketCounter.dll
 
 To use this module you just need to install it through installation wizard and you are good to go. The installer will install above files on your sitecore instance.