namespace SimpleMessageBoard.Tests
{
    using Microsoft.EntityFrameworkCore;
    using SimpleMessageBoard.DAL;
    using System;
    using System.Collections.Generic;

    public class DbFake
    {
        private readonly DbContextOptions<MessageBoardDbContext> _options;

        public DbFake()
        {
            _options = new DbContextOptionsBuilder<MessageBoardDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        public DbFake Seed<TEntity>(params TEntity[] entities) where TEntity : class
        {
            return Seed((IEnumerable<TEntity>)entities);
        }

        public DbFake Seed<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            using (var ctx = new MessageBoardDbContext(_options))
            {
                ctx.Set<TEntity>().AddRange(entities);
                ctx.SaveChanges();
            }

            return this;
        }

        public MessageBoardDbContext GetContext()
        {
            return new MessageBoardDbContext(_options);
        }
    }
}
