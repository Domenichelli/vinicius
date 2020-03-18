using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Data.Linq;
using DevExpress.Data.Linq.Helpers;
using DevExpress.Web.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DevExpress.Web.Demos
{
    public static class GridViewCustomBindingHandlers
    {

        static int count;
        private static void BuildRequestHeader(HttpClient client)
        {
            client.BaseAddress = new Uri("https://mfitonline.co.uk");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("token", "3fba1ff4-59d4-457f-8b58-c760793dda9d");
        }

        public static async Task<int> GetDataCount(string apiUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    BuildRequestHeader(client);

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Int32.Parse(result);
                    }
                    else
                    {
                        throw new HttpResponseException(response);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<DataTable> GetDataAsync(string apiUrl)
        {
            using (var client = new HttpClient())
            {
                BuildRequestHeader(client);

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
					DataTable dt = JsonHelper.JsonToDataTable(json);
                    return dt;
                }
                else
                {
                    throw new HttpResponseException(response);
                }
            }
        }

        public static void GetDataRowCountAdvanced(GridViewCustomBindingGetDataRowCountArgs e)
        {
			count  = Task.Run(async ()=> await GetDataCount("/api/v1/appointments/count") ).Result;
			
			e.DataRowCount = count;
        }

        public static void GetDataAdvanced(GridViewCustomBindingGetDataArgs e)
        {
            try
            {
				var pageOffset = (int)e.StartDataRowIndex / e.DataRowCount;
				var result = Task.Run(async ()=> 
					await GetDataAsync("/api/v1/appointments/full?PageOffSet=" + pageOffset.ToString() + "&PageSize=" + e.DataRowCount.ToString()).ConfigureAwait(false)
				).Result;

				e.Data = result.GetData();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static IEnumerable GetData(this DataTable dataTable)
        {
            foreach (DataRow data in dataTable.AsEnumerable())
            {
                yield return GetElement(data, dataTable.Columns);
            }
        }
        static object GetElement(DataRow dataRow, DataColumnCollection columns)
        {
            var element = (IDictionary<string, object>)new ExpandoObject();

            foreach (DataColumn column in columns)
            {
                element.Add(column.ColumnName, dataRow[column.ColumnName]);
            }
            return element;
        }
    }

    public static class GridViewCustomBindingSummaryCache
    {
        const string CacheKey = "B08E5DF5-4D10-45C7-B4F1-C95EB2FE69C8";
        static HttpContext Context { get { return HttpContext.Current; } }
        static Dictionary<string, int> Cache
        {
            get
            {
                if (Context.Items[CacheKey] == null)
                    Context.Items[CacheKey] = new Dictionary<string, int>();
                return (Dictionary<string, int>)Context.Items[CacheKey];
            }
        }
        public static bool TryGetCount(string key, out int count)
        {
            count = 0;
            if (!Cache.ContainsKey(key))
                return false;
            count = Cache[key];
            return true;
        }
        public static void SaveCount(string key, int count)
        {
            Cache[key] = count;
        }
    }
}
