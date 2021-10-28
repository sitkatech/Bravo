using Newtonsoft.Json;
using Bravo.Common.DataContracts.APIFunctionModels;
using Bravo.Common.Utilities;
using System;
using System.Net.Http;
using System.Text;

namespace Bravo.Accessors.APIFunctions
{
    class APIFunctionsAccessor : BaseTableAccessor, IAPIFunctionsAccessor
    {
        public void MakeFunctionCall(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = client.GetAsync(url).Result)
                using (HttpContent respContent = response.Content)
                {
                    var tr = respContent.ReadAsStringAsync().Result;
                }
            }
        }
    }
}
