﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using assetgrid_backend.Models;

#nullable disable

namespace assetgrid_backend.Migrations.MySql
{
    [DbContext(typeof(AssetgridDbContext))]
    partial class AssetgridDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("assetgrid_backend.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("assetgrid_backend.Models.AccountUniqueIdentifier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("AccountUniqueIdentifiers");
                });

            modelBuilder.Entity("assetgrid_backend.models.Automation.TransactionAutomation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Actions")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Query")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<bool>("TriggerOnCreate")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("TriggerOnModify")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("TransactionAutomations");
                });

            modelBuilder.Entity("assetgrid_backend.models.Automation.UserTransactionAutomation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Permissions")
                        .HasColumnType("int");

                    b.Property<int>("TransactionAutomationId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TransactionAutomationId");

                    b.HasIndex("UserId");

                    b.ToTable("UserTransactionAutomations");
                });

            modelBuilder.Entity("assetgrid_backend.Models.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.Property<int?>("DestinationAccountId")
                        .HasColumnType("int");

                    b.Property<bool>("IsSplit")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("SourceAccountId")
                        .HasColumnType("int");

                    b.Property<long>("Total")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("DateTime");

                    b.HasIndex("DestinationAccountId");

                    b.HasIndex("SourceAccountId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("assetgrid_backend.Models.TransactionLine", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<long>("Amount")
                        .HasColumnType("bigint");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.Property<int>("Order")
                        .HasColumnType("int");

                    b.Property<int>("TransactionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Category")
                        .HasFilter("Category IS NOT NULL");

                    b.HasIndex("TransactionId");

                    b.ToTable("TransactionLines");
                });

            modelBuilder.Entity("assetgrid_backend.Models.TransactionUniqueIdentifier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("TransactionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Identifier");

                    b.HasIndex("TransactionId");

                    b.ToTable("TransactionUniqueIdentifiers");
                });

            modelBuilder.Entity("assetgrid_backend.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("HashedPassword")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("NormalizedEmail")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("assetgrid_backend.Models.UserAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<bool>("Favorite")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IncludeInNetWorth")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("Permissions")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("UserId");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("assetgrid_backend.Models.UserCsvImportProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ImportProfile")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<string>("ProfileName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserCsvImportProfiles");
                });

            modelBuilder.Entity("assetgrid_backend.Models.UserPreferences", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("DateFormat")
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("DateTimeFormat")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("DecimalDigits")
                        .HasColumnType("int");

                    b.Property<string>("DecimalSeparator")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("ThousandsSeparator")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserPreferences");
                });

            modelBuilder.Entity("assetgrid_backend.Models.AccountUniqueIdentifier", b =>
                {
                    b.HasOne("assetgrid_backend.Models.Account", "Account")
                        .WithMany("Identifiers")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("assetgrid_backend.models.Automation.UserTransactionAutomation", b =>
                {
                    b.HasOne("assetgrid_backend.models.Automation.TransactionAutomation", "TransactionAutomation")
                        .WithMany()
                        .HasForeignKey("TransactionAutomationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("assetgrid_backend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TransactionAutomation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("assetgrid_backend.Models.Transaction", b =>
                {
                    b.HasOne("assetgrid_backend.Models.Account", "DestinationAccount")
                        .WithMany()
                        .HasForeignKey("DestinationAccountId");

                    b.HasOne("assetgrid_backend.Models.Account", "SourceAccount")
                        .WithMany()
                        .HasForeignKey("SourceAccountId");

                    b.Navigation("DestinationAccount");

                    b.Navigation("SourceAccount");
                });

            modelBuilder.Entity("assetgrid_backend.Models.TransactionLine", b =>
                {
                    b.HasOne("assetgrid_backend.Models.Transaction", "Transaction")
                        .WithMany("TransactionLines")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("assetgrid_backend.Models.TransactionUniqueIdentifier", b =>
                {
                    b.HasOne("assetgrid_backend.Models.Transaction", "Transaction")
                        .WithMany("Identifiers")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("assetgrid_backend.Models.UserAccount", b =>
                {
                    b.HasOne("assetgrid_backend.Models.Account", "Account")
                        .WithMany("Users")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("assetgrid_backend.Models.User", "User")
                        .WithMany("Accounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("User");
                });

            modelBuilder.Entity("assetgrid_backend.Models.UserCsvImportProfile", b =>
                {
                    b.HasOne("assetgrid_backend.Models.User", "User")
                        .WithMany("CsvImportProfiles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("assetgrid_backend.Models.UserPreferences", b =>
                {
                    b.HasOne("assetgrid_backend.Models.User", "User")
                        .WithOne("Preferences")
                        .HasForeignKey("assetgrid_backend.Models.UserPreferences", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("assetgrid_backend.Models.Account", b =>
                {
                    b.Navigation("Identifiers");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("assetgrid_backend.Models.Transaction", b =>
                {
                    b.Navigation("Identifiers");

                    b.Navigation("TransactionLines");
                });

            modelBuilder.Entity("assetgrid_backend.Models.User", b =>
                {
                    b.Navigation("Accounts");

                    b.Navigation("CsvImportProfiles");

                    b.Navigation("Preferences")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
