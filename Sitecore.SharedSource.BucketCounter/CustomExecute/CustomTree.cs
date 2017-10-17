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

        /// <summary>Renders the tree node.</summary>
        /// <param name="output">The output.</param>
        /// <param name="item">The item.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="active">if set to <c>true</c> [active].</param>
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
            output.Write(" <span style='color:#AAAAAA;font-style:italic;'>" + counterUtility.GetChildrenCounter(item) + "</span> </span>");
            output.Write("</a>");
            if (inner.Length > 0)
            {
                output.Write("<div>");
                output.Write(inner);
                output.Write("</div>");
            }
            output.Write("</div>");
        }

        /// <summary>Gets the style.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The style.</returns>
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

        /// <summary>Gets the node ID.</summary>
        /// <param name="shortID">The short ID.</param>
        /// <returns></returns>
        /// <contract>
        ///   <requires name="shortID" condition="not empty" />
        ///   <ensures condition="nullable" />
        /// </contract>
        private string GetNodeID(string shortID)
        {
            Assert.ArgumentNotNullOrEmpty(shortID, "shortID");
            return ID + "_Node_" + shortID;
        }

        /// <summary>Renders the tree node icon.</summary>
        /// <param name="output">The output.</param>
        /// <param name="item">The item.</param>
        private static void RenderTreeNodeIcon(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(item, "item");
            output.Write(RenderIcon(item));
        }

        /// <summary>Gets the name of the CSS class for the item.</summary>
        /// <param name="item">The item.</param>
        /// <param name="active">if set to <c>true</c> the item should be shown active</param>
        /// <returns>Class name</returns>
        private static string GetClassName(Item item, bool active)
        {
            return !active
                ? (!IsItemUIStatic(item) ? "scContentTreeNodeNormal" : "scContentTreeNodeStatic")
                : "scContentTreeNodeActive";
        }

        /// <summary>Determines whether the item is UI static.</summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if the item is UI static; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsItemUIStatic(Item item)
        {
            return item[FieldIDs.UIStaticItem] == "1";
        }

        /// <summary>Renders the icon.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The icon.</returns>
        /// <contract>
        ///   <requires name="item" condition="not null" />
        ///   <ensures condition="not null" />
        /// </contract>
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

        /// <summary>Renders the tree node glyph.</summary>
        /// <param name="output">The output.</param>
        /// <param name="id">The id.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="item">The item.</param>
        /// <contract>
        ///   <requires name="output" condition="not null" />
        ///   <requires name="id" condition="not empty" />
        ///   <requires name="inner" condition="not null" />
        ///   <requires name="item" condition="not null" />
        /// </contract>
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

        /// <summary>Renders the child nodes.</summary>
        /// <param name="parent">The selected.</param>
        /// <returns>The child nodes.</returns>
        /// <contract>
        /// 	<requires name="parent" condition="not null" />
        /// 	<ensures condition="not null" />
        /// </contract>
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