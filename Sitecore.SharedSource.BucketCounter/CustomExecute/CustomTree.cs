using System.IO;
using System.Web;
using System.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.BucketCounter.Utilities;
using Sitecore.Shell.Applications.ContentManager.Sidebars;
using Sitecore.Text;

namespace Sitecore.SharedSource.BucketCounter.CustomExecute
{
    public class CustomTree : Tree
    {
        private void RenderTreeNode(HtmlTextWriter output, Item item, string inner, bool active)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(inner, "inner");
            var counterUtility = new CounterUtility();
            var str = item.ID.ToShortID().ToString();
            output.Write("<div class=\"scContentTreeNode\">");
            RenderTreeNodeGlyph(output, str, inner, item);
            RenderGutter(output, item);
            var nodeId = GetNodeID(str);
            var className = GetClassName(item, active);
            output.Write("<a hidefocus=\"true\" id=\"");
            output.Write(nodeId);
            output.Write("\" href=\"#\" class=\"" + className + "\"");
            if (!string.IsNullOrEmpty(item.Help.Text))
            {
                output.Write("title=\"");
                output.Write(StringUtil.EscapeQuote(item.Help.Text));
                output.Write("\"");
            }
            output.Write(">");
            var style = GetStyle(item);
            output.Write("<span");
            output.Write(style);
            output.Write('>');
            RenderTreeNodeIcon(output, item);
            output.Write(item.Appearance.DisplayName);
            output.Write(" " + counterUtility.GetChildrenCounter(item) + " </span>");
            output.Write("</a>");
            if (inner.Length > 0)
            {
                output.Write("<div>");
                output.Write(inner);
                output.Write("</div>");
            }
            output.Write("</div>");
        }

        private static string GetStyle(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            if (item.TemplateID == TemplateIDs.TemplateField)
                return string.Empty;
            var str = item.Appearance.Style;
            if (string.IsNullOrEmpty(str) &&
                (item.Appearance.Hidden || item.RuntimeSettings.IsVirtual || item.IsItemClone))
                str = "color:#666666";
            if (!string.IsNullOrEmpty(str))
                str = " style=\"" + str + "\"";
            return str;
        }

        private string GetNodeID(string shortID)
        {
            Assert.ArgumentNotNullOrEmpty(shortID, "shortID");
            return ID + "_Node_" + shortID;
        }

        private static void RenderTreeNodeIcon(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(item, "item");
            output.Write(RenderIcon(item));
        }

        private static string GetClassName(Item item, bool active)
        {
            return !active
                ? (!IsItemUIStatic(item) ? "scContentTreeNodeNormal" : "scContentTreeNodeStatic")
                : "scContentTreeNodeActive";
        }

        private static bool IsItemUIStatic(Item item)
        {
            return item[FieldIDs.UIStaticItem] == "1";
        }

        private static string RenderIcon(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            var urlBuilder = new UrlBuilder(item.Appearance.Icon);
            if (item.Paths.IsMediaItem)
            {
                urlBuilder["rev"] = item.Statistics.Revision;
                urlBuilder["la"] = item.Language.ToString();
            }
            var imageBuilder = new ImageBuilder
            {
                Src = HttpUtility.HtmlDecode(urlBuilder.ToString()),
                Width = 16,
                Height = 16,
                Class = "scContentTreeNodeIcon"
            };
            if (!string.IsNullOrEmpty(item.Help.Text))
                imageBuilder.Title = item.Help.Text;
            return imageBuilder.ToString();
        }

        private void RenderTreeNodeGlyph(HtmlTextWriter output, string id, string inner, Item item)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNullOrEmpty(id, "id");
            Assert.ArgumentNotNull(inner, "inner");
            Assert.ArgumentNotNull(item, "item");
            var imageBuilder = new ImageBuilder();
            if (inner.Length > 0)
            {
                imageBuilder.Src = "images/treemenu_expanded.png";
            }
            else
            {
                bool flag;
                if (!Settings.ContentEditor.CheckHasChildrenOnTreeNodes)
                {
                    flag = true;
                }
                else
                {
                    var securityCheck = Settings.ContentEditor.CheckSecurityOnTreeNodes
                        ? SecurityCheck.Enable
                        : SecurityCheck.Disable;
                    flag = ItemManager.HasChildren(item, securityCheck);
                }
                imageBuilder.Src = flag ? "images/treemenu_collapsed.png" : "images/noexpand15x15.gif";
            }
            imageBuilder.Class = "scContentTreeNodeGlyph";
            imageBuilder.ID = ID + "_Glyph_" + id;
            output.Write(imageBuilder.ToString());
        }

        public override string RenderChildNodes(ID parent)
        {
            var currentItem = FolderItem.Database.GetItem(parent, FolderItem.Language);

            var output = new HtmlTextWriter(new StringWriter());

            if (currentItem != null)
            {
                foreach (Item filterChild in FilterChildren(currentItem))
                {
                    RenderTreeNode(output, filterChild, string.Empty, filterChild.ID == FolderItem.ID);
                }
            }

            return output.InnerWriter.ToString();
        }
    }
}