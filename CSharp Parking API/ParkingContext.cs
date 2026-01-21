using System;
using System.Collections.Generic;
using CSharp_Parking_API.Models.Generated;
using Microsoft.EntityFrameworkCore;

namespace CSharp_Parking_API;

public partial class ParkingContext : DbContext
{
    public ParkingContext(DbContextOptions<ParkingContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
