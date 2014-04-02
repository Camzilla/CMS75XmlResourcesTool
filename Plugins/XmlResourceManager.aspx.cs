namespace Piab.CMS.Business.Plugins.Common
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.UI.WebControls;
    using System.Xml;

    using EPiServer;
    using EPiServer.DataAbstraction;
    using EPiServer.PlugIn;
    using EPiServer.ServiceLocation;
    using EPiServer.UI.WebControls;

    [GuiPlugIn(
        DisplayName = "Xml resource manager",
        Area = PlugInArea.AdminMenu,
        Url = "~/Business/Plugins/Common/XmlResourceManager.aspx")]
    public partial class XmlResourceManager : EPiServer.Shell.WebForms.WebFormsBase 
    {
        #region vars

        private string xmlfilename = "/Resources/LanguageFiles/{0}_{1}.xml";
        private string viewsxmlfilename = "/Resources/LanguageFiles/Views{0}.xml";
        private const string Xmlfilenametemplate = "{0}_{1}.xml";
        private const string Viewsxmlfilenametemplate = "Views{0}.xml";
        private const string Viewsfilenamepostfix = "_";

        #endregion

        #region Inner classes

        protected class ViewResultItem
        {
            public string ContainingElementNameForDisplay { get; set; }
            public string ContainingElementName { get; set; }
            public string ElementName { get; set; }
            public string MasterElementValue { get; set; }
            public string ElementValue { get; set; }
            public string XPath { get; set; }
        }

        #endregion

        #region Overrides

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.MasterPageFile = UriSupport.ResolveUrlFromUIBySettings("MasterPages/EPiServerUI.master");
            this.SystemMessageContainer.Heading = "Translation for Piab.com";
            this.SystemMessageContainer.Description = "Select your language and start translating! Empty fields will be marked yellow.";
         
        }

        protected override void OnInit(EventArgs e)
        {
            this.DdlSelectLanguage.SelectedIndexChanged += this.Refresh;
            this.ViewsControl.RowDataBound += ViewsControlRowDataBound;
            base.OnInit(e);
        }

        static void ViewsControlRowDataBound(object sender, GridViewRowEventArgs e)
        {
            var textbox = (TextBox)e.Row.Cells[4].FindControl("ElementValue");
            var item = e.Row.DataItem as ViewResultItem;
            if (item != null && string.IsNullOrWhiteSpace(item.ElementValue))
            {
                textbox.BackColor = Color.LightGoldenrodYellow;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var path = WebConfigurationManager.AppSettings.Get("resourcemanagerpath");
            if (string.IsNullOrEmpty(path))
            {
                path = "/Resources/LanguageFiles/";
            }

            this.xmlfilename = path + Xmlfilenametemplate;
            this.viewsxmlfilename = path + Viewsxmlfilenametemplate;

            var repository = ServiceLocator.Current.GetInstance<ILanguageBranchRepository>();

            if (this.IsPostBack)
            {
                return;
            }

            this.DdlSelectLanguage.DataSource = repository.ListEnabled();
            this.DdlSelectLanguage.DataTextField = "Name";
            this.DdlSelectLanguage.DataValueField = "LanguageID";
            this.DdlSelectLanguage.DataBind();

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.DdlSelectLanguage.SelectedValue);

            this.Inits();
        }

        #endregion

        #region Eventshandlers

        protected void CreateXml(object sender, EventArgs e)
        {
            this.CreateLangXmlFiles(((ToolButton)sender).CommandName);
            Thread.Sleep(2000);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.DdlSelectLanguage.SelectedValue);
            this.Inits();

        }

        private void Refresh(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.DdlSelectLanguage.SelectedValue);
            this.Inits();
        }

        #endregion

        #region Methods

        private void Inits()
        {
            var lang = this.DdlSelectLanguage.SelectedValue;

            var masterLangElements = this.GetViewElements("en-GB");

            var currentLangElements = this.GetViewElements(lang);

            foreach (var masterLangItem in masterLangElements)
            {
                var currentLangItem = currentLangElements.FirstOrDefault(i => i.XPath.Equals(masterLangItem.XPath));

                masterLangItem.MasterElementValue = masterLangItem.ElementValue;
                masterLangItem.ElementValue = currentLangItem != null ? currentLangItem.ElementValue : string.Empty;
            }

            this.UntranslatedFrases = masterLangElements.Count - currentLangElements.Count;
            this.ViewsControl.DataSource = masterLangElements;
            this.ViewsControl.DataBind();
        }

        private void CreateLangXmlFiles(string type)
        {
            var typeUpperCase = type;

            var doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(docNode);

            XmlNode languagesNode = doc.CreateElement("languages");
            doc.AppendChild(languagesNode);

            XmlNode languageNode = doc.CreateElement("language");
            var name = doc.CreateAttribute("name");
            name.Value = this.DdlSelectLanguage.SelectedItem.Text;

            var id = doc.CreateAttribute("id");
            id.Value = this.DdlSelectLanguage.SelectedItem.Value;

            if (languageNode.Attributes != null)
            {
                languageNode.Attributes.Append(name);
                languageNode.Attributes.Append(id);
            }

            languagesNode.AppendChild(languageNode);

            type = type.ToLower();
            
            this.AddViewsTranslations(doc);

            doc.Save(type == "views" ? this.Server.MapPath(string.Format(this.viewsxmlfilename, Viewsfilenamepostfix + this.DdlSelectLanguage.SelectedItem.Value)) 
                : this.Server.MapPath(string.Format(this.xmlfilename, typeUpperCase, this.DdlSelectLanguage.SelectedItem.Value)));
        }

        private XmlNode CreateXpathIfRequired(XmlDocument doc, string xpath, string tagName)
        {

                var element = doc.SelectSingleNode(xpath);
                if (element != null) return element;

                var lastIndex = xpath.LastIndexOf('/');
                var shorterPath = xpath.Substring(0, lastIndex);
                var newLastIndex = shorterPath.LastIndexOf('/');
            var newTagName = shorterPath.Substring(newLastIndex + 1);

                    var parentNode = doc.SelectSingleNode(shorterPath) ?? this.CreateXpathIfRequired(doc, shorterPath, newTagName);
                    var newNode = doc.CreateElement(tagName);
                    parentNode.AppendChild(newNode);
                    return newNode;
        }

        private void AddViewsTranslations(XmlDocument doc)
        {
            foreach (GridViewRow item in this.ViewsControl.Rows)
            {
                var newTranslation = ((TextBox)item.FindControl("ElementValue")).Text;
                var xpath = "//language/" + ((Label)item.FindControl("XPath")).Text;

                var element = ((Label)item.FindControl("LblElement")).Text;

                if (!string.IsNullOrWhiteSpace(newTranslation))
                {

                    var node = this.CreateXpathIfRequired(doc, xpath, element);
                    node.AppendChild(doc.CreateTextNode(newTranslation));
                }
                else
                {
                    continue;
                }
             }
        }

        private static readonly string[] UninterestingNames = { "languages", "language" };

        static string FindXPath(XmlNode node)
        {
            node = node.ParentNode;
            var builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        if (UninterestingNames.Contains(node.Name))
                            return builder.ToString().Substring(1);

                        builder.Insert(0, "/" + node.Name);
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString().Substring(1);
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        private void AddElementsRecursive(XmlNode parent, ICollection<ViewResultItem> collection, string containingElementNameForDisplay = null)
        {
            var x = 0;

            var elements = parent.ChildNodes.OfType<XmlElement>().ToArray();
            if (!elements.Any())
            {
                if (parent.ParentNode != null)
                {
                    collection.Add(new ViewResultItem
                                   {
                                       ContainingElementNameForDisplay = containingElementNameForDisplay,
                                       ContainingElementName = parent.ParentNode.Name,
                                       ElementName = parent.Name,
                                       ElementValue = parent.InnerText,
                                       XPath = FindXPath(parent) + "/" + parent.Name
                                   });
                }
                return;
            }

            foreach (var element in elements)
            {
                x++;
                this.AddElementsRecursive(element, collection, (x == 1 ? FindXPath(element) : null));
            }
        }

        private List<ViewResultItem> GetViewElements(string lang)
        {
            var result = new List<ViewResultItem>();
            var viewelements = new List<string>();
            const string Xpathexpression = "languages/language";
            var doc = new XmlDocument();

            var suffix = this.Server.MapPath(string.Format(this.viewsxmlfilename, Viewsfilenamepostfix + lang));

            if (File.Exists(suffix))
            {
                doc.Load(suffix);
            }
            else
            {
                return result;
            }

            var xmlNodeList = doc.SelectNodes(Xpathexpression + "/*");

            if (xmlNodeList != null)
            {
                foreach (XmlElement rootelement in xmlNodeList)
                {
                    viewelements.Add(rootelement.Name);
                }
            }

            foreach (var relement in viewelements)
            {
                var nodes = doc.SelectNodes(Xpathexpression + "/" + relement + "/*");
                if (nodes != null && nodes.Count == 0)
                {
                    result.Add(new ViewResultItem { ContainingElementNameForDisplay = relement });
                }
                else
                {
                    var x = 0;

                    if (nodes == null)
                    {
                        continue;
                    }

                    var nodesWithoutElementChildren =
                        nodes.Cast<XmlElement>().Where(e => !e.ChildNodes.OfType<XmlElement>().Any());
                    var otherNodes = nodes.Cast<XmlElement>().Where(e => e.ChildNodes.OfType<XmlElement>().Any());

                    foreach (var element in nodesWithoutElementChildren)
                    {
                        x++;
                        this.AddElementsRecursive(element, result, (x == 1 ? relement : null));
                    }

                    foreach (var element in otherNodes)
                    {
                        x++;
                        this.AddElementsRecursive(element, result, (x == 1 ? relement : null));
                    }
                }
            }
            return result;
        }

        #endregion

        #region GetDataItems

        protected ViewResultItem Element { get { return this.Page.GetDataItem() as ViewResultItem; } }

        protected int UntranslatedFrases { get; set; }

        #endregion

    }
}
