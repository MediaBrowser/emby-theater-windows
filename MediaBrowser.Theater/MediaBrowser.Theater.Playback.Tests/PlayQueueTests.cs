using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MediaBrowser.Theater.Playback.Tests
{
    [TestClass]
    public class PlayQueueTests
    {
        [TestMethod]
        public async Task PlayOrder_SequentialForwardNoRepeat()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue();
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveSameCount(media);
            played.Should().ContainInOrder(media);
        }

        [TestMethod]
        public async Task PlayOrder_SequentialForwardRepeatAll()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.All };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < queue.Count*3; i++) {
                if (await sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(media.Length*3);
            played.Take(media.Length).Should().ContainInOrder(media);
            played.Skip(media.Length).Take(media.Length).Should().ContainInOrder(media);
            played.Skip(media.Length*2).Take(media.Length).Should().ContainInOrder(media);
        }

        [TestMethod]
        public async Task PlayOrder_SequentialForwardRepeatOne_First()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.Single };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (await sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[0], media[0], media[0], media[0], media[0] });
        }

        [TestMethod]
        public async Task PlayOrder_SequentialForwardRepeatOne_Intermediate()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (await sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[1], media[1], media[1], media[1], media[1] });

            queue.RepeatMode = RepeatMode.None;

            await sequence.Next();
            sequence.Current.Should().Be(media[2]);
        }

        [TestMethod]
        public async Task PlayOrder_SequentialForwardRepeatOne_Last()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();
            await sequence.Next();
            await sequence.Next();
            await sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (await sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[4], media[4], media[4], media[4], media[4] });
        }

        [TestMethod]
        public async Task PlayOrder_SequentialReverseNoRepeat()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue();
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            while (await sequence.Previous()) {
                played.Add(sequence.Current);
            }

            played.Should().BeEmpty();
        }

        [TestMethod]
        public async Task PlayOrder_SequentialReverseNoRepeat_Intermediate()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue();
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();
            sequence.Next();

            var played = new List<Media>();
            while (await sequence.Previous()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(2);
            played.Should().ContainInOrder(new[] { media[1], media[0] });
        }

        [TestMethod]
        public async Task PlayOrder_SequentialReverseRepeatAll()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.All };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < queue.Count*3; i++) {
                if (await sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(media.Length*3);
            played.Take(media.Length).Should().ContainInOrder(media.Reverse());
            played.Skip(media.Length).Take(media.Length).Should().ContainInOrder(media.Reverse());
            played.Skip(media.Length*2).Take(media.Length).Should().ContainInOrder(media.Reverse());
        }

        [TestMethod]
        public async Task PlayOrder_SequentialReverseRepeatOne_First()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.Single };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (await sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[0], media[0], media[0], media[0], media[0] });
        }

        [TestMethod]
        public async Task PlayOrder_SequentialReverseRepeatOne_Intermediate()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (await sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[1], media[1], media[1], media[1], media[1] });

            queue.RepeatMode = RepeatMode.None;

            await sequence.Next();
            sequence.Current.Should().Be(media[2]);
        }

        [TestMethod]
        public async Task PlayOrder_SequentialReverseRepeatOne_Last()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();
            await sequence.Next();
            await sequence.Next();
            await sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (await sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[4], media[4], media[4], media[4], media[4] });
        }

        [TestMethod]
        public async Task PlayOrder_ForwardThenReverseThenForward()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var forward = new List<Media>();
            while (await sequence.Next()) {
                forward.Add(sequence.Current);
            }

            forward.Should().HaveCount(media.Length);
            forward.Should().ContainInOrder(media);

            var reverse = new List<Media>();
            while (await sequence.Previous()) {
                reverse.Add(sequence.Current);
            }

            reverse.Should().HaveCount(media.Length);
            reverse.Should().ContainInOrder(media.Reverse());

            forward.Clear();
            while (await sequence.Next()) {
                forward.Add(sequence.Current);
            }

            forward.Should().HaveCount(media.Length);
            forward.Should().ContainInOrder(media);
        }

        [TestMethod]
        public async Task PlayOrder_Insert_AfterCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            Media additional = new Mock<Media>().Object;
            queue.Insert(2, additional);


            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(2);
            played.Should().ContainInOrder(new[] { additional, media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_Insert_OnCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            Media additional = new Mock<Media>().Object;
            queue.Insert(1, additional);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_Insert_BeforeCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            Media additional = new Mock<Media>().Object;
            queue.Insert(0, additional);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_InsertMultiple_AfterCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            Media additional1 = new Mock<Media>().Object;
            Media additional2 = new Mock<Media>().Object;
            Media additional3 = new Mock<Media>().Object;
            queue.Insert(2, additional1);
            queue.Insert(3, additional2);
            queue.Insert(4, additional3);


            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(4);
            played.Should().ContainInOrder(new[] { additional1, additional2, additional3, media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_InsertMultiple_OnCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            Media additional1 = new Mock<Media>().Object;
            Media additional2 = new Mock<Media>().Object;
            Media additional3 = new Mock<Media>().Object;
            queue.Insert(1, additional1);
            queue.Insert(2, additional2);
            queue.Insert(3, additional3);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_InsertMultiple_BeforeCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            Media additional1 = new Mock<Media>().Object;
            Media additional2 = new Mock<Media>().Object;
            Media additional3 = new Mock<Media>().Object;
            queue.Insert(0, additional1);
            queue.Insert(1, additional2);
            queue.Insert(2, additional3);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public async Task Clear()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            (await sequence.Next()).Should().BeTrue();

            queue.Clear();

            (await sequence.Next()).Should().BeFalse();
        }

        [TestMethod]
        public async Task Clear_WhileShuffled()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None, SortMode = SortMode.Shuffle };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            (await sequence.Next()).Should().BeTrue();

            queue.Clear();

            (await sequence.Next()).Should().BeFalse();
        }

        [TestMethod]
        public async Task PlayOrder_Remove_BeforeCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            queue.Remove(media[0]);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_Remove_OnCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            queue.Remove(media[1]);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public async Task PlayOrder_Remove_OnCurrentIndex_MovePrevious()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            queue.Remove(media[1]);

            var played = new List<Media>();
            while (await sequence.Previous()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[0] });
        }

        [TestMethod]
        public async Task PlayOrder_Remove_AfterCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            await sequence.Next();
            await sequence.Next();

            queue.Remove(media[2]);

            (await sequence.Next()).Should().BeFalse();
        }

        [TestMethod]
        public async Task PlayOrder_Shuffled_Forward()
        {
            Media[] media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToArray();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle };

            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();
            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveSameCount(media);
            played.Should().Contain(media);
        }

        [TestMethod]
        public async Task PlayOrder_Shuffled_NewItemsDoPlay()
        {
            Media[] media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToArray();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle };

            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var previous = new List<Media>();
            for (int i = 0; i < 10; i++) {
                if (await sequence.Next()) {
                    previous.Add(sequence.Current);
                }
            }

            Media[] additional = Enumerable.Range(0, 5).Select(i => new Mock<Media>().Object).ToArray();
            queue.AddRange(additional);

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().Contain(media.Where(m => !previous.Contains(m)), additional);
            played.Should().NotContain(previous);
        }

        [TestMethod]
        public async Task PlayOrder_Shuffled_RemovedItemsDontPlay()
        {
            Media[] media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToArray();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle };

            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var previous = new List<Media>();
            for (int i = 0; i < 10; i++) {
                if (await sequence.Next()) {
                    previous.Add(sequence.Current);
                }
            }

            List<Media> toRemove = media.Where(m => !previous.Contains(m)).Take(10).ToList();
            foreach (Media item in toRemove) {
                queue.Remove(item);
            }

            var played = new List<Media>();
            while (await sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().Contain(media.Where(m => !previous.Contains(m) && !toRemove.Contains(m)));
            played.Should().NotContain(previous);
            played.Should().NotContain(toRemove);
        }

        [TestMethod]
        public async Task PlayOrder_ShuffleDisabled_PlaysNextLinear()
        {
            List<Media> media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToList();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle, RepeatMode = RepeatMode.All };

            queue.AddRange(media);

            IPlaySequence<Media> sequence = queue.GetPlayOrder();

            var previous = new List<Media>();
            for (int i = 0; i < 10; i++) {
                await sequence.Next();
                previous.Add(sequence.Current);
            }

            queue.SortMode = SortMode.Linear;

            int previousIndex = media.IndexOf(previous.Last());
            Media expected = previousIndex == media.Count - 1 ? media.First() : media[previousIndex + 1];

            await sequence.Next();
            sequence.Current.Should().Be(expected);
        }
    }
}