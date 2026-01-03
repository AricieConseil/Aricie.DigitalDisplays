using Aricie.DigitalDisplays.Components.Entities;
using Aricie.DigitalDisplays.Components.Settings;
using Aricie.DigitalDisplays.Controller;
using Aricie.DNN.UI.Controls;
using System;

namespace Aricie.DigitalDisplays
{
    public partial class Settings : AriciePortalModuleBase
    {
        public ADSettings AdSettings
        {
            get
            {
                ADSettings settings = BusinessController.Instance.GetSettings(ModuleId);
                if (settings == null)
                {
                    settings = new ADSettings();
                }
                return settings;
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            this.pEditor.LocalResourceFile = this.SharedResourceFile;

            this.pEditor.SetSessionDataSource(new Lazy<ADSettings>(() => AdSettings));
            this.pEditor.DataBind();

            if (((ADSettings)this.pEditor.DataSource).FontAwesome)
            {
                addFontAwesomeInclusion();
            }
        }

        private void addFontAwesomeInclusion()
        {
            var fontAwesomeUrl = "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.2/css/all.min.css";
            var key = "IncludeFontAwesome";
            if (Page.Header.FindControl(key) == null)
            {
                var link = new System.Web.UI.HtmlControls.HtmlLink
                {
                    Href = fontAwesomeUrl,
                    ID = key
                };
                link.Attributes["rel"] = "stylesheet";
                Page.Header.Controls.Add(link);
            }
        }
    }
}