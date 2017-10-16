using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Shell.Applications.ContentManager.Sidebars;
using Sitecore.Web.UI.HtmlControls;

namespace Sitecore.SharedSource.BucketCounter.CustomExecute
{
    public class CustomContentEditor : ContentEditorForm
    {
        protected override Sidebar GetSidebar()
        {
            var result = new CustomTree
            {
                ID = "Tree",
                DataContext = new DataContext
                {
                    DataViewName = "Master"
                }
            };

            return Assert.ResultNotNull(result);
        }
    }
}