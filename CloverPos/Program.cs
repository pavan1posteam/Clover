using CloverPos.Models;
using System;
using System.Configuration;

namespace CloverPos
{

    class Program
    {
        private static void Main(string[] args)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            try
            {
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                foreach (POSSetting current in pOSSettings.PosDetails)
                {
                    try
                    {
                        //if (current.StoreSettings.StoreId == 12821)
                        //{

                        //}
                        //else
                        //{
                        //    continue;
                        //}
                        if (current.StoreSettings.POSSettings != null && current.StoreSettings.POSSettings.categories != null)
                        {
                            if (current.Refresh_token != "")// && current.StoreSettings.StoreId == 10917
                            {
                                clsCloverPos clsCloverPos = new clsCloverPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.merchantId, current.StoreSettings.POSSettings.tokenid, current.StoreSettings.POSSettings.ClientId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.instock, current.StoreSettings.POSSettings.categories, current.Refresh_token);
                                Console.WriteLine();
                            }
                            else if (current.PosName.ToUpper() == "CLOVER" && current.Refresh_token == "")
                            {
                                clsCloverPos clsCloverPos = new clsCloverPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.merchantId, current.StoreSettings.POSSettings.tokenid, current.StoreSettings.POSSettings.ClientId, current.StoreSettings.POSSettings.Code, current.StoreSettings.POSSettings.instock, current.StoreSettings.POSSettings.categories);
                                Console.WriteLine();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in CloverPos@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
        }
    }
}