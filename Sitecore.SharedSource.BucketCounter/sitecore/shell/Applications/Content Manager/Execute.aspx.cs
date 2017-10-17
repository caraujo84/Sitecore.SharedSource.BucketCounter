using System;
using System.Collections.Specialized;
using ComponentArt.Web.UI;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Treeviews;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Reflection;
using Sitecore.SharedSource.BucketCounter.CustomExecute;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls.Ribbons;
using Page = System.Web.UI.Page;

namespace Sitecore.SharedSource.BucketCounter.sitecore.shell.Applications.Content_Manager
{
    public partial class Execute : Page
    {
        private void Page_Load(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            if (!ShellPage.IsLoggedIn())
                return;
            var s = string.Empty;
            switch (WebUtil.GetQueryString("cmd").ToLowerInvariant())
            {
                case "gettreeviewchildren":
                    s = GetTreeViewChildren();
                    break;
                case "expandtreeviewtonode":
                    s = ExpandTreeViewToNode();
                    break;
                case "getcontextualtabs":
                    s = GetContextualTabs();
                    break;
                case "convert":
                    s = DesignTimeConvert();
                    break;
                case "treeviewcontent":
                    s = GetTreeViewContent();
                    break;
            }
            Response.Write(s);
        }

        /// <summary>Expands the tree view to node.</summary>
        /// <returns>The tree view to node.</returns>
        private static string ExpandTreeViewToNode()
        {
            var str1 = WebUtil.GetQueryString("root");
            var str2 = WebUtil.GetQueryString("id");
            var queryString = WebUtil.GetQueryString("la");
            if (str2.IndexOf('_') >= 0)
                str2 = StringUtil.Mid(str2, str2.LastIndexOf('_') + 1);
            if (str1.IndexOf('_') >= 0)
                str1 = StringUtil.Mid(str1, str1.LastIndexOf('_') + 1);
            if (str2.Length > 0 && str1.Length > 0)
            {
                var language = Language.Parse(queryString);
                var folder = Client.ContentDatabase.GetItem(ShortID.DecodeID(str2), language);
                var root = Client.ContentDatabase.GetItem(ShortID.DecodeID(str1), language);
                if (folder != null && root != null)
                    return GetTree(folder, root).RenderTree(false);
            }
            return string.Empty;
        }

        /// <summary>Gets the tree view children.</summary>
        /// <returns>The get tree view children.</returns>
        /// <contract>
        ///     <ensures condition="not null" />
        /// </contract>
        private static string GetTreeViewChildren()
        {
            var queryString1 = WebUtil.GetQueryString("id");
            var queryString2 = WebUtil.GetQueryString("la");
            if (string.IsNullOrEmpty(queryString1))
                return string.Empty;
            var language = Language.Parse(queryString2);
            var folder = Client.ContentDatabase.GetItem(ShortID.DecodeID(queryString1), language);
            if (folder == null)
                return string.Empty;
            var rootItem = folder.Database.GetRootItem(language);
            if (rootItem == null)
                return string.Empty;
            return GetTree(folder, rootItem).RenderChildNodes(folder.ID);
        }

        /// <summary>The get tree.</summary>
        /// <param name="folder">The folder.</param>
        /// <param name="root">The root.</param>
        /// <returns>
        ///     The <see cref="T:Sitecore.Shell.Applications.ContentManager.Sidebars.Tree" />.
        /// </returns>
        private static CustomTree GetTree(Item folder, Item root)
        {
            Assert.IsNotNull(folder, "folder is null");
            Assert.IsNotNull(root, "root is null");
            var tree = new CustomTree();
            tree.ID = WebUtil.GetSafeQueryString("treeid");
            tree.FolderItem = folder;
            tree.RootItem = root;
            tree.DataContext = new DataContext
            {
                DataViewName = "Master"
            };
            return tree;
        }

