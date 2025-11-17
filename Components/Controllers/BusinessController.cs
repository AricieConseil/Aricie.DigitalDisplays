using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using System.Collections.Generic;
using Aricie.DigitalDisplays.Components.Entities;
using Aricie.DNN.Settings;
using System.Collections;
using Aricie.Services;
using Aricie.DigitalDisplays.Components.Settings;
using System.Collections.ObjectModel;
using System;

namespace Aricie.DigitalDisplays.Controller
{
    /// ----------------------------------------------------------------------------- 
    /// <summary> 
    /// The Businesscontroller class for Angularmodule
    /// </summary> 
    /// ----------------------------------------------------------------------------- 
    //[DNNtc.BusinessControllerClass()]
    public class BusinessController : ICustomTokenProvider
    {
        public const string AricieDisplayKey = "Aricie.Displays";

        private static BusinessController _instance;

        public static BusinessController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BusinessController();
                }
                return _instance;
            }
        }
        
        public IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext)
        {
            var tokens = new Dictionary<string, IPropertyAccess>();
            tokens["moduleproperties"] = new ModulePropertiesPropertyAccess(moduleContext);
            return tokens;
        }


        public ObservableCollection<Counter> GetQuantities(int moduleID)
        {
            var db = new PetaPoco.Database("SiteSqlServer");

            ADSettings result = GetSettings(moduleID);

            if (result != null)
            {
                foreach (Counter counter in result.Displays)
                {
                    string query = getQueryFromSettings(counter).Key;
                    long count = db.ExecuteScalar<long>(query);
                    if (counter.approximativeValue)
                    {
                        count = RoundValue(count);
                    }
                    counter.value = (int)count;
                }
            }

            return result.Displays;
        }

        public ADSettings GetSettings(int moduleID)
        {
            Dictionary<string, string> settings = null;
            if (!string.IsNullOrEmpty(SettingsController.FetchFromModuleSettings(SettingsScope.ModuleSettings, moduleID, AricieDisplayKey, ref settings)))
            {
                ADSettings result = ReflectionHelper.Deserialize<ADSettings>(SettingsController.FetchFromModuleSettings(SettingsScope.ModuleSettings, moduleID, AricieDisplayKey, ref settings));
                return result;
            }
            return null;
        }

        private KeyValuePair<string, string> getQueryFromSettings(Counter counter)
        {
            string query = $"select Count(*) from {counter.table}";
            string errorMessage = "";
            if (!string.IsNullOrEmpty(counter.condition))
            {
                KeyValuePair<string, string> queryAnalysis = extractConditionFromSetting(counter.condition);
                query += queryAnalysis.Key;
                errorMessage = queryAnalysis.Value;
            }
            return new KeyValuePair<string, string>(query, errorMessage);
        }

        private KeyValuePair<string, string> extractConditionFromSetting(string condition)
        {
            KeyValuePair<bool, string> queryAnalysis = ValidateQueryCriterias(condition);
            if (queryAnalysis.Key == true)
            {
                return new KeyValuePair<string, string>($" where {condition}", "");
            }
            return new KeyValuePair<string, string>($"", queryAnalysis.Value);
        }

        public KeyValuePair<bool, string> ValidateQueryCriterias(string condition)
        {
            KeyValuePair<bool, string> result = new KeyValuePair<bool, string>(true, "");

            condition = condition.ToLower();
            if (!condition.Contains(";"))
            {
                //bool isValid = true;
                //List<string> logicalsOperatorsRead
                ////string logicalOperator = "";
                //List<string> logicalsOperators = new List<string>{ ">", "<", "=", "!=", ">=", "<=" };
                //foreach (string logicalOperator in logicalsOperators)
                //{
                //    if (condition.Contains(logicalOperator))
                //    {
                //        int index = logicalOperator.IndexOf(logicalOperator);
                //        string part1 = condition.Remove(index);
                //        string part2 = condition.Substring(index + logicalOperator.Length);
                //    }
                //}
                bool part1read = false;
                bool operatorRead = false;
                //bool part2read = false;
                bool logicalOperatorRead = false;
                List<string> comparators = new List<string> { ">", "<", "=", "<>", ">=", "<=", "like", "is", "is not", "in" };

                string previousPart = "";

                foreach (string part in condition.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (part != "and" && part != "or")
                    {
                        logicalOperatorRead = false;

                        if (!part1read && !comparators.Contains(part))
                        {
                            part1read = true;
                        }
                        else if (part1read && !operatorRead && comparators.Contains(part))
                        {
                            operatorRead = true;
                        }
                        else if (part1read && operatorRead && !comparators.Contains(part))
                        {
                            part1read = false;
                            operatorRead = false;
                        }
                        else
                        {
                            result = new KeyValuePair<bool, string>(false, $"Erreur dans la condition vers {previousPart} et {part}");
                            break;
                        }
                    }
                    else
                    {
                        if (logicalOperatorRead)
                        {
                            result = new KeyValuePair<bool, string>(false, $"Erreur dans la condition vers {previousPart} et {part}");
                            break;
                        }
                        else
                        {
                            logicalOperatorRead = true;
                        }
                    }
                    previousPart = part;
                }
                return result;
            }
            return new KeyValuePair<bool, string>(false, "La condition ne peut pas contenir le caractère ;");
        }

        public long RoundValue(double value)
        {
            if (value > 1000 && value < 10000)
            {
                if ((value / 10000) > 0.75)
                {
                    value = (long)(10000 * 0.75);
                }
                else if((value / 10000) > 0.5)
                {
                    value = (long)(10000 * 0.5);
                }
                else if((value / 10000) > 0.25)
                {
                    value = (long)(10000 * 0.25);
                }
                else if((value / 10000) > 0.1)
                {
                    value = (long)(10000 * 0.1);
                }
            }
            else
            {
                int i = 0;
                while (value > 10)
                {
                    value = (int)Math.Round((value / 10));
                    i++;
                }
                for (int j = 0; j < i; j++)
                {
                    value = value * 10;
                }
            }
            return (long)value;
        }

        public List<string> GetAllTables()
        {
            var tables = new List<string>();
            var db = new PetaPoco.Database("SiteSqlServer");

            var sql = @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
            foreach (var row in db.Fetch<dynamic>(sql))
            {
                tables.Add(row.TABLE_NAME);
            }
            
            return tables;
        }
    }
}