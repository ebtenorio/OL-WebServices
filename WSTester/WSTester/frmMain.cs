using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WSTester
{
    public partial class frmMain : Form
    {
        private static ws_LoginSVC.Service1SoapClient mLoginService;
        private static ws_OrderLinc.mobileservicesSoapClient  mService; 
        private Boolean IsOnline = false;

        private DataSet DSReference = new DataSet();


        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            ClearTextBoxes();

            this.txtUrl.Text = Properties.Settings.Default.serverURL.ToString();
            this.txtLoginUri.Text = Properties.Settings.Default.LoginServerURL.ToString();
            this.txtUsername.Text = Properties.Settings.Default.Username.ToString();
            this.txtPassword.Text = Properties.Settings.Default.Password.ToString();
            this.txtDeviceNo.Text = Properties.Settings.Default.DeviceNo.ToString();
            this.pnlCommands.Enabled = false;
            pnlCommand3.Enabled = false;
            SetButtonState(IsOnline);
            DisplayServerStatus(IsOnline);

            this.pbSignature.Image = DrawText("Signed " + DateTime.Now.ToString("dd.MM.yyyy HH:mm tt"), this.lblConnStatus.Font, System.Drawing.Color.Black, System.Drawing.Color.Gainsboro);
            dtpHoldUntil.Enabled = false;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.txtUrl.Text != "NA")
                {
                    string hostName = this.txtUrl.Text;

                    mService = new ws_OrderLinc.mobileservicesSoapClient();
                    mService.Endpoint.Address = new System.ServiceModel.EndpointAddress(hostName + "mobileservices.asmx");
                   
                    string message = "";

                    bool Result = mService.VersionControl(out message, "2", "2");

                    if (mService.IsOnline())
                    {
                        Properties.Settings.Default.serverURL = this.txtUrl.Text;
                        Properties.Settings.Default.Save();
                        IsOnline = true;
                        this.pnlCommands.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Cannot connect... restoring orginal configuration");
                        this.txtUrl.Text = Properties.Settings.Default.serverURL.ToString();
                        IsOnline = false;
                    }



                }
                else
                {

                }
            }
            catch
            {
                MessageBox.Show("Cannot connect... restoring orginal configuration");
                this.txtUrl.Text = Properties.Settings.Default.serverURL.ToString();
                IsOnline = false;
            }
        }

        private void SetButtonState(Boolean mstate)
        {

            this.pnlCommand2.Enabled = mstate;
        }

        private void DisplayServerStatus(Boolean mstate)
        {
            if (mstate == true)
            {
                lblConnStatus.Text = "Service Online";
                this.lblConnStatus.BackColor = System.Drawing.Color.Green;
                lblConnStatus.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                lblConnStatus.Text = "Service Offline";
                this.lblConnStatus.BackColor = System.Drawing.Color.Red;
                lblConnStatus.ForeColor = System.Drawing.Color.Gray;
            }


        }

        private Image DrawText(String text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width, (int)textSize.Height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;

        }

        private byte[] GetSignatureArray()
        {
            byte[] signatureArray;

            if (this.pbSignature.Image != null)
            {


                //picture to array
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                this.pbSignature.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] bytes = ms.ToArray();
                signatureArray = bytes;

            }
            else
            {
                signatureArray = null;

            }
            return signatureArray;

        }

        private void btnGetAccount_Click(object sender, EventArgs e)
        {
            //this.txtAccountID.Text = "218";
            //this.txtSalesOrgID.Text = "1001";
        }



        #region *********************************** Tests **********************************************************


        private void btnGetReference_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.txtAccountID.Text) || string.IsNullOrEmpty(this.txtSalesOrgID.Text))
                {
                    MessageBox.Show("please provide a Sales Rep No and Sales OrgNo");
                }
                else
                {
                    Int64 saleOrgNo = Int64.Parse(this.txtSalesOrgID.Text);
                    Int64 salesRepNo = Int64.Parse(this.txtAccountID.Text);

                    DataSet mDS = mService.GetReferenceData(saleOrgNo, salesRepNo);

                    this.dgData.DataSource = mDS;
                    DSReference = mDS;

                    populateProvider();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetReference Oops: \n" + ex.Message.ToString());
            }

        }


        #endregion

        private void btnCreateTestOrder_Click(object sender, EventArgs e)
        {
             if (DSReference.Tables.Count > 1)
            {

                this.pbSignature.Image = DrawText("Signed " + DateTime.Now.ToString("dd.MM.yyyy HH:mm tt"), this.lblConnStatus.Font, System.Drawing.Color.Black, System.Drawing.Color.Gainsboro);

                Int64 repNo = Int64.Parse(this.txtAccountID.Text);
                Int64 salesOrgNo = Int64.Parse(this.txtSalesOrgID.Text);
                Int64 custNo = Int64.Parse(this.cboCustomer.SelectedValue.ToString());
                Int64 ProviderNo = Int64.Parse(this.cboProvider.SelectedValue.ToString());
                int ProviderWarehouseNo = int.Parse(this.cboProviderWarehouse.SelectedValue.ToString());

                CreateOrder(repNo, custNo, salesOrgNo, ProviderNo, ProviderWarehouseNo, txtUsername.Text, txtPassword.Text, txtDeviceNo.Text);
                CreateOrder(repNo, custNo, salesOrgNo, ProviderNo, ProviderWarehouseNo, txtUsername.Text, txtPassword.Text, txtDeviceNo.Text);

            }
            else
            {
                MessageBox.Show("duhhh, please do Step 2, get reference data.");
            }

        }




        private void populateProvider()
        {
            if (DSReference.Tables.Count > 1)
            {
                this.cboProvider.DisplayMember = "ProviderName";
                this.cboProvider.ValueMember = "ProviderID";
                this.cboProvider.DataSource = DSReference.Tables["tblProvider"];
            }

        }

        private void populateProviderWarehouse(Int64 mProviderNo)
        {
            DataRow[] mRows = DSReference.Tables["tblProviderWarehouse"].Select("ProviderID=" + mProviderNo);
            DataTable mRes = mRows.CopyToDataTable();

            this.cboProviderWarehouse.DisplayMember = "ProviderWarehouseName";
            this.cboProviderWarehouse.ValueMember = "ProviderWarehouseID";
            this.cboProviderWarehouse.DataSource = mRes;

            //   DataRow mRow = DTResult.NewRow();
            //   mRow["AccountLocation"] = "Select";
            //   mRow["AccountLocationID"] = "0";
            //   mRow["AccountID"] = GlobalVariables.mAccountID;
            //   mRow["PostalCode"] = "0000";
            //   DTResult.Rows.InsertAt(mRow, 0);
            //   cboAccountLocation.SelectedIndex = 0;
        }

        private void cboProvider_SelectedIndexChanged(object sender, EventArgs e)
        {
            Int64 mRecNo = int.Parse(this.cboProvider.SelectedValue.ToString());
            populateProviderWarehouse(mRecNo);
            PopulateCustomer(mRecNo);

        }

        private void PopulateCustomer(Int64 mProviderNo)
        {
            DataRow[] mRows = DSReference.Tables["tblCustomer"].Select("ProviderID=" + mProviderNo);
            DataTable mRes = mRows.CopyToDataTable();
            this.cboCustomer.DisplayMember = "CustomerName";
            this.cboCustomer.ValueMember = "CustomerID";

            this.cboCustomer.DataSource = mRes;
        }

        private void CreateOrder(Int64 repNo, Int64 custNo, Int64 salesOrgNo, Int64 providerNo, int warehouseNo, string uName, string uPW, string uDeviceNo)
        {
            ws_OrderLinc.DTOOrder mOrder = new ws_OrderLinc.DTOOrder();
            mOrder.CreatedByUserID = (int)repNo;
            mOrder.CustomerID = custNo;
            mOrder.OrderDate = DateTime.Today;
            mOrder.DateCreated = DateTime.Today;
            mOrder.DateUpdated = DateTime.Today;
            mOrder.DeliveryDate = DateTime.Today.AddDays(7);
            mOrder.InvoiceDate = DateTime.Today.AddDays(7);
            mOrder.IsHeld = true;
            mOrder.IsSent = false;

            if (this.chkHoldUntil.Checked == true)
            {
                mOrder.HoldUntilDate = this.dtpHoldUntil.Value;
            }
            else
            {
                mOrder.HoldUntilDate = null;
            }

            Guid GUID = Guid.NewGuid();

            mOrder.IsMobile = true;
            mOrder.OrderGUID =  "TESTGUID123452"; // GUID.ToString();
            mOrder.OrderID = 0;
            mOrder.OrderNumber = "42343433";
            mOrder.ProviderID = providerNo;
            mOrder.ProviderWarehouseID = warehouseNo;
            mOrder.ProviderCustomerCode = txtCustomerCode.Text;
            mOrder.ReceivedDate = DateTime.Now;
            mOrder.ReleaseDate = null;
            mOrder.SalesOrgID = salesOrgNo;
            mOrder.SalesRepAccountID = (int)repNo;
            mOrder.SYSOrderStatusID = 106;
            mOrder.UpdatedByUserID = (int)repNo;
            // mOrder.OrderGUID = "d4300334-1682-40b0-8e6a-093186af2dbc";
           

            if (DSReference.Tables["tblProviderProduct"].Rows.Count > 2)
            {
                List<ws_OrderLinc.DTOOrderLine> mOrderLines = new List<ws_OrderLinc.DTOOrderLine>();
                for (int i = 0; i < 2; i++)
                {
                    ws_OrderLinc.DTOOrderLine mLine = new ws_OrderLinc.DTOOrderLine();
                    mLine.OrderLineID = 0;
                    mLine.OrderID = 0;
                    mLine.LineNum = i + 1;
                    mLine.ItemStatus = "";
                    mLine.OrderQty = 1;
                    mLine.OrderPrice = (float)45;
                    mLine.DespatchQty = 0;
                    mLine.DespatchPrice = 0;
                    mLine.ErrorText = "";
                    DataRow mRow = DSReference.Tables["tblProviderProduct"].Rows[i];
                    mLine.ProductID = Int64.Parse(mRow["ProductID"].ToString());
                    mLine.UOM = mRow["ProductUOM"].ToString();
                    mOrderLines.Add(mLine);
                }

                mOrder.OrderLine = mOrderLines.ToArray();
            }

            //get byte array for signature
            byte[] signatureArray = GetSignatureArray();

            //create contact info
            ws_OrderLinc.DTOContact mContact = new ws_OrderLinc.DTOContact();

            mContact.ContactID = int.Parse(this.txtContactID.Text);
            mContact.FirstName = this.txtFirstName.Text;
            mContact.LastName = this.txtLastName.Text;
            mContact.Phone = this.txtPhone.Text;
            mContact.Email = this.txtEmail.Text;
            mContact.Fax = this.txtFax.Text;
            mContact.Mobile = this.txtMobile.Text;

            // USE THIS if no updates to be sent
            // mContact = null; 

            string mServerMessage = "";

            Int64 mRes = mService.SubmitOrder(mOrder, signatureArray, mContact, uName, uPW, uDeviceNo, out mServerMessage);

            this.txtOrderID.Text = mRes.ToString();

            if (mServerMessage != "OK")
            {
                MessageBox.Show(mServerMessage);
            }
            else
            {
                this.Text = mServerMessage;
            }

        }

        private void chkHoldUntil_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkHoldUntil.Checked == true)
            {
                dtpHoldUntil.Enabled = true;
            }
            else
            {
                dtpHoldUntil.Enabled = false;
            }
        }


        private void cboCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            Int64 mCustNo = Int64.Parse(this.cboCustomer.SelectedValue.ToString());

            PopulateContact(mCustNo);
        }


        private void PopulateContact(Int64 customerNo)
        {
            ClearTextBoxes();
            if (DSReference.Tables["tblCustomer"].Rows.Count > 0)
            {
                DataRow[] mRows = DSReference.Tables["tblCustomer"].Select("CustomerID =" + customerNo);

                if (mRows.Length > 0)
                {
                    DataRow mRow = mRows[0]; //only has 1 contact per customer
                    this.txtContactID.Text = mRow["ContactID"].ToString();
                    this.txtCustomerCode.Text = mRow["ProviderCustomerCode"].ToString();
                    this.txtFirstName.Text = mRow["FirstName"].ToString();
                    this.txtLastName.Text = mRow["LastName"].ToString();
                    this.txtEmail.Text = mRow["Email"].ToString();
                    this.txtFax.Text = mRow["Fax"].ToString();
                    this.txtMobile.Text = mRow["Mobile"].ToString();
                    this.txtPhone.Text = mRow["Phone"].ToString();
                }

            }
        }

        private void ClearTextBoxes()
        {
            this.txtContactID.Text = "0";
            this.txtFirstName.Text = "";
            this.txtLastName.Text = "";
            this.txtMobile.Text = "";
            this.txtFax.Text = "";
            this.txtPhone.Text = "";
            this.txtEmail.Text = "";
            this.txtCustomerCode.Text = "";

        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnTestLoginUri_Click(object sender, EventArgs e)
        {
           
            
            if (this.txtLoginUri.Text != "NA")
            {
                string hostName = this.txtLoginUri.Text;

                IsOnline = false;
                mLoginService = new ws_LoginSVC.Service1SoapClient("LoginServiceSoap");
                mLoginService.Endpoint.Address = new System.ServiceModel.EndpointAddress(hostName + "OrderLincRegistry.asmx");

                try
                {
                    string errorMessage = "";

                    Boolean mVersionResult = mLoginService.VersionControl(out errorMessage,"2","2" );

                  
                    ws_LoginSVC.DTOMobileAccount mAccount = mLoginService.Login("", "", "");
                    if (mAccount.AccountID == 0)
                    {
                        Properties.Settings.Default.LoginServerURL = this.txtLoginUri.Text;
                        Properties.Settings.Default.Save();

                        IsOnline = true;
                    }
                    else
                    {
                        MessageBox.Show("Cannot connect... restoring orginal configuration");
                        this.txtLoginUri.Text = Properties.Settings.Default.LoginServerURL.ToString();
                        IsOnline = false;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Cannot connect... restoring orginal configuration");
                    this.txtLoginUri.Text = Properties.Settings.Default.LoginServerURL.ToString();
                    IsOnline = false;
                }

                SetButtonState(IsOnline);
                DisplayServerStatus(IsOnline);
            }
            else
            {

            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.txtLoginUri.Text != "NA")
            {
                string hostName = this.txtLoginUri.Text;

                IsOnline = false;
                mLoginService = new ws_LoginSVC.Service1SoapClient("LoginServiceSoap");
                mLoginService.Endpoint.Address = new System.ServiceModel.EndpointAddress(hostName + "OrderLincRegistry.asmx");

                try
                {
                    ws_LoginSVC.DTOMobileAccount mAccount = mLoginService.Login(txtUsername.Text, txtPassword.Text, txtDeviceNo.Text);
                    if (mAccount != null && mAccount.AccountID != 0)
                    {
                        Properties.Settings.Default.LoginServerURL = this.txtLoginUri.Text;
                        Properties.Settings.Default.Username = txtUsername.Text;
                        Properties.Settings.Default.Password = txtPassword.Text;
                        Properties.Settings.Default.DeviceNo = txtDeviceNo.Text;
                        txtUrl.Text = mAccount.ServerUrl.Replace("mobileservices.asmx", "");
                        Properties.Settings.Default.serverURL = txtUrl.Text;
                        Properties.Settings.Default.Save();
                        pnlCommand3.Enabled = true;
                        IsOnline = true;
                    }
                    else
                    {
                        MessageBox.Show("Invalid credentials");
                        this.txtLoginUri.Text = Properties.Settings.Default.LoginServerURL.ToString();
                        IsOnline = false;
                    }


                }
                catch
                {
                    MessageBox.Show("Cannot connect... restoring orginal configuration");
                    this.txtLoginUri.Text = Properties.Settings.Default.LoginServerURL.ToString();
                    IsOnline = false;
                }


            }
            else
            {

            }

        }






    } // class
} // ns
