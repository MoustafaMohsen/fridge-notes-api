﻿// <auto-generated />
using FridgeServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace FridgeServer.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20180409081216_GroceryUpdate1")]
    partial class GroceryUpdate1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FridgeServer.Models.Grocery", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CurrentCount");

                    b.Property<int?>("DueCount");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long?>("Timeout");

                    b.Property<bool>("basic");

                    b.HasKey("Id");

                    b.ToTable("Grocery");
                });
#pragma warning restore 612, 618
        }
    }
}