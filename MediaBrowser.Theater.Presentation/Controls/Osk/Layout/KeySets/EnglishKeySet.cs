namespace MediaBrowser.Theater.Presentation.Controls.Osk.Layout.KeySets
{
    public class KeySets
    {
        public static KeySet En()
        {
            var group = new KeySetGroup();

            var letters = new KeySet("EN")
            {
                new KeySetRow
                {
                    new Key('q'),
                    new Key('w'),
                    new Key('e', 'ë', 'é', 'ê', 'è'),
                    new Key('r'),
                    new Key('t', 'þ', 'ť', 'ţ'),
                    new Key('y', 'Ÿ', 'ý'),
                    new Key('u', 'ü', 'ú', 'û', 'ù'),
                    new Key('i', 'ï', 'í', 'î', 'ì'),
                    new Key('o', 'ö', 'ó', 'ô', 'ò', 'ø', 'œ'),
                    new Key('p'),
                    new Key(new BackspaceAction()) {Width = 2}
                },
                new KeySetRow(0.25)
                {
                    new Key('a', 'ä', 'á', 'â', 'à', 'å', 'æ'),
                    new Key('s', 'ß', '§'),
                    new Key('d', 'ð'),
                    new Key('f'),
                    new Key('g'),
                    new Key('h'),
                    new Key('j'),
                    new Key('k'),
                    new Key('l'),
                    new Key('\''),
                    new Key(new AcceptAction()) {Width = 1.75}
                },
                new KeySetRow
                {
                    new Key(new ShiftAction()),
                    new Key('z'),
                    new Key('x'),
                    new Key('c', 'ç', '©'),
                    new Key('v'),
                    new Key('b'),
                    new Key('n', 'ñ'),
                    new Key('m', 'µ'),
                    new Key(','),
                    new Key('.'),
                    new Key('?', '¿'),
                    new Key(new ShiftAction())
                },
                new KeySetRow
                {
                    new Key(new KeySetAction(group)) {Width = 1.25},
                    new Key('/'),
                    new Key(' ') {Width = 6.75},
                    new Key(new LeftAction()),
                    new Key(new RightAction()),
                    new Key(new CancelAction())
                }
            };

            var numbers = new KeySet("EN")
            {
                new KeySetRow
                {
                    new Key('1'),
                    new Key('2'),
                    new Key('3'),
                    new Key('4'),
                    new Key('5'),
                    new Key('6'),
                    new Key('7'),
                    new Key('8'),
                    new Key('9'),
                    new Key('0'),
                    new Key(new BackspaceAction()) {Width = 2}
                },
                new KeySetRow(0.25)
                {
                    new Key('\\'),
                    new Key('@'),
                    new Key('#'),
                    new Key(':'),
                    new Key(';'),
                    new Key('~'),
                    new Key('-'),
                    new Key('+'),
                    new Key('(', '{', '[', '<'),
                    new Key(')', '}', ']', '>'),
                    new Key(new AcceptAction()) {Width = 1.75}
                },
                new KeySetRow
                {
                    new Key('&'),
                    new Key('?', '¿'),
                    new Key('!', '¡'),
                    new Key('$', '£', '€', '¥', '¢', '₹', '¤'),
                    new Key('£', '$', '€', '¥', '¢', '₹', '¤'),
                    new Key('€', '$', '£', '¥', '¢', '₹', '¤'),
                    new Key('%', '‰'),
                    new Key('_'),
                    new Key('^'),
                    new Key('*'),
                    new Key('"'),
                    new Key('=')
                },
                new KeySetRow
                {
                    new Key(new KeySetAction(group)) {Width = 1.25},
                    new Key('/'),
                    new Key(' ') {Width = 6.75},
                    new Key(new LeftAction()),
                    new Key(new RightAction()),
                    new Key(new CancelAction())
                }
            };

            group.Add("abc", letters);
            group.Add("&123", numbers);

            return letters;
        }
    }
}