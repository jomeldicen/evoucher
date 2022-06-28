using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace WebApp.Models
{
    public class CustomChangeLog
    {
        public int Id { get; set; }
        public string vMenuID { get; set; }
        public string MenuName { get; set; }
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string ObjectType { get; set; }
        public string ObjectID { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public string UserName { get; set; }
    }

    public class CustomRegisterModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordSendMailModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class CustomUserRegisterModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string RoleId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string Company { get; set; }
        [Required]
        public string Rank { get; set; }
        [Required]
        public string Position { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        public string Photo { get; set; }
        [Required]
        public bool EmailVerificationDisabled { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string BlockedAccount { get; set; }
        [Required]
        public string ResetPassword { get; set; }
    }

    public class UpdateUserModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        public string Id { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string RoleId { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Photo { get; set; }
        [Required]
        public string Company { get; set; }
        [Required]
        public string Rank { get; set; }
        [Required]
        public string Position { get; set; }
        [Required]
        public string Department { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string BlockedAccount { get; set; }
        public string ResetPassword { get; set; }
    }

    public class CustomUsers
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public int NosProjects { get; set; }
        public string Photo { get; set; }
        public string Status { get; set; }
        public string BlockedAccount { get; set; }
        public string ResetPassword { get; set; }
        public string Company { get; set; }
        public string Position { get; set; }
        public string Rank { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class Menu
    {
        public string vMenuID { get; set; }
        public string NameWithParent { get; set; }
        public string nvMenuName { get; set; }
        public int iSerialNo { get; set; }
        public string nvFabIcon { get; set; }
        public string vParentMenuID { get; set; }
        public string nvPageUrl { get; set; }
        public List<Menu> Child { get; set; }
        public string PrefixCode { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<Menu> dsList { get; set; }
        public List<IdList> OptionIDs { get; set; }
        public List<IdLabelList> ControlIDs { get; set; }
    }

    public class Role
    {
        public int sl { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string IndexPage { get; set; }
        public int totalUser { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<Role> dsList { get; set; }
        public List<IdList> OptionIDs { get; set; }
    }

    public class CustomControl
    {
        public int wView { get; set; }
        public int wAdd { get; set; }
        public int wEdit { get; set; }
        public int wDelete { get; set; }
        public int wExtract { get; set; }
        public int wPrint { get; set; }
        public int wDownload { get; set; }
        public int wUpload { get; set; }
        public int wSync { get; set; }
        public int wFetch { get; set; }
    }

    public class CustomRoles
    {
        public Role Role { get; set; }
        public List<Menu> MenuList { get; set; }
    }

    // No need anymore
    public class CustomRole
    {
        public AspNetRole Role { get; set; }
        public List<AspNetUsersMenuPermission> MenuList { get; set; }
    }

    public class CustomSettings
    {
        public bool UserRegister { get; set; }
        public bool EmailVerificationDisable { get; set; }
        public string UserRole { get; set; }
        public bool RecoverPassword { get; set; }
        public bool ChangePassword { get; set; }
        public bool ChangeProfile { get; set; }
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public bool SiteOffline { get; set; }
        public string SiteOfflineMessage { get; set; }
        public bool BodySmallText { get; set; }
        public bool NavbarSmallText { get; set; }
        public bool SidebarNavSmallText { get; set; }
        public bool FooterSmallText { get; set; }
        public bool SidebarNavFlatStyle { get; set; }
        public bool SidebarNavLegacyStyle { get; set; }
        public bool SidebarNavCompact { get; set; }
        public bool SidebarNavChildIndent { get; set; }
        public bool BrandLogoSmallText { get; set; }
        public string NavbarColorVariants { get; set; }
        public string LinkColorVariants { get; set; }
        public string DarkSidebarVariants { get; set; }
        public string LightSidebarVariants { get; set; }
        public string BrandLogoVariants { get; set; }
        public string SiteMetaDescription { get; set; }
        public string SiteMetaKeyword { get; set; }
        public string Robots { get; set; }
        public string SiteAuthor { get; set; }
        public string ContentRight { get; set; }
        public string IPAddress { get; set; }
        public string SystemEnvironment { get; set; }
        public string DefaultIndex { get; set; }
        public string SecretKey { get; set; }
        public string ProjectDirectory { get; set; }
        public string UploadPath { get; set; }
        public string ReportPath { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ReplyToEmail { get; set; }
        public string ReplyToName { get; set; }
        public string Mailer { get; set; }
        public string SMTPHost { get; set; }
        public string SMTPPort { get; set; }
        public string SMTPSecurity { get; set; }
        public bool SMTPAuthentication { get; set; }
        public bool SMTPEnableSSL { get; set; }
        public string Company { get; set; }
        public string CorporateAddress { get; set; }
        public string CorporateContactNos { get; set; }
        public string CorporateFaxNos { get; set; }
        public string CorporateEmail { get; set; }
        public string CorporateWebsite { get; set; }
        public string FacebookPage { get; set; }
        public string TwitterPage { get; set; }
        public string InstagramPage { get; set; }
        public string YoutubePage { get; set; }
        public string LinkedinPage { get; set; }
        public string PinterestPage { get; set; }
        public string MarketingEmail { get; set; }
        public string ExecutiveEmail { get; set; }
        public string ITEmail { get; set; }
        public decimal VoucherThreshold { get; set; }
    }

    public class IdList
    {
        public int id { get; set; }
    }

    public class IdLabelList
    {
        public int id { get; set; }
        public string label { get; set; }
    }

    public class listData
    {
        public int Id { get; set; }
        public bool isChecked { get; set; }
    }

    public class listData2
    {
        public string Id { get; set; }
        public bool isChecked { get; set; }
    }

    public class CustomOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OptionGroup { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomEmailTemplate
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomRestaurant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomTemplate
    {
        public int Id { get; set; }
        public string PropertyID { get; set; }
        public string Name { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
        public List<CustomTemplateCondition> Conditions { get; set; }
    }

    public class CustomTemplateCondition
    {
        public string Id { get; set; }
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public decimal Amount { get; set; }
        public decimal Amount2 { get; set; }
        public string TemplatePath { get; set; }
        public string Guidelines { get; set; }
        public string VoucherBase64 { get; set; }
        public string Published { get; set; }
        public bool isChecked { get; set; }
        public string IsCustomTemplate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData2> dsList { get; set; }
    }

    public class CustomProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string ContactPerson { get; set; }
        public string Published { get; set; }
        public string PropertyImage { get; set; }
        public string OldImage { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomMerchant
    {
        public int Id { get; set; }
        public int PropertyID { get; set; }
        public string PropertyName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string ContactPerson { get; set; }
        public string Published { get; set; }
        public string Section { get; set; }
        public string MerchantImage { get; set; }
        public string OldImage { get; set; }
        public bool isChecked { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomVoucher
    {
        public int Id { get; set; }
        public int TemplateID { get; set; }
        public int? MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyLocation { get; set; }
        public string TempCondID { get; set; }
        public string TemplateName { get; set; }
        public string EventName { get; set; }
        public string UniqueID { get; set; }
        public string Recipient { get; set; }
        public string VoucherCode { get; set; }
        public decimal Amount { get; set; }
        public string VoucherBase64 { get; set; }
        public string QRCodeBase64 { get; set; }
        public string UrlLink { get; set; }
        public string VoucherStatus { get; set; }
        public string WithDiscrepancy { get; set; }
        public string GenerationID { get; set; }
        public string Remarks { get; set; }
        public bool isChecked { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
        public List<listData> dsList { get; set; }
    }

    public class CustomRecipient
    {
        public int Id { get; set; }
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyLocation { get; set; }
        public string MerchantName { get; set; }
        public string EventName { get; set; }
        public string UniqueID { get; set; }
        public string Recipient { get; set; }
        public string VoucherCode { get; set; }
        public decimal Amount { get; set; }
        public string VoucherBase64 { get; set; }
        public DateTime ProcessDate { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public string IsEmailSent { get; set; }
        public string EmailDetail { get; set; }
        public DateTime EmailSentDate { get; set; }
        public DateTime? EmailResentDate { get; set; }
        public string EmailErrorLog { get; set; }
    }

    public class CustomVoucherStatus
    {
        public int Id { get; set; }
        public int TemplateID { get; set; }
        public string TemplateName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyLocation { get; set; }
        public string PropertyEmail { get; set; }
        public string MerchantName { get; set; }
        public string MerchantEmail { get; set; }
        public string EventName { get; set; }
        public string UniqueID { get; set; }
        public string Recipient { get; set; }
        public string VoucherCode { get; set; }
        public decimal Amount { get; set; }
        public string VoucherStatus { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public DateTime? ProcessDate { get; set; }
    }

    public class CustomSendingLogs
    {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public string HashCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedByPK { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ModifiedByPK { get; set; }
    }

}