﻿// <auto-generated />
using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DiscordBot.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.8");

            modelBuilder.Entity("DiscordBot.Models.Auth", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("IP")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("Serverid")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Token")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Auth");
                });

            modelBuilder.Entity("DiscordBot.Models.Notify", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Channelid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Serverid")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("Notify");
                });

            modelBuilder.Entity("DiscordBot.Models.OnHold", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("IP")
                        .HasColumnType("TEXT");

                    b.Property<string>("Token")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("OnHold");
                });

            modelBuilder.Entity("DiscordBot.Models.OnJoin", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Channelid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Messageid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Roleid")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("Serverid")
                        .HasColumnType("INTEGER");

                    b.Property<int>("sevent")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("OnJoin");
                });
#pragma warning restore 612, 618
        }
    }
}
