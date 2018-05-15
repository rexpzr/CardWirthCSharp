class Dice
{
    public int roll(int times=1, int sided=6)
    {
        if (sided <= 1) return times;
        int n = 0;
        for (int _i=0; i<times; i++) {
            n += (int)(random.random() * sided) + 1; // TODO: python package
        }
        return n;
    }
    //def choice(self, seq):
    //    if seq:
    //        return random.choice(seq)
    //    else:
    //        return None

    //def shuffle(self, seq):
    //    seq2 = copy.copy(seq)
    //    random.shuffle(seq2)
    //    return seq2

    //def pop(self, seq):
    //    item = self.choice(seq)

    //    if item:
    //        seq.remove(item)

    //    return item
}
