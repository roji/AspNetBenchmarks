// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Benchmarks.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Benchmarks.Data
{
    public class EfDb : IDb
    {
        private readonly IRandom _random;
        private readonly string _connectionString;

        public EfDb(IRandom random, IOptions<AppSettings> appSettings)
        {
            _random = random;
            _connectionString = appSettings.Value.ConnectionString;
        }

        public Task<World> LoadSingleQueryRow()
            => throw new NotSupportedException();

        public async Task<World[]> LoadMultipleQueriesRows(int count)
            => throw new NotSupportedException();

        public async Task<World[]> LoadMultipleUpdatesRows(int count)
            => throw new NotSupportedException();

#if NETCOREAPP2_1 || NETCOREAPP2_2
        private static readonly Func<ApplicationDbContext, AsyncEnumerable<Fortune>> _fortunesQuery
            = EF.CompileAsyncQuery((ApplicationDbContext context) => context.Fortune);

        public async Task<IEnumerable<Fortune>> LoadFortunesRows()
        {
            var result = await _fortunesQuery(_dbContext).ToListAsync();

            result.Add(new Fortune { Message = "Additional fortune added at request time." });
            result.Sort();

            return result;
        }

#else
        // private static readonly Func<ApplicationDbContext, IAsyncEnumerable<Fortune>> _fortunesQuery
        //     = EF.CompileAsyncQuery((ApplicationDbContext context) => context.Fortune);

        // private static readonly Func<ApplicationDbContext, IQueryable<Fortune>> _fortunesQuery
        //     = CompiledQuery.Compile<ApplicationDbContext, IQueryable<Fortune>>(ctx => ctx.Fortune);

        public async Task<IEnumerable<Fortune>> LoadFortunesRows()
        {
            var result = new List<Fortune>();

            using var dbContext = new ApplicationDbContext(_connectionString);

            // foreach (var fortune in _fortunesQuery(_dbContext))
            foreach (var fortune in dbContext.Fortune)
            {
                result.Add(fortune);
            }

            result.Add(new Fortune { Message = "Additional fortune added at request time." });
            
            result.Sort();

            return result;
        }

#endif
    }

    class NpgsqlConfiguration : DbConfiguration
    {
        public NpgsqlConfiguration()
        {
            SetProviderFactory("Npgsql", NpgsqlFactory.Instance);
            SetProviderServices("Npgsql", NpgsqlServices.Instance);
            SetDefaultConnectionFactory(new NpgsqlConnectionFactory());
        }
    }
}
