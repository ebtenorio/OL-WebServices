using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using OrderLinc.DTOs;
using System.Data;
using System.IO;
using System.Transactions;
using PTI;
using OrderLinc.IDataContracts;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace OrderLincWS
{
    /// <summary>
    /// Summary description for mobileservices
    /// </summary>
    [WebService(Namespace = "http://4solutions.co.au/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class mobileservices : System.Web.Services.WebService
    {

        EmailSVC _emailService;

        string mSourceName = "Order Web Service";
        string MailHost = "";
        string MailSender = "";
        int MailPort = 0;
        string MailUserName = "";
        string MailPassword = "";
        string MailCC = "";
        string MailUseDefaultCredential = "";
        bool mMailUseDefaultCredential = false;
        string EmailFooter = "";

        [WebMethod(Description = "Returns true, webservice is active.")]
        public Boolean IsOnline()
        {
            return true;
        }

        public DTOSYSConfig ServiceInterval { get; private set; }


        #region **********************Email notification ***************
        [WebMethod(Description = "Test email notification.")]
        public Boolean EmailNotification(string Email, out string ErrorMessage)
        {
            try
            {
                Boolean result = false;
                ErrorMessage = "";

                if (InitEmailService(out ErrorMessage))
                {

                    if (IsValidEmail(Email))
                    {
                        SendMail(Email, "Test Email Notification", "Test Email Notificaiton");
                        result = true;
                    }
                    else
                    {

                        result = false;
                        ErrorMessage = "Invalid email";
                    }
                    ErrorMessage = "Successfully Sent";

                }
                else
                {

                    result = false;

                }
                return result;
            }
            catch (Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                ErrorMessage = ex.Message;
                return false;
            }
        }


        private bool SendMail(string email, string body, string subject)
        {

            bool ret = false;

            if (string.IsNullOrEmpty(email))
                return ret;

            if (IsValidEmail(email))
            {
                ret = _emailService.SendMail(email, "", subject, body);
                if (ret == false)
                {

                }

            }
            return ret;
        }

        private bool IsValidEmail(string emailAddress)
        {

            // Return true if emailAddress is in valid e-mail format.
            try
            {
                return Regex.IsMatch(emailAddress, @"^([0-9a-zA-Z]([-\.\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9})$");
            }
            catch (Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                return false;
            }
        }

        private Boolean InitEmailService(out string ErrorMessageInit)
        {
            try
            {
                Boolean res = false;
                DTOSYSConfig mConfig = new DTOSYSConfig();

                DTOSYSConfigList mDTOSYSConfigList = GlobalVariables.mService.ConfigurationService.SYSConfigList();

                ServiceInterval = mDTOSYSConfigList.Where(p => p.ConfigKey.ToLower() == "notifyinterval").FirstOrDefault();

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailHost" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailHost = mConfig.ConfigValue.ToString();
                    res = true;
                }
                else
                {

                    ErrorMessageInit = "MailHost Null";
                    res = false;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailSender" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailSender = mConfig.ConfigValue.ToString();
                    res = true;
                }
                else
                {
                    ErrorMessageInit = "MailSender Null";
                    res = false;
                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailPort" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailPort = int.Parse(mConfig.ConfigValue.ToString());
                    res = true;
                }
                else
                {
                    ErrorMessageInit = "MailPort Null";
                    res = false;

                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailUserName" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailUserName = mConfig.ConfigValue.ToString();
                    res = true;
                }
                else
                {
                    ErrorMessageInit = "MailUserName Null";
                    res = false;


                }

                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailPassword" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailPassword = mConfig.ConfigValue.ToString();
                    res = true;
                }
                else
                {
                    ErrorMessageInit = "MailPassword Null";
                    res = false;


                }



                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailCC" select sysconfig).FirstOrDefault();

                if (mConfig != null)
                {
                    MailCC = mConfig.ConfigValue.ToString();
                    res = true;
                }
                else
                {
                    res = true;
                }


                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "MailUseDefaultCredential" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    MailUseDefaultCredential = mConfig.ConfigValue.ToString();


                    if (MailUseDefaultCredential == "0")
                    {
                        mMailUseDefaultCredential = false;
                    }
                    else
                    {
                        mMailUseDefaultCredential = true;
                    }

                    res = true;
                }
                else
                {
                    ErrorMessageInit = "MailUseDefaultCredential Null";
                    res = false;
                }


                mConfig = (from sysconfig in mDTOSYSConfigList where sysconfig.ConfigKey == "EmailFooter" select sysconfig).FirstOrDefault();
                if (mConfig != null)
                {
                    EmailFooter = mConfig.ConfigValue.ToString();
                    res = true;
                }
                else
                {

                    res = true;


                }



                _emailService = new PTI.EmailSVC(MailHost.Trim(), MailPort, mMailUseDefaultCredential, MailUserName.Trim(), MailPassword.Trim(), MailSender.Trim());

                ErrorMessageInit = "OK";
                return res;
            }
            catch (Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                ErrorMessageInit = ex.Message;
                return false;

            }
        }

        # endregion **************************************


        [WebMethod(Description = "Check Application Version")]
        public Boolean VersionControl(string VersionMajor, string VersionMinor, out string ErroMessage)
        {

            Boolean result = false;

            if (GlobalVariables.MajorVersion == VersionMajor) // && GlobalVariable.Minor == VersionMinor - As requested only major version should be evaluated
            {

                result = true;
                ErroMessage = "Version matched.";
            }
            else
            {

                result = false;

                ErroMessage = "Connection rejected. OrderLinc must be updated to the latest version.";

            }

            result = true;//return true for the meantime
            return result; 
        }

        [WebMethod(Description = "gets reference data for a sales Organization")]
        public DataSet GetReferenceData(Int64 salesOrgNo, Int64 repAccountNo, string mOrderLincVersion, string mIOSVersion)
        {
            DataSet mDS = new DataSet();
            DTOAccount _salesRepAccount = new DTOAccount();

            _salesRepAccount = GlobalVariables.mService.AccountService.AccountListByID(repAccountNo);

            _salesRepAccount.OrderLincVersion = mOrderLincVersion;
            _salesRepAccount.iOSVersion = mIOSVersion;

            GlobalVariables.mService.AccountService.AccountSaveRecord(_salesRepAccount);

            mDS = GlobalVariables.mService.CatalogService.GetReferenceData(salesOrgNo, repAccountNo);

            // From 6 to 7
            if (mDS.Tables.Count == 7)
            {
                mDS.Tables[0].TableName = "tblProviderProduct";
                mDS.Tables[1].TableName = "tblCustomer";
                mDS.Tables[2].TableName = "tblProvider";
                mDS.Tables[3].TableName = "tblProviderWarehouse";
                mDS.Tables[4].TableName = "tblProductGroup";
                mDS.Tables[5].TableName = "tblProductGroupLine";
                mDS.Tables[6].TableName = "tblProductUOM";
            }

            return mDS;
        }

        private Int64 checkifOrderGuiDalreadyexist(string GuiD, out string OrderNumber)
        {
            OrderNumber = "0";
            try
            {
                
                Int64 result = 0;
                DTOOrder mDTO = GlobalVariables.mService.OrderService.OrderListByGUIDID(GuiD);               

                if (mDTO == null)
                {

                    result = 0;

                }
                else
                {
                    result = mDTO.OrderID;
                    OrderNumber = mDTO.OrderNumber;
                }

                return result;
            }
            catch (Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                return 0;
            }
        }

        private DateTime ConvertUTCToLocalTime(DateTime pUTCDateTime)
        {
            DateTime _UTCDateTime = new DateTime(pUTCDateTime.Year,
                                                 pUTCDateTime.Month,
                                                 pUTCDateTime.Day,
                                                 pUTCDateTime.Hour,
                                                 pUTCDateTime.Minute,
                                                 pUTCDateTime.Second,
                                                 DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(_UTCDateTime, TimeZoneInfo.Local);
        }

        [WebMethod(Description = "Accepts mobile orders and the signature image array associated with it.")]
        public Int64 SubmitOrder(DTOOrder order, byte[] imageArray, out string ServerMessage, DTOContact contact, string uName, string uPW, string deviceNo)
        {
            Int64 objectID = 0;
            long OrderNumber = 0;
            try
            {
                //Check if GUID / order already exist 
                try
                {
                    string _OrderNumber = "";

                    this.WriteEventLog(mSourceName, "Checking GUID " + order.OrderGUID, order, EventLogEntryType.Information);

                    Int64 mOrderID = checkifOrderGuiDalreadyexist(order.OrderGUID, out _OrderNumber);

                    if (mOrderID != 0)
                    {
                        ServerMessage = "GUID already exist";
                        return mOrderID; // originally coded by Ringo
                        //return long.Parse(_OrderNumber); // changed by Eldon -- Order.OrderNumber is NULL so exception.
                    }

                    if (order.OrderLine.Count == 0)
                    {
                        this.WriteEventLog(mSourceName, "No orderlines for Order with GUID " + order.OrderGUID, order, EventLogEntryType.Information);
                        ServerMessage = "Please add order line items";
                        return 0;
                    }

                }
                catch (Exception ex)
                {
                    this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                    throw new ApplicationException("Error on order GUID validation.\n\n" + ex.Message);
                }

                // Get Max Order Number - Ringo Ray Piedraverde 9-4-2014

                OrderNumber = GlobalVariables.mService.CatalogService.GetMaxProductNumberbySalesOrg(int.Parse(order.SalesOrgID.ToString()));


                // Increment Ordernumber by one
                if (OrderNumber == 0)
                    OrderNumber = 0;

                
                for (int i = 0; i <= 100; i++)   // Check for OrderNumber by SalesOrg if already exist - Ringo Ray Piedraverde 9-9-2014 
                {
                    OrderNumber = OrderNumber + 1;
                    Boolean result = GlobalVariables.mService.CatalogService.CheckProductNumberIfExistBySalesOrg(int.Parse(order.SalesOrgID.ToString()), OrderNumber);                    

                    if (result == true)
                    {
                    
                        if (i == 100)
                        {

                            ServerMessage = "Check Product Number Already exist - Order cannot be saved. Please try again";
                            DTOLog mDTO = new DTOLog();

                            mDTO.CreatedByUserID = order.CreatedByUserID;
                            mDTO.Source = "Web Service";
                            mDTO.LogID = 0;
                            mDTO.Description = "Check Product Number Already exist - Order cannot be saved. Please try again";
                            mDTO.DateCreated = DateTime.Now;

                            GlobalVariables.mService.LogService.LogSave(mDTO);
                            this.WriteEventLog(mSourceName, ServerMessage, EventLogEntryType.Warning);
                            return 0;
                        }

                    }
                    else
                    {
                        order.OrderNumber = OrderNumber.ToString();
                        break;

                    }


                }


                // convert UAT to server local time - expect the mobile device to send an Order datetime in UTC.
                try
                {
                    order.OrderDate = ConvertUTCToLocalTime(order.OrderDate);
                }
                catch (Exception ex)
                {
                    this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                    throw new ApplicationException("Error on converting UTC time to serverlocal time.");
                }

                ServerMessage = "OK";
                using (TransactionScope mScope = new TransactionScope()) // TRANSACTION DEPENDENCIES
                {
                    ////1. authenticate user 
                    //AuthenticationWS.DTOMobileAccount mAccount = new AuthenticationWS.DTOMobileAccount();
                    //try
                    //{
                    //    mAccount = GlobalVariables.sv_Authentication.Login(uName, uPW, deviceNo);

                    //    if (mAccount == null || (mAccount != null && mAccount.AccountID == 0))
                    //    {
                    //        throw new ApplicationException("Invalid or Expired User Credentials.");
                    //    }
                    //}
                    //catch (Exception _ex)
                    //{
                    //    this.WriteEventLog(mSourceName, _ex, EventLogEntryType.Error);
                    //    throw new ApplicationException("Connection to Authentication Server Failed. " + _ex.Message);
                    //}


                    // 3.5 Update Status Rules base on autorelease configuration of hold unitl and Sales Org IsOrderHeld Rules
                    // Will this be replaced by a script that runs on a scheduled time? 
                    if (order.HoldUntilDate.HasValue)
                    {
                        order.SYSOrderStatusID = 100; // set as received
                    }
                    else // check if want autosends
                    {
                        try
                        {
                            DTOSalesOrg mOrg = GlobalVariables.mService.CustomerService.SalesOrgListByID(order.SalesOrgID);
                            if (mOrg.IsOrderHeld == false) // Does allow
                            {
                                // Update 15 April 2015 for the new auto release function
                                // If the order is REGULAR and its REQUESTED RELEASE DATE IS EQUAL TO NOW:

                                //GlobalVariables.mService.LogService.LogSave(mSourceName, ((bool)order.IsRegularOrder).ToString(), order.SalesRepAccountID);

                                if ((bool)order.IsRegularOrder)           
                                {                                         
                                    // DO THE CORRECTION HERE
                                    // if (string.Format("{0:dd/MM/yyyy}", order.RequestedReleaseDate) == string.Format("{0:dd/MM/yyyy}", DateTime.Now))
                                    if ( ((DateTime)order.RequestedReleaseDate).Date <= DateTime.Now.Date)
                                    {
                                        order.SYSOrderStatusID = 102; // set as released
                                        order.IsHeld = false;
                                        order.ReleaseDate = DateTime.Now;
                                    }
                                }
                            }
                            else // Does not allow
                            {
                                order.SYSOrderStatusID = 100; //set as received
                            }

                        }
                        catch (Exception _ex)
                        {
                            this.WriteEventLog(mSourceName, _ex, EventLogEntryType.Error);
                            throw new ApplicationException("Error fetching Sales Organization Auto Release rules.");
                        }
                    }

                    // 4. Add Order Record  and Image to File.

                    order = GlobalVariables.mService.OrderService.OrderSaveRecord(order);
                    
                    // objectID = order.OrderID;  Reintegration return orderNumber instead of orderID - Ringo Ray Piedraverde 9-9-2014

                    objectID = OrderNumber;
                    if (objectID > 0) //successfully added?
                    {
                        foreach (DTOOrderLine mDTO in order.OrderLine)
                        {
                            mDTO.OrderID = order.OrderID; 
                            mDTO.OrderLineID = 0;

                            string msg = string.Empty;

                            try
                            {

                                if (GlobalVariables.mService.OrderService.OrderLineIsValid(mDTO, out msg))
                                {                                    
                                    try
                                    {
                                        GlobalVariables.mService.OrderService.OrderLineSaveRecord(mDTO);
                                    }
                                    catch (Exception _ex2)
                                    {
                                        ServerMessage = _ex2.Message;
                                        throw _ex2;
                                    }
                                }
                                else
                                {
                                    this.WriteEventLog(mSourceName, msg, order, EventLogEntryType.Error);
                                    throw new ApplicationException(string.Format("Order line validation error '{0}'.", msg));
                                }
                            }
                            catch (Exception _ex)
                            {
                                this.WriteEventLog(mSourceName, _ex, order, EventLogEntryType.Error);
                                throw new ApplicationException(string.Format("Error saving line number {0}. \n\r" + _ex.Message, mDTO.LineNum));
                            }
                        }
                        // save image record and file
                        if (SaveSignatureArray(order.OrderID, imageArray))
                        {
                        }
                        else
                        {                            
                            throw new ApplicationException("Error saving signature");
                        }

                        // 2. Update Contact 
                        DTOCustomer mCustomer = GlobalVariables.mService.CustomerService.CustomerListByID(order.ProviderID, order.CustomerID);
                        if (contact != null) //Add update contact
                        {
                            long contactID = contact.ContactID;
                            DTOContact contactPrev = new DTOContact();
                            if (contactID != 0)
                            {
                                 contactPrev = GlobalVariables.mService.CustomerService.ContactListByID(contactID);

                                contact.OldID = contactPrev.OldID;
                            }

                            if (!AddOrUpdateContact(contact))
                            {
                                ServerMessage = "Error adding contact.";
                            }
                            else
                            {// update tblcustomer contactID if new
                                // Added by Ringo Ray Piedraverde 5/26/14
                                if (contactID == 0)
                                {

                                    mCustomer.ContactID = contact.ContactID;
                                    mCustomer.DateUpdated = DateTime.Now;

                                    GlobalVariables.mService.CustomerService.CustomerSaveRecord(mCustomer);
                                }
                                else if (CheckifCustomerContactisChanged(contactPrev, contact) == false || mCustomer.CustomerCode.Trim() != order.ProviderCustomerCode.Trim())
                                {
                                    mCustomer.DateUpdated = DateTime.Now;
                                    GlobalVariables.mService.CustomerService.CustomerSaveRecord(mCustomer);
                                }

                            }
                        }


                        // 3.Check if customer code has been change and update customer code snippet here.
                        if (order.ProviderCustomerCode != null)
                        {
                            try
                            {
                                GlobalVariables.mService.ProviderService.ProviderUpdateCustomerCode(order.CustomerID, order.ProviderID, order.ProviderCustomerCode);
                            }
                            catch (Exception _ex)
                            {
                                this.WriteEventLog(mSourceName, _ex, EventLogEntryType.Error);
                                throw new ApplicationException("Error updating provider customer code.");
                            }
                        }


                    }
                    else
                    {
                        throw new ApplicationException("Error saving order.");
                    }


                    ////////////////////////////// REMOVE ///////////////////////////////////////////////////////// Begin

                    //order = GlobalVariables.mService.OrderService.OrderSaveRecord(order);

                    //// objectID = order.OrderID;  Reintegration return orderNumber instead of orderID - Ringo Ray Piedraverde 9-9-2014

                    //objectID = OrderNumber;
                    //if (objectID > 0) //successfully added?
                    //{
                    //    foreach (DTOOrderLine mDTO in order.OrderLine)
                    //    {
                    //        mDTO.OrderID = order.OrderID;
                    //        mDTO.OrderLineID = 0;

                    //        string msg = string.Empty;

                    //        try
                    //        {

                    //            if (GlobalVariables.mService.OrderService.OrderLineIsValid(mDTO, out msg))
                    //            {
                    //                try
                    //                {
                    //                    GlobalVariables.mService.OrderService.OrderLineSaveRecord(mDTO);
                    //                }
                    //                catch (Exception _ex2)
                    //                {
                    //                    ServerMessage = _ex2.Message;
                    //                    throw _ex2;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                this.WriteEventLog(mSourceName, msg, order, EventLogEntryType.Error);
                    //                throw new ApplicationException(string.Format("Order line validation error '{0}'.", msg));
                    //            }
                    //        }
                    //        catch (Exception _ex)
                    //        {
                    //            this.WriteEventLog(mSourceName, _ex, order, EventLogEntryType.Error);
                    //            throw new ApplicationException(string.Format("Error saving line number {0}. \n\r" + _ex.Message, mDTO.LineNum));
                    //        }
                    //    }
                    //    // save image record and file
                    //    if (SaveSignatureArray(order.OrderID, imageArray))
                    //    {
                    //    }
                    //    else
                    //    {
                    //        throw new ApplicationException("Error saving signature");
                    //    }


                    //    // REMOVE

                    //    // REMOVE


                    //    // 2. Update Contact 
                    //    DTOCustomer mCustomer = GlobalVariables.mService.CustomerService.CustomerListByID(order.ProviderID, order.CustomerID);
                    //    if (contact != null) //Add update contact
                    //    {
                    //        long contactID = contact.ContactID;
                    //        DTOContact contactPrev = new DTOContact();
                    //        if (contactID != 0)
                    //        {
                    //            contactPrev = GlobalVariables.mService.CustomerService.ContactListByID(contactID);

                    //            contact.OldID = contactPrev.OldID;
                    //        }

                    //        if (!AddOrUpdateContact(contact))
                    //        {
                    //            ServerMessage = "Error adding contact.";
                    //        }
                    //        else
                    //        {// update tblcustomer contactID if new
                    //            // Added by Ringo Ray Piedraverde 5/26/14
                    //            if (contactID == 0)
                    //            {

                    //                mCustomer.ContactID = contact.ContactID;
                    //                mCustomer.DateUpdated = DateTime.Now;

                    //                GlobalVariables.mService.CustomerService.CustomerSaveRecord(mCustomer);
                    //            }
                    //            else if (CheckifCustomerContactisChanged(contactPrev, contact) == false || mCustomer.CustomerCode.Trim() != order.ProviderCustomerCode.Trim())
                    //            {
                    //                mCustomer.DateUpdated = DateTime.Now;
                    //                GlobalVariables.mService.CustomerService.CustomerSaveRecord(mCustomer);
                    //            }

                    //        }
                    //    }


                    //    // 3.Check if customer code has been change and update customer code snippet here.
                    //    if (order.ProviderCustomerCode != null)
                    //    {
                    //        try
                    //        {
                    //            GlobalVariables.mService.ProviderService.ProviderUpdateCustomerCode(order.CustomerID, order.ProviderID, order.ProviderCustomerCode);
                    //        }
                    //        catch (Exception _ex)
                    //        {
                    //            this.WriteEventLog(mSourceName, _ex, EventLogEntryType.Error);
                    //            throw new ApplicationException("Error updating provider customer code.");
                    //        }
                    //    }


                    //}
                    ////////////////////////////// REMOVE ///////////////////////////////////////////////////////// End



                    mScope.Complete();
                } //end using


                if (objectID != 0)
                {
                    this.WriteEventLog(mSourceName, string.Format("Returning Order No {0} for GUID {1} for AccountID {2}.", objectID, order.OrderGUID, order.SalesRepAccountID), EventLogEntryType.Information);
                }

                return objectID;

            }
            catch (Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                ServerMessage = ex.Message.ToString();
                return 0;
            }

        }

        private Boolean CheckifCustomerContactisChanged(DTOContact mPrev, DTOContact mNewContact)
        {
            try
            {
                Boolean result = true;

                // DTOContact mPrev = GlobalVariables.mService.CustomerService.ContactListByID(ContactID);

                if (mPrev == null)
                    return false;

                if (mPrev.Email != mNewContact.Email)
                {
                    result = false;
                }


                if (mPrev.FirstName != mNewContact.FirstName)
                {
                    result = false;
                }

                if (mPrev.LastName != mNewContact.LastName)
                {
                    result = false;
                }

                if (mPrev.Phone != mNewContact.Phone)
                {
                    result = false;
                }

                return result;
            }
            catch(Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                return false;
            }

        }

        private Boolean SaveSignatureArray(Int64 orderNo, byte[] signatureArray)
        {
            Boolean mRes = true;
            string mfilename = Guid.NewGuid().ToString() + ".png";

            string fullfilename = System.IO.Path.Combine(GlobalVariables.SignaturePath, mfilename);

            FileStream file = new FileStream(fullfilename, FileMode.Create);
            file.Write(signatureArray, 0, signatureArray.Length);
            file.Close();
            file.Dispose();

            DTOOrderSignature mDTO = new DTOOrderSignature();

            mDTO.OrderID = orderNo;
            mDTO.Path = mfilename;
            mDTO.DateCreated = DateTime.Now;

            GlobalVariables.mService.OrderService.OrderSignatureSaveRecord(mDTO);

            return mRes;
        }

        private Boolean AddOrUpdateContact(DTOContact mDTOContact)
        {
            Boolean mRes = true;
            try
            {
                GlobalVariables.mService.CustomerService.ContactSaveRecord(mDTOContact);
                return mRes;
            }
            catch (Exception ex)
            {
                this.WriteEventLog(mSourceName, ex, EventLogEntryType.Error);
                return false;
            }

        }

        [WebMethod(Description = "Creates error")]
        public int CreateError()
        {
            Context.Response.StatusCode = 400;
            Context.Response.StatusDescription = "Error";

            return 10;
        }

        [WebMethod(Description = "Check The Last Release Date of the Web Service")]
        public string LastReleaseDate()
        {
            string result = "";
            DTOSYSConfig _config = new DTOSYSConfig();
            _config = GlobalVariables.mService.ConfigurationService.SYSConfigListByKey("LastVersionReleaseDate");

            if (_config != null)
            {
                result = _config.ConfigValue;
            }

            return result;
        }

        // Additional overloaded method for WriteEventLog - Eldon 2015-02-26 11:50PHT
        private void WriteEventLog(String sourceName, Exception exception, DTOOrder order, EventLogEntryType logType)
        {            
            GlobalVariables.mService.LogService.LogSave(sourceName, exception.Message, order.SalesRepAccountID);
        }

        private void WriteEventLog(String sourceName, String message, DTOOrder order, EventLogEntryType logType)
        {
            GlobalVariables.mService.LogService.LogSave(sourceName, message, order.SalesRepAccountID);
        }

        private void WriteEventLog(String sourceName, Exception exception, EventLogEntryType logType)
        {
            GlobalVariables.mService.LogService.LogSave(sourceName, exception.Message, 0);
        }

        private void WriteEventLog(String sourceName, String message, EventLogEntryType logType)
        {
            GlobalVariables.mService.LogService.LogSave(sourceName, message, 0);
        }


    } //class
} //ns
