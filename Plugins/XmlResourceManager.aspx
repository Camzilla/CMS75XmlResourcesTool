<%@ Page Language="C#" EnableViewState="true" Codebehind="XmlResourceManager.aspx.cs" AutoEventWireup="False" Inherits="Piab.CMS.Business.Plugins.Common.XmlResourceManager" Title="XmlResourceManager" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Register Namespace="EPiServer.UI.WebControls" TagPrefix="EPiServerUI" assembly="EPiServer.UI" %>
<%@ Register TagPrefix="EPiServerScript" Namespace="EPiServer.ClientScript.WebControls" Assembly="EPiServer, Version=7.6.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7" %>

<asp:Content ContentPlaceHolderID="MainRegion" runat="server">
    <div class="epi-formArea">
        <div class="epi-buttonDefault epi-size25">
            <asp:DropDownList ID="DdlSelectLanguage" runat="server" AutoPostback="true" EnableViewState="true" />
        </div>
    </div>
    <div>
        <asp:Label runat="server" ID="Untranslated">Currently you have (<%= UntranslatedFrases.ToString(CultureInfo.InvariantCulture) %>) phrases to translate.</asp:Label>
    </div>
    <asp:Panel runat="server" ID="tabView" CssClass="epi-padding">
        <div class="epi-formArea" ID="Views" runat="server">
            <div class="epi-size25"> 
                <asp:GridView ID="ViewsControl" runat="server" AutoGenerateColumns="false" >
                <Columns>
                    <asp:TemplateField HeaderText="XPath" ItemStyle-Wrap="false" Visible="true">
                        <ItemTemplate>
			                <b><asp:Label id="XPath" Text="<%#Element.XPath%>" runat="server" /></b>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ElementContainer" ItemStyle-Wrap="false" Visible="false">
                        <ItemTemplate>
			                <b><asp:Label id="LblElementContainer" Text="<%#Element.ContainingElementName%>" runat="server" /></b>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Category" ItemStyle-Wrap="false">                
                        <ItemTemplate>
			                <b><asp:Label id="LblView" Text="<%#Element.ContainingElementNameForDisplay%>" runat="server" /></b>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Tag name" ItemStyle-Wrap="false" Visible="False">
                        <ItemTemplate>
                            <asp:Label id="LblElement" Text="<%#Element.ElementName%>" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Master language" ItemStyle-Wrap="false" Visible="true">
                        <ItemTemplate>
                            <asp:Label id="LblMasterLanguage" Text="<%#Element.MasterElementValue%>" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Translation" ItemStyle-Wrap="false">                
                        <ItemTemplate>
                            <asp:TextBox ID="ElementValue" Text='<%#Element.ElementValue%>' CssClass="EP-requiredField" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            </div>
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="Save" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="Views"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="Cancel" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent6" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>
        </div>
    </asp:Panel>
</asp:Content>

