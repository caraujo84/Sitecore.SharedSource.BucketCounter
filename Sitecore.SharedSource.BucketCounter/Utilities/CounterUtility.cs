using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.SharedSource.BucketCounter.Utilities
{
    public class CounterUtility
    {
        /// <summary>
        /// Gets the children counter.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>string that reprensts the counter</returns>
        public string GetChildrenCounter(Item item)
        {
            Assert.IsNotNull(item,"item can't be null");

            var counterAllItems = Configuration.Settings.GetSetting("SharedSource.BucketCounter.AllItems");

            var searchUtility = new SearchUtility();

            if (!item.Paths.FullPath.ToLower().StartsWith("/sitecore/content/")) return string.Empty;

            if (item.Fields["__Is Bucket"] != null && item.Fields["__Is Bucket"].Value.Equals("1"))
            {
                return $"({searchUtility.FindBucketableItems(item)})";
            }

            return counterAllItems.Equals("1") ? $"({item.Children.Count})" : string.Empty;
        }
    }
}