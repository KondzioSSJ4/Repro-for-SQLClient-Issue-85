using Microsoft.EntityFrameworkCore;
using SQLClient_Issue85;
using SQLClient_Issue85.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class UnitTest1 : IDisposable
    {

        public UnitTest1()
        {
            Clean();
        }

        public void Dispose()
        {
            Clean();
        }

        private void Clean()
        {
            using (var context = new NorthwindContext())
            {
                context.Products.RemoveRange(context.Products.Where(s => !s.SupplierId.HasValue));
                context.SaveChanges();
            }
        }

        [Theory]
        [MemberData(nameof(Tests_Cases))]
        public async Task Tests(int iteration)
        {
            var data = Enumerable.Range(0, 10000)
                .Select(i => new Products()
                {
                    CategoryId = null,
                    Discontinued = false,
                    ProductName = $"lol_{i}",
                }).ToList();

            var bulk = new SimpleBulkCopy<Products>(NorthwindContext.ConnectionString, new ProductDefinition(), 2);
            using (var blockingContext = new NorthwindContext())
            {
                using (var blockingTran = blockingContext.Database.BeginTransaction())
                {
                    blockingContext.Products.FromSqlRaw("SELECT TOP 1 * FROM  Products (TABLOCKX)").ToList();

                    var releaseTask = Task.Delay(4000).ContinueWith(e =>
                    {
                        blockingTran.Commit();
                    });

                    await Task.WhenAll(releaseTask, DoSave(data, bulk));
                }
            }
        }

        private static async Task DoSave(List<Products> data, SimpleBulkCopy<Products> bulk)
        {
            try
            {
                await bulk.BegginSave();

                await bulk.Save(data);

                await bulk.FinalizeSaving(false);
            }
            catch(System.Threading.Tasks.TaskCanceledException exc)
            {
                // It's expected result
            }
        }

        public static IEnumerable<object[]> Tests_Cases()
        {
            return Enumerable.Range(0, 100).Select(o => new object[] { o }).ToList();
        }
    }
}
