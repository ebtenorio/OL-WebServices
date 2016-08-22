using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using OrderLinc.DTOs;
using PL.PersistenceServices.DTOS;

namespace OrderLincRegistryWebservice
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {


        public string HelloWorld()
        {
            return "Hello World";
        }


        [WebMethod(Description = "Check Application Version")]
        public Boolean VersionControl(string VersionMajor, string VersionMinor, out string ErroMessage)
        {
            Boolean result = false;

            if (GlobalVariable.MajorVersion == VersionMajor) //&& GlobalVariable.Minor == VersionMinor - As requested only major version should be evaluated
            {

                result = true;
                ErroMessage = "Version matched.";
            }
            else
            {

                result = false;

                ErroMessage = "Connection rejected. OrderLinc must be updated to the latest version.";

            }

            result = true; // return true for the meantime.

            return result;
        }



        [WebMethod]
        public DTOMobileAccount Login(string mUserName, string mPassword, string mDeviceNo)
        {
            try
            {
                DTOMobileAccount mAccount = new DTOMobileAccount();

                if (mUserName != "" && mPassword != "" && mDeviceNo != "")
                {
                    mAccount = GlobalVariable.OrderAppLib.AccountService.AccountAuthenticate(mUserName, mPassword, mDeviceNo);

                    if (mAccount != null && mAccount.AccountID != 0)
                    {
                        DTOAccount mDTO = GlobalVariable.OrderAppLib.AccountService.AccountListByID(mAccount.AccountID);

                        mDTO.LastLoginDate = DateTime.Now;

                        GlobalVariable.OrderAppLib.AccountService.AccountSaveRecord(mDTO);

                        GlobalVariable.OrderAppLib.LogService.LogSave("Login Web Service", "Device No " + mDTO.DeviceNo + " has been successfully logged on.", mDTO.AccountID);

                    }

                }
                
                return mAccount;
            }
            catch (Exception ex)
            {
                DTOMobileAccount mAccountEx = new DTOMobileAccount();
                GlobalVariable.OrderAppLib.LogService.LogSave("Login Web Service", ex.Message, 0);
                return mAccountEx;
            }
        }

        
        [WebMethod]
        public DTOMobileAccount RegisterDevice(string mUserName, string mPassword, string mDeviceNo)
        {
            try
            {

                DTOAccount mAccount = GlobalVariable.OrderAppLib.AccountService.AccountAuthenticate(mUserName, mPassword);
                DTOMobileAccount mAccountMobile = new DTOMobileAccount();
                if(mAccount != null)
                if (mAccount.AccountID != 0 && mDeviceNo != "" && mAccount.DeviceNo == "")
                {
                    mAccount = GlobalVariable.OrderAppLib.AccountService.AccountListByID(int.Parse(mAccount.AccountID.ToString()));
                    mAccount.DeviceNo = mDeviceNo;
                    mAccount.DateActivated = DateTime.Now;
                    mAccount.DateUpdated = DateTime.Now;

                    GlobalVariable.OrderAppLib.AccountService.AccountSaveRecord(mAccount);

                    mAccountMobile = Login(mUserName, mPassword, mDeviceNo);

                }
                else {

                    mAccountMobile = Login(mUserName, mPassword, mDeviceNo);
                }

                return mAccountMobile;
            }
            catch (Exception ex)
            {
                DTOMobileAccount mAccountEx = new DTOMobileAccount();
                return mAccountEx;
            }
        }

         [WebMethod]
        public Boolean UnRegisterDevice(string mUserName, string mPassword)
        {
            try
            {
                Boolean result;
                DTOAccount mAccount = GlobalVariable.OrderAppLib.AccountService.AccountAuthenticate(mUserName, mPassword);

                if (mAccount != null)
                {
                    if (mAccount.AccountID != 0)
                    {
                        mAccount = GlobalVariable.OrderAppLib.AccountService.AccountListByID(int.Parse(mAccount.AccountID.ToString()));
                        mAccount.DeviceNo = "";
                        mAccount.OrderLincVersion = "NOT SET";
                        mAccount.iOSVersion = "NOT SET";

                        GlobalVariable.OrderAppLib.AccountService.AccountSaveRecord(mAccount);

                        result = true;

                    }
                    else
                    {
                        result = false;
                    }
                }
                else {
                  result =  false;
                }

                return result;
            }
            catch (Exception ex)
            {
             
                return false;
            }
        }




    }
}