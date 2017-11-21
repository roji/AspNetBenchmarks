// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Peregrine;

namespace Benchmarks.Data
{
    public class PeregrineDb : IDb
    {
        private readonly IRandom _random;
        private readonly PGSessionPool _sessionPool;

        public PeregrineDb(IRandom random, PGSessionPool sessionPool)
        {
            _random = random;
            _sessionPool = sessionPool;
        }

        public async Task<World> LoadSingleQueryRow()
        {
            var session = await _sessionPool.Rent();

            try
            {
                return await ReadSingleRow(session, _random.Next(1, 10001));
            }
            finally
            {
                _sessionPool.Return(session);
            }
        }

        public async Task<World[]> LoadMultipleQueriesRows(int count)
        {
            var session = await _sessionPool.Rent();

            try
            {
                var result = new World[count];

                for (var i = 0; i < count; i++)
                {
                    result[i] = await ReadSingleRow(session, _random.Next(1, 10001));
                }

                return result;
            }
            finally
            {
                _sessionPool.Return(session);
            }
        }

        private static async Task<World> ReadSingleRow(PGSession session, int id)
        {
            World world = null;

            World CreateWorld()
            {
                world = new World();

                return world;
            }

            void BindColumn(World w, ReadBuffer readBuffer, int index, int _)
            {
                switch (index)
                {
                    case 0:
                        w.Id = readBuffer.ReadInt();
                        break;
                    case 1:
                        w.RandomNumber = readBuffer.ReadInt();
                        break;
                }
            }

            await session.ExecuteAsync("w", CreateWorld, BindColumn, id);

            return world;
        }

        public async Task<IEnumerable<Fortune>> LoadFortunesRows()
        {
            var result = new List<Fortune>();
            var session = await _sessionPool.Rent();

            try
            {
                await session.ExecuteAsync("f", result, CreateFortune, BindFortuneColumn);
            }
            finally
            {
                _sessionPool.Return(session);
            }

            result.Add(
                new Fortune
                {
                    Message = "Additional fortune added at request time."
                });

            result.Sort();

            return result;
        }

        private static Fortune CreateFortune(List<Fortune> results)
        {
            var fortune = new Fortune();

            results.Add(fortune);

            return fortune;
        }

        private static void BindFortuneColumn(Fortune fortune, ReadBuffer readBuffer, int index, int length)
        {
            switch (index)
            {
                case 0:
                    fortune.Id = readBuffer.ReadInt();
                    break;
                case 1:
                    fortune.Message = readBuffer.ReadString(length);
                    break;
            }
        }

        public Task<World[]> LoadMultipleUpdatesRows(int count)
        {
            throw new NotImplementedException();
        }
    }
}
