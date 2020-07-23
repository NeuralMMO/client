using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

internal class UnsafeRingQueueTests
{
    [Test]
    public void UnsafeRingQueue_Enqueue_Dequeue()
    {
        var test = new UnsafeRingQueue<int>(16, Allocator.Persistent);

        Assert.AreEqual(0, test.Length);

        int item;
        Assert.False(test.TryDequeue(out item));

        test.Enqueue(123);
        Assert.AreEqual(1, test.Length);

        Assert.True(test.TryEnqueue(456));
        Assert.AreEqual(2, test.Length);

        Assert.True(test.TryDequeue(out item));
        Assert.AreEqual(123, item);
        Assert.AreEqual(1, test.Length);

        Assert.AreEqual(456, test.Dequeue());
        Assert.AreEqual(0, test.Length);

        test.Dispose();
    }

    [Test]
    public void UnsafeRingQueue_Throws()
    {
        using (var test = new UnsafeRingQueue<int>(1, Allocator.Persistent))
        {
            Assert.Throws<InvalidOperationException>(() => { test.Dequeue(); });

            Assert.DoesNotThrow(() => { test.Enqueue(123); });
            Assert.Throws<InvalidOperationException>(() => { test.Enqueue(456); });

            int item = 0;
            Assert.DoesNotThrow(() => { item = test.Dequeue(); });
            Assert.AreEqual(123, item);

            Assert.DoesNotThrow(() => { test.Enqueue(456); });
            Assert.DoesNotThrow(() => { item = test.Dequeue(); });
            Assert.AreEqual(456, item);

            Assert.Throws<InvalidOperationException>(() => { test.Dequeue(); });
        }
    }
}
