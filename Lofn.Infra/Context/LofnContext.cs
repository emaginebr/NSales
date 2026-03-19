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

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreUser> StoreUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_store");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_item_order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_item_product");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Image)
                .HasMaxLength(150)
                .HasColumnName("image");
            entity.Property(e => e.Limit).HasColumnName("limit");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Price).HasColumnName("price");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_store");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("slug");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_category");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("slug");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.StoreId).HasColumnName("store_id");

            entity.HasOne(d => d.Store).WithMany(p => p.Categories)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_category_store");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("stores_pkey");

            entity.ToTable("stores");

            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("slug");
            entity.HasIndex(e => e.Slug)
                .IsUnique()
                .HasDatabaseName("ix_stores_slug");
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
            entity.HasKey(e => e.StoreUserId).HasName("store_users_pkey");

            entity.ToTable("store_users");

            entity.Property(e => e.StoreUserId).HasColumnName("store_user_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreUsers)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_store_user_store");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("product_images_pkey");

            entity.ToTable("product_images");

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
                .HasConstraintName("fk_product_image_product");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
