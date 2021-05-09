// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Entity;

namespace Benchmarks.Data
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(string connectionString)
            : base(connectionString)
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        public DbSet<World> World { get; set; }

        public DbSet<Fortune> Fortune { get; set; }
    }
}