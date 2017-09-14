namespace RolesMultilist
{
    using Sitecore;
    using Sitecore.Configuration;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Resources;
    using Sitecore.Security.Accounts;
    using Sitecore.Security.Domains;
    using Sitecore.SecurityModel;
    using Sitecore.Shell.Applications.ContentEditor;
    using System;
    using System.Linq;
    using System.Web.UI;

    class RolesMultilistField : Sitecore.Web.UI.HtmlControls.Control, IContentField
    {
        private string _source;

        public string Source
        {
            get
            {
                return this._source;
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                this._source = value;
            }
        }

        public RolesMultilistField()
        {
            this.Class = "scContentControlMultilist";
            base.Activation = true;
            this._source = string.Empty;
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            string text = Sitecore.Context.ClientPage.ClientRequest.Form[this.ID + "_value"];
            if (text != null)
            {
                if (base.GetViewStateString("Value", string.Empty) != text)
                {
                    Sitecore.Context.ClientPage.Modified = true;
                }
                base.SetViewStateString("Value", text);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, "output");

            base.ServerProperties["ID"] = this.ID;

            Domain currentDomain;
            var defaultDomainName = Settings.GetSetting("RolesMultilist.DefaultDomain");
            var defaultDomain = DomainManager.GetDomain(defaultDomainName);
            if (!string.IsNullOrEmpty(this.Source))
            {
                currentDomain = DomainManager.GetDomain(this.Source) ?? defaultDomain;
            }
            else
            {
                currentDomain = defaultDomain;
            }

            var rolesInDomain = currentDomain.GetRoles();
            var rawSelectedRoles = this.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var selectedRoles = rawSelectedRoles.Select(r => Role.FromName(r));

            string text = string.Empty;
            output.Write(string.Concat(new string[]
            {
                "<input id=\"",
                this.ID,
                "_Value\" type=\"hidden\" value=\"",
                StringUtil.EscapeQuote(this.Value),
                "\" />"
            }));
            output.Write("<div class='scContentControlMultilistContainer'>");
            output.Write("<table" + this.GetControlAttributes() + ">");
            output.Write("<tr>");
            output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("All") + "</td>");
            output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");
            output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("Selected") + "</td>");
            output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");
            output.Write("</tr>");
            output.Write("<tr>");
            output.Write("<td valign=\"top\" height=\"100%\">");
            output.Write(string.Concat(new string[]
            {
                "<select id=\"",
                this.ID,
                "_unselected\" class=\"scContentControlMultilistBox\" multiple=\"multiple\" size=\"10\"",
                text,
                " ondblclick=\"javascript:scContent.multilistMoveRight('",
                this.ID,
                "')\" onchange=\"javascript:document.getElementById('",
                this.ID,
                "_all_help').innerHTML=this.selectedIndex>=0?this.options[this.selectedIndex].innerHTML:''\" >"
            }));

            // all except selected
            foreach (var role in rolesInDomain)
            {
                if (!selectedRoles.Contains(role))
                {
                    output.Write(string.Format("<option value=\"{0}\">{1}</option>", role.Name, role.Name));
                }
            }

            output.Write("</select>");
            output.Write("</td>");
            output.Write("<td valign=\"top\">");
            this.RenderButton(output, "Office/16x16/navigate_right.png", "javascript:scContent.multilistMoveRight('" + this.ID + "')");
            output.Write("<br />");
            this.RenderButton(output, "Office/16x16/navigate_left.png", "javascript:scContent.multilistMoveLeft('" + this.ID + "')");
            output.Write("</td>");
            output.Write("<td valign=\"top\" height=\"100%\">");
            output.Write(string.Concat(new string[]
            {
                "<select id=\"",
                this.ID,
                "_selected\" class=\"scContentControlMultilistBox\" multiple=\"multiple\" size=\"10\"",
                text,
                " ondblclick=\"javascript:scContent.multilistMoveLeft('",
                this.ID,
                "')\" onchange=\"javascript:document.getElementById('",
                this.ID,
                "_selected_help').innerHTML=this.selectedIndex>=0?this.options[this.selectedIndex].innerHTML:''\">"
            }));

            // selected
            foreach (var role in selectedRoles)
            {
                output.Write(string.Format("<option value=\"{0}\">{1}</option>", role.Name, role.Name));
            }

            output.Write("</select>");
            output.Write("</td>");
            output.Write("<td valign=\"top\">");
            this.RenderButton(output, "Office/16x16/navigate_up.png", "javascript:scContent.multilistMoveUp('" + this.ID + "')");
            output.Write("<br />");
            this.RenderButton(output, "Office/16x16/navigate_down.png", "javascript:scContent.multilistMoveDown('" + this.ID + "')");
            output.Write("</td>");
            output.Write("</tr>");
            output.Write("<tr>");
            output.Write("<td valign=\"top\">");
            output.Write("<div class=\"scContentControlMultilistHelp\" id=\"" + this.ID + "_all_help\"></div>");
            output.Write("</td>");
            output.Write("<td></td>");
            output.Write("<td valign=\"top\">");
            output.Write("<div class=\"scContentControlMultilistHelp\" id=\"" + this.ID + "_selected_help\"></div>");
            output.Write("</td>");
            output.Write("<td></td>");
            output.Write("</tr>");
            output.Write("</table>");
            output.Write("</div>");
        }

        public string GetValue()
        {
            return this.Value;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            this.Value = value;
        }

        private void RenderButton(HtmlTextWriter output, string icon, string click)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(icon, "icon");
            Assert.ArgumentNotNull(click, "click");
            ImageBuilder imageBuilder = new ImageBuilder();
            imageBuilder.Src = icon;
            imageBuilder.Class = "scNavButton";
            imageBuilder.Width = 16;
            imageBuilder.Height = 16;
            imageBuilder.Margin = "2px";
            imageBuilder.OnClick = click;

            output.Write(imageBuilder.ToString());
        }
    }
}