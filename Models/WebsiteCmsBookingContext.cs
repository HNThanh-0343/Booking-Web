using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WEBSITE_TRAVELBOOKING.Models;

public partial class WebsiteCmsBookingContext : DbContext
{
    public WebsiteCmsBookingContext()
    {
    }

    public WebsiteCmsBookingContext(DbContextOptions<WebsiteCmsBookingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CatAminitieseRoom> CatAminitieseRooms { get; set; }

    public virtual DbSet<CatBank> CatBanks { get; set; }

    public virtual DbSet<CatBedRoom> CatBedRooms { get; set; }

    public virtual DbSet<CatCategory> CatCategories { get; set; }

    public virtual DbSet<CatContry> CatContries { get; set; }

    public virtual DbSet<CatFaq> CatFaqs { get; set; }

    public virtual DbSet<CatGuest> CatGuests { get; set; }

    public virtual DbSet<CatPromotionGuest> CatPromotionGuests { get; set; }

    public virtual DbSet<CatRoomService> CatRoomServices { get; set; }

    public virtual DbSet<CatTypeBlog> CatTypeBlogs { get; set; }

    public virtual DbSet<CatTypeRoom> CatTypeRooms { get; set; }

    public virtual DbSet<CatWhyBookWithU> CatWhyBookWithUs { get; set; }

    public virtual DbSet<SysAdvetise> SysAdvetises { get; set; }

    public virtual DbSet<SysBlog> SysBlogs { get; set; }

    public virtual DbSet<SysBooking> SysBookings { get; set; }

    public virtual DbSet<SysCallLog> SysCallLogs { get; set; }

    public virtual DbSet<SysClientTestimonial> SysClientTestimonials { get; set; }

    public virtual DbSet<SysContact> SysContacts { get; set; }

    public virtual DbSet<SysDemoPic> SysDemoPics { get; set; }

    public virtual DbSet<SysEvaluate> SysEvaluates { get; set; }

    public virtual DbSet<SysGuest> SysGuests { get; set; }

    public virtual DbSet<SysGuestNoUser> SysGuestNoUsers { get; set; }

    public virtual DbSet<SysHotel> SysHotels { get; set; }

    public virtual DbSet<SysInfo> SysInfos { get; set; }

    public virtual DbSet<SysInfoHead> SysInfoHeads { get; set; }

    public virtual DbSet<SysInvoiceRoom> SysInvoiceRooms { get; set; }

    public virtual DbSet<SysItinerary> SysItineraries { get; set; }

    public virtual DbSet<SysLike> SysLikes { get; set; }

    public virtual DbSet<SysLog> SysLogs { get; set; }

    public virtual DbSet<SysLogPayment> SysLogPayments { get; set; }

    public virtual DbSet<SysMenu> SysMenus { get; set; }

    public virtual DbSet<SysModule> SysModules { get; set; }

    public virtual DbSet<SysPayment> SysPayments { get; set; }

    public virtual DbSet<SysPost> SysPosts { get; set; }

    public virtual DbSet<SysPricingUnit> SysPricingUnits { get; set; }

    public virtual DbSet<SysPromotion> SysPromotions { get; set; }

    public virtual DbSet<SysRole> SysRoles { get; set; }

    public virtual DbSet<SysRoom> SysRooms { get; set; }

    public virtual DbSet<SysRoomHomeStay> SysRoomHomeStays { get; set; }

    public virtual DbSet<SysRule> SysRules { get; set; }

    public virtual DbSet<SysSettingMenu> SysSettingMenus { get; set; }

    public virtual DbSet<SysSocial> SysSocials { get; set; }

    public virtual DbSet<SysUser> SysUsers { get; set; }

    public virtual DbSet<SysVilla> SysVillas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=WEBSITE_CMS_BOOKING;User Id=sa;Password=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatAminitieseRoom>(entity =>
        {
            entity.ToTable("Cat_Aminitiese_Room");

            entity.Property(e => e.Icon).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(500);
        });

        modelBuilder.Entity<CatBank>(entity =>
        {
            entity.ToTable("Cat_Bank");

            entity.Property(e => e.KeyBank).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<CatBedRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Cat_BadRoom");

            entity.ToTable("Cat_BedRoom");

            entity.Property(e => e.AcreageBed).HasMaxLength(255);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("((1))")
                .IsFixedLength();
        });

        modelBuilder.Entity<CatCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_sys_Categories");

            entity.ToTable("Cat_Categories");

            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<CatContry>(entity =>
        {
            entity.ToTable("Cat_Contry");

            entity.Property(e => e.Image).HasMaxLength(255);
        });

        modelBuilder.Entity<CatFaq>(entity =>
        {
            entity.ToTable("Cat_Faq");
        });

        modelBuilder.Entity<CatGuest>(entity =>
        {
            entity.ToTable("Cat_Guests");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<CatPromotionGuest>(entity =>
        {
            entity.ToTable("Cat_Promotion_Guest");

            entity.Property(e => e.DayReceive).HasColumnType("datetime");
        });

        modelBuilder.Entity<CatRoomService>(entity =>
        {
            entity.ToTable("Cat_RoomServices");

            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<CatTypeBlog>(entity =>
        {
            entity.ToTable("Cat_TypeBlog");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<CatTypeRoom>(entity =>
        {
            entity.ToTable("Cat_TypeRoom");

            entity.Property(e => e.Icon).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(500);
        });

        modelBuilder.Entity<CatWhyBookWithU>(entity =>
        {
            entity.ToTable("Cat_WhyBookWithUs");

            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        modelBuilder.Entity<SysAdvetise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Advetise");

            entity.ToTable("sys_Advetise");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Urlimg).HasMaxLength(50);
        });

        modelBuilder.Entity<SysBlog>(entity =>
        {
            entity.ToTable("sys_Blog");

            entity.Property(e => e.ContentsShort).HasMaxLength(255);
            entity.Property(e => e.DateCreate).HasColumnType("datetime");
            entity.Property(e => e.DateEdit).HasColumnType("datetime");
            entity.Property(e => e.ListImg).HasColumnName("ListIMG");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Tag).HasMaxLength(255);

            entity.HasOne(d => d.IdTypeBlogNavigation).WithMany(p => p.SysBlogs)
                .HasForeignKey(d => d.IdTypeBlog)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_sys_Blog_Cat_TypeBlog");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.SysBlogs)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_sys_Blog_sys_User");
        });

        modelBuilder.Entity<SysBooking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC0750A39046");

            entity.ToTable("sys_Booking");

            entity.Property(e => e.BookingDate).HasColumnType("datetime");
            entity.Property(e => e.CheckInDate).HasColumnType("datetime");
            entity.Property(e => e.Deposit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DesQr)
                .HasMaxLength(50)
                .HasColumnName("DesQR");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EmailGuest).HasMaxLength(255);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PhoneGuest).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdCategoriesNavigation).WithMany(p => p.SysBookings)
                .HasForeignKey(d => d.IdCategories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Categories");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.SysBookings)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_Booking_Users");
        });

        modelBuilder.Entity<SysCallLog>(entity =>
        {
            entity.ToTable("sys_CallLog");

            entity.Property(e => e.Ip).HasColumnName("IP");
            entity.Property(e => e.Time).HasColumnType("datetime");
        });

        modelBuilder.Entity<SysClientTestimonial>(entity =>
        {
            entity.ToTable("sys_ClientTestimonial");

            entity.Property(e => e.Avatar).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<SysContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Contact");

            entity.ToTable("sys_Contact");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<SysDemoPic>(entity =>
        {
            entity.ToTable("sys_DemoPic");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Pic1).HasMaxLength(50);
            entity.Property(e => e.Pic2).HasMaxLength(50);
            entity.Property(e => e.Pic3).HasMaxLength(50);
            entity.Property(e => e.Pic4).HasMaxLength(50);
            entity.Property(e => e.Pic5).HasMaxLength(50);
        });

        modelBuilder.Entity<SysEvaluate>(entity =>
        {
            entity.ToTable("sys_Evaluate");

            entity.Property(e => e.Avgreview).HasColumnName("AVGReview");
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SysGuest>(entity =>
        {
            entity.ToTable("sys_Guest");

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CardName).HasMaxLength(255);
            entity.Property(e => e.CardNumber).HasMaxLength(255);
            entity.Property(e => e.Ccv)
                .HasMaxLength(255)
                .HasColumnName("CCV");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Local).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(255);
            entity.Property(e => e.ValidFrom).HasColumnType("datetime");
            entity.Property(e => e.ValidTill).HasColumnType("datetime");
        });

        modelBuilder.Entity<SysGuestNoUser>(entity =>
        {
            entity.ToTable("sys_Guest_NoUser");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Ho).HasMaxLength(500);
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .HasColumnName("SDT");
            entity.Property(e => e.Ten).HasMaxLength(500);
        });

        modelBuilder.Entity<SysHotel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Hotel");

            entity.ToTable("sys_Hotel");

            entity.Property(e => e.ListImg).HasColumnName("ListIMG");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.NumStar).HasDefaultValue(0);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PriceMin).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TimeCreate).HasColumnType("datetime");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.SysHotels)
                .HasForeignKey(d => d.IdCategory)
                .HasConstraintName("FK_sys_Hotel_sys_Categories");

            entity.HasOne(d => d.IdContryNavigation).WithMany(p => p.SysHotels)
                .HasForeignKey(d => d.IdContry)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_sys_Hotel_Cat_Contry");

            entity.HasOne(d => d.IdPromotionNavigation).WithMany(p => p.SysHotels)
                .HasForeignKey(d => d.IdPromotion)
                .HasConstraintName("FK_sys_Hotel_sys_Promotion");
        });

        modelBuilder.Entity<SysInfo>(entity =>
        {
            entity.ToTable("sys_Info");

            entity.Property(e => e.Contents).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SysInfoHead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_InfoHead");

            entity.ToTable("sys_InfoHead");

            entity.Property(e => e.Line1).HasMaxLength(50);
            entity.Property(e => e.Line2).HasMaxLength(50);
            entity.Property(e => e.Line3).HasMaxLength(50);
            entity.Property(e => e.UrlVideo).HasMaxLength(50);
        });

        modelBuilder.Entity<SysInvoiceRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sys_Invoice");

            entity.ToTable("Sys_InvoiceRoom");

            entity.Property(e => e.DateCreate).HasColumnType("datetime");
            entity.Property(e => e.EmailGuest).HasMaxLength(500);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.FullNameGuest).HasMaxLength(500);
            entity.Property(e => e.ListIdRoomBooking).HasMaxLength(500);
            entity.Property(e => e.PhoneGuest).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.TotalMoney).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<SysItinerary>(entity =>
        {
            entity.ToTable("Sys_Itinerary");

            entity.Property(e => e.DayName).HasMaxLength(50);
            entity.Property(e => e.LocalName).HasMaxLength(50);
        });

        modelBuilder.Entity<SysLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sys_Like__3214EC07ED3D069A");

            entity.ToTable("sys_Like");

            entity.Property(e => e.Idcategory).HasColumnName("IDcategory");
        });

        modelBuilder.Entity<SysLog>(entity =>
        {
            entity.ToTable("sys_Log");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.IdBoss).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NumberOfGuests).HasMaxLength(50);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.RoomType).HasMaxLength(50);
            entity.Property(e => e.SpecialRequests).HasMaxLength(50);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(50);
        });

        modelBuilder.Entity<SysLogPayment>(entity =>
        {
            entity.ToTable("sys_Log_payment");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NumberOfGuests).HasMaxLength(50);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.RoomType).HasMaxLength(50);
            entity.Property(e => e.SpecialRequests).HasMaxLength(50);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(50);
        });

        modelBuilder.Entity<SysMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Menu");

            entity.ToTable("sys_Menu");

            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SysModule>(entity =>
        {
            entity.ToTable("sys_Module");

            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.NameController).HasMaxLength(255);
            entity.Property(e => e.PrentId).HasMaxLength(50);
        });

        modelBuilder.Entity<SysPayment>(entity =>
        {
            entity.ToTable("sys_Payment");

            entity.Property(e => e.Action).HasMaxLength(50);
        });

        modelBuilder.Entity<SysPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Post");

            entity.ToTable("sys_Post");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Feature).HasMaxLength(50);
            entity.Property(e => e.GuestsNumber).HasMaxLength(50);
            entity.Property(e => e.IddemoPic).HasColumnName("IDDemoPic");
            entity.Property(e => e.Idpromotion).HasColumnName("IDPromotion");
            entity.Property(e => e.Local).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Overview).HasMaxLength(50);
            entity.Property(e => e.Picture).HasMaxLength(50);
            entity.Property(e => e.Price).HasMaxLength(50);
        });

        modelBuilder.Entity<SysPricingUnit>(entity =>
        {
            entity.ToTable("sys_PricingUnit");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SysPromotion>(entity =>
        {
            entity.ToTable("sys_Promotion");

            entity.Property(e => e.Code).HasMaxLength(255);
            entity.Property(e => e.ConditionNumber).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Role");

            entity.ToTable("sys_Role");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SysRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Room");

            entity.ToTable("sys_Room");

            entity.Property(e => e.AdultsMax).HasColumnName("adultsMax");
            entity.Property(e => e.ChildrenMax).HasColumnName("childrenMax");
            entity.Property(e => e.IdTypeBed).HasMaxLength(50);
            entity.Property(e => e.ListAminities).HasMaxLength(255);
            entity.Property(e => e.ListImg).HasColumnName("ListIMG");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.TotalRoom).HasColumnName("totalRoom");

            entity.HasOne(d => d.TypeRoomNavigation).WithMany(p => p.SysRooms)
                .HasForeignKey(d => d.TypeRoom)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_sys_Room_Cat_TypeRoom");
        });

        modelBuilder.Entity<SysRoomHomeStay>(entity =>
        {
            entity.ToTable("Sys_RoomHomeStay");

            entity.Property(e => e.AdultsMax).HasColumnName("adultsMax");
            entity.Property(e => e.ChildrenMax).HasColumnName("childrenMax");
            entity.Property(e => e.IdTypeBed).HasMaxLength(50);
            entity.Property(e => e.ListAminities).HasMaxLength(255);
            entity.Property(e => e.ListImg).HasColumnName("ListIMG");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.TotalRoom).HasColumnName("totalRoom");
        });

        modelBuilder.Entity<SysRule>(entity =>
        {
            entity.ToTable("sys_Rule");

            entity.HasOne(d => d.IdModuleNavigation).WithMany(p => p.SysRules)
                .HasForeignKey(d => d.IdModule)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_sys_Rule_sys_Module");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.SysRules)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_sys_Rule_sys_Role");
        });

        modelBuilder.Entity<SysSettingMenu>(entity =>
        {
            entity.ToTable("sys_SettingMenu");
        });

        modelBuilder.Entity<SysSocial>(entity =>
        {
            entity.ToTable("sys_Social");

            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UrlSocial).HasMaxLength(50);
        });

        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.ToTable("sys_User");

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CardNumber).HasMaxLength(255);
            entity.Property(e => e.Ccv)
                .HasMaxLength(255)
                .HasColumnName("CCV");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Local).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.Time).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(255);
            entity.Property(e => e.ValidFrom).HasColumnType("datetime");
            entity.Property(e => e.ValidTill).HasColumnType("datetime");

            entity.HasOne(d => d.CardNameNavigation).WithMany(p => p.SysUsers)
                .HasForeignKey(d => d.CardName)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_sys_User_Cat_Bank");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.SysUsers)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_sys_User_sys_Role");
        });

        modelBuilder.Entity<SysVilla>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Villa");

            entity.ToTable("sys_Villa");

            entity.Property(e => e.IdPromotion).HasColumnName("idPromotion");
            entity.Property(e => e.ListImg).HasColumnName("ListIMG");
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.TimeCreate).HasColumnType("datetime");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.SysVillas)
                .HasForeignKey(d => d.IdCategory)
                .HasConstraintName("FK_sys_Villa_sys_Categories");

            entity.HasOne(d => d.IdPromotionNavigation).WithMany(p => p.SysVillas)
                .HasForeignKey(d => d.IdPromotion)
                .HasConstraintName("FK_sys_Villa_sys_Promotion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
