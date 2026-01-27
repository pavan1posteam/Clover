
using CloverPos.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;


namespace CloverPos
{
    public class clsCloverPos : clsBOClover
    {
        string constr = ConfigurationManager.AppSettings.Get("LiquorAppsConnectionString");
        string baseUrl = ConfigurationManager.AppSettings["CloverBaseURL"];
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string upcasskuandcode = ConfigurationManager.AppSettings.Get("upc_sku_code");
        string upcassku = ConfigurationManager.AppSettings.Get("upc_sku");
        string upcasskuandappend = ConfigurationManager.AppSettings.Get("upc_sku_append");
        string upcnotnullstores = ConfigurationManager.AppSettings.Get("upc_not_null");
        string exception = ConfigurationManager.AppSettings.Get("exception_store");
        string qtymerging = ConfigurationManager.AppSettings.Get("qty_merging");
        string UOMbaseddeposit = ConfigurationManager.AppSettings.Get("uom_based_deposit");
        string ExcludeToGo = ConfigurationManager.AppSettings.Get("ExcludeToGo");
        string StaticQty = ConfigurationManager.AppSettings.Get("StaticQty");
        string Deposit = ConfigurationManager.AppSettings.Get("Deposit");
        string Staticqty100 = ConfigurationManager.AppSettings.Get("Staticqty100");
        string EnabledOnline = ConfigurationManager.AppSettings.Get("EnabledOnline");
        string NegativeToPositive = ConfigurationManager.AppSettings.Get("NegativeToPositive");
        string upcskucodeidnull= ConfigurationManager.AppSettings.Get("upcskucodeid");


        StoreSetting ss = new StoreSetting();
        int storeId;

