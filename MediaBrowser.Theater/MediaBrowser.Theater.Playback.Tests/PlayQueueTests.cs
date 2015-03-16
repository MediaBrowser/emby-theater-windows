using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Model.Dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MediaBrowser.Theater.Playback.Tests
{
    [TestClass]
    public class PlayQueueTests
    {
        [TestMethod]
        public void PlayOrder_SequentialForwardNoRepeat()
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

            var sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveSameCount(media);
            played.Should().ContainInOrder(media);
        }

        [TestMethod]
        public void PlayOrder_SequentialForwardRepeatAll()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue {RepeatMode = RepeatMode.All};
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < queue.Count*3; i++) {
                if (sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(media.Length*3);
            played.Take(media.Length).Should().ContainInOrder(media);
            played.Skip(media.Length).Take(media.Length).Should().ContainInOrder(media);
            played.Skip(media.Length*2).Take(media.Length).Should().ContainInOrder(media);
        }

        [TestMethod]
        public void PlayOrder_SequentialForwardRepeatOne_First()
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

            var sequence = queue.GetPlayOrder();
            
            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] {media[0], media[0], media[0], media[0], media[0]});
        }
        
        [TestMethod]
        public void PlayOrder_SequentialForwardRepeatOne_Intermediate()
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

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[1], media[1], media[1], media[1], media[1] });

            queue.RepeatMode = RepeatMode.None;

            sequence.Next();
            sequence.Current.Should().Be(media[2]);
        }

        [TestMethod]
        public void PlayOrder_SequentialForwardRepeatOne_Last()
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

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();
            sequence.Next();
            sequence.Next();
            sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (sequence.Next()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[4], media[4], media[4], media[4], media[4] });
        }

        [TestMethod]
        public void PlayOrder_SequentialReverseNoRepeat()
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

            var sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            while (sequence.Previous()) {
                played.Add(sequence.Current);
            }

            played.Should().BeEmpty();
        }

        [TestMethod]
        public void PlayOrder_SequentialReverseNoRepeat_Intermediate()
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

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();
            sequence.Next();

            var played = new List<Media>();
            while (sequence.Previous()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(2);
            played.Should().ContainInOrder(new[] {media[1], media[0]});
        }

        [TestMethod]
        public void PlayOrder_SequentialReverseRepeatAll()
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

            var sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < queue.Count * 3; i++) {
                if (sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(media.Length * 3);
            played.Take(media.Length).Should().ContainInOrder(media.Reverse());
            played.Skip(media.Length).Take(media.Length).Should().ContainInOrder(media.Reverse());
            played.Skip(media.Length * 2).Take(media.Length).Should().ContainInOrder(media.Reverse());
        }

        [TestMethod]
        public void PlayOrder_SequentialReverseRepeatOne_First()
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

            var sequence = queue.GetPlayOrder();

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[0], media[0], media[0], media[0], media[0] });
        }

        [TestMethod]
        public void PlayOrder_SequentialReverseRepeatOne_Intermediate()
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

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[1], media[1], media[1], media[1], media[1] });

            queue.RepeatMode = RepeatMode.None;

            sequence.Next();
            sequence.Current.Should().Be(media[2]);
        }

        [TestMethod]
        public void PlayOrder_SequentialReverseRepeatOne_Last()
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

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();
            sequence.Next();
            sequence.Next();
            sequence.Next();

            queue.RepeatMode = RepeatMode.Single;

            var played = new List<Media>();
            for (int i = 0; i < 5; i++) {
                if (sequence.Previous()) {
                    played.Add(sequence.Current);
                }
            }

            played.Should().HaveCount(5);
            played.Should().Contain(new[] { media[4], media[4], media[4], media[4], media[4] });
        }

        [TestMethod]
        public void PlayOrder_ForwardThenReverseThenForward()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            var forward = new List<Media>();
            while (sequence.Next()) {
                forward.Add(sequence.Current);
            }

            forward.Should().HaveCount(media.Length);
            forward.Should().ContainInOrder(media);

            var reverse = new List<Media>();
            while (sequence.Previous()) {
                reverse.Add(sequence.Current);
            }

            reverse.Should().HaveCount(media.Length);
            reverse.Should().ContainInOrder(media.Reverse());

            forward.Clear();
            while (sequence.Next()) {
                forward.Add(sequence.Current);
            }

            forward.Should().HaveCount(media.Length);
            forward.Should().ContainInOrder(media);
        }

        [TestMethod]
        public void PlayOrder_Insert_AfterCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            var additional = new Mock<Media>().Object;
            queue.Insert(2, additional);


            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(2);
            played.Should().ContainInOrder(new[] {additional, media[2]});
        }

        [TestMethod]
        public void PlayOrder_Insert_OnCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            var additional = new Mock<Media>().Object;
            queue.Insert(1, additional);
            
            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public void PlayOrder_Insert_BeforeCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();
            
            var additional = new Mock<Media>().Object;
            queue.Insert(0, additional);

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public void PlayOrder_InsertMultiple_AfterCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            var additional1 = new Mock<Media>().Object;
            var additional2 = new Mock<Media>().Object;
            var additional3 = new Mock<Media>().Object;
            queue.Insert(2, additional1);
            queue.Insert(3, additional2);
            queue.Insert(4, additional3);


            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(4);
            played.Should().ContainInOrder(new[] { additional1, additional2, additional3, media[2] });
        }

        [TestMethod]
        public void PlayOrder_InsertMultiple_OnCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            var additional1 = new Mock<Media>().Object;
            var additional2 = new Mock<Media>().Object;
            var additional3 = new Mock<Media>().Object;
            queue.Insert(1, additional1);
            queue.Insert(2, additional2);
            queue.Insert(3, additional3);

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public void PlayOrder_InsertMultiple_BeforeCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            var additional1 = new Mock<Media>().Object;
            var additional2 = new Mock<Media>().Object;
            var additional3 = new Mock<Media>().Object;
            queue.Insert(0, additional1);
            queue.Insert(1, additional2);
            queue.Insert(2, additional3);

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public void Clear()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next().Should().BeTrue();

            queue.Clear();

            sequence.Next().Should().BeFalse();
        }

        [TestMethod]
        public void Clear_WhileShuffled()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue {RepeatMode = RepeatMode.None, SortMode = SortMode.Shuffle};
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next().Should().BeTrue();

            queue.Clear();

            sequence.Next().Should().BeFalse();
        }

        [TestMethod]
        public void PlayOrder_Remove_BeforeCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            queue.Remove(media[0]);

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public void PlayOrder_Remove_OnCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            queue.Remove(media[1]);

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[2] });
        }

        [TestMethod]
        public void PlayOrder_Remove_OnCurrentIndex_MovePrevious()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            queue.Remove(media[1]);

            var played = new List<Media>();
            while (sequence.Previous()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveCount(1);
            played.Should().ContainInOrder(new[] { media[0] });
        }

        [TestMethod]
        public void PlayOrder_Remove_AfterCurrentIndex()
        {
            var media = new[] {
                new Mock<Media>().Object,
                new Mock<Media>().Object,
                new Mock<Media>().Object
            };

            var queue = new PlayQueue { RepeatMode = RepeatMode.None };
            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            sequence.Next();
            sequence.Next();

            queue.Remove(media[2]);

            sequence.Next().Should().BeFalse();
        }

        [TestMethod]
        public void PlayOrder_Shuffled_Forward()
        {
            var media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToArray();
            var queue = new PlayQueue {SortMode = SortMode.Shuffle};

            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();
            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().HaveSameCount(media);
            played.Should().Contain(media);
        }

        [TestMethod]
        public void PlayOrder_Shuffled_NewItemsDoPlay()
        {
            var media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToArray();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle };

            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            var previous = new List<Media>();
            for (int i = 0; i < 10; i++) {
                if (sequence.Next()) {
                    previous.Add(sequence.Current);
                }
            }

            var additional = Enumerable.Range(0, 5).Select(i => new Mock<Media>().Object).ToArray();
            queue.AddRange(additional);

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().Contain(media.Where(m => !previous.Contains(m)), additional);
            played.Should().NotContain(previous);
        }

        [TestMethod]
        public void PlayOrder_Shuffled_RemovedItemsDontPlay()
        {
            var media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToArray();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle };

            queue.AddRange(media);
            
            var sequence = queue.GetPlayOrder();

            var previous = new List<Media>();
            for (int i = 0; i < 10; i++) {
                if (sequence.Next()) {
                    previous.Add(sequence.Current);
                }
            }

            var toRemove = media.Where(m => !previous.Contains(m)).Take(10).ToList();
            foreach (var item in toRemove) {
                queue.Remove(item);
            }

            var played = new List<Media>();
            while (sequence.Next()) {
                played.Add(sequence.Current);
            }

            played.Should().Contain(media.Where(m => !previous.Contains(m) && !toRemove.Contains(m)));
            played.Should().NotContain(previous);
            played.Should().NotContain(toRemove);
        }

        [TestMethod]
        public void PlayOrder_ShuffleDisabled_PlaysNextLinear()
        {
            var media = Enumerable.Range(0, 30).Select(i => new Mock<Media>().Object).ToList();
            var queue = new PlayQueue { SortMode = SortMode.Shuffle, RepeatMode = RepeatMode.All };

            queue.AddRange(media);

            var sequence = queue.GetPlayOrder();

            var previous = new List<Media>();
            for (int i = 0; i < 10; i++) {
                sequence.Next();
                previous.Add(sequence.Current);
            }

            queue.SortMode = SortMode.Linear;

            var previousIndex = media.IndexOf(previous.Last());
            var expected = previousIndex == media.Count - 1 ? media.First() : media[previousIndex + 1];

            sequence.Next();
            sequence.Current.Should().Be(expected);
        }
    }
}
