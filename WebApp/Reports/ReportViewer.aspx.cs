using System;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Views.Shared
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                var cId = User.Identity.GetUserId();
            }
        }

        private void render2PDF(string _paperType, string _report)
        {
            try
            {
                string _deviceInfo = @"<DeviceInfo><MarginLeft>0in</MarginLeft><MarginRight>0in</MarginRight><MarginTop>0in</MarginTop><MarginBottom>0in</MarginBottom>";
                if (_paperType == "PortraitA4")
                    _deviceInfo = _deviceInfo + @"<PageWidth>8.3in</PageWidth><PageHeight>11.7in</PageHeight>";
                else if (_paperType == "LandscapeA4")
                    _deviceInfo = _deviceInfo + @"<PageWidth>11.7in</PageWidth><PageHeight>8.3in</PageHeight>";
                else if (_paperType == "PortraitLegal")
                    _deviceInfo = _deviceInfo + @"<PageWidth>8.5in</PageWidth><PageHeight>14in</PageHeight>";
                else if (_paperType == "LandscapeLegal")
                    _deviceInfo = _deviceInfo + @"<PageWidth>14in</PageWidth><PageHeight>8.5in</PageHeight>";
                else if (_paperType == "PortraitLetter")
                    _deviceInfo = _deviceInfo + @"<PageWidth>8.5in</PageWidth><PageHeight>11in</PageHeight>";
                else if (_paperType == "LandscapeLetter")
                    _deviceInfo = _deviceInfo + @"<PageWidth>11in</PageWidth><PageHeight>8.5in</PageHeight>";
                _deviceInfo = _deviceInfo + @"</DeviceInfo>";

                this.Title = _report;

                string[] _streams = { "" };
                Warning[] _warnings = null;
                string _mimeType = null;
                string _encoding = null;
                string _fileExtension = null;
                byte[] bytes;

                bytes = this.ReportViewer1.LocalReport.Render("PDF", _deviceInfo, out _mimeType, out _encoding, out _fileExtension, out _streams, out _warnings);
                Response.Buffer = false;
                Response.BufferOutput = false;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.AppendHeader("Content-Disposition", "inline;filename=" + _report + "." + _fileExtension);
                Response.ContentType = _mimeType;
                Response.BinaryWrite(bytes);
                Response.End();

            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                try
                {
                    string rep = Request.QueryString["rep"].ToString();
                    string json = Request.QueryString["json"].ToString();

                    DateTime dt = DateTime.Today;
                    var arr = JsonConvert.DeserializeObject<ControllerParam[]>(json).SingleOrDefault();

                    //string fileName = "";

                    using (WebAppEntities db = new WebAppEntities())
                    {
                        using (var dbContextTransaction = db.Database.BeginTransaction())
                        {
                            if (rep == "t2xf1F10jklxM30923llkj")
                            {
                                IQueryable<VW_VoucherStatus> source = db.VW_VoucherStatus.
                                OrderBy(x => x.TemplateID).OrderBy(x => x.Amount).OrderBy(x => x.VoucherStatus);

                                //param2:
                                if (!string.IsNullOrWhiteSpace(arr.param2))
                                    source = source.Where(x => x.TemplateID.ToString() == arr.param2);

                                //searching
                                if (!string.IsNullOrWhiteSpace(arr.param4))
                                {
                                    arr.param4 = arr.param4.ToLower();
                                    source = source.Where(x => x.TemplateName.ToLower().Contains(arr.param4) || x.EventName.ToLower().Contains(arr.param4) ||
                                                        x.VoucherStatus.ToLower().Contains(arr.param4) || x.Recipient.ToLower().Contains(arr.param4) ||
                                                        x.Amount.ToString().ToLower().Contains(arr.param4));
                                }

                                // Get the final list base on the define linq queryable parameter
                                var results = source.ToList();

                                IEnumerable<CustomVoucherStatus> voucherStatus = null;
                                voucherStatus = results.Select(x => new CustomVoucherStatus
                                {
                                    PropertyName = x.PropertyName,
                                    PropertyLocation = x.PropertyLocation,
                                    PropertyEmail = x.PropertyEmail,
                                    MerchantName = x.MerchantName,
                                    MerchantEmail = x.MerchantEmail,
                                    TemplateName = x.TemplateName,
                                    EventName = x.EventName,
                                    Recipient = x.Recipient,
                                    VoucherCode = x.VoucherCode,
                                    Amount = x.Amount,
                                    VoucherStatus = x.VoucherStatus,
                                    ClaimedDate = x.ClaimedDate,
                                    ProcessDate = x.ProcessDate
                                }).AsEnumerable();

                                this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/VoucherStatus.rdlc";
                                this.ReportViewer1.LocalReport.DataSources.Clear();
                                this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSVoucherStatus", voucherStatus));
                                this.ReportViewer1.LocalReport.Refresh();
                            }
                            else if (rep == "v5nf1F10jklxM30923fg12")
                            {
                                IQueryable<CustomRecipient> source = (from rc in db.Recipients
                                                                    join vc in db.Vouchers on rc.UniqueID equals vc.UniqueID
                                                                    select new CustomRecipient
                                                                    {
                                                                        Id = rc.Id,
                                                                        UniqueID = rc.UniqueID,
                                                                        Recipient = vc.Recipient,
                                                                        VoucherCode = vc.VoucherCode,
                                                                        Amount = vc.Amount,
                                                                        ProcessDate = rc.ProcessDate,
                                                                        IsEmailSent = (rc.IsEmailSent) ? "Success" : "Failed",
                                                                        EmailSentDate = rc.EmailSentDate,
                                                                        EmailResentDate = rc.EmailResentDate,
                                                                        TemplateName = vc.TemplateCondition.Template.Name,
                                                                        PropertyName = vc.TemplateCondition.Template.Property.Name,
                                                                        PropertyLocation = vc.TemplateCondition.Template.Property.Location,
                                                                        MerchantName = vc.Merchant.Name,
                                                                    });

                                //param2:
                                if (!string.IsNullOrWhiteSpace(arr.param2))
                                    source = source.Where(x => x.TemplateID.ToString() == arr.param2);

                                //searching
                                if (!string.IsNullOrWhiteSpace(arr.param4))
                                {
                                    arr.param4 = arr.param4.ToLower();
                                    source = source.Where(x => x.TemplateName.ToLower().Contains(arr.param4) || x.EventName.ToLower().Contains(arr.param4) ||
                                                        x.Recipient.ToLower().Contains(arr.param4) || x.Amount.ToString().ToLower().Contains(arr.param4));
                                }

                                // Get the final list base on the define linq queryable parameter
                                var results = source.ToList();

                                IEnumerable<CustomRecipient> recipients = null;
                                recipients = results.AsEnumerable();

                                this.ReportViewer1.ProcessingMode = ProcessingMode.Local;
                                this.ReportViewer1.LocalReport.ReportPath = @"Reports/Rdlc/RecipientList.rdlc";
                                this.ReportViewer1.LocalReport.DataSources.Clear();
                                this.ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DSRecipients", recipients));
                                this.ReportViewer1.LocalReport.Refresh();

                                //fileName = String.Concat("TOSchedule", dt.ToString("yyyyMMdd"));
                                //render2PDF("PortraitLetter", fileName);
                            }
                            else
                            {
                                this.ReportViewer1.Visible = false;
                                lblMessage.Visible = true;
                                lblMessage.Text = "Invalid Report Parameter!";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.ReportViewer1.Visible = false;
                    lblMessage.Visible = true;
                    lblMessage.Text = String.Concat("Invalid Report Parameter! ", ex.Message);
                }
            }
        }
    }
}