﻿// <auto-generated />
using System;
using FridgeServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FridgeServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FridgeServer.Models.Grocery", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Userid");

                    b.Property<bool>("basic");

                    b.Property<bool>("groceryOrBought");

                    b.Property<string>("name")
                        .IsRequired();

                    b.Property<string>("owner");

                    b.Property<long?>("timeout");

                    b.HasKey("id");

                    b.HasIndex("Userid");

                    b.ToTable("userGroceries");
                });

            modelBuilder.Entity("FridgeServer.Models.MoreInformation", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Groceryid");

                    b.Property<bool>("bought");

                    b.Property<long?>("date");

                    b.Property<long?>("lifeTime");

                    b.Property<int?>("no");

                    b.Property<string>("typeOfNo");

                    b.HasKey("id");

                    b.HasIndex("Groceryid");

                    b.ToTable("moreInformations");
                });

            modelBuilder.Entity("FridgeServer.Models.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("firstname");

                    b.Property<string>("lastname");

                    b.Property<byte[]>("passwordHash");

                    b.Property<byte[]>("passwordSalt");

                    b.Property<string>("secretId");

                    b.Property<string>("username");

                    b.HasKey("id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("FridgeServer.Models.UserFriend", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("AreFriends");

                    b.Property<int>("Userid");

                    b.Property<string>("friendEncryptedCode");

                    b.Property<int>("friendUserId");

                    b.Property<string>("friendUsername");

                    b.HasKey("id");

                    b.HasIndex("Userid");

                    b.ToTable("userFriends");
                });

            modelBuilder.Entity("FridgeServer.Models.Grocery", b =>
                {
                    b.HasOne("FridgeServer.Models.User")
                        .WithMany("userGroceries")
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("FridgeServer.Models.MoreInformation", b =>
                {
                    b.HasOne("FridgeServer.Models.Grocery")
                        .WithMany("moreInformations")
                        .HasForeignKey("Groceryid")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("FridgeServer.Models.UserFriend", b =>
                {
                    b.HasOne("FridgeServer.Models.User")
                        .WithMany("userFriends")
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
