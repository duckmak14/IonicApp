using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using WebMatrix.WebData;

namespace WebModels
{
    public partial class SupportOnline
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "Title")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Yahoo")]
        public string Yahoo { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Skype")]
        public string Skype { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Email")]
        public string Email { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Phone")]
        public string Phone { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public int Order { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public int Status { get; set; }
        public string Culture { get; set; }

    }
    [Table("UploadFiles")]
    public partial class UploadFile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        public string Title { get; set; }
        public string Link { get; set; }
    }
    [Table("WebContentUploads")]
    public class WebContentUpload
    {
        public WebContentUpload()
        {
            this.SubWebContentUploads = new HashSet<WebContentUpload>();

        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string FullPath { get; set; }
        public string UID { get; set; }
        //   public int RefID { get; set; }
        public int? FolderID { get; set; }
        public string MimeType { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "CreatedBy")]
        public string CreatedBy { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "CreatedDate")]
        public DateTime? CreatedDate { get; set; }
        // public string Module { get; set; }

        [ForeignKey("FolderID")]
        public virtual ICollection<WebContentUpload> SubWebContentUploads { get; set; }
    }
    public partial class WebSlide : BaseEntity
    {
        public WebSlide()
        {


        }
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Link")]
        public string Link { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Target")]
        public string Target { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public int? Order { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public int Status { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "CreatedDate")]
        public DateTime? CreatedDate { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(ResourceType = typeof(WebResources), Name = "ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }


        public string Culture { get; set; }
    }
    //[Table("Provinces")]
    //public partial class Province : BaseEntity
    //{
    //    public Province()
    //    {

    //    }
    //    [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
    //    [Display(ResourceType = typeof(WebResources), Name = "Title")]
    //    public string Title { get; set; }
    //    [Display(ResourceType = typeof(WebResources), Name = "CountryID")]
    //    public Nullable<int> CountryID { get; set; }
    //    [Display(ResourceType = typeof(WebResources), Name = "Area")]
    //    public string Area { get; set; }

    //    public virtual Country Country { get; set; }

    //}


    [Table("Countries")]
    public partial class Country : BaseEntity
    {
        public Country()
        {
            this.Provinces = new List<Province>();

        }

        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [StringLength(10)]
        [Display(ResourceType = typeof(WebResources), Name = "IsoCode")]
        public string IsoCode { get; set; }
 
        public virtual ICollection<Province> Provinces { get; set; }

    }



    [Table("WebSimpleContents")]
    public partial class WebSimpleContent : BaseEntity
    {
        public WebSimpleContent()
        {

        }
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Link")]
        public string Link { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Key")]
        public string Key { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Body")]
        public string Body { get; set; }

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

        [Display(ResourceType = typeof(WebResources), Name = "MetaTitle")]
        public string MetaTitle { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "MetaKeywords")]
        public string MetaKeywords { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "MetaDescription")]
        public string MetaDescription { get; set; }

        public string Culture { get; set; }
    }



    [Table("Advertisements")]
    public partial class Advertisement : BaseEntity
    {
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Media")]
        public string Media { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Link")]
        public string Link { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Target")]
        public string Target { get; set; }

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

        [Display(ResourceType = typeof(WebResources), Name = "Position")]
        [Column("AdvertisementPositionID")]
        public int? AdvertisementPositionID { get; set; }

        public virtual AdvertisementPosition AdvertisementPosition { get; set; }

        [NotMapped]
        public string Position { get; set; }

        public string Video { get; set; }
        
        public string Culture { get; set; }
    }


    [Table("AdvertisementPositions")]
    public partial class AdvertisementPosition : BaseEntity
    {
        public AdvertisementPosition()
        {
            this.Advertisements = new HashSet<Advertisement>();
        }

        [Display(ResourceType = typeof(WebResources), Name = "UID")]
        public string UID { get; set; }

        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }

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



        public virtual ICollection<Advertisement> Advertisements { get; set; }
    }


    [Table("WebLinks")]
    public partial class WebLink : BaseEntity
    {
        [Required(ErrorMessageResourceType = typeof(WebResources), ErrorMessageResourceName = "RequiredTitle")]
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Description")]
        public string Description { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Link")]
        public string Link { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Target")]
        public string Target { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Image")]
        public string Image { get; set; }
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

        [Display(ResourceType = typeof(WebResources), Name = "Order")]
        public int? Order { get; set; }

        public string Culture { get; set; }
    }

    [Table("Helps")]
    public partial class Help : BaseEntity
    {
        [Display(ResourceType = typeof(WebResources), Name = "Title")]
        public string Title { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Body")]
        public string Body { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }







}
