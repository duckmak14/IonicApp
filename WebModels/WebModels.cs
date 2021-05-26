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


    public partial class WebModule : BaseWebContent
    {
        public WebModule()
        {
            this.AccessWebModules = new HashSet<AccessWebModule>();
            this.AccessWebModuleRoles = new HashSet<AccessWebModuleRole>();
            this.WebContents = new HashSet<WebContent>();
            this.WebContacts = new HashSet<WebContact>();
            this.WebFAQs = new HashSet<WebFAQ>();
            this.WebRedirects = new HashSet<WebRedirect>();

            this.ModuleNavigations = new HashSet<ModuleNavigation>();
        }
        public virtual ICollection<ModuleNavigation> ModuleNavigations { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Body")]
        public string Body { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Icon")]
        public string Icon { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "CodeColor")]
        public string CodeColor { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ParentID")]
        public int? ParentID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ContentTypeID")]
        public string ContentTypeID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "URL")]
        public string URL { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Target")]
        public string Target { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "SeoTitle")]
        public string SeoTitle { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "MetaTitle")]
        public string MetaTitle { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "MetaKeywords")]
        public string MetaKeywords { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "MetaDescription")]
        public string MetaDescription { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public int? Order { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "UID")]
        public string UID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "IndexLayout")]
        public string IndexLayout { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "IndexView")]
        public string IndexView { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "PublishIndexView")]
        public string PublishIndexView { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "PublishDetailView")]
        public string PublishDetailView { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "PublishIndexLayout")]
        public string PublishIndexLayout { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "PublishDetailLayout")]
        public string PublishDetailLayout { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Img")]
        public string Img { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ImgActive")]
        public string ImgActive { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public int? Status { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "SubQuerys")]
        public string SubQuerys { get; set; }
        public string ActiveArticle { get; set; }

        public virtual ICollection<AccessWebModule> AccessWebModules { get; set; }
        public virtual ICollection<AccessWebModuleRole> AccessWebModuleRoles { get; set; }
        public virtual ContentType ContentType { get; set; }
        public virtual ICollection<WebContent> WebContents { get; set; }
        [ForeignKey("ParentID")]
        public virtual ICollection<WebModule> SubWebModules { get; set; }
        public virtual ICollection<WebContact> WebContacts { get; set; }
        public virtual ICollection<WebFAQ> WebFAQs { get; set; }
        public virtual ICollection<WebRedirect> WebRedirects { get; set; }
        public virtual WebModule Parent { get; set; }
     

        public Boolean ShowOnMenu { get; set; }
        public Boolean ShowOnAdmin { get; set; }
        public string Culture { get; set; }

    }
    [Table("ContentRelateds")]
    public partial class ContentRelated
    {
        [Key]
        [ForeignKey("MainContent")]
        [Column(Order = 1)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int MainID { get; set; }
        [Key]
        [ForeignKey("RelateContent")]
        [Column(Order = 2)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int RelatedID { get; set; }
        [DataType(DataType.Text)]
        public string Type { get; set; }
        [DataType(DataType.Text)]
        public int Order { get; set; }
        public virtual WebContent MainContent { get; set; }
        public virtual WebContent RelateContent { get; set; }
    }


    [Table("SubscribeNotices")]
    public partial class SubscribeNotice
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

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

        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public int Status { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Email")]
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredEmail")]
        [RegularExpression(@"[\w\d._%+-]+@[\w\d.-]+\.\w{2,4}", ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "EmailNotValid")]
        public string Email { get; set; }
    }



    public partial class WebContent : BaseWebContent
    {
        public WebContent()
        {
            //if (WebContentCategories == null)
            // this.WebContentCategories = new HashSet<WebContentCategory>();
            //if(ProductInfo==null)     this.ProductInfo = new ProductInfo();
        }
        public string UID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Body")]
        public string Body { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Link")]
        public string Link { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "MetaTitle")]
        public string MetaTitle { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "MetaKeywords")]
        public string MetaKeywords { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "MetaDescription")]
        public string MetaDescription { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public Nullable<int> Status { get; set; }
        public string StatusText
        {
            get
            {
                if (Status == null)
                {
                    return string.Empty;
                }
                if (Status.Value == (int)StatusEnum.Internal)
                {
                    return "Chờ duyệt";
                }
                if (Status.Value == (int)StatusEnum.Public)
                {
                    return "Đã phát hành";
                }

                return string.Empty;
            }
        }
        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public Nullable<int> Order { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "LinkVideo")]
        public string LinkVideo { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Event")]
        public DateTime? Event { get; set; }

        public string SeoTitle { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "WebModuleID")]
        public Nullable<int> WebModuleID { get; set; }
        public decimal? CountView { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "PublishDate")]
        public DateTime? PublishDate { get; set; }
        public virtual WebModule WebModule { get; set; }
        public virtual ICollection<WebContentCategory> WebContentCategories { get; set; }
        public virtual ICollection<ContentImage> ContentImages { get; set; }
        public virtual ProductInfo ProductInfo { get; set; }
        public virtual FaceInfo FaceInfo { get; set; }
    }

    public enum StatusEnum { Private = -1, Internal = 0, Public = 1 }

    [Table("ProductInfos")]
    public partial class ProductInfo
    {
        [Key]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Price")]
        [Column("Price")]
        public Nullable<double> Price { get; set; }

        public string Code { get; set; }
        public virtual WebContent WebContent { get; set; }
    }



    [Table("Destinations")]
    public partial class Destination
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string IsoCode { get; set; }

        public int Order { get; set; }
        public int WebModuleID { get; set; }

    }



    [Table("FaceInfos")]
    public partial class FaceInfo
    {
        [Key]
        [ForeignKey("WebContent")]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Column("Fullname")]
        [Display(ResourceType = typeof(WebResources), Name = "FullName")]
        public string Fullname { get; set; }
        [Column("Bday")]
        [Display(ResourceType = typeof(WebResources), Name = "Bday")]
        public string Bday { get; set; }
        [Column("Gender")]
        [Display(ResourceType = typeof(WebResources), Name = "Gender")]
        public bool? Gender { get; set; }
        [Column("Height")]
        [Display(ResourceType = typeof(WebResources), Name = "Height")]
        public string Height { get; set; }
        [Column("Nationality")]
        [Display(ResourceType = typeof(WebResources), Name = "Nationality")]
        public string Nationality { get; set; }
        [Column("Marriage")]
        [Display(ResourceType = typeof(WebResources), Name = "Marriage")]
        public string Marriage { get; set; }
        [Column("Assizes")]
        [Display(ResourceType = typeof(WebResources), Name = "Assizes")]
        public string Assizes { get; set; }
        [Column("View")]
        [Display(ResourceType = typeof(WebResources), Name = "View")]
        public int? View { get; set; }
        public virtual WebContent WebContent { get; set; }
    }

    public partial class WebFAQ : BaseWebContent
    {
        public WebFAQ()
        {
        }


        [Display(ResourceType = typeof(WebResources), Name = "Answer")]
        public string Ans { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "WebModuleID")]
        public Nullable<int> WebModuleID { get; set; }
        public virtual WebModule WebModule { get; set; }
    }

    public partial class WebRedirect : BaseWebContent
    {
        [Display(ResourceType = typeof(WebResources), Name = "URL")]
        public string URL { get; set; }
        public Nullable<int> WebModuleID { get; set; }
        public Nullable<int> TimeRedirect { get; set; }
        public virtual WebModule WebModule { get; set; }
    }

    [Table("WebContacts")]
    public partial class WebContact : BaseEntity
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [Display(ResourceType = typeof(WebResources), Name = "Body")]
        public string Body { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(ResourceType = typeof(WebResources), Name = "FullName")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(WebResources), Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn số điện thoại")]
        [Display(ResourceType = typeof(WebResources), Name = "Mobile")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(ResourceType = typeof(WebResources), Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        public DateTime? NgayBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại đơn hàng")]
        public int LoaiDonHang { get; set; }

        [NotMapped]
        public string LoaiDonHangText
        {
            get
            {
                return LoaiDonHang == 1 ? "Điện tử" : "Giấy";
            }
        }
        public int LoaiLienHe { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "CreatedDate")]
        public DateTime CreatedDate { get; set; }
        public int? WebModuleID { get; set; }
    }



    [Table("ContentImages")]
    public class ContentImage : BaseEntity
    {
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        public string Image { get; set; }

        [DefaultValue(0)]
        public int Order { get; set; }

        public int WebContentID { get; set; }
        public virtual WebContent WebContent { get; set; }
    }

    [Table("WebCategories")]
    public class WebCategory : BaseEntity
    {
        public WebCategory()
        {
            this.SubWebCategories = new HashSet<WebCategory>();
            this.WebContentCategories = new HashSet<WebContentCategory>();
        }
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Body")]
        public string Body { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "MetaTitle")]
        public string MetaTitle { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public Nullable<int> Status { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public Nullable<int> Order { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "MetaDescription")]
        public string MetaDescription { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "MetaKeywords")]
        public string MetaKeywords { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "ParentID")]
        public Nullable<int> ParentID { get; set; }


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

        public virtual ICollection<WebCategory> SubWebCategories { get; set; }

        [ForeignKey("ParentID")]
        public virtual WebCategory ParentWebCategory { get; set; }

        public virtual ICollection<WebContentCategory> WebContentCategories { get; set; }

        public int CType { get; set; }

    }

    [Table("WebContentCategories")]
    public class WebContentCategory
    {
        [Key]
        [Column(Order = 0)]
        [Display(ResourceType = typeof(WebResources), Name = "WebCategoryID")]
        public int WebCategoryID { get; set; }

        [Key]
        [Column(Order = 1)]
        [Display(ResourceType = typeof(WebResources), Name = "WebContentID")]
        public int WebContentID { get; set; }

        public int? Order { get; set; }

        public WebCategory WebCategory { get; set; }
        public WebContent WebContent { get; set; }
    }

    [Table("ModuleNavigations")]
    public class ModuleNavigation
    {
        [Key]
        [Column(Order = 0)]
        [Display(ResourceType = typeof(WebResources), Name = "WebModuleID")]
        public int WebModuleID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int NavigationID { get; set; }


        public virtual Navigation Navigation { get; set; }

        public virtual WebModule WebModule { get; set; }



    }
    [Table("Navigations")]
    public class Navigation : BaseEntity
    {
        public Navigation()
        {
            this.ModuleNavigations = new HashSet<ModuleNavigation>();
        }
        public virtual ICollection<ModuleNavigation> ModuleNavigations { get; set; }


        [Display(ResourceType = typeof(WebResources), Name = "Key")]
        public string Key { get; set; }

        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        [DefaultValue(0)]
        public int Order { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }
    }

    [Table("DayOfTours")]
    public class DayOfTour
    {
        //[Key]
        //[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public string Body { get; set; }
        public int WebContentID { get; set; }
    }
}
