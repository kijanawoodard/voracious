using System.IO;
using Newtonsoft.Json;
using Voron;
using Voron.Impl;
using Xunit;

namespace Voracious.Tests
{
    public class Excercises
    {
        [Fact]
        public void WriteSomethingToVoron()
        {
            var serializer = new JsonSerializer();

            using (var storage = new StorageEnvironment(StorageEnvironmentOptions.GetInMemory()))
            {
                using (var tx = storage.NewTransaction(TransactionFlags.ReadWrite))
                {
                    storage.CreateTree(tx, "foos");
                    tx.Commit();
                }

                {
                    var ms = new MemoryStream();
                    var batch = new WriteBatch();
                    var foo = new Foo { Id = "hello", Value = 99 };

                    using (var writer = new StreamWriter(ms))
                    {
                        serializer.Serialize(new JsonTextWriter(writer), foo);
                        writer.Flush();

                        ms.Position = 0;
                        //var key = new Slice(EndianBitConverter.Big.GetBytes(counter++));
                        batch.Add(foo.Id, ms, "foos");
                        storage.Writer.Write(batch);
                    }
                }

                using (var tx = storage.NewTransaction(TransactionFlags.Read))
                {
                    var foos = tx.GetTree("foos");
                    var readResult = foos.Read(tx, "hello");
                    using (var stream = readResult.Reader.AsStream())
                    {
                        var foo = serializer.Deserialize<Foo>(new JsonTextReader(new StreamReader(stream)));
                        Assert.Equal(99, foo.Value);
                    }
                }
            }
        }

        public class Foo
        {
            public string Id { get; set; }
            public int Value { get; set; }
        }
    }
}
