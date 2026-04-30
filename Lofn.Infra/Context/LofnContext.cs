using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Lofn.Infra.Context;

public partial class LofnContext : DbContext
{
    public LofnContext()
    {
    }

    public LofnContext(DbContextOptions<LofnContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreUser> StoreUsers { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<ProductTypeFilter> ProductTypeFilters { get; set; }

    public virtual DbSet<ProductTypeFilterAllowedValue> ProductTypeFilterAllowedValues { get; set; }

    public virtual DbSet<ProductTypeCustomizationGroup> ProductTypeCustomizationGroups { get; set; }

    public virtual DbSet<ProductTypeCustomizationOption> ProductTypeCustomizationOptions { get; set; }

    public virtual DbSet<ProductFilterValue> ProductFilterValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("lofn_products_pkey");

            entity.ToTable("lofn_products");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Limit).HasColumnName("limit");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Discount)
                .HasDefaultValue(0.0)
                .HasColumnName("discount");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lofn_product_store");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("slug");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.ProductType)
                .HasDefaultValue(1)
                .HasColumnName("product_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Featured)
                .HasDefaultValue(false)
                .HasColumnName("featured");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lofn_product_category");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("lofn_categories_pkey");

            entity.ToTable("lofn_categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnName("slug");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");

            entity.HasOne(d => d.Store).WithMany(p => p.Categories)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lofn_category_store");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_lofn_category_parent");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Categories)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_lofn_categories_product_type");

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("ix_lofn_categories_slug_global")
                .HasFilter("store_id IS NULL");

            entity.HasIndex(e => e.ParentId)
                .HasDatabaseName("ix_lofn_categories_parent_id")
                .HasFilter("parent_id IS NOT NULL");

            entity.HasIndex(e => e.ProductTypeId)
                .HasDatabaseName("ix_lofn_categories_product_type_id")
                .HasFilter("product_type_id IS NOT NULL");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("lofn_stores_pkey");

            entity.ToTable("lofn_stores");

            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("slug");
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("ix_lofn_stores_slug");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Logo)
                .HasMaxLength(150)
                .HasColumnName("logo");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
        });

        modelBuilder.Entity<StoreUser>(entity =>
        {
            entity.HasKey(e => e.StoreUserId).HasName("lofn_store_users_pkey");

            entity.ToTable("lofn_store_users");

            entity.Property(e => e.StoreUserId).HasColumnName("store_user_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreUsers)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_store_user_store");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("lofn_product_images_pkey");

            entity.ToTable("lofn_product_images");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Image)
                .HasMaxLength(150)
                .HasColumnName("image");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_image_product");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.ProductTypeId).HasName("lofn_product_types_pkey");
            entity.ToTable("lofn_product_types");

            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("ix_lofn_product_types_name_unique");
        });

        modelBuilder.Entity<ProductTypeFilter>(entity =>
        {
            entity.HasKey(e => e.FilterId).HasName("lofn_product_type_filters_pkey");
            entity.ToTable("lofn_product_type_filters");

            entity.Property(e => e.FilterId).HasColumnName("filter_id");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("label");
            entity.Property(e => e.DataType)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("data_type");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(false)
                .HasColumnName("is_required");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Filters)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_type_filter_type");

            entity.HasIndex(e => new { e.ProductTypeId, e.Label })
                .IsUnique()
                .HasDatabaseName("ix_lofn_product_type_filter_label_unique");
        });

        modelBuilder.Entity<ProductTypeFilterAllowedValue>(entity =>
        {
            entity.HasKey(e => e.AllowedValueId).HasName("lofn_product_type_filter_allowed_values_pkey");
            entity.ToTable("lofn_product_type_filter_allowed_values");

            entity.Property(e => e.AllowedValueId).HasColumnName("allowed_value_id");
            entity.Property(e => e.FilterId).HasColumnName("filter_id");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("value");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");

            entity.HasOne(d => d.Filter).WithMany(p => p.AllowedValues)
                .HasForeignKey(d => d.FilterId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_type_filter_allowed_value_filter");

            entity.HasIndex(e => new { e.FilterId, e.Value })
                .IsUnique()
                .HasDatabaseName("ix_lofn_product_type_filter_allowed_value_unique");
        });

        modelBuilder.Entity<ProductTypeCustomizationGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("lofn_product_type_customization_groups_pkey");
            entity.ToTable("lofn_product_type_customization_groups");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("label");
            entity.Property(e => e.SelectionMode)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("selection_mode");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(false)
                .HasColumnName("is_required");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ProductType).WithMany(p => p.CustomizationGroups)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_type_customization_group_type");

            entity.HasIndex(e => new { e.ProductTypeId, e.Label })
                .IsUnique()
                .HasDatabaseName("ix_lofn_product_type_customization_group_label_unique");
        });

        modelBuilder.Entity<ProductTypeCustomizationOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("lofn_product_type_customization_options_pkey");
            entity.ToTable("lofn_product_type_customization_options");

            entity.Property(e => e.OptionId).HasColumnName("option_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("label");
            entity.Property(e => e.PriceDeltaCents)
                .HasDefaultValue(0L)
                .HasColumnName("price_delta_cents");
            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");
            entity.Property(e => e.DisplayOrder)
                .HasDefaultValue(0)
                .HasColumnName("display_order");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Group).WithMany(p => p.Options)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_type_customization_option_group");

            entity.HasIndex(e => new { e.GroupId, e.Label })
                .IsUnique()
                .HasDatabaseName("ix_lofn_product_type_customization_option_label_unique");
        });

        modelBuilder.Entity<ProductFilterValue>(entity =>
        {
            entity.HasKey(e => e.ProductFilterValueId).HasName("lofn_product_filter_values_pkey");
            entity.ToTable("lofn_product_filter_values");

            entity.Property(e => e.ProductFilterValueId).HasColumnName("product_filter_value_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.FilterId).HasColumnName("filter_id");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("value");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Product).WithMany(p => p.FilterValues)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_filter_value_product");

            entity.HasOne(d => d.Filter).WithMany()
                .HasForeignKey(d => d.FilterId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_lofn_product_filter_value_filter");

            entity.HasIndex(e => new { e.ProductId, e.FilterId })
                .IsUnique()
                .HasDatabaseName("ix_lofn_product_filter_value_product_filter_unique");

            entity.HasIndex(e => new { e.FilterId, e.Value })
                .HasDatabaseName("ix_lofn_product_filter_value_filter_value");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
