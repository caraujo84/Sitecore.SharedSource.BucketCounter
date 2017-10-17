using Sitecore.Data.Items;

namespace Sitecore.SharedSource.BucketCounter.Utilities
{
    public class CounterUtility
    {
        public string GetChildrenCounter(Item item)
        {
            var searchUtility = new SearchUtility();

            if (!item.Paths.FullPath.ToLower().StartsWith("/sitecore/content/")) return string.Empty;

            if (item.Fields["__Is Bucket"] != null && item.Fields["__Is Bucket"].Value.Equals("1"))
            {
                return $"({searchUtility.FindBucketableItems(item)})";
            }

            return string.Empty;
        }
    }
}