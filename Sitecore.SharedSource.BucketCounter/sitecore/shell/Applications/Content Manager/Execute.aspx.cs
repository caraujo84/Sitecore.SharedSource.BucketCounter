using ComponentArt.Licensing.Providers;
using System.Collections.Specialized;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Templates;
using Sitecore.Data.Treeviews;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Reflection;
using Sitecore.SharedSource.BucketCounter.CustomExecute;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.ContentManager;
using Sitecore.Shell.Applications.Install.Controls;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls.Ribbons;

namespace Sitecore.SharedSource.BucketCounter.sitecore.shell.Applications.Content_Manager
{
    using System;
    using System.Web;
    using System.Web.UI;
    using Sitecore;
    public partial class Execute : System.Web.UI.Page
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull((object)e, "e");
            if (!ShellPage.IsLoggedIn())
                return;
            string s = string.Empty;
            switch (WebUtil.GetQueryString("cmd").ToLowerInvariant())
            {
                case "gettreeviewchildren":
                    s = Execute.GetTreeViewChildren();
                    break;
                case "expandtreeviewtonode":
                    s = Execute.ExpandTreeViewToNode();
                    break;
                case "getcontextualtabs":
                    s = this.GetContextualTabs();
                    break;
                case "convert":
                    s = this.DesignTimeConvert();
                    break;
                case "treeviewcontent":
                    s = this.GetTreeViewContent();
                    break;
            }
            this.Response.Write(s);
        }

        /// <summary>Expands the tree view to node.</summary>
        /// <returns>The tree view to node.</returns>
        private static string ExpandTreeViewToNode()
        {
            string str1 = WebUtil.GetQueryString("root");
            string str2 = WebUtil.GetQueryString("id");
            string queryString = WebUtil.GetQueryString("la");
            if (str2.IndexOf('_') >= 0)
                str2 = StringUtil.Mid(str2, str2.LastIndexOf('_') + 1);
            if (str1.IndexOf('_') >= 0)
                str1 = StringUtil.Mid(str1, str1.LastIndexOf('_') + 1);
            if (str2.Length > 0 && str1.Length > 0)
            {
                Language language = Language.Parse(queryString);
                Item folder = Sitecore.Client.ContentDatabase.GetItem(ShortID.DecodeID(str2), language);
                Item root = Sitecore.Client.ContentDatabase.GetItem(ShortID.DecodeID(str1), language);
                if (folder != null && root != null)
                    return Execute.GetTree(folder, root).RenderTree(false);
            }
            return string.Empty;
        }

        /// <summary>Gets the tree view children.</summary>
        /// <returns>The get tree view children.</returns>
        /// <contract>
        ///   <ensures condition="not null" />
        /// </contract>
        private static string GetTreeViewChildren()
        {
            string queryString1 = WebUtil.GetQueryString("id");
            string queryString2 = WebUtil.GetQueryString("la");
            if (string.IsNullOrEmpty(queryString1))
                return string.Empty;
            Language language = Language.Parse(queryString2);
            Item folder = Sitecore.Client.ContentDatabase.GetItem(ShortID.DecodeID(queryString1), language);
            if (folder == null)
                return string.Empty;
            Item rootItem = folder.Database.GetRootItem(language);
            if (rootItem == null)
                return string.Empty;
            return Execute.GetTree(folder, rootItem).RenderChildNodes(folder.ID);
        }

        /// <summary>The get tree.</summary>
        /// <param name="folder">The folder.</param>
        /// <param name="root">The root.</param>
        /// <returns>
        /// The <see cref="T:Sitecore.Shell.Applications.ContentManager.Sidebars.Tree" />.
        /// </returns>
        private static CustomTree GetTree(Item folder, Item root)
        {
            Assert.IsNotNull((object)folder, "folder is null");
            Assert.IsNotNull((object)root, "root is null");
            CustomTree tree = new CustomTree();
            tree.ID = WebUtil.GetSafeQueryString("treeid");
            tree.FolderItem = folder;
            tree.RootItem = root;
            tree.DataContext = new DataContext()
            {
                DataViewName = "Master"
            };
            return tree;
        }

        /// <summary>Converts HTML.</summary>
        /// <returns>The time convert.</returns>
        private string DesignTimeConvert()
        {
            string queryString = WebUtil.GetQueryString("mode");
            string body = StringUtil.GetString(new string[1]
            {
        this.Request.Form["html"]
            });
            string str;
            if (queryString == "HTML")
            {
                str = RuntimeHtml.Convert(body, Settings.HtmlEditor.SupportWebControls);
            }
            else
            {
                NameValueCollection querystring = new NameValueCollection(1)
        {
          {
            "sc_live",
            "0"
          }
        };
                string controlName = string.Empty;
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
            string queryString = WebUtil.GetQueryString("parameters");
            if (string.IsNullOrEmpty(queryString) || queryString.IndexOf("&fld=", StringComparison.InvariantCulture) < 0)
                return string.Empty;
            return this.GetFieldContextualTab(queryString);
        }

        /// <summary>Gets the field contextual tab.</summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The field contextual tab.</returns>
        private string GetFieldContextualTab(string parameters)
        {
            Assert.ArgumentNotNull((object)parameters, "parameters");
            int num = parameters.IndexOf("&fld=", StringComparison.InvariantCulture);
            ItemUri uri = ItemUri.Parse(StringUtil.Left(parameters, num));
            if (uri == (ItemUri)null)
                return string.Empty;
            Item obj = Database.GetItem(uri);
            if (obj == null)
                return string.Empty;
            NameValueCollection urlParameters = WebUtil.ParseUrlParameters(StringUtil.Mid(parameters, num));
            string index = StringUtil.GetString(new string[1]
            {
        urlParameters["fld"]
            });
            string str = StringUtil.GetString(new string[1]
            {
        urlParameters["ctl"]
            });
            Sitecore.Data.Fields.Field field = obj.Fields[index];
            if (field == null)
                return string.Empty;
            TemplateField templateField = field.GetTemplateField();
            if (templateField == null)
                return string.Empty;
            string fieldType = StringUtil.GetString(new string[2]
            {
        templateField.TypeKey,
        "text"
            });
            Item fieldTypeItem = FieldTypeManager.GetFieldTypeItem(fieldType);
            if (fieldTypeItem == null)
                return string.Empty;
            //Database database = Context.Database;
            Database database = Sitecore.Context.Database;
            if (database == null)
                return string.Empty;
            Item child;
            if (fieldType == "rich text")
            {
                string queryString = WebUtil.GetQueryString("mo", "Editor");
                string path = StringUtil.GetString(new string[2]
                {
          templateField.Source,
          queryString == "IDE" ? "/sitecore/system/Settings/Html Editor Profiles/Rich Text IDE" : Settings.HtmlEditor.DefaultProfile
                }) + "/Ribbon";
                child = database.GetItem(path);
            }
            else
                child = fieldTypeItem.Children["Ribbon"];
            if (child == null)
                return string.Empty;
            Ribbon ribbon1 = new Ribbon();
            ribbon1.ID = "Ribbon";
            Ribbon ribbon2 = ribbon1;
            CommandContext commandContext = new CommandContext(obj);
            ribbon2.CommandContext = commandContext;
            commandContext.Parameters["FieldID"] = index;
            commandContext.Parameters["ControlID"] = str;
            string navigator;
            string strips;
            ribbon2.Render(child, true, out navigator, out strips);
            this.Response.Write("{ \"navigator\": " + StringUtil.EscapeJavascriptString(navigator) + ", \"strips\": " + StringUtil.EscapeJavascriptString(strips) + " }");
            return string.Empty;
        }

        /// <summary>
        /// Gets the content of the <c>treeview</c>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.String" />.
        /// </returns>
        private string GetTreeViewContent()
        {
            this.Response.ContentType = "text/xml";
            ItemUri queryString = ItemUri.ParseQueryString();
            if (queryString == (ItemUri)null)
                return string.Empty;
            Item parent = Database.GetItem(queryString);
            if (parent == null)
                return string.Empty;
            Type typeInfo = ReflectionUtil.GetTypeInfo(WebUtil.GetQueryString("typ"));
            if (typeInfo == (Type)null)
                return string.Empty;
            TreeviewSource treeviewSource = ReflectionUtil.CreateObject(typeInfo) as TreeviewSource;
            if (treeviewSource == null)
                return string.Empty;
            var treeview = new ComponentArt.Web.UI.TreeView();
            treeviewSource.Render(treeview, parent);
            return treeview.GetXml();
        }
    }
}
