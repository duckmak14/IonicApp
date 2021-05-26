using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using WebMatrix.WebData;

namespace WebModels
{
    [Table("AdminSites")]
    public class AdminSite : BaseEntity 
    {
        public AdminSite()
        {
            this.AccessAdminSites = new HashSet<AccessAdminSite>();
            this.SubAdminSites = new HashSet<AdminSite>(); 
        }

        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "URL")]
        public string Url { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "AccessKey")]
        public string AccessKey { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ParentID")]
        public Nullable<int> ParentID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Img")]
        public string Img { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ImgActive")]
        public string ImgActive { get; set; }

        [ForeignKey("ParentID")]
        [Display(ResourceType = typeof(WebResources), Name = "SubAdminSites")]
        public virtual ICollection<AdminSite> SubAdminSites { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "AccessAdminSites")]
        public virtual ICollection<AccessAdminSite> AccessAdminSites { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ParentAdminSite")]
        public virtual AdminSite ParentAdminSite { get; set; } 

        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public int? Order { get; set; }

        public virtual ICollection<AccessAdminSiteRole> AccessAdminSiteRoles { get; set; }
    }
    
    public partial class AccessWebModule
    {
        [Key, Column(Order = 0)]
        [Display(ResourceType = typeof(WebResources), Name = "UserId")]
        public int UserId { get; set; }
        [Key, Column(Order = 1)]
        [Display(ResourceType = typeof(WebResources), Name = "WebModuleID")]
        public int WebModuleID { get; set; } 
        public virtual WebModule WebModule { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "View")]
        public bool View { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "Add")]
        public bool Add { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "Edit")]
        public bool Edit { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "Delete")]
        public bool Delete { get; set; }
        [ForeignKey("UserId")]
        public virtual UserProfile UserProfile { get; set; }
    }

    [Table("AccessWebModuleRole")]
    public partial class AccessWebModuleRole
    {
        [Key, Column(Order = 0)]
        public int RoleId { get; set; }

        [Key, Column(Order = 1)]
        [Display(ResourceType = typeof(WebResources), Name = "WebModuleID")]
        public int WebModuleID { get; set; }
        public virtual WebModule WebModule { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "View")]
        public bool? View { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "Add")]
        public bool? Add { get; set; }

        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "Edit")]
        public bool? Edit { get; set; }


        [DefaultValue(false)]
        [Display(ResourceType = typeof(WebResources), Name = "Delete")]
        public bool? Delete { get; set; }

 

        [ForeignKey("RoleId")]
        public virtual WebRole WebRole { get; set; }

        [NotMapped]
        public string CheckAllName { get; set; }
    }
    public partial class ContentType
    {
        public ContentType()
        {
            this.WebModules = new HashSet<WebModule>();
        }
        [Key]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public string ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Controller")]
        public string Controller { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public Nullable<int> Order { get; set; }
        public string Entity { get; set; }
        public virtual ICollection<WebModule> WebModules { get; set; }
    }
    [Table("UserProfiles")]
    public partial class WebUserProfile
    {
        public WebUserProfile()
        {
            this.AccessWebModules = new HashSet<AccessWebModule>();
        }
        [Key]
        [Display(ResourceType = typeof(WebResources), Name = "UserId")]
        public int UserId { get; set; }
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredUserName")]
        [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9]{2,255}$", ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "UserNameNotValid")]
        [Display(ResourceType = typeof(WebResources), Name = "UserName")]
        public string UserName { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "FullName")]
        public string FullName { get; set; }
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(WebResources), Name = "Email")]
        public string Email { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Mobile")]
        public string Mobile { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Avatar")]
        public string Avatar { get; set; }

        public virtual ICollection<AccessWebModule> AccessWebModules { get; set; }
    }
    [Table("WebConfigs")]
    public partial class WebConfig
    {
        [Key]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Key")]
        public string Key { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Value")]
        public string Value { get; set; }
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(WebResources), Name = "CreatedBy")]
        public string CreatedBy { get; set; }
        [DataType(DataType.DateTime)]
        [Display(ResourceType = typeof(WebResources), Name = "CreatedDate")]
        public DateTime? CreatedDate { get; set; }
        [DataType(DataType.Text)]
        [Display(ResourceType = typeof(WebResources), Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }
        [DataType(DataType.DateTime)]
        [Display(ResourceType = typeof(WebResources), Name = "ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }
     

    [Table("AccessAdminSites")]
    public class AccessAdminSite
    {
        [Display(ResourceType = typeof(WebResources), Name = "UserId")]
        [Key, Column(Order = 0)]
        public int UserId { get; set; }
        [Key, Column(Order = 1)]
        [Display(ResourceType = typeof(WebResources), Name = "AdminSiteID")]
        public int AdminSiteID { get; set; } 
        [Display(ResourceType = typeof(WebResources), Name = "AdminSite")]
        public virtual AdminSite AdminSite { get; set; }
    }

    [Table("AccessAdminSiteRole")]
    public class AccessAdminSiteRole
    {
        [Key, Column(Order = 0)]
        public int RoleId { get; set; }

        [Key, Column(Order = 1)]
        [Display(ResourceType = typeof(WebResources), Name = "AdminSiteID")]

        public int AdminSiteID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "AdminSite")]

        public virtual AdminSite AdminSite { get; set; }
    }

    public partial class AccessLog
    {
        public AccessLog() { }
        public AccessLog(string title, string action, string actor)
        {
            this.Title = title; this.Action = action;

            // WebSecurity.GetUserId(User.Identity.Name)

            this.CreatedDate = DateTime.Now;
            this.CreatedBy = actor;
            //this.CreatedBy = WebSecurity.GetUserId(User.Identity.Name) + ":" + User.Identity.Name;
        }

        public void Insert()
        {
            try
            {
                using (var db = new WebContext())
                {
                    db.Set<AccessLog>().Add(this);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            { }
        }
        [Key]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public long ID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "CreatedDate")]
        public Nullable<System.DateTime> CreatedDate { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "CreatedBy")]
        public string CreatedBy { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Action")]
        public string Action { get; set; }
    }

}
