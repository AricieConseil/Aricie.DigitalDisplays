using Aricie.Collections;
using Aricie.DigitalDisplays.Components.Entities;
using Aricie.DNN.ComponentModel;
using Aricie.DNN.Settings;
using Aricie.DNN.UI.Attributes;
using Aricie.DNN.UI.Controls;
using Aricie.DNN.UI.WebControls;
using Aricie.DNN.UI.WebControls.EditControls;
using Aricie.Services;
//using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.UI.WebControls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Reflection;
using System.Xml.Serialization;
using static Aricie.DNN.Images;

namespace Aricie.DigitalDisplays.Components.Settings
{
    public class ADSettings
    {
        //protected INavigationManager navigationManager;

        private int displayMode = -1;
        private ObservableCollection<Counter> displays = new ObservableCollection<Counter>();

        //public ADSettings()
        //{
        //    navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        //}

        private bool fontAwesome = false;

        public enum Display
        {
            Counter,
            CountDown
        }

        private Display _displayMode = Display.Counter;
        [AutoPostBack]
        public Display NewDisplayMode
        {
            get
            {
                if (DisplaysModeSpecified)
                {
                    switch (_displayMode)
                    {
                        case Display.CountDown:
                            ShowCountDownSettings = true;
                            ShowCountersList = false;
                            break;
                        case Display.Counter:
                            ShowCountersList = true;
                            ShowCountDownSettings = false;
                            break;
                        default:
                            ShowCountersList = false;
                            ShowCountDownSettings = false;
                            break;
                    }
                }
                else
                {
                    if (DisplayMode != -1)
                    {
                        _displayMode = (Display)DisplayMode;
                        DisplaysModeSpecified = true;
                    }
                }

                return _displayMode;
            }
            set
            {
                _displayMode = value;
                displayMode = (int)value;
                DisplaysModeSpecified = true;

                switch (_displayMode)
                {
                    case Display.CountDown:
                        ShowCountDownSettings = true;
                        ShowCountersList = false;
                        break;
                    default:
                        ShowCountersList = true;
                        ShowCountDownSettings = false;
                        break;
                }
                RaisePropertyChanged();
            }

        }

        public bool FontAwesome
        {
            get
            {
                return fontAwesome;
            }
            set
            {
                if (fontAwesome != value)
                {
                    fontAwesome = value;
                    RaisePropertyChanged();
                }
            }
        }



        [Browsable(false)]
        public int DisplayMode
        {
            get
            {
                return displayMode;
            }
            set
            {
                if (displayMode == -1 && value != -1)
                {
                    displayMode = value;
                }
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        [Browsable(false)]
        public bool DisplaysModeSpecified { get; set; } = false;

        [XmlIgnore]
        [JsonIgnore]
        [Browsable(false)]
        public bool ShowCountersList { get; set; } = false;

        [XmlIgnore]
        [JsonIgnore]
        [Browsable(false)]
        public bool ShowCountDownSettings { get; set; } = false;

        //[ExtendedCategory("", "WorkExperience", ExtendedCategoryFeatures.UseTemplate /*| ExtendedCategoryFeatures.InsertUpdatePanel*/)]
        //[XmlArrayItem("WorkExperience", IsNullable = false)]
        //[JsonProperty("WorkExperience", Required = Required.DisallowNull, NullValueHandling = NullValueHandling.Ignore)]
        [ConditionalVisible(nameof(ShowCountersList))]
        [CollectionEditor(Features = CollectionFeature.Default & ~CollectionFeature.Insert)]
        public ObservableCollection<Counter> Displays
        {
            get { return displays; }
            set
            {
                if (displays != value)
                {
                    displays = value;
                    RaisePropertyChanged();
                }
            }
        }
        [ConditionalVisible(nameof(ShowCountDownSettings))]
        [Editor(typeof(AricieDateEditControl), typeof(EditControl))]
        public DateTime EditDate
        {
            get; set;
        }

        [ActionButton(IconName.Undo, IconOptions.Normal, "CancelSettings.Warning", Features = ActionButtonFeatures.CloseSection | ActionButtonFeatures.CloseListItem | ActionButtonFeatures.SkipValidation)]
        public virtual void Cancel(AriciePropertyEditorControl pe)
        {
            //navigationManager = pe.ParentModule.DependencyProvider.GetRequiredService<INavigationManager>();
            pe.Page.Response.Redirect(Globals.NavigateURL());
        }

        [ActionButton(IconName.FloppyO, IconOptions.Normal, Features = ActionButtonFeatures.CloseSection | ActionButtonFeatures.CloseListItem)]
        public virtual void Save(AriciePropertyEditorControl pe)
        {
            bool stopSaving = false;
            try
            {
                DoSave(pe);
            }
            catch (Exception ex)
            {
                stopSaving = true;
                pe.DisplayMessage($"{ex.Message}", DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.RedError);
            }
            if (!stopSaving)
            {
                pe.Page.Response.Redirect(Globals.NavigateURL());
                //pe.Page.Response.Redirect(navigationManager.NavigateURL());
            }
        }

        public bool DoSave(AriciePropertyEditorControl pe)
        {
            //var key = E2CSettings.Instance.GetSmartFileKey(this.PortalId, this.UserId);
            //UserInfo currentUser = UserController.Instance.GetUser(this.PortalId, this.UserId);
            var currentPe = pe;
            currentPe.ItemChanged = true;

            var currentSettings = (ADSettings)currentPe.DataSource;
            int i = 0;
            foreach (Counter display in currentSettings.Displays)
            {
                if (!string.IsNullOrEmpty(display.condition))
                {
                    KeyValuePair<bool, string> validationResult = Controller.BusinessController.Instance.ValidateQueryCriterias(display.condition);
                    if (!validationResult.Key)
                    {
                        throw new Exception($"{validationResult.Value} pour l'affichage n°{i} !");
                    }
                }
                display.index = i;
                i++;
            }
            //if (!string.IsNullOrEmpty(currentCounter.Current.Current.Salarie.Entreprise.Principal?.Presentation?.Name))
            //{
            //XmlSerializer xmls = new XmlSerializer(typeof(Counter));
            //SettingsController.UpdateSettings(SettingsScope.ModuleSettings, pe.ParentModule.ModuleId, key, xmls.Serialize(currentCounter));
            SettingsController.UpdateSettings(SettingsScope.ModuleSettings, pe.ParentModule.ModuleId, Controller.BusinessController.AricieDisplayKey, ReflectionHelper.Serialize(currentSettings).OuterXml);
            //}

            return true;
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}