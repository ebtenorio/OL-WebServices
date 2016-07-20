using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PL.PersistenceServices.DTOS;



namespace OrderLincRegistryWebservice
{
    public class GlobalVariable
    {
        public static OrderLinc.OrderLincServices mOrderApp;
        public static string _registryService;
      
        public static OrderLinc.OrderLincServices OrderAppLib
        {
            get
            {


                string mServerName = System.Configuration.ConfigurationManager.AppSettings["ServerName"];
                string mDatabaseName = System.Configuration.ConfigurationManager.AppSettings["Database"];
                string mUserName = System.Configuration.ConfigurationManager.AppSettings["Username"];
                string mPassword = System.Configuration.ConfigurationManager.AppSettings["Password"];
                string mAuthenticationType = System.Configuration.ConfigurationManager.AppSettings["AuthenticationType"].ToString();

                bool AuthenticationType = false;
                if (mAuthenticationType == "1")
                {
                    //mConfig.AuthenticationType = PL.PersistenceServices.Enumerations.DatabaseAuthenticationTypes.ServerAuthentication;
                    AuthenticationType = false;
                }
                else
                {
                    AuthenticationType = true;

                }
                mOrderApp = new OrderLinc.OrderLincServices(mServerName, mDatabaseName, AuthenticationType, mUserName, mPassword);

                

                return mOrderApp;
            }


        }

        public static string RegistryWebserviceURL
        {

            get { return _registryService; }

        }

        public static string MajorVersion {

            get { return System.Configuration.ConfigurationManager.AppSettings["VersionMajor"]; }
        }


        public static string MinorVersion {

            get { return System.Configuration.ConfigurationManager.AppSettings["VersionMinor"]; }
        }


    }
}