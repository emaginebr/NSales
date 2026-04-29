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

            entity.HasOne(d => d.Store).WithMany(p => p.Categories)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lofn_category_store");

            entity.HasOne(d => d.Parent).WithMany(p => p.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_lofn_category_parent");

            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("ix_lofn_categories_slug_global")
                .HasFilter("store_id IS NULL");

            entity.HasIndex(e => e.ParentId)
                .HasDatabaseName("ix_lofn_categories_parent_id")
                .HasFilter("parent_id IS NOT NULL");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
