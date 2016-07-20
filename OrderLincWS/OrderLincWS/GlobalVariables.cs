using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OrderLinc;
using System.Configuration;
using OrderLinc.DTOs;
using System.IO;

namespace OrderLincWS
{
    public class GlobalVariables
    {
        // public static string AppVersion = "1";

        //publish \\192.168.50.24\OrderLincWS

        public static OrderLincServices mService;
        public static string SignaturePath;
        static string AuthenticatioServiceUri;
        public static string Major;
        public static string Minor;

        public static AuthenticationWS.Service1SoapClient sv_Authentication; 


        public void SetVariables()
        {

            mService = new OrderLincServices(ConfigurationManager.AppSettings["DBServer"], ConfigurationManager.AppSettings["DBName"], false, ConfigurationManager.AppSettings["DBUsername"], ConfigurationManager.AppSettings["DBPassword"]);

            
            //signatureimagepath

            DTOSYSConfig mDTO = mService.ConfigurationService.SYSConfigListByKey("SignatureImagePath");
            SignaturePath = mDTO.ConfigValue.ToString();
            PrepareWorkspaceDirectories(SignaturePath);

            //login service uri
            mDTO = mService.ConfigurationService.SYSConfigListByKey("AuthenticationWS");
            AuthenticatioServiceUri = mDTO.ConfigValue.ToString();


            sv_Authentication = new AuthenticationWS.Service1SoapClient();
            sv_Authentication.Endpoint.Address = new System.ServiceModel.EndpointAddress(AuthenticatioServiceUri);


        }

        private void PrepareWorkspaceDirectories(string mPath)
        {
            DirectoryInfo dirInfo = null;
            if (!Directory.Exists(mPath))
            {
                dirInfo = Directory.CreateDirectory(mPath);
            }
        }


        public static string MajorVersion
        {

            get { return System.Configuration.ConfigurationManager.AppSettings["VersionMajor"]; }
        }


        public static string MinorVersion
        {

            get { return System.Configuration.ConfigurationManager.AppSettings["VersionMinor"]; }
        }


    } //class
} // ns