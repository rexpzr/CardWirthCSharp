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

    public UNK? choice(UNK seq)
    {
        if (seq) {
            return random.choice(seq); // TODO
        } else {
            return null;
        }
    }

    public UNK shuffle(UNK seq)
    {
        UNK seq2 = copy.copy(seq); // TODO
        random.shuffle(seq2); // TODO
        return seq2;
    }

    public UNK pop(UNK seq)
    {
        UNK item = self.choice(seq); // TODO

        if (item) {
            seq.remove(item); // TODO
        }

        return item;
    }
}
