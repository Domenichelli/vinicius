using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace DevExpress.Web.Demos
{
    public static class JsonHelper
    {

        //If we are showing the APPT details as we need the Start Date and End date, 
        //So we pass the API URL that is termed here as Method.
        //If method param contains the Appointments then we add the start date and end date to the datatable
        //Startdate and enddate columns are added to the Datatable only if JSON contains a field or Parameter with Date
        public static DataTable JsonToDataTable(string Json)
        {
            try
            {
                //When using var JsonDictionary not able to get the Count Parameter.
                dynamic JsonDictionary = JsonConvert.DeserializeObject(Json.ToString());
             
                List<Dictionary<string, object>> ListRecords = new List<Dictionary<string, object>>();
                if (JsonDictionary.Count == null)
                    ListRecords.Add(JsonHelper.DeserializeAndFlatten(Json));
                else
                    for (int i = 0; i < JsonDictionary.Count; i++)
                        ListRecords.Add(JsonHelper.DeserializeAndFlatten(JsonDictionary[i].ToString()));

                DataTable JsonToDataTable = new DataTable();
                if (ListRecords.Count == 0)
                    return JsonToDataTable;

                var columnNames = ListRecords.SelectMany(dict => dict.Keys).Distinct();
                JsonToDataTable.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
                foreach (Dictionary<string, object> item in ListRecords)
                {
                    var row = JsonToDataTable.NewRow();
                    foreach (var key in item.Keys)
                    {
                        row[key] = item[key];
                    }
                    JsonToDataTable.Rows.Add(row);
                }
                return JsonToDataTable;
            }
            catch (Exception)
            {
                return default(DataTable);
            }
        }

        public static Dictionary<string, object> DeserializeAndFlatten(string json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            JToken token = JToken.Parse(json);
            FillDictionaryFromJToken(dict, token, "");
            return dict;
        }

        private static void FillDictionaryFromJToken(Dictionary<string, object> dict, JToken token, string prefix)
        {
            DateTime dateValue;
            DateTimeOffset offsetDateValue;
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (JProperty prop in token.Children<JProperty>())
                    {
                        FillDictionaryFromJToken(dict, prop.Value, Join(prefix, prop.Name));
                    }
                    break;

                case JTokenType.Array:
                    int index = 0;
                    foreach (JToken value in token.Children())
                    {
                        FillDictionaryFromJToken(dict, value, Join(prefix, index.ToString()));
                        index++;
                    }
                    break;

                default:
                    try
                    {
						object value = ((JValue)token).Value;
                        if ((value != null) && DateTimeOffset.TryParse(value.ToString(), out offsetDateValue))
                        {
                            if (offsetDateValue != DateTimeOffset.MinValue)
                            {
                                ((JValue)token).Value = String.Format("{0:dd/MM/yyyy}", offsetDateValue);
                            }
                        }
						else if ((value != null) && DateTime.TryParse(((JValue)token).Value.ToString(), out dateValue))
                        {
                            if (dateValue != DateTime.MinValue)
                            {
                                ((JValue)token).Value = String.Format("{0:dd/MM/yyyy}", dateValue);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    dict.Add(prefix, ((JValue)token).Value);
                    break;
            }
        }

        private static string Join(string prefix, string name)
        {
            return (string.IsNullOrEmpty(prefix) ? name : prefix + "." + name);
        }

    }
}