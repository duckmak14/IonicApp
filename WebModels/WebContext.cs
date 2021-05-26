using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace WebModels
{

    public static class DBSetExtension
    {
        public static void RemoveMany<TEntity>(this DbSet<TEntity> thisDbSet, IEnumerable<TEntity> entities) where TEntity : class
        {
            for (int i = entities.Count() - 1; i >= 0; i--)
            {
                if (entities.ElementAt(i) != null)
                    thisDbSet.Remove(entities.ElementAt(i));
            }
        }
    }
    public partial class WebContext : DbContext
    {
        public WebContext()
            : base("DefaultConnection")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WebContent>().HasKey(e => e.ID);
            modelBuilder.Entity<WebContent>().HasRequired(t => t.ProductInfo).WithRequiredPrincipal(t => t.WebContent).WillCascadeOnDelete(true); ;
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WebRole> WebRoles { get; set; }
        public DbSet<WebContentUpload> WebContentUploads { get; set; }
        public DbSet<AccessWebModule> AccessWebModules { get; set; }
        public DbSet<ContentType> ContentTypes { get; set; }
        public DbSet<WebContact> WebContacts { get; set; }
        public DbSet<WebContent> WebContents { get; set; }
        public DbSet<FaceInfo> FaceInfos { get; set; }
        public DbSet<ContentRelated> ContentRelateds { get; set; }
        public DbSet<ProductInfo> ProductInfos { get; set; }
        public DbSet<SubscribeNotice> SubscribeNotices { get; set; }
        public DbSet<WebRedirect> WebRedirects { get; set; }
        public DbSet<WebModule> WebModules { get; set; }
        public virtual DbSet<Navigation> Navigations { get; set; }
        public virtual DbSet<ModuleNavigation> ModuleNavigations { get; set; }
        public DbSet<WebSimpleContent> WebSimpleContents { get; set; }
        public DbSet<WebSlide> WebSlides { get; set; }
        public DbSet<WebLink> WebLinks { get; set; }

        public DbSet<Country> Countries { get; set; }
        public DbSet<WebFAQ> WebFAQs { get; set; }
        public DbSet<AdminSite> AdminSites { get; set; }
        public DbSet<AccessAdminSite> AccessAdminSites { get; set; }
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<WebConfig> WebConfigs { get; set; }
        public virtual DbSet<AdvertisementPosition> AdvertisementPositions { get; set; }
        public virtual DbSet<Advertisement> Advertisements { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Help> Helps { get; set; }
        public DbSet<WebCategory> WebCategories { get; set; }
        public DbSet<WebContentCategory> WebContentCategories { get; set; }
        public DbSet<ContentImage> ContentImages { get; set; }
        public DbSet<SupportOnline> SupportOnlines { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<UploadFile> UploadFiles { get; set; }
        public DbSet<DayOfTour> DayOfTours { get; set; }
        public DbSet<Partner> Partner { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Route> Route { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<Province> Province { get; set; }
        public DbSet<PricingTable> PricingTable { get; set; }
        public DbSet<PriceChangeLog> PriceChangeLogs { get; set; }
        public DbSet<VehicleWeight> VehicleWeight { get; set; }
        public DbSet<AccessWebModuleRole> AccessWebModuleRoles { get; set; }
        public DbSet<AccessAdminSiteRole> AccessAdminSitesRoles { get; set; }
        public DbSet<PriceChangePeriod> PriceChangePeriods { get; set; }
        public DbSet<TransportPlan> TransportPlans { get; set; }
        public DbSet<UserInRole> UserInRoles { get; set; }
        public DbSet<TransportActual> TransportActuals { get; set; }
        public DbSet<EnumerationChangeLog> EnumerationChangeLogs { get; set; }

    }

    public enum CTypeCategories
    {
        Common = 0,
        Product = 1,
        News = 2
    }

    public enum AccessLogActions
    {
        View,
        Add,
        Edit,
        Delete,
        Login
    }
    public enum AccessKeys
    {
        Home,
        Role,
        User,
        AdminSite,
        ContentType,
        AccessLog,
        WebConfig,
        WebModule,
        Navigation,
        WebContent,
        WebSimpleContent,
        Country,
        Province,
        Support,
        WebLink,
        WebSlide,
        AdvPosition,
        Advertisement,
        Helps,
        WebCategory,
        ProductCategory,
        SubscribeNotice
    }

    public enum Permissions
    {
        View,
        Add,
        Edit,
        Delete
    }
    public enum DataModules
    {
        WebContent,
        WebSimpleContent
    }
    public enum Status { Private = -1, Internal = 0, Public = 1 }

    public enum StatusUser { TaoMoi = -2, GuiDi = -3, TraLai = -4 }

    public enum LoaiLienHe { YKienCuaBan = 1, DatMua = 2, LienHeToaSon = 3, ThuMoiNghienCuu = 4 }
    public enum LoaiDonHang { DienTu = 1, Giay = 2 }

}