        /// <summary>Converts HTML.</summary>
        /// <returns>The time convert.</returns>
        private string DesignTimeConvert()
        {
            var queryString = WebUtil.GetQueryString("mode");
            var body = StringUtil.GetString(Request.Form["html"]);
            string str;
            if (queryString == "HTML")
            {
                str = RuntimeHtml.Convert(body, Settings.HtmlEditor.SupportWebControls);
            }
            else
            {
                var querystring = new NameValueCollection(1)
                {
                    {
                        "sc_live",
                        "0"
                    }
                };
                var controlName = string.Empty;
                if (Settings.HtmlEditor.SupportWebControls)
                    controlName = "control:IDEHtmlEditorControl";
                str = DesignTimeHtml.Convert(body, controlName, querystring);
            }
            return "<?xml:namespace prefix = sc />" + str;
        }

        /// <summary>Gets the contextual tabs.</summary>
        /// <returns>The get contextual tabs.</returns>
        private string GetContextualTabs()
        {
            var queryString = WebUtil.GetQueryString("parameters");
            if (string.IsNullOrEmpty(queryString) || queryString.IndexOf("&fld=", StringComparison.InvariantCulture) < 0)
                return string.Empty;
            return GetFieldContextualTab(queryString);
        }

        /// <summary>Gets the field contextual tab.</summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The field contextual tab.</returns>
        private string GetFieldContextualTab(string parameters)
        {
            Assert.ArgumentNotNull(parameters, "parameters");
            var num = parameters.IndexOf("&fld=", StringComparison.InvariantCulture);
            var uri = ItemUri.Parse(StringUtil.Left(parameters, num));
            if (uri == null)
                return string.Empty;
            var obj = Database.GetItem(uri);
            if (obj == null)
                return string.Empty;
            var urlParameters = WebUtil.ParseUrlParameters(StringUtil.Mid(parameters, num));
            var index = StringUtil.GetString(urlParameters["fld"]);
            var str = StringUtil.GetString(urlParameters["ctl"]);
            var field = obj.Fields[index];
            if (field == null)
                return string.Empty;
            var templateField = field.GetTemplateField();
            if (templateField == null)
                return string.Empty;
            var fieldType = StringUtil.GetString(templateField.TypeKey, "text");
            var fieldTypeItem = FieldTypeManager.GetFieldTypeItem(fieldType);
            if (fieldTypeItem == null)
                return string.Empty;
            //Database database = Context.Database;
            var database = Sitecore.Context.Database;
            if (database == null)
                return string.Empty;
            Item child;
            if (fieldType == "rich text")
            {
                var queryString = WebUtil.GetQueryString("mo", "Editor");
                var path =
                    StringUtil.GetString(templateField.Source,
                        queryString == "IDE"
                            ? "/sitecore/system/Settings/Html Editor Profiles/Rich Text IDE"
                            : Settings.HtmlEditor.DefaultProfile) + "/Ribbon";
                child = database.GetItem(path);
            }
            else
            {
                child = fieldTypeItem.Children["Ribbon"];
            }
            if (child == null)
                return string.Empty;
            var ribbon1 = new Ribbon();
            ribbon1.ID = "Ribbon";
            var ribbon2 = ribbon1;
            var commandContext = new CommandContext(obj);
            ribbon2.CommandContext = commandContext;
            commandContext.Parameters["FieldID"] = index;
            commandContext.Parameters["ControlID"] = str;
            string navigator;
            string strips;
            ribbon2.Render(child, true, out navigator, out strips);
            Response.Write("{ \"navigator\": " + StringUtil.EscapeJavascriptString(navigator) + ", \"strips\": " +
                           StringUtil.EscapeJavascriptString(strips) + " }");
            return string.Empty;
        }

        /// <summary>
        ///     Gets the content of the <c>treeview</c>.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.String" />.
        /// </returns>
        private string GetTreeViewContent()
        {
            Response.ContentType = "text/xml";
            var queryString = ItemUri.ParseQueryString();
            if (queryString == null)
                return string.Empty;
            var parent = Database.GetItem(queryString);
            if (parent == null)
                return string.Empty;
            var typeInfo = ReflectionUtil.GetTypeInfo(WebUtil.GetQueryString("typ"));
            if (typeInfo == null)
                return string.Empty;
            var treeviewSource = ReflectionUtil.CreateObject(typeInfo) as TreeviewSource;
            if (treeviewSource == null)
                return string.Empty;
            var treeview = new TreeView();
            treeviewSource.Render(treeview, parent);
            return treeview.GetXml();
        }
    }
}