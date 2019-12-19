﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using System;

namespace ShareBook.Repository.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180807184445_AlterTables_AddIdBookUser")]
    partial class AlterTables_AddIdBookUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ShareBook.Domain.Book", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Approved");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<Guid>("CategoryId");

                    b.Property<DateTime?>("CreationDate");

                    b.Property<int>("FreightOption");

                    b.Property<string>("ImageSlug")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<Guid?>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("UserId");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("ShareBook.Domain.BookUser", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("BookId");

                    b.Property<Guid>("UserId");

                    b.Property<DateTime?>("CreationDate");

                    b.HasKey("Id", "BookId", "UserId");

                    b.HasIndex("BookId");

                    b.HasIndex("UserId");

                    b.ToTable("BookUser");
                });

            modelBuilder.Entity("ShareBook.Domain.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CreationDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ShareBook.Domain.LogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CreationDate");

                    b.Property<Guid>("EntityId");

                    b.Property<string>("EntityName");

                    b.Property<DateTime>("LogDateTime");

                    b.Property<string>("Operation");

                    b.Property<Guid?>("UserId");

                    b.Property<string>("ValuesChanges");

                    b.HasKey("Id");

                    b.ToTable("LogEntries");
                });

            modelBuilder.Entity("ShareBook.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("CreationDate");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Linkedin")
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50);

                    b.Property<string>("Phone")
                        .HasColumnType("varchar(30)")
                        .HasMaxLength(30);

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("varchar(15)")
                        .HasMaxLength(15);

                    b.Property<int>("Profile");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ShareBook.Domain.Book", b =>
                {
                    b.HasOne("ShareBook.Domain.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ShareBook.Domain.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("ShareBook.Domain.BookUser", b =>
                {
                    b.HasOne("ShareBook.Domain.Book", "Book")
                        .WithMany("BookUsers")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ShareBook.Domain.User", "User")
                        .WithMany("BookUsers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
