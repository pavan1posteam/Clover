
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
            #region OLD Approach
            //string accesstoken = "";
            ////string refreshtoken = "";
            //string[] token_info = new string[2];
            //try
            //{
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //    string baseUrl1 = baseUrl + "/oauth/v2/refresh";
            //    var client = new RestClient(baseUrl1);
            //    var request = new RestRequest(Method.POST);
            //    request.AddHeader("cache-control", "no-cache");
            //    request.AddHeader("content-type", "application/json");

            //    Authdetails_Refresh auth = new Authdetails_Refresh();
            //    auth.client_id = clientid;
            //    auth.refresh_token = refresh_token;

            //    string auth_json = JsonConvert.SerializeObject(auth);

            //    request.AddParameter("application/json", auth_json, ParameterType.RequestBody);
            //    IRestResponse response = client.Execute(request);
            //    string clover_response = response.Content;
            //    dynamic responseData = JsonConvert.DeserializeObject(response.Content);
            //    accesstoken = responseData["access_token"].Value;

            //    if (response.StatusCode.ToString() == "OK")
            //    {
            //        accesstoken = responseData.access_token;
            //        refresh_token = responseData.refresh_token;
            //        token_info[0] = accesstoken;
            //        token_info[1] = refresh_token;

            //        DataTable dsupstoken = new DataTable();
            //        DatabaseObject dbObjItem1 = new DatabaseObject();
            //        List<SqlParameter> sItemParams = new List<SqlParameter>();
            //        try
            //        {
            //            sItemParams.Add(new SqlParameter("@StoreId", StoreId));
            //            sItemParams.Add(new SqlParameter("@ClientId", clientid));
            //            sItemParams.Add(new SqlParameter("@AccessToken", accesstoken));
            //            sItemParams.Add(new SqlParameter("@refresh_token", refresh_token));
            //            sItemParams.Add(new SqlParameter("@Request", auth_json));
            //            sItemParams.Add(new SqlParameter("@Response", clover_response));
            //            sItemParams.Add(new SqlParameter("@Status", "Refresh_Token"));
            //            dsupstoken = dbObjItem1.GetDataTable("usp_bc_CloverAccessTokenInsert", sItemParams);
            //        }
            //        catch (Exception ex)
            //        { Console.WriteLine(ex.Message); }

            //        SaveCloverAccessTokenReqResInfo(StoreId, auth_json, clover_response, "Refresh_Token");
            //    }
            //    else
            //    {
            //        SaveCloverAccessTokenReqResInfo(StoreId, auth_json, clover_response, "Refresh_Token");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    new clsEmail().sendEmail(DeveloperId, "", "", "Error in CloverPos@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
            //}
            //return token_info;

            #endregion

            string text = "";
            //string text2 = "";
            string[] array = new string[2];
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string text3 = baseUrl + "/oauth/v2/refresh";
                RestClient val = new RestClient(text3);
                RestRequest val2 = new RestRequest((Method)1);
                val2.AddHeader("cache-control", "no-cache");
                val2.AddHeader("content-type", "application/json");
                Authdetails_Refresh authdetails_Refresh = new Authdetails_Refresh();
                authdetails_Refresh.client_id = clientid;
                authdetails_Refresh.refresh_token = refresh_token;
                string text4 = JsonConvert.SerializeObject((object)authdetails_Refresh);
                val2.AddParameter("application/json", (object)text4, (ParameterType)4);
                IRestResponse val3 = val.Execute((IRestRequest)(object)val2);
                string content = val3.Content;
                dynamic val4 = JsonConvert.DeserializeObject(val3.Content);
                text = val4["access_token"].Value;
                if (val3.StatusCode.ToString() == "OK")
                {
                    text = val4.access_token;
                    refresh_token = val4.refresh_token;
                    array[0] = text;
                    array[1] = refresh_token;
                    DataTable dataTable = new DataTable();
                    DatabaseObject databaseObject = new DatabaseObject();
                    List<SqlParameter> list = new List<SqlParameter>();
                    try
                    {
                        list.Add(new SqlParameter("@StoreId", StoreId));
                        list.Add(new SqlParameter("@ClientId", clientid));
                        list.Add(new SqlParameter("@AccessToken", text));
                        list.Add(new SqlParameter("@refresh_token", refresh_token));
                        list.Add(new SqlParameter("@Request", text4));
                        list.Add(new SqlParameter("@Response", content));
                        list.Add(new SqlParameter("@Status", "Refresh_Token"));
                        dataTable = databaseObject.GetDataTable("usp_bc_CloverAccessTokenInsert", list);
                    }
                    catch (Exception)
                    {
                    }
                    SaveCloverAccessTokenReqResInfo(StoreId, text4, content, "Refresh_Token");
                }
                else
                {
                    SaveCloverAccessTokenReqResInfo(StoreId, text4, content, "Refresh_Token");
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in CloverPos@" + DateTime.UtcNow.ToString() + " GMT", ex2.Message + "<br/>" + ex2.StackTrace);
            }
            return array;

        }
        public string CloverSettings(int StoreId, string MerchantId, string ClientId, string TokenId, string Code, string InStock, List<categories> Category)
        {
            #region OLD Approach
            //clsBOCloverStoreSettings cloverposserttings = new clsBOCloverStoreSettings();
            //categories cats = new categories();
            ////cloverposserttings = JsonConvert.DeserializeObject<clsBOCloverStoreSettings>();
            ////List<categories> ExistCategories = cloverposserttings.categories;
            //List<categories> ExistCategories = Category;
            //cloverposserttings.categories = new List<categories>();

            //string catjson = getCategories(MerchantId, TokenId, StoreId);
            //if (!string.IsNullOrEmpty(catjson))
            //{
            //    List<categories> cat = (List<categories>)JsonConvert.DeserializeObject(catjson, typeof(List<categories>));
            //    cat.Add(new categories { id = "Other", name = "Other", selected = false, taxrate = 0 });
            //    foreach (var item in ExistCategories)
            //    {
            //        cats = new categories();
            //        if (ExistCategories != null)
            //        {
            //            var findcat = ExistCategories.Where(m => m.id == item.id).FirstOrDefault();
            //            if (findcat != null)
            //            {
            //                cats.taxrate = findcat.taxrate;
            //                cats.selected = findcat.selected;
            //            }
            //        }
            //        cats.id = item.id;
            //        cats.name = item.name;
            //        if (item.id != "AKGXX4R4H9YP2")
            //        {
            //            cloverposserttings.categories.Add(cats);
            //        }
            //    }


            //    //clsBOCloverStoreSettings cloverposserttings = new clsBOCloverStoreSettings();
            //    cloverposserttings.merchantid = MerchantId;
            //    cloverposserttings.clientid = ClientId;
            //    cloverposserttings.code = Code;
            //    cloverposserttings.tokenid = TokenId;
            //    cloverposserttings.instock = InStock;

            //    //cloverposserttings.categories = new List<categories>();

            //    foreach (var item in cloverposserttings.categories)
            //    {
            //        if (item.selected)
            //        {
            //            cats = new categories();
            //            cats.id = item.id;
            //            //cat.name = item.name;
            //            //cat.selected = item.selected;
            //            cats.taxrate = item.taxrate;
            //            cloverposserttings.categories.Add(cats);
            //        }
            //    }
            //    JsonSerializer serializer = new JsonSerializer();
            //    string cloversettings = JsonConvert.SerializeObject(cloverposserttings);
            //    //StoreAddress st = new StoreAddress();
            //    //st.UpdatePosSettings(StoreId, "CLOVER", cloversettings);
            //    string filename = GenerateCSVFiles(StoreId.ToString(), cloverposserttings);


            //    return filename;
            //}
            //else
            //{ return ""; }
            #endregion
            clsBOCloverStoreSettings clsBOCloverStoreSettings = new clsBOCloverStoreSettings();
            categories categories = new categories();
            clsBOCloverStoreSettings.categories = new List<categories>();
            string categories2 = getCategories(MerchantId, TokenId, StoreId);
            if (!string.IsNullOrEmpty(categories2))
            {
                List<categories> list = (List<categories>)JsonConvert.DeserializeObject(categories2, typeof(List<categories>));
                list.Add(new categories
                {
                    id = "Other",
                    name = "Other",
                    selected = false,
                    taxrate = 0m
                });
                foreach (categories item in Category)
                {
                    categories = new categories();
                    if (Category != null)
                    {
                        categories categories3 = Category.Where((categories m) => m.id == item.id).FirstOrDefault();
                        if (categories3 != null)
                        {
                            categories.taxrate = categories3.taxrate;
                            categories.selected = categories3.selected;
                        }
                    }
                    categories.id = item.id;
                    categories.name = item.name;
                    if (item.id != "AKGXX4R4H9YP2")
                    {
                        clsBOCloverStoreSettings.categories.Add(categories);
                    }
                }
                clsBOCloverStoreSettings.merchantid = MerchantId;
                clsBOCloverStoreSettings.clientid = ClientId;
                clsBOCloverStoreSettings.code = Code;
                clsBOCloverStoreSettings.tokenid = TokenId;
                clsBOCloverStoreSettings.instock = InStock;
                foreach (categories category in clsBOCloverStoreSettings.categories)
                {
                    if (category.selected)
                    {
                        categories = new categories();
                        categories.id = category.id;
                        categories.taxrate = category.taxrate;
                        clsBOCloverStoreSettings.categories.Add(categories);
                    }
                }
                JsonSerializer val = new JsonSerializer();
                string text = JsonConvert.SerializeObject((object)clsBOCloverStoreSettings);
                return GenerateCSVFiles(StoreId.ToString(), clsBOCloverStoreSettings);
            }
            return "";
        }

        public string getCategories(string merchant_id, string accessToken, int StoreId)
        {
            Thread.Sleep(1000);
            #region Old Approch , Changes AccessToken Passed Along with Url
            //var client = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/categories?limit=100&access_token=" + accessToken);
            //var request = new RestRequest(Method.GET);
            //request.AddHeader("cache-control", "no-cache");
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            #endregion New changes are accesstocken passed in header
            Thread.Sleep(1000);
            RestClient val = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/categories?limit=100");
            RestRequest val2 = new RestRequest((Method)0);
            val2.AddHeader("cache-control", "no-cache");
            val2.AddHeader("content-type", "application/x-www-form-urlencoded");
            val2.AddHeader("Authorization", "Bearer " + accessToken);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse val3 = val.Execute((IRestRequest)(object)val2);
            try
            {
                if (!(val3.StatusCode.ToString().ToUpper() == "UNAUTHORIZED"))
                {
                    string content = val3.Content;
                    content = content.Substring(content.IndexOf('['));
                    return content.Substring(0, content.IndexOf(']') + 1);
                }
                Exception ex = new Exception();
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in " + StoreId + " CloverPos@" + DateTime.UtcNow.ToString() + " GMT", "StatusCode:Unauthorized Response while Getting Category<br/>" + ex.StackTrace);
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
                return "";
            }
            return "";
        }
        public string gettocken(string clientid, string code)
        {
            string text = ConfigurationManager.AppSettings["CloverAPPSecrete"];
            string text2 = baseUrl + "/oauth/token";
            RestClient val = new RestClient(text2);
            RestRequest val2 = new RestRequest((Method)1);
            val2.AddHeader("cache-control", "no-cache");
            val2.AddHeader("content-type", "application/x-www-form-urlencoded");
            val2.AddHeader("authorization", "Bearer <access_token>");
            val2.AddHeader("TokenExpiry", "100");
            string text3 = "client_id=" + clientid + "&client_secret=" + text + "&code=" + code;
            val2.AddParameter("application/x-www-form-urlencoded", (object)text3, (ParameterType)4);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse val3 = val.Execute((IRestRequest)(object)val2);
            dynamic val4 = JsonConvert.DeserializeObject(val3.Content);
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
                decimal num = default(decimal);
                RestClient val = new RestClient(baseUrl + "/v3/merchants/" + merchantid + "/tax_rates?limit=100");
                RestRequest val2 = new RestRequest((Method)0);
                val2.AddHeader("cache-control", "no-cache");
                val2.AddHeader("content-type", "application/x-www-form-urlencoded");
                val2.AddHeader("Authorization", "Bearer " + tokenid);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse val3 = val.Execute((IRestRequest)(object)val2);
                //File.AppendAllText("10917(1).json", val3.Content);
                string content = val3.Content;
                Tax tax = Load<Tax>(content);
                int num2 = (from t in tax.elements
                            where t.isDefault = true
                            select t.rate).FirstOrDefault();
                if (num2 != 0)
                {
                    num = (decimal)num2 / Convert.ToDecimal(100000);
                    num /= 100m;
                }
                List<ExportProducts> list = new List<ExportProducts>();

                //new include for fullname file
                List<FullNameProductModel> fullNameList = new List<FullNameProductModel>();

                parentItems parentItems = new parentItems();
                int num3 = 0;
                decimal num4 = default(decimal);
                string text = string.Join(",", settings.categories.Select((categories x) => x.id));
                num3 = 0;
                for (int i = 0; i <= 100000; i++)
                {
                    try
                    {
                        if (i != 0)
                        {
                            num3 = 1000;
                            num3 = num3 * i + 1;
                        }
                        else
                        {
                            num3 = 0;
                        }
                        string text2 = storeid.ToString();
                        RestClient val4 = new RestClient(baseUrl + "/v3/merchants/" + merchantid + "/items?expand=itemStock,taxRates,categories&offset=" + num3 + "&limit=1000");
                        RestRequest val5 = new RestRequest((Method)0);
                        val5.AddHeader("cache-control", "no-cache");
                        val5.AddHeader("Authorization", "Bearer " + tokenid);
                        val5.AddHeader("content-type", "application/x-www-form-urlencoded");
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        IRestResponse val6 = val4.Execute((IRestRequest)(object)val5);
                        string content2 = val6.Content;
                        //File.AppendAllText("12615.json", content2);
                        if (val6.StatusCode.ToString().ToUpper() != "OK")
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
                            num4 = default(decimal);
                            bool flag2 = true;
                            if (element.defaultTaxRates != null && element.defaultTaxRates.ToUpper() == "TRUE")
                            {
                                flag2 = false;
                                num4 = num;
                            }
                            if (flag2 && element.taxRates != null && element.taxRates.elements.Count > 0 && element.taxRates.elements[0].rate != 0)
                            {
                                num4 = (decimal)element.taxRates.elements[0].rate / Convert.ToDecimal(100000);
                                num4 /= 100m;
                            }
                            if (settings.instock.ToUpper() == "TRUE" && element.itemStock != null && (element.itemStock.stockCount == 0 || element.itemStock.stockCount <= 0))
                            {
                                continue;
                            }
                            ExportProducts exportProducts = new ExportProducts();
                            //new include
                            FullNameProductModel fn = fn = new FullNameProductModel();

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
                            exportProducts.tax = num4;
                            exportProducts.upc = "";
                            if (element.code != null)
                            {
                                exportProducts.upc = "#" + element.code.ToString();
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
                                    exportProducts.upc = "#9911258" + id;
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
                                string input2 = exportProducts.StoreProductName.ToString();
                                Regex regex2 = new Regex("\\b(\\d+(?:\\.\\d+)?\\s?(?:pk|oz|ml|l|L|Pack|Oz|OZ|can|Can))\\b", RegexOptions.IgnoreCase);
                                string text3 = regex2.Match(input2).ToString().ToUpper();
                                if (text3.Contains("OZ") || text3.Contains("ML"))
                                {
                                    exportProducts.deposit = Convert.ToDecimal(0.06);
                                }
                                else if (text3.Contains("PK") || text3.Contains("PACK"))
                                {
                                    Match match2 = Regex.Match(text3, "\\d+(\\.\\d+)?");
                                    exportProducts.deposit = (decimal)Convert.ToInt32(match2.Value) * Convert.ToDecimal(0.06);
                                }
                            }
                            if (UOMbaseddeposit.Contains(storeid.ToString()))
                            {
                                string input3 = exportProducts.StoreProductName.ToString();
                                Regex regex3 = new Regex("(\\.\\d+\\s+Oz|\\d+\\s+LB|\\d+\\s+Lb|\\d+.\\d+\\s+LB|\\d+\\s+LE|\\d+\\s+Le|\\d+\\s+Lo|\\d+\\s+Lu|\\d+\\s+Li|\\d+\\s+ml|\\d+ml|\\d+.\\d+L|\\dL|\\d+Oz|\\d+.\\d+Oz|\\d+\\s+Oz|\\d+.\\d+\\s+Oz|\\d+\\s+Oz.|\\d+\\s+L|\\d+.\\d+\\s+L|\\d+\\s+Ml|\\d+Ml)");
                                Match match3 = regex3.Match(input3);
                                if (match3.Success)
                                {
                                    string input4 = match3.ToString();
                                    Regex regex4 = new Regex("(\\d+\\s+LB|\\d+.\\d+\\s+LB|\\d+\\s+LE|\\d+\\s+Le|\\d+\\s+Lo|\\d+\\s+Lu|\\d+\\s+Li|\\d+\\s+Lb)");
                                    Match match4 = regex4.Match(input4);
                                    if (match4.Success)
                                    {
                                        exportProducts.uom = " ";
                                    }
                                    else
                                    {
                                        exportProducts.uom = match3.Value;
                                    }
                                }
                                else
                                {
                                    exportProducts.uom = " ";
                                }
                                string input5 = exportProducts.uom.ToString().ToUpper();
                                Regex regex5 = new Regex("(\\.\\d+\\s+Oz|\\d+Oz|\\d+.\\d+Oz|\\d+\\s+Oz|\\d+.\\d+\\s+Oz|\\d+\\s+Oz.)");
                                Match match5 = regex5.Match(input5);
                                if (match5.Success)
                                {
                                    string value = match5.Value;
                                    string value2 = Regex.Replace(value, "[^\\d+.\\d]", "");
                                    double num5 = 29.5735;
                                    decimal num6 = (decimal)num5;
                                    exportProducts.uom = Convert.ToString(Convert.ToDecimal(value2) * num6);
                                }
                                else
                                {
                                    string input6 = exportProducts.uom.ToString().ToUpper();
                                    Regex regex6 = new Regex("(\\d+.\\d+L|\\dL|\\d+\\s+L|\\d+.\\d+\\s+L)");
                                    Match match6 = regex6.Match(input6);
                                    if (match6.Success)
                                    {
                                        string value3 = match6.Value;
                                        string value4 = Regex.Replace(value3, "[^\\d+.\\d]", "");
                                        double num7 = 1000.0;
                                        decimal num8 = (decimal)num7;
                                        exportProducts.uom = Convert.ToString(Convert.ToDecimal(value4) * num8);
                                    }
                                }
                                string input7 = exportProducts.uom.ToString().ToUpper();
                                string value5 = Regex.Replace(input7, "[^\\d+.\\d]", "");
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
                                string input8 = exportProducts.pack.ToString().ToUpper();
                                string value6 = Regex.Replace(input8, "[^\\d+.\\d]", "");
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
                                string input9 = exportProducts.StoreProductName.ToString();
                                Regex regex7 = new Regex("(\\.\\d+\\s+Oz|\\d+\\s+ml|\\d+ml|\\d+.\\d+L|\\dL|\\d+Oz|\\d+.\\d+Oz|\\d+\\s+Oz|\\d+.\\d+\\s+Oz|\\d+\\s+Oz.|\\d+\\s+L|\\d+.\\d+\\s+L|\\d+\\s+Ml|\\d+Ml)");
                                Match match7 = regex3.Match(input9);
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
                            

                            if (upcnotnullstores.Contains(storeid.ToString()) && exportProducts.upc != "")
                            {
                                list.Add(exportProducts);
                                fullNameList.Add(fn);
                            }
                            else if (exportProducts.CategoryId != "AKGXX4R4H9YP2" && element.code != null && element.code != "" && exportProducts.sku != "YWBMNBHY8J63E" && exportProducts.sku != "BSX0WDE4S26GR")
                            {
                                list.Add(exportProducts);
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
                    var source = from a in list
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
                    List<ExportProductss> source4 = (from a in list
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
                List<ExportProducts> list3 = new List<ExportProducts>();
                List<FullNameProductModel> fnlist = new List<FullNameProductModel>();

                if (ExcludeToGo.Contains(storeid))
                {
                    list3.AddRange(list);
                }
                else
                {
                    foreach (categories categoryItemid in settings.categories)
                    {
                        List<ExportProducts> collection = list.Where((ExportProducts x) => x.CategoryId.Contains(categoryItemid.id)).ToList();
                        list3.AddRange(collection);
                        List<FullNameProductModel> collectionfn = fullNameList.Where((FullNameProductModel x) => x.CategoryId.Contains(categoryItemid.id)).ToList();
                        fnlist.AddRange(collectionfn);
                    }
                }
                
                
                string text5 = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\PRODUCT" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                CreateCSVFromGenericList(list3, text5, storeid);
                string text6 = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\FULLNAME" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                CreateCSVFromGenericList(fnlist, text6, storeid);
                
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