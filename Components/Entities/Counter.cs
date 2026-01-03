using Aricie.DNN.ComponentModel;
using Aricie.DNN.UI.Attributes;
using Aricie.DNN.UI.WebControls;
using Aricie.DNN.UI.WebControls.EditControls;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace Aricie.DigitalDisplays.Components.Entities
{
    public class Counter : ISelector
    {
        [Browsable(false)]
        public int index { get; set; } = 0;

        [Browsable(false)]
        public int value { get; set; } = 0;
        public bool approximativeValue { get; set; } = false;

        public string label { get; set; }
        public string icon { get; set; }

        private IconName _icon2 = IconName.None;

        [AutoPostBack]
        [Editor(typeof(SelectorEditControl), typeof(EditControl))]
        [Selector("Text", "Value", false, false, "", "", false, false)]
        public IconName icon2 {
            get { return _icon2; }
            set
            {
                if ((value != IconName.None && _icon2 == IconName.None) || (_icon2 != IconName.None && icon == ""))
                {
                    icon = (new IconActionControl()).GetCssClass((IconName)value, IconOptions.x5, false);
                }
                _icon2 = value;
            }
        }

        [ReadOnly(true)]
        [Editor(typeof(SelectorEditControl), typeof(EditControl))]
        [Selector("Text", "Value", false, false, "", "", false, false)]
        public string table { get; set; } = "";

        public string condition { get; set; }

        private ListItemCollection values = null;

        
        public IList GetSelector(string propertyName)
        {
            if (propertyName == nameof(table))
            {
                var tables = Aricie.DigitalDisplays.Controller.BusinessController.Instance.GetAllTables();
                var list = new ListItemCollection();
                foreach (string tableName in tables)
                {
                    list.Add(tableName);
                }
                return list;
            }

            if (values == null)
            {
                values = new ListItemCollection();
                foreach (IconName value in Enum.GetValues(typeof(IconName)))
                {
                    values.Add(new ListItem(Localization.GetString(value.ToString(), "~/DesktopModules/Aricie.DigitalDisplays/App_LocalResources/SharedResources.resx"), ((int)value).ToString()));
                }
            }
            return values;
        }
    }
}