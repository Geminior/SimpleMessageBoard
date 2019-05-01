namespace SimpleMessageBoard.Tests
{
    using FakeItEasy;
    using Microsoft.Extensions.Logging;
    using SimpleMessageBoard.DTOs;
    using SimpleMessageBoard.Model;
    using SimpleMessageBoard.Services;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class MessageBoardServiceTests
    {
        private readonly DbFake _dbFake;
        private readonly ILogger<MessageBoardService> _logger;

        public MessageBoardServiceTests()
        {
            _logger = A.Dummy<ILogger<MessageBoardService>>();
            _dbFake = new DbFake();
        }

        private void SeedSingleEntry(out BoardMessage msg, out BoardUser author)
        {
            author = new BoardUser
            {
                UserName = "TheAuthor"
            };

            _dbFake.Seed(author);

            msg = new BoardMessage
            {
                Message = "Hello Board",
                AuthorId = author.Id
            };

            _dbFake.Seed(msg);

            msg.Author = author; //Need to add this after the seed or it will be seen as a new entity to add
        }

        [Fact]
        public async Task Getting_a_nonexisting_message_should_return_null()
        {
            //Arrange
            var invalidId = -1;

            var author = new BoardUser
            {
                UserName = "TheAuthor"
            };

            _dbFake.Seed(author);

            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                //Act
                var msgRequesterInvalid = await msgService.GetMessage(invalidId, -1);
                var msgRequesterValid = await msgService.GetMessage(invalidId, author.Id);
                var msgRequesterNone = await msgService.GetMessage(invalidId, null);

                //Assert
                //Result should be the same regardless on the requester id
                Assert.Null(msgRequesterInvalid);
                Assert.Null(msgRequesterValid);
                Assert.Null(msgRequesterNone);
            }
        }

        [Fact]
        public async Task Getting_an_existing_message_should_return_it()
        {
            //Arrange
            SeedSingleEntry(out var msg, out var author);

            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                //Act
                var msgRequesterInvalid = await msgService.GetMessage(msg.Id, -1);
                var msgRequesterValid = await msgService.GetMessage(msg.Id, author.Id);
                var msgRequesterNone = await msgService.GetMessage(msg.Id, null);

                //Assert
                AssertMatch(msgRequesterValid, msg, true);
                AssertMatch(msgRequesterInvalid, msg, false);
                AssertMatch(msgRequesterNone, msg, false);
            }

            void AssertMatch(MessageBoardEntry result, BoardMessage source, bool editable)
            {
                Assert.NotNull(result);
                Assert.Equal(source.Id, result.Id);
                Assert.Equal(source.Message, result.Message);
                Assert.Equal(source.Author.UserName, result.Author);
                Assert.Equal(editable, result.CanEdit);
            }
        }

        [Fact]
        public async Task Creating_a_message_as_a_valid_author_creates_the_message()
        {
            //Arrange
            SeedSingleEntry(out var _, out var author);
            var msg = new MessageBoardEntry
            {
                Message = "Creating as a valid user"
            };

            //Act
            MessageBoardEntry result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                result = await msgService.CreateMessage(msg, author.Id);
            }

            BoardMessage persisted;
            using (var ctx = _dbFake.GetContext())
            {
                persisted = await ctx.Messages.FindAsync(result.Id);
            }

            //Assert
            Assert.NotNull(result);
            Assert.Equal(msg.Message, result.Message);
            Assert.Equal(author.UserName, result.Author);
            Assert.True(result.CanEdit);

            Assert.NotNull(persisted);
            Assert.Equal(msg.Message, persisted.Message);
            Assert.Equal(author.Id, persisted.AuthorId);
        }

        [Fact]
        public async Task Creating_a_message_as_an_invalid_author_creates_nothing()
        {
            //Arrange
            var invalidUserId = 0;
            var msg = new MessageBoardEntry
            {
                Message = "Creating as an invalid user"
            };

            int preMessageCount;
            int postMessageCount;

            //Act
            MessageBoardEntry result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                preMessageCount = ctx.Messages.Count();
                result = await msgService.CreateMessage(msg, invalidUserId);
            }

            using (var ctx = _dbFake.GetContext())
            {
                postMessageCount = ctx.Messages.Count();
            }

            //Assert
            Assert.Null(result);
            Assert.Equal(preMessageCount, postMessageCount);
        }

        [Fact]
        public async Task Updating_an_invalid_message_should_fail()
        {
            //Arrange
            SeedSingleEntry(out var _, out var author);
            var editedMsg = new MessageBoardEntry
            {
                Id = -1,
                Message = "Invalid message"
            };

            //Act
            bool result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);
                
                result = await msgService.UpdateMessage(editedMsg, author.Id);
            }

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Updating_a_valid_message_of_another_author_should_fail_and_do_nothing()
        {
            //Arrange
            SeedSingleEntry(out var msg, out var author);
            var editedMsg = new MessageBoardEntry
            {
                Id = msg.Id,
                Message = "A new message"
            };

            //Act
            bool result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                result = await msgService.UpdateMessage(editedMsg, author.Id + 1);
            }

            BoardMessage persisted;
            using (var ctx = _dbFake.GetContext())
            {
                persisted = await ctx.Messages.FindAsync(msg.Id);
            }

            //Assert
            Assert.False(result);
            Assert.Equal(msg.Message, persisted.Message);
            Assert.Equal(msg.AuthorId, persisted.AuthorId);
        }

        [Fact]
        public async Task Updating_a_valid_message_as_the_author_should_succeed()
        {
            //Arrange
            SeedSingleEntry(out var msg, out var author);
            var editedMsg = new MessageBoardEntry
            {
                Id = msg.Id,
                Message = "A new message"
            };

            //Act
            bool result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                result = await msgService.UpdateMessage(editedMsg, author.Id);
            }

            BoardMessage persisted;
            using (var ctx = _dbFake.GetContext())
            {
                persisted = await ctx.Messages.FindAsync(msg.Id);
            }

            //Assert
            Assert.True(result);
            Assert.Equal(editedMsg.Message, persisted.Message);
            Assert.Equal(msg.AuthorId, persisted.AuthorId);
        }

        [Fact]
        public async Task Deleting_a_valid_message_as_the_author_should_succeed()
        {
            //Arrange
            SeedSingleEntry(out var msg, out var author);

            //Act
            bool result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                result = await msgService.DeleteMessage(msg.Id, author.Id);
            }

            BoardMessage persisted;
            using (var ctx = _dbFake.GetContext())
            {
                persisted = await ctx.Messages.FindAsync(msg.Id);
            }

            //Assert
            Assert.True(result);
            Assert.Null(persisted);
        }

        [Fact]
        public async Task Deleting_an_invalid_message_should_succeed_but_make_no_change()
        {
            //Arrange
            SeedSingleEntry(out var msg, out var author);

            int preMessageCount;
            int postMessageCount;

            //Act
            bool result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                preMessageCount = ctx.Messages.Count();
                result = await msgService.DeleteMessage(msg.Id + 1, author.Id);
            }

            using (var ctx = _dbFake.GetContext())
            {
                postMessageCount = ctx.Messages.Count();
            }

            //Assert
            Assert.True(result);
            Assert.Equal(preMessageCount, postMessageCount);
        }

        [Fact]
        public async Task Deleting_a_valid_message_form_another_author_should_fail_and_make_no_change()
        {
            //Arrange
            SeedSingleEntry(out var msg, out var author);

            int preMessageCount;
            int postMessageCount;

            //Act
            bool result;
            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                preMessageCount = ctx.Messages.Count();
                result = await msgService.DeleteMessage(msg.Id, author.Id + 1);
            }

            using (var ctx = _dbFake.GetContext())
            {
                postMessageCount = ctx.Messages.Count();
            }

            //Assert
            Assert.False(result);
            Assert.Equal(preMessageCount, postMessageCount);
        }

        [Fact]
        public async Task Getting_all_messages_should_work()
        {
            //Arrange
            var authors = new[]
            {
                new BoardUser
                {
                    UserName = "Author1"
                },
                new BoardUser
                {
                    UserName = "Author2"
                },
                new BoardUser
                {
                    UserName = "Author3"
                }
            };

            _dbFake.Seed(authors);

            var msgs = new[]
            {
                new BoardMessage
                {
                    Message = "Hello Board",
                    AuthorId = authors[0].Id
                },
                new BoardMessage
                {
                    Message = "Goodbye Board",
                    AuthorId = authors[1].Id
                },
                new BoardMessage
                {
                    Message = "Hello again",
                    AuthorId = authors[2].Id
                },
            };

            _dbFake.Seed(msgs);

            for (int i = 0; i < msgs.Length; i++)
            {
                msgs[i].Author = authors[i];
            }

            using (var ctx = _dbFake.GetContext())
            {
                var msgService = new MessageBoardService(ctx, _logger);

                //Act
                var msgRequesterInvalid = await msgService.GetAllMessages(-1);
                var msgRequesterValid = await msgService.GetAllMessages(authors[1].Id);
                var msgRequesterNone = await msgService.GetAllMessages(null);

                //Assert
                AssertMatch(msgRequesterValid, msgs, 1);
                AssertMatch(msgRequesterInvalid, msgs, -1);
                AssertMatch(msgRequesterNone, msgs, -1);
            }

            void AssertMatch(IEnumerable<MessageBoardEntry> result, BoardMessage[] source, int editableIdx)
            {
                Assert.NotNull(result);

                var resultArr = result.ToArray();
                Assert.Equal(resultArr.Length, source.Length);

                for (int i = 0; i < resultArr.Length; i++)
                {
                    Assert.Equal(source[i].Id, resultArr[i].Id);
                    Assert.Equal(source[i].Message, resultArr[i].Message);
                    Assert.Equal(source[i].Author.UserName, resultArr[i].Author);
                    Assert.Equal(editableIdx == i, resultArr[i].CanEdit);
                }
            }
        }
    }
}