        public clsCloverPos(int StoreId, string MerchantId, string TokenId, string ClientId, string Code, string InStock, List<categories> Category, string refreshtoken)
        {
            Console.WriteLine("Generating Product File of Clover " + StoreId);
            string[] array = Clover_RefreshToken(ClientId, refreshtoken, StoreId, 0, 0);
            string value = CloverSettings(StoreId, MerchantId, ClientId, array[0], Code, InStock, Category);
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Product File Generated for Clover " + StoreId);
            }
            else
            {
                Console.WriteLine("Product File Not Generated For Clover " + StoreId);
            }
            storeId = StoreId;
        }
        public clsCloverPos(int StoreId, string MerchantId, string TokenId, string ClientId, string Code, string InStock, List<categories> Category)
        {
            Console.WriteLine("Generating Product File of Clover " + StoreId);
            string value = CloverSettings(StoreId, MerchantId, ClientId, TokenId, Code, InStock, Category);
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Product File Generated for Clover " + StoreId);
            }
            else
            {
                Console.WriteLine("Product File Not Generated For Clover " + StoreId);
            }
            storeId = StoreId;
        }

        public string[] Clover_RefreshToken(string clientid, string refresh_token, int StoreId, int expiresintime, int AccessTokenLastUpdate)
        {
            string accesstoken = "";
            //string refreshtoken = "";
            string[] token_info = new string[2];
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string Url = baseUrl + "/oauth/v2/refresh";
                RestClient client = new RestClient(Url);
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                Authdetails_Refresh authdetails_Refresh = new Authdetails_Refresh();
                authdetails_Refresh.client_id = clientid;
                authdetails_Refresh.refresh_token = refresh_token;
                string auth_json = JsonConvert.SerializeObject(authdetails_Refresh);
                request.AddParameter("application/json", auth_json, (ParameterType)4);
                IRestResponse response = client.Execute(request);
                string content = response.Content;
                dynamic responseData = JsonConvert.DeserializeObject(response.Content);
                accesstoken = responseData["access_token"].Value;
                if (response.StatusCode.ToString() == "OK")
                {
                    accesstoken = responseData.access_token;
                    refresh_token = responseData.refresh_token;
                    token_info[0] = accesstoken;
                    token_info[1] = refresh_token;
                    DataTable dataTable = new DataTable();
                    DatabaseObject databaseObject = new DatabaseObject();
                    List<SqlParameter> list = new List<SqlParameter>();
                    try
                    {
                        list.Add(new SqlParameter("@StoreId", StoreId));
                        list.Add(new SqlParameter("@ClientId", clientid));
                        list.Add(new SqlParameter("@AccessToken", accesstoken));
                        list.Add(new SqlParameter("@refresh_token", refresh_token));
                        list.Add(new SqlParameter("@Request", auth_json));
                        list.Add(new SqlParameter("@Response", content));
                        list.Add(new SqlParameter("@Status", "Refresh_Token"));
                        dataTable = databaseObject.GetDataTable("usp_bc_CloverAccessTokenInsert", list);
                    }
                    catch (Exception)
                    {
                    }
                    SaveCloverAccessTokenReqResInfo(StoreId, auth_json, content, "Refresh_Token");
                }
                else
                {
                    SaveCloverAccessTokenReqResInfo(StoreId, auth_json, content, "Refresh_Token");
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in CloverPos@" + DateTime.UtcNow.ToString() + " GMT", ex2.Message + "<br/>" + ex2.StackTrace);
            }
            return token_info;

        }
        public string CloverSettings(int StoreId, string MerchantId, string ClientId, string TokenId, string Code, string InStock, List<categories> Category)
        {
            clsBOCloverStoreSettings cloverposserttings = new clsBOCloverStoreSettings();
            categories categories = new categories();
            List<categories> ExistCategories = Category;
            cloverposserttings.categories = new List<categories>();
            string catjson = getCategories(MerchantId, TokenId, StoreId);
            if (!string.IsNullOrEmpty(catjson))
            {
                List<categories> cat = (List<categories>)JsonConvert.DeserializeObject(catjson, typeof(List<categories>));
                cat.Add(new categories
                {
                    id = "Other",
                    name = "Other",
                    selected = false,
                    taxrate = 0m
                });
                foreach (categories item in ExistCategories)
                {
                    categories = new categories();
                    if (ExistCategories != null)
                    {
                        categories findcat = ExistCategories.Where((categories m) => m.id == item.id).FirstOrDefault();
                        if (findcat != null)
                        {
                            categories.taxrate = findcat.taxrate;
                            categories.selected = findcat.selected;
                        }
                    }
                    categories.id = item.id;
                    categories.name = item.name;
                    if (item.id != "AKGXX4R4H9YP2")
                    {
                        cloverposserttings.categories.Add(categories);
                    }
                }
                cloverposserttings.merchantid = MerchantId;
                cloverposserttings.clientid = ClientId;
                cloverposserttings.code = Code;
                cloverposserttings.tokenid = TokenId;
                cloverposserttings.instock = InStock;
                foreach (categories item in cloverposserttings.categories)
                {
                    if (item.selected)
                    {
                        categories = new categories();
                        categories.id = item.id;
                        //cat.name = item.name;
                        //cat.selected = item.selected;
                        categories.taxrate = item.taxrate;
                        cloverposserttings.categories.Add(categories);
                    }
                }
                JsonSerializer serializer = new JsonSerializer();
                string cloversettings = JsonConvert.SerializeObject(cloverposserttings);
                return GenerateCSVFiles(StoreId.ToString(), cloverposserttings);
            }
            return "";
        }

        public string getCategories(string merchant_id, string accessToken, int StoreId)
        {
            //Thread.Sleep(1000);
            #region Old Approch , Changes AccessToken Passed Along with Url
            //var client = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/categories?limit=100&access_token=" + accessToken);
            //var request = new RestRequest(Method.GET);
            //request.AddHeader("cache-control", "no-cache");
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            #endregion New changes are accesstocken passed in header
            Thread.Sleep(1000);
            RestClient client = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/categories?limit=100");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("Authorization", "Bearer " + accessToken);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            try
            {
                if (!(response.StatusCode.ToString().ToUpper() == "UNAUTHORIZED"))
                {
                    string content = response.Content;
                    content = content.Substring(content.IndexOf('['));
                    return content.Substring(0, content.IndexOf(']') + 1);
                }
                Exception ex = new Exception();
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in " + StoreId + " CloverPos@" + DateTime.UtcNow.ToString() + " GMT", "StatusCode:Unauthorized Response while Getting Category<br/>" + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
            return "";
        }
        public string gettoken(string clientid, string code)
        {
            string text = ConfigurationManager.AppSettings["CloverAPPSecrete"];
            string url = baseUrl + "/oauth/token";
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "Bearer <access_token>");
            request.AddHeader("TokenExpiry", "100");
            string json = "client_id=" + clientid + "&client_secret=" + text + "&code=" + code;
            request.AddParameter("application/x-www-form-urlencoded", json, (ParameterType)4);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            dynamic val4 = JsonConvert.DeserializeObject(response.Content);
            //File.AppendAllText("11264.json", val4);
            return val4["access_token"].Value;

        }
        public T Load<T>(string jsonstr)
        {
            return JsonConvert.DeserializeObject<T>(jsonstr);
        }
        //new include for getPack and getVolume
        public int getpack(string prodName)
        {
            if (string.IsNullOrEmpty(prodName))
                return 1;

            prodName = prodName.ToUpper();
            var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)\s*PK");
            var prodPack = regexMatch.Groups["Result"].Value;

            if (!string.IsNullOrEmpty(prodPack))
            {
                int.TryParse(prodPack, out int outVal);
                return outVal;
            }

            return 1;
        }

        public string getVolume(string prodName)
        {
            if (string.IsNullOrEmpty(prodName))
                return "";

            prodName = prodName.ToUpper();

            var regexMatch = Regex.Match(
                prodName,
                @"(?<Result>\d+\s*ML|\d+\s*LTR|\d+\s*L|\d+\s*OZ)",
                RegexOptions.IgnoreCase
            );

            return regexMatch.Success ? regexMatch.Value : "";
        }

        //public void SaveRequestResponse(string jsonStringReq, string CLOVERJsonResp, string ErrorMessage, string storeid)
        //{
        //    DataSet dsResult = new DataSet();

        //    List<SqlParameter> sItemParams = new List<SqlParameter>();
        //    try
        //    {

        //        sItemParams.Add(new SqlParameter("@PosName", "CLOVER"));
        //        sItemParams.Add(new SqlParameter("@PosRequest", jsonStringReq));
        //        sItemParams.Add(new SqlParameter("@PosResponse", CLOVERJsonResp));
        //        sItemParams.Add(new SqlParameter("@ErrorMessage", ErrorMessage));
        //        sItemParams.Add(new SqlParameter("@StoreID", storeid));

        //        using (SqlConnection con = new SqlConnection(constr))
        //        {
        //            using (SqlCommand cmd = new SqlCommand())
        //            {
        //                cmd.Connection = con;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.CommandText = "usp_bc_Clover_PosReqResInsert";
        //                cmd.CommandTimeout = 3600;
        //                foreach (SqlParameter par in sItemParams)
        //                {
        //                    cmd.Parameters.Add(par);
        //                }
        //                using (SqlDataAdapter da = new SqlDataAdapter())
        //                {
        //                    da.SelectCommand = cmd;
        //                    da.Fill(dsResult);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    { }
        //}
        public string GenerateCSVFiles(string storeid, clsBOCloverStoreSettings settings)
        {
            try
            {
                string merchantid = settings.merchantid;
                string tokenid = settings.tokenid;
                decimal defTaxNum = default(decimal);
                RestClient client = new RestClient(baseUrl + "/v3/merchants/" + merchantid + "/tax_rates?limit=100");
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", "Bearer " + tokenid);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                //File.AppendAllText($"{storeid_tax_rates}.json", response.Content);
                string content = response.Content;
                Tax tax = Load<Tax>(content);
                int ElementTaxValue = (from t in tax.elements
                            where t.isDefault = true
                            select t.rate).FirstOrDefault();
                if (ElementTaxValue != 0)
                {
                    defTaxNum = (decimal)ElementTaxValue / Convert.ToDecimal(100000);
                    defTaxNum /= 100m;
                }
                List<ExportProducts> Productlist = new List<ExportProducts>();

                //new include for fullname file
                List<FullNameProductModel> fullNameList = new List<FullNameProductModel>();

                parentItems parentItems = new parentItems();
                int offset = 0;
                decimal TaxValue = default(decimal);
                string allCategories = string.Join(",", settings.categories.Select((categories x) => x.id));
                for (int i = 0; i <= 100000; i++)
                {
                    try
                    {
                        if (i != 0)
                        {
                            offset = 1000;
                            offset = offset * i + 1;
                        }
                        else
                        {
                            offset = 0;
                        }
                        string StoreId = storeid.ToString();
                        RestClient client1 = new RestClient(baseUrl + "/v3/merchants/" + merchantid + "/items?expand=itemStock,taxRates,categories&offset=" + offset + "&limit=1000");
                        RestRequest request1 = new RestRequest(Method.GET);
                        request1.AddHeader("cache-control", "no-cache");
                        request1.AddHeader("Authorization", "Bearer " + tokenid);
                        request1.AddHeader("content-type", "application/x-www-form-urlencoded");
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        IRestResponse response1 = client1.Execute(request1);
                        string content2 = response1.Content;
                        // File.AppendAllText($"{storeid_items}.json", content2);
                        if (response1.StatusCode.ToString().ToUpper() != "OK")
                        {
                            if (!exception.Contains(storeid.ToString()))
                            {
                                Exception ex = new Exception();
                                new clsEmail().sendEmail(DeveloperId, "", "", "Error in " + storeid + " CloverPos@" + DateTime.UtcNow.ToString() + " GMT", "StatusCode:ERROR Response1<br/>" + ex.Message + "<br/>" + ex.StackTrace);
                            }
                            continue;
                        }
                        parentItems parentItems2 = Load<parentItems>(content2);
                        if (parentItems2.elements == null)
                        {
                            i = 1000;
                            break;
                        }
                        if (parentItems2.elements.Count == 0)
                        {
                            i = 1000;
                            break;
                        }
                        foreach (Products element in parentItems2.elements)
                        {
                            element.id = ((element.code == null) ? element.code : element.id.ToString().Replace("\n", ""));
                            element.code = ((element.code == null) ? element.code : element.code.ToString().Replace("\n", ""));
                            if (Staticqty100.Contains(storeid.ToString()) && !element.available && element.hidden)
                            {
                                continue;
                            }
                            if (EnabledOnline.Contains(storeid.ToString()) && !element.enabledOnline)
                            {
                                continue;
                            }
                            TaxValue = default(decimal);
                            bool NodefaultTax = true;
                            if (element.defaultTaxRates != null && element.defaultTaxRates.ToUpper() == "TRUE")
                            {
                                NodefaultTax = false;
                                TaxValue = defTaxNum;
                            }
                            if (NodefaultTax && element.taxRates != null && element.taxRates.elements.Count > 0 && element.taxRates.elements[0].rate != 0)
                            {
                                TaxValue = (decimal)element.taxRates.elements[0].rate / Convert.ToDecimal(100000);
                                TaxValue /= 100m;
                            }
                            if (settings.instock.ToUpper() == "TRUE" && element.itemStock != null && (element.itemStock.stockCount == 0 || element.itemStock.stockCount <= 0))
                            {
                                continue;
                            }
                            ExportProducts exportProducts = new ExportProducts();
                            //new include
                            FullNameProductModel fn = new FullNameProductModel();

                            exportProducts.storeid = storeid;
                            exportProducts.StoreProductName = "";
                            if (element.name != null)
                            {
                                if (ExcludeToGo.Contains(storeid))
                                {
                                    if (!element.name.Contains("TO GO"))
                                    {
                                        continue;
                                    }
                                    string name = element.name;
                                    exportProducts.StoreProductName = Regex.Replace(name, "TO GO", "");
                                }
                                else
                                {
                                    exportProducts.StoreProductName = element.name;
                                }
                            }
                            exportProducts.Storedescription = "";
                            if (element.alternateName != null)
                            {
                                exportProducts.Storedescription = element.alternateName;
                            }
                            exportProducts.sku = "";
                            if (element.id != null)
                            {
                                exportProducts.sku = element.id;
                            }
                            exportProducts.pack = "1";
                            if (UOMbaseddeposit.Contains(storeid.ToString()))
                            {
                                string input = exportProducts.StoreProductName.ToString().ToUpper();
                                Regex regex = new Regex("(\\d+\\s+PACK)");
                                Match match = regex.Match(input);
                                if (match.Success)
                                {
                                    exportProducts.pack = match.Value;
                                }
                                else
                                {
                                    exportProducts.pack = "1";
                                }
                            }
                            exportProducts.qty = 0L;
                            if (element.itemStock != null && element.itemStock.stockCount != 0)
                            {
                                if (element.itemStock.stockCount > 9999)
                                {
                                    exportProducts.qty = 9999L;
                                }
                                else
                                {
                                    exportProducts.qty = element.itemStock.stockCount;
                                }
                            }
                            if (NegativeToPositive.Contains(storeid.ToString()))
                            {
                                exportProducts.qty = Convert.ToInt32(Regex.Replace(exportProducts.qty.ToString(), "-", ""));
                            }
                            if (StaticQty.Contains(storeid.ToString()))
                            {
                                exportProducts.qty = 999L;
                            }
                            if (Staticqty100.Contains(storeid.ToString()))
                            {
                                exportProducts.qty = 100L;
                            }
                            exportProducts.qty = Convert.ToInt32(exportProducts.qty);
                            exportProducts.price = 0m;
                            if (element.price != 0m && element.price != 0m)
                            {
                                exportProducts.price = element.price / 100m;
                            }
                            if (exportProducts.price <= 0m)
                            {
                                continue;
                            }
                            exportProducts.tax = TaxValue;
                            exportProducts.upc = "";
                            if (element.code != null)
                            {
                                exportProducts.upc = "#" + element.code.ToString();
                            }
                            if (upcskucodeidnull.Contains(storeid.ToString()))
                            {
                                exportProducts.upc = "";
                                exportProducts.sku = "";
                                element.code = ((element.code == null) ? "" : element.code);
                                element.sku = ((element.sku == null) ? "" : element.sku);
                                exportProducts.upc = "#" + element.code.ToString();
                                exportProducts.sku = "#" + element.sku;
                                if (exportProducts.upc.Length <= 1)
                                {
                                    if (exportProducts.sku.Length <= 1)
                                    {
                                        exportProducts.upc = "#" + element.id;
                                        exportProducts.sku = "#" + element.id;
                                    }
                                    else
                                    {
                                        exportProducts.upc = exportProducts.sku;
                                    }
                                }
                                if (exportProducts.sku.Length <=1)
                                {
                                    if (exportProducts.upc.Length <= 1)
                                    {
                                        exportProducts.upc = "#" + element.id;
                                        exportProducts.sku = "#" + element.id;
                                    }
                                    else
                                    {
                                        exportProducts.sku = exportProducts.upc;
                                    }
                                }
                            }
                            if (upcasskuandcode.Contains(storeid.ToString()))
                            {
                                element.code = ((element.code == null) ? "" : element.code);
                                element.sku = ((element.sku == null) ? "" : element.sku);
                                exportProducts.upc = "";
                                if (element.sku != "")
                                {
                                    exportProducts.upc = "#" + element.sku.ToString();
                                }
                                else if (element.code != "")
                                {
                                    exportProducts.upc = "#" + element.code.ToString();
                                }
                                else
                                {
                                    exportProducts.upc = "";
                                }
                            }
                            if (upcassku.Contains(storeid.ToString()))
                            {
                                element.code = ((element.code == null) ? "" : element.code);
                                element.sku = ((element.sku == null) ? "" : element.sku);
                                exportProducts.upc = "";
                                if (element.sku != "")
                                {
                                    exportProducts.upc = "#" + element.sku.ToString();
                                }
                                else if (element.sku == "")
                                {
                                    exportProducts.upc = "";
                                }
                                if (element.code != "")
                                {
                                    exportProducts.sku = "#" + element.code.ToString();
                                }
                            }
                            if (upcasskuandappend.Contains(storeid.ToString()))
                            {
                                string id = element.id;
                                id = Regex.Replace(id, "[^0-9]", string.Empty);
                                element.sku = ((element.sku == null) ? "" : element.sku);
                                exportProducts.upc = "";
                                if (element.sku != "")
                                {
                                    exportProducts.upc = "#" + element.sku.ToString();
                                }
                                else
                                {
                                    exportProducts.upc = "#99" +storeid+ id;
                                }
                            }
                            if (exportProducts.storeid == "12120" && element.code != null)
                            {
                                exportProducts.upc = "#" + Regex.Replace(element.code.ToString().Replace(".", ""), "E.*", "");
                            }
                            exportProducts.altupc1 = element.altupc1;
                            exportProducts.altupc2 = element.altupc2;
                            exportProducts.altupc3 = element.altupc3;
                            exportProducts.altupc4 = element.altupc4;
                            exportProducts.altupc5 = element.altupc5;
                            exportProducts.StoreProductName.Trim();
                            exportProducts.Storedescription.Trim();
                            if (Deposit.Contains(storeid.ToString()))
                            {
                                string ProdName = exportProducts.StoreProductName.ToString();
                                Regex regex2 = new Regex("\\b(\\d+(?:\\.\\d+)?\\s?(?:pk|oz|ml|l|L|Pack|Oz|OZ|can|Can))\\b", RegexOptions.IgnoreCase);
                                string matchedObject = regex2.Match(ProdName).ToString().ToUpper();
                                if (matchedObject.Contains("OZ") || matchedObject.Contains("ML"))
                                {
                                    exportProducts.deposit = Convert.ToDecimal(0.06);
                                }
                                else if (matchedObject.Contains("PK") || matchedObject.Contains("PACK"))
                                {
                                    Match matchFound = Regex.Match(matchedObject, "\\d+(\\.\\d+)?");
                                    exportProducts.deposit = Convert.ToInt32(matchFound.Value) * Convert.ToDecimal(0.06);
                                }
                            }
                            if (UOMbaseddeposit.Contains(storeid.ToString()))
                            {
                                string prodName = exportProducts.StoreProductName.ToString();
                                Regex regexPattern = new Regex("(\\.\\d+\\s+Oz|\\d+\\s+LB|\\d+\\s+Lb|\\d+.\\d+\\s+LB|\\d+\\s+LE|\\d+\\s+Le|\\d+\\s+Lo|\\d+\\s+Lu|\\d+\\s+Li|\\d+\\s+ml|\\d+ml|\\d+.\\d+L|\\dL|\\d+Oz|\\d+.\\d+Oz|\\d+\\s+Oz|\\d+.\\d+\\s+Oz|\\d+\\s+Oz.|\\d+\\s+L|\\d+.\\d+\\s+L|\\d+\\s+Ml|\\d+Ml)");
                                Match matchPattern = regexPattern.Match(prodName);
                                if (matchPattern.Success)
                                {
                                    string ResultView = matchPattern.ToString();
                                    Regex regexPattern2 = new Regex("(\\d+\\s+LB|\\d+.\\d+\\s+LB|\\d+\\s+LE|\\d+\\s+Le|\\d+\\s+Lo|\\d+\\s+Lu|\\d+\\s+Li|\\d+\\s+Lb)");
                                    Match matchPattern2 = regexPattern2.Match(ResultView);
                                    if (matchPattern2.Success)
                                    {
                                        exportProducts.uom = " ";
                                    }
                                    else
                                    {
                                        exportProducts.uom = matchPattern.Value;
                                    }
                                }
                                else
                                {
                                    exportProducts.uom = " ";
                                }
                                string prodUom = exportProducts.uom.ToString().ToUpper();
                                Regex regexPattern3 = new Regex("(\\.\\d+\\s+Oz|\\d+Oz|\\d+.\\d+Oz|\\d+\\s+Oz|\\d+.\\d+\\s+Oz|\\d+\\s+Oz.)");
                                Match matchPattern3 = regexPattern3.Match(prodUom);
                                if (matchPattern3.Success)
                                {
                                    string FoundValue = matchPattern3.Value;
                                    string value2 = Regex.Replace(FoundValue, "[^\\d+.\\d]", "");
                                    double num5 = 29.5735;
                                    decimal num6 = (decimal)num5;
                                    exportProducts.uom = Convert.ToString(Convert.ToDecimal(value2) * num6);
                                }
                                else
                                {
                                    Regex regexPattern4 = new Regex("(\\d+.\\d+L|\\dL|\\d+\\s+L|\\d+.\\d+\\s+L)");
                                    Match matchPattern4 = regexPattern4.Match(prodUom);
                                    if (matchPattern4.Success)
                                    {
                                        string value3 = matchPattern4.Value;
                                        string value4 = Regex.Replace(value3, "[^\\d+.\\d]", "");
                                        double num7 = 1000.0;
                                        decimal num8 = (decimal)num7;
                                        exportProducts.uom = Convert.ToString(Convert.ToDecimal(value4) * num8);
                                    }
                                }
                                string prodUom2 = exportProducts.uom.ToString().ToUpper();
                                string value5 = Regex.Replace(prodUom2, "[^\\d+.\\d]", "");
                                if (exportProducts.uom != " " && exportProducts.uom != null)
                                {
                                    decimal num9 = Convert.ToDecimal(value5);
                                    if (num9 < 710m)
                                    {
                                        exportProducts.deposit = Convert.ToDecimal(0.05);
                                    }
                                    else
                                    {
                                        exportProducts.deposit = Convert.ToDecimal(0.1);
                                    }
                                }
                                string prodPack = exportProducts.pack.ToString().ToUpper();
                                string value6 = Regex.Replace(prodPack, "[^\\d+.\\d]", "");
                                if (exportProducts.pack != "1" && exportProducts.uom != " " && exportProducts.uom != null)
                                {
                                    decimal num10 = Convert.ToDecimal(value5);
                                    if (num10 < 710m)
                                    {
                                        double num11 = 0.05;
                                        decimal num12 = (decimal)num11;
                                        exportProducts.deposit = Convert.ToDecimal(value6) * num12;
                                    }
                                    else
                                    {
                                        double num13 = 0.1;
                                        decimal num14 = (decimal)num13;
                                        exportProducts.deposit = Convert.ToDecimal(value6) * num14;
                                    }
                                }
                                string prodName2 = exportProducts.StoreProductName.ToString();
                                Regex regex7 = new Regex("(\\.\\d+\\s+Oz|\\d+\\s+ml|\\d+ml|\\d+.\\d+L|\\dL|\\d+Oz|\\d+.\\d+Oz|\\d+\\s+Oz|\\d+.\\d+\\s+Oz|\\d+\\s+Oz.|\\d+\\s+L|\\d+.\\d+\\s+L|\\d+\\s+Ml|\\d+Ml)");
                                Match match7 = regexPattern.Match(prodName2);
                                if (match7.Success)
                                {
                                    string input10 = match7.ToString();
                                    Regex regex8 = new Regex("(\\d+\\s+LB|\\d+.\\d+\\s+LB|\\d+\\s+LE|\\d+\\s+Le|\\d+\\s+Lo|\\d+\\s+Lu|\\d+\\s+Li|\\d+\\s+Lb)");
                                    Match match8 = regex8.Match(input10);
                                    if (match8.Success)
                                    {
                                        exportProducts.uom = " ";
                                    }
                                    else
                                    {
                                        exportProducts.uom = match7.Value;
                                    }
                                }
                                else
                                {
                                    exportProducts.uom = " ";
                                }
                            }
                            exportProducts.CategoryId = ((element.categories.elements.Count > 0) ? string.Join(",", element.categories.elements.Select((Categoryelements x) => x.id)) : "Other");
                            //new include for fullname file
                            fn.pname = exportProducts.StoreProductName;
                            fn.pdesc = exportProducts.StoreProductName;
                            fn.sku = exportProducts.sku;
                            fn.upc = exportProducts.upc;
                            fn.Price = exportProducts.price;
                            fn.uom = exportProducts.uom;
                            fn.pack = int.TryParse(exportProducts.pack, out int p) ? p : 1;
                            if (element.categories != null)
                            {
                                if (element.categories.elements != null &&
                                element.categories.elements.Count > 0)
                                {
                                    fn.pcat = element.categories.elements.Count > 0 ? element.categories.elements[0].name : "";
                                }

                            }
                            fn.uom = string.IsNullOrEmpty(fn.uom) ? getVolume(fn.pname) : fn.uom;
                            exportProducts.uom = fn.uom;
                            fn.pcat1 = "";
                            fn.pcat2 = "";
                            fn.CategoryId = exportProducts.CategoryId;
                            if (fn.pack == 1)
                            {
                                int derivedPack = getpack(exportProducts.StoreProductName);
                                fn.pack = derivedPack;                         // override fullname pack
                                exportProducts.pack = derivedPack.ToString();  // override export pack
                                
                            }

                            fn.country = "";
                            fn.region = "";
                            

                            if ((upcnotnullstores.Contains(storeid.ToString()) || upcskucodeidnull.Contains(storeid.ToString())) && exportProducts.upc != "")
                            {
                                Productlist.Add(exportProducts);
                                fullNameList.Add(fn);
                            }
                            else if (exportProducts.CategoryId != "AKGXX4R4H9YP2" && element.code != null && element.code != "" && exportProducts.sku != "YWBMNBHY8J63E" && exportProducts.sku != "BSX0WDE4S26GR")
                            {
                                Productlist.Add(exportProducts);
                                fullNameList.Add(fn);
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
                        if (!exception.Contains(storeid.ToString()))
                        {
                            Console.WriteLine(ex2.Message);
                        }
                    }
                }
                if (qtymerging.Contains(storeid.ToString()))
                {
                    var source = from a in Productlist
                                 group a by a.upc into values
                                 select new
                                 {
                                     upc = values.Key,
                                     qty = values.Sum((ExportProducts x) => x.qty)
                                 };
                    var source2 = source.ToList();
                    List<Duplicateslist> list2 = new List<Duplicateslist>();
                    Duplicateslist duplicateslist = new Duplicateslist();
                    IEnumerable<Duplicateslist> source3 = source2.Select(d => new Duplicateslist
                    {
                        upc = d.upc,
                        qty = d.qty
                    });
                    List<Duplicateslist> inner = source3.ToList();
                    List<ExportProductss> source4 = (from a in Productlist
                                                     join b in inner on a.upc equals b.upc
                                                     select new ExportProductss
                                                     {
                                                         storeid = a.storeid,
                                                         upc = a.upc,
                                                         sku = a.sku,
                                                         uom = a.uom,
                                                         qty = b.qty,
                                                         pack = a.pack,
                                                         StoreProductName = a.StoreProductName,
                                                         Storedescription = a.Storedescription,
                                                         price = a.price,
                                                         sprice = a.sprice,
                                                         start = a.start,
                                                         end = a.end,
                                                         tax = a.tax,
                                                         altupc1 = a.altupc1,
                                                         altupc2 = a.altupc2,
                                                         altupc3 = a.altupc3,
                                                         altupc4 = a.altupc4,
                                                         altupc5 = a.altupc5,
                                                         CategoryId = a.CategoryId
                                                     }).ToList();
                    source4 = (from x in source4.AsEnumerable()
                               group x by x.upc into y
                               select y.First()).ToList();
                    string text4 = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\product" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                    CreateCSVFromGenericList(source4, text4, storeid);
                    return text4;
                }
                List<ExportProducts> FinalProdList = new List<ExportProducts>();
                List<FullNameProductModel> FinalFullNamelist = new List<FullNameProductModel>();

                if (ExcludeToGo.Contains(storeid))
                {
                    FinalProdList.AddRange(Productlist);
                }
                else
                {
                    foreach (categories categoryItemid in settings.categories)
                    {
                        List<ExportProducts> collection = Productlist.Where((ExportProducts x) => x.CategoryId.Contains(categoryItemid.id)).ToList();
                        FinalProdList.AddRange(collection);
                        List<FullNameProductModel> collectionfn = fullNameList.Where((FullNameProductModel x) => x.CategoryId.Contains(categoryItemid.id)).ToList();
                        FinalFullNamelist.AddRange(collectionfn);
                    }
                }
                
                
                string text5 = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\PRODUCT" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                CreateCSVFromGenericList(FinalProdList, text5, storeid);
                string text6 = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\FULLNAME" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                CreateCSVFromGenericList(FinalFullNamelist, text6, storeid);
                
                return text5;
            }
            catch (Exception ex3)
            {
                //new clsEmail().sendEmail(DeveloperId, "", "", "Error in " + storeid + " CloverPos@" + DateTime.UtcNow.ToString() + " GMT", "StatusCode:ERROR Response3<br/>" + ex3.Message + "<br/>" + ex3.StackTrace);
                return ex3.Message.ToString();
            }
        }
        public static void CreateCSVFromGenericList<T>(List<T> list, string csvNameWithExt, string storeid)
        {
            if (list == null || list.Count == 0)
            {
                return;
            }
            if (!Directory.Exists(ConfigurationManager.AppSettings["BaseDirectory"] +"\\" + storeid + "\\Upload\\"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\");
            }
            Type type = list[0].GetType();
            string newLine = Environment.NewLine;
            using(StreamWriter streamWriter = new StreamWriter(csvNameWithExt))
            {
                object obj = Activator.CreateInstance(type);
                PropertyInfo[] properties = obj.GetType().GetProperties();
                PropertyInfo[] array = properties;
                foreach (PropertyInfo propertyInfo in array)
                {
                    if (!(propertyInfo.Name.ToUpper() == "CATEGORYID"))
                    {
                        streamWriter.Write(propertyInfo.Name.ToUpper() + ",");
                    }
                }
                streamWriter.Write(newLine);
                foreach (T item in list)
                {
                    PropertyInfo[] array2 = properties;
                    foreach (PropertyInfo propertyInfo2 in array2)
                    {
                        if (!(propertyInfo2.Name.ToUpper() == "CATEGORYID"))
                        {
                            string value = Convert.ToString(item.GetType().GetProperty(propertyInfo2.Name).GetValue(item, null)).Replace(',', ' ') + ",";
                            streamWriter.Write(value);
                        }
                    }
                    streamWriter.Write(newLine);
                }
            }
        }

        public int SaveCloverAccessTokenReqResInfo(int StoreId, string request, string response, string status)
        {
            int result = 0;
            DataTable dataTable = new DataTable();
            DatabaseObject databaseObject = new DatabaseObject();
            List<SqlParameter> list = new List<SqlParameter>();
            try
            {
                list.Add(new SqlParameter("@StoreId", StoreId));
                list.Add(new SqlParameter("@RequestJson", request));
                list.Add(new SqlParameter("@ResponseJson", response));
                list.Add(new SqlParameter("@Status", status));
                dataTable = databaseObject.GetDataTable("Usp_bc_CloverRequestResponse", list);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public class Tax
        {
            public List<TaxElements> elements { set; get; }
        }
        public class TaxElements
        {
            public string id { set; get; }
            public string name { set; get; }
            public Int32 rate { set; get; }
            public Boolean isDefault { set; get; }

        }
        public class FullName
        {
            public string storeid { set; get; }
            public string upc { set; get; }
            public Int32 qty { set; get; }
            public string sku { set; get; }
        }
        public class ExportProductss
        {
            public string storeid { set; get; }
            public string upc { set; get; }
            public long qty { set; get; }
            public string sku { set; get; }
            public string pack { set; get; }
            public string uom { set; get; }
            public string StoreProductName { set; get; }
            public string Storedescription { set; get; }
            public decimal price { set; get; }
            public decimal sprice { set; get; }
            public string start { set; get; }
            public string end { set; get; }
            public decimal tax { set; get; }
            public string altupc1 { set; get; }
            public string altupc2 { set; get; }
            public string altupc3 { set; get; }
            public string altupc4 { set; get; }
            public string altupc5 { set; get; }
            public string CategoryId { get; set; }
        }
        public class Duplicateslist
        {
            public string upc { set; get; }
            public long qty { set; get; }
        }
    }
}